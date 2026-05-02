#include "mqtt_client.h"
#include "../config/config.h"

#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>

static WiFiClientSecure wifiClient;
static PubSubClient     mqtt(wifiClient);

// Bağlantı yokken gönderilemeyen paketler burada bekler
struct BufferedPacket {
    SensorPacket packet;
    AlertStatus  status;
};

static BufferedPacket s_buf[MQTT_BUFFER_SIZE];
static uint8_t        s_head  = 0;
static uint8_t        s_tail  = 0;
static uint8_t        s_count = 0;

// Paketi JSON string'e çevirir, hem publish hem flush bu fonksiyonu kullanır
static void buildJson(char* out, size_t len, const SensorPacket& pkt, const AlertStatus& st) {
    const SensorReading& r = pkt.reading;
    snprintf(out, len,
        "{\"device_id\":\"%s\","
        "\"timestamp\":\"%s\","
        "\"temperature\":%.1f,"
        "\"humidity\":%.1f,"
        "\"air_adc\":%u,"
        "\"motion\":%s,"
        "\"alert\":%s}",
        pkt.deviceId,
        pkt.timestamp,
        r.dhtValid ? r.temperature : -999.0f,
        r.dhtValid ? r.humidity    : -999.0f,
        r.airQualityAdc,
        r.motionDetected ? "true" : "false",
        (st.anyActive() || r.motionDetected) ? "true" : "false"
    );
}

// Paketi kuyruğa atar
static void enqueue(const SensorPacket& pkt, const AlertStatus& st) {
    s_buf[s_head] = { pkt, st };
    s_head = (s_head + 1) % MQTT_BUFFER_SIZE;
    if (s_count < MQTT_BUFFER_SIZE) {
        s_count++;
    } else {
        s_tail = (s_tail + 1) % MQTT_BUFFER_SIZE;
    }
}

// Bağlantı geri gelince kuyrukta biriken paketleri sırayla gönderir
static void flushBuffer() {
    char buf[512];
    while (s_count > 0 && mqtt.connected()) {
        buildJson(buf, sizeof(buf), s_buf[s_tail].packet, s_buf[s_tail].status);
        if (!mqtt.publish(MQTT_TOPIC, buf)) break;
        s_tail  = (s_tail + 1) % MQTT_BUFFER_SIZE;
        s_count--;
    }
}

// Broker'a bağlı mı değil mi, dışarıdan sorulabilsin diye.
bool mqtt_isConnected() {
    return mqtt.connected();
}

// Broker adresini ve buffer boyutunu ayarlar, başka bir şey yapmaz.
void mqtt_init() {
    wifiClient.setInsecure();  // Sertifika doğrulaması yapma (self-signed / ortak CA olmayan brokerlar için)
    mqtt.setServer(MQTT_BROKER, MQTT_PORT);
    mqtt.setBufferSize(512);
    mqtt.setSocketTimeout(5);
}

// Bağlantı kopmuşsa yeniden bağlanmayı dener.
void mqtt_reconnectIfNeeded() {
    if (mqtt.connected()) return;
    if (WiFi.status() != WL_CONNECTED) return;

    static uint32_t lastAttempt = 0;
    if (millis() - lastAttempt < 5000) return;
    lastAttempt = millis();

    if (MQTT_USER[0] != '\0')
        mqtt.connect(MQTT_CLIENT_ID, MQTT_USER, MQTT_PASS);
    else
        mqtt.connect(MQTT_CLIENT_ID);
}

// MQTT protokol trafiğini işler (PINGREQ/PINGRESP vb.). taskMqtt'nin her iterasyonunda çağrılmalı.
void mqtt_loop() {
    mqtt.loop();
}

// Sensör paketini JSON'a çevirip broker'a yollar. Bağlantı yoksa buffer'a atar, bağlantı gelince önce birikenleri gönderir.
bool mqtt_publish(const SensorPacket& packet, const AlertStatus& status) {
    mqtt_reconnectIfNeeded();
    if (!mqtt.connected()) {
        enqueue(packet, status);
        return false;
    }

    flushBuffer();

    char buf[512];
    buildJson(buf, sizeof(buf), packet, status);
    if (!mqtt.publish(MQTT_TOPIC, buf)) {
        enqueue(packet, status);  // publish başarısız olursa paketi kurban etme
        return false;
    }
    return true;
}
