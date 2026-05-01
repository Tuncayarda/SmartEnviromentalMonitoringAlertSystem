-- IoT Backend API - PostgreSQL Veritabanı Şeması
-- Manuel kurulum için SQL script

-- Veritabanını oluştur
CREATE DATABASE iotdb
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'Turkish_Turkey.1254'
    LC_CTYPE = 'Turkish_Turkey.1254'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Veritabanına bağlan
\c iotdb

-- sensor_readings tablosunu oluştur
CREATE TABLE sensor_readings (
    id BIGSERIAL PRIMARY KEY,
    device_id VARCHAR(50) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    temperature NUMERIC(5,2) NOT NULL,
    humidity NUMERIC(5,2) NOT NULL,
    air_quality_adc INTEGER NOT NULL,
    motion_detected BOOLEAN NOT NULL,
    is_alarm BOOLEAN NOT NULL DEFAULT false,
    alarm_reason VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Performans için indeksler
CREATE INDEX ix_sensor_readings_device_id ON sensor_readings(device_id);
CREATE INDEX ix_sensor_readings_timestamp ON sensor_readings(timestamp);
CREATE INDEX ix_sensor_readings_is_alarm ON sensor_readings(is_alarm);

-- Composite index - device ve timestamp için
CREATE INDEX ix_sensor_readings_device_timestamp ON sensor_readings(device_id, timestamp DESC);

-- Örnek veri ekleme (opsiyonel - test için)
INSERT INTO sensor_readings (device_id, timestamp, temperature, humidity, air_quality_adc, motion_detected, is_alarm, alarm_reason)
VALUES 
    ('ESP32_001', NOW() - INTERVAL '5 minutes', 25.5, 60.0, 1500, false, false, NULL),
    ('ESP32_001', NOW() - INTERVAL '3 minutes', 38.0, 85.0, 3500, true, true, 'Sıcaklık çok yüksek (38.0°C > 35.0°C); Nem çok yüksek (85.0% > 80.0%); Hava kalitesi kötü (ADC: 3500 > 3000)'),
    ('ESP32_002', NOW() - INTERVAL '2 minutes', 22.0, 55.0, 1200, false, false, NULL);

-- Veritabanı bilgilerini göster
SELECT 
    COUNT(*) as total_readings,
    COUNT(DISTINCT device_id) as total_devices,
    COUNT(CASE WHEN is_alarm THEN 1 END) as total_alarms
FROM sensor_readings;

-- Tablo yapısını göster
\d+ sensor_readings
