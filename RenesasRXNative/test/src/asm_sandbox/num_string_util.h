#ifndef NUM_STRING_UTIL_H
#define NUM_STRING_UTIL_H

#include <stdint.h>
#include <stddef.h>
#include <stdbool.h>

// 数値と文字列の変換に関するユーティリティ関数

// uint32_t を2進数の文字列に変換したときの文字列の長さ
#define NUM_STRING_UTIL__U32_BIN_STR_LENGTH (32)

// uint32_t の値を2進法の文字列に変換します。
// 指定した先がNULLや容量が足りない場合は、何もせずNULLを返します。
bool NumStringUtil_u32toBinStr(char* dest, size_t destLength, uint32_t value);

#endif
