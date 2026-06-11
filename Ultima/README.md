# Ultima Library

This is the Ultima.DLL source library, providing direct access to Ultima Online client data files (`.mul`, `.uop`).

Sourced from [jedi661/UoFiddlerPixel](https://github.com/jedi661/UoFiddlerPixel) — see root README for full attribution.

## Key Classes for Map Rendering

| Class | Purpose |
|---|---|
| `Map` | Facet definitions, dimensions, tile access |
| `TileMatrix` | Low-level tile/static reading from map*.mul |
| `TileMatrixPatch` | Reads mapdif*.mul overlay patches |
| `RadarCol` | 16-bit radar color lookup per tile ID |
| `Art` | Land and static art bitmaps |
| `TileData` | Tile flags (impassable, wall, water, etc.) |
| `Files` | Path configuration for the UO data directory |
