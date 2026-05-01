#pragma once

#include <cstdint>

static constexpr char DEVICE_ID[] = DEVICE_ID_VAL;

static constexpr char WIFI_SSID[]     = WIFI_SSID_VAL;
static constexpr char WIFI_PASSWORD[] = WIFI_PASSWORD_VAL;

static constexpr long NTP_TZ_OFFSET_SEC = 3L * 3600L;
static constexpr int  NTP_DST_SEC       = 0;
static constexpr char NTP_SERVER[]      = "pool.ntp.org";

static constexpr uint8_t PIN_PIR    = 26;
static constexpr uint8_t PIN_MQ135  = 32;
static constexpr uint8_t PIN_DHT11  = 27;
static constexpr uint8_t PIN_BUZZER = 33;
static constexpr uint8_t PIN_LED    = 25;

static constexpr uint32_t SENSOR_TASK_INTERVAL_MS = 100UL;   // PIR + MQ135
static constexpr uint32_t DHT_READ_INTERVAL_MS    = 2500UL;  // DHT11 
static constexpr uint32_t MQTT_TASK_INTERVAL_MS   = 1000UL;  // MQTT yayın

// Bağlantı koptuğunda bellekte tutulacak maksimum paket sayısı
static constexpr uint8_t  MQTT_BUFFER_SIZE        = 30U;

// MQ135 ADC geçerlilik aralığı — 0 ve 4095 sensör bağlı değil/satüre demek
static constexpr uint16_t ADC_MIN_VALID           = 10U;
static constexpr uint16_t ADC_MAX_VALID           = 4085U;

static constexpr float    TEMP_THRESHOLD_C  = 35.0f;
static constexpr float    HUM_THRESHOLD_PCT = 80.0f;
static constexpr uint16_t AIR_THRESHOLD_ADC = 2000U;

static constexpr char     MQTT_BROKER[]    = MQTT_BROKER_VAL;
static constexpr uint16_t MQTT_PORT        = MQTT_PORT_VAL;
static constexpr char     MQTT_CLIENT_ID[] = MQTT_CLIENT_ID_VAL;
static constexpr char     MQTT_TOPIC[]     = MQTT_TOPIC_VAL;
static constexpr char     MQTT_USER[]      = MQTT_USER_VAL;
static constexpr char     MQTT_PASS[]      = MQTT_PASS_VAL;
