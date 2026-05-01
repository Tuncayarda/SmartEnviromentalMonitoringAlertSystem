#pragma once

#include <cstddef>

void ntp_init();
void ntp_getTimestamp(char* buf, size_t len);
