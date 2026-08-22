#!/usr/bin/env pwsh
[cmdletbinding()]
param()

$ErrorActionPreference = "Stop"

function ExecSafe([scriptblock] $ScriptBlock) {
  & $ScriptBlock
  if ($LASTEXITCODE -ne 0) {
      exit $LASTEXITCODE
  }
}

$env:GITHUB_TOKEN = ExecSafe { gh auth token }

ExecSafe { bicep local-deploy "$PSScriptRoot/repo/main.bicepparam" }
