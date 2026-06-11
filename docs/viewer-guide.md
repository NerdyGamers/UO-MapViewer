# Viewer Guide

## Opening the Map

1. Launch `UO.MapViewer.exe`
2. A folder browser appears — point it at your UO data directory (the folder containing `map0.mul` or `map0LegacyMUL.uop`)
3. Felucca loads automatically at 2px/tile

## Navigation

| Action | How |
|---|---|
| Pan | Left-click drag or Middle-click drag |
| Zoom in | Scroll up |
| Zoom out | Scroll down |
| Jump to coord | (planned — use the coord HUD to orient yourself) |
| Switch facet | Toolbar dropdown |

## Exporting

Click **Export...** in the toolbar.

### Mode: Deep Zoom (recommended)

Produces a `.dzi` + `_files/` tile pyramid. Open it with [OpenSeadragon](https://openseadragon.github.io/):

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/openseadragon/4.1.0/openseadragon.min.js"></script>
<div id="osd" style="width:100%;height:100vh"></div>
<script>
  OpenSeadragon({ id: 'osd', tileSources: './felucca.dzi' });
</script>
```

### Mode: PNG Tile Grid

Flat `tile_RRRR_CCCC.png` files. Easy to import into map editors or tile-based tools.

### Mode: Full PNG

One giant image. At Scale=1, Felucca is **7168 × 4096 px**. At Scale=4, that's **28672 × 16384 px** — make sure you have the RAM.

## Scale Multiplier

The scale setting upscales each UO tile before export:

| Scale | Pixel size per UO tile | Felucca total |
|---|---|---|
| 1 | 1 px | 7168 × 4096 |
| 2 | 2 px | 14336 × 8192 |
| 4 | 4 px | 28672 × 16384 |
| 8 | 8 px | 57344 × 32768 |

For web use (OpenSeadragon), Scale=2 or Scale=4 with Deep Zoom mode gives excellent detail without a prohibitive file size.
