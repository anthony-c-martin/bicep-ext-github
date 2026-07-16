#!/usr/bin/env pwsh
[cmdletbinding()]
param(
   [Parameter(Mandatory=$true)][string]$Target
)

$ErrorActionPreference = "Stop"

function ExecSafe([scriptblock] $ScriptBlock) {
  & $ScriptBlock
  if ($LASTEXITCODE -ne 0) {
      exit $LASTEXITCODE
  }
}

$root="$PSScriptRoot/.."
$extName="bicep-ext-github"

# prefer bicep from $PATH, fall back to ~/.azure/bin/bicep
$bicepCmd = if (Get-Command bicep -ErrorAction SilentlyContinue) { "bicep" } else { "$HOME/.azure/bin/bicep" }

# build various flavors
ExecSafe { dotnet publish --configuration Release $root -r osx-arm64 }
ExecSafe { dotnet publish --configuration Release $root -r linux-x64 }
ExecSafe { dotnet publish --configuration Release $root -r linux-arm64 }
ExecSafe { dotnet publish --configuration Release $root -r win-x64 }
ExecSafe { dotnet publish --configuration Release $root -r win-arm64 }

# publish to the registry
ExecSafe { & $bicepCmd publish-extension `
  --bin-osx-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/osx-arm64/publish/$extName" `
  --bin-linux-x64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/linux-x64/publish/$extName" `
  --bin-linux-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/linux-arm64/publish/$extName" `
  --bin-win-x64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/win-x64/publish/$extName.exe" `
  --bin-win-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/win-arm64/publish/$extName.exe" `
  --target "$target" `
  --force }