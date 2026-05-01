#pragma once

#include "../data/data.h"
#include "../control/control.h"

void mqtt_init();
void mqtt_reconnectIfNeeded();
bool mqtt_publish(const SensorPacket& packet, const AlertStatus& status);
bool mqtt_isConnected();
