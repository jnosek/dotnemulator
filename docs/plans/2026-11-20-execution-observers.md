# Plan: move debug features onto execution observers

Status: planned (not started). Builds on `IExecutionObserver` and `BootMonitor`
from branch `boot-monitor`.

Goal: split `DebugExecutionStrategy`, which today runs seven separate features
from one loop, into small observers that can be attached to either execution
strategy. Keep the current properties working, so existing tests and the
functional tests don't change.

## Background

`IExecutionObserver` (added for `BootMonitor`) is called before every step and
returns `false` to detach. The runtime strategy then switches to its plain
`Step()` loop, so a detached observer costs nothing.

`DebugExecutionStrategy.Execute` currently does all of this itself:

| Feature | Runs | Uses |
|---|---|---|
| `MaxSteps` | before the step | step count, throws `StepLimitException` |
| `BreakpointCallbacks` | before the step | PC → `Action<MOS6510Cpu>` |
| `IsStopOnNopEnabled` | after the step | `InstructionResult.Instruction` |
| `IsJumpLoopDetectionEnabled` | after the step | last result, throws `JumpLoopException` |
| Rolling instruction log (100 entries) | after the step | `InstructionResult` |
| `EndAddress` | after the step | `InstructionResult.Address`, skips interrupt sequences |
| `MemoryWatchCallbacks` / `MemoryWriteWatchCallbacks` | every bus cycle | `AddressBus` subscription |

## 1. Widen the observer interface

The current interface is too narrow for most of these features:

- **After-step hook.** Stop-on-NOP, `EndAddress`, jump-loop detection and the
  instruction log all need the `InstructionResult` after `Step()`.
- **Stop the run.** An observer can detach but can't end `Execute`. Replace the
  `bool` return with an enum:

  ```csharp
  internal enum ObserverAction { Continue, Detach, Stop }

  internal interface IExecutionObserver
  {
      ObserverAction BeforeStep(MOS6510Cpu cpu) => ObserverAction.Continue;
      ObserverAction AfterStep(MOS6510Cpu cpu, InstructionResult result) => ObserverAction.Continue;
  }
  ```

  Failures (jump loop, step limit) keep throwing exceptions, as they do now.
- **Several observers.** Replace `IExecutionStrategy.Observer` with an ordered
  list (`Observers`) and `EmulatorBuilder.WithExecutionObserver(...)` adds to it.
  Order matters: breakpoints run before the step, and the trace must record a
  step before jump-loop detection compares against it.
- **Runtime loop.** It keeps a watching loop while any observer is attached and
  switches to the plain `Step()` loop once all have detached. `Stop` ends
  `Execute`, which the runtime strategy never did before.
- **`BootMonitor`** moves to the new interface: `BeforeStep` returns `Detach`
  instead of `false`.

## 2. Features that become observers

New observers go in `Diagnostics/`:

| # | Observer | Replaces | Hook | Notes |
|---|---|---|---|---|
| 1 | `BreakpointObserver` | `BreakpointCallbacks` | before | Address → `Action<MOS6510Cpu>`. Optional one-shot breakpoints that remove themselves; detaches when none are left. |
| 2 | `StopAtAddressObserver` | `EndAddress` | after | Must keep ignoring interrupt sequences (covered by `Interrupt_Test.DebugStrategy_DoesNotStopAtEndAddress_OnInterruptSequence`). |
| 3 | `StopOnOpcodeObserver` | `IsStopOnNopEnabled` | after | Any opcode, not just `NOP`. `WithUnitTestMode` attaches it for `$EA`. |
| 4 | `InstructionTraceObserver` | the rolling instruction log | after | Fixed-size ring buffer, allocation-free per step. The jump-loop and step-limit exceptions take their log from it, and a crash report could use it too. |
| 5 | `JumpLoopObserver` | `IsJumpLoopDetectionEnabled` | after | Reads the trace from #4. Optionally also detects short loops of several instructions; the `$FF5E` raster wait was one the current check missed. |
| 6 | `StepLimitObserver` | `MaxSteps` | before | `BootMonitor` can reuse it for its own stall limit. |

After this, `DebugExecutionStrategy` is the runtime loop plus `StartAddress`
plus a list of observers. Its current properties stay as a thin layer that
creates the matching observers, so `OpCodeTest.cs`, the unit tests and
`WithUnitTestMode` keep working unchanged. Later the two strategies could merge
into one class.

## 3. Memory watches: a bus probe instead of a step observer

`MemoryWatchCallbacks` and `MemoryWriteWatchCallbacks` watch bus cycles, not
instructions, so they don't belong in a step observer. They already work as
observers through `AddressBus.Subscribe` and dispose themselves when the run
ends.

- Pull them out into `Diagnostics/MemoryWatch` (`IDisposable`), which subscribes
  to `AddressBus` and calls per-address callbacks for all cycles or writes only.
- It can then be attached to any emulator, including one running the runtime
  strategy, not only the debug one.
- `FunctionalTestConsole` and `TestCaseProgress` attach it directly. The debug
  strategy's two dictionaries create one, as today.
- It must still be subscribed after `MemoryMap` and `VicII`, so the data bus
  holds the value read or written when it runs.

## 4. Test cleanup

- `KernalBoot_Test.BootReachesEachStageInOrder` repeats the stage addresses by
  hand with breakpoints. Replace it with an assertion on
  `BootMonitor.StagesReached`, as `BootMonitor_Test` already does.

## 5. New features this enables (later)

- **Fault detection.** A `FaultObserver` checks, before each step, whether the
  opcode about to run is undefined or a JAM opcode. It reports PC, registers and
  the trace from #4 to the console before the CPU crashes. It needs a
  side-effect-free read through the memory map: `PeekRam` only sees RAM, not
  ROM. The `InstructionSet` null-crash on an undefined opcode in Release builds
  still needs its own fix.
- **`--trace` option.** An observer that logs every instruction for the first N
  steps, or between two addresses, then detaches.

## Not observers

- **`MemoryMap`, `VicII`, `MemoryPla`:** hardware on the bus. They stay on
  `SlimEventSource`, as `CLAUDE.md` says. The VIC-II's raster clock should
  later come from a clock wire, not an observer.
- **`ConsoleScreenRenderer`:** stays on its own timer. Driving it from the CPU
  loop would slow emulation and tie the frame rate to the CPU.

## Order of work

1. Widen the interface: after-step hook, `ObserverAction`, observer list. Move
   `BootMonitor` over.
2. `BreakpointObserver`, `StopAtAddressObserver`, `StopOnOpcodeObserver`, with
   the debug strategy's properties creating them.
3. `InstructionTraceObserver` and `JumpLoopObserver`.
4. `StepLimitObserver` and the `MemoryWatch` probe.
5. `KernalBoot_Test` cleanup.
6. Later: `FaultObserver`, `--trace`.

Each step should leave every test passing without changes to existing tests,
except the `KernalBoot_Test` cleanup in step 5.

## Performance

A list of observers adds a few interface calls per step while any are attached.
That's fine for debugging and tests. The runtime loop switches back to the plain
loop once everything has detached, so a normal run is unaffected after boot.
Observers must not allocate per step (no LINQ; the trace uses a fixed ring
buffer).

## Verification

- [ ] `dotnet build Dotnemulator.sln` with no new warnings
- [ ] `dotnet test src/Abstraction.Test`, `src/Platforms/Commodore64.Test` and
      `src/Platforms/Commodore64.FunctionalTest` pass after each step
- [ ] New unit tests for each observer: breakpoint, stop at address (including
      the interrupt case), stop on opcode, trace window, jump loop, step limit,
      memory watch (all cycles and writes only)
- [ ] `dotnet run --project src/Platforms/Commodore64` still prints the boot log
      and then the screen
- [ ] `CLAUDE.md` updated: observer list, `ObserverAction`, `Diagnostics/`
      contents, `MemoryWatch`
