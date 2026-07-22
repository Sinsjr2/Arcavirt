using Clock;
using GPIO;

namespace Peripheral.Renesas;

public class RSPI {
    static readonly int  SPCR_SPMS   = 1 << 0;
    static readonly int  SPCR_TXMD   = 1 << 1;
    static readonly int  SPCR_MODFEN = 1 << 2;
    static readonly int  SPCR_MSTR   = 1 << 3;
    static readonly int  SPCR_SPEIE  = 1 << 4;
    static readonly int  SPCR_SPTIE  = 1 << 5;
    static readonly int  SPCR_SPE    = 1 << 6;
    static readonly int  SPCR_SPRIE  = 1 << 7;

    static readonly int SSL0P =        1 << 0;
    static readonly int SSL1P =        1 << 1;
    static readonly int SSL2P =        1 << 2;
    static readonly int SSL3P =        1 << 3;

    static readonly int SPPCR_SPLP  =  1 << 0;
    static readonly int SPPCR_SPLP2 =  1 << 1;
    static readonly int SPPCR_SPOM  =  1 << 2;
    static readonly int SPPCR_MOIFV =  1 << 4;
    static readonly int SPPCR_MOIFE =  1 << 5;

    static readonly int SPSR_OVRF    =    1 << 0;
    static readonly int SPSR_IDLNF   =    1 << 1;
    static readonly int SPSR_MODF    =    1 << 2;
    static readonly int SPSR_PERF    =    1 << 3;
    static readonly int SPSR_SPTEF   =    1 << 5;
    static readonly int SPSR_SPRF    =    1 << 7;
    static readonly int SPSCR_SPSLN_MSK  = 7;
    static readonly int SPSSR_SPCP_MSK   = 7;
    static readonly int SPSSR_SPECM_MSK  = 7 << 4;
    static readonly int SPDCR_SPFC_MSK   = 3 << 0;
    static readonly int SPDCR_SPFC_SLSEL = 3 << 2;
    static readonly int SPDCR_SPRDTD     = 1 << 4;
    static readonly int SPDCR_SPLW       = 1 << 5;
    static readonly int SPCKD_SCKDL_MSK  = 7 << 0;
    static readonly int SSLND_SLNDL_MSK  = 7 << 0;
    static readonly int SPND_SPNDL_MSK   = 7 << 0;
    static readonly int SPCR2_SPPE       = 1 << 0;
    static readonly int SPCR2_SPOE       = 1 << 1;
    static readonly int SPCR2_SPIIE      = 1 << 2;
    static readonly int SPCR2_PTE        = 1 << 3;
    static readonly int SPCMD_CPHA       = 1 << 0;
    static readonly int SPCMD_CPOL       = 1 << 1;
    static readonly int SPCMD_BRDV_MSK   = 3 << 2;
    static readonly int SPCMD_BRDV_SHIFT = 2;
    static readonly int SPCMD_SSLA_MSK   = 3 << 4;
    static readonly int SPCMD_SSLKP      = 1 << 7;
    static readonly int SPCMD_SPB_MSK    = 0xf << 8;
    static readonly int SPCMD_SPB_SHIFT  = 8;
    static readonly int SPCMD_LSBF       = 1 << 12;
    static readonly int SPCMD_SPNDEN     = 1 << 13;
    static readonly int SPCMD_SLNDEN     = 1 << 14;
    static readonly int SPCMD_SCKDEN     = 1 << 15;

    // #define ICODE_SIZE  (512)

    const uint INSTR_MOSI_HIGH   =  0x01000000;
    const uint INSTR_MOSI_LOW    =  0x02000000;
    const uint INSTR_MOSI_OPEN   =  0x03000000;
    const uint INSTR_MISO_HIGH   =  0x04000000;
    const uint INSTR_MISO_LOW    =  0x05000000;
    const uint INSTR_MISO_OPEN   =  0x06000000;
    const uint INSTR_CLK_HIGH    =  0x07000000;
    const uint INSTR_CLK_LOW     =  0x08000000;
    static uint INSTR_CS_HIGH(uint csNr) => 0x09000000 | csNr;
    static uint INSTR_CS_LOW(uint csNr)  => 0x0a000000 | csNr;
    static uint INSTR_NDELAY(uint ns)   => 0x0b000000 | ns;
    static uint INSTR_CLKDELAY(uint cyc) => 0x0c000000 | cyc;
    const uint INSTR_SHIFTOUT_MOSI    = 0x0d000000;
    const uint INSTR_SAMPLE_MOSI      = 0x0e000000;
    const uint INSTR_SAMPLE_MISO      = 0x10000000;
    const uint INSTR_TRANSFER_RXSHIFT = 0x11000000;
    const uint INSTR_RELOAD_TXSHIFT   = 0x12000000;
    const uint INSTR_TRIGGER_SPRI     = 0x13000000;
    const uint INSTR_TRIGGER_SPTI     = 0x14000000;
    const uint INSTR_TRIGGER_SPII     = 0x15000000;
    const uint INSTR_ENDSCRIPT        = 0x16000000;

    const uint RXBUF_LONGWORDS = 4;
    const uint TXBUF_LONGWORDS = 4;
    uint RXBUF_WP() => rxBufWp % ((SPDCR.Value & 3u) + 1u);
    uint RXBUF_RP() => rxBufRp & (RXBUF_LONGWORDS - 1);
    uint TXBUF_WP() => txBufWp & (TXBUF_LONGWORDS - 1);
    uint TXBUF_RP() => txBufRp & (TXBUF_LONGWORDS - 1);

    public uint[] icode = new uint[512];
    public uint instrP;
    public uint instrWp;
    // public SubClock clkIn;
    public SubClock clkBase; /* Rate behind divisor by SPBR is called Base Bitrate in 38.2.14 */
    public CountTimer instrDelayTimer;

    public OutputSignalBit sigMosi;
    public SignalBit sigMiso;

    // #if 0
    //     SigNode *sigShiftOut; /* Connected to MOSI in master mode or to Miso in Slave mode */
    //     SigNode *sigShiftIn;
    // #endif

    public OutputSignalBit sigSclk;
    public OutputSignalBit[] sigSSL = new OutputSignalBit[4];
    public OutputSignalBit sigIrqSpti; /* SPI TX interrupt */
    public OutputSignalBit sigIrqSpri; /* SPI RX interrupt */
    public OutputSignalBit sigIrqSpii; /* SPI Idle interrupt */
    public OutputSignalBit sigIrqSpei; /* SPI Error interrupt */

    public uint txShiftReg;    /* Maybe uint64_t because of parity bit ? */
    public uint txShiftCnt;
    public uint txShiftEmpty;
    public uint[] txBuf32 = new uint[4];

    public uint[] rxBuf32 = new uint[4];
    public uint txBufWp;
    public uint txBufRp;
    public uint rxShiftReg;
    public uint rxBufWp;
    public uint rxBufRp;
    public readonly RegisterValue32<byte> SPCR;
    public readonly RegisterValue32<byte> SSLP;
    public readonly RegisterValue32<byte> SPPCR;
    public readonly RegisterValue32<byte> SPSR;
    public readonly RegisterValue32<byte> SPSCR;
    public readonly RegisterValue32<byte> SPSSR;
    public readonly RegisterValue32<byte> SPBR;
    public readonly RegisterValue32<byte> SPDCR;
    public readonly RegisterValue32<byte> SPCKD;
    public readonly RegisterValue32<byte> SSLND;
    public readonly RegisterValue32<byte> SPND;
    public readonly RegisterValue32<byte> SPCR2;
    public IReadOnlyList<RegisterValue32<ushort>> SPCMD =
        Enumerable.Range(0, 8)
        .Select(_ => new RegisterValue32<ushort>(0))
        .ToArray();

    // Translate the spd RSPI Data Length Setting field to a number of Bits
    static readonly byte[] gSpbToLength = new byte[] {
        /* 0  */ 20,
        /* 1  */ 24,
        /* 2  */ 32,
        /* 3  */ 32,
        /* 4  */  8,
        /* 5  */  8,
        /* 6  */  8,
        /* 7  */  8,
        /* 8  */  9,
        /* 9  */ 10,
        /* 10 */ 11,
        /* 11 */ 12,
        /* 12 */ 13,
        /* 13 */ 14,
        /* 14 */ 15,
        /* 15 */ 16,
    };

    void TriggerTxInterrupt() {
        sigIrqSpti.SetOutputSignal(false);
        sigIrqSpti.SetOutputSignal(true);
    }

     void TriggerRxInterrupt() {
        sigIrqSpri.SetOutputSignal(false);
        sigIrqSpri.SetOutputSignal(true);
    }

     void TriggerIdleInterrupt() {
        sigIrqSpii.SetOutputSignal(false);
        sigIrqSpii.SetOutputSignal(true);
    }

     bool EvalInstr() {
        uint icode;
        uint instr, arg;
        bool retval = false;
        RSPI rspi = this;
        if (rspi.instrP >= rspi.icode.Length) {
            Console.Error.WriteLine("Illegal instruction pointer in %s", nameof(EvalInstr));
            // Returns true if no more instructions
            return true;
        }
        icode = rspi.icode[rspi.instrP];
        instr = icode & 0xff000000;
        arg = icode & 0xffffff;
        rspi.instrP++;
        switch (instr) {
            case INSTR_MOSI_HIGH:
                rspi.sigMosi.SetOutputSignal(true);
                break;

            case INSTR_MOSI_LOW:
                rspi.sigMosi.SetOutputSignal(false);
                break;

            case INSTR_MOSI_OPEN:
                rspi.sigMosi.SetOutputSignal(false);
                break;

            case INSTR_MISO_HIGH:
                rspi.sigMiso.SetOutputSignal(true);
                break;

            case INSTR_MISO_LOW:
                rspi.sigMiso.SetOutputSignal(false);
                break;

            case INSTR_MISO_OPEN:
                rspi.sigMiso.SetOutputSignal(false);
                break;

            case INSTR_CLK_HIGH:
                // if (master) { }
                rspi.sigSclk.SetOutputSignal(true);
                break;

            case INSTR_CLK_LOW:
                // if (master) { }
                rspi.sigSclk.SetOutputSignal(false);
                break;

            case var x when (instr == INSTR_CS_HIGH(0)):
                if (arg < rspi.sigSSL.Length) {
                    rspi.sigSSL[arg].SetOutputSignal(true);
                }
                break;
            case var x when (instr == INSTR_CS_LOW(0)):
                if (arg < rspi.sigSSL.Length) {
                    rspi.sigSSL[arg].SetOutputSignal(false);
                }
                break;
            case var x when (instr == INSTR_NDELAY(0)):
                rspi.instrDelayTimer.SetTrigger(0, new CountTimerTrigger(TimerTriggerKind.ResetZero, arg));
                break;

                // Delay by nubmer of base clock cycles
                // The SPCMD specific multiplication is done at compile time.
            case var x when (instr == INSTR_CLKDELAY(0)):
                {
                    rspi.instrDelayTimer.SetTrigger(0, new CountTimerTrigger(TimerTriggerKind.ResetZero, arg));
                }
                retval = true;
                break;

                /* Bit order is converted on overtake from shiftreg to fifo */
            case INSTR_SAMPLE_MOSI:
                {
                    if (rspi.sigMiso.InputSignal) {
                        rspi.rxShiftReg  = (rspi.rxShiftReg << 1) | 1;
                    } else {
                        rspi.rxShiftReg  = (rspi.rxShiftReg << 1) | 0;
                    }
                }
                break;

                // MSB First variant shifts in on the right side
            case INSTR_SAMPLE_MISO:
                {
                    if (rspi.sigMiso.InputSignal) {
                        rspi.rxShiftReg  = (rspi.rxShiftReg << 1) | 1;
                    } else {
                        rspi.rxShiftReg  = (rspi.rxShiftReg << 1) | 0;
                    }
                }
                break;

            case INSTR_TRANSFER_RXSHIFT:
                break;

            case INSTR_RELOAD_TXSHIFT:
                break;

            case INSTR_TRIGGER_SPRI:
                TriggerRxInterrupt();
                break;

            case INSTR_TRIGGER_SPTI:
                TriggerTxInterrupt();
                break;

            case INSTR_TRIGGER_SPII:
                TriggerIdleInterrupt();
                break;

            case INSTR_ENDSCRIPT:
                retval = true;
                break;

            default:
                break;
        }
        return retval;
    }

    void InterpLoop() {
        while (EvalInstr());
    }

    static void update_rx_interrupt() {
        // if(rspi.regSPCR & SPCR_TI) {};
    }

    // #if 0
    // static void update_tx_interrupt(RxRSpi rspi) {
    //     /*
    //      */
    // }
    // #endif

    void InstrAppend(uint icode) {
        if (instrWp < this.icode.Length) {
            this.icode[instrWp] = icode;
            instrWp++;
        } else {
            Console.Error.WriteLine("RX-RSpi Icode to long");
        }
    }

    void ScriptClear(uint icode) {
        instrWp = 0;
        // Timer_Cancel
    }

    void spcmd_asm_master_script() {
        RSPI rspi = this;
        int spiCmdPtr = 0; /* TODO, use real command pointer. This is a dummy */
        ushort spiCmd = rspi.SPCMD[spiCmdPtr].Value;
        bool cpha = (spiCmd & SPCMD_CPHA) != 0;
        bool cpol = (spiCmd & SPCMD_CPOL) != 0;
        bool sslkp = (spiCmd & SPCMD_SSLKP) != 0;
        bool spms = (rspi.SPCR.Value & SPCR_SPMS) != 0;
        byte ssla = (byte)((spiCmd >> 4) & 7);
        uint brdiv = 1u << ((spiCmd & SPCMD_BRDV_MSK) >> SPCMD_BRDV_SHIFT);
        uint nrBits = gSpbToLength[(spiCmd & SPCMD_SPB_MSK) >> SPCMD_SPB_SHIFT];
        byte regSSLP = rspi.SSLP.Value;
        bool sslpol = (byte)((regSSLP >> ssla) & 1) != 0;
        bool txmd = (rspi.SPCR.Value & SPCR_TXMD) != 0;
        if (ssla > 3) {
            Console.Error.WriteLine("SSLA out of range: %u", ssla);
            ssla &= 0x3;
        }
        InstrAppend(INSTR_RELOAD_TXSHIFT);
        InstrAppend(INSTR_TRIGGER_SPTI); /* After loading the shift register trigger an TX empty interrupt */
        if (!spms) {
            if (!sslpol) {
                InstrAppend(INSTR_CS_LOW(ssla));
            } else {
                InstrAppend(INSTR_CS_HIGH(ssla));
            }
        }
        if (!cpol) {
            InstrAppend(INSTR_CLK_LOW);
        } else {
            InstrAppend(INSTR_CLK_HIGH);
        }
        for (int i = 0; i < nrBits; i++) {
            /* First comes the odd EDGE */
            if (!cpha) {
                InstrAppend(INSTR_SAMPLE_MISO);
            } else {
                InstrAppend(INSTR_SHIFTOUT_MOSI);
            }
            if (!cpol) {
                InstrAppend(INSTR_CLK_HIGH);
            } else {
                InstrAppend(INSTR_CLK_LOW);
            }
            //InstrAppend(rspi, INSTR_HALFCLOCKDELAY);
            /* Now the even edge */
            if (cpha) {
                InstrAppend(INSTR_SAMPLE_MISO);
            } else {
                InstrAppend(INSTR_SHIFTOUT_MOSI);
            }
            //InstrAppend(rspi, INSTR_HALFCLOCKDELAY);
            if (!cpol) {
                InstrAppend(INSTR_CLK_LOW);
            } else {
                InstrAppend(INSTR_CLK_HIGH);
            }
        }
        if (!cpol) {
            // InstrAppend(rspi, INSTR_DELAY(t2));
            // InstrAppend(rspi, INSTR_RELEASE_MOSI);
            if (sslkp) {
                if (!spms) {
                    if (!sslpol) {
                        InstrAppend(INSTR_CS_HIGH(ssla));
                    } else {
                        InstrAppend(INSTR_CS_LOW(ssla));
                    }
                }
            }
        } else {
            // InstrAppend(rspi, INSTR_DELAY(t2));
            // InstrAppend(rspi, INSTR_RELEASE_MOSI);
            if (sslkp) {
                if (!spms) {
                    if (!sslpol) {
                        InstrAppend(INSTR_CS_HIGH(ssla));
                    } else {
                        InstrAppend(INSTR_CS_LOW(ssla));
                    }
                }
            }
        }
        // InstrAppend(rspi, INSTR_DELAY(t3));
        if (!txmd) {
            InstrAppend(INSTR_TRIGGER_SPRI);
        }
        //InstrAppend(rspi, INSTR_RELOAD_TXSHIFT);
    }

    /// <summary>
    /// Suspend any serial transfer,
    /// Stop driving the output signals in slave mode.
    /// Initialize the internal state ????
    /// Initialize the transmit buffer
    /// </summary>
    void spe_clear() {
    }

    void spe_set() {
    }
    /// <summary>
    ///  RSPI Control Register
    ///  Bit 7 SPRIE RSPI Receive Interrupt Enable
    ///  Bit 6 SPE RSPI Function Enable
    ///  Bit 5 SPTIE RSPI Tranmit Interrupt Enable
    ///  Bit 4 SPEIE RSPI Error Interrupt Enable
    ///  Bit 3 MSTR RSPI Master Slave Mode select.
    ///  Bit 2 MODFEN Mode Fault Error Detection Enable
    ///  Bit 1 TXMD Communications Operating Mode Select. Full duplex/transmit only
    /// </summary>
    uint ReadSPCR(uint address, int rqlen) {
        return SPCR.Value;
    }

    /// <summary>
    /// Immediate triggering of TX interrupt when SPCR is changed:
    ///
    /// HAE-01 > rspi1 spti
    /// Transition SPE/SPTIE -> SPE/SPTIE
    ///   0 -> 0: 0
    ///   0 -> 1: 0
    ///   0 -> 2: 0
    ///   0 -> 3: 1
    ///   1 -> 0: 0
    ///   1 -> 1: 0
    ///   1 -> 2: 0
    ///   1 -> 3: 1
    ///   2 -> 0: 0
    ///   2 -> 1: 1
    ///   2 -> 2: 0
    ///   2 -> 3: 0
    ///   3 -> 0: 0
    ///   3 -> 1: 1
    ///   3 -> 2: 0
    ///   3 -> 3: 0
    /// </summary>
    void WriteSPCR(uint value, uint address, int rqlen) {
        RSPI rspi = this;
        byte diff = (byte)(rspi.SPCR.Value ^ value);
        // Enabling SPE together with SPTIE triggers an TX empty interrupt.
        if ((diff & SPCR_SPE) != 0 && (value & SPCR_SPTIE) != 0) {
            TriggerTxInterrupt();
        }
        rspi.SPCR.Value = (byte)value;
        if ((diff & SPCR_SPE) != 0) {
            if ((value & SPCR_SPE) != 0) {
                spe_set();
            } else {
                spe_clear();
            }
        }
        //    update_tx_interrupt(rspi);
        update_rx_interrupt();
    }

    // Slave select polarity register
    uint ReadSSLP(uint address, int rqlen) {
        return SSLP.Value;
    }

    /// <summary>
    /// Set polatity of chip select signals.
    /// Change of polarity when during active controller (SPE == 1) may cause problems.
    /// </summary>
    void WriteSSLP(uint value, uint address, int rqlen) {
        RSPI rspi = this;
        byte diff = (byte)(rspi.SSLP.Value ^ value);
        if ((rspi.SPCR.Value & SPCR_SPE) != 0) {
            Console.Error.WriteLine("Warning: Chip select polarity change while SPI is enabled");
        }
        rspi.SSLP.Value = (byte)(value & 0xf);
        for (int i = 0; i < 4; i++) {
            if ((diff &  (1 << i)) == 0) {
                continue;
            }
            if(((value >> i) & 1) != 0) {
                rspi.sigSSL[i].SetOutputSignal(false);
            } else {
                rspi.sigSSL[i].SetOutputSignal(true);
            }
        }
    }

    /// <summary>
    /// SPPCR
    /// Spi Pin Control register
    /// Bit 0 SPLP  RSPI Loopback mode
    /// Bit 1 SPLP2 Loopback mode 2 (transmit data = received data)
    /// Bit 3 MOIV  Mosi Idle Fixed Value
    /// Bit 4 MOIFE Mosi Idle Value Fixing Enable
    /// </summary>
    uint ReadSPPCR(uint address, int rqlen) {
        return SPPCR.Value;
    }

    void WriteSPPCR(uint value, uint address, int rqlen) {
        if ((value & SPPCR_SPLP) != 0) {
            // #if 0
            //             SigNode_RemoveLink(rspi.sigMosi, rspi.sigShiftOut);
            //             SigNode_RemoveLink(rspi.sigMiso, rspi.sigShiftOut);
            //             SigNode_RemoveLink(rspi.sigMosi, rspi.sigShiftIn);
            //             SigNode_RemoveLink(rspi.sigMiso, rspi.sigShiftIn);
            // #endif
        } else if ((value & SPPCR_SPLP2) != 0) {
            // #if 0
            //             SigNode_RemoveLink(rspi.sigMosi, rspi.sigShiftOut);
            //             SigNode_RemoveLink(rspi.sigMiso, rspi.sigShiftOut);
            //             SigNode_RemoveLink(rspi.sigMosi, rspi.sigShiftIn);
            //             SigNode_RemoveLink(rspi.sigMiso, rspi.sigShiftIn);
            // #endif
        }
        SPPCR.Value = (byte)(value & 0x37);
    }

    /// <summary>
    /// RSPI Status Register SPSR
    /// </summary>
    static uint ReadSPSR(RSPI clientData, uint address, int rqlen) {
        RSPI rspi = clientData;
        return rspi.SPSR.Value;
    }

    static void WriteSPSR(RSPI clientData, uint value, uint address, int rqlen) {
        RSPI rspi = clientData;
        int mask = (int)value | 0xf2;
        rspi.SPSR.Value = (byte)(rspi.SPSR.Value & mask);
    }

    /// <summary>
    /// Reading the receive buffer switches the receiver buffer read pointer
    /// to the next buffer automatically. The transmit buffer read pointer is not
    /// updated when reading from the transmit buffer.
    /// When reading from the transmit buffer the value most recently written
    /// is read. After generation of tbufEmtpy Interrupt 0 is read.
    /// </summary>
    uint ReadSPDR(uint address, int rqlen) {
        RSPI rspi = this;
        bool splw = (rspi.SPDCR.Value & SPDCR_SPLW) != 0; /* true means longword access */
        uint spfc = rspi.SPDCR.Value & 3u;  /* 0-3 is 1 to 4 frames */
        uint nrStages = spfc + 1;
        uint spdrValue;
        if ((rspi.SPDCR.Value & SPDCR_SPRDTD) != 0) {
            spdrValue = rspi.txBuf32[(rspi.txBufWp + nrStages - 1) % nrStages];
        } else {
            uint rxBufNr = RXBUF_RP();
            spdrValue = rspi.rxBuf32[rxBufNr];
            rspi.rxBufRp = (rspi.rxBufRp + 1) % nrStages;
        }
        return spdrValue;
    }

    void WriteSPDR(uint value, uint address, int rqlen) {
        RSPI rspi = this;
        bool splw = (rspi.SPDCR.Value & SPDCR_SPLW) != 0; /* true means longword access */
        uint nrStages = (rspi.SPDCR.Value & 3u) + 1;
        //nrBits = gSpbToLength[(spiCmd & SPCMD_SPB_MSK) >> SPCMD_SPB_SHIFT];
        rspi.txBuf32[TXBUF_WP()] = value;
        if (rspi.txBufWp < nrStages) {
            rspi.txBufWp = rspi.txBufWp + 1;
        } else {
            rspi.txBufWp = 0;
        }
    }

    /// <summary>
    /// SPSCR
    /// RSPI Sequence control register. Specifies the sequence length pf the SPCMD's
    /// 0-7 means usage of SPCMD0 only to usage of SPCMD0-7.
    /// </summary>
    uint ReadSPSCR(uint address, int rqlen) {
        return 0;
    }

    void WriteSPSCR(uint value, uint address, int rqlen) {
    }

    uint ReadSPSSR(uint address, int rqlen) {
        return 0;
    }

    /// <summary>
    /// RSPI Sequence Status Register
    /// Bit 0-2 SPCP Indicates the number of the current SPCMD (0-7).
    /// Bit 4-6 RSPI Error command indicates the SPCMD which caused an error
    /// </summary>
    void WriteSPSSR(uint value, uint address, int rqlen) {
    }

    uint ReadSPBR(uint address, int rqlen) {
        return SPBR.Value;
    }

    /// <summary>
    /// SPBR
    /// RSPI Bit Rate register
    /// </summary>
    void WriteSPBR(uint value, uint address, int rqlen) {
        SPBR.Value = (byte)value;
        clkBase.MulDiv = new Fraction(1, 2 * (value + 1));
    }

    /// <summary>
    /// RSPI Data control register
    /// </summary>
    uint ReadSPDCR(uint address, int rqlen) {
        return SPDCR.Value;
    }

    void WriteSPDCR(uint value, uint address, int rqlen) {
        SPDCR.Value = (byte)(value & 0x33);
    }

    uint ReadSPCKD(uint address, int rqlen) {
        return SPCKD.Value;
    }

    void WriteSPCKD(uint value, uint address, int rqlen) {
        SPCKD.Value = (byte)(value & 0x7);
    }

    uint ReadSSLND(uint address, int rqlen) {
        return SSLND.Value;
    }

    void WriteSSLND(uint value, uint address, int rqlen) {
        SSLND.Value = (byte)(value & 0x7);
    }

    uint ReadSPND(uint address, int rqlen) {
        return SPND.Value;
    }

    void WriteSPND(uint value, uint address, int rqlen) {
        SPND.Value = (byte)(value & 7);
    }

    uint ReadSPCR2(uint address, int rqlen) {
        return SPCR2.Value;
    }

    void WriteSPCR2(uint value, uint address, int rqlen) {
        SPCR2.Value = (byte)(value & 0xf);
    }

    uint ReadSPCMD0(uint address, int rqlen) {
        return SPCMD[0].Value;
    }

    void WriteSPCMD0(uint value, uint address, int rqlen) {
        SPCMD[0].Value = (ushort)value;
    }

    uint spcmd1_read(uint address, int rqlen) {
        return SPCMD[1].Value;
    }

    void spcmd1_write(uint value, uint address, int rqlen) {
        SPCMD[1].Value = (ushort)value;
    }

    uint spcmd2_read(uint address, int rqlen) {
        return SPCMD[2].Value;
    }

    void spcmd2_write(uint value, uint address, int rqlen) {
        SPCMD[2].Value = (ushort)value;
    }

    uint spcmd3_read(uint address, int rqlen) {
        return SPCMD[3].Value;
    }

    void spcmd3_write(uint value, uint address, int rqlen) {
        SPCMD[3].Value = (ushort)value;
    }

    uint spcmd4_read(uint address, int rqlen) {
        return SPCMD[4].Value;
    }

    void spcmd4_write(uint value, uint address, int rqlen) {
        SPCMD[4].Value = (ushort)value;
    }

    uint spcmd5_read(uint address, int rqlen) {
        return SPCMD[5].Value;
    }

    void spcmd5_write(uint value, uint address, int rqlen) {
        SPCMD[5].Value = (ushort)value;
    }

    uint spcmd6_read(uint address, int rqlen) {
        return SPCMD[6].Value;
    }

    void spcmd6_write(uint value, uint address, int rqlen) {
        SPCMD[6].Value = (ushort)value;
    }

    uint spcmd7_read(uint address, int rqlen) {
        return SPCMD[7].Value;
    }

    void spcmd7_write(uint value, uint address, int rqlen) {
        SPCMD[7].Value = (ushort)value;
    }

    /*
      static void Rspi_Map(void *owner, uint base, uint mask, uint mapflags) {
      RxRSpi rspi = owner;
      IOH_New8(REG_SPCR(base), spcr_read, spcr_write, rspi);
      IOH_New8(REG_SSLP(base),sslp_read,sslp_write,rspi);
      IOH_New8(REG_SPPCR(base),sppcr_read,sppcr_write,rspi);
      IOH_New8(REG_SPSR(base),spsr_read,spsr_write,rspi);
      IOH_New32(REG_SPDR(base),spdr_read,spdr_write,rspi);
      IOH_New8(REG_SPSCR(base),spscr_read,spscr_write,rspi);
      IOH_New8(REG_SPSSR(base),spssr_read,spssr_write,rspi);
      IOH_New8(REG_SPBR(base),spbr_read,spbr_write,rspi);
      IOH_New8(REG_SPDCR(base),spdcr_read,spdcr_write,rspi);
      IOH_New8(REG_SPCKD(base),spckd_read,spckd_write,rspi);
      IOH_New8(REG_SSLND(base),sslnd_read,sslnd_write,rspi);
      IOH_New8(REG_SPND(base),spnd_read,spnd_write,rspi);
      IOH_New8(REG_SPCR2(base),spcr2_read,spcr2_write,rspi);
      IOH_New16(REG_SPCMD0(base),spcmd0_read,spcmd0_write,rspi);
      IOH_New16(REG_SPCMD1(base),spcmd1_read,spcmd1_write,rspi);
      IOH_New16(REG_SPCMD2(base),spcmd2_read,spcmd2_write,rspi);
      IOH_New16(REG_SPCMD3(base),spcmd3_read,spcmd3_write,rspi);
      IOH_New16(REG_SPCMD4(base),spcmd4_read,spcmd4_write,rspi);
      IOH_New16(REG_SPCMD5(base),spcmd5_read,spcmd5_write,rspi);
      IOH_New16(REG_SPCMD6(base),spcmd6_read,spcmd6_write,rspi);
      IOH_New16(REG_SPCMD7(base),spcmd7_read,spcmd7_write,rspi);
      }
    */

    public RSPI(string name, SubClock clkIn, IClock clock) {
        RSPI rspi = this;
        // rspi.bdev.first_mapping = NULL;
        // rspi.bdev.Map = Rspi_Map;
        // rspi.bdev.UnMap = Rspi_Unmap;
        // rspi.bdev.owner = rspi;
        // rspi.bdev.hw_flags = MEM_FLAG_WRITABLE | MEM_FLAG_READABLE;
        sigMosi = new OutputSignalBit($"{name}.mosi");
        sigMiso = new SignalBit($"{name}.miso", null);
        sigSclk = new OutputSignalBit($"{name}.sclk");
        //rspi.sigShiftOut = SigNode_New("%s.shiftOut", name);
        //rspi.sigShiftIn = SigNode_New("%s.shiftIn", name);
        clkBase = new SubClock($"{name}.clk_base", clkIn, new Fraction(1, 2 * 256));
        SPBR = new RegisterValue32<byte>(255);
        // Clock_MakeDerived(rspi.clkBase, rspi.clkIn, 1, 2 * 256);
        // if (!rspi.sigMosi || !rspi.sigMiso || !rspi.sigSclk) {
        //     throw new Exception("Can not create signals for SPI controller");
        // }
        for (int i = 0; i < rspi.sigSSL.Length; i++) {
            rspi.sigSSL[i] = new OutputSignalBit($"{name}.ssl{i}");
            // if (!rspi.sigSSL[i]) {
            //     throw new Exception("Can not create signals for SPI controller");
            // }
        }
        // rspi.sigIrqSpti = SigNode_New($"{name}.irqSpti", name);
        // rspi.sigIrqSpri = SigNode_New($"{name}.irqSpri", name);
        // rspi.sigIrqSpii = SigNode_New($"{name}.irqSpii", name);
        // rspi.sigIrqSpei = SigNode_New($"{name}.irqSpei", name);
        // if (!rspi.sigIrqSpti || !rspi.sigIrqSpri || !rspi.sigIrqSpii || !rspi.sigIrqSpei) {
        //     throw new Exception("Can not create Interrupts for SPI controller");
        // }

        // Edge interrupt (neg)
        sigIrqSpti = new OutputSignalBit($"{name}.irqSpti", true);
        // Edge interrupt (neg)
        sigIrqSpri = new OutputSignalBit($"{name}.irqSpri", true);
        // Level interrupt (low)
        sigIrqSpii = new OutputSignalBit($"{name}.irqSpii", true);
        // Group interrupt
        sigIrqSpei = new OutputSignalBit($"{name}.irqSpei", true);


        // SigNode_Set(rspi.sigIrqSpti, SIG_HIGH);
        // SigNode_Set(rspi.sigIrqSpri, SIG_HIGH);
        // SigNode_Set(rspi.sigIrqSpii, SIG_HIGH);
        // SigNode_Set(rspi.sigIrqSpei, SIG_HIGH);
        rspi.instrDelayTimer = new CountTimer(
            rspi.clkBase, clock,
            new TickTimerTriggerAction[] {
                new(new(TimerTriggerKind.ResetZero, uint.MaxValue),
                    () => InterpLoop())
            }, TimerUpperLimitTriggerKind.ResetZero, uint.MaxValue, null);
    }
}

public static class RSPIOffset {
    public static readonly uint SPCR    = 0x00;
    public static readonly uint SSLP    = 0x01;
    public static readonly uint SPPCR   = 0x02;
    public static readonly uint SPSR    = 0x03;
    public static readonly uint SPDR    = 0x04;
    public static readonly uint SPSCR   = 0x08;
    public static readonly uint SPSSR   = 0x09;
    public static readonly uint SPBR    = 0x0a;
    public static readonly uint SPDCR   = 0x0b;
    public static readonly uint SPCKD   = 0x0c;
    public static readonly uint SSLND   = 0x0d;
    public static readonly uint SPND    = 0x0e;
    public static readonly uint SPCR2   = 0x0f;
    public static readonly uint SPCMD0  = 0x10;
    public static readonly uint SPCMD1  = 0x12;
    public static readonly uint SPCMD2  = 0x14;
    public static readonly uint SPCMD3  = 0x16;
    public static readonly uint SPCMD4  = 0x18;
    public static readonly uint SPCMD5  = 0x1A;
    public static readonly uint SPCMD6  = 0x1C;
    public static readonly uint SPCMD7  = 0x1E;

}
