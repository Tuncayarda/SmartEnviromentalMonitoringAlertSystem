#pragma once

#include "../data/data.h"

struct AlertStatus {
    bool temperature;
    bool humidity;
    bool airQuality;
    bool motion;

    bool anyActive() const {
        return temperature || humidity || airQuality || motion;
    }
};

void        control_init();
AlertStatus control_evaluate(const SensorPacket& packet);
void        control_apply(const AlertStatus& status);
