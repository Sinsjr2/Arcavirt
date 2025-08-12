using Composite = RX.CompositeFormatter;
using LEUInt = RX.LEUIntegerFormatter;
using LEInt = RX.LEIntegerFormatter;
using Skip = RX.SkipFormatter;
using BEUConnection = RX.BEUBitConnectionFormatter;
using System.Buffers;

namespace RX {

    public class Translate {

        public static readonly Translate Instance = new();

        readonly IAssemblyCode32Formatter opCodeFormatter;

        OpCodePair32 Create(OpCode kind,
                               string opcode,
                               IAssemblyCode32Formatter? operandFormatter = null
        ) {
            return new OpCodePair32(
                new OpCode32(kind.ToString(), (uint)kind, opcode),
                operandFormatter ?? EmptyFormatter.Instance);
        }

        OpCodePair32 CreateGroup(
            OpCode kind, string decodeOpCode, string[] encodeOpCodes, IAssemblyCode32Formatter? operandFormatter) {
            return new OpCodePair32(
                new OpCode32(kind.ToString(), (uint)kind, decodeOpCode, encodeOpCodes),
                operandFormatter ?? EmptyFormatter.Instance);
        }

        Translate() {

            var mov_rm = new Composite(new LEUInt(4, 2, 1), new BEUConnection(2, new[] { (1, 3), (4, 7) }), new LEUInt(12, 3, 2), new LEUInt(8, 3, 2), new Skip(2));
            var mov_mr = new Composite(new LEUInt(4, 2, 1), new BEUConnection(2, new[] { (1, 3), (4, 7) }), new LEUInt(12, 3, 2), new LEUInt(8, 3, 2), new Skip(2));
            var mov_im = new Composite(new LEUInt(0, 2, 1), new BEUConnection(2, new[] { (4, 0), (1, 7) }), new LEUInt(12, 3, 2), new Skip(2), new LEUInt(0, 8, 1), new Skip(1));
            var mov_rr = new Composite(new LEUInt(4, 2, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2));
            var mov_l_mr = new Composite(new LEUInt(4, 2, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new DisplacementValueFormatter(2, 0));
            var mov_r_dsp = new Composite(new LEUInt(4, 2, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new DisplacementValueFormatter(2, 2));
            var mov_mm = new Composite(new LEUInt(4, 2, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new TwoDisplacementValueFormatter(2, 0, 2));

            var movu_mr = new Composite(new LEUInt(3, 1, 1), new BEUConnection(2, new[] { (1, 3), (4, 7) }), new LEUInt(12, 3, 2), new LEUInt(8, 3, 2), new Skip(2));
            var movu_mr_ptr = new Composite(new LEUInt(2, 1, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new DisplacementValueFormatter(2, 0));

            var b1_bcnd_s = new Composite(new LEUInt(3, 1, 1), new LEUInt(0, 3, 1), new Skip(1));
            var b2_bcnd_b = new Composite(new LEUInt(0, 4, 2), new LEInt(8, 8, 2), new Skip(2));
            var b3_bcnd_w = new Composite(new LEUInt(0, 1, 1), new LEInt(8, 16, 3), new Skip(3));

            var b1_bra_s = new Composite(new LEUInt(0, 3, 1), new Skip(1));
            var b2_bra_b = new Composite(new LEInt(8, 8, 2), new Skip(2));
            var b3_bra_w = new Composite(new LEInt(8, 16, 3), new Skip(3));
            var b4_bra_a = new Composite(new LEInt(8, 24, 4), new Skip(4));

            var b1_imm8 = new Composite(new LEUInt(8, 8, 2), new Skip(2));

            var b2_rds = new Composite(new LEUInt(8, 4, 2), new Skip(2));
            var b2_rds_li = new Composite(new LEUInt(8, 4, 2), new ImmediateValueFormatter(2, 0));
            var b2_rd_rs_li = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new ImmediateValueFormatter(2, 0));
            var b2_rs2_rd_li = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new ImmediateValueFormatter(2, 0));
            var b2_rds_uimm4 = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2));
            var b2_rds_imm5 = new Composite(new BEUIntegerFormatter(4, 5, 2), new LEUInt(8, 4, 2), new Skip(2));
            var b2_rd_ld_ub = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new DisplacementValueFormatter(2, 0));
            var b2_ld_imm3 = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 3, 2), new DisplacementValueFormatter(2, 0));
            var b2_rs2_uimm4 = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2));
            var b2_r = new Composite(new LEUInt(8, 4, 2), new Skip(2));
            var b2_cr = new Composite(new LEUInt(8, 4, 2), new Skip(2));
            var b2_imm8 = new Composite(new LEUInt(16, 8, 3), new Skip(3));
            var b2_rs_rs2 = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2));
            var b2_r_imm8 = new Composite(new LEUInt(8, 4, 2), new LEUInt(16, 8, 3), new Skip(3));
            var b2_rd_rd = new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2));
            var b2_sz = new Composite(new LEUInt(8, 2, 2), new Skip(2));
            var b2_sz_r = new Composite(new LEUInt(12, 2, 2), new LEUInt(8, 4, 2), new Skip(2));

            var b3_rd_rs_rs2 = new Composite(
                new LEUInt(8, 4, 3),
                new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_rs_rs2 = new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_rd_rs = new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_rd_ld = new Composite(new LEUInt(14, 2, 3), new LEUInt(20, 4, 3),
                                         new LEUInt(16, 4, 3), new DisplacementValueFormatter(3, 8));
            var b3_rd_ld_ub = new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new DisplacementValueFormatter(3, 8));
            var b3_rs_rd = new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_rd_li = new Composite(new LEUInt(16, 4, 3), new ImmediateValueFormatter(3, 10));
            var b3_rd_ld_ul = new Composite(
                new LEUInt(20, 4 , 3), new LEUInt(16, 4 , 3), new DisplacementValueFormatter(3, 8));
            var b3_rd_rs_imm5 = new Composite(
                new LEUInt(8, 5, 3),
                new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_sz_ld_rd_cd = new Composite(new LEUInt(10, 2, 3), new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new DisplacementValueFormatter(3, 8));
            var b3_rds_imm5 = new Composite(new BEUIntegerFormatter(4, 5, 3), new LEUInt(16, 4, 3), new Skip(3));
            var b3_ld_rd_rs = new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new DisplacementValueFormatter(3, 8));
            var b3_ir = new Composite(new LEUInt(16, 4, 3), new Skip(3), new LEUInt(0, 32, 4), new Skip(4));
            var b3_r = new Composite(new LEUInt(16, 4, 3), new Skip(3));
            var b3_imm4 = new Composite(new LEUInt(16, 4, 3), new Skip(3));

            var b4_rd_ldmi = new Composite(new LEUInt(14, 2, 4), new LEUInt(28, 4, 4), new LEUInt(24, 4, 4), new DisplacementValueFormatter(4, 8));

            var conditionPatterns = Enumerable.Range(0, 14)
                .Select(i => Convert.ToString(i, 2).PadLeft(4, '0'))
                .ToArray();

            var table = new OpCodePair32[] {

                // ABS rd
                Create(OpCode.ABS_rd, "0111 1110 0010 ....", b2_rds),
                // ABS rs, rd
                Create(OpCode.ABS_rs_rd, "1111 1100 0000 1111 .... ....", b3_rd_rs),
                // ADC //imm, rd
                Create(OpCode.ADC_ir, "1111 1101 0111 ..00 0010 ....", b3_rd_li),
                // ADC rs, rd
                Create(OpCode.ADC_rr, "1111 1100 0000 1011 .... ....", b3_rd_rs),
                // ADC dsp[rs].l, rd
                // Note only mi==2 allowed.
                Create(OpCode.ADC_mr, "0000 0110 ..10 00.. 0000 0010 .... ....", b4_rd_ldmi),

                // ADD //uimm4, rd
                Create(OpCode.ADD_4irr, "0110 0010 .... ....", b2_rds_uimm4),
                // (2) ADD //imm, rs, rd
                //Create(OpCode.ADD_irr, "0111 00.. .... ....", b2_rd_rs_li),
                // ADD dsp[rs].ub, rd
                // (3) ADD rs, rd
                Create(OpCode.ADD_ub_rs_mr, "0100 10.. .... ....", b2_rd_ld_ub),
                // (3) ADD dsp[rs], rd
                Create(OpCode.ADD_mr, "0000 0110 ..00 10.. .... ....", b3_rd_ld),
                // (4)
                Create(OpCode.ADD_irrr, "0111 00.. .... ....", b2_rs2_rd_li),
                // ADD rs, rs2, rd
                Create(OpCode.ADD_rrr, "1111 1111 0010 .... .... ....", b3_rd_rs_rs2),

                // AND //uimm4, rd
                Create(OpCode.AND_4ir, "0110 0100 .... ....", b2_rds_uimm4),
                // AND //imm, rd
                Create(OpCode.AND_ir, "0111 01.. 0010 ....", b2_rds_li),
                // AND dsp[rs].ub, rd
                // AND rs, rd
                Create(OpCode.AND_ub_rs_mr, "0101 00.. .... ....", b2_rd_ld_ub),
                // AND dsp[rs], rd
                Create(OpCode.AND_mr, "0000 0110 ..01 00.. .... ....", b3_rd_ld),
                // AND rs, rs2, rd
                Create(OpCode.AND_rrr, "1111 1111 0100 .... .... ....", b3_rd_rs_rs2),

                // (1) BCLR //imm, dsp[rd] (rd, imm, ld[, dsp])
                Create(OpCode.BCLR_im, "1111 00.. .... 1...", b2_ld_imm3),
                // (2) BCLR rs, dsp[rd] (rd, rs, ld[, dsp])
                CreateGroup(OpCode.BCLR_rm, "1111 1100 0110 01.. .... ....", new[] {
                        "1111 1100 0110 0100 .... ....",
                        "1111 1100 0110 0101 .... ....",
                        "1111 1100 0110 0110 .... ...."
                    }, b3_rd_ld_ub),
                // (3) BCLR //imm, rs (imm, rd)
                Create(OpCode.BCLR_ir, "0111 101. .... ....", b2_rds_imm5),
                // (4) BCLR rs, rd (rd, rs)
                Create(OpCode.BCLR_rr, "1111 1100 0110 0111 .... ....", b3_rs_rd),
                // BCnd.s dsp
                Create(OpCode.BCnd_s, "0001 ....", b1_bcnd_s),
                // BCnd.b dsp
                CreateGroup(OpCode.BCnd_b, "0010 .... .... ....",
                        Enumerable.Range(0, 14).Select(x => "0010 " + Convert.ToString(x, 2).PadLeft(4, '0') + " .... ....").ToArray(),
                        b2_bcnd_b),
                // BCnd.w dsp
                Create(OpCode.BCnd_w, "0011 101 . .... .... .... ....", b3_bcnd_w),
                // (1) BMCnd //imm, dsp[rd] (imm, rd, cd, ld[, dest])
                CreateGroup(OpCode.BMCnd_im, "1111 1100 111. .... .... ....",
                            conditionPatterns
                            .Select(p => $"1111 1100 111. .... .... {p}")
                            .ToArray(),
                    new Composite(new LEUInt(10, 3, 2), new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new DisplacementValueFormatter(3, 8))),
                // (2) BMCnd //imm, rd ()
                CreateGroup(OpCode.BMCnd_ir, "1111 1101 111. .... .... ....",
                            conditionPatterns
                            .Select(p => $"1111 1101 111. .... {p} ....")
                            .ToArray()
                            ,new Composite(new LEUInt(8, 5, 2), new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3))),
                // (1) BNOT //imm, dsp[rd] (imm, rd, ld[, dsp])
                Create(OpCode.BNOT_im, "1111 1100 111. .... .... 1111",
                    new Composite(new LEUInt(10, 3, 2), new LEUInt(20, 4, 3), new DisplacementValueFormatter(3, 8))),
                // (2) BNOT rs, dsp[rd] (rd, rs, ld[, dsp])
                CreateGroup(OpCode.BNOT_rm, "1111 1100 0110 11.. .... ....", new[] {
                        "1111 1100 0110 1100 .... ....",
                        "1111 1100 0110 1101 .... ....",
                        "1111 1100 0110 1110 .... ...."
                    }, b3_rd_ld_ub),

                // (3) BNOT //imm, rd (imm, rd)
                Create(OpCode.BNOT_ir, "1111 1101 111. .... 1111 ....", new Composite(new LEUInt(8, 5, 2), new LEUInt(16, 4, 3), new Skip(3))),
                // (4) BNOT rs, rd (rd, rs)
                Create(OpCode.BNOT_rr, "1111 1100 0110 1111 .... ....", b3_rs_rd),
                // (1) BRA.s dsp
                Create(OpCode.BRA_s, "0000 1 ...", b1_bra_s),
                // (2) BRA.b dsp
                Create(OpCode.BRA_b, "0010 1110 .... ....", b2_bra_b),
                // (3) BRA.w dsp
                Create(OpCode.BRA_w, "0011 1000 .... .... .... ....", b3_bra_w),
                // (4) BRA.a dsp
                Create(OpCode.BRA_a, "0000 0100 .... .... .... .... .... ....", b4_bra_a),
                // (5) BRA.l rs
                Create(OpCode.BRA_l, "0111 1111 0100 ....", b2_r),

                Create(OpCode.BRK, "0000 0000"),

                // (1) BSET //imm, dsp[rd] (rd, imm, ld[, dsp])
                Create(OpCode.BSET_im, "1111 00.. .... 0...", b2_ld_imm3),
                // (2) BSET rs, dsp[rd] (rd, rs, ld[, dsp])
                CreateGroup(OpCode.BSET_rm, "1111 1100 0110 00.. .... ....", new[] {
                        "1111 1100 0110 0000 .... ....",
                        "1111 1100 0110 0001 .... ....",
                        "1111 1100 0110 0010 .... ...."
                    }, b3_rd_ld_ub),
                // (3) BSET //imm, rd (imm, rd)
                Create(OpCode.BSET_ir, "0111 100. .... ....", b2_rds_imm5),
                // (4) BSET rs, rd (rd, rs)
                Create(OpCode.BSET_rr, "1111 1100 0110 0011 .... ....", b3_rs_rd),

                // BSR.w dsp
                Create(OpCode.BSR_w, "0011 1001 .... .... .... ....", b3_bra_w),
                // BSR.a dsp
                Create(OpCode.BSR_a, "0000 0101 .... .... .... .... .... ....", b4_bra_a),
                // BSR.l rs
                Create(OpCode.BSR_l, "0111 1111 0101 ....", b2_r),

                // (1) BTST //imm, dsp[rd] (rs, imm, ld[, dsp])
                Create(OpCode.BTST_im, "1111 01.. .... 0...", b2_ld_imm3),
                // (2) BTST rs, dsp[rd] (rs2, rs, ld[, dsp])
                CreateGroup(OpCode.BTST_rm, "1111 1100 0110 10.. .... ....", new[] {
                        "1111 1100 0110 1000 .... ....",
                        "1111 1100 0110 1001 .... ....",
                        "1111 1100 0110 1010 .... ...."
                    }, b3_rd_ld_ub),
                // (3) BTST //imm, rd (imm, rs)
                Create(OpCode.BTST_ir, "0111 110. .... ....", b2_rds_imm5),
                // (4) BTST rs, rd (rs2, rs)
                Create(OpCode.BTST_rr, "1111 1100 0110 1011 .... ....", b3_rs_rd),

                // CLRSPW psw
                Create(OpCode.CLRPSW, "0111 1111 1011 ....",  b2_cr),

                // (1) CMP //uimm4, rs2
                Create(OpCode.CMP_4ir, "0110 0001 .... ....", b2_rs2_uimm4),
                // (2) CMP //uimm8, rs2
                Create(OpCode.CMP_8ir, "0111 0101 0101 ....", b2_r_imm8),
                // (3) CMP //imm, rs2 (rs, li, imm)
                Create(OpCode.CMP_ir, "0111 01.. 0000 ....", new Composite(new LEUInt(8, 4, 2), new ImmediateValueFormatter(2, 0))),
                // (4) CMP dsp[rs].ub, rs2
                // (4) CMP rs, rs2
                Create(OpCode.CMP_ub_rs_mr, "0100 01.. .... ....", b2_rd_ld_ub),
                // (4) CMP dsp[rs], rs2
                Create(OpCode.CMP_mr, "0000 0110 ..00 01.. .... ....", b3_rd_ld),

                // DIV //imm, rd
                Create(OpCode.DIV_ir, "1111 1101 0111 ..00 1000 ....", b3_rd_li),
                // DIV dsp[rs].ub, rd
                // DIV rs, rd
                Create(OpCode.DIV_ub_rs_mr, "1111 1100 0010 00.. .... ....", b3_rd_ld_ub),
                // DIV dsp[rs], rd
                Create(OpCode.DIV_mr, "0000 0110 ..10 00.. 0000 1000 .... ....", b4_rd_ldmi),

                // DIVU //imm, rd
                Create(OpCode.DIVU_ir, "1111 1101 0111 ..00 1001 ....", b3_rd_li),
                // DIVU dsp[rs].ub, rd
                // DIVU rs, rd
                Create(OpCode.DIVU_ub_rs_mr, "1111 1100 0010 01.. .... ....", b3_rd_ld_ub),
                // DIVU dsp[rs], rd
                Create(OpCode.DIVU_mr, "0000 0110 ..10 00.. 0000 1001 .... ....", b4_rd_ldmi),

                // EMUL //imm, rd
                Create(OpCode.EMUL_ir, "1111 1101 0111 ..00 0110 ....", b3_rd_li),
                // EMUL dsp[rs].ub, rd
                // EMUL rs, rd
                Create(OpCode.EMUL_ub_rs_mr, "1111 1100 0001 10.. .... ....", b3_rd_ld_ub),
                // EMUL dsp[rs], rd
                Create(OpCode.EMUL_mr, "0000 0110 ..10 00.. 0000 0110 .... ....", b4_rd_ldmi),

                // EMULU //imm, rd
                Create(OpCode.EMULU_ir, "1111 1101 0111 ..00 0111 ....", b3_rd_li),
                // EMULU dsp[rs].ub, rd
                // EMULU rs, rd
                Create(OpCode.EMULU_ub_rs_mr, "1111 1100 0001 11.. .... ....", b3_rd_ld_ub),
                // EMULU dsp[rs], rd
                Create(OpCode.EMULU_mr, "0000 0110 ..10 00.. 0000 0111 .... ....", b4_rd_ldmi),

                // FADD //imm, rd
                Create(OpCode.FADD_ir, "1111 1101 0111 0010 0010 ....", b3_ir),
                // FADD rs, rd
                // FADD dsp[rs], rd
                Create(OpCode.FADD_mr, "1111 1100 1000 10.. .... ....", b3_rd_ld_ul),

                // FCMP //imm, rd
                Create(OpCode.FCMP_ir, "1111 1101 0111 0010 0001 ....", b3_ir),
                // FCMP rs, rd
                // FCMP dsp[rs], rd
                Create(OpCode.FCMP_mr, "1111 1100 1000 01.. .... ....", b3_rd_ld_ul),

                // FDIV //imm, rd
                Create(OpCode.FDIV_ir, "1111 1101 0111 0010 0100 ....", b3_ir),
                // FDIV rs, rd
                // FDIV dsp[rs], rd
                Create(OpCode.FDIV_mr, "1111 1100 1001 00.. .... ....", b3_rd_ld_ul),

                // FMUL //imm, rd
                Create(OpCode.FMUL_ir, "1111 1101 0111 0010 0011 ....", b3_ir),
                // FMUL rs, rd
                // FMUL dsp[rs], rd
                Create(OpCode.FMUL_mr, "1111 1100 1000 11.. .... ....", b3_rd_ld_ul),

                // FSUB //imm, rd
                Create(OpCode.FSUB_ir, "1111 1101 0111 0010 0000 ....", b3_ir),
                // FSUB rs, rd
                // FSUB dsp[rs], rd
                Create(OpCode.FSUB_mr, "1111 1100 1000 00.. .... ....", b3_rd_ld_ul),

                // FTOI rs, rd
                // FTOI dsp[rs], rd
                Create(OpCode.FTOI, "1111 1100 1001 01.. .... ....", b3_rd_ld_ul),

                // INT //uimm8
                Create(OpCode.INT, "0111 0101 0110 0000 .... ....", b2_imm8),

                // ITOF dsp[rs].ub, rd
                // ITOF rs, rd
                Create(OpCode.ITOF_ub_rs, "1111 1100 0100 01.. .... ....", b3_rd_ld_ub),
                // ITOF dsp[rs], rd
                Create(OpCode.ITOF_mr, "0000 0110 ..10 00.. 0001 0001 .... ....", b4_rd_ldmi),

                // JMP rs
                Create(OpCode.JMP, "0111 1111 0000 ....", b2_r),
                // JSR rs
                Create(OpCode.JSR, "0111 1111 0001 ....", b2_r),

                // MACHI rs, rs2
                Create(OpCode.MACHI, "1111 1101 0000 0100 .... ....", b3_rs_rs2),
                // MACLO rs, rs2
                Create(OpCode.MACLO, "1111 1101 0000 0101 .... ....", b3_rs_rs2),

                // MAX //imm, rd
                Create(OpCode.MAX_ir, "1111 1101 0111 ..00 0100 ....", b3_rd_li),
                // MAX dsp[rs].ub, rd
                // MAX rs, rd
                Create(OpCode.MAX_ub_rs_mr, "1111 1100 0001 00.. .... ....", b3_rd_ld_ub),
                // MAX dsp[rs], rd
                Create(OpCode.MAX_mr, "0000 0110 ..10 00.. 0000 0100 .... ....", b4_rd_ldmi),

                // MIN //imm, rd
                Create(OpCode.MIN_ir, "1111 1101 0111 ..00 0101 ....", b3_rd_li),
                // MIN dsp[rs].ub, rd
                // MIN rs, rd
                Create(OpCode.MIN_ub_rs_mr, "1111 1100 0001 01.. .... ....", b3_rd_ld_ub),
                // MIN dsp[rs], rd
                Create(OpCode.MIN_mr, "0000 0110 ..10 00.. 0000 0101 .... ....", b4_rd_ldmi),

                // (1) MOV.<bwl> rs, dsp5[rd] (sz, dsp, rd, rs)
                CreateGroup(OpCode.MOV_rm, "10.. 0... .... ....", new[] {
                        "1000 0... .... ....",
                        "1001 0... .... ....",
                        "1010 0... .... ...."
                    },
                    mov_rm),
                // (2) MOV.<bwl> dsp5[rs], rd (sz, dsp, rs, rd)
                CreateGroup(OpCode.MOV_mr, "10.. 1... .... ....", new[] {
                        "1000 1... .... ....",
                        "1001 1... .... ....",
                        "1010 1... .... ...."
                }, mov_mr),
                // (3) MOV.l //uimm4, rd
                Create(OpCode.MOV_4ir, "0110 0110 .... ....", new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2))),
                // (4) MOV.<bwl> //imm8, dsp5[rd] (sz, dsp, rd, imm)
                CreateGroup(OpCode.MOV_im, "0011 11.. .... .... .... ....", new[] {
                        "0011 1100 .... .... .... ....",
                        "0011 1101 .... .... .... ....",
                        "0011 1110 .... .... .... ...."
                }, mov_im),
                // (5) MOV.l //imm8, rd
                Create(OpCode.MOV_u8ir, "0111 0101 0100 ....", b2_r_imm8),
                // (6) MOV.l //mm8, rd
                Create(OpCode.MOV_ir, "1111 1011 .... ..10", new Composite(new LEUInt(12, 4, 2), new ImmediateValueFormatter(2, 10))),
                // (7) MOV.<bwl> rs rd (sz. rs, rd)
                CreateGroup(OpCode.MOV_rr, "11.. 1111 .... ....", new[] {
                        "1100 1111 .... ....",
                        "1101 1111 .... ....",
                        "1110 1111 .... ...."
                }, mov_rr),
                // (8) MOV.<bwl> //imm, [rd] (rd, sz, ld, li, imm)
                Create(OpCode.MOV_im_p, "1111 1000 .... ...."    , new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 2, 2), new LEUInt(0, 2, 1), new ImmediateValueFormatter(2, 10))),
                // (8) MOV.<bwl> //imm, dsp8[rd] (rd, sz, ld, dsp, li, imm)
                Create(OpCode.MOV_im_dsp8, "1111 1001 .... ...." , new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 2, 2), new LEUInt(0, 2, 1), new LEUInt(16, 8, 3), new ImmediateValueFormatter(3, 10))),
                // (8) MOV.<bwl> //imm, dsp16[rd] (rd, sz, ld, dsp, li, imm)
                Create(OpCode.MOV_im_dsp16, "1111 1010 .... ....", new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 2, 2), new LEUInt(0, 2, 1), new LEUInt(16, 16, 4), new ImmediateValueFormatter(4, 10))),
                // (9) (sz, rs, rd, ld, dsp)
                // (9) MOV.<bwl> ([Rs]/dsp:8[Rs]/dsp:16[Rs]) rd
                CreateGroup(OpCode.MOV_l_mr, "11.. 11.. .... ....", new[] {
                        "1100 1100 .... ....",
                        "1100 1101 .... ....",
                        "1100 1110 .... ....",
                        "1101 1100 .... ....",
                        "1101 1101 .... ....",
                        "1101 1110 .... ....",
                        "1110 1100 .... ....",
                        "1110 1101 .... ....",
                        "1110 1110 .... ....",
                    }, mov_l_mr),
                // (10) MOV.<bwl> [ri,rb], rd (sz, ri, rb, rd)
                Create(OpCode.MOV_ar, "1111 1110 01.. .... .... ....", new Composite(new LEUInt(12, 2, 2), new LEUInt(8, 4, 2), new Skip(2), new LEUInt(4, 4, 1), new LEUInt(0, 4, 1), new Skip(1))),
                // (11) MOV.<bwl> rs ([Rd]/dsp:8[Rd]/dsp:16[Rd]) (sz, rd, rs, ld[, dsp])
                CreateGroup(OpCode.MOV_r_dsp, "11.. ..11 .... ....", new string[] {
                        "1100 0011 .... ....",
                        "1100 0111 .... ....",
                        "1100 1011 .... ....",
                        "1101 0011 .... ....",
                        "1101 0111 .... ....",
                        "1101 1011 .... ....",
                        "1110 0011 .... ....",
                        "1110 0111 .... ....",
                        "1110 1011 .... ....",
                    }, mov_r_dsp),
                // (12) MOV.<bwl> rs, [ri,rb] (sz, ri, rb, rs)
                Create(OpCode.MOV_ra, "1111 1110 00.. .... .... ....", new Composite(new Skip(1), new LEUInt(4, 2, 1), new LEUInt(0, 4, 1), new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2))),
                // (13) (sz, rs, rd, ldd, ldd_dsp, lds, lds_dsp)
                // (13) MOV.<blw> ([rs]/dsp:8[rd]/dsp:16[rd]), ([rd]/dsp:8[rd]/dsp:16[rd])
                CreateGroup(OpCode.MOV_mm, "11.. .... .... ....", new[] {
                        "1100 0000 .... ....",
                        "1100 0100 .... ....",
                        "1100 1000 .... ....",
                        "1100 0001 .... ....",
                        "1100 0101 .... ....",
                        "1100 1001 .... ....",
                        "1100 0010 .... ....",
                        "1100 0110 .... ....",
                        "1100 1010 .... ....",
                        "1101 0000 .... ....",
                        "1101 0100 .... ....",
                        "1101 1000 .... ....",
                        "1101 0001 .... ....",
                        "1101 0101 .... ....",
                        "1101 1001 .... ....",
                        "1101 0010 .... ....",
                        "1101 0110 .... ....",
                        "1101 1010 .... ....",
                        "1110 0000 .... ....",
                        "1110 0100 .... ....",
                        "1110 1000 .... ....",
                        "1110 0001 .... ....",
                        "1110 0101 .... ....",
                        "1110 1001 .... ....",
                        "1110 0010 .... ....",
                        "1110 0110 .... ....",
                        "1110 1010 .... ....",
                }, mov_mm),
                // (14) MOV.l rs, [rd+] (ad, sz, rd, rs)
                // (14) MOV.l rs, [-rd]
                Create(OpCode.MOV_rp, "1111 1101 0010 0... .... ....", new Composite(new LEUInt(10, 1, 2), new LEUInt(8, 2, 2), new Skip(2), new LEUInt(4, 4, 1), new LEUInt(0, 4, 1), new Skip(1))),
                // (15) MOV.l [rs+], rd
                // (15) MOV.l [-rs], rd
                Create(OpCode.MOV_pr, "1111 1101 0010 1... .... ....", new Composite(new LEUInt(10, 2, 2), new LEUInt(8, 2, 2), new Skip(2), new LEUInt(4, 4, 1), new LEUInt(0, 4, 1), new Skip(1))),

                // (1) MOVU.<bw> dsp5[rs], rd (sz, dsp, rs, rd)
                Create(OpCode.MOVU_dsp5_mr, "1011 .... .... ....", movu_mr),
                // (2) MOVU.<bw> [rs], rd (sz, rs, rd, ld [, dsp])
                Create(OpCode.MOVU_mr, "0101 1... .... ....", movu_mr_ptr),
                // (3) MOVU.<bw> [ri, rb], rd (sz, ri, rb, rd)
                Create(OpCode.MOVU_ar, "1111 1110 110 .... .... .... ....",
                       new Composite(new LEUInt(12, 1, 2), new LEUInt(8, 4, 2), new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3))),
                // (4) MOVU.<bw> [rs+], rd (ad, sz, rs, rd)
                Create(OpCode.MOVU_pr, "1111 1101 0011 1.0. .... ....",
                       new Composite(new LEUInt(10, 2, 2), new LEUInt(8, 1, 2), new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3))),

                // MUL //uimm4, rd
                Create(OpCode.MUL_4ir, "0110 0011 .... ....", b2_rds_uimm4),
                // MUL //imm4, rd
                Create(OpCode.MUL_ir, "0111 01.. 0001 ....", b2_rds_li),
                // MUL dsp[rs].ub, rd
                // MUL rs, rd
                Create(OpCode.MUL_ub_rs_mr, "0100 11.. .... ....", b2_rd_ld_ub),
                // MUL dsp[rs], rd
                Create(OpCode.MUL_mr, "0000 0110 ..00 11.. .... ....", b3_rd_ld),
                // MUL rs, rs2, rd
                Create(OpCode.MUL_rrr, "1111 1111 0011 .... .... ....", b3_rd_rs_rs2),

                // MULHI rs, rs2
                Create(OpCode.MULHI, "1111 1101 0000 0000 .... ....", b3_rs_rs2),
                // MULLO rs, rs2
                Create(OpCode.MULLO, "1111 1101 0000 0001 .... ....", b3_rs_rs2),

                // MVFACHI rd
                Create(OpCode.MVFACHI, "1111 1101 0001 1111 0000 ....", b3_r),
                // MVFACMI rd
                Create(OpCode.MVFACMI, "1111 1101 0001 1111 0010 ....", b3_r),

                // MVFC cr, rd
                Create(OpCode.MVFC, "1111 1101 0110 1010 .... ....", new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3))),

                // MVTACHI rs
                Create(OpCode.MVTACHI, "1111 1101 0001 0111 0000 ....", b3_r),
                // MVTACLO rs
                Create(OpCode.MVTACLO, "1111 1101 0001 0111 0001 ....", b3_r),

                // MVTC //imm, cr
                Create(OpCode.MVTC_i, "1111 1101 0111 ..11 0000 ....", new Composite(new LEUInt(16, 4, 3), new ImmediateValueFormatter(3, 10))),
                // MVMicrosoft.Windows.Simulator.ClientTC rs, cr
                Create(OpCode.MVTC_r, "1111 1101 0110 1000 .... ....", new Composite(new LEUInt(20, 4, 3), new LEUInt(16, 4, 3), new Skip(3))),

                // MVTIPL //imm
                Create(OpCode.MVTIPL, "0111 0101 0111 0000 0000 ....", b3_imm4),

                // NEG rd
                Create(OpCode.NEG_rd, "0111 1110 0001 ....", b2_rds),
                // NEG rs, rd
                Create(OpCode.NEG_rs_rd, "1111 1100 0000 0111 .... ....", b3_rd_rs),
                Create(OpCode.NOP, "0000 0011", new Skip(1)),

                // NOT rd
                Create(OpCode.NOT_rd, "0111 1110 0000 ....", b2_rds),
                // NOT rs, rd
                Create(OpCode.NOT_rs_rd, "1111 1100 0011 1011 .... ....", b3_rd_rs),

                // OR //uimm4, rd
                Create(OpCode.OR_4ir, "0110 0101 .... ....", b2_rds_uimm4),
                // OR //imm, rd
                Create(OpCode.OR_ir, "0111 01.. 0011 ....", b2_rds_li),
                // OR dsp[rs].ub, rd
                // OR rs, rd
                Create(OpCode.OR_ub_rs_mr, "0101 01.. .... ....", b2_rd_ld_ub),
                // OR dsp[rs], rd
                Create(OpCode.OR_mr, "0000 0110 .. 0101 .. .... ....", b3_rd_ld),
                // OR rs, rs2, rd
                Create(OpCode.OR_rrr, "1111 1111 0101 .... .... ....", b3_rd_rs_rs2),

                // POP cr
                Create(OpCode.POPC, "0111 1110 1110 ....", b2_cr),
                // POP rd-rd2
                Create(OpCode.POPM, "0110 1111 .... ....", b2_rd_rd),

                // POP rd
                // PUSH.<bwl> rs
                Create(OpCode.POP, "0111 1110 1011 ....", b2_r),
                CreateGroup(OpCode.PUSH_r, "0111 1110 10.. ....", new[] {
                        "0111 1110 1000 ....",
                        "0111 1110 1001 ....",
                        "0111 1110 1010 ...."
                    }, b2_sz_r),

                // PUSH.<bwl> dsp[rs]
                Create(OpCode.PUSH_m, "1111 01.. .... 10..", new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 2, 2), new DisplacementValueFormatter(2, 0))),
                // PUSH cr
                Create(OpCode.PUSHC, "0111 1110 1100 ....", b2_cr),
                // PUSHM rs-rs2
                Create(OpCode.PUSHM, "0110 1110 .... ....", b2_rs_rs2),

                // RACW //imm
                Create(OpCode.RACW, "1111 1101 0001 1000 000. 0000", new Composite(new LEUInt(20, 1, 3), new Skip(3))),

                // REVL rs,rd
                Create(OpCode.REVL, "1111 1101 0110 0111 .... ....", b3_rd_rs),
                // REVW rs,rd
                Create(OpCode.REVW, "1111 1101 0110 0101 .... ....", b3_rd_rs),

                // SMOVF
                // RPMA.<bwl>
                Create(OpCode.SMOVF, "0111 1111 1000 1111", new Skip(2)),
                CreateGroup(OpCode.RMPA, "0111 1111 1000 11..", new[] {
                        "0111 1111 1000 1100",
                        "0111 1111 1000 1101",
                        "0111 1111 1000 1110"
                    }, b2_sz),

                // ROLC rd
                Create(OpCode.ROLC, "0111 1110 0101 ....", b2_rds),
                // RORC rd
                Create(OpCode.RORC, "0111 1110 0100 ....", b2_rds),

                // ROTL //imm, rd
                Create(OpCode.ROTL_ir, "1111 1101 0110 111. .... ....", b3_rds_imm5),
                // ROTL rs, rd
                Create(OpCode.ROTL_rr, "1111 1101 0110 0110 .... ....", b3_rd_rs),

                // ROTR //imm, rd
                Create(OpCode.ROTR_ir, "1111 1101 0110 110. .... ....", b3_rds_imm5),
                // ROTR //imm, rd
                Create(OpCode.ROTR_rr, "1111 1101 0110 0100 .... ....", b3_rd_rs),

                // ROUND rs,rd
                // ROUND dsp[rs],rd
                Create(OpCode.ROUND, "1111 1100 1001 10 .. .... ....", b3_ld_rd_rs),
                Create(OpCode.RTE, "0111 1111 1001 0101", new Skip(2)),
                Create(OpCode.RTFI, "0111 1111 1001 0100", new Skip(2)),
                Create(OpCode.RTS, "0000 0010", new Skip(1)),

                // RTSD //imm
                Create(OpCode.RTSD_i, "0110 0111", b1_imm8),
                // RTSD //imm, rd-rd2
                Create(OpCode.RTSD_irr, "0011 1111 .... .... .... ....", new Composite(new LEUInt(12, 4, 2), new LEUInt(8, 4, 2), new Skip(2), new LEUInt(0, 8, 1), new Skip(1))),

                // SAT rd
                Create(OpCode.SAT, "0111 1110 0011 ....", b2_rds),
                // SATR
                Create(OpCode.SATR, "0111 1111 1001 0011", new Skip(2)),

                // SBB rs, rd
                Create(OpCode.SBB_rr, "1111 1100 0000 0011 .... ....", b3_rd_rs),
                // SBB dsp[rs].l, rd
                // Note only mi==2 allowed.
                Create(OpCode.SBB_mr, "0000 0110 1010 00.. 0000 0000 .... ....", b4_rd_ldmi),

                // SCCnd dsp[rd]
                // SCCnd rd
                Create(OpCode.SCCnd, "1111 1100 1101 .... .... ....", b3_sz_ld_rd_cd),
                // SETPSW psw
                Create(OpCode.SETPSW, "0111 1111 1010 ....", b2_cr),

                // SHAR //imm, rd
                Create(OpCode.SHAR_5irr, "0110 101. .... ....", b2_rds_imm5),
                // SHAR //imm, rs, rd
                Create(OpCode.SHAR_irr, "1111 1101 101. .... .... ....", b3_rd_rs_imm5),
                // SHAR rs, rd
                Create(OpCode.SHAR_rr, "1111 1101 0110 0001 .... ....", b3_rd_rs),

                // SHLL //imm, rd
                Create(OpCode.SHLL_5irr, "0110 110. .... ....", b2_rds_imm5),
                // SHLL //imm, rs, rd
                Create(OpCode.SHLL_irr, "1111 1101 110. .... .... ....", b3_rd_rs_imm5),
                // SHLL rs, rd
                Create(OpCode.SHLL_rr, "1111 1101 0110 0010 .... ....", b3_rd_rs),

                // SHLR //imm, rd
                Create(OpCode.SHLR_5irr, "0110 100. .... ....", b2_rds_imm5),
                // SHLR //imm, rs, rd
                Create(OpCode.SHLR_irr, "1111 1101 100. .... .... ....", b3_rd_rs_imm5),
                // SHLR rs, rd
                Create(OpCode.SHLR_rr, "1111 1101 0110 0000 .... ....", b3_rd_rs),

                // SMOVB
                // SSTR.<bwl>
                Create(OpCode.SMOVB, "0111 1111 1000 1011", new Skip(2)),
                CreateGroup(OpCode.SSTR, "0111 1111 1000 10..", new[] {
                        "0111 1111 1000 1000",
                        "0111 1111 1000 1001",
                        "0111 1111 1000 1010"
                    }, b2_sz),

                // STNZ //imm, rd
                Create(OpCode.STNZ, "1111 1101 0111 ..00 1111 ....", b3_rd_li),
                // STZ //imm, rd
                Create(OpCode.STZ, "1111 1101 0111 ..00 1110 ....", b3_rd_li),

                // SUB //uimm4, rd
                Create(OpCode.SUB_ir, "0110 0000 .... ....", b2_rds_uimm4),
                // SUB dsp[rs].ub, rd
                // SUB rs, rd
                Create(OpCode.SUB_ub_rs_mr, "0100 00.. .... ....", b2_rd_ld_ub),
                // SUB dsp[rs], rd
                Create(OpCode.SUB_mr, "0000 0110 ..00 00.. .... ....", b3_rd_ld),
                // SUB rs, rs2, rd
                Create(OpCode.SUB_rrr, "1111 1111 0000 .... .... ....", b3_rd_rs_rs2),

                // SCMPU
                // SUNTIL.<bwl>
                Create(OpCode.SCMPU, "0111 1111 1000 0011", new Skip(2)),
                CreateGroup(OpCode.SUNTIL, "0111 1111 1000 00..", new[] {
                        "0111 1111 1000 0000",
                        "0111 1111 1000 0001",
                        "0111 1111 1000 0010"
                    }, b2_sz),

                // SMOVU
                // SWHILE.<bwl>
                Create(OpCode.SMOVU, "0111 1111 1000 0111", new Skip(2)),
                CreateGroup(OpCode.SWHILE, "0111 1111 1000 01..", new[] {
                        "0111 1111 1000 0100",
                        "0111 1111 1000 0101",
                        "0111 1111 1000 0110"
                    }, b2_sz),

                // TST //imm, rd
                Create(OpCode.TST_ir, "1111 1101 0111 ..00 1100 ....", b3_rd_li),
                // TST dsp[rs].ub, rd
                // TST rs, rd
                Create(OpCode.TST_ub_rs_mr, "1111 1100 0011 00.. .... ....", b3_rd_ld_ub),
                // TST dsp[rs], rd
                Create(OpCode.TST_mr, "0000 0110 ..10 00.. 0000 1100 .... ....", b4_rd_ldmi),

                Create(OpCode.WAIT, "0111 1111 1001 0110", new Skip(2)),

                // XCHG rs, rd (rs, rd, ld[, dsp])
                // XCHG dsp[rs].ub, rd
                Create(OpCode.XCHG_ub_rs_mr, "1111 1100 0100 00.. .... ....", b3_rd_ld_ub),
                // XCHG dsp[rs], rd (mi, rs, rd, ld[, dsp])
                Create(OpCode.XCHG_mr, "0000 0110 ..10 00.. 0001 0000 .... ....", b4_rd_ldmi),

                // XOR //imm, rd
                Create(OpCode.XOR_ir, "1111 1101 0111 ..00 1101 ....", b3_rd_li),
                // XOR dsp[rs].ub, rd
                // XOR rs, rd
                Create(OpCode.XOR_ub_rs_mr, "1111 1100 0011 01.. .... ....", b3_rd_ld_ub),
                // XOR dsp[rs], rd
                Create(OpCode.XOR_mr, "0000 0110 ..10 00.. 0000 1101 .... ....", b4_rd_ldmi),
            };

            opCodeFormatter = new OpCode32PatternMatchFormatter(0xFFFF_FFFFu, table);
        }

        public void CreateBinary(Instruction32 instruction, ref AssemblyWriter writer) {
            var queue = new Queue<uint>();
            queue.Enqueue(instruction.OpcodeKind);
            foreach (var operand in instruction.Operands) {
                queue.Enqueue(operand);
            }
            opCodeFormatter.Serialize(queue, ref writer);
        }

        public void ParseAssembly(ref Reader reader, List<uint> result) {
            opCodeFormatter.Deserialize(ref reader, result);
        }
    }
}
