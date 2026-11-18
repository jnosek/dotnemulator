# Running the tests

All test projects use MSTest 4 on .NET 10 and run on
[Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro) (MTP),
selected for `dotnet test` by `global.json`. Run the commands from the repository root.

## Prerequisites

- .NET 10 SDK (`dotnet --version` should report `10.x`)
- For running tests in an IDE: one that supports MTP, such as Visual Studio 2022 17.14 or later, Rider, or VS Code
  with C# Dev Kit

Only regenerating the functional test ROMs needs more than that (see [the last section](#regenerating-the-functional-test-roms)).

## Test projects

| Project | What it tests | Typical run time |
|---|---|---|
| `src/Abstraction.Test` | Shared hardware primitives: `Bus`, `Wire`, `OpenCollectorWire`, `ReadWriteWire`, `Register`, `BitWidth`, `SlimEventSource` | < 1 s |
| `src/Platforms/Commodore64.Test` | C64 hardware and the 6510 CPU: one file per instruction under `Instructions/`, plus CPU, interrupts, 6510 port, memory PLA/map, `StatusRegister`, `DebugExecutionStrategy` | < 1 s |
| `src/Platforms/Commodore64.FunctionalTest` | Klaus Dormann's 6502 functional test ROM, run end to end (`Full` and `FullWithReport`) | ~10 s per test |

## Run everything

```bash
dotnet test
```

With no project given, `dotnet test` runs every test project in `Dotnemulator.sln`. To pick one, give its folder
(as below) or use `--project <folder>`.

MTP options differ from the older VSTest runner: there is no `--logger` (use `--output detailed` for more output,
see below), and `dotnet test --project <folder> --help` lists what is available.

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
| `FullWithReport` | `6502_functional_test_io.bin` (`report = 1`) | The ROM prints its progress and, on a failure, a register/stack/workspace dump. The test writes that output to the console, plus a line for each test case that passes, and checks that all 44 test cases (`00` to `2B`) passed. |

### Running `FullWithReport` and seeing its output

Use the script (bash, or PowerShell on Windows, Linux or macOS):

```bash
tools/run-full-with-report.sh
```

```powershell
tools/run-full-with-report.ps1
```

It runs:

```bash
dotnet test --project src/Platforms/Commodore64.FunctionalTest --filter "FullyQualifiedName~OpCodeTest.FullWithReport" --output detailed
```

The test writes its output with `Console.WriteLine`. MTP always shows a failing test's output; `--output detailed`
also shows it for a passing test.

Extra arguments are passed on to `dotnet test`, for example:

```bash
tools/run-full-with-report.sh --configuration Release
```

```powershell
tools/run-full-with-report.ps1 --configuration Release
```

If PowerShell refuses to run the script because of its execution policy, run it with
`powershell -ExecutionPolicy Bypass -File tools/run-full-with-report.ps1`, or run the `dotnet test` command above
directly.

A passing run looks like this (`Started testing` and `All tests completed` come from the ROM, the
`Test case NN passed` lines from the test):

```
passed FullWithReport (12s 397ms)
  Standard output
    Started testing
    Test case 00 passed
    Test case 01 passed
    ...
    Test case 2B passed
    All tests completed, press R to repeat
...
Test run summary: Passed!
  total: 1
  failed: 0
  succeeded: 1
```

The test case lines come from watching the ROM's current test number at `$0200` (`test_case`). The ROM stores the
next test's number there after each test's checks pass, and `$F0` after the last test, so each new number means the
previous test case passed. To see what a test case covers, search the listing for its next number, e.g.
`lda #test_num` with `a923` for the end of test case `22`.

A failing run (with or without `--output detailed`) shows the failure message, which holds the ROM's dump, then the
test's whole output: the passed test cases followed by the dump:

```
failed FullWithReport (284ms)
  Assert.Fail failed. Functional test reported an error in test case 22:
  Started testing
  regs Y  X  A  PS PCLPCH
  01F9 04 00 7F 31 92 33 30
  000C 7F 80 E1 C1 A0 80 00
  0200 22 00 00 C0 80 E1 C1 A0 80
  press C to continue or S to skip current test
    at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(String message) ...
    ...
  Standard output
    Started testing
    Test case 00 passed
    ...
    Test case 21 passed
    regs Y  X  A  PS PCLPCH
    01F9 04 00 7F 31 92 33 30
    000C 7F 80 E1 C1 A0 80 00
    0200 22 00 00 C0 80 E1 C1 A0 80
    press C to continue or S to skip current test
...
Test run summary: Failed!
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

```powershell
tools/functional-test-roms/build.ps1
```

`build.sh` needs `git`, `unzip`, `python3` and the 32-bit libraries for the bundled Linux `as65` assembler.
`build.ps1` needs `git` and Python 3. On Windows it uses the bundled `as65.exe`, so nothing else is needed. See
[`tools/functional-test-roms/README.md`](tools/functional-test-roms/README.md).
