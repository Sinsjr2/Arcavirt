using System;
using System.Collections.Generic;
using System.Linq;
using CPU;
using Gdb;
using GdbStub;
using Microsoft.Extensions.Logging;
using Peripheral.Renesas;
using Pheripheral;
using Renesas;
using RX;

namespace Lancher;

public class Program {
    public static void Main(string[] args) {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var looper = new Looper(factory.CreateLogger("ChannelLooper"));
        var memory = new RAM32Bit("sram", 0x80000);
        var rom = new RAM32Bit("rom", 4194304);
        var ofs = new RAM32Bit("ofs", 256);
        uint ramBeginAddress = 0;
        var ramEndAddress = ramBeginAddress + memory.MemorySize;
        var busManager = new BusManager();
        busManager.AddRangedAddressMapping(ramBeginAddress, ramEndAddress, memory);
        busManager.AddRangedAddressMapping(0x120000, 256, ofs);
        var memoryMappings = new List<Register32MappingInfo>();

        var iceDebug = new IceDebug();
        memoryMappings.AddRange(
            IceDebugMapping.CreateMapping(iceDebug)
            .Select(x => x with { Offset = x.Offset + 0x8_4080}));

        var clockMapping = new CLOCKRX64MMapping();
        // HOCO 安定化完了をセット
        clockMapping.Clock.OSCOVFSR.Value = 1 << 3;
        memoryMappings.AddRange(clockMapping.Mapping.Mapping.Select(x => x with { Offset = x.Offset + 0x0008_0000 }));
        foreach (var mapping in memoryMappings) {
            busManager.AddMapping(mapping.Offset, mapping.Register);
        }
        uint romEndAddress = 0xFFFFFFFF;
        uint romBeginAddress = romEndAddress + 1 - rom.MemorySize;
        busManager.AddRangedAddressMapping(romBeginAddress, romEndAddress, rom);
        var rxCore = new RXv1Core(busManager);
        var instLoop = new InstructionLoop<RXv1Core>(act => looper.RunCallback(act, obj => ((Action?)obj)!()), new RXv1Core(busManager), new Clock.SimulationClock());
        var gdbStub = new GDBTCPServer(looper.RunTask, factory.CreateLogger("GDB"), 3333, new RXv1Stub(instLoop));

        looper.RunTask(null, _ => gdbStub.ConnectionLoop(default));
        looper.Start(default);
    }
}
