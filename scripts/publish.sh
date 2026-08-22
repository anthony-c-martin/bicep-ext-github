#!/bin/bash
set -e

usage="Usage: ./publish.sh <target> [version]"
target=${1:?"Missing target. ${usage}"}
version=${2:-}

root="$(dirname ${BASH_SOURCE[0]})/.."
ext_name="bicep-ext-github"

# prefer bicep from $PATH, fall back to ~/.azure/bin/bicep
bicep_cmd=$(command -v bicep || echo "$HOME/.azure/bin/bicep")

version_arg=()
if [[ -n "$version" ]]; then
  version_arg=("-p:Version=$version")
fi

# build various flavors
dotnet publish --configuration Release $root -r osx-arm64 "${version_arg[@]}"
dotnet publish --configuration Release $root -r linux-x64 "${version_arg[@]}"
dotnet publish --configuration Release $root -r linux-arm64 "${version_arg[@]}"
dotnet publish --configuration Release $root -r win-x64 "${version_arg[@]}"
dotnet publish --configuration Release $root -r win-arm64 "${version_arg[@]}"

# publish to the registry
"$bicep_cmd" publish-extension \
  --bin-osx-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/osx-arm64/publish/$ext_name" \
  --bin-linux-x64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/linux-x64/publish/$ext_name" \
  --bin-linux-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/linux-arm64/publish/$ext_name" \
  --bin-win-x64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/win-x64/publish/$ext_name.exe" \
  --bin-win-arm64 "$root/src/Bicep.Extension.GitHub/bin/Release/net10.0/win-arm64/publish/$ext_name.exe" \
  --target "$target" \
  --force