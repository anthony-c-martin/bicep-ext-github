# GitHub Bicep Extension

## Usage

1. Download the [Samples folder](https://download-directory.github.io/?url=https%3A%2F%2Fgithub.com%2Fanthony-c-martin%2Fbicep-ext-github%2Ftree%2Fmain%2Fsamples), and unzip it.
1. Open the unzipped Samples folder in VSCode, and select one of the `.bicepparam` files you wish to deploy.
1. Launch the [Deploy Pane](https://github.com/Azure/bicep/blob/main/docs/experimental/deploy-ui.md) to run the deployment.

> [!NOTE]
> Extension binary packages are not signed on a Mac. If you see the following error, you will need to manually sign the extension package:
> 
> `Failed to launch provider: Failed to connect to provider /Users/ant/.bicep/br/bicepextdemo.azurecr.io/extensions$github/0.1.1$/extension.bin`
> 
> To work around it, run the following in a terminal window, using the path from the error message:
> 
> `codesign -s - '/Users/ant/.bicep/br/bicepextdemo.azurecr.io/extensions$github/0.1.1$/extension.bin'`

## Build + Test Locally

### Rebuild the extension
These commands publish the extension to the local file system, and updates the sample bicepconfig to point to the local extension.
```sh
./scripts/publish.sh ./bin/bicep-ext-github
jq '.extensions.github="../bin/bicep-ext-github"' ./samples/bicepconfig.json > ./samples/bicepconfig.new.json
mv ./samples/bicepconfig.new.json ./samples/bicepconfig.json
```

### Test the extension
Run the deployment.
```sh
~/.azure/bin/bicep local-deploy ./samples/basic/main.bicepparam
```

To enable verbose tracing, run the following beforehand.
```sh
export BICEP_TRACING_ENABLED=true
```

## Releasing

Releases are cut manually so that versioning stays under explicit control — pushing to `main` does
not publish anything. To release, run the **Release** workflow from the Actions tab (or with
`gh workflow run release.yml -f version=0.2.0`) and supply the exact version to publish.

The workflow validates the version, builds and tests, publishes
`br:ghcr.io/anthony-c-martin/bicep-ext-github:<version>` and then pushes a `v`-prefixed git tag
(`v0.2.0`) and GitHub Release. Note that the OCI artifact is tagged with the bare version, while the
git tag carries the `v` prefix. The workflow refuses to run if the tag already exists, so published
versions are never replaced; releases must be cut from `main`.

The version supplied to the workflow is stamped into the binary via `-p:Version=`, and is what the
extension reports to Bicep. Local builds use the placeholder `0.0.1-dev` version from
[`Bicep.Extension.GitHub.csproj`](./src/Bicep.Extension.GitHub/Bicep.Extension.GitHub.csproj).

To pick up a new version after publishing, update your `bicepconfig.json` to reference the new tag.

## Repository configuration

To configure this repository's GitHub branch protection and collaborators, login with the `gh` CLI and run:

```powershell
./scripts/setup.ps1
```

The script obtains a token with `gh auth token` and deploys
[`scripts/repo/main.bicepparam`](./scripts/repo/main.bicepparam).

## Building other extensions

This repo is also intended to demonstrate how to build + publish an end-to-end Bicep extension in C#. Feel free to copy, rename and modify it to prototype building an extension to extend other services.