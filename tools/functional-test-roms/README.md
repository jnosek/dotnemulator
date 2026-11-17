# Functional test ROMs

`build.sh` rebuilds the ROMs that `src/Platforms/Commodore64.FunctionalTest` runs, from
[Klaus Dormann's 6502 functional tests](https://github.com/Klaus2m5/6502_65C02_functional_tests)
at a pinned commit, using the AS65 assembler bundled in that repository (`as65_142.zip`).

The outputs are committed, so building and testing never need the assembler. Run the script only to change the
ROM configuration or the upstream commit.

| Output (in `src/Platforms/Commodore64.FunctionalTest/`) | Contents |
|---|---|
| `Roms/6502_functional_test.bin` / `.lst` | `report = 0`, the upstream default: failures and success end in `jmp *`-style trap loops |
| `Roms/6502_functional_test_io.bin` / `.lst` | `report = 1`: includes `report.i65`, which prints progress and error dumps through an I/O port |
| `FunctionalTestSymbols.g.cs` | addresses the tests use (start, success/error wait loops, I/O ports), read from the listings |

## How it works

1. Fetches the pinned upstream commit and unzips `as65`.
2. Sets `report` in the configuration block of `6502_functional_test.a65` (the only source change; `report.i65`
   is used unmodified, so output goes to `$F001` and input comes from `$F004`).
3. Assembles with `as65 -q -l -m -t -w -h0 -z -o<name>.bin` (listing with macros and symbol table, raw binary,
   gaps filled with `$00`).
4. `postprocess.py` pads each binary to 64 KB (as65 starts the file at the lowest address used, `$000A`), copies
   the binaries and listings into `Roms/`, and generates `FunctionalTestSymbols.g.cs`.

## Requirements

- `git`, `unzip`, `python3`
- The Linux `as65` is a 32-bit binary. On Debian/Ubuntu:

  ```bash
  sudo dpkg --add-architecture i386 && sudo apt-get update && sudo apt-get install libc6:i386 libstdc++6:i386
  ```

  Alternatively, set `AS65` to another as65 executable (for example `as65.exe` under WSL).
