#include "num_string_util.h"
#include <limits.h>

bool NumStringUtil_u32toBinStr(char* dest, size_t destLength, uint32_t value) {
    if (dest == NULL || destLength < NUM_STRING_UTIL__U32_BIN_STR_LENGTH) {
        return false;
    }
    uint32_t mask = 1 << (NUM_STRING_UTIL__U32_BIN_STR_LENGTH - 1);
    for (size_t i = 0; i < NUM_STRING_UTIL__U32_BIN_STR_LENGTH; i++) {
        dest[i] = mask & value ? '1' : '0';
        mask >>= 1;
    }
    return true;
}
