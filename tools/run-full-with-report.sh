#!/usr/bin/env bash
#
# Runs the Commodore 64 FullWithReport functional test and shows the output the
# 6502 functional test ROM prints ("Started testing", "All tests completed", or
# the register/stack dump of a failing test).
#
# Usage: tools/run-full-with-report.sh [extra dotnet test arguments]
#   e.g. tools/run-full-with-report.sh --configuration Release
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet test "$REPO_ROOT/src/Platforms/Commodore64.FunctionalTest" \
    --filter "FullyQualifiedName~OpCodeTest.FullWithReport" \
    --logger "console;verbosity=detailed" \
    "$@"
