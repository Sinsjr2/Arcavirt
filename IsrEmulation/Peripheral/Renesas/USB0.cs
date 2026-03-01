using Clock;
using GPIO;
using USB;

namespace Peripheral.Renesas;

public class USB0 {
    static readonly ushort SYSCFG_SCKE     = (1 << 10);
    static readonly ushort SYSCFG_DCFM     = (1 << 6);
    static readonly ushort SYSCFG_DRPD     = (1 << 5);
    static readonly ushort SYSCFG_DPRPU    = (1 << 4);
    static readonly ushort SYSCFG_USBE     = (1 << 0);

    static readonly ushort SYSSTS0_OVCMON_MSK   = (3 << 14);
    static readonly ushort SYSSTS0_HTACT        = (1 << 6);
    static readonly ushort SYSSTS0_IDMON        = (1 << 2);
    static readonly ushort SYSSTS0_LNST         = (3 << 0);

    static readonly ushort DVSTCTR0_NNPBTOA      = (1 << 11);
    static readonly ushort DVSTCTR0_EXICEN       = (1 << 10);
    static readonly ushort DVSTCTR0_VBUSEN       = (1 << 9);
    static readonly ushort DVSTCTR0_WKUP         = (1 << 8);
    static readonly ushort DVSTCTR0_RWUPE        = (1 << 7);
    static readonly ushort DVSTCTR0_USBRST       = (1 << 6);
    static readonly ushort DVSTCTR0_RESUME       = (1 << 5);
    static readonly ushort DVSTCTR0_UACT         = (1 << 4);
    static readonly ushort DVSTCTR0_RHST_MSK = (7);

    static readonly ushort FIFOSEL_RCNT    = (1 << 15);
    static readonly ushort FIFOSEL_REW     = (1 << 14);
    static readonly ushort FIFOSEL_MBW     = (1 << 10);
    static readonly ushort FIFOSEL_BIGEND  = (1 << 8);
    static readonly ushort FIFOSEL_ISEL    = (1 << 5);
    static readonly ushort FIFOSEL_CURPIPE_MSK = (0xf);

    static readonly ushort FIFOCTR_BVAL = (1 << 15);
    static readonly ushort FIFOCTR_BCLR = (1 << 14);
    static readonly ushort FIFOCTR_FRDY = (1 << 13);
    static readonly ushort FIFOCTR_DTLN_MSK     = (0x1ff);

    static readonly ushort INTENB0_VBSE = (1 << 15);
    static readonly ushort INTENB0_RSME = (1 << 14);
    static readonly ushort INTENB0_SOFE = (1 << 13);
    static readonly ushort INTENB0_DVSE = (1 << 12);
    static readonly ushort INTENB0_CTRE = (1 << 11);
    static readonly ushort INTENB0_BEMPE        = (1 << 10);
    static readonly ushort INTENB0_NRDYE        = (1 << 9);
    static readonly ushort INTENB0_BRDYE        = (1 << 8);
    static readonly ushort INTENB1_OVRCHRE      = (1 << 15);
    static readonly ushort INTENB1_BCHGE        = (1 << 14);
    static readonly ushort INTENB1_DTCHE        = (1 << 12);
    static readonly ushort INTENB1_ATTCHE       = (1 << 11);
    static readonly ushort INTENB1_EOFERRE      = (1 << 6);
    static readonly ushort INTENB1_SIGNE        = (1 << 5);
    static readonly ushort INTENB1_SACKE        = (1 << 4);
    static readonly ushort BEMPE_PIPO0  = (1 << 0);
    static readonly ushort BEMPE_PIPO1  = (1 << 1);
    static readonly ushort BEMPE_PIPO2  = (1 << 2);
    static readonly ushort BEMPE_PIPO3  = (1 << 3);
    static readonly ushort BEMPE_PIPO4  = (1 << 4);
    static readonly ushort BEMPE_PIPO5  = (1 << 5);
    static readonly ushort BEMPE_PIPO6  = (1 << 6);
    static readonly ushort BEMPE_PIPO7  = (1 << 7);
    static readonly ushort BEMPE_PIPO8  = (1 << 8);
    static readonly ushort BEMPE_PIPO9  = (1 << 9);
    static readonly ushort SOCFG_TRNENSEL       = (1 << 8);
    static readonly ushort SOCFG_BRDYM  = (1 << 6);
    static readonly ushort SOCFG_EDGESTS        = (1 << 4);
    static readonly ushort INTSTS0_VBINT        =       (1 << 15);
    static readonly ushort INTSTS0_RESM =       (1 << 14);
    static readonly ushort INTSTS0_SOFR =       (1 << 13);
    static readonly ushort INTSTS0_DVST =       (1 << 12);
    static readonly ushort INTSTS0_CTRT =       (1 << 11);
    static readonly ushort INTSTS0_BEMP =       (1 << 10);
    static readonly ushort INTSTS0_NRDY =       (1 << 9);
    static readonly ushort INTSTS0_BRDY =       (1 << 8);
    static readonly ushort INTSTS0_VBSTS        =       (1 << 7);
    static readonly ushort INTSTS0_DVSQ_OFS     = (4);
    static readonly ushort INTSTS0_DVSQ_MSK     = (ushort)(7 << INTSTS0_DVSQ_OFS);
    static readonly ushort INTSTS0_VALID        =       (1 << 3);
    static readonly ushort INTSTS0_CTSQ_OFS     = (0);
    static readonly ushort INTSTS0_CTSQ_MSK     = (ushort)(7 << INTSTS0_CTSQ_OFS);
    public enum USBDeviceState {
        Powered = 0b000,
        Default = 0b001,
        Address = 0b010,
        Configured = 0b011,
        Suspended = 0b100
    }
    // const ushort DVSQ_POWS    = (0);  /* Powered state */
    // const ushort DVSQ_DEFS    = (1);  /* Default state */
    // const ushort DVSQ_ADRS    = (2);  /* Address state */
    // const ushort DVSQ_CFGS    = (3);  /* Configured state */
    // const ushort DVSQ_SUSF    = (4);  /* Suspended state */
    const ushort CTSQ_IDST    = (0);  /* Idle or setup stage */
    const ushort CTSQ_RDDS    = (1);  /* Ctrl read data stage */
    const ushort CTSQ_RDSS    = (2);  /* Ctrl read status stage */
    const ushort CTSQ_WRDS    = (3);  /* Ctrl write data stage */
    const ushort CTSQ_WRSS    = (4);  /* Ctrl write status stage */
    const ushort CTSQ_WRNDSS  = (5);  /* Ctrl write (no data); status stage */
    const ushort CTSQ_TSQERR  = (6);  /* Ctrl transfer sequence error */
    static readonly ushort INTSTS1_OVRCR        =       (1 << 15);
    static readonly ushort INTSTS1_BCHG =       (1 << 14);
    static readonly ushort INTSTS1_DTCH =       (1 << 12);
    static readonly ushort INTSTS1_ATTCH        =       (1 << 11);
    static readonly ushort INTSTS1_EOFERR       =       (1 << 6);
    static readonly ushort INTSTS1_SIGN =       (1 << 5);
    static readonly ushort INTSTS1_SACK =       (1 << 4);
    static readonly ushort FRMNUM_OVRN  = (1 << 15);
    static readonly ushort FRMNUM_CRCE  = (1 << 14);
    static readonly ushort DVCHGR_DVCHG = (1 << 15);
    static readonly ushort USBADDR_STSRECONV_MSK =      (0xf << 8);
    static readonly ushort USBADDR_ADDR_MSK     = (0x7f);
    static readonly ushort DCPCFG_SHTNAK        = (1 << 7);
    static readonly ushort DCPCFG_DIR   = (1 << 4);
    static readonly ushort DCPMAXP_DEVSEL_MSK = (0xf << 12);
    static readonly ushort DCPMAXP_MXPS_MSK     =(0x7f);
    static readonly ushort DCPCTR_BSTS          = (1 << 15);
    static readonly ushort DCPCTR_SUREQ = (1 << 14);
    static readonly ushort DCPCTR_SUREQCLR      = (1 << 11);
    static readonly ushort DCPCTR_SQCLR = (1 << 8);
    static readonly ushort DCPCTR_SQSET = (1 << 7);
    static readonly ushort DCPCTR_SQMON = (1 << 6);
    static readonly ushort DCPCTR_PBUSY = (1 << 5);
    static readonly ushort DCPCTR_CCPL          = (1 << 2);
    static readonly ushort DCPCTR_PID_MSK       = (3);

    static readonly byte PID_NAK        = (0);
    static readonly byte PID_BUF        = (1);
    static readonly byte PID_STALL      = (2);

    static readonly ushort PIPESEL_PIPESEL_MSK  = (0xf);
    static readonly ushort PIPECFG_TYPE_OFS     = (14);
    static readonly ushort PIPECFG_TYPE_MSK     = (ushort)(3 << PIPECFG_TYPE_OFS);
    static readonly ushort PIPECFG_BFRE         = (1 << 10);
    static readonly ushort PIPECFG_DBLB         = (1 << 9);
    static readonly ushort PIPECFG_SHTNAK       = (1 << 7);
    static readonly ushort PIPECFG_DIR          = (1 << 4);
    static readonly ushort PIPECFG_EPNUM_OFS    = (0);
    static readonly ushort PIPECFG_EPNUM_MSK    = (ushort)(0xf << PIPECFG_EPNUM_OFS);
    static readonly ushort PIPEMAXP_MXPS_MSK    = (0x1ff);
    static readonly ushort PIPEMAXP_DEVSEL_MSK  = (0xf << 12);

    static readonly ushort TYPE_BULK    = (1);
    static readonly ushort TYPE_INTERRUPT       = (2);
    static readonly ushort TYPE_ISOCHRONOUS     = (3);

    static readonly ushort PIPEPERI_IFIS                = (1 << 12);
    static readonly ushort PIPEPERI_IITV_MSK    = (0x7);
    static readonly ushort PIPE15CTR_BSTS               = (1 << 15);
    static readonly ushort PIPE15CTR_INBUFM      = (1 << 14);
    static readonly ushort PIPE15CTR_ATREPM      = (1 << 10);
    static readonly ushort PIPE15CTR_ACLRM              = (1 << 9);
    static readonly ushort PIPE15CTR_SQCLR              = (1 << 8);
    static readonly ushort PIPE15CTR_SQSET              = (1 << 7);
    static readonly ushort PIPE15CTR_SQMON              = (1 << 6);
    static readonly ushort PIPE15CTR_PBUSY              = (1 << 5);
    static readonly ushort PIPE15CTR_PID_MSK    = (3);
    static readonly ushort PIPE69CTR_BSTS               = (1 << 15);
    static readonly ushort PIPE69CTR_ACLRM              = (1 << 9);
    static readonly ushort PIPE69CTR_SQCLR              = (1 << 8);
    static readonly ushort PIPE69CTR_SQSET              = (1 << 7);
    static readonly ushort PIPE69CTR_SQMON              = (1 << 6);
    static readonly ushort PIPE69CTR_PBUSY              = (1 << 5);
    static readonly ushort PIPE69CTR_PID_MSK    = (3);

    static readonly ushort PIPETRE_TRENB        = (1 << 9);
    static readonly ushort PIPETRE_TRCLR        = (1 << 8);

    static readonly ushort DEVADDR_USBSPD_MSK = (3 << 6);



    static readonly uint DPUSR0R_DVBSTS1= (1u << 31);
    static readonly uint DPUSR0R_DOVCB1 = (1 << 29);
    static readonly uint DPUSR0R_DOVCA1 = (1 << 28);
    static readonly uint DPUSR0R_DM1    = (1 << 25);
    static readonly uint DPUSR0R_DP1    = (1 << 24);
    static readonly uint DPUSR0R_DVBSTS0        = (1 << 23);
    static readonly uint DPUSR0R_DOVCB0 = (1 << 21);
    static readonly uint DPUSR0R_DOVCA0 = (1 << 20);
    static readonly uint DPUSR0R_DM0    = (1 << 17);
    static readonly uint DPUSR0R_DP0    = (1 << 16);
    static readonly uint DPUSR0R_FIXPHY1        = (1 << 12);
    static readonly uint DPUSR0R_SRPC1  = (1 << 8);
    static readonly uint DPUSR0R_FIXPHY0        = (1 << 4);
    static readonly uint DPUSR0R_SRPC0  = (1 << 0);
    static readonly uint DPUSR1R_DVBINT1        = (1u << 31);
    static readonly uint DPUSR1R_DOVRCRB1       = (1 << 29);
    static readonly uint DPUSR1R_DOVRCRA1       = (1 << 28);
    static readonly uint DPUSR1R_DMINT1         = (1 << 25);
    static readonly uint DPUSR1R_DPINT1 = (1 << 24);
    static readonly uint DPUSR1R_DVBINT0        = (1 << 23);
    static readonly uint DPUSR1R_DOVRCRB0       = (1 << 21);
    static readonly uint DPUSR1R_DOVRCRA0       = (1 << 20);
    static readonly uint DPUSR1R_DMINT0 = (1 << 25);
    static readonly uint DPUSR1R_DPINT0 = (1 << 16);
    static readonly uint DPUSR1R_DVBSE1 = (1 << 15);
    static readonly uint DPUSR1R_DOVRCRBE1      = (1 << 13);
    static readonly uint DPUSR1R_DOVRCRAE1      = (1 << 12);
    static readonly uint DPUSR1R_DMINTE1        = (1 << 9);
    static readonly uint DPUSR1R_DPINTE1        = (1 << 8);
    static readonly uint DPUSR1R_DVBSE0 = (1 << 7);
    static readonly uint DPUSR1R_DOVRCRBE0      = (1 << 5);
    static readonly uint DPUSR1R_DOVRCRAE0      = (1 << 4);
    static readonly uint DPUSR1R_DMINTE0        = (1 << 1);
    static readonly uint DPUSR1R_DPINTE0        = (1 << 0);

    // static USB_FIFO_BUF_SZ 256

    public class usb_fifo_t {
        public byte[] buf = new byte[256];
        public int pos_w;
        public int pos_r;
    }

    public class RXUsbPipe {
        public RegisterValue32<ushort> regPIPECFG;
        public RegisterValue32<ushort> regPIPEMAXP;
        public RegisterValue32<ushort> regPIPEPERI;
        public RegisterValue32<ushort> regPIPECTR;
        public RegisterValue32<ushort> regPIPETRE;
        public RegisterValue32<ushort> regPIPETRN;
        public bool isel;
        public usb_fifo_t fifo = new();
        public bool frdy;
        public bool brdy;
        public bool nrdy;
        public bool bemp;
    }

    public RegisterValue32<ushort> regSYSCFG;
    public RegisterValue32<ushort> regSYSSTS0;
    public RegisterValue32<ushort> regDVSTCTR0;

    public RegisterValue32<ushort> regCFIFOSEL;
    public RegisterValue32<ushort> regCFIFOCTR;
    public RegisterValue32<ushort> regD0FIFOSEL;
    public RegisterValue32<ushort> regD0FIFOCTR;
    public RegisterValue32<ushort> regD1FIFOSEL;
    public RegisterValue32<ushort> regD1FIFOCTR;
    public RegisterValue32<ushort> regINTENB0;
    public RegisterValue32<ushort> regINTENB1;
    public RegisterValue32<ushort> regBRDYENB;
    public RegisterValue32<ushort> regNRDYENB;
    public RegisterValue32<ushort> regBEMPENB;
    public RegisterValue32<ushort> regSOFCFG;
    public RegisterValue32<ushort> regINTSTS0;
    public RegisterValue32<ushort> regINTSTS1;
    public RegisterValue32<ushort> regFRMNUM;
    public RegisterValue32<ushort> regDVCHGR;
    public RegisterValue32<ushort> regUSBADDR;
    public RegisterValue32<ushort> regUSBREQ;
    public RegisterValue32<ushort> regUSBVAL;
    public RegisterValue32<ushort> regUSBINDX;
    public RegisterValue32<ushort> regUSBLENG;

    public RegisterValue32<ushort> regPIPESEL;
    public readonly RXUsbPipe[] Pipe = Enumerable.Range(0, 10).Select(_ => new RXUsbPipe()).ToArray();

    public RegisterValue32<ushort> regDEVADD0;
    public RegisterValue32<ushort> regDEVADD1;
    public RegisterValue32<ushort> regDEVADD2;
    public RegisterValue32<ushort> regDEVADD3;
    public RegisterValue32<ushort> regDEVADD4;
    public RegisterValue32<ushort> regDEVADD5;

    public SignalBit sigIrq;
    public IAlarm sofTimer;
    public bool int_req;

    int Counter = 0;
    public USBDeviceState DevSeq;
    public ushort CtrSeq;
    public ushort HostFrmSeq;
    public ushort HostPipe;
    public ushort HostStupSeq;
    public bool HostInRecv;

    readonly IUSBBus usbBus;

    void usb_fifo_clear(usb_fifo_t fifo) {
        fifo.pos_r = fifo.pos_w = 0;
    }

    int usb_fifo_length(usb_fifo_t fifo) {
        int pos_w = fifo.pos_w;
        int pos_r = fifo.pos_r;
        if (pos_r > pos_w)
            pos_w += fifo.buf.Length;
        return pos_w - pos_r;
    }

    bool usb_fifo_is_writeble(usb_fifo_t fifo) {
        int pos_w = fifo.pos_w + 1;
        if (pos_w >= fifo.buf.Length)
            pos_w = 0;
        return fifo.pos_r != pos_w;
    }

    void usb_fifo_write(usb_fifo_t fifo, byte c) {
        fifo.buf[fifo.pos_w++] = c;
        if (fifo.pos_w >= fifo.buf.Length)
            fifo.pos_w = 0;
    }

    bool usb_fifo_is_readable(usb_fifo_t fifo) {
        return fifo.pos_r != fifo.pos_w;
    }

    byte usb_fifo_read(usb_fifo_t fifo) {
        byte c = fifo.buf[fifo.pos_r++];
        if (fifo.pos_r >= fifo.buf.Length)
            fifo.pos_r = 0;
        return c;
    }

    /*
*******************************************************************************
* Registers Initialized when USBE is written 0 in function mode
* (docu says on write, not on edge but i should check the real hardware)
* Table 28.4
*******************************************************************************
*/
// #if 0
//     static void usbe0_function(RXUsb *ru) {
//         //ru.regSYSSTS0 LNST
//         // RHST
//         //DVSQ
//         // USBADDR
//         //USBREQ BREQUEST,BMREQUESTTYPE
//         //WVALUE
//         //WINDEX
//         //WLENGHT
//     }
// #endif
    /**
***********************************************************************
* Registers initialized when USBE is written 0 in host mode
***********************************************************************
*/
// #if 0
//     static void usbe0_host(RXUsb *ru) {
//         //RHST
//         //FRNM
//     }
// #endif

    /**
* Bit 0: USBE USB Module enable
* Bit 4: DPRPU D+ Line Pullup Control
* Bit 5: DRPD D+/D- Pulldown control
* Bit 6: DCFM Controller function select 0 = function 1 = host
* Bit 10: SCKE  USB Module Clock Enable
*/
    // static uint syscfg_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regSYSCFG;
    // }

    void syscfg_write(ushort value, uint address, int rqlen) {
        regSYSCFG.Value = value;

        if ((regSYSCFG.Value & SYSCFG_USBE) != 0) {
            sofTimer.Schedule(TimeSpan.FromMilliseconds(1).TotalNanoseconds);
        }
        else {
            sofTimer.Cancel();
        }
    }

    /*
* Register SYSSTS0
* Bits 14+15: OVCMON Overcurrent monitor
* Bit 6: HTACT Host sequencer active 0 = stopped 1 = not stopped
* Bit 2: IDMON Status of the ID pin
* Bits 0 + 1: Linestatus 0 = SE0, 1 = J, 2 = K, 3 = SE1
*/
    // static uint syssts0_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regSYSSTS0;
    // }

    // static void syssts0_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regSYSSTS0 = value;
    // }

    /**
* DVSTCTR0 Device status control register
* RHST 0-2: 0 Speed not determined, 1 = LS, 2 = FS. FUN: 2 = FS 4 = Reset in progress
* Bit 4: UACT 0 = Disa downstream, 1 = enable downstream (SOF)
* Bit 5: RESUME 1 = output a resume signal
* Bit 6: USBRST 1 = output a ru reset signal
* Bit 7: RWUPE 1 = Downstream wakeup enabled
* Bit 8: WKUP 1 = output a remote wake up signal.
* Bit 9: VBUSEN The VBUSEN
* Bit 10: EXICEN The Status of the external exicen pin.
* Bit 11: Host negotiation protocol
*/
    // static uint dvstctr0_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDVSTCTR0;
    // }

    // static void dvstctr0_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDVSTCTR0 = value;
    // }

    /**
* Register CFIFO
* 8/16 Bit control fifo register. The assignment to a pipe is done with the cfifosel register.
*/
    ushort cfifo_read(uint address, int rqlen) {
        ushort result = 0;
        int curpipe = regCFIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (usb_fifo_is_readable(pipe.fifo))
            result |= usb_fifo_read(pipe.fifo);

        // 16bit
        if ((rqlen > 1) && (regCFIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regCFIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                result <<= 8;
                if (usb_fifo_is_readable(pipe.fifo))
                    result |= usb_fifo_read(pipe.fifo);
            }
            else {
                if (usb_fifo_is_readable(pipe.fifo))
                    result |= (ushort)(usb_fifo_read(pipe.fifo) << 8);
            }
        }

        return result;
    }

    void cfifo_write(uint value, uint address, int rqlen) {
        int curpipe = regCFIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        // 16bit
        if ((rqlen > 1) && (regCFIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regCFIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                if (usb_fifo_is_writeble(pipe.fifo))
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
                if (usb_fifo_is_writeble(pipe.fifo))
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
            }
            else {
                if (usb_fifo_is_writeble(pipe.fifo))
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
                if (usb_fifo_is_writeble(pipe.fifo))
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
            }
        }
        else {
            if (usb_fifo_is_writeble(pipe.fifo))
                usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
        }
    }

    /**
* Register D0FIFO
* One of two data fifos. The assignement to a Pipe is done
* with the D0FIFOSEL register.
*/
    ushort d0fifo_read(uint address, int rqlen) {
        ushort result = 0;
        int curpipe = regD0FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (usb_fifo_is_readable(pipe.fifo)) {
            result |= usb_fifo_read(pipe.fifo);
        }

        // 16bit
        if ((rqlen > 1) && (regD0FIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regD0FIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                result <<= 8;
                if (usb_fifo_is_readable(pipe.fifo)) {
                    result |= usb_fifo_read(pipe.fifo);
                }
            }
            else {
                if (usb_fifo_is_readable(pipe.fifo)) {
                    result |= (ushort)(usb_fifo_read(pipe.fifo) << 8);
                }
            }
        }

        return result;
    }

    void d0fifo_write(uint value, uint address, int rqlen) {
        int curpipe = regD0FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        // 16bit
        if ((rqlen > 1) && (regD0FIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regD0FIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
                }
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
                }
            }
            else {
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
                }
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
                }
            }
        }
        else {
            if (usb_fifo_is_writeble(pipe.fifo)) {
                usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
            }
        }
    }

    /**
* Register D1FIFO
* One of two data fifos. The assignment to a pipe is done
* with the D1FIFOSEL register.
*/
    ushort d1fifo_read(uint address, int rqlen) {
        ushort result = 0;
        int curpipe = regD1FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (usb_fifo_is_readable(pipe.fifo)) {
            result |= usb_fifo_read(pipe.fifo);
        }

        // 16bit
        if ((rqlen > 1) && (regD1FIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regD1FIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                result <<= 8;
                if (usb_fifo_is_readable(pipe.fifo)) {
                    result |= usb_fifo_read(pipe.fifo);
                }
            }
            else {
                if (usb_fifo_is_readable(pipe.fifo)) {
                    result |= (ushort)(usb_fifo_read(pipe.fifo) << 8);
                }
            }
        }

        return result;
    }

    void d1fifo_write(uint value, uint address, int rqlen) {
        int curpipe = regD1FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        // 16bit
        if ((rqlen > 1) && (regD1FIFOSEL.Value & FIFOSEL_MBW) != 0) {
            if ((regD1FIFOSEL.Value & FIFOSEL_BIGEND) != 0) {
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
                }
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
                }
            }
            else {
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
                }
                if (usb_fifo_is_writeble(pipe.fifo)) {
                    usb_fifo_write(pipe.fifo, (byte)((value >> 8) & 0xFF));
                }
            }
        }
        else {
            if (usb_fifo_is_writeble(pipe.fifo)) {
                usb_fifo_write(pipe.fifo, (byte)(value & 0xFF));
            }
        }
    }

    /**
* Register xFIFOSEL registers
* Do the assignent between a pipe and the control fifo access port CFIFO
* Bits 0 - 3: Access pipe number (0 == DCP ... 9 == PIPE9)
* Bit  5: ISEL 0 = Read from fifo 1 = write to fifo
* Bit  8: BIGEND Fifo Port endianess:  0 = little endian, 1 = bigendian
* Bit 10: MBW access width: 0 = 8Bit 1 = 16Bit
* Bit 14: Buffer pointer rewind.
* Bit 15: RCNT Read count mode: 0 Clear DTLN when read is complete. 1 = Decrement DTLN
*/
    uint cfifosel_read(uint address, int rqlen) {
        ushort result = regCFIFOSEL.Value;
        int curpipe = result & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (pipe.isel) {
            result |= FIFOSEL_ISEL;
        }
        else {
            result &= (ushort)(~FIFOSEL_ISEL);
        }

        return result;
    }

    void cfifosel_write(ushort value, uint address, int rqlen) {
        int curpipe = value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if ((value & FIFOSEL_ISEL) != 0) {
            pipe.isel = true;
        }
        else {
            pipe.isel = false;
        }

        regCFIFOSEL.Value = value;
    }

    /**
* Register xFIFOCR
* Bit 0 - 8 DTLN Indicates the length of recieve data. Depends on RCNT
* Bit 13 Fifo Port Ready 0 = port access disabled, 1 = enabled
* Bit 14: 1 = Clear the buffer memory
* Bit 15: 1 = Buffer Memory valid flag 1 = Writing ended
*/
    uint cfifoctr_read(uint address, int rqlen) {
        return regCFIFOCTR.Value;
    }

    void cfifoctr_write(ushort value, uint address, int rqlen) {
        int curpipe = regCFIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if ((value & FIFOCTR_BCLR) != 0) {
            usb_fifo_clear(pipe.fifo);
        }
        value &= (ushort)(~(FIFOCTR_DTLN_MSK | FIFOCTR_FRDY));
        value |= (ushort)(usb_fifo_length(pipe.fifo) & FIFOCTR_DTLN_MSK);
        if (pipe.frdy) {
            value |= FIFOCTR_FRDY;
        }
        regCFIFOCTR.Value = value;
    }

    uint d0fifosel_read(uint address, int rqlen) {
        ushort result = regD0FIFOSEL.Value;
        int curpipe = result & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (pipe.isel) {
            result |= FIFOSEL_ISEL;
        }
        else {
            result &= (ushort)(~FIFOSEL_ISEL);
        }

        return result;
    }

    void d0fifosel_write(ushort value, uint address, int rqlen) {
        int curpipe = value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if ((value & FIFOSEL_ISEL) != 0) {
            pipe.isel = true;
        }
        else {
            pipe.isel = false;
        }

        regD0FIFOSEL.Value = value;
    }

    // static uint d0fifoctr_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regD0FIFOCTR;
    // }

    void d0fifoctr_write(ushort value, uint address, int rqlen) {
        int curpipe = regD0FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        value &= (ushort)(~(FIFOCTR_DTLN_MSK | FIFOCTR_FRDY));
        value |= (ushort)usb_fifo_length(pipe.fifo);
        if (pipe.frdy) {
            value |= FIFOCTR_FRDY;
        }
        regD0FIFOCTR.Value = value;
    }

    uint d1fifosel_read(uint address, int rqlen) {
        ushort result = regD1FIFOSEL.Value;
        int curpipe = result & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if (pipe.isel) {
            result |= FIFOSEL_ISEL;
        }
        else {
            result &= (ushort)~FIFOSEL_ISEL;
        }

        return result;
    }

    void d1fifosel_write(ushort value, uint address, int rqlen) {
        int curpipe = value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        if ((value & FIFOSEL_ISEL) != 0) {
            pipe.isel = true;
        }
        else {
            pipe.isel = false;
        }

        regD1FIFOSEL.Value = value;
    }

    // static uint d1fifoctr_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regD1FIFOCTR;
    // }

    void d1fifoctr_write(ushort value, uint address, int rqlen) {
        int curpipe = regD1FIFOSEL.Value & FIFOSEL_CURPIPE_MSK;
        RXUsbPipe pipe = Pipe[curpipe];

        value &= (ushort)~(FIFOCTR_DTLN_MSK | FIFOCTR_FRDY);
        value |= (ushort)usb_fifo_length(pipe.fifo);
        if (pipe.frdy) {
            value |= FIFOCTR_FRDY;
        }
        regD1FIFOCTR.Value = value;
    }

    /**
* Register INTENB0:
* Interrupt enable register 0
* Bit 8: BRDYE  Buffer ready interrupt enable.
* Bit 9: NRDYE Buffer not ready respones interrupt enable
* Bit 10: BEMPE Buffer empty interrupt enable.
* Bit 11: Control transfer stage transition interrupt enable
* Bit 12: DVSE Device state transition interrupt enable.
* Bit 13: SOFE Start of frame interrupt enable
* Bit 14: RSME Resume interrupt enable
* Bit 15: VBSE VBUS interrupt enable
*/
    // static uint intenb0_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regINTENB0;
    // }

    void intenb0_write(ushort value, uint address, int rqlen) {
        ushort intsts0 = (ushort)(regINTSTS0.Value & (INTSTS0_VBINT | INTSTS0_RESM | INTSTS0_SOFR | INTSTS0_DVST | INTSTS0_CTRT | INTSTS0_BEMP | INTSTS0_NRDY | INTSTS0_BRDY));
        value &= (ushort)(INTENB0_VBSE | INTENB0_RSME | INTENB0_SOFE | INTENB0_DVSE | INTENB0_CTRE | INTENB0_BEMPE | INTENB0_NRDYE | INTENB0_BRDYE);
        regINTENB0.Value = value;
        if ((intsts0 & value) != 0) {
            int_req = true;
            sofTimer.Schedule(TimeSpan.FromMicroseconds(1).TotalNanoseconds);
        }
    }

    /**
* Register INTENB1:
* Interrupt enable register 1. ONLY Host controller mode !
* Bit 4: SACKE Setup transaction normale response interrupt enable.
* Bit 5: SIGNE Setup transaction error interrupt enable.
* Bit 6: EOFERRE Error detection interrupt enable.
* Bit 11: ATTCHE Connection detection interrupt enable.
* Bit 12: DTCHE Disconnect detection interrupt enable.
* Bit 14: BCHGE Interrupt output disabled.
* Bit 15: OVRCRE Overcurrent Change interrupt eneble
*/
    // static uint intenb1_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regINTENB1;
    // }

    // static void intenb1_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regINTENB1 = value;
    // }

    /**
* Register BRDYENB
* Bufer Ready Interrupt enables
*/
    // static uint brdyenb_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regBRDYENB;
    // }

    // static void brdyenb_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regBRDYENB = value;
    // }

    /**
* Register NRDYENB
* Bufer not Ready Interrupt enables
*/
    // static uint nrdyenb_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regNRDYENB;
    // }

    // static void nrdyenb_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regNRDYENB = value;
    // }

    /**
* Buffer empty interrupt enables.
*/
    // static uint bempenb_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regBEMPENB;
    // }

    // static void bempenb_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regBEMPENB = value;
    // }

    /**
* Start of frame output configuration register.
* Bit 4: EDGESTS ? Was ist edge processing ?????
* Bit 5: BRDYM set this bit to 0
* Bit 6: TRNSEL Transaction enabled time select. Set to 0 in function mode !
*/
    // static uint sofcfg_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regSOFCFG;
    // }

    // static void sofcfg_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regSOFCFG = value;
    // }

    /**
* Register INTSTS0
* Interrupt status register.
* Some of the bits are write 0 to clear.
* Bits 0 - 2 CTSQ Control transfer stage.
* Bit 3: VALID 1 = setup packet reception.
* Bit 4 - 6: Device state
* Bit 7: VBus Input status
* Bit 8: BRDY Buffer ready interrupt status.
* Bit 9: NRDY Buffer not ready interrupt status.
* Bit 10: BEMP Buffer empty interrupt status.
* Bit 11: CTRT Control transfer stage transition interrupt status.
* Bit 12: DVST device state transition interrupt status.
* Bit 13: SOFR Start of frame interrupt status.
* Bit 14: RESM Resume interrupt status
* Bit 15: VBINT VBUS interrupt status.
*/
    uint intsts0_read(uint address, int rqlen) {
        ushort intsts0 = regINTSTS0.Value;
        intsts0 &= (ushort)~(INTSTS0_DVSQ_MSK | INTSTS0_CTSQ_MSK);
        intsts0 |= (ushort)((byte)DevSeq << INTSTS0_DVSQ_OFS);
        intsts0 |= (ushort)(CtrSeq << INTSTS0_CTSQ_OFS);
        return intsts0;
    }

    void intsts0_write(ushort value, uint address, int rqlen) {
        value |= (ushort)~(INTSTS0_VBINT | INTSTS0_RESM | INTSTS0_SOFR | INTSTS0_DVST | INTSTS0_CTRT | INTSTS0_BEMP | INTSTS0_NRDY | INTSTS0_BRDY);

        //assert(((~ru.regINTSTS0) & (~value)) == 0);

        regINTSTS0.Value &= value;
    }

    /**
* Register INTSTS1
* Interrupt status register 1. Host mode only
* Bit 15: OVRCR  Overrcurrent interrupt status.
* Bit 14: BCHG USB Bus Change interrupt Status.
* Bit 12: DTCH disconnection interrupt status
* Bit 11: ATTCH Connect interrupt status.
* Bit 6: EOFERR EOF error detection interrupt status.
* Bit 5: SIGN Setup Transaction interrupt status.
* Bit 4: SACK Setup transaction normal response interrupt status.
*/
    // static uint intsts1_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regINTSTS1;
    // }

    // static void intsts1_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regINTSTS1 = value;
    // }

    /**
* BRDYSTS
* Buffer ready interrupt status register
*/
    uint brdysts_read(uint address, int rqlen) {
        uint result = 0;
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if (Pipe[i].brdy) {
                result |= b;
            }
        }
        return result;
    }

    void brdysts_write(uint value, uint address, int rqlen) {
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if ((value & b) == 0) {
                Pipe[i].brdy = false;
            }
        }
    }

    /**
* Buffer Not ready interrupt status register
*/
    uint nrdysts_read(uint address, int rqlen) {
        uint result = 0;
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if (Pipe[i].nrdy) {
                result |= b;
            }
        }
        return result;
    }

    void nrdysts_write(uint value, uint address, int rqlen) {
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if ((value & b) == 0) {
                Pipe[i].nrdy = false;
            }
        }
    }

    /**
* BEMPTSTS
* Buffer empty interrupt status register.
* BEMTS is not set if buffer is emptied by BCLR.
*/
    uint bempsts_read(uint address, int rqlen) {
        uint result = 0;
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if (Pipe[i].bemp) {
                result |= b;
            }
        }
        return result;
    }

    void bempsts_write(uint value, uint address, int rqlen) {
        uint b = 0x001;
        for (int i = 0; i < 9; i++, b <<= 1) {
            if ((value & b) == 0) {
                Pipe[i].bemp = false;
            }
        }
    }

    /**
* Register  FRMNUM
* latest framenumber
* Bits 0 - 10 Framenumber of last received or sent SOF packet.
* Bit 14: CRCE Receive data error.
* Bit 15: Overrun/Underrun detection status.
*/
    // static uint frmnum_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regFRMNUM;
    // }

    // static void frmnum_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regFRMNUM = value;
    // }

    /**
* Register DVCHGR
* Device state change register.
* Bit 15
*/
    // static uint dvchgr_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDVCHGR;
    // }

    // static void dvchgr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDVCHGR = value;
    // }

    /**
* Register USBADDR
* Bits 0-6: Indicates the assigned USB device address (Function mode only)
* Bits 8-11: Status recovery.
*/
    // static uint usbaddr_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regUSBADDR;
    // }

    // static void usbaddr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regUSBADDR = value;
    // }

    /**
* USB request
* Host: USB request to send, Function: USB request received.
*/
    // static uint usbreq_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regUSBREQ;
    // }

    // static void usbreq_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regUSBREQ = value;
    // }

    /**
* Register USBVAL
* Stores the request value.
*/
    // static uint usbval_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regUSBVAL;
    // }

    // static void usbval_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regUSBVAL = value;
    // }

    /**
* Register USBINDX
*/
    // static uint usbindx_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regUSBINDX;
    // }

    // static void usbindx_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regUSBINDX = value;
    // }

    /**
* USB request length.
*/
    // static uint usbleng_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regUSBLENG;
    // }

    // static void usbleng_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regUSBLENG = value;
    // }

    /**
* DCPCFG
* Bit 4: DIR Transfer Direction
* Bit 7: SHTNAK Pipe is disabled at end of tranfer (following packets
*         will be NAKed)
*/
    uint dcpcfg_read(uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];

        return pipe.regPIPECFG.Value;
    }

    void dcpcfg_write(ushort value, uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];

        pipe.regPIPECFG.Value = value;

        if ((value & PIPECFG_DIR) != 0) {
            pipe.isel = true;
        }
        else {
            pipe.isel = false;
        }
    }

    /**
* DCPMAXP
* DCP maximum packet size register
* Bits 0-6: Maximum packet size.
* Bits 12-15: Device select 0-5
*/
    uint dcpmaxp_read(uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];

        return pipe.regPIPEMAXP.Value;
    }

    void dcpmaxp_write(ushort value, uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];

        pipe.regPIPEMAXP.Value = value;
    }

    /**
* DCP Control Register DCPCTR
* Bit 0-1: PID Respnse PID 00 = NAK, 01 = BUF, 10,11 = STALL
* Bit 2: CCPL Control Tranfer End Enable
* Bit 5: PBUSY Pipe Busy
* Bit 6: SQMON Sequence Toggle Bit monitor
* Bit 7: SQSET Toggle Bit Set
* Bit 8: SQCLR Toggle Bit Clear
* Bit 11: SUREQCLR SUREQ Bit clear
* Bit 14: SUREQ Setup Token Transmission
* Bit 15: BSTS Buffer Status
*/
    ushort dcpctr_read(uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];
        ushort result = pipe.regPIPECTR.Value;

        if (pipe.isel) {
            if (true) {
                result |= DCPCTR_BSTS;
            }
            else {
                result &= (ushort)~DCPCTR_BSTS;
            }
        }
        else {
            if (true) {
                result |= DCPCTR_BSTS;
            }
            else {
                result &= (ushort)~DCPCTR_BSTS;
            }
        }

        return result;
    }

    void dcpctr_write(ushort value, uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[0];

        pipe.regPIPECTR.Value = value;
    }

    /**
* Register PIPESEL
* Pipe Window Select Register, 0 = No pipe, 1-9
*/
    // static uint pipesel_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regPIPESEL;
    // }

    // static void pipesel_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regPIPESEL = value;
    // }

    /**
* Register PIECFG
* Bit 0 - 3: Endpoint Number
* Bit 4: Transfer Direction
* Bit 7: SHTNAK Disable pipe at end of transfer
* Bit 9: DBLB Double buffer mode
* Bit 10: BFRE BRDY Interrupt operation specification
* Bit 14 - 15: Transfer Type 00: unused 01: Bulk, 11: Isochronous
*/
    uint pipecfg_read(uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return 0;
        }
        return pipe.regPIPECFG.Value;
    }

    void pipecfg_write(ushort value, uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return;
        }
        pipe.regPIPECFG.Value = value;

        if ((value & PIPECFG_DIR) != 0) {
            pipe.isel = true;
        }
        else {
            pipe.isel = false;
        }
    }

    uint pipemaxp_read(uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return 0;
        }
        return pipe.regPIPEMAXP.Value;
    }

    void pipemaxp_write(ushort value, uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return;
        }
        pipe.regPIPEMAXP.Value = value;
    }

    uint pipeperi_read(uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return 0;
        }
        return pipe.regPIPEPERI.Value;
    }

    void pipeperi_write(ushort value, uint address, int rqlen) {
        int curpipe = regPIPESEL.Value & PIPESEL_PIPESEL_MSK;
        RXUsbPipe pipe = Pipe[curpipe];
        if ((curpipe <= 0) || (curpipe > 9)) {
            return;
        }
        pipe.regPIPEPERI.Value = value;
    }

    uint pipe1ctr_read(uint address, int rqlen) {
        RXUsbPipe pipe = Pipe[1];
        return pipe.regPIPETRE.Value;
    }

    // static void pipe1ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE.Value = value;
    // }

    // static uint pipe2ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe2ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe3ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe3ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe4ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe4ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe5ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe5ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe6ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe6ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe7ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe7ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe8ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe8ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe9ctr_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe9ctr_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe1tre_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe1tre_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe1trn_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRN;
    // }

    // static void pipe1trn_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRN = value;
    // }

    // static uint pipe2tre_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe2tre_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe2trn_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRN;
    // }

    // static void pipe2trn_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRN = value;
    // }

    // static uint pipe3tre_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe3tre_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe3trn_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRN;
    // }

    // static void pipe3trn_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRN = value;
    // }

    // static uint pipe4tre_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe4tre_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe4trn_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRN;
    // }

    // static void pipe4trn_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRN = value;
    // }

    // static uint pipe5tre_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRE;
    // }

    // static void pipe5tre_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRE = value;
    // }

    // static uint pipe5trn_read(USB0 ru, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     return pipe.regPIPETRN;
    // }

    // static void pipe5trn_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     RXUsbPipe pipe = ru.Pipe[1];
    //     pipe.regPIPETRN = value;
    // }

    // static uint devadd0_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD0;
    // }

    // static void devadd0_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD0 = value;
    // }

    // static uint devadd1_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD1;
    // }

    // static void devadd1_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD1 = value;
    // }

    // static uint devadd2_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD2;
    // }

    // static void devadd2_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD2 = value;
    // }

    // static uint devadd3_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD3;
    // }

    // static void devadd3_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD3 = value;
    // }

    // static uint devadd4_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD4;
    // }

    // static void devadd4_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD4 = value;
    // }

    // static uint devadd5_read(USB0 ru, uint address, int rqlen) {
    //     return ru.regDEVADD5;
    // }

    // static void devadd5_write(USB0 ru, ushort value, uint address, int rqlen) {
    //     ru.regDEVADD5 = value;
    // }

//     static void RxUsbFun_Map(void *owner, uint base, uint mask, uint flags) {
//         void *usbfn = owner;
//         IOH_New16(REG_SYSCFG(base), syscfg_read, syscfg_write, usbfn);
//         IOH_New16(REG_SYSSTS0(base), syssts0_read, syssts0_write, usbfn);
//         IOH_New16(REG_DVSTCTR0(base), dvstctr0_read, dvstctr0_write, usbfn);
//         IOH_New16(REG_CFIFO(base), cfifo_read, cfifo_write, usbfn);
//         IOH_New16(REG_D0FIFO(base), d0fifo_read, d0fifo_write, usbfn);
//         IOH_New16(REG_D1FIFO(base), d1fifo_read, d1fifo_write, usbfn);
//         IOH_New16(REG_CFIFOSEL(base), cfifosel_read, cfifosel_write, usbfn);
//         IOH_New16(REG_CFIFOCTR(base), cfifoctr_read, cfifoctr_write, usbfn);
//         IOH_New16(REG_D0FIFOSEL(base), d0fifosel_read, d0fifosel_write, usbfn);
//         IOH_New16(REG_D0FIFOCTR(base), d0fifoctr_read, d0fifoctr_write, usbfn);
//         IOH_New16(REG_D1FIFOSEL(base), d1fifosel_read, d1fifosel_write, usbfn);
//         IOH_New16(REG_D1FIFOCTR(base), d1fifoctr_read, d1fifoctr_write, usbfn);
//         IOH_New16(REG_INTENB0(base), intenb0_read, intenb0_write, usbfn);
//         IOH_New16(REG_INTENB1(base), intenb1_read, intenb1_write, usbfn);
//         IOH_New16(REG_BRDYENB(base), brdyenb_read, brdyenb_write, usbfn);
//         IOH_New16(REG_NRDYENB(base), nrdyenb_read, nrdyenb_write, usbfn);
//         IOH_New16(REG_BEMPENB(base), bempenb_read, bempenb_write, usbfn);
//         IOH_New16(REG_SOFCFG(base), sofcfg_read, sofcfg_write, usbfn);
//         IOH_New16(REG_INTSTS0(base), intsts0_read, intsts0_write, usbfn);
//         IOH_New16(REG_INTSTS1(base), intsts1_read, intsts1_write, usbfn);
//         IOH_New16(REG_BRDYSTS(base), brdysts_read, brdysts_write, usbfn);
//         IOH_New16(REG_NRDYSTS(base), nrdysts_read, nrdysts_write, usbfn);
//         IOH_New16(REG_BEMPSTS(base), bempsts_read, bempsts_write, usbfn);
//         IOH_New16(REG_FRMNUM(base), frmnum_read, frmnum_write, usbfn);
//         IOH_New16(REG_DVCHGR(base), dvchgr_read, dvchgr_write, usbfn);
//         IOH_New16(REG_USBADDR(base), usbaddr_read, usbaddr_write, usbfn);
//         IOH_New16(REG_USBREQ(base), usbreq_read, usbreq_write, usbfn);
//         IOH_New16(REG_USBVAL(base), usbval_read, usbval_write, usbfn);
//         IOH_New16(REG_USBINDX(base), usbindx_read, usbindx_write, usbfn);
//         IOH_New16(REG_USBLENG(base), usbleng_read, usbleng_write, usbfn);
//         IOH_New16(REG_DCPCFG(base), dcpcfg_read, dcpcfg_write, usbfn);
//         IOH_New16(REG_DCPMAXP(base), dcpmaxp_read, dcpmaxp_write, usbfn);
//         IOH_New16(REG_DCPCTR(base), dcpctr_read, dcpctr_write, usbfn);
//         IOH_New16(REG_PIPESEL(base), pipesel_read, pipesel_write, usbfn);
//         IOH_New16(REG_PIPECFG(base), pipecfg_read, pipecfg_write, usbfn);
//         IOH_New16(REG_PIPEMAXP(base), pipemaxp_read, pipemaxp_write, usbfn);
//         IOH_New16(REG_PIPEPERI(base), pipeperi_read, pipeperi_write, usbfn);
//         IOH_New16(REG_PIPE1CTR(base), pipe1ctr_read, pipe1ctr_write, usbfn);
//         IOH_New16(REG_PIPE2CTR(base), pipe2ctr_read, pipe2ctr_write, usbfn);
//         IOH_New16(REG_PIPE3CTR(base), pipe3ctr_read, pipe3ctr_write, usbfn);
//         IOH_New16(REG_PIPE4CTR(base), pipe4ctr_read, pipe4ctr_write, usbfn);
//         IOH_New16(REG_PIPE5CTR(base), pipe5ctr_read, pipe5ctr_write, usbfn);
//         IOH_New16(REG_PIPE6CTR(base), pipe6ctr_read, pipe6ctr_write, usbfn);
//         IOH_New16(REG_PIPE7CTR(base), pipe7ctr_read, pipe7ctr_write, usbfn);
//         IOH_New16(REG_PIPE8CTR(base), pipe8ctr_read, pipe8ctr_write, usbfn);
//         IOH_New16(REG_PIPE9CTR(base), pipe9ctr_read, pipe9ctr_write, usbfn);
//         IOH_New16(REG_PIPE1TRE(base), pipe1tre_read, pipe1tre_write, usbfn);
//         IOH_New16(REG_PIPE1TRN(base), pipe1trn_read, pipe1trn_write, usbfn);
//         IOH_New16(REG_PIPE2TRE(base), pipe2tre_read, pipe2tre_write, usbfn);
//         IOH_New16(REG_PIPE2TRN(base), pipe2trn_read, pipe2trn_write, usbfn);
//         IOH_New16(REG_PIPE3TRE(base), pipe3tre_read, pipe3tre_write, usbfn);
//         IOH_New16(REG_PIPE3TRN(base), pipe3trn_read, pipe3trn_write, usbfn);
//         IOH_New16(REG_PIPE4TRE(base), pipe4tre_read, pipe4tre_write, usbfn);
//         IOH_New16(REG_PIPE4TRN(base), pipe4trn_read, pipe4trn_write, usbfn);
//         IOH_New16(REG_PIPE5TRE(base), pipe5tre_read, pipe5tre_write, usbfn);
//         IOH_New16(REG_PIPE5TRN(base), pipe5trn_read, pipe5trn_write, usbfn);
//         IOH_New16(REG_DEVADD0(base), devadd0_read, devadd0_write, usbfn);
//         IOH_New16(REG_DEVADD1(base), devadd1_read, devadd1_write, usbfn);
//         IOH_New16(REG_DEVADD2(base), devadd2_read, devadd2_write, usbfn);
//         IOH_New16(REG_DEVADD3(base), devadd3_read, devadd3_write, usbfn);
//         IOH_New16(REG_DEVADD4(base), devadd4_read, devadd4_write, usbfn);
//         IOH_New16(REG_DEVADD5(base), devadd5_read, devadd5_write, usbfn);
// #if 0
//         IOH_New32(REG_DPUSR0R);
//         IOH_New32(REG_DPUSR1R);
// #endif
//     }

    static ushort control_seq(USB0 ru, bool setup) {
        RXUsbPipe pipe = ru.Pipe[0];

        switch (ru.HostStupSeq) {
            case 0:
                if (!setup) {
                    ru.regINTSTS0.Value |= INTSTS0_VALID;
                    pipe.regPIPETRE.Value &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL); /*NAK*/
                    // ru.regUSBREQ = /*bRequest*/(USBRQ_GET_DESCRIPTOR << 8) | /*bmRequestType*/(0x80);
                    // ru.regUSBVAL = /*bDescriptorType*/(USB_DT_DEVICE << 8) | /*Descriptor Index*/(0x00);
                    ru.regUSBINDX.Value = /*Language Id*/0;
                    ru.regUSBLENG.Value = /*wLength*/18;
                }
                return CTSQ_RDDS;
            case 2:
                if (!setup) {
                    ru.regINTSTS0.Value |= INTSTS0_VALID;
                    pipe.regPIPETRE.Value &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL); /*NAK*/
                    // ru.regUSBREQ = /*bRequest*/(USBRQ_GET_DESCRIPTOR << 8) | /*bmRequestType*/(0x80);
                    // ru.regUSBVAL = /*bDescriptorType*/(USB_DT_CONFIG << 8) | /*Descriptor Index*/(0x00);
                    ru.regUSBINDX.Value = /*Language Id*/0;
                    ru.regUSBLENG.Value = /*wLength*/100;
                }
                return CTSQ_RDDS;
            case 4:
                if (!setup) {
                    ru.regINTSTS0.Value |= INTSTS0_VALID;
                    pipe.regPIPETRE.Value &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL); /*NAK*/
                    // ru.regUSBREQ = /*bRequest*/(USBRQ_SET_CONFIGURATION << 8) | /*bmRequestType*/(0x00);
                    ru.regUSBVAL.Value = /*bConfigurationValue*/1;
                    ru.regUSBINDX.Value = /*wIndex*/0;
                    ru.regUSBLENG.Value = /*wLength*/0;
                }
                return CTSQ_WRDS;
            default:
                return CTSQ_IDST;
        }
    }

    ValueTask SendUSBPacket(USBPacket packet) {
        return ValueTask.CompletedTask;
    }

    ValueTask Wait(TimeSpan time, CancellationToken token) {
        return ValueTask.CompletedTask;
    }

    void SetUSBIRQStatus(ushort intsts0_int, ushort intenb0) {
        regINTSTS0.Value |= intsts0_int;

        ushort intsts0_now = (ushort)(regINTSTS0.Value &
            INTSTS0_VBINT | INTSTS0_RESM | INTSTS0_SOFR | INTSTS0_DVST | INTSTS0_CTRT | INTSTS0_BEMP | INTSTS0_NRDY | INTSTS0_BRDY);

        if (intsts0_now != intsts0_int && (regINTENB0.Value & intenb0) != 0) {
            sigIrq.SetOutputSignal(false);
            sigIrq.SetOutputSignal(true);
        }
    }

    void SetControlSequence(ushort ctrseq) {
        bool isChanged = CtrSeq != ctrseq;
        CtrSeq = ctrseq;
        if (isChanged) {
            SetUSBIRQStatus(INTSTS0_CTRT, INTENB0_CTRE);
        }
    }

    // async ValueTask SendUSBHostPackets(CancellationToken token) {
    //     await SendUSBPacket(USBPacket.SOF(regFRMNUM.Value));
    //     regFRMNUM.Value = (ushort)((regFRMNUM.Value + 1) & 0x07FF);
    //     SetUSBIRQStatus(INTSTS0_SOFR, INTENB0_SOFE);

    // }


    /// <summary>
    /// 一連のパケットの処理を行います。
    /// </summary>
    async ValueTask HostProcess(CancellationToken token) {
        USB0 ru = this;
        if (ru.int_req) {
            ru.int_req = false;
            ru.sigIrq.SetOutputSignal(false);
            ru.sigIrq.SetOutputSignal(true);
            ru.sofTimer.Schedule(TimeSpan.FromMicroseconds(1).TotalNanoseconds);
            return;
        }

        TimeSpan timeout = TimeSpan.Zero;

        do {
            ushort curpipe = ru.HostPipe;
            RXUsbPipe pipe = ru.Pipe[curpipe];
            ushort pipecfg = pipe.regPIPECFG.Value;
            ushort pipemaxp = pipe.regPIPEMAXP.Value;
            ushort pipeperi = pipe.regPIPEPERI.Value;
            var pipectr =  pipe.regPIPECTR.Value;

            switch (ru.HostFrmSeq) {
                case 0:
                    // SOF
                    ru.regFRMNUM.Value = (ushort)((regFRMNUM.Value + 1) & 0x07FF);

                    // 送り始めを通知する
                    SetUSBIRQStatus(INTSTS0_SOFR, INTENB0_SOFE);
                    // USB の仕様がフレーム番号0からスタートするのか
                    // 1からスタートするのか不明
                    await SendUSBPacket(USBPacket.SOF(regFRMNUM.Value));
                    ru.HostFrmSeq = 1;
                    ru.HostPipe = 1;
                    break;
                case 1:
                    // Cyclic Trans
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        if ((pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                            if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_ISOCHRONOUS) {
                                if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                    if ((pipecfg & PIPECFG_DIR) != 0/*OUT*/) {
                                        SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                        await Wait(TimeSpan.FromMicroseconds(125), token);
                                        break;
                                    }
                                    else {
                                        SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                        await Wait(TimeSpan.FromMicroseconds(125), token);
                                        break;
                                    }
                                }
                            }
                            else if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_INTERRUPT) {
                                if ((pipecfg & PIPECFG_DIR) != 0/*OUT*/) {
                                    if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                        SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                        await Wait(TimeSpan.FromMicroseconds(125), token);
                                        break;
                                    }
                                }
                                else {
                                    if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                        SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                        await Wait(TimeSpan.FromMicroseconds(125), token);
                                        break;
                                    }
                                }
                            }
                        }
                        if (ru.HostPipe == 9) {
                            ru.HostFrmSeq = 2;
                            ru.HostPipe = 0;
                        }
                        else {
                            ru.HostPipe++;
                        }
                    }
                    else {
                        ru.HostFrmSeq = 2;
                        ru.HostPipe = 0;
                    }
                    break;
                case 2:
                    // Control Setup
                    if (CtrSeq == CTSQ_IDST) {
                        switch (ru.DevSeq) {
                            /* Powered state */
                            case USBDeviceState.Powered:
                                while (!usbBus.IsConnected)
                                {
                                    await Wait(TimeSpan.FromMilliseconds(1), token);
                                }
                                await Wait(TimeSpan.FromMilliseconds(1), token);
                                usbBus.ResetSignal = true;
                                await Wait(TimeSpan.FromMilliseconds(1), token);
                                usbBus.ResetSignal = false;
                                await Wait(TimeSpan.FromMilliseconds(1), token);

                                ru.DevSeq = USBDeviceState.Default;
                                SetUSBIRQStatus((ushort)(INTSTS0_VBINT | INTSTS0_DVST), (ushort)(INTENB0_VBSE | INTENB0_DVSE));
                                ru.regINTSTS0.Value |= INTSTS0_VBSTS;
                                break;
                            case USBDeviceState.Default:
                                ru.regUSBADDR.Value = (ushort)((0x0800 | ((byte)ru.DevSeq << 8))/*STSRECOV*/ | 1/*USBADDR*/);
                                ru.DevSeq = USBDeviceState.Address;
                                SetUSBIRQStatus(INTSTS0_DVST, INTENB0_DVSE);
                                await Wait(TimeSpan.FromMicroseconds(125), token);
                                break;
                            case USBDeviceState.Address:
                                ru.HostStupSeq = 0;
                                ru.DevSeq = USBDeviceState.Configured;
                                ru.HostStupSeq++;
                                if ((ru.regUSBREQ.Value & 0x80) != 0) {
                                    ru.HostInRecv = true;
                                }
                                SetUSBIRQStatus(INTSTS0_DVST, INTENB0_DVSE);
                                control_seq(ru, false);
                                SetControlSequence(CTSQ_RDDS);
                                await Wait(TimeSpan.FromMicroseconds(125), token);
                                break;
                            case USBDeviceState.Configured:
                                if (control_seq(ru, false) != CTSQ_IDST) {
                                    ru.HostStupSeq++;
                                    if ((ru.regUSBREQ.Value & 0x80) != 0)
                                        ru.HostInRecv = true;
                                    SetControlSequence(CTSQ_RDDS);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                                else if (ru.HostInRecv) {
                                    SetControlSequence(CTSQ_WRDS);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                                else if (pipe.frdy) {
                                    SetControlSequence(CTSQ_RDDS);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                                break;
                                /* Suspended state */
                            default:
                                break;
                        }
                    }
                    ru.HostFrmSeq = 3;
                    ru.HostPipe = 0;
                    break;
                case 3:
                    // Control Data/State
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        switch (CtrSeq) {
                            case CTSQ_IDST:
                                if (ru.HostPipe == 9) {
                                    ru.HostFrmSeq = 4;
                                    ru.HostPipe = 1;
                                }
                                else {
                                    ru.HostPipe++;
                                }
                                break;
                            case CTSQ_WRDS:
                                if ((curpipe == 0) || (pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                                    if ((pipectr & DCPCTR_PID_MSK) == PID_BUF) {
                                        if ((pipectr & DCPCTR_CCPL) == 0) {
                                            bool fifo_bval = false;
                                            bool fifo_full = false;
                                            RegisterValue32<ushort> fifoctr;
                                            if (curpipe == 0) {
                                                fifoctr = ru.regCFIFOCTR;
                                            }
                                            else if ((pipectr & DCPCTR_SQMON) == 0) {
                                                fifoctr = ru.regD0FIFOCTR;
                                            }
                                            else {
                                                fifoctr = ru.regD1FIFOCTR;
                                            }
                                            fifo_bval = (fifoctr.Value & FIFOCTR_BVAL) != 0;
                                            fifo_full = usb_fifo_length(pipe.fifo) == (pipemaxp & DCPMAXP_MXPS_MSK);
                                            if (fifo_bval || fifo_full) {
                                                fifoctr.Value &= (ushort)(~FIFOCTR_DTLN_MSK);
                                                fifoctr.Value |= (ushort)(usb_fifo_length(pipe.fifo) & FIFOCTR_DTLN_MSK);
                                                usb_fifo_clear(pipe.fifo);
                                                if (fifo_bval) {
                                                    ru.HostStupSeq++;
                                                    ru.HostInRecv = false;
                                                }
                                                else {
                                                    ru.HostInRecv = true;
                                                }
                                            }
                                            pipe.brdy = true;
                                            SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                            SetControlSequence(CTSQ_WRSS);
                                            await Wait(TimeSpan.FromMicroseconds(125), token);
                                        }
                                        else {
                                            ru.HostStupSeq++;
                                            ru.HostInRecv = false;
                                            pipe.brdy = true;
                                            SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                            SetControlSequence(CTSQ_WRSS);
                                            await Wait(TimeSpan.FromMicroseconds(125), token);
                                        }
                                    }
                                }
                                break;
                            case CTSQ_WRSS:
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                SetControlSequence(CTSQ_IDST);
                                await Wait(TimeSpan.FromMicroseconds(125), token);
                                break;
                            case CTSQ_WRNDSS:
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                SetControlSequence(CTSQ_IDST);
                                await Wait(TimeSpan.FromMicroseconds(125), token);
                                break;
                            case CTSQ_RDDS:
                                if ((pipectr & DCPCTR_PID_MSK) == PID_BUF) {
                                    SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                    pipe.brdy = true;
                                    SetControlSequence(CTSQ_RDSS);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                                break;
                            case CTSQ_RDSS:
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                SetControlSequence(CTSQ_IDST);
                                await Wait(TimeSpan.FromMicroseconds(125), token);
                                break;
                        }
                    }
                    if (timeout == TimeSpan.Zero) {
                        //ctrseq = CTSQ_IDST;
                        ru.HostFrmSeq = 4;
                        ru.HostPipe = 1;
                    }
                    break;
                case 4:
                    // Bulk Data
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        if ((pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                            if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_BULK) {
                                if (((pipecfg & PIPECFG_DIR) != 0/*OUT*/) && ((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) == 0)) {
                                    // (HOST)�f�[�^��M
                                    SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                            }
                            else if (CtrSeq == CTSQ_RDDS) {
                                if (((pipecfg & PIPECFG_DIR) == 0/*IN*/) && ((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) == 0)) {
                                    // (HOST)�f�[�^���M
                                    SetUSBIRQStatus(INTSTS0_BRDY, INTENB0_BRDYE);
                                    await Wait(TimeSpan.FromMicroseconds(125), token);
                                }
                            }
                        }
                        if (ru.HostPipe == 9) {
                            ru.HostFrmSeq = 5; /* 1ms�Ɏ��܂��Ă���΁A4�̌J��Ԃ� */
                            ru.HostPipe = 1;
                        }
                        else {
                            ru.HostPipe++;
                        }
                    }
                    else {
                        ru.HostFrmSeq = 5;
                        ru.HostPipe = 1;
                    }
                    break;
                default:
                    // Idle
                    ru.HostFrmSeq = 0;
                    ru.HostPipe = 1;
                    await Wait(TimeSpan.FromMicroseconds(125), token);
                    break;
            }
        } while (timeout == TimeSpan.Zero);

        // ru.sofTimer.Schedule(timeout.TotalNanoseconds);
    }


    void timer_event() {
        USB0 ru = this;
        if (ru.int_req) {
            ru.int_req = false;
            ru.sigIrq.SetOutputSignal(false);
            ru.sigIrq.SetOutputSignal(true);
            ru.sofTimer.Schedule(TimeSpan.FromMicroseconds(1).TotalNanoseconds);
            return;
        }

        ushort intsts0_int = 0;
        ushort intenb0 = 0;
        TimeSpan timeout = TimeSpan.Zero;
        ushort ctrseq = ru.CtrSeq;

        do {
            ushort curpipe = ru.HostPipe;
            RXUsbPipe pipe = ru.Pipe[curpipe];
            ushort pipecfg = pipe.regPIPECFG.Value;
            ushort pipemaxp = pipe.regPIPEMAXP.Value;
            ushort pipeperi = pipe.regPIPEPERI.Value;
            var pipectr =  pipe.regPIPECTR.Value;

            switch (ru.HostFrmSeq) {
                case 0:
                    // SOF
                    ru.regFRMNUM.Value = (ushort)((regFRMNUM.Value + 1) & 0x07FF);
                    intsts0_int |= INTSTS0_SOFR;
                    intenb0 |= INTENB0_SOFE;
                    timeout = TimeSpan.FromMicroseconds(125);
                    ru.HostFrmSeq = 1;
                    ru.HostPipe = 1;
                    break;
                case 1:
                    // Cyclic Trans
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        if ((pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                            if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_ISOCHRONOUS) {
                                if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                    if ((pipecfg & PIPECFG_DIR) != 0/*OUT*/) {
                                        // (HOST)�f�[�^��M
                                        intsts0_int |= INTSTS0_BRDY;
                                        intenb0 |= INTENB0_BRDYE;
                                        timeout = TimeSpan.FromMicroseconds(125);
                                        break;
                                    }
                                    else {
                                        // (HOST)�f�[�^���M
                                        intsts0_int |= INTSTS0_BRDY;
                                        intenb0 |= INTENB0_BRDYE;
                                        timeout = TimeSpan.FromMicroseconds(125);
                                        break;
                                    }
                                }
                            }
                            else if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_INTERRUPT) {
                                if ((pipecfg & PIPECFG_DIR) != 0/*OUT*/) {
                                    if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                        // (HOST)�f�[�^��M
                                        intsts0_int |= INTSTS0_BRDY;
                                        intenb0 |= INTENB0_BRDYE;
                                        timeout = TimeSpan.FromMicroseconds(125);
                                        break;
                                    }
                                }
                                else {
                                    if (((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) != 0)) {
                                        // (HOST)�f�[�^���M
                                        intsts0_int |= INTSTS0_BRDY;
                                        intenb0 |= INTENB0_BRDYE;
                                        timeout = TimeSpan.FromMicroseconds(125);
                                        break;
                                    }
                                }
                            }
                        }
                        if (ru.HostPipe == 9) {
                            ru.HostFrmSeq = 2;
                            ru.HostPipe = 0;
                        }
                        else {
                            ru.HostPipe++;
                        }
                    }
                    else {
                        ru.HostFrmSeq = 2;
                        ru.HostPipe = 0;
                    }
                    break;
                case 2:
                    // Control Setup
                    if (ctrseq == CTSQ_IDST) {
                        switch (ru.DevSeq) {
                            /* Powered state */
                            case USBDeviceState.Powered:
                                if (ru.Counter >= 1000) {
                                    ru.Counter = 0;
                                    intsts0_int |= (ushort)(INTSTS0_VBINT | INTSTS0_DVST);
                                    intenb0 |= (ushort)(INTENB0_VBSE | INTENB0_DVSE);
                                    timeout = TimeSpan.FromMicroseconds(125);
                                    ru.DevSeq = USBDeviceState.Default;
                                    ru.regINTSTS0.Value |= INTSTS0_VBSTS;
                                }
                                else {
                                    ru.Counter++;
                                }
                                break;
                            case USBDeviceState.Default:
                                intsts0_int |= INTSTS0_DVST;
                                intenb0 |= INTENB0_DVSE;
                                timeout = TimeSpan.FromMicroseconds(125);
                                ru.DevSeq = USBDeviceState.Address;
                                ru.regUSBADDR.Value = (ushort)((0x0800 | ((byte)ru.DevSeq << 8))/*STSRECOV*/ | 1/*USBADDR*/);
                                break;
                            case USBDeviceState.Address:
                                ru.HostStupSeq = 0;
                                intsts0_int |= INTSTS0_DVST;
                                intenb0 |= INTENB0_DVSE;
                                timeout = TimeSpan.FromMicroseconds(125/*TODO*/);
                                ru.DevSeq = USBDeviceState.Configured;
                                control_seq(ru, false);
                                ru.HostStupSeq++;
                                if ((ru.regUSBREQ.Value & 0x80) != 0)
                                    ru.HostInRecv = true;
                                ctrseq = CTSQ_RDDS;
                                break;
                            case USBDeviceState.Configured:
                                if (control_seq(ru, false) != CTSQ_IDST) {
                                    ru.HostStupSeq++;
                                    if ((ru.regUSBREQ.Value & 0x80) != 0)
                                        ru.HostInRecv = true;
                                    timeout = TimeSpan.FromMicroseconds(125/*TODO*/);
                                    ctrseq = CTSQ_RDDS;
                                }
                                else if (ru.HostInRecv) {
                                    timeout = TimeSpan.FromMicroseconds(125/*TODO*/);
                                    ctrseq = CTSQ_WRDS;
                                }
                                else if (pipe.frdy) {
                                    timeout = TimeSpan.FromMicroseconds(125/*TODO*/);
                                    ctrseq = CTSQ_RDDS;
                                }
                                break;
                                /* Suspended state */
                            default:
                                break;
                        }
                    }
                    ru.HostFrmSeq = 3;
                    ru.HostPipe = 0;
                    break;
                case 3:
                    // Control Data/State
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        switch (ctrseq) {
                            case CTSQ_IDST:
                                if (ru.HostPipe == 9) {
                                    ru.HostFrmSeq = 4;
                                    ru.HostPipe = 1;
                                }
                                else {
                                    ru.HostPipe++;
                                }
                                break;
                            case CTSQ_WRDS:
                                // (HOST)�f�[�^���M
                                if ((curpipe == 0) || (pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                                    if ((pipectr & DCPCTR_PID_MSK) == PID_BUF) {
                                        if ((pipectr & DCPCTR_CCPL) == 0) {
                                            bool fifo_bval = false;
                                            bool fifo_full = false;
                                            RegisterValue32<ushort> fifoctr;
                                            if (curpipe == 0) {
                                                fifoctr = ru.regCFIFOCTR;
                                            }
                                            else if ((pipectr & DCPCTR_SQMON) == 0) {
                                                fifoctr = ru.regD0FIFOCTR;
                                            }
                                            else {
                                                fifoctr = ru.regD1FIFOCTR;
                                            }
                                            fifo_bval = (fifoctr.Value & FIFOCTR_BVAL) != 0;
                                            fifo_full = usb_fifo_length(pipe.fifo) == (pipemaxp & DCPMAXP_MXPS_MSK);
                                            if (fifo_bval || fifo_full) {
                                                fifoctr.Value &= (ushort)(~FIFOCTR_DTLN_MSK);
                                                fifoctr.Value |= (ushort)(usb_fifo_length(pipe.fifo) & FIFOCTR_DTLN_MSK);
                                                usb_fifo_clear(pipe.fifo);
                                                if (fifo_bval) {
                                                    ru.HostStupSeq++;
                                                    ru.HostInRecv = false;
                                                }
                                                else {
                                                    ru.HostInRecv = true;
                                                }
                                            }
                                            timeout = TimeSpan.FromMicroseconds(125);
                                            intsts0_int |= INTSTS0_BRDY;
                                            intenb0 |= INTENB0_BRDYE;
                                            pipe.brdy = true;
                                            ctrseq = CTSQ_WRSS;
                                        }
                                        else {
                                            ru.HostStupSeq++;
                                            ru.HostInRecv = false;
                                            timeout = TimeSpan.FromMicroseconds(125);
                                            intsts0_int |= INTSTS0_BRDY;
                                            intenb0 |= INTENB0_BRDYE;
                                            pipe.brdy = true;
                                            ctrseq = CTSQ_WRSS;
                                        }
                                    }
                                }
                                break;
                            case CTSQ_WRSS:
                                // (HOST)�X�e�[�^�X��M
                                timeout = TimeSpan.FromMicroseconds(125);
                                ctrseq = CTSQ_IDST;
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                break;
                            case CTSQ_WRNDSS:
                                // (HOST)�m�[�f�[�^�X�e�[�^�X��M
                                timeout = TimeSpan.FromMicroseconds(125);
                                ctrseq = CTSQ_IDST;
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                break;
                            case CTSQ_RDDS:
                                // (HOST)�f�[�^���M
                                if ((pipectr & DCPCTR_PID_MSK) == PID_BUF) {
                                    timeout = TimeSpan.FromMicroseconds(125/*TODO*/);
                                    ctrseq = CTSQ_RDSS;
                                    intsts0_int |= INTSTS0_BRDY;
                                    intenb0 |= INTENB0_BRDYE;
                                    pipe.brdy = true;
                                }
                                break;
                            case CTSQ_RDSS:
                                // (HOST)�X�e�[�^�X���M
                                timeout = TimeSpan.FromMicroseconds(125);
                                ctrseq = CTSQ_IDST;
                                pipectr &= (ushort)~(DCPCTR_PID_MSK | DCPCTR_CCPL);
                                break;
                        }
                    }
                    if (timeout == TimeSpan.Zero) {
                        //ctrseq = CTSQ_IDST;
                        ru.HostFrmSeq = 4;
                        ru.HostPipe = 1;
                    }
                    break;
                case 4:
                    // Bulk Data
                    if (ru.DevSeq == USBDeviceState.Configured) {
                        if ((pipecfg & PIPECFG_EPNUM_MSK) != 0) {
                            if ((pipecfg & PIPECFG_TYPE_MSK) == TYPE_BULK) {
                                if (((pipecfg & PIPECFG_DIR) != 0/*OUT*/) && ((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) == 0)) {
                                    // (HOST)�f�[�^��M
                                    intsts0_int |= INTSTS0_BRDY;
                                    intenb0 |= INTENB0_BRDYE;
                                    timeout = TimeSpan.FromMicroseconds(125);
                                }
                            }
                            else if (ctrseq == CTSQ_RDDS) {
                                if (((pipecfg & PIPECFG_DIR) == 0/*IN*/) && ((pipectr & DCPCTR_PID_MSK) == PID_BUF) && ((pipeperi & PIPEPERI_IITV_MSK) == 0)) {
                                    // (HOST)�f�[�^���M
                                    intsts0_int |= INTSTS0_BRDY;
                                    intenb0 |= INTENB0_BRDYE;
                                    timeout = TimeSpan.FromMicroseconds(125);
                                }
                            }
                        }
                        if (ru.HostPipe == 9) {
                            ru.HostFrmSeq = 5; /* 1ms�Ɏ��܂��Ă���΁A4�̌J��Ԃ� */
                            ru.HostPipe = 1;
                        }
                        else {
                            ru.HostPipe++;
                        }
                    }
                    else {
                        ru.HostFrmSeq = 5;
                        ru.HostPipe = 1;
                    }
                    break;
                default:
                    // Idle
                    ru.HostFrmSeq = 0;
                    ru.HostPipe = 1;
                    timeout = TimeSpan.FromMilliseconds(1);
                    break;
            }
        } while (timeout == TimeSpan.Zero);

        if (ru.CtrSeq != ctrseq) {
            ru.CtrSeq = ctrseq;
            intsts0_int |= INTSTS0_CTRT;
            intenb0 |= INTENB0_CTRE;
        }
        ru.regINTSTS0.Value |= intsts0_int;

        ushort intsts0_now = (ushort)(ru.regINTSTS0.Value &
            INTSTS0_VBINT | INTSTS0_RESM | INTSTS0_SOFR | INTSTS0_DVST | INTSTS0_CTRT | INTSTS0_BEMP | INTSTS0_NRDY | INTSTS0_BRDY);

        if (intsts0_now != intsts0_int && (ru.regINTENB0.Value & intenb0) != 0) {
            ru.sigIrq.SetOutputSignal(false);
            ru.sigIrq.SetOutputSignal(true);
        }

        ru.sofTimer.Schedule(timeout.TotalNanoseconds);
    }

    /*
* BusDevice * RX_UsbFnNew(const char *name)
* Create a new USB function module.
*/
    void RX_UsbNew(string name, IClock clock) {
        USB0 ru = new USB0();
        // ru.bdev.owner = ru;
        // ru.bdev.first_mapping = NULL;
        // ru.bdev.Map = RxUsbFun_Map;
        // ru.bdev.UnMap = RxUsbFun_Unmap;
        // ru.bdev.hw_flags = MEM_FLAG_WRITABLE | MEM_FLAG_READABLE;

        ru.sigIrq = new SignalBit($"{name}.irq", null);
        ru.sofTimer = clock.CreateAlarm(() => timer_event());
    }
}

public static class USB0Offset {
    static readonly uint SYSCFG    = 0x00;
    static readonly uint SYSSTS0   = 0x04;
    static readonly uint DVSTCTR0  = 0x08;
    static readonly uint CFIFO     = 0x14;
    static readonly uint D0FIFO    = 0x18;
    static readonly uint D1FIFO    = 0x1c;
    static readonly uint CFIFOSEL  = 0x20;
    static readonly uint CFIFOCTR  = 0x22;
    static readonly uint D0FIFOSEL = 0x28;
    static readonly uint D0FIFOCTR = 0x2a;
    static readonly uint D1FIFOSEL = 0x2c;
    static readonly uint D1FIFOCTR = 0x2e;
    static readonly uint INTENB0   = 0x30;
    static readonly uint INTENB1   = 0x32;
    static readonly uint BRDYENB   = 0x36;
    static readonly uint NRDYENB   = 0x38;
    static readonly uint BEMPENB   = 0x3a;
    static readonly uint SOFCFG    = 0x3c;
    static readonly uint INTSTS0   = 0x40;
    static readonly uint INTSTS1   = 0x42;
    static readonly uint BRDYSTS   = 0x46;
    static readonly uint NRDYSTS   = 0x48;
    static readonly uint BEMPSTS   = 0x4a;
    static readonly uint FRMNUM    = 0x4c;
    static readonly uint DVCHGR    = 0x4e;
    static readonly uint USBADDR   = 0x50;
    static readonly uint USBREQ    = 0x54;
    static readonly uint USBVAL    = 0x56;
    static readonly uint USBINDX   = 0x58;
    static readonly uint USBLENG   = 0x5a;
    static readonly uint DCPCFG    = 0x5c;
    static readonly uint DCPMAXP   = 0x5e;
    static readonly uint DCPCTR    = 0x60;
    static readonly uint PIPESEL   = 0x64;
    static readonly uint PIPECFG   = 0x68;
    static readonly uint PIPEMAXP  = 0x6c;
    static readonly uint PIPEPERI  = 0x6e;
    static readonly uint PIPE1CTR  = 0x70;
    static readonly uint PIPE2CTR  = 0x72;
    static readonly uint PIPE3CTR  = 0x74;
    static readonly uint PIPE4CTR  = 0x76;
    static readonly uint PIPE5CTR  = 0x78;
    static readonly uint PIPE6CTR  = 0x7a;
    static readonly uint PIPE7CTR  = 0x7c;
    static readonly uint PIPE8CTR  = 0x7e;
    static readonly uint PIPE9CTR  = 0x80;
    static readonly uint PIPE1TRE  = 0x90;
    static readonly uint PIPE1TRN  = 0x92;
    static readonly uint PIPE2TRE  = 0x94;
    static readonly uint PIPE2TRN  = 0x96;
    static readonly uint PIPE3TRE  = 0x98;
    static readonly uint PIPE3TRN  = 0x9a;
    static readonly uint PIPE4TRE  = 0x9c;
    static readonly uint PIPE4TRN  = 0x9e;
    static readonly uint PIPE5TRE  = 0xa0;
    static readonly uint PIPE5TRN  = 0xa2;
    static readonly uint DEVADD0   = 0xd0;
    static readonly uint DEVADD1   = 0xd2;
    static readonly uint DEVADD2   = 0xd4;
    static readonly uint DEVADD3   = 0xd6;
    static readonly uint DEVADD4   = 0xd8;
    static readonly uint DEVADD5   = 0xda;
}
// #define REG_DPUSR0R              0x000a0400
// #define REG_DPUSR1R              0x000a0404

public class USB0Mapping {
    public IReadOnlyList<Register32MappingInfo> Mapping { get; }
    public USB0 Instance { get; }

    public USB0Mapping(IReadOnlyList<Register32MappingInfo> mapping, USB0 instance) {
        Mapping = mapping;
        Instance = instance;
    }
}

public class USB0RX64MMapping {
    public USB0Mapping Mapping { get; }

    public USB0RX64MMapping() {
        var obj = new USB0();
    }
}
