# Architecture

This document explains **how Dotnemulator is put together and why**: the hardware model, how components talk to each other, how the CPU and instructions are built, and the planned front end (video output, debugger UI). It is the design reference.

[`CLAUDE.md`](CLAUDE.md) is the working guide: repository layout, build and test commands, coding rules, and step-by-step recipes. When the two overlap, `CLAUDE.md` states the rule and this document explains it.

Sections marked **(planned)** describe intended designs that are not implemented yet. Change them freely as the implementation teaches us more.

---

## 1. Design principles

1. **Model the hardware, not shortcuts.** Every significant physical part of the machine is an object: CPU, registers, buses, wires, the memory PLA, ROM/RAM banks, and each CPU instruction. Components talk the way chips do, by driving values onto buses and wires and reacting to signal changes, not by calling into each other's internals.
2. **Ownership follows the hardware.** Whatever is inside one chip (registers, latches, decoders) is created by that chip. Whatever is a trace on the board between chips (buses, R/W, IRQ/NMI, port pins, expansion-port lines, later RESET, clock, BA/AEC) is created by the composition root (`Emulator`) and passed to each chip's constructor.
3. **Stateless decoders may be called directly.** Combinational logic with no clock or stored state (such as the memory PLA) can be exposed as an interface and called by the component handling a bus cycle. Anything with state or timing (CPU, CIA, VIC-II, SID, ...) communicates only over buses and wires.
4. **A shared foundation.** Platform-agnostic primitives live in `Abstraction` so several 8-bit systems can be built on them. Platform chips live in `Platforms/<System>/Architecture`.
5. **Fast enough to run in real time.** Signal propagation, `Step()`, bus handlers and instruction `Execute` methods do not allocate. This is why `SlimEventSource` exists instead of .NET events.
6. **Accuracy you can check.** Per-instruction unit tests and the Klaus Dormann 6502 functional test ROM confirm CPU behaviour. Video output will get the same treatment (see [§7.7](#77-testing-video-output-planned)).
7. **The core is headless.** The emulator core never references a UI, windowing, graphics or audio library. Front ends observe it through narrow output contracts (frames, samples, state snapshots) and drive it through input contracts (keys, joysticks, run/stop/step).

---

## 2. Abstraction layer (`src/Abstraction`)

Platform-agnostic primitives that model digital hardware.

| Type | Role |
|---|---|
| `SlimEventSource` / `SlimEventSource<T>` | Minimal pub/sub. `Subscribe` registers a handler and returns an `IDisposable`; disposing it removes the handler (setup time only). The protected `Notify()` / `Notify(T)` calls one delegate: a shared no-op with no subscribers, the subscriber itself with one, or a loop over a subscriber array with several, so signal propagation stays cheap in the hot loop. Only subclasses (`Bus`, the wires) can notify. |
| `Bus` | N-bit (1–32, size required) value holder that is a `SlimEventSource`. `Strobe(value)` puts a masked value on the bus and notifies subscribers; it is used by the component **starting** a bus cycle. `Drive(value)` puts it on **without** notifying; it is used by components **responding** to a cycle. `Read()` / `ReadBit()` sample it. |
| `Wire` | Single-bit signal (`SlimEventSource<bool>`) with one driver. `true` means **asserted**, whatever the pin voltage, so an active-low pin pulled low is `true`. `Strobe(bool)` sets it and notifies **only if the level changed**, so every notification is an edge. `Drive(bool)` sets it without notifying. |
| `OpenCollectorWire` | Shared, wired-OR single-bit line (`SlimEventSource<bool>`), such as IRQ/NMI. Each device calls `RegisterSource()` once and uses its `Source.Assert()` / `Release()`. `Value` is `true` while any source asserts it, and subscribers are notified only when that combined level changes. Up to 32 sources. |
| `ReadWriteWire` / `ReadWriteMode` | Read/write select line (`SlimEventSource<ReadWriteMode>`). `Drive(ReadWriteMode.Read/Write)`, `Mode`, `IsRead` / `IsWrite`. `Strobe` notifies only on a change, like `Wire`. |
| `BitWidth` | Shared size check (1–32 bits) and mask calculation used by `Bus` and `Register`. |
| `Register` | N-bit (1–32, size required) masked storage with `Value`, `SetFlag` / `GetFlag` (the flag must fit the width; DEBUG assert), `Increment` / `Decrement` (return the new value) and `Advance` (returns the old value, then increments). Overflow wraps like real hardware. No member is virtual, so access is a direct call. Registers with fixed bits or fewer operations are separate classes (see `StatusRegister`). |
| `MemorySegment` | Byte array mapped at a starting address. `Read` / `Write` take **absolute** addresses (range-checked in DEBUG only), `Load(Stream)` fills it from a ROM file, and `GetHash()` returns a SHA-256 for ROM checks. |
| `IInstruction` / `InstructionSet` | An instruction exposes its `OpCode` and `Execute(int instruction)`. `InstructionSet` is a fixed-size opcode → instruction table that throws on duplicate opcodes. |

### Why `Strobe` vs. `Drive`

A bus cycle has exactly one initiator. If every component that put a value on a bus also notified, a responder driving the data bus would re-trigger the cycle it is answering. The rule is: **the initiator strobes, responders drive.** One `Strobe` causes one round of synchronous handlers, and the initiator then samples the result with `Read()`.

---

## 3. Signal-driven wiring (Commodore 64)

Components are connected through shared `Bus` / `Wire` instances created by `Emulator`. Stateless decode logic is the one exception (principle 3).

```
            AddressBus(16) ──────────────────────────────► MemoryMap ──SelectBank()──► MemoryPla
 MOS6510Cpu ─ DataBus(8) ────────────────────────────────► MemoryMap                       ▲
            ReadWrite (ReadWriteWire) ───────────────────► MemoryMap                       │
            PortBus(6) ────────────────────────────────────────────────────────────────────┘
            Game / ExRom Wires (expansion port) ───────────────────────────────────────────► MemoryPla
            IRQ / NMI (OpenCollectorWire) ◄── sources registered by peripherals (future: CIA, VIC-II, RESTORE, ...)
```

### 3.1 Bus cycles

Every memory access is one **bus cycle**, started by a single `AddressBus.Strobe()`:

- **CPU read**: `ReadWrite.Drive(Read)`, then `AddressBus.Strobe(address)`. `MemoryMap` asks the PLA for the bank (`SelectBank(address)`) and drives the byte onto `DataBus`. The CPU then calls `DataBus.Read()`.
- **CPU write**: `ReadWrite.Drive(Write)`, `DataBus.Drive(value)`, then `AddressBus.Strobe(address)`. `MemoryMap` stores the byte already on `DataBus` in the selected bank. Writes to ROM areas go to the RAM underneath. No read happens during a write.

### 3.2 The 6510 on-chip I/O port

Addresses `$0000` (data direction, `Address0`) and `$0001` (data, `Address1`) are registers **inside the CPU**. `PortBus` is the six pins P0–P5.

- Writing either register strobes `PortBus` with the pin levels. Output pins (DDR bit 1) carry the data bit. Input pins read high, because they are pulled up on the C64.
- Reading `$0001` gives the data register for output bits and the pins for input bits.
- Both registers are cleared at reset, so all pins start as inputs and the default configuration is selected.
- The PLA selects the matching row of its block table (LORAM/HIRAM/CHAREN → BASIC/KERNAL/CHAR/IO/RAM via the `MEMORY_CONFIGS` table).
- A port access does **not** start a memory cycle.

### 3.3 Interrupts

- `Emulator` creates `InterruptRequest` and `NonMaskableInterrupt` as `OpenCollectorWire`s and passes them to the CPU. Each device that can interrupt registers its own source.
- Interrupts are only taken between instructions, at the start of `Step()`.
- **IRQ is level-triggered**: it is serviced while any source asserts it and the I flag is clear.
- **NMI is edge-triggered**: the CPU latches each released-to-asserted change and services it at the next boundary. While one source holds NMI, another source asserting it gives no new edge, as on the C64.
- NMI wins if both are pending.
- Servicing pushes PC and P (B clear), sets the I flag and jumps through the vector, as its own `Step()`. That step's `InstructionResult.Interrupt` is `InterruptKind.Irq` or `InterruptKind.Nmi`.

### 3.4 Subscriber ordering

Subscribers run synchronously in the order they subscribed. Do not build components that depend on that order. If a component needs another's output during the same cycle, and that output is stateless decode logic (like `MemoryPla` implementing `IMemoryBankSelector`), pass the decoder in through the constructor and call it.

---

## 4. CPU (`Architecture/MOS6510Cpu.cs`)

- Registers are `Register` objects: `PC` (16 bits) and `A`, `X`, `Y`, `_sp` (8 bits, exposed as `SP`). `P` is a `StatusRegister`, which does **not** derive from `Register`: it only exposes `Value` and the named flag properties, as on the real chip.
- `StatusRegister` always forces the Unused bit on and the Break bit off when `Value` is set, and has no other way to write those bits. B is only set in the copy pushed by `BRK` / `PHP`.
- `Step()` first checks for a pending NMI or IRQ and, if one is taken, runs the interrupt sequence as the whole step. Otherwise it does fetch → decode (`instructionSet[opcode]`) → execute. It returns an `InstructionResult(Address, Instruction, Operand, Interrupt)`:
  - For an executed instruction, `Interrupt` is `InterruptKind.None`.
  - For an interrupt sequence, `Interrupt` is `Irq` or `Nmi`, `Instruction` is `$00` (the real 6502 forces `BRK` during the sequence), and `Address` is the return address, whose instruction has not run yet.
- Helpers: `Read`, `Write`, `ReadNextByte`, `ReadNextWord` (little-endian), `StackPush` / `StackPop` (page `$01`, wrapping SP).
- `Initialize(pc?)` clears the port registers (all pins inputs, reading high) and loads PC from the reset vector `$FFFC`, or uses the value passed in, for tests.
- The CPU keeps **references** to the board's buses and wires (`internal` fields, passed into its constructor). It does not own them.

---

## 5. Instructions (`Architecture/Instruction/`)

Each mnemonic is its own class. Opcodes are **composed** from a base opcode and an addressing-mode bit pattern, the same way the 6502 encodes them:

- `ImpliedInstruction`: no operand (such as `CLC`, `TAX`, `NOP`, `RTS`).
- `RelativeInstruction`: branches (`BNE`, `BEQ`, ...).
- `AddressInstruction`: base class for memory-operand instructions. It has an operand decoder for each addressing mode (`GetZeroPageXOperand`, `GetIndirectIndexedOperand`, ...) and the abstract `Execute(int instruction, int address)`.
  - `AccumulatorInstruction`: the 6502 "group 1" encoding (`ORA AND EOR ADC STA LDA CMP SBC`).
  - `XAddressInstruction` / `YAddressInstruction`: groups 2 and 0 (shifts, `INC` / `DEC`, `LDX` / `STX`, `LDY` / `STY`, `CPX` / `CPY`, `BIT`).
  - Accumulator-mode shift variants are separate classes (`ASLA`, `LSRA`, `ROLA`, `RORA`).
- Every address-mode instruction declares `public const int BASE_OP_CODE`, a `public static readonly int[] AddressModes`, and a **private** `(MOS6510Cpu, int addressMode)` constructor. `AddressInstruction.Build<T>(cpu)` uses reflection, at setup time only, to create one instance per supported mode. `OpCode = BASE_OP_CODE | addressMode`.
- `MOS6510InstructionSet` registers everything in a 256-entry `InstructionSet`.
- Instruction cycle counts (`Cycles` arrays) are recorded but **not yet used for timing**.

---

## 6. Composition and execution

### 6.1 `Emulator` (composition root)

`Emulator` owns every board-level bus and wire (address, data, port, `ReadWrite`, IRQ/NMI, cartridge `Game` / `ExRom`). It constructs `MOS6510Cpu`, `MemoryPla` and `MemoryMap` (which takes the PLA as its `IMemoryBankSelector`), plus an `IExecutionStrategy`.

`EmulatorBuilder` is the fluent setup API: `WithRoms()`, `WithImage(byte[])` (a full 64 KB image), and internal test helpers `WithTestKernel(byte[])`, `WithRam(...)`, `WithMemoryMap(...)`, `WithExecutionStrategy(...)` and `WithUnitTestMode(startAddress?)`.

### 6.2 Execution strategies

`IExecutionStrategy` separates *how* the CPU is run from the machine itself:

- `RuntimeExecutionStrategy`: reset, then `Step()` forever.
- `DebugExecutionStrategy`: start/end address, stop on `NOP` (`$EA`), jump-loop detection (throws `JumpLoopException` with a rolling instruction log), breakpoint callbacks, and memory-watch callbacks.

The planned debugger UI (§7) should be built on this seam rather than adding UI hooks to the CPU.

---

## 7. Front end: video, debugger UI and host (planned)

### 7.1 Technology choices

| Concern | Library | Why |
|---|---|---|
| Window, rendering, input, audio, timing | **SDL3**, through the `ppy.SDL3-CS` bindings | A streaming texture is the simplest way to show a framebuffer. One library also covers SID audio (audio streams), keyboard and joystick (CIA), and vsync. Native binaries ship in the NuGet package for Windows, Linux and macOS. |
| Debugger panels and controls | **Dear ImGui** (`Hexa.NET.ImGui` or `ImGui.NET`) | Immediate-mode UI suits a debugger: little code per panel, redrawn every frame from current state. It is the usual choice for emulator debuggers. |
| Custom 2D diagrams | **SkiaSharp** (CPU raster) | Draws the live architecture view (chips, buses, wire levels, register values). It renders into a texture shown inside an ImGui window. |
| Headless frame tests | SkiaSharp or ImageSharp | Encode frames as PNG for golden-image tests (§7.7). |

Alternatives considered:
- **Silk.NET**: binds SDL2 in the 2.x line and needs an OpenGL or Vulkan pipeline just to show a framebuffer. Reconsider it only if GPU shaders become a requirement.
- **Avalonia**: the better choice for a polished end-user application, and it uses Skia, so diagram code would carry over. It owns the main loop, which makes it heavier than needed for a developer-focused debugger.
- **WinForms, WPF, MAUI**: no Linux support.

> **Check before implementing:** Hexa.NET's ready-made ImGui SDL3 backends are built against `Hexa.NET.SDL3`, not `ppy.SDL3-CS`. Choose one of these:
> (a) use `ppy.SDL3-CS` and write a small ImGui renderer on `SDL_RenderGeometry` (a C# port of ImGui's `imgui_impl_sdlrenderer3`, a few hundred lines), or
> (b) use `Hexa.NET.SDL3` with the Hexa backends in place of `ppy.SDL3-CS`.
> Record the decision here.

### 7.2 Project layout

```
src/
  Frontend/                         # Dotnemulator.Frontend (library): platform-agnostic host pieces
    Host/                           # SDL3 window, main loop, emulator thread, frame pacing
    Video/                          # framebuffer → texture upload, palettes, scaling
    Debugger/                       # ImGui renderer and shared panels (memory, disassembly, registers)
    Diagrams/                       # SkiaSharp drawing helpers for architecture views
  Platforms/
    Commodore64/                    # emulator core: stays headless, no front-end packages
    Commodore64.Frontend/           # exe: C64-specific panels and diagrams, wires the core to Dotnemulator.Frontend
```

- The core projects (`Abstraction`, `Platforms/*`) must not reference SDL, ImGui or SkiaSharp.
- `Commodore64` is currently an `Exe` with a placeholder `Program.Main`. When the front end arrives it should become a class library, and `Commodore64.Frontend` becomes the executable.
- Most core types are `internal`. The front end needs either a small **public** surface (preferred: the contracts in §7.3 and §7.5) or an `InternalsVisibleTo` entry. Prefer the public contracts, so front ends cannot reach into hardware state.

### 7.3 Video output contract

The VIC-II is a stateful chip, so it follows the same rules as everything else. It shares buses with the CPU (via BA/AEC), asserts IRQ through its own `OpenCollectorWire` source, and is constructed by `Emulator` with references to board traces.

Its *video output* is modelled as the chip's output pins feeding a display:

- **Pixels are palette indices, not colours.** The VIC-II writes 4-bit colour indices (0–15) into a `byte[]` framebuffer, one byte per pixel. Colour conversion happens in the front end, so palette choice (Pepto, Colodore, ...) is a display setting and not chip behaviour.
- **The framebuffer covers the visible raster, borders included.** PAL is about 403 × 284 visible pixels (504 × 312 for the full raster including blanking). NTSC differs. Record the exact geometry with the VIC-II implementation.
- **End of frame is a signal.** At the end of the last raster line, the VIC-II hands the finished frame to an output (sketch below) and continues into the next frame in a second buffer.

```csharp
// Sketch only: names and placement to be settled when the VIC-II is implemented.
public interface IVideoSink
{
    /// Called on the emulator thread when a frame is complete. The span is valid only during the call.
    void FrameCompleted(ReadOnlySpan<byte> paletteIndices, int width, int height);
}
```

- The core ships a no-op sink (for headless runs and CPU tests) and a capturing sink (for frame tests).
- `IVideoSink` is generic enough to live in `Abstraction` (platform-agnostic) once a second platform needs it. Until then it can live in the C64 project.
- Hot-path rule: writing a pixel is a byte store into a preallocated array. No allocation, no colour conversion, no virtual call per pixel.

### 7.4 Threading and frame pacing

```
 Emulator thread                                   Main / UI thread (SDL requires it)
 ───────────────                                   ──────────────────────────────────
 Step() … VIC-II fills back buffer                  poll SDL events → input queue
 end of frame → IVideoSink.FrameCompleted ──swap──► newest completed frame
 copy debug snapshot at safe point ────────swap──► newest snapshot
 pace to 50.12 Hz (PAL) / 59.83 Hz (NTSC)           upload frame texture, draw ImGui + Skia, present (vsync)
```

- SDL window and event calls stay on the **main thread** (required on macOS, and safest everywhere).
- The emulator runs on its **own thread**. Frames move through a **triple buffer** (emulator writes one, UI reads one, one spare), so neither side waits for the other and the UI always shows the newest complete frame.
- **Pacing**: the C64's refresh rate is not the monitor's refresh rate, so the emulator paces itself to the C64 frame rate (later, to audio buffer fill, which is the usual way to avoid sound glitches). The UI presents at monitor vsync and shows the latest frame. A frame may be shown twice or skipped. That is acceptable for a first version.
- **Input** from SDL is queued by the UI thread and applied by the emulator thread between steps (later, written into the CIA's keyboard matrix as wire and port levels).

### 7.5 Debugger UI

- **Data flow is one-way per frame.** Panels read from a **state snapshot**: an immutable copy of registers, flags, selected memory pages, bus and wire levels, the last `InstructionResult`s, and later VIC/CIA/SID registers. The emulator thread captures it at an instruction boundary. Panels never read live hardware objects while the emulator is running.
- **Commands go the other way.** Run, pause, step, step-over, reset, set or clear breakpoint, and memory edits are queued as commands and applied by the emulator thread between steps. Breakpoints and watches build on `DebugExecutionStrategy`'s existing callbacks.
- **Snapshot cost**: only capture while a debugger window is open, and no more than once per displayed frame. A paused emulator is captured once.
- Planned panels: CPU registers and flags, disassembly around PC, memory hex view, stack, breakpoints and watches, memory map/PLA configuration, interrupt lines, and later VIC-II, CIA and SID registers.

### 7.6 Architecture diagram view (SkiaSharp)

A live picture of the machine following §3: chips as boxes, buses and wires as lines, asserted lines highlighted, current bus values labelled.

- **Render path**: lock an SDL streaming texture → `SKSurface.Create(info, pixels, pitch)` → draw → unlock. The texture is then shown inside an ImGui window with `ImGui.Image`.
- **Pixel formats must match**: `SKColorType.Rgba8888` ↔ `SDL_PIXELFORMAT_RGBA32` (or `Bgra8888` ↔ `BGRA32`). SDL's `*32` names describe memory byte order, which is what Skia uses.
- **Redraw only when needed**: when the snapshot changes, and at most once per displayed frame.
- Use the **CPU raster** backend. Skia's GPU backends (GL, Vulkan) cannot share a window with SDL's 2D renderer, and the diagram is too small to need them.
- **Linux**: reference `SkiaSharp.NativeAssets.Linux.NoDependencies` (or `SkiaSharp.NativeAssets.Linux` if system fontconfig is acceptable).
- **Fonts**: ship one font file and load it with `SKTypeface.FromStream`, so text looks the same on every OS.
- **High-DPI**: size textures in real pixels (`SDL_GetWindowPixelDensity`) and scale the canvas to match.
- Skia surfaces are **single-threaded**: draw only on the UI thread, from the snapshot.

### 7.7 Testing video output (planned)

- Frame tests run headless with a capturing `IVideoSink`: boot or load a program, run N frames, convert the indices to RGBA with a fixed palette, and compare with a reference PNG stored in the test project.
- Compare palette indices (exact match), not encoded images, so palette or encoder changes do not break tests. PNGs are for humans to inspect a failure.
- Test ROMs for VIC-II timing (such as the VICE test suite programs) can be added the way the 6502 functional test was.

---

## 8. Roadmap

- The 6510 legal opcode set is implemented and passes the 6502 functional test. Decimal mode and cycle accuracy still need checking.
- `MemoryMap._io` is a plain RAM-backed placeholder. VIC-II, SID, CIA 1/2 and the cartridge lines (`Emulator.Game` / `ExRom`, read by the PLA but currently unused, default `false` = not asserted) are not modelled yet.
- `Emulator.Stop()` / `Reset()` are stubs.
- Front end (§7): the SDL3 host and frame display, then the ImGui debugger, then the Skia architecture view.
- Cycle timing: instruction `Cycles` are recorded but unused. VIC-II emulation (badlines, BA/AEC cycle stealing, raster IRQ timing) will need the CPU to advance in cycles, not whole instructions. This is the largest architectural change ahead, and it should be designed before VIC-II work starts.
