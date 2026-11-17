#Requires -Version 5.1
<#
.SYNOPSIS
    Rebuilds the Klaus Dormann 6502 functional test ROMs used by src/Platforms/Commodore64.FunctionalTest.

.DESCRIPTION
    PowerShell version of build.sh, producing the same files:

      Roms/6502_functional_test.bin / .lst     report = 0 (trap loops, upstream default)
      Roms/6502_functional_test_io.bin / .lst  report = 1 (report.i65 I/O channel)
      FunctionalTestSymbols.g.cs               addresses the tests need, read from the listings

    Requirements: git and Python 3 (python3, python or the py launcher). On Windows the as65.exe bundled with the
    upstream repository is used. On Linux the bundled 32-bit as65 binary is used (it needs libc6:i386 and
    libstdc++6:i386). Set the AS65 environment variable to use another as65 executable (required on macOS).

.EXAMPLE
    tools/functional-test-roms/build.ps1
#>
$ErrorActionPreference = 'Stop'

$UpstreamUrl = 'https://github.com/Klaus2m5/6502_65C02_functional_tests.git'
$UpstreamCommit = '7954e2dbb49c469ea286070bf46cdd71aeb29e4b'

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$testProject = Join-Path $repoRoot 'src/Platforms/Commodore64.FunctionalTest'
$romDir = Join-Path $testProject 'Roms'

# $IsWindows only exists in PowerShell 6+; Windows PowerShell 5.1 only runs on Windows
$onWindows = ($PSVersionTable.PSVersion.Major -lt 6) -or $IsWindows

# the .a65 sources are plain ASCII; Latin-1 reads and writes every byte unchanged (and without a BOM)
$latin1 = [System.Text.Encoding]::GetEncoding(28591)

function Invoke-Native {
    param([string] $What, [scriptblock] $Command)
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$What failed with exit code $LASTEXITCODE"
    }
}

# returns the command and its leading arguments, e.g. @('py', '-3')
function Find-Python {
    foreach ($command in 'python3', 'python', 'py') {
        if (-not (Get-Command $command -CommandType Application -ErrorAction SilentlyContinue)) {
            continue
        }

        $candidate = @($command)
        if ($command -eq 'py') {
            $candidate += '-3'
        }
        $extra = @($candidate | Select-Object -Skip 1)

        # also skips the Windows Store "python" stub, which exists but does not run
        try {
            $version = & $command @extra --version
        }
        catch {
            continue
        }
        if ($LASTEXITCODE -eq 0 -and "$version" -match '^Python 3\.') {
            return , $candidate
        }
    }

    throw 'Python 3 not found (tried python3, python and py -3)'
}

$work = Join-Path ([System.IO.Path]::GetTempPath()) ("functional-test-roms-" + [System.Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null

try {
    Write-Host "Fetching upstream $UpstreamCommit"
    $upstream = Join-Path $work 'upstream'
    Invoke-Native 'git init' { git -c advice.detachedHead=false init -q $upstream }
    Invoke-Native 'git fetch' { git -C $upstream fetch -q --depth 1 $UpstreamUrl $UpstreamCommit }
    Invoke-Native 'git checkout' { git -C $upstream -c advice.detachedHead=false checkout -q FETCH_HEAD }

    $as65 = $env:AS65
    if (-not $as65) {
        $as65Dir = Join-Path $work 'as65'
        Expand-Archive -Path (Join-Path $upstream 'as65_142.zip') -DestinationPath $as65Dir

        if ($onWindows) {
            $as65 = Join-Path $as65Dir 'as65.exe'
        }
        else {
            $as65 = Join-Path $as65Dir 'as65'
            Invoke-Native 'chmod' { chmod +x $as65 }
        }
    }

    foreach ($variant in @(
            @{ Name = '6502_functional_test'; Report = 0 },
            @{ Name = '6502_functional_test_io'; Report = 1 })) {
        $name = $variant.Name
        $report = $variant.Report
        $dir = Join-Path $work "build-$name"
        New-Item -ItemType Directory -Path $dir | Out-Null
        Copy-Item (Join-Path $upstream '6502_functional_test.a65'), (Join-Path $upstream 'report.i65') $dir

        # the configuration block sets "report = 0"; make sure exactly that line is changed
        $source = Join-Path $dir '6502_functional_test.a65'
        $text = [System.IO.File]::ReadAllText($source, $latin1)
        $pattern = '(?m)^report = 0(?=\r?$)'
        if (-not [regex]::IsMatch($text, $pattern)) {
            throw "'report = 0' not found in 6502_functional_test.a65"
        }
        $text = [regex]::Replace($text, $pattern, "report = $report")
        [System.IO.File]::WriteAllText($source, $text, $latin1)

        Write-Host "Assembling $name (report = $report)"
        # -l pass 2 listing, -m expand macros, -t symbol table, -w wide, -h0 no page headers,
        # -z fill gaps with $00, no -s/-s2 so the output is a raw binary
        Push-Location $dir
        try {
            Invoke-Native "as65 ($name)" { & $as65 -q -l -m -t -w -h0 -z "-o$name.bin" 6502_functional_test.a65 | Out-Null }
        }
        finally {
            Pop-Location
        }
    }

    $python = Find-Python
    $pythonArgs = @($python | Select-Object -Skip 1) + @(
        '-I', (Join-Path $PSScriptRoot 'postprocess.py'),
        '--commit', $UpstreamCommit,
        '--trap', (Join-Path $work 'build-6502_functional_test'),
        '--io', (Join-Path $work 'build-6502_functional_test_io'),
        '--roms', $romDir,
        '--symbols', (Join-Path $testProject 'FunctionalTestSymbols.g.cs'))
    Invoke-Native 'postprocess.py' { & $python[0] @pythonArgs }
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}
