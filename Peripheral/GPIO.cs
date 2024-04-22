using GPIO;

namespace Pheripheral {

    /// <summary>
    /// 入出力の方向を決定します。
    /// </summary>
    public enum GPIODir {
        Input,
        Output,
    }

    public enum GPIORegister {
        /// <summary>
        /// 入出力のどちらとして使用するか決定する
        /// <see>GPIODir</see>
        /// </summary>
        PortDir,

        /// <summary>
        /// 入力値を読み込むためのレジスタ
        /// 書き込んでも値は変わりません。
        /// </summary>
        PortInput,

        /// <summary>
        /// 出力値を書き込むためのレジスタ
        /// </summary>
        PortOutput
    }

    public class GPIO8Bit : IPheripheral {

        class GPIOPin : IGPIO {
            GPIODir dir;
            bool outputValue;
            bool outputIsChanged = false;

            public GPIODir Dir {
                get => dir;
                set {
                    if (dir == value) {
                        return;
                    }
                    dir = value;
                    outputIsChanged = true;
                }
            }
            public bool InputValue = false;
            public bool OutputValue {
                get => outputValue;
                set {
                    if (outputValue == value) {
                        return;
                    }
                    outputValue = value;
                    outputIsChanged = true;
                }
            }


            public bool GPIOValue {
                get => Dir switch {
                    GPIODir.Input => InputValue,
                    GPIODir.Output => OutputValue,
                    _ => throw new ArgumentException($"not supported value {Dir}")
                };
                set {
                    InputValue = value;
                }
            }

            public event Action? OnOutputValueChanged;

            public void Update() {
                if (!outputIsChanged) {
                    return;
                }
                outputIsChanged = false;
                OnOutputValueChanged?.Invoke();
            }
        }
        readonly IReadOnlyList<GPIOPin> pins = Enumerable
            .Range(0, 8).Select(_ => new GPIOPin())
            .ToArray();

        public readonly IReadOnlyList<IGPIO> Pins;

        public GPIO8Bit() {
            pins = Enumerable
                .Range(0, 8).Select(_ => new GPIOPin())
                .ToArray();

            Pins = pins.Select<GPIOPin, IGPIO>(x => x)
                .ToArray();
       }

        public uint ReadUint32(uint offset) {
            var register = (GPIORegister)offset;
            switch (register) {
                case GPIORegister.PortDir:
                    return ((uint)pins[7].Dir << 7) |
                        ((uint)pins[6].Dir << 6) |
                        ((uint)pins[5].Dir << 5) |
                        ((uint)pins[4].Dir << 4) |
                        ((uint)pins[3].Dir << 3) |
                        ((uint)pins[2].Dir << 2) |
                        ((uint)pins[1].Dir << 1) |
                        ((uint)pins[0].Dir << 0);
                case GPIORegister.PortInput:
                    return (pins[7].InputValue ? (1u << 7) : 0u) |
                        (pins[6].InputValue ? (1u << 6) : 0u) |
                        (pins[5].InputValue ? (1u << 5) : 0u) |
                        (pins[4].InputValue ? (1u << 4) : 0u) |
                        (pins[3].InputValue ? (1u << 3) : 0u) |
                        (pins[2].InputValue ? (1u << 2) : 0u) |
                        (pins[1].InputValue ? (1u << 1) : 0u) |
                        (pins[0].InputValue ? (1u << 0) : 0u);
                case GPIORegister.PortOutput:
                    return (pins[7].OutputValue ? (1u << 7) : 0u) |
                        (pins[6].OutputValue ? (1u << 6) : 0u) |
                        (pins[5].OutputValue ? (1u << 5) : 0u) |
                        (pins[4].OutputValue ? (1u << 4) : 0u) |
                        (pins[3].OutputValue ? (1u << 3) : 0u) |
                        (pins[2].OutputValue ? (1u << 2) : 0u) |
                        (pins[1].OutputValue ? (1u << 1) : 0u) |
                        (pins[0].OutputValue ? (1u << 0) : 0u);
                default:
                    return 0;
            }
        }

        public void WriteUint32(uint offset, uint value) {
            var register = (GPIORegister)offset;
            switch (register) {
                case GPIORegister.PortDir:
                    pins[7].Dir = ((value & (1 << 7)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[6].Dir = ((value & (1 << 6)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[5].Dir = ((value & (1 << 5)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[4].Dir = ((value & (1 << 4)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[3].Dir = ((value & (1 << 3)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[2].Dir = ((value & (1 << 2)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[1].Dir = ((value & (1 << 1)) != 0) ? GPIODir.Output : GPIODir.Input;
                    pins[0].Dir = ((value & (1 << 0)) != 0) ? GPIODir.Output : GPIODir.Input;
                    break;
                case GPIORegister.PortInput:
                    pins[7].InputValue = ((value & (1 << 7)) != 0);
                    pins[6].InputValue = ((value & (1 << 6)) != 0);
                    pins[5].InputValue = ((value & (1 << 5)) != 0);
                    pins[4].InputValue = ((value & (1 << 4)) != 0);
                    pins[3].InputValue = ((value & (1 << 3)) != 0);
                    pins[2].InputValue = ((value & (1 << 2)) != 0);
                    pins[1].InputValue = ((value & (1 << 1)) != 0);
                    pins[0].InputValue = ((value & (1 << 0)) != 0);
                    break;
                case GPIORegister.PortOutput:
                    pins[7].OutputValue = ((value & (1 << 7)) != 0);
                    pins[6].OutputValue = ((value & (1 << 6)) != 0);
                    pins[5].OutputValue = ((value & (1 << 5)) != 0);
                    pins[4].OutputValue = ((value & (1 << 4)) != 0);
                    pins[3].OutputValue = ((value & (1 << 3)) != 0);
                    pins[2].OutputValue = ((value & (1 << 2)) != 0);
                    pins[1].OutputValue = ((value & (1 << 1)) != 0);
                    pins[0].OutputValue = ((value & (1 << 0)) != 0);
                    break;
                default:
                    return;
            }
            foreach (var pin in pins) {
                pin.Update();
            }
        }
    }
}
