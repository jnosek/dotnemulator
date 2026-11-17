# Plan: functional test with I/O report output

Goal: build a second copy of Klaus Dormann's 6502 functional test with its I/O
report channel turned on, and change `Commodore64.FunctionalTest` so the text
the ROM prints ("Started testing", the register/stack dump on failure, "All
tests completed") shows up in the .NET test console.

Source: <https://github.com/Klaus2m5/6502_65C02_functional_tests>, pinned at
commit `7954e2dbb49c469ea286070bf46cdd71aeb29e4b` (2020-01-05).

## 1. What upstream gives us

- `6502_functional_test.a65` has a `report` setting (line 122). `report = 0`
  (the default, and what our current `.bin` uses) ends every failure in a
  `jmp *`/`bne *` trap loop. `report = 1` includes `report.i65`, which:
  - calls `report_init` at `start`, printing `Started testing`;
  - turns every `trap_*` macro into `jsr report_error`. It prints the saved
    registers, stack, zero page workspace and data workspace, then waits in
    `rerr1` for `C` (continue) or `S` (skip test);
  - turns `success` into `jsr report_success`. It prints
    `All tests completed, press R to repeat` and waits in `rsuc1` for `R`.
- All output goes through `rchar` (`sta $F001`) and all input through `rget`
  (`lda $F004`), the Kowalski simulator defaults.
- `as65_142.zip` contains `as65` (32-bit Linux ELF) and `as65.exe` (Windows).

## 2. Generating the ROMs

Add a script, `tools/functional-test-roms/build.sh` (plus a short README):

1. Clone upstream at the pinned commit into a temp directory and unzip
   `as65_142.zip`.
2. Copy `6502_functional_test.a65` and `report.i65` to a build directory. For
   the I/O build, change the config with
   `sed -i 's/^report = 0/report = 1/'`, and fail if the line was not found.
   `report.i65` needs no change (see section 3).
3. Assemble each variant:
   ```bash
   as65 -q -l -m -t -w -h0 -z -o<name>.bin 6502_functional_test.a65
   ```
   `-l -m -w -h0` matches the upstream header, `-t` adds the symbol table to the
   listing, `-z` fills gaps with `$00` instead of `$FF`, and leaving out `-s2`
   writes a raw binary instead of Intel hex.
4. as65 writes the binary starting at the lowest used address (`$000A`, the
   zero page start), so it is 65,526 bytes. Pad it with 10 leading zero bytes
   to a full 64 KB image, which is what `EmulatorBuilder.WithImage` expects.
5. Read the symbol table from the `.lst` and write
   `Commodore64.FunctionalTest/FunctionalTestSymbols.g.cs` with the addresses
   the test needs (`start`, `rsuc1`, `rerr1`, `report_success`, `test_case`,
   and the `jmp *` success address of the `report = 0` build). This replaces
   the hard-coded `0x3469`.
6. Write the outputs to `src/Platforms/Commodore64.FunctionalTest/Roms/`:
   - `6502_functional_test.bin` / `.lst` (`report = 0`, rebuilt from the same
     commit so binary, listing and symbols match)
   - `6502_functional_test_io.bin` / `.lst` (`report = 1`)

Commit the generated `.bin`, `.lst` and `.g.cs` files, so a normal
`dotnet build`/`dotnet test` never needs the assembler. The script is only
for regenerating them.

Prerequisites: the Linux `as65` is a 32-bit binary, so it needs
`libc6:i386` and `libstdc++6:i386`
(`dpkg --add-architecture i386 && apt-get install libc6:i386 libstdc++6:i386`).
On Windows, run `as65.exe` instead (a `build.ps1` wrapper can come later).

Checked in this session:
- The pinned source with `report = 0` assembles, and its success trap is at
  `$3469`, the same as the current test's `EndAddress`.
- Its zero page init data does **not** byte-match the committed `.bin`, so the
  committed image came from a slightly different build. Rebuilding both images
  from one commit removes that ambiguity.
- The `report = 1` build assembles with no errors. Symbols: `report_init $444C`,
  `report_error $445B`, `rerr1 $44CD`, `report_success $4524`, `rsuc1 $4532`,
  `rget $453A`, `rchar $4565`. `success` becomes `jsr report_success` at `$3F46`.

## 3. Capturing output in the emulator

**Recommended: keep upstream's `$F001`/`$F004` and use the existing memory
watch.**

- On the C64 map, `$F001` is in the KERNAL ROM area. Writes go to the RAM
  underneath, so the `sta $F001` in `rchar` is harmless, and every write still
  appears as a bus cycle with the character on `DataBus`.
- `DebugExecutionStrategy.MemoryWatchCallbacks[0xF001]` already sees that
  cycle and is given the data bus value, so no emulator change is needed.
- Neither wait loop (`rget` reading `$F004`) is ever allowed to spin. The test
  stops at `rsuc1` (success) or `rerr1` (failure) first, so there is nothing to
  feed into `rget`.

Small hardening change in the same branch: memory-watch callbacks currently
fire on reads **and** writes, and do not receive the R/W mode. The ROM never
reads `$F001`, so this works today. Still, add the mode to the callback (or add a
separate `MemoryWriteWatchCallbacks`) so the console only records writes.

**Alternative, more faithful to the hardware (later):** change `rchar`/`rget`
to the expansion port I/O1 area (`$DE00`/`$DE01`) and model a small "debug
console" cartridge device that `MemoryMap` routes I/O1 cycles to. That is how
a real C64 peripheral would sit on the bus. It depends on replacing the
placeholder `MemoryMap._io` with real I/O decoding, so it is out of scope for
this branch. It would also let `rget` return `C`, so the test could continue
past failures and report all of them in one run.

## 4. Changes to `Commodore64.FunctionalTest`

1. **`.csproj`**: embed `Roms/6502_functional_test_io.bin`. Do not embed the
   `.lst` files; they are reference only.
2. **`FunctionalTestConsole`** (new, test project): a small sink that
   - takes bytes from the memory watch;
   - collects them into lines (the ROM sends `LF CR`; treat `LF` as end of line
     and ignore `CR`);
   - writes each completed line to `Console.Out` and to
     `TestContext.WriteLine`, and keeps the full text for assertion messages.
3. **New test `FullWithReport`** (keep `Full` as the plain trap-loop run):
   ```csharp
   var console = new FunctionalTestConsole(TestContext);
   var emulator = new EmulatorBuilder()
       .WithImage(LoadRom("6502_functional_test_io.bin"))
       .WithExecutionStrategy(new DebugExecutionStrategy
       {
           StartAddress = FunctionalTestSymbols.Io.Start,
           EndAddress = FunctionalTestSymbols.Io.SuccessWait,          // rsuc1
           IsJumpLoopDetectionEnabled = true,
           MemoryWatchCallbacks = new() { [0xF001] = console.Write },
           BreakpointCallbacks = new()
           {
               [FunctionalTestSymbols.Io.ErrorWait] = _ =>               // rerr1
                   Assert.Fail($"Functional test reported an error:\n{console.Text}")
           }
       })
       .Build();

   emulator.Start();

   StringAssert.Contains(console.Text, "All tests completed");
   ```
   - Leave out the `WithRam([(0x0200, 0x2A)])` "skip test 0 and 1" preload;
     with `report = 1` every test runs from the start. If the skip is still
     wanted, take the address from `test_case` in the symbols file.
   - Keep jump-loop detection. In report mode the traps are gone, but a stray
     `jmp *` or `bne *` should still fail fast instead of hanging.
   - Use `test_case` (`$0200`) in the failure message to name the failing
     test.
4. **`Full`**: switch `EndAddress` to the generated symbol and use the rebuilt
   `report = 0` image.

Seeing the output: MSTest records `Console` output per test. To print it in
the terminal, run:

```bash
dotnet test src/Platforms/Commodore64.FunctionalTest --logger "console;verbosity=detailed"
```

`TestContext.WriteLine` also puts it in IDE test output and `.trx` files.

## 5. Prototype result

A throwaway test (not committed) loaded the `report = 1` image (padded to
64 KB), watched `$F001`, stopped at `rsuc1` and broke on `rerr1`. It passed in
about 10 s and printed:

```
Started testing
All tests completed, press R to repeat
```

So the approach works on today's emulator without any emulator changes.

## 6. Work breakdown (commits on this branch)

1. `tools/functional-test-roms/build.sh` + README (pinned commit, as65 flags,
   padding, symbol export).
2. Regenerated `6502_functional_test.bin`/`.lst`, new
   `6502_functional_test_io.bin`/`.lst`, `FunctionalTestSymbols.g.cs`.
3. Memory-watch R/W mode (`DebugExecutionStrategy`), plus a unit test in
   `Commodore64.Test`.
4. `FunctionalTestConsole` + `FullWithReport`; update `Full` to use the
   symbols.
5. CLAUDE.md: describe the two ROM variants, the regeneration script and how
   to see the console output.

## Open questions

- Keep both `Full` (trap loops) and `FullWithReport`, or replace `Full`? Each
  run takes about 10 s. Recommendation: keep both for now. The trap version is
  the canonical upstream configuration.
- Should the I/O1 debug-console device (section 3, alternative) be its own
  follow-up branch once real I/O decoding exists?
