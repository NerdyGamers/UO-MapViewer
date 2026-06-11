# Contributing to UO-MapViewer

Thank you for your interest in contributing! This document covers everything you need to get started.

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows (WinForms viewer requires Windows; CLI exporter can run on any OS)
- A copy of the Ultima Online client files (`.mul` or `.uop` format)
- Visual Studio 2022+ or VS Code with the C# Dev Kit extension

---

## Project Structure

```
UO-MapViewer/
├── Ultima/             # Embedded Ultima.DLL source (UoFiddlerPixel fork)
├── UO.MapViewer/       # WinForms interactive viewer
├── UO.MapExporter/     # CLI deep-zoom / tile / full-PNG exporter
├── docs/               # Architecture and web viewer docs
└── UO-MapViewer.sln
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for a deeper breakdown of how the three projects interact.

---

## Building

```bash
git clone https://github.com/NerdyGamers/UO-MapViewer.git
cd UO-MapViewer
dotnet restore
dotnet build
```

To run the viewer:

```bash
dotnet run --project UO.MapViewer
```

To run the exporter:

```bash
dotnet run --project UO.MapExporter -- --help
```

---

## Setting Up Client Data

The viewer and exporter both expect UO client files. Set the path in the app settings or pass `--client-path` on the CLI:

```bash
dotnet run --project UO.MapExporter -- --client-path "C:/UO" --facet felucca --format dzi --output ./out
```

Both `.mul` (classic) and `.uop` (Enhanced Client) formats are supported.

---

## Coding Style

- Follow existing naming conventions (PascalCase for types/methods, camelCase for locals)
- Keep `Ultima/` read-only unless fixing a bug in the data reader layer
- New exporter formats should implement a common `IExporter` interface
- Add XML doc comments to any public API surface you add
- No trailing whitespace; 4-space indentation (not tabs)

---

## Reporting Bugs

Use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.yml). Please include:

- Your .NET version (`dotnet --version`)
- Your UO client version and file format (`.mul` / `.uop`)
- The facet and coordinates where the issue occurs
- A screenshot or exported tile if the bug is visual

---

## Requesting Features

Use the [feature request template](.github/ISSUE_TEMPLATE/feature_request.yml). Check open issues first to avoid duplicates.

---

## Pull Requests

1. Fork the repo and create a branch: `git checkout -b feature/my-feature`
2. Make your changes and ensure `dotnet build` passes cleanly
3. Open a PR against `main` using the PR template
4. Describe what changed and why, and link any related issues

---

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
