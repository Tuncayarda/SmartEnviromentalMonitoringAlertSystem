#include "data.h"
#include "../config/config.h"

#include <cstring>

// Ham sensör okumasını ve zaman damgasını bir araya getirip gönderilebilir bir paket yapar.
SensorPacket data_createPacket(const SensorReading& reading, const char* timestamp) {
    SensorPacket p;
    p.deviceId = DEVICE_ID;
    strncpy(p.timestamp, timestamp, sizeof(p.timestamp) - 1);
    p.timestamp[sizeof(p.timestamp) - 1] = '\0';
    p.reading = reading;
    return p;
}

