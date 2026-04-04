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