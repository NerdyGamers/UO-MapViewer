# UO-MapViewer

A high-resolution, deep-zoom UO map viewer and exporter built on **Ultima.DLL** — the same library powering UoFiddler.

## Features

- **Interactive map viewer** — pan/zoom any Ultima Online facet (Felucca, Trammel, Ilshenar, Malas, Tokuno, TerMur)
- **Statics overlay** — renders statics on top of the land layer with RadarColor lookups
- **Deep-zoom export** — exports full-resolution map images sliced into Deep Zoom Image (DZI) tiles for use with OpenSeadragon
- **PNG tile export** — exports fixed-size PNG tiles at any zoom level for archival or web use
- **Coordinate HUD** — live X/Y display as you mouse over the map
- **Multi-map support** — all six standard facets + custom map support via `Files.SetMulPath()`

## Requirements

- .NET 8 (Windows)
- Ultima Online client files (any era, .mul or .uop)
- Visual Studio 2022 / Rider

## Project Layout

```
UO-MapViewer/
├── UO-MapViewer.sln
├── Ultima/                    # Ultima.DLL source (from UoFiddlerPixel)
│   ├── Map.cs
│   ├── TileMatrix.cs
│   ├── RadarCol.cs
│   └── ... (full library)
├── UO.MapViewer/              # WinForms viewer application
│   ├── MainForm.cs
│   ├── MapPanel.cs
│   ├── ExportDialog.cs
│   └── UO.MapViewer.csproj
└── UO.MapExporter/            # CLI deep-zoom / PNG tile exporter
    ├── Program.cs
    ├── DeepZoomExporter.cs
    ├── TileExporter.cs
    └── UO.MapExporter.csproj
```

## Quick Start

1. Clone the repo
2. Open `UO-MapViewer.sln` in Visual Studio 2022
3. Build the solution (Ctrl+Shift+B)
4. Run `UO.MapViewer` — it will prompt for your UO data directory on first launch
5. Select a facet, pan/zoom, and optionally export

## Export Formats

### Deep Zoom Image (DZI)
Outputs a `.dzi` descriptor and a `_files/` tile directory compatible with [OpenSeadragon](https://openseadragon.github.io/). Each tile is 256×256 PNG.

```
output/
├── felucca.dzi
└── felucca_files/
    ├── 0/
    ├── 1/
    └── ... (zoom levels)
```

### PNG Tile Grid
Outputs fixed-size PNG tiles in a flat directory:
```
output/felucca/
├── tile_0000_0000.png
├── tile_0000_0001.png
└── ...
```

### Full Map PNG
Single full-resolution image (warning: Felucca is 7168×4096 px — large!):
```
dotnet run --project UO.MapExporter -- --map Felucca --mode full --out ./output
```

## CLI Usage (UO.MapExporter)

```bash
dotnet run --project UO.MapExporter -- \
  --datapath "C:\Program Files\Ultima Online" \
  --map Felucca \
  --mode deepzoom \
  --out ./output/felucca \
  --tilesize 256 \
  --statics true
```

| Flag | Default | Description |
|---|---|---|
| `--datapath` | (required) | Path to UO client data folder |
| `--map` | `Felucca` | Map name: Felucca, Trammel, Ilshenar, Malas, Tokuno, TerMur |
| `--mode` | `deepzoom` | Export mode: `deepzoom`, `tiles`, `full` |
| `--out` | `./output` | Output directory |
| `--tilesize` | `256` | Tile size in pixels (deepzoom/tiles mode) |
| `--statics` | `true` | Render statics layer |
| `--scale` | `1` | Pixel scale multiplier (2 = 2x upscale per UO tile) |

## Source Attribution

The `Ultima/` library directory is sourced from [jedi661/UoFiddlerPixel](https://github.com/jedi661/UoFiddlerPixel), which is itself derived from the UoFiddler project. Original authors: **Malt/Zerodown**, **Folkmar Bienert**, and UoFiddler contributors.

## License

MIT — see LICENSE
