# Dostar CLI (`dostar`)

The `dostar` CLI is a .NET global tool for scaffolding and managing [Dostar](https://github.com/piers-sinclair/Dostar) modular monolith projects.

## Install

```bash
dotnet tool install -g Dostar.Cli
```

## Build and install locally

```bash
dotnet pack
dotnet tool install -g Dostar.Cli --add-source bin/Release/
```

After installation, the `dostar` command is available on your PATH.

## Usage

```bash
dostar --help
dostar --version
dostar new-project MyStartup
```

## Uninstall

```bash
dotnet tool uninstall -g Dostar.Cli
```

## Publishing a release

Releases are published to NuGet automatically when a `v*` tag is pushed to `main`.

1. Bump `<Version>` in `Dostar.Cli.csproj`.
2. Commit and push the version bump.
3. Tag the release:
   ```bash
   git tag v0.2.0
   git push origin v0.2.0
   ```
4. The `Release` workflow packs and pushes to NuGet.

The `NUGET_API_KEY` repository secret must be set for the workflow to succeed.
See [docs/cli-publish.md](https://github.com/piers-sinclair/Dostar/blob/main/docs/cli-publish.md) in the template repo for the full publish workflow.