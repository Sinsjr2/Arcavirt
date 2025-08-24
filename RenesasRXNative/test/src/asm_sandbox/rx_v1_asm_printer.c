#include <stdint.h>
#include <inttypes.h>
#include <string.h>
#include "i_logger.h"
#include "num_string_util.h"

typedef struct RXv1AsmPrinter_DECL {

    void* obj;
    uint8_t numOfRegisters;
    // 指定したレジスタの番号を取得します。
    uint32_t (*getRegisterValue)(void* obj, uint8_t regNo);
    uintptr_t (*getUSP)(void* obj);
    uintptr_t (*getISP)(void* obj);
    uintptr_t (*getINTB)(void* obj);
    uintptr_t (*getPC)(void* obj);
    uint32_t (*getPSW)(void* obj);
    uintptr_t (*getBPC)(void* obj);
    uint32_t (*getBPSW)(void* obj);
    uintptr_t (*getFINTV)(void* obj);
    uint32_t (*getFPSW)(void* obj);
    uint8_t numOfACC;
    uint64_t (*getACC)(void* obj, uint8_t regNo);
} RXv1AsmPrinter;

void RXv1AsmPrinter_printRegisters(RXv1AsmPrinter* self, ILogger* logger);

void RXv1AsmPrinter_printRegisters(RXv1AsmPrinter* self, ILogger* logger) {
    if (self == NULL) {
        return;
    }
    if (self->getRegisterValue != NULL) {
        for (uint8_t i = 0; i < self->numOfRegisters; i++) {
            uint32_t regValue = self->getRegisterValue(self->obj, i);
            ILogger_printf(logger, "reg[%d]:  %d, %u, 0x%x\n", i, regValue, regValue, regValue);
        }
    }
    if (self->getUSP != NULL) {
        const uintptr_t usp = self->getUSP(self->obj);
        ILogger_printf(logger, "usp: 0x%x\n", usp);
    }
    if (self->getISP != NULL) {
        const uintptr_t isp = self->getISP(self->obj);
        ILogger_printf(self->obj, "isp: 0x%x\n", isp);
    }
    if (self->getINTB != NULL) {
        const uintptr_t intb = self->getINTB(self->obj);
        ILogger_printf(logger, "intb: 0x%x\n", intb);
    }
    if (self->getPC != NULL) {
        const uintptr_t pc = self->getPC(self->obj);
        ILogger_printf(logger, "pc: 0x%x\n", pc);
    }
    if (self->getPSW != NULL) {
        const uint32_t psw = self->getPSW(self->obj);
        // 終端文字用に+1
        char pswStr[NUM_STRING_UTIL__U32_BIN_STR_LENGTH + 1];
        memset(pswStr, 0, sizeof(pswStr));
        NumStringUtil_u32toBinStr(pswStr, sizeof(pswStr) / sizeof(pswStr[0]), psw);
        ILogger_printf(logger, "psw: 0b%s\n", pswStr);
    }
    if (self->getBPC != NULL) {
        const uintptr_t pc = self->getBPC(self->obj);
        ILogger_printf(logger, "bpc: 0x%x\n", pc);
    }
    if (self->getBPSW != NULL) {
        const uint32_t bpsw = self->getBPSW(self->obj);
        // 集団文字用に+1
        char bpswStr[NUM_STRING_UTIL__U32_BIN_STR_LENGTH + 1];
        memset(bpswStr, 0, sizeof(bpswStr));
        NumStringUtil_u32toBinStr(bpswStr, sizeof(bpswStr) / sizeof(bpswStr[0]), bpsw);
        ILogger_printf(logger, "bpsw: 0x%s\n", bpswStr);
    }
    if (self->getFINTV != NULL) {
        const uintptr_t fintv = self->getFINTV(self->obj);
        ILogger_printf(logger, "fintv: 0x%x\n", fintv);
    }
    if (self->getFPSW != NULL) {
        const uint32_t fpsw = self->getFPSW(self->obj);
        // 終端文字用に+1
        char fpswStr[NUM_STRING_UTIL__U32_BIN_STR_LENGTH + 1];
        memset(fpswStr, 0, sizeof(fpswStr));
        NumStringUtil_u32toBinStr(fpswStr, sizeof(fpswStr) / sizeof(fpswStr[0]), fpsw);
        ILogger_printf(logger, "fpsw: 0x%x\n", fpsw);
    }
    if (self->getACC != NULL) {
        for (uint8_t i = 0; i < self->numOfACC; i++) {
            uint64_t acc = self->getACC(self->obj, i);
            ILogger_printf(logger, "acc[%d]: %" PRIu64, i, acc);
        }
    }
}