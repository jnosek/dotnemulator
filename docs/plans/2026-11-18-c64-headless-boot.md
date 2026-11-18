# Plan: C64 headless boot (VIC-II raster stub, boot smoke test, console screen)

Status: planned on branch `headless-boot`.

Goal: get the real KERNAL to boot to `READY.` on its own, and make the screen
visible in the console, so boot problems can be seen without a video front end.

## Background

The KERNAL boot currently stalls in CINT at `$FF5E`/`$FF61`:

```
FF5E  LDA $D012
FF61  BNE $FF5E
```

The rev 3 KERNAL waits there for the VIC-II raster counter to reach line 0 before
it detects PAL or NTSC. The `$D000` I/O area is plain RAM, so `$D012` never
changes. A probe run that forced PC past the loop did finish booting: BASIC cold
start, the `**** COMMODORE 64 BASIC V2 ****` banner, `38911 BASIC BYTES FREE`,
`READY.`, then the keyboard wait loop at `$E5CD`.

Three pieces of work:

1. A minimal VIC-II raster stub, so CINT's wait ends.
2. A boot smoke test that proves the KERNAL reaches `READY.`.
3. A console pseudo-screen that prints screen RAM in place, refreshed
   periodically, when the emulator is run with `dotnet run`.

## 1. VIC-II raster stub

A new `VicII` chip in `Architecture/VicII.cs` owns `$D000–$D3FF` and does only
what the KERNAL needs to boot: a raster counter that moves.

**Wiring**, following the existing bus-cycle model:

- `Emulator` constructs `VicII(AddressBus, DataBus, ReadWrite, Pla)`, alongside
  `MemoryMap`.
- `VicII` subscribes to `AddressBus`. It handles a cycle only when the PLA
  selects the IO bank (`SelectBank`, a stateless decoder called directly) and
  the address is in `$D000–$D3FF`. The 64 registers repeat every 64 bytes in that
  range (`address & 0x3F`).
- In the IO bank, `MemoryMap` ignores `$D000–$D3FF` and leaves it to the VIC-II.
  Exactly one component answers each cycle, so the result doesn't depend on
  subscription order.

**Registers**

| Register | Read | Write |
| --- | --- | --- |
| `$D012` RASTER | raster line, bits 0–7 | raster compare, bits 0–7 |
| `$D011` CR1 bit 7 | raster line, bit 8 | raster compare, bit 8 |
| `$D011` bits 0–6 | stored value | stored |
| all others | stored value | stored |

**Timing.** There is no clock wire yet, so each bus cycle counts as one φ2
cycle (each memory access on a 6510 takes one cycle). PAL timing: 63 cycles per
line and 312 lines, so the counter wraps from 311 to 0. When a clock/φ2 wire is
added, the VIC-II switches to it.

**Out of scope:** raster IRQs and the `$D019` latch, so CINT detects NTSC
(`$02A6 = 0`). Also sprites, video output, and bad-line/BA cycle stealing.

**Tests** (`VicII_Test.cs`):

- the counter advances and wraps at 312
- `$D011` bit 7 reads as bit 8 of the raster line
- writes to `$D012` set the compare value, not the counter
- other registers store their values
- RAM under `$D000` is untouched

## 2. Boot smoke test

`Commodore64.Test/KernalBoot_Test.cs` boots the real ROMs with no patches and
asserts that the screen shows the BASIC banner and `READY.`.

1. Build with `WithRoms()` and a `DebugExecutionStrategy` with
   `EndAddress = 0xE5CD` (the KERNAL's keyboard wait loop, reached after
   `READY.` is printed).
2. Hang guard: a new `DebugExecutionStrategy.MaxSteps` (nullable, off by
   default) throws `StepLimitException` with the PC and the instruction log once
   that many steps have run. The test sets 5,000,000, about 3x the boot. A
   regression then fails fast with a useful message instead of hanging the test
   run.
3. Read screen RAM through the new `TextScreen` helper (section 3) and assert:
    - row 1 contains `**** COMMODORE 64 BASIC V2 ****`
    - row 3 contains `64K RAM SYSTEM  38911 BASIC BYTES FREE`
    - row 5 is `READY.`
4. A second test checks the milestone order through `BreakpointCallbacks`, so a
   failure says which stage was not reached: RESET `$FCE2` → IOINIT `$FDA3` →
   RAMTAS `$FD50` → RESTOR `$FD15` → CINT `$FF5B` → BASIC cold start `$E394`.

`TextScreen` gets its own unit tests for screen-code decoding: `@`, `A–Z`,
digits and punctuation, reverse video (bit 7), and graphics characters.

## 3. Console pseudo-screen

`dotnet run --project src/Platforms/Commodore64` shows the 40×25 text screen in
the terminal, redrawn in place 10 times a second, until Ctrl+C.

**`TextScreen`** (`Display/TextScreen.cs`, internal) is a read-only view of
screen RAM at `$0400–$07E7`. `GetRows()` returns 25 strings of 40 characters,
decoded from screen codes (uppercase/graphics set):

| Screen code | Shown as |
| --- | --- |
| 0 | `@` |
| 1–26 | `A–Z` |
| 27–31 | `[` `£` `]` `↑` `←` |
| 32–63 | same as ASCII (space, digits, punctuation) |
| 64–127 | `▒` (graphics, not decoded yet) |
| +128 (bit 7) | reverse video: same character, inverted with ANSI on the console |

It reads through `MemoryMap.PeekRam`, so it starts no bus cycles. It doesn't
follow `$D018` or the CIA 2 VIC bank yet. The screen is always taken to be at
`$0400`, which is the KERNAL default.

**`ConsoleScreenRenderer`** (`Display/ConsoleScreenRenderer.cs`) is the front
end:

- `Start()` clears the console once, hides the cursor and starts a
  `System.Threading.Timer` (100 ms).
- Each tick moves the cursor home (`Console.SetCursorPosition(0, 0)`) and writes
  a framed 42×27 block, plus one status line (PC and frame count). Each frame
  overwrites the last one, so the output looks like a screen.
- The frame is built in one reused `StringBuilder` and written with a single
  `Console.Write`, to avoid flicker.
- `Dispose()` stops the timer, shows the cursor again and moves below the frame.
  `Program` calls it from `Console.CancelKeyPress` and on exit.
- With redirected output (`Console.IsOutputRedirected`), the cursor can't move,
  so it prints one frame at exit instead.

**Threading.** The CPU loop keeps running on the main thread, and the timer
thread only reads RAM bytes. A torn frame (a row read in the middle of a scroll)
is harmless and corrected on the next tick, so no locks go into the hot path.

**`Program` changes.** Add `.WithRoms()`, since it currently builds with no
ROMs. Then start the renderer and call `emulator.Start()`.

## Files

| File | Change |
| --- | --- |
| `Commodore64/Architecture/VicII.cs` | new: raster stub and register storage |
| `Commodore64/Architecture/MemoryMap.cs` | leave `$D000–$D3FF` in the IO bank to the VIC-II |
| `Commodore64/Emulator.cs` | construct `VicII`, expose it as `internal` |
| `Commodore64/DebugExecutionStrategy.cs`, `StepLimitException.cs` | `MaxSteps` hang guard |
| `Commodore64/Display/TextScreen.cs` | new: screen RAM → text |
| `Commodore64/Display/ConsoleScreenRenderer.cs` | new: in-place console refresh |
| `Commodore64/Program.cs` | `WithRoms()`, start the renderer |
| `Commodore64.Test/VicII_Test.cs`, `TextScreen_Test.cs`, `KernalBoot_Test.cs` | new tests |
| `CLAUDE.md` | document the VIC-II stub, the console screen and `MaxSteps` |

## Verification

- [ ] `dotnet build Dotnemulator.sln` with no new warnings
- [ ] `dotnet test src/Abstraction.Test` and `src/Platforms/Commodore64.Test`
      pass, including the boot smoke test
- [ ] `dotnet test src/Platforms/Commodore64.FunctionalTest` still passes (the
      test image doesn't touch `$D000` through the I/O bank)
- [ ] `dotnet run --project src/Platforms/Commodore64` shows the banner and
      `READY.` redrawn in place, and Ctrl+C restores the cursor

## Not in this change

Keyboard input (CIA 1), cursor blink (needs the CIA 1 timer IRQ), colour RAM,
lowercase character set, following `$D018`, a real clock wire, and a proper
`Emulator.Stop()`.
