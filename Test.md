# Running the tests

All test projects use MSTest 4 on .NET 10. Run the commands from the repository root.

## Prerequisites

- .NET 10 SDK (`dotnet --version` should report `10.x`)

Only regenerating the functional test ROMs needs more than that (see [the last section](#regenerating-the-functional-test-roms)).

## Test projects

| Project | What it tests | Typical run time |
|---|---|---|
| `src/Abstraction.Test` | Shared hardware primitives: `Bus`, `Wire`, `OpenCollectorWire`, `ReadWriteWire`, `Register`, `BitWidth`, `SlimEventSource` | < 1 s |
| `src/Platforms/Commodore64.Test` | C64 hardware and the 6510 CPU: one file per instruction under `Instructions/`, plus CPU, interrupts, 6510 port, memory PLA/map, `StatusRegister`, `DebugExecutionStrategy` | < 1 s |
| `src/Platforms/Commodore64.FunctionalTest` | Klaus Dormann's 6502 functional test ROM, run end to end (`Full` and `FullWithReport`) | ~10 s per test |

## Run everything

```bash
dotnet test Dotnemulator.sln
```

## Unit tests

```bash
dotnet test src/Abstraction.Test
```

```bash
dotnet test src/Platforms/Commodore64.Test
```

Run one test class or one test with `--filter`:

```bash
dotnet test src/Platforms/Commodore64.Test --filter "FullyQualifiedName~Instructions.LDA_Test"
```

```bash
dotnet test src/Abstraction.Test --filter "FullyQualifiedName~Hardware.Bus_Test"
```

```bash
dotnet test src/Platforms/Commodore64.Test --filter "Name=DebugStrategy_MemoryWriteWatch_IgnoresReads"
```

List the tests without running them:

```bash
dotnet test src/Platforms/Commodore64.Test --list-tests
```

## Functional tests

```bash
dotnet test src/Platforms/Commodore64.FunctionalTest
```

There are two tests in `OpCodeTest`, one per build of the ROM (both are embedded resources under `Roms/`):

| Test | ROM | How it reports |
|---|---|---|
| `Full` | `6502_functional_test.bin` (`report = 0`, the upstream default) | Passes when execution reaches the success trap. A failing check ends in a trap loop (`jmp *`), which jump-loop detection turns into a failure listing the last instructions. |
| `FullWithReport` | `6502_functional_test_io.bin` (`report = 1`) | The ROM prints its progress and, on a failure, a register/stack/workspace dump. The test writes that output to the console. |

### Running `FullWithReport` and seeing its output

Use the script:

```bash
tools/run-full-with-report.sh
```

It runs:

```bash
dotnet test src/Platforms/Commodore64.FunctionalTest --filter "FullyQualifiedName~OpCodeTest.FullWithReport" --logger "console;verbosity=detailed"
```

Extra arguments are passed on to `dotnet test`, for example:

```bash
tools/run-full-with-report.sh --configuration Release
```

On Windows (PowerShell), run the `dotnet test` command above directly.

`--logger "console;verbosity=detailed"` matters: the test writes the ROM's output with `Console.WriteLine`, and
`dotnet test` only shows a passing test's console output at detailed verbosity.

A passing run looks like this:

```
  Passed FullWithReport [10 s]
  Standard Output Messages:
 Started testing
 All tests completed, press R to repeat
```

A failing run prints the ROM's dump, both under "Standard Output Messages" and in the failure message:

```
  Failed FullWithReport [211 ms]
  Error Message:
   Assert.Fail failed. Functional test reported an error in test case 22:
Started testing
regs Y  X  A  PS PCLPCH
01F9 04 00 7F 31 92 33 30
000C 7F 80 E1 C1 A0 80 00
0200 22 00 00 C0 80 E1 C1 A0 80
press C to continue or S to skip current test
```

Reading the dump:

- `regs` line: the stack, starting at the address after the stack pointer (`01F9`). The first bytes are the
  registers saved when the error was found, `Y X A PS` (`04 00 7F 31`). Next is the address `jsr report_error`
  pushed, low byte first (`92 33` = `$3392`). The failing check's `jsr` starts 2 bytes earlier, at `$3390`.
- `000C` line: the zero page workspace from `$000C`.
- `0200` line: the data workspace from `$0200`. Its first byte is the failing test case number (`22`).

To find the failing code, search the listing
`src/Platforms/Commodore64.FunctionalTest/Roms/6502_functional_test_io.lst` for that address at the start of a
line (`3390 :`). The instructions just above it are the check that failed. In this example that is
`cmp rINC,x ;test result`.

### In an IDE

In Visual Studio Test Explorer, Rider or VS Code (C# Dev Kit), run `OpCodeTest.FullWithReport` and open the test's
output to see the same text.

## Regenerating the functional test ROMs

The ROMs, their listings and `FunctionalTestSymbols.g.cs` are generated and committed, so you only need this to
change the ROM configuration or the upstream version:

```bash
tools/functional-test-roms/build.sh
```

It needs `git`, `unzip`, `python3` and the 32-bit libraries for the bundled `as65` assembler. See
[`tools/functional-test-roms/README.md`](tools/functional-test-roms/README.md).
