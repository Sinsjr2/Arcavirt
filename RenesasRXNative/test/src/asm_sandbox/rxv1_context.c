#include <stdint.h>

typedef struct RXv1Context_DECL {
    uint32_t registers[15];
    uintptr_t usp;
    uintptr_t isp;
    uintptr_t intb;
    uintptr_t pc;
    uint32_t psw;
    uintptr_t bpc;
    uint32_t bpsw;
    uintptr_t fintv;
    uint32_t fpsw;
    uint64_t acc;
} RXv1Context;

void RXv1Context_storeContext(RXv1Context* dest) {
    // R0 もプログラムカウンタが入っているのでずれない様に初めに格納しておく
    //mvfc pc, dest->pc;
    //mov.l R0, dest->registers[0];
    // 1~15まで書く
    
    //mvfc usp, dest->usp;
    //mvfc isp, dest->isp;
    //mvfc intb, dest->intb;
    //mvfc psw, dest->psw;
    //mvfc bpc, dest->bpc;
    //mvfc bpsw, dest->bpsw;
    //mvfc fintv, dest->fintv;
    //mvfc fpsw, dest->fpsw;

    // 退避させ始めたときのカウンタを知れるように mvfc のサイズ分減らす
    //dest->registers[0] -= 3;
}