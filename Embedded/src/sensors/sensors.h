#pragma once

#include <cstdint>

struct SensorReading {
    float    temperature;
    float    humidity;
    uint16_t airQualityAdc;
    bool     motionDetected;
    bool     dhtValid;
    bool     airValid;
};

void sensors_init();
bool     sensors_readPIR();              // PIR: hareket var mı?
void     sensors_readMQ(SensorReading& r);  // MQ135: ADC + geçerlilik
void     sensors_readDHT(SensorReading& r); // DHT11: sıcaklık + nem (~18ms)
