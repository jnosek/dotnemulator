# CLAUDE.md

Guidance for Claude Code (and humans) working in this repository.

## Purpose

**Dotnemulator** is a .NET emulator platform for small 8-bit computer systems. The goals are:

- **Shareable platform**: hardware building blocks (registers, buses, wires, memory, instruction tables) live in a shared `Abstraction` library so that several 8-bit systems can be built on the same foundation.
- **Highly object-oriented model of the hardware**: every significant physical part of the machine (CPU, registers, address/data/port buses, interrupt lines, the memory PLA, ROM/RAM banks, individual CPU instructions) is its own object. Components talk to each other the way real chips do, by driving values onto buses and wires and reacting to signal changes, not by calling into each other's internals.
- **Accuracy you can check**: per-instruction unit tests plus the Klaus Dormann 6502 functional test ROM confirm CPU behavior.

The first (and so far only) platform is the **Commodore 64** (MOS 6510 CPU).

## Repository layout

```
Dotnemulator.sln
src/
  Abstraction/                      # Dotnemulator.Abstraction: shared, platform-agnostic building blocks
    SlimEventSource*.cs, IEventSink.cs   # lightweight signal/event plumbing
    Hardware/                       # Register, Bus, Wire, MemorySegment
    Operations/                     # IInstruction, InstructionSet, IRead/IWrite/IReadWrite
  Platforms/
    Commodore64/                    # Dotnemulator.Platforms.Commodore64 (exe)
      Architecture/                 # chips & CPU internals: MOS6510Cpu, StatusRegister, MemoryPla, MemoryMap, ...
        Instruction/                # one class per 6502/6510 instruction (LDA.cs, JSR.cs, ...)
      Roms/                         # BASIC, KERNAL, CHAR ROMs (embedded resources)
      Emulator.cs, EmulatorBuilder.cs   # composition root + fluent builder
      IExecutionStrategy.cs, RuntimeExecutionStrategy.cs, DebugExecutionStrategy.cs
    Commodore64.Test/               # MSTest unit tests, one file per instruction under Instructions/
    Commodore64.FunctionalTest/     # runs 6502_functional_test.bin end to end
```

New platforms go under `src/Platforms/<SystemName>/` with matching `<SystemName>.Test` projects, and should reference `Abstraction`.

## Architecture

### Abstraction layer (`src/Abstraction`)

Platform-agnostic primitives that model digital hardware:

| Type | Role |
|---|---|
| `SlimEventSource` / `SlimEventSource<T>` | Minimal pub/sub. `Subscribe` registers a handler. With one subscriber, `Trigger`/`Update` points straight at it (no fan-out cost), so signal propagation stays cheap in the hot loop. |
| `Bus` | N-bit (1–32) value holder that is a `SlimEventSource`. `Drive(value)` puts a masked value on the bus **without** notifying. `Trigger()` notifies subscribers. `Read()` / `ReadBit()` sample it. |
| `Wire` | Single-bit signal (`SlimEventSource<bool>`). `Place(bool)` sets it and notifies, e.g. IRQ/NMI lines. |
| `Register` | N-bit masked storage with `Value`, `SetFlag`/`GetFlag`, `Increment`/`Decrement`/`Advance` (post-increment). Overflow wraps like real hardware. Can be subclassed (see `StatusRegister`). |
| `MemorySegment` | Byte array mapped at a starting address. `Read`/`Write` take **absolute** addresses (range-checked in DEBUG only), `Load(Stream)` fills it from a ROM file, and `GetHash()` gives a SHA-256 for ROM checks. |
| `IInstruction` / `InstructionSet` | An instruction exposes its `OpCode` and `Execute(int instruction)`. `InstructionSet` is a fixed-size opcode → instruction table that throws on duplicate opcodes. |

### Signal-driven wiring (core design idea)

Components are connected only through shared `Bus`/`Wire` instances, which are created by the composition root (`Emulator`). For the C64:

```
            AddressBus(16) ──┬──────────────► MemoryPla ──MemoryBankBus(5)──► MemoryMap
 MOS6510Cpu ─ DataBus(8) ────┼──────────────────────────────────────────────► MemoryMap
            PortBus(8) ──────┴──────────────► MemoryPla
            IRQ / NMI Wires ◄── (future peripherals: CIA, VIC-II, ...)
```

- **CPU read**: drive the address onto `AddressBus` and `Trigger()` it. `MemoryPla.Evaluate` drives the bank selection onto `MemoryBankBus`, and `MemoryMap.ReadHandler` drives the byte onto `DataBus`. The CPU then calls `DataBus.Read()`.
- **CPU write**: drive and trigger `AddressBus`, then drive and trigger `DataBus`. `MemoryMap.WriteHandler` stores the byte in the selected bank. Writes to ROM areas go to the RAM underneath.
- **6510 on-chip I/O port**: addresses `$0000`/`$0001` are handled inside the CPU (`Address0`/`Address1` registers). Writing `$0001` drives `PortBus`, which makes the PLA re-evaluate banking (LORAM/HIRAM/CHAREN → BASIC/KERNAL/CHAR/IO/RAM via the `MEMORY_CONFIGS` table).
- **Interrupts**: `Wire.Place(true)` on `InterruptRequest`/`NonMaskableInterrupt` invokes the CPU handler, which pushes PC/P and jumps through the vector.

Subscription order matters: subscribers run synchronously in the order they subscribed. For example, the PLA must update `MemoryBankBus` before `MemoryMap` reacts to the same `AddressBus` trigger. `Emulator` builds components in an order that guarantees this, so keep it in mind when adding components.

### CPU (`Architecture/MOS6510Cpu.cs`)

- Registers are `Register`/`StatusRegister` objects: `PC`(16), `A`, `X`, `Y`, `_sp`(8, exposed as `SP`), `P`.
- `Step()` does fetch → decode (`instructionSet[opcode]`) → execute and returns an `InstructionResult(Address, Instruction, Operand)`.
- Helpers: `Read`, `Write`, `ReadNextByte`, `ReadNextWord` (little-endian), `StackPush`/`StackPop` (page `$01`, wrapping SP).
- `Initialize(pc?)` sets the default port value (`$07`) and loads PC from the reset vector `$FFFC` (or uses the value passed in, for tests).
- `StatusRegister` always forces the Unused bit on and the Break bit off when `Value` is set. B is only set in the copy pushed by `BRK`/`PHP`.

### Instructions (`Architecture/Instruction/`)

Each mnemonic is its own class, and opcodes are **composed** from a base opcode and an addressing-mode bit pattern, the same way the 6502 encodes them:

- `ImpliedInstruction`: no operand (e.g. `CLC`, `TAX`, `NOP`, `RTS`).
- `RelativeInstruction`: branches (`BNE`, `BEQ`, ...).
- `AddressInstruction`: base class for memory-operand instructions. It has an operand decoder for each addressing mode (`GetZeroPageXOperand`, `GetIndirectIndexedOperand`, ...) and the abstract `Execute(int instruction, int address)`.
  - `AccumulatorInstruction`: the 6502 "group 1" encoding (`ORA AND EOR ADC STA LDA CMP SBC`).
  - `XAddressInstruction` / `YAddressInstruction`: groups 2 and 0 (shifts, `INC`/`DEC`, `LDX`/`STX`, `LDY`/`STY`, `CPX`/`CPY`, `BIT`).
  - Accumulator-mode shift variants are separate classes (`ASLA`, `LSRA`, `ROLA`, `RORA`).
- Every address-mode instruction declares `public const int BASE_OP_CODE`, a `public static readonly int[] AddressModes`, and a **private** `(MOS6510Cpu, int addressMode)` constructor. `AddressInstruction.Build<T>(cpu)` uses reflection to create one instance per supported mode. `OpCode = BASE_OP_CODE | addressMode`.
- `MOS6510InstructionSet` registers everything in a 256-entry `InstructionSet`.

To add an instruction: create the class following the pattern above, register it in `MOS6510InstructionSet`, and add `Commodore64.Test/Instructions/<MNEMONIC>_Test.cs`.

### Emulator composition and execution

- `Emulator` is the composition root. It owns the buses and constructs `MOS6510Cpu`, `MemoryPla` and `MemoryMap`, plus an `IExecutionStrategy`.
- `EmulatorBuilder` is the fluent setup API: `WithRoms()`, `WithImage(byte[])` (full 64 KB image), and internal test helpers `WithTestKernel(byte[])`, `WithRam(...)`, `WithMemoryMap(...)`, `WithExecutionStrategy(...)`, `WithUnitTestMode(startAddress?)`.
- `IExecutionStrategy` separates *how* the CPU is run from the machine itself:
  - `RuntimeExecutionStrategy`: reset, then `Step()` forever.
  - `DebugExecutionStrategy`: start/end address, stop on `NOP` (`$EA`), jump-loop detection (throws `JumpLoopException` with a rolling instruction log), breakpoint callbacks, and memory-watch callbacks.
- Internals are exposed to the test assemblies with `InternalsVisibleTo` in `Program.cs`. Most hardware types are `internal` on purpose.

## Build & test

Requires the .NET 10 SDK (`net10.0`). Tests use MSTest 4.

```bash
dotnet build Dotnemulator.sln
```

```bash
dotnet test src/Platforms/Commodore64.Test
```

```bash
dotnet test src/Platforms/Commodore64.FunctionalTest
```

```bash
dotnet run --project src/Platforms/Commodore64
```

### Test conventions

Unit tests build a tiny machine, run it until a `NOP` is reached, and assert on CPU state:

```csharp
var emulator = new EmulatorBuilder()
    .WithUnitTestMode()                       // DebugExecutionStrategy, stop on NOP
    .WithTestKernel([ LDA.BASE_OP_CODE | AccumulatorInstruction.Immediate, 0x02, 0xEA ])
    .WithRam([(0x03, 0x00)])                  // optional RAM preload
    .Build();
emulator.Start();
Assert.AreEqual(0x02, emulator.Cpu.A.Value);
```

`WithTestKernel` writes the program at `$E000` and points the reset vector there (and the IRQ vector at `$0100`). The functional test loads the full 6502 test image and runs from `$0400` to the success address.

## Conventions & guidelines

- **Model hardware, not shortcuts.** Prefer adding a component that talks over a `Bus`/`Wire` to reaching into another component's state. Generic primitives belong in `Abstraction`; platform-specific chips belong in `Platforms/<System>/Architecture`.
- **Mask values to their width.** `Register` and `Bus` mask automatically, so wraparound (SP, PC, zero-page indexing `& 0xFF`, 16-bit address `& 0xFFFF`) must match real silicon.
- **Keep the hot path allocation-free.** `SlimEventSource` exists to avoid delegate fan-out overhead. Avoid LINQ, boxing or allocations inside `Step()`, bus handlers and instruction `Execute` methods. Use `[Conditional("DEBUG")]` / `Debug.Assert` for checks.
- **Document hardware quirks** with XML doc comments (`<summary>`, `<remarks>`), as the existing code does (e.g. the 6510 port at `$00/$01`, B-flag behavior).
- File-scoped namespaces follow the folder structure: `Dotnemulator.Abstraction.*`, `Dotnemulator.Platforms.Commodore64.*`. Nullable and implicit usings are enabled.
- Instruction cycle counts (`Cycles` arrays) are recorded but **not yet used for timing**.

## Current status / roadmap hints

- 6510 legal opcode set is implemented and passes the 6502 functional test (decimal mode and cycle accuracy are areas to verify).
- `MemoryMap._io` is a plain RAM-backed placeholder. VIC-II, SID, CIA 1/2 and cartridge lines (`MemoryPla.Game`/`ExRom`, currently unused) are not modelled yet.
- `Emulator.Stop()` / `Reset()` are stubs. There is no video/audio/input front end yet.
