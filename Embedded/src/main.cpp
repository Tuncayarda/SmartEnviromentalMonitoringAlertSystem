#include <Arduino.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <freertos/semphr.h>

#include "config/config.h"
#include "sensors/sensors.h"
#include "data/data.h"
#include "control/control.h"
#include "comm/wifi_ntp.h"
#include "comm/mqtt_client.h"

static SensorReading     g_sharedReading = {};
static SemaphoreHandle_t g_mutex         = nullptr;

// Core 1 burada çalışır.
// PIR + MQ135 her 100ms'de okunur, LED/buzzer güncellenir.
// DHT11 her 2 saniyede bir okunur.
void taskSensors(void* pvParams) {
    TickType_t sensorTick = xTaskGetTickCount();
    TickType_t dhtTick  = xTaskGetTickCount() - pdMS_TO_TICKS(DHT_READ_INTERVAL_MS);
    SensorReading r = {};

    while (true) {
        const TickType_t now = xTaskGetTickCount();

        // DHT okuması — 2.5 saniyede bir
        if ((now - dhtTick) >= pdMS_TO_TICKS(DHT_READ_INTERVAL_MS)) {
            dhtTick = now;
            sensors_readDHT(r);
        }

        // Hızlı sensörler — 100ms'de bir
        if ((now - sensorTick) >= pdMS_TO_TICKS(SENSOR_TASK_INTERVAL_MS)) {
            sensorTick = now;

            r.motionDetected = sensors_readPIR();
            sensors_readMQ(r);

            const bool tempAlert   = r.dhtValid && (r.temperature > TEMP_THRESHOLD_C || r.humidity > HUM_THRESHOLD_PCT);
            const bool airAlert    = r.airValid && (r.airQualityAdc > AIR_THRESHOLD_ADC);
            const bool alertActive = tempAlert || airAlert || r.motionDetected;

            if (xSemaphoreTake(g_mutex, pdMS_TO_TICKS(5)) == pdTRUE) {
                g_sharedReading = r;
                xSemaphoreGive(g_mutex);
            }

            digitalWrite(PIN_LED,    alertActive ? HIGH : LOW);
            digitalWrite(PIN_BUZZER, alertActive ? LOW  : HIGH);
        }

        vTaskDelay(1);
    }
}

// Core 0 burada çalışır. Her 1 saniyede son sensör verisini alıp MQTT'ye gönderir.
// Geçersiz okuma varsa göndermez.
void taskMqtt(void* pvParams) {
    TickType_t lastTick = xTaskGetTickCount();

    while (true) {
        const TickType_t now = xTaskGetTickCount();
        if ((now - lastTick) >= pdMS_TO_TICKS(MQTT_TASK_INTERVAL_MS)) {
            lastTick = now;

            char timestamp[32];
            ntp_getTimestamp(timestamp, sizeof(timestamp));

            SensorReading r = {};
            if (xSemaphoreTake(g_mutex, pdMS_TO_TICKS(5)) == pdTRUE) {
                r = g_sharedReading;
                xSemaphoreGive(g_mutex);
            }

            if (!r.dhtValid || !r.airValid) {
                vTaskDelay(1);
                continue;
            }

            const SensorPacket packet = data_createPacket(r, timestamp);
            const AlertStatus  status = control_evaluate(packet);
            mqtt_publish(packet, status);
        }
        vTaskDelay(1);
    }
}

// Cihaz ayağa kalkar. Her şeyi sırayla başlatır, mutex oluşturur, iki task'ı iki ayrı core'a atar.
void setup() {
    Serial.begin(115200);
    sensors_init();
    control_init();
    ntp_init();
    mqtt_init();

    g_mutex = xSemaphoreCreateMutex();
    configASSERT(g_mutex);

    xTaskCreatePinnedToCore(taskSensors, "SensorTask", 4096, nullptr, 1, nullptr, 1);  // Core 1: DHT busy-wait burda WiFi stack'ini etkilemez
    xTaskCreatePinnedToCore(taskMqtt,    "MqttTask",   8192, nullptr, 1, nullptr, 0);  // Core 0: WiFi stack ile aynı core, TCP gecikme yok
}

// Arduino'nun kendi loop task'ı artık işimize yaramıyor, kendini sil.
void loop() {
    vTaskDelete(nullptr);
}

