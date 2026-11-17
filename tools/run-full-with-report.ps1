#Requires -Version 5.1
<#
.SYNOPSIS
    Runs the Commodore 64 FullWithReport functional test and shows the output the 6502 functional test ROM prints.

.DESCRIPTION
    PowerShell version of run-full-with-report.sh. The ROM prints "Started testing" and "All tests completed", or the
    register/stack dump of a failing test. Any arguments are passed on to dotnet test.

    -tl:off turns off the MSBuild terminal logger, which .NET 9+ uses in interactive terminals and which hides the
    test output (it only shows when output is piped).

.EXAMPLE
    tools/run-full-with-report.ps1

.EXAMPLE
    tools/run-full-with-report.ps1 --configuration Release
#>
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

& dotnet test (Join-Path $repoRoot 'src/Platforms/Commodore64.FunctionalTest') `
    --filter 'FullyQualifiedName~OpCodeTest.FullWithReport' `
    --logger 'console;verbosity=detailed' `
    -tl:off `
    @args

exit $LASTEXITCODE
