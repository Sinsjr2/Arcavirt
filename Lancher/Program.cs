using System;
using CPU;
using Gdb;
using GdbStub;
using Microsoft.Extensions.Logging;
using Peripheral.Renesas;
using Pheripheral;
using RX;

namespace Lancher;

public class Program {
    public static void Main(string[] args) {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

        var looper = new Looper(factory.CreateLogger("ChannelLooper"));
        var memory = new RAM32Bit("sram", 0x80000);
        var rom = new RAM32Bit("rom", 4194304);
        var ofs = new RAM32Bit("ofs", 256);
        uint ramBeginAddress = 0;
        var ramEndAddress = ramBeginAddress + memory.MemorySize;
        var busManager = new BusManager();
        busManager.AddRangedAddressMapping(ramBeginAddress, ramEndAddress, memory);
        busManager.AddRangedAddressMapping(0x120000, 256, ofs);
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
