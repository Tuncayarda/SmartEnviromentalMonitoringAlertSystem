#include "wifi_ntp.h"
#include "../config/config.h"

#include <Arduino.h>
#include <WiFi.h>
#include <esp_wifi.h>
#include <sys/time.h>

// WiFi'ye bağlanır, bağlantı olursa NTP sunucusundan saati senkronize eder.
void ntp_init() {
    esp_wifi_set_ps(WIFI_PS_NONE);  // WiFi.begin() öncesi driver seviyesinde power save kapat
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

    uint32_t start = millis();
    while (WiFi.status() != WL_CONNECTED && millis() - start < 10000) {
        delay(500);
    }

    if (WiFi.status() != WL_CONNECTED) return;

    esp_wifi_set_ps(WIFI_PS_NONE);  // bağlantı sonrası tekrar garantile — bazı sürücülerde reset'leniyor

    configTime(NTP_TZ_OFFSET_SEC, NTP_DST_SEC, NTP_SERVER);

    struct tm t = {};
    start = millis();
    while (!getLocalTime(&t) && millis() - start < 8000) {
        delay(500);
    }
}

// O anki saati ISO 8601 formatında yazar.
void ntp_getTimestamp(char* buf, size_t len) {
    struct timeval tv = {};
    gettimeofday(&tv, nullptr);
    struct tm* t = localtime(&tv.tv_sec);

    const long  offsetSec = NTP_TZ_OFFSET_SEC;
    const int   offH      = static_cast<int>(offsetSec / 3600);
    const int   offM      = static_cast<int>((offsetSec % 3600) / 60);
    const char  sign      = (offsetSec >= 0) ? '+' : '-';

    snprintf(buf, len, "%04d-%02d-%02dT%02d:%02d:%02d%c%02d:%02d",
             t->tm_year + 1900, t->tm_mon + 1, t->tm_mday,
             t->tm_hour, t->tm_min, t->tm_sec,
             sign, abs(offH), abs(offM));
}
