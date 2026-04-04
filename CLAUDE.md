# Dostar.Cli — Claude Code Context

`dostar` is a .NET 10 global tool (System.CommandLine) for scaffolding and managing [Dostar](https://github.com/piers-sinclair/Dostar) modular monolith projects.

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
  TemplateRenamer.cs     ← renames Dostar → <ProjectName> in cloned template
Dostar.Cli.csproj
GlobalUsings.cs          ← global using directives shared across the project
Program.cs               ← entry point; registers all commands
CLAUDE.md                ← this file
```

---

## Key conventions

- **Global usings**: `GlobalUsings.cs` declares `global using` for namespaces used across multiple files. Avoid repeating `using` statements inside individual files.
- **No comments that restate code**: use readable method and variable names instead.
- **No generic abstractions**: keep helpers specific to what they actually do (e.g. `CloneDostarAsync` not `RunProcessAsync`).
- **Test assertions**: **Shouldly** — never FluentAssertions or `Assert.*`.

---

## Commands

```bash
dostar new-project <ProjectName>   # clone Dostar template and rename references
```

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

## Keeping this file up to date

Update `CLAUDE.md` whenever you add a new command or change a key convention.