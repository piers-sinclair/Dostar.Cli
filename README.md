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

## Release process

Releases are fully automated via [release-please](https://github.com/googleapis/release-please).
**No manual version bumps or git tags are needed.**

### How it works

```
conventional commits land on main
        │
        ▼
release-please opens a "chore(main): release X.Y.Z" PR
  • bumps <Version> in Dostar.Cli.csproj
  • updates CHANGELOG.md
        │
        ▼  (maintainer merges the Release PR)
        │
        ▼
GitHub Release + git tag created (e.g. v0.2.0)
        │
        ▼
nuget-publish.yml fires → packs + pushes to NuGet.org
```

### Commit format

Use [Conventional Commits](https://www.conventionalcommits.org):

| Prefix | Semver bump | Example |
|--------|-------------|---------|
| `fix:` | patch | `fix: handle empty project name` |
| `feat:` | minor | `feat: add remove-module command` |
| `feat!:` or `BREAKING CHANGE:` | major | `feat!: rename new-project flags` |
| `chore:`, `ci:`, `refactor:`, `test:` | none | hidden from changelog |

### One-time setup (maintainer)

Set the `NUGET_API_KEY` secret in **repo Settings → Secrets and variables → Actions**:
- Name: `NUGET_API_KEY`
- Value: a NuGet.org API key with **Push new packages and package versions** scope for `Dostar.Cli`

See [docs/cli-publish.md](https://github.com/piers-sinclair/Dostar/blob/main/docs/cli-publish.md) in the template repo for the full distribution model.