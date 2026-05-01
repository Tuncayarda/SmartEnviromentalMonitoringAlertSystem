#include "sensors.h"
#include "../config/config.h"

#include <Arduino.h>
#include <DHT.h>

static DHT s_dht(PIN_DHT11, DHT11);

// Pin modlarını ayarlar, DHT kütüphanesini başlatır.
// DHT11 ölçüm öncesi stabilize olmak için 1 saniye beklenir.
void sensors_init() {
    pinMode(PIN_PIR,   INPUT);
    pinMode(PIN_MQ135, INPUT);
    s_dht.begin();
    delay(1000);  // DHT11 güç sonrası stabilizasyon
}

// PIR: anlık hareket durumu.
bool sensors_readPIR() {
    return digitalRead(PIN_PIR) == HIGH;
}

// MQ135: ham ADC değerini okur, geçerlilik aralığını kontrol eder.
void sensors_readMQ(SensorReading& r) {
    r.airQualityAdc = static_cast<uint16_t>(analogRead(PIN_MQ135));
    r.airValid      = (r.airQualityAdc >= ADC_MIN_VALID && r.airQualityAdc <= ADC_MAX_VALID);
}

// DHT11: sıcaklık ve nem okur. Geçersizse dhtValid = false.
void sensors_readDHT(SensorReading& r) {
    const float t = s_dht.readTemperature();
    const float h = s_dht.readHumidity();
    r.dhtValid = !isnan(t) && !isnan(h)
                 && t >= -40.0f && t <= 85.0f
                 && h >=   0.0f && h <= 100.0f;
    if (r.dhtValid) {
        r.temperature = t;
        r.humidity    = h;
    }
}
