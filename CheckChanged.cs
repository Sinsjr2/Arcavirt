public class HalRegU8  {
    public byte Read() {
        return 0;
    }
}


// 指定した値が変化しているかを判定します。
class CheckChanged1 {
    HalRegU8[] regs;
    byte[] prevRegs;
    byte[] masks;

    public CheckChanged1(HalRegU8[] regs, byte[] prevRegs, byte[] masks) {
        this.regs = regs;
        this.prevRegs = prevRegs;
        this.masks = masks;
    }

    public bool Update() {
        bool isChanged = false;
        var regs = this.regs;
        var prevRegs = this.prevRegs;
        var masks = this.masks;
        for (int i = 0; i < regs.Length; i++) {
            var current = regs[i].Read();
            ref var prev = ref prevRegs[i];
            var mask = masks[i];
            isChanged = isChanged || (((current ^ prev) & mask) != 0);
            prev = current;
        }
        return isChanged;
    }
}

class CheckChanged2 {
    HalRegU8[] regs;
    uint[] prevRegs;
    uint[] masks;

    public CheckChanged2(HalRegU8[] regs, uint[] prevRegs, uint[] masks) {
        this.regs = regs;
        this.prevRegs = prevRegs;
        this.masks = masks;
    }

    public bool Update() {
        bool isChanged = false;
        var regs = this.regs;
        var prevRegs = this.prevRegs;
        var masks = this.masks;
        int i = 0;
        int j = 0;
        for (; i < regs.Length / 4; j++) {
            uint current = (uint)(regs[i++].Read() |
                                  regs[i++].Read() << 8 |
                                  regs[i++].Read() << 16 |
                                  regs[i++].Read() << 24);
            ref var prev = ref prevRegs[j];
            var mask = masks[j];
            isChanged = isChanged || (((current ^ prev) & mask) != 0);
            prev = current;
        }

        if (i < regs.Length) {
            uint current = regs[i++].Read();
            for (int k = 8; i < regs.Length; i++, k+=8) {
                current = current | ((uint)regs[i].Read() << k);
            }
            var prev = prevRegs[j];
            var mask = masks[j];
            isChanged = isChanged || (((current ^ prev) & mask) != 0);
            prevRegs[j] = current;
        }

        return isChanged;
    }
}
