#!/bin/bash
set -e

root="$(dirname ${BASH_SOURCE[0]})/.."

dotnet run --project "$root/src/Bicep.Types.GitHub/Bicep.Types.GitHub.csproj" -- --outdir "$root/types"