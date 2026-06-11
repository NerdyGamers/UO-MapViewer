# Architecture

This document describes the structure of the UO-MapViewer solution and how the three projects interact.

---

## Solution Overview

```
UO-MapViewer.sln
+-- Ultima/                  # Data layer: UO client file readers
+-- UO.MapViewer/            # Presentation layer: WinForms interactive viewer
+-- UO.MapExporter/          # Export layer: CLI tile/DZI/PNG exporter
```

All three projects target **.NET 8**. `UO.MapViewer` requires Windows (WinForms). `UO.MapExporter` can run cross-platform.

---

## Ultima/ (Data Layer)

A lightly-modified fork of the `Ultima.dll` source from UoFiddlerPixel. Provides low-level readers for UO client files.

### Key Classes

| Class | Responsibility |
|---|---|
| `Map` | Loads facet map data; routes between `.mul` and `.uop` formats |
| `TileMatrix` | Reads map tile blocks and static blocks for a given facet |
| `Statics` | Provides static item data per tile coordinate |
| `RadarColor` | Maps tile/static IDs to RGB colors for radar-style rendering |
| `Files` | Central path resolver; points to the UO client data directory |

### File Format Support

- **`.mul`** - Classic Client format (map0.mul, statics0.mul, staidx0.mul, etc.)
- **`.uop`** - Enhanced Client format (map0LegacyMUL.uop, etc.)

`Files.SetMulPath()` or `Files.SetUltimaOnlinePath()` must be called before any data access.

---

## UO.MapViewer/ (Presentation Layer)

A WinForms application that renders map tiles and statics interactively.

### Rendering Pipeline

1. User selects a facet (or default loads Felucca)
2. `Files.SetMulPath()` called with the configured client path
3. `MapRenderer` iterates `TileMatrix` blocks and calls `RadarColor` for each tile ID
4. Statics layer is composited on top if enabled
5. Result is drawn to the WinForms canvas with pan/zoom transforms applied
6. Coordinate HUD reads the inverse transform to display X, Y in UO tile space

---

## UO.MapExporter/ (Export Layer)

A CLI tool that exports map data to static image formats.

### CLI Flags

| Flag | Default | Description |
|---|---|---|
| `--client-path` | required | Path to UO client files |
| `--facet` | `felucca` | Facet to export |
| `--format` | `dzi` | Output format: `dzi`, `tiles`, `full` |
| `--scale` | `1` | Scale multiplier |
| `--statics` | `false` | Include statics layer |
| `--output` | `./output` | Output directory |

### Facet Dimensions (at scale 1)

| Facet | Width (tiles) | Height (tiles) |
|---|---|---|
| Felucca | 7168 | 4096 |
| Trammel | 7168 | 4096 |
| Ilshenar | 2304 | 1600 |
| Malas | 2560 | 2048 |
| Tokuno | 1448 | 1448 |
| TerMur | 1280 | 4096 |

---

## Data Flow

```
[UO Client Files (.mul / .uop)]
          |
          v
    [Ultima/ layer]
    TileMatrix, Statics, RadarColor
          |
    +-----+-----+
    |           |
    v           v
[UO.MapViewer]  [UO.MapExporter]
WinForms canvas  CLI: DZI / Tiles / Full PNG
```

---

## Adding a New Export Format

1. Create a new class in `UO.MapExporter/Exporters/` implementing `IExporter`
2. Register it in `Program.cs` format dispatch table
3. Add the new `--format` value to the CLI help text
4. Update this document and `CHANGELOG.md`
