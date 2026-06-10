# Contributing to Dostar.Cli

## Branch & PR workflow

1. Create a branch from `main` named `feat/issue-<N>-<short-description>`.
2. Implement your changes.
3. Build — must pass with 0 warnings before committing:
   ```bash
   dotnet build
   ```
4. Commit using [Conventional Commits](https://www.conventionalcommits.org) format (see below — the PR title check enforces this).
5. Push and open a PR targeting `main`.

## Commit format

Use Conventional Commits. The PR title must follow this format — CI enforces it.

| Prefix | Semver bump | When to use |
|--------|-------------|-------------|
| `fix:` | patch | bug fix |
| `feat:` | minor | new command or flag |
| `feat!:` or `BREAKING CHANGE:` in body | major | breaking CLI change |
| `docs:` | none | documentation only |
| `chore:`, `ci:`, `refactor:`, `test:` | none | hidden from changelog |

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

See [docs/cli-publish.md](https://github.com/piers-sinclair/Dostar/blob/main/docs/cli-publish.md) in the template repo for the full distribution model and one-time `NUGET_API_KEY` setup.

## Keeping docs up to date

Update `CLAUDE.md` whenever you add a new command, change a key convention, or update the repo structure.
