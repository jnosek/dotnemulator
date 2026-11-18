#!/usr/bin/env bash
#
# Rebuilds the Klaus Dormann 6502 functional test ROMs used by
# src/Platforms/Commodore64.FunctionalTest from upstream source:
#
#   Roms/6502_functional_test.bin / .lst     report = 0 (trap loops, upstream default)
#   Roms/6502_functional_test_io.bin / .lst  report = 1 (report.i65 I/O channel)
#   FunctionalTestSymbols.g.cs               addresses the tests need, read from the listings
#
# Requirements: git, unzip, python3, and a way to run as65 (the Linux binary in
# as65_142.zip is 32-bit: install libc6:i386 and libstdc++6:i386), or set AS65 to
# another as65 executable.
#
# Usage: tools/functional-test-roms/build.sh
set -euo pipefail

UPSTREAM_URL="https://github.com/Klaus2m5/6502_65C02_functional_tests.git"
UPSTREAM_COMMIT="7954e2dbb49c469ea286070bf46cdd71aeb29e4b"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
TEST_PROJECT="$REPO_ROOT/src/Platforms/Commodore64.FunctionalTest"
ROM_DIR="$TEST_PROJECT/Roms"

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

echo "Fetching upstream $UPSTREAM_COMMIT"
git -c advice.detachedHead=false init -q "$WORK/upstream"
git -C "$WORK/upstream" fetch -q --depth 1 "$UPSTREAM_URL" "$UPSTREAM_COMMIT"
git -C "$WORK/upstream" -c advice.detachedHead=false checkout -q FETCH_HEAD

if [[ -z "${AS65:-}" ]]; then
    unzip -q "$WORK/upstream/as65_142.zip" as65 -d "$WORK/as65"
    chmod +x "$WORK/as65/as65"
    AS65="$WORK/as65/as65"
fi

# assemble <name> <report value>
assemble() {
    local name="$1" report="$2"
    local dir="$WORK/build-$name"
    mkdir -p "$dir"
    cp "$WORK/upstream/6502_functional_test.a65" "$WORK/upstream/report.i65" "$dir/"

    # the configuration block sets "report = 0"; make sure exactly that line is changed
    grep -q '^report = 0$' "$dir/6502_functional_test.a65" ||
        { echo "error: 'report = 0' not found in 6502_functional_test.a65" >&2; exit 1; }
    sed -i "s/^report = 0\$/report = $report/" "$dir/6502_functional_test.a65"

    echo "Assembling $name (report = $report)"
    # -l pass 2 listing, -m expand macros, -t symbol table, -w wide, -h0 no page headers,
    # -z fill gaps with $00, no -s/-s2 so the output is a raw binary
    (cd "$dir" && "$AS65" -q -l -m -t -w -h0 -z "-o$name.bin" 6502_functional_test.a65 > /dev/null)
}

assemble 6502_functional_test 0
assemble 6502_functional_test_io 1

python3 -I "$SCRIPT_DIR/postprocess.py" \
    --commit "$UPSTREAM_COMMIT" \
    --trap "$WORK/build-6502_functional_test" \
    --io "$WORK/build-6502_functional_test_io" \
    --roms "$ROM_DIR" \
    --symbols "$TEST_PROJECT/FunctionalTestSymbols.g.cs"
