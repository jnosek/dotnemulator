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
global.json                         # selects Microsoft.Testing.Platform as the dotnet test runner
src/
  Abstraction/                      # Dotnemulator.Abstraction: shared, platform-agnostic building blocks
    SlimEventSource*.cs, IEventSource.cs # lightweight signal/event plumbing (namespace Dotnemulator.Abstraction)
    Hardware/                       # Register, Bus, Wire, OpenCollectorWire, ReadWriteWire, BitWidth, MemorySegment
    Operations/                     # IInstruction, InstructionSet, IRead/IWrite/IReadWrite
  Abstraction.Test/                 # MSTest unit tests for Abstraction, mirroring its folders (Hardware/, ...)
  Platforms/
    Commodore64/                    # Dotnemulator.Platforms.Commodore64 (exe)
      Architecture/                 # chips & CPU internals: MOS6510Cpu, StatusRegister, MemoryPla, MemoryMap, VicII, ...
        Instruction/                # one class per 6502/6510 instruction (LDA.cs, JSR.cs, ...)
      Diagnostics/                  # BootMonitor: logs the KERNAL boot stages, then detaches
      Display/                      # TextScreen (screen RAM -> text), ConsoleScreenRenderer (console pseudo-screen)
      Roms/                         # BASIC, KERNAL, CHAR ROMs (embedded resources)
      Emulator.cs, EmulatorBuilder.cs   # composition root + fluent builder
      IExecutionStrategy.cs, RuntimeExecutionStrategy.cs, DebugExecutionStrategy.cs, IExecutionObserver.cs
    Commodore64.Test/               # MSTest unit tests for C64-specific hardware only; one file per instruction under Instructions/
    Commodore64.FunctionalTest/     # runs the 6502 functional test ROMs end to end (Roms/, FunctionalTestSymbols.g.cs)
tools/
  functional-test-roms/             # build.sh / build.ps1: regenerate the functional test ROMs with upstream's as65
  run-full-with-report.sh / .ps1    # runs the FullWithReport functional test and shows the ROM's output
Test.md                             # how to run the unit and functional tests
```

New platforms go under `src/Platforms/<SystemName>/` with matching `<SystemName>.Test` projects, and should reference `Abstraction`.

Tests live next to what they test: tests of `Abstraction` types (`Bus`, `Wire`, `Register`, `SlimEventSource`, ...) go in `Abstraction.Test`, and a platform's test project only holds tests of that platform's chips, wiring and behaviour (including platform classes built on Abstraction types, such as `StatusRegister`).

## Architecture

### Abstraction layer (`src/Abstraction`)

Platform-agnostic primitives that model digital hardware:

| Type | Role |
|---|---|
| `SlimEventSource` / `SlimEventSource<T>` | Minimal pub/sub. `Subscribe` registers a handler and returns an `IDisposable`; disposing it removes the handler (setup-time only). The protected `Notify()` / `Notify(T)` calls one delegate: a shared no-op with no subscribers, the subscriber itself with one, or a loop over a subscriber array with several, so signal propagation stays cheap in the hot loop. Only subclasses (`Bus`, the wires) can notify. |
| `Bus` | N-bit (1–32, size required) value holder that is a `SlimEventSource`. `Strobe(value)` puts a masked value on the bus and notifies subscribers (used by the component **starting** a bus cycle). `Drive(value)` puts it on **without** notifying (used by components **responding** to a cycle). `Read()` / `ReadBit()` sample it. |
| `Wire` | Single-bit signal (`SlimEventSource<bool>`) with one driver. `true` = **asserted**, whatever the pin voltage (so an active-low pin pulled low is `true`). `Strobe(bool)` sets it and notifies **only if the level changed** (every notification is an edge). `Drive(bool)` sets it without notifying. |
| `OpenCollectorWire` | Shared, wired-OR single-bit line (`SlimEventSource<bool>`), e.g. IRQ/NMI. Each device calls `RegisterSource()` once and uses its `Source.Assert()`/`Release()`; `Value` is `true` while any source asserts it, and subscribers are notified only when that combined level changes. Up to 32 sources. |
| `ReadWriteWire` / `ReadWriteMode` | Read/write select line (`SlimEventSource<ReadWriteMode>`). `Drive(ReadWriteMode.Read/Write)`, `Mode`, `IsRead`/`IsWrite`. `Strobe` notifies only on a change, like `Wire`. |
| `BitWidth` | Shared size check (1–32 bits) and mask calculation used by `Bus` and `Register`. |
| `Register` | N-bit (1–32, size required) masked storage with `Value`, `SetFlag`/`GetFlag` (flag must fit the width; DEBUG assert), `Increment`/`Decrement` (return the new value) and `Advance` (returns the old value, then increments). Overflow wraps like real hardware. No member is virtual, so access is a direct call; registers with fixed bits or a narrower set of operations are separate classes (see `StatusRegister`). |
| `MemorySegment` | Byte array mapped at a starting address. `Read`/`Write` take **absolute** addresses (range-checked in DEBUG only), `Load(Stream)` fills it from a ROM file, and `GetHash()` gives a SHA-256 for ROM checks. |
| `IInstruction` / `InstructionSet` | An instruction exposes its `OpCode` and `Execute(int instruction)`. `InstructionSet` is a fixed-size opcode → instruction table that throws on duplicate opcodes. |

### Signal-driven wiring (core design idea)

Components are connected through shared `Bus`/`Wire` instances, which are created by the composition root (`Emulator`). Stateless decode logic is the one exception: it is queried directly (see below). For the C64:

```
            AddressBus(16) ──────────────────────────────► MemoryMap ──SelectBank()──► MemoryPla
 MOS6510Cpu ─ DataBus(8) ────────────────────────────────► MemoryMap                       ▲
            ReadWrite (ReadWriteWire) ───────────────────► MemoryMap                       │
            AddressBus / DataBus / ReadWrite ────────────► VicII ──────SelectBank()────────┤
            PortBus(6) ────────────────────────────────────────────────────────────────────┘
            Game / ExRom Wires (expansion port) ───────────────────────────────────────────► MemoryPla
            IRQ / NMI (OpenCollectorWire) ◄── sources registered by peripherals (future: CIA, VIC-II, RESTORE, ...)
```

Every memory access is one **bus cycle**, started by a single `AddressBus.Strobe()`:

- **CPU read**: `ReadWrite.Drive(Read)`, then `AddressBus.Strobe(address)`. `MemoryMap` asks the PLA for the bank (`SelectBank(address)`) and drives the byte onto `DataBus`. The CPU then calls `DataBus.Read()`.
- **CPU write**: `ReadWrite.Drive(Write)`, `DataBus.Drive(value)`, then `AddressBus.Strobe(address)`. `MemoryMap` stores the byte already on `DataBus` in the selected bank. Writes to ROM areas go to the RAM underneath. No read happens during a write.
- **VIC-II registers**: while the PLA selects the IO bank, `$D000–$D3FF` is answered by `VicII`, not `MemoryMap` (which leaves that range alone, so exactly one component responds). The 64 registers repeat every 64 bytes. `VicII` is a stub: `$D012` and bit 7 of `$D011` read the raster line and write the raster compare; every other register only stores its value. With no clock wire yet, every bus cycle counts as one φ2 cycle (PAL: 63 cycles a line, 312 lines). The moving raster is what lets the KERNAL's CINT get past its `LDA $D012 / BNE` wait at `$FF5E`. No raster IRQ, sprites or video.
- **6510 on-chip I/O port**: addresses `$0000` (data direction, `Address0`) and `$0001` (data, `Address1`) are registers inside the CPU. `PortBus` is the six pins P0–P5. Writing either register strobes `PortBus` with the pin levels: output pins (DDR bit 1) carry the data bit, input pins read high (pulled up on the C64). Reading `$0001` gives the data register for output bits and the pins for input bits. Both registers are cleared at reset, so all pins start as inputs and the default configuration is selected. The PLA selects the matching row of its block table (LORAM/HIRAM/CHAREN → BASIC/KERNAL/CHAR/IO/RAM via the `MEMORY_CONFIGS` table). This does not start a memory cycle.
- **Interrupts**: `Emulator` creates `InterruptRequest` and `NonMaskableInterrupt` as `OpenCollectorWire`s and passes them to the CPU; each device that can interrupt registers its own source. Interrupts are only taken between instructions, at the start of `Step()`. IRQ is level-triggered: it is serviced while any source asserts it and the I flag is clear. NMI is edge-triggered: the CPU latches each released-to-asserted change and services it at the next boundary; while one source holds NMI, another asserting it gives no new edge (as on the C64). NMI wins if both are pending. Servicing pushes PC and P (B clear), sets the I flag and jumps through the vector, as its own `Step()` whose `InstructionResult.Interrupt` is `InterruptKind.Irq`/`Nmi`.

Subscribers run synchronously in the order they subscribed. Avoid components that depend on that order: if a component needs another's output during the same cycle, and that output is stateless decode logic (like `MemoryPla` implementing `IMemoryBankSelector`), pass the decoder in through the constructor and call it.

### CPU (`Architecture/MOS6510Cpu.cs`)

- Registers are `Register` objects: `PC`(16), `A`, `X`, `Y`, `_sp`(8, exposed as `SP`). `P` is a `StatusRegister`, which does **not** derive from `Register`: it only exposes `Value` and the named flag properties, as on the real chip.
- `Step()` first checks for a pending NMI or IRQ and, if one is taken, runs the interrupt sequence as the whole step. Otherwise it does fetch → decode (`instructionSet[opcode]`) → execute. It returns an `InstructionResult(Address, Instruction, Operand, Interrupt)`. For an executed instruction `Interrupt` is `InterruptKind.None`; for an interrupt sequence it is `Irq`/`Nmi`, `Instruction` is `$00` (the real 6502 forces `BRK` during the sequence) and `Address` is the return address, whose instruction has not run yet.
- Helpers: `Read`, `Write`, `ReadNextByte`, `ReadNextWord` (little-endian), `StackPush`/`StackPop` (page `$01`, wrapping SP).
- `Initialize(pc?)` clears the port registers (all pins inputs, reading high) and loads PC from the reset vector `$FFFC` (or uses the value passed in, for tests).
- The CPU keeps **references** to the board's buses and wires (`internal` fields, passed into its constructor); it does not own them.
- `StatusRegister` always forces the Unused bit on and the Break bit off when `Value` is set, and has no other way to write those bits. B is only set in the copy pushed by `BRK`/`PHP`.

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

- `Emulator` is the composition root. It owns every board-level bus and wire (address, data, port, `ReadWrite`, IRQ/NMI, cartridge `Game`/`ExRom`), and constructs `MOS6510Cpu`, `MemoryPla`, `MemoryMap` and `VicII` (both take the PLA as their `IMemoryBankSelector`), plus an `IExecutionStrategy`.
- `EmulatorBuilder` is the fluent setup API: `WithRoms()`, `WithImage(byte[])` (full 64 KB image), and internal test helpers `WithTestKernel(byte[])`, `WithRam(...)`, `WithMemoryMap(...)`, `WithExecutionStrategy(...)`, `WithUnitTestMode(startAddress?)`.
- `IExecutionStrategy` separates *how* the CPU is run from the machine itself:
  - `RuntimeExecutionStrategy`: reset, then `Step()` forever.
  - `DebugExecutionStrategy`: start/end address, stop on `NOP` (`$EA`), jump-loop detection (throws `JumpLoopException` with a rolling instruction log), a `MaxSteps` hang guard (throws `StepLimitException`), breakpoint callbacks, and memory-watch callbacks (`MemoryWatchCallbacks` for every cycle, `MemoryWriteWatchCallbacks` for writes only).
- Both strategies take an optional `IExecutionObserver` (`IExecutionStrategy.Observer`, or `EmulatorBuilder.WithExecutionObserver(...)`), called with the CPU before every step until it returns `false`. The runtime strategy then switches to its plain `Step()` loop, so a detached observer costs nothing.
- `Diagnostics/BootMonitor` is such an observer. It recognises the rev 3 KERNAL's reset stages by PC (reset, cartridge `CBM80` check, IOINIT, RAMTAS, RESTOR, CINT, raster wait, PAL/NTSC, BASIC cold start, banner, keyboard wait at `$E5CD`) and logs one line for each. Results known only after a stage (top of memory, `$02A6`) are read with `PeekRam`. It finishes, calls `Finished` and detaches on boot complete, cartridge autostart, or a stall (`DEFAULT_STEP_LIMIT`, 2,000,000 steps).
- Internals are exposed to the test assemblies with `InternalsVisibleTo` in `Program.cs`. Most hardware types are `internal` on purpose.

## Build & test

Requires the .NET 10 SDK (`net10.0`). Tests use MSTest 4 on Microsoft.Testing.Platform: `global.json` selects it for `dotnet test`, and each test project sets `OutputType` `Exe` and `EnableMSTestRunner`. Use MTP options (`--output detailed`, `--filter`, `--list-tests`), not VSTest ones such as `--logger`. `Test.md` has the full guide to running the tests (filters, the functional test output, reading a failure dump).

```bash
dotnet build Dotnemulator.sln
```

```bash
dotnet test src/Abstraction.Test
```

```bash
dotnet test src/Platforms/Commodore64.Test
```

```bash
dotnet test src/Platforms/Commodore64.FunctionalTest
```

To see the functional test ROM's own output (progress, and the register/stack dump on a failure):

```bash
dotnet test --project src/Platforms/Commodore64.FunctionalTest --output detailed
```

```bash
dotnet run --project src/Platforms/Commodore64
```

With no video front end yet, `Program` boots the real ROMs with a `BootMonitor` writing the boot stages to the console. When the boot ends, `ConsoleScreenRenderer` draws the 40×25 text screen (decoded from screen RAM at `$0400` by `TextScreen`) below the log, redrawn in place every 100 ms (by moving the cursor up over the previous frame), with the PC on a status line. Ctrl+C draws a last frame and restores the cursor. With redirected output it prints one frame at exit.

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

`KernalBoot_Test` boots the real ROMs (`WithRoms()`) to the KERNAL keyboard wait loop (`$E5CD`) and checks the banner and `READY.` through `TextScreen`, plus the order of the boot stages. It sets `MaxSteps` so a hang fails quickly; the boot takes about 600,000 steps.

`WithTestKernel` writes the program at `$E000` and points the reset vector there (and the IRQ vector at `$0100`). The functional tests load a full 64 KB 6502 test image and run from `$0400`:

- `Full` runs `6502_functional_test.bin` (`report = 0`, the upstream default) until the success trap. A failure ends in a trap loop, which jump-loop detection turns into a test failure.
- `FullWithReport` runs `6502_functional_test_io.bin` (`report = 1`). The ROM prints through `report.i65`: each character is written to `$F001` (in the KERNAL area, so it lands in RAM) and captured with `DebugExecutionStrategy.MemoryWriteWatchCallbacks` by `FunctionalTestConsole`, which writes it to the console. `TestCaseProgress` watches writes to `test_case` (`$0200`) and prints a line for each test case that passes; the test also checks that every test case passed. The test ends at the success wait loop (`rsuc1`) or fails at the error wait loop (`rerr1`) with the ROM's dump in the message.

The ROMs, their listings and `FunctionalTestSymbols.g.cs` (the addresses above) are generated by `tools/functional-test-roms/build.sh` (or `build.ps1`) from a pinned upstream commit and committed; don't edit them by hand. The listings are the place to look up a failing test's code.

## Conventions & guidelines

- **Model hardware, not shortcuts.** Prefer adding a component that talks over a `Bus`/`Wire` to reaching into another component's state.
- **Ownership follows the hardware.** Anything inside one chip (registers, latches, decoders) is created by that chip. Anything that is a trace on the board between chips (buses, R/W, IRQ/NMI, port pins, expansion-port lines, and later RESET, clock, BA/AEC) is created by `Emulator` and passed to each chip's constructor. Generic primitives belong in `Abstraction`; platform-specific chips belong in `Platforms/<System>/Architecture`.
- **Stateless decoders may be called directly.** Combinational logic with no clock or stored state (address decoders such as the memory PLA) can be exposed as an interface and called by the component handling a bus cycle. Anything with state or timing (CPU, CIA, VIC-II, SID, ...) still communicates over buses and wires.
- **Mask values to their width.** `Register` and `Bus` mask automatically, so wraparound (SP, PC, zero-page indexing `& 0xFF`, 16-bit address `& 0xFFFF`) must match real silicon.
- **Keep the hot path allocation-free.** `SlimEventSource` exists to avoid delegate fan-out overhead. Avoid LINQ, boxing or allocations inside `Step()`, bus handlers and instruction `Execute` methods. Use `[Conditional("DEBUG")]` / `Debug.Assert` for checks.
- **Document hardware quirks** with XML doc comments (`<summary>`, `<remarks>`), as the existing code does (e.g. the 6510 port at `$00/$01`, B-flag behavior).
- File-scoped namespaces follow the folder structure: `Dotnemulator.Abstraction.*`, `Dotnemulator.Platforms.Commodore64.*`. Nullable and implicit usings are enabled.
- Instruction cycle counts (`Cycles` arrays) are recorded but **not yet used for timing**.

## Current status / roadmap hints

- 6510 legal opcode set is implemented and passes the 6502 functional test (decimal mode and cycle accuracy are areas to verify).
- The real KERNAL boots to `READY.` (no keyboard yet). `MemoryMap._io` is a plain RAM-backed placeholder for the I/O area outside the VIC-II stub's range. The full VIC-II, SID, CIA 1/2 and cartridge lines (`Emulator.Game`/`ExRom`, read by the PLA but currently unused, default `false` = not asserted) are not modelled yet.
- `Emulator.Stop()` / `Reset()` are stubs. There is no video/audio/input front end yet, only the console text screen.
