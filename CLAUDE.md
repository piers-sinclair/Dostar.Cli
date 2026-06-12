# Dostar.Cli — Claude Code Context

`dostar` is the CLI companion to [Dostar](https://github.com/piers-sinclair/Dostar) — a fullstack template built around exceptional developer experience, complete DevSecOps, and CI/CD to production in under 30 minutes. Built with .NET 10 + System.CommandLine, it handles project creation and module scaffolding so teams skip straight to building features.

---

## Stack

| | |
|---|---|
| Runtime | .NET 10 |
| CLI framework | System.CommandLine 2.x |
| Package manager | NuGet |

---

## Repo structure

```
Commands/
  NewProjectCommand.cs   ← dostar new-project
  AddModuleCommand.cs    ← dostar add-module
  TemplateRenamer.cs     ← renames Dostar → <ProjectName> in cloned template
Templates/               ← embedded Scriban templates for add-module scaffolding
Dostar.Cli.csproj
Dostar.Cli.slnx
GlobalUsings.cs          ← global using directives shared across the project
Program.cs               ← entry point; registers all commands
CLAUDE.md                ← this file
```

---

## Key conventions

- **Global usings**: `GlobalUsings.cs` declares `global using` for namespaces used across multiple files. Avoid repeating `using` statements inside individual files.
- **No comments that restate code**: use readable method and variable names instead.
- **Test assertions**: **Shouldly** — never FluentAssertions or `Assert.*`.

---

## Commands

```bash
dostar new-project <ProjectName>              # clone Dostar template and rename references
dostar add-module <ModuleName>                # scaffold backend Contracts/Implementation/UnitTests/IntegrationTests
dostar add-module <ModuleName> --no-endpoints # scaffold as IModule (no HTTP endpoints)
dostar remove-module <ModuleName>             # remove backend module + solution/Program.cs cleanup
dostar add-feature <FeatureName>              # scaffold frontend/src/features/<name>/ (planned)
dostar remove-feature <FeatureName>           # delete frontend/src/features/<name>/ (planned)
```

Backend modules (`add-module`/`remove-module`) and frontend features (`add-feature`/`remove-feature`) are
intentionally separate commands — a backend module does not always have a corresponding frontend feature,
and vice versa.

---

## Running locally

```bash
dotnet run -- new-project MyStartup
```

## Build and install locally

```bash
dotnet pack
dotnet tool install -g Dostar.Cli --add-source bin/Release/
```

---

## Licensing policy

All NuGet dependencies must be **free for commercial use in closed-source projects**.
Acceptable: MIT, Apache 2.0, BSD-2, BSD-3, ISC.
Avoid: GPL, LGPL, AGPL, SSPL, BSL.

---

## Working on a GitHub issue

Every issue must be implemented on a dedicated feature branch and merged via a pull request. Never commit directly to `main`.

### Branch & PR workflow

1. **Create a branch** from `main` named `feat/issue-<N>-<short-description>`
   ```bash
   git checkout main && git pull
   git checkout -b feat/issue-9-add-module-cmd
   ```
2. **Implement** the changes.
3. **Build** — must pass with 0 warnings before committing:
   ```bash
   dotnet build
   ```
4. **Commit** with a message that references the issue (`Closes #N`).
5. **Push** and open a PR targeting `main`:
   ```bash
   git push -u origin <branch>
   gh pr create --title "..." --body "..."
   ```

Only open the PR once the build passes locally.

---



Update `CLAUDE.md` whenever you add a new command or change a key convention.## Cross-repo dependency

This CLI scaffolds and manages projects based on the [Dostar](https://github.com/piers-sinclair/Dostar) template. Changes to either repo can require updates to the other.

**When Dostar changes, check if Dostar.Cli needs updating:**
- New parameters in `infra/main.bicep` or parameter files → `ProjectService` may need to inject or placeholder those values during `new-project`
- New template files containing project-name tokens → token replacement logic may need extending
- New module structure conventions → `add-module` Scriban templates may need updating

**When Dostar.Cli changes, check if Dostar needs updating:**
- New `add-module` scaffold structure → verify it matches the module pattern in `CLAUDE.md` and `docs/module-pattern.md`
- Changes to `new-project` output → verify the generated project still builds and runs correctly

## Keeping this file up to date

Update `CLAUDE.md` whenever you add a new command or change a key convention.
