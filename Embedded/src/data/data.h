#pragma once

#include "../sensors/sensors.h"
#include <cstdint>

struct SensorPacket {
    const char*   deviceId;
    char          timestamp[32];
    SensorReading reading;
};

SensorPacket data_createPacket(const SensorReading& reading, const char* timestamp);
