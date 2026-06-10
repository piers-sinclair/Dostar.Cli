# Contributing to Dostar.Cli

## Distribution model

| Artifact | How it's distributed |
|----------|---------------------|
| `dostar` CLI | NuGet global tool — `dotnet tool install -g Dostar.Cli` |
| Dostar template | Cloned directly from `main` of [piers-sinclair/Dostar](https://github.com/piers-sinclair/Dostar) at scaffold time |

**No npm package.** `dostar` is a .NET global tool, not a Node tool.

**No separate template package.** `dostar new-project` clones the latest `main` branch of the template repo. Template changes reach users automatically — no CLI update needed.

### How `dostar new-project` works

1. Clones `https://github.com/piers-sinclair/Dostar.git` into the output directory.
2. Strips the `.git` history so the user's repo starts fresh.
3. Renames every `Dostar` / `dostar` token to the chosen project name (files, directories, and file contents).

---

## Branch & PR workflow

1. Create a branch from `main` named `feat/issue-<N>-<short-description>`.
2. Implement your changes.
3. Build — must pass with 0 warnings before committing:
   ```bash
   dotnet build
   ```
4. Commit using [Conventional Commits](https://www.conventionalcommits.org) format (see below — the PR title check enforces this).
5. Push and open a PR targeting `main`.

---

## Commit format

Use Conventional Commits. The PR title must follow this format — CI enforces it.

| Prefix | Semver bump | When to use |
|--------|-------------|-------------|
| `fix:` | patch | bug fix |
| `feat:` | minor | new command or flag |
| `feat!:` or `BREAKING CHANGE:` in body | major | breaking CLI change |
| `docs:` | none | documentation only |
| `chore:`, `ci:`, `refactor:`, `test:` | none | hidden from changelog |

---

## Release process

Releases are automated via [release-please](https://github.com/googleapis/release-please).
**No manual version bumps or git tags are needed.**

```
feat: or fix: commits land on main
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

The Release PR only touches `CHANGELOG.md`, `.release-please-manifest.json`, and `Dostar.Cli.csproj` — it never triggers any other workflow.

### Versioning

The CLI and the template are versioned **independently**. Template changes don't require a CLI release — users always clone `main`. Only release the CLI when the CLI code itself changes.

### One-time setup

The `NUGET_API_KEY` secret must be set in repo **Settings → Secrets and variables → Actions**:
- Name: `NUGET_API_KEY`
- Value: a NuGet.org API key with **Push new packages and package versions** scope for `Dostar.Cli`

### Checking the published package

```bash
dotnet tool search Dostar.Cli          # find the latest published version
dotnet tool update -g Dostar.Cli       # update an existing install
dotnet tool install -g Dostar.Cli --version 0.2.0   # install a specific version
```

---

## Keeping docs up to date

Update `CLAUDE.md` whenever you add a new command, change a key convention, or update the repo structure.
