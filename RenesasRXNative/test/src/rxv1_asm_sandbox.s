
    .section  .data
_g_u8Value: .dc.b 0
_g_u16Value: .dc.w 0
    .global _g_u8Value
    .global _g_u16Value

    .section .bss
#define G_WORK_AREA_SIZE 128
_g_workArea: .ds.b G_WORK_AREA_SIZE
    .global _g_workArea

    .section .text

// 使用するグルーバル変数を初期化します。
// void sandbox_init();
sandbox_init:
    mov #_g_u8Value, R1
    mov.b #71h, [R1]

    mov #_g_u16Value, R1
    mov.b #89h, [R1]

    mov #_g_workArea, R1
    ;; 何バイト書き込まれたのか確認できるようにFFhで埋める
    mov #0FFh, R2
    mov #G_WORK_AREA_SIZE, R3
    sstr.b
    rts

sandbox_abs:
	mov #0FFFFFFFFh, R1
	abs R1

	mov #80000000h, R1
	abs R1

	mov #0, R1
	abs R1

	mov #-1000, R1
	abs R1

	mov #0FFFFFFFFh, R1
	abs R1, R2

	mov #80000000h, R1
	abs R1, R2

	mov #0, R1
	abs R1, R2

	rts

sandbox_adc:
	clrpsw c
	mov #128, R1
	adc #-128:8, R1

	clrpsw c
	mov #0, R1
	adc #0, R1

	clrpsw c
	mov #0, R1
	adc #0, R1

	clrpsw c
	mov #0FFFFFFFFh, R1
	adc #1, R1

	clrpsw c
	mov #7FFFFFFFh, R1
	adc #1, R1

	clrpsw c
	mov #80000000h, R1
	adc #-1, R1

	clrpsw c
	mov #80h,R1
	adc #80h:8, R1

	clrpsw c
	mov #80000000h, R1
	adc #80000000h, R1

	mov #100001h, R1
	clrpsw c
	adc #7FFFFFh:24, R1

	rts

sandbox_mov:
    ;; (4) mov.size (imm8, uimm8) (dsp:5[rd]) の動作確認
    mov #_g_workArea, R1
    ;; mov.b #0ABh:8, 1:5[R1]
    mov.w #0ABh:8, 2:5[R1]
    ;; mov.l #0ABh:8, 4:5[R1]
    rts

sandbox_cmp:
    mov #80000000h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #70000000h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #0FFFF8000h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #7000h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #8000h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #70h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #80h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #0FFFFFF80h, R1
    mov #0FFFFFFFFh, R2
    cmp R1, R2

    mov #5, R1
    mov #1, R2
    cmp R1, R2

    mov #0FFFFFC18h, R1
    mov #07FFFFFFFh, R2
    cmp R1, R2

    mov #07FFFFFFFh, R1
    mov #0FFFFFC18h, R2
    cmp R1, R2

    rts

sandbox_add:
	mov #0Fh, R1
	mov #0FFFFFFF0h, R2
	add R1, R2

	mov #1, R1
	mov #0FFFFFFFFh, R2
	add R1, R2

	mov #0Fh, R1
	mov #07FFFFFFFh, R2
	add R1, R2

	mov #080000000h, R1
	mov #080000000h, R2
	add R1, R2
	rts

sandbox_fadd:
	mov #125, R1
	itof R1, R2
	fadd #30.586, R2
	rts

sandbox_rmpa:
	mov #0, R6
	mov #0, R5
	mov #0, R4

	mov #_g_workArea, R7
	mov.b #-1, 0:8[R7]

	mov.b #-1, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	;; 0 + -1*-1
	rmpa.b

	;; sign change size B
	mov #0, R6
	mov #0, R5
	mov #0, R4

	mov #_g_workArea, R7
	mov.b #-1, 0:8[R7]

	mov.b #1, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	;; 0 + -1*1
	rmpa.b

	;;
	mov #0, R6
	mov #0FFFFFFFFh, R5
	mov #0FFFFFFFFh, R4

	mov #_g_workArea, R7
	mov.l #7FFFFFFFh, 0:8[R7]

	mov.l #7FFFFFFFh, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	rmpa.l

	;;
	mov #00003FFFh, R6
	mov #0FFFFFFFFh, R5
	mov #0FFFFFFFFh, R4

	mov #_g_workArea, R7
	mov.l #7FFFFFFFh, 0:8[R7]

	mov.l #7FFFFFFFh, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	rmpa.l

	;;
	mov #00008000h, R6
	mov #0, R5
	mov #0, R4

	mov #_g_workArea, R7
	mov.l #7FFFFFFFh, 0:8[R7]

	mov.l #0, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	rmpa.l

	;;
	mov #00004000h, R6
	mov #0h, R5
	mov #0Fh, R4

	mov #_g_workArea, R7
	mov.l #0FFFFFFFFh, 0:8[R7]
	mov.l #0Fh, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	rmpa.l

	;; sign change size L
	mov #0, R6
	mov #0, R5
	mov #0, R4

	mov #_g_workArea, R7
	mov.l #-1, 0:8[R7]

	mov.l #1, 64:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3

	;; 0 + -1*1
	rmpa.l

	mov #0, R6
	mov #0, R5
	mov #0, R4

	mov #_g_workArea, R7
	mov.l #20, 0:8[R7]
	mov.l #9, 4:8[R7]

	mov.l #2, 64:8[R7]
	mov.l #3, 68:8[R7]

	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #2, R3

	;; 20*2 + 9*3
	rmpa.l

	;; overflow check

	;; none overflow
	mov #0, R6
	mov #07FFFFFFFh, R5
	mov #0FFFFFFFDh, R4

	mov.l #1, 0:8[R7]
	mov.l #1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #0, R3
	rmpa.l

	;; none overflow
	mov #0, R6
	mov #07FFFFFFFh, R5
	mov #0FFFFFFFDh, R4

	mov.l #1, 0:8[R7]
	mov.l #1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	;; none overflow
	mov #0, R6
	mov #07FFFFFFFh, R5
	mov #0FFFFFFFEh, R4

	mov.l #1, 0:8[R7]
	mov.l #1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	;; overflow
	mov #0, R6
	mov #07FFFFFFFh, R5
	mov #0FFFFFFFFh, R4

	mov.l #1, 0:8[R7]
	mov.l #1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	;; none overflow
	mov #0FFFFFFFFh, R6
	mov #080000000h, R5
	mov #000000001h, R4

	mov.l #-1, 0:8[R7]
	mov.l #1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	;; none overflow
	mov #0FFFFFFFFh, R6
	mov #080000000h, R5
	mov #000000002h, R4

	mov.l #-1, 0:8[R7]
	mov.l #2, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	;; overflow
	mov #0FFFFFFFFh, R6
	mov #080000000h, R5
	mov #000000000h, R4

	mov.l #1, 0:8[R7]
	mov.l #-1, 64:8[R7]
	mov #_g_workArea + 0, R1
	mov #_g_workArea + 64, R2
	mov #1, R3
	rmpa.l

	rts

.global _v1Sandbox
_v1Sandbox:
    mov #sandbox_init, R1
    jsr R1
    mov #sandbox_abs, R1
    jsr R1
    mov #sandbox_mov, R1
    jsr R1
    mov #sandbox_cmp, R1
    jsr R1
    mov #sandbox_adc, R1
    jsr R1
    mov #sandbox_add, R1
    jsr R1
    mov #sandbox_fadd, R1
    jsr R1
    mov #sandbox_rmpa, R1
    jsr R1
    rts
