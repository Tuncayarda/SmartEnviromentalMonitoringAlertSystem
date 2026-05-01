#include "control.h"
#include "../config/config.h"

#include <Arduino.h>

// LED ve buzzeri output olarak ayarlar, başlangıçta ikisini de kapalı konuma getirir.
void control_init() {
    pinMode(PIN_LED,        OUTPUT);
    pinMode(PIN_BUZZER,     OUTPUT);
    digitalWrite(PIN_LED,        LOW);
    digitalWrite(PIN_BUZZER,     HIGH);
}

// Gelen paketteki değerlere bakıp hangi alarmların aktif olduğuna karar verir.
AlertStatus control_evaluate(const SensorPacket& packet) {
    const SensorReading& r = packet.reading;

    AlertStatus status = {};

    if (r.dhtValid) {
        status.temperature = (r.temperature > TEMP_THRESHOLD_C);
        status.humidity    = (r.humidity    > HUM_THRESHOLD_PCT);
    }

    status.airQuality = r.airValid && (r.airQualityAdc > AIR_THRESHOLD_ADC);
    status.motion     = r.motionDetected;

    return status;
}

