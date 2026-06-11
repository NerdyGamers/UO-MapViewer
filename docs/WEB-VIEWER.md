# Web Viewer

This document describes how to use UO-MapViewer's DZI export with [OpenSeadragon](https://openseadragon.github.io/) to create a browser-based, zoomable UO map viewer.

---

## Overview

The `UO.MapExporter` CLI exports any UO facet as a **Deep Zoom Image (DZI)** - a tile pyramid format designed for high-resolution pan/zoom in a browser. OpenSeadragon is a free, open-source JavaScript viewer with native DZI support.

This means you can:
- Host a zoomable UO map on any static web server (GitHub Pages, Netlify, etc.)
- Navigate the full 7168x4096 Felucca map smoothly at any zoom level
- Optionally include a statics overlay

---

## Step 1: Export a Facet as DZI

```bash
dotnet run --project UO.MapExporter -- \
  --client-path "C:/Program Files (x86)/Ultima Online" \
  --facet felucca \
  --format dzi \
  --statics true \
  --output ./web/maps/felucca
```

Output structure:

```
web/maps/felucca/
  felucca.dzi              # DZI manifest
  felucca_files/
    0/                     # Zoom level 0 (thumbnail)
    ...
    13/                    # Full resolution 256x256 tiles
      0_0.png
      0_1.png
```

---

## Step 2: Create the HTML Page

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <title>UO Map Viewer</title>
  <style>
    html, body { margin: 0; padding: 0; background: #000; }
    #viewer { width: 100vw; height: 100vh; }
  </style>
</head>
<body>
  <div id="viewer"></div>
  <script src="https://cdn.jsdelivr.net/npm/openseadragon@4.1/build/openseadragon/openseadragon.min.js"></script>
  <script>
    OpenSeadragon({
      id: 'viewer',
      prefixUrl: 'https://cdn.jsdelivr.net/npm/openseadragon@4.1/build/openseadragon/images/',
      tileSources: 'maps/felucca/felucca.dzi',
      defaultZoomLevel: 1,
      showNavigator: true,
      navigatorPosition: 'BOTTOM_RIGHT'
    });
  </script>
</body>
</html>
```

---

## Step 3: Serve Locally

```bash
# Python
python -m http.server 8080 --directory ./web

# Node
npx serve ./web
```

Open `http://localhost:8080`.

---

## Step 4: Deploy to GitHub Pages

1. Push your `web/` output to the `gh-pages` branch
2. Enable GitHub Pages in Settings > Pages
3. Live at `https://nerdygamers.github.io/UO-MapViewer/`

> **Note:** Full Felucca DZI with statics is ~500MB+. Use `--scale 0.5` for GitHub Pages or host tiles on a CDN.

---

## Multiple Facets

```javascript
const facets = {
  felucca:  'maps/felucca/felucca.dzi',
  trammel:  'maps/trammel/trammel.dzi',
  ilshenar: 'maps/ilshenar/ilshenar.dzi',
  malas:    'maps/malas/malas.dzi',
  tokuno:   'maps/tokuno/tokuno.dzi',
  termur:   'maps/termur/termur.dzi'
};

function switchFacet(name) {
  viewer.open(facets[name]);
}
```

---

## Tips

- `--scale 0.5` halves tile dimensions and significantly reduces export size
- `--statics false` for a faster base-layer-only export during development
- OpenSeadragon supports overlays and mouse tracking if you want a coordinate HUD
- See the [OpenSeadragon docs](https://openseadragon.github.io/docs/) for advanced config
