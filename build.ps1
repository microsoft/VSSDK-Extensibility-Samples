<#
.Synopsis
    Acquires dependencies and builds all solutions in this repo.
.Parameter Configuration
    The build configuration to build.
#>
[CmdletBinding(SupportsShouldProcess=$true,ConfirmImpact='Medium')]
Param(
    [ValidateSet('Debug','Release')]
    $Configuration='Release'
)


$failed = @()

$AppVeyorLogger = "$env:ProgramFiles\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"
if (Test-Path $AppVeyorLogger) {
    $Logger = "/logger:`"$AppVeyorLogger`""
}

Get-ChildItem $PSScriptRoot\*.sln -rec |% {
    Write-Output "Restoring packages for $($_.Name)"
    nuget restore $_ -Verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        $failed += $_
    } else {
        Write-Output "Building $($_.Name)"
        msbuild $_ /nologo /m /verbosity:minimal /p:Configuration=$Configuration $Logger
        if ($LASTEXITCODE -ne 0) {
            $failed += $_
        }
    }
}

if ($failed.length -gt 0) {
    exit $failed.length
}
