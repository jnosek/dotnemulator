#Requires -Version 5.1
<#
.SYNOPSIS
    Runs the Commodore 64 FullWithReport functional test and shows the output the 6502 functional test ROM prints.

.DESCRIPTION
    PowerShell version of run-full-with-report.sh. The ROM prints "Started testing" and "All tests completed", or the
    register/stack dump of a failing test. Any arguments are passed on to dotnet test.

    The tests run on Microsoft.Testing.Platform (see global.json). It always shows the output of a failing test;
    --output detailed also shows it for a passing one.

.EXAMPLE
    tools/run-full-with-report.ps1

.EXAMPLE
    tools/run-full-with-report.ps1 --configuration Release
#>
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

Push-Location $repoRoot
try {
    & dotnet test `
        --project src/Platforms/Commodore64.FunctionalTest `
        --filter 'FullyQualifiedName~OpCodeTest.FullWithReport' `
        --output detailed `
        @args
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

exit $exitCode
