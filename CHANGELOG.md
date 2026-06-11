# Changelog

All notable changes to UO-MapViewer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Web-based viewer using OpenSeadragon and pre-exported DZI tiles
- Facet selector UI in viewer toolbar
- Configurable statics layer toggle at runtime
- Export progress bar and cancellation support

---

## [0.1.0] - 2026-06-11

### Added
- Initial release of UO-MapViewer solution
- `UO.MapViewer` — WinForms interactive map viewer
  - Pan and zoom across all six facets (Felucca, Trammel, Ilshenar, Malas, Tokuno, TerMur)
  - Statics overlay rendered via RadarColor lookups
  - Live coordinate HUD (X, Y, Facet)
  - Export dialog with format and scale options
- `UO.MapExporter` — CLI export tool
  - Deep Zoom Image (DZI) tile export for OpenSeadragon
  - PNG tile grid export (`tile_XXXX_XXXX.png` naming)
  - Full-resolution single PNG export (Felucca = 7168x4096)
  - `--facet`, `--format`, `--scale`, `--statics`, `--output` flags
  - Both `.mul` and `.uop` client file support
- `Ultima/` — embedded Ultima.DLL source (UoFiddlerPixel fork)
  - TileMatrix, RadarColor, Map, and Statics data readers
- MIT License
- README with build, run, and CLI usage documentation
- `.gitignore` for .NET projects

[Unreleased]: https://github.com/NerdyGamers/UO-MapViewer/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/NerdyGamers/UO-MapViewer/releases/tag/v0.1.0
