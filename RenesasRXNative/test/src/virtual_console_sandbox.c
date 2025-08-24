#include <stddef.h>
#include <string.h>
// #include <stdio.h>
#include "r_smc_entry.h"
#include "virtual_console_sandbox.h"
#include "stdarg.h"

static void VirtualConsoleSandbox_puts(const char* str) {
    if (str == NULL) {
        return;
    }
    const size_t length = strlen(str);
    for (size_t i = 0; i < length; i++) {
        charput(str[i]);
    }
}

// static void VirtualConsoleSandbox_printf(const char* format, ...) {
//     char buf[1000];
//     buf[0] = '\0';
//     va_list ap;
//     va_start(ap, format);
//     vsnprintf(buf, sizeof(buf), format, ap);
//     VirtualConsoleSandbox_printf(buf);
//     va_end(ap);
// }

static int32_t VirtualConsoleSandbox_abs32(int32_t value) {
    return value < 0 ? -value : value;
}

// 10進数の桁数を返します。
static int32_t VirtualConsoleSandbox_getDecimalDigitLength(int32_t num) {
    int32_t x = VirtualConsoleSandbox_abs32(num);
    int32_t length = 1;
    x /= 10;
    while (0 < x) {
        x = x / 10;
        length++;
    }
    return length;
}

static void VirtualConsoleSandbox_printNumber(int32_t num) {
    static const char numToAscii[] = {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
    };
    int32_t absValue = VirtualConsoleSandbox_abs32(num);
    // // 最上位の数値を探索する
    int32_t maxNumber = 1;
    while (true) {
        int32_t nextValue = maxNumber * 10;
        if (absValue < nextValue) {
            break;
        }
        maxNumber = nextValue;
    }

    if (num < 0) {
        charput('-');
    }

    int32_t x = maxNumber;
    for (; 0 < x; x /= 10) {
        int32_t currentNum = (absValue / x) % 10;
        charput(numToAscii[currentNum]);
    }
}

void VirtualConsoleSandbox_run(void) {
    while (true) {
        VirtualConsoleSandbox_printNumber(1);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(3);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(-3);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(-99);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(-100);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(100);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(999);
        VirtualConsoleSandbox_puts("\n");

        VirtualConsoleSandbox_printNumber(1000);
        VirtualConsoleSandbox_puts("\n");

        // for (int32_t i = 0; i < 10; i++) {
        //     VirtualConsoleSandbox_printNumber(i); //VirtualConsoleSandbox_printf("%d", i);
        // }
        return;
        R_BSP_SoftwareDelay(500, BSP_DELAY_MILLISECS);
    }
}
