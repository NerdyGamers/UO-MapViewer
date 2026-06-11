using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;

namespace UO.MapViewer
{
    public class CoordinateEventArgs : EventArgs
    {
        public int X { get; init; }
        public int Y { get; init; }
    }

    /// <summary>
    /// Double-buffered panel that renders UO map tiles using RadarColor data.
    /// Supports pan (LMB drag) and zoom (mouse wheel).
    /// </summary>
    public class MapPanel : Panel
    {
        public event EventHandler<CoordinateEventArgs>? CoordinateChanged;

        public Map? CurrentMap { get; private set; }

        // Viewport origin in tile coordinates
        private float _originX = 0f;
        private float _originY = 0f;

        // Pixels per tile (zoom level); base is 1 px/tile at minimum, can go up to 8+
        private float _tileSize = 2f;
        private const float MinTileSize = 1f;
        private const float MaxTileSize = 16f;

        // Panning state
        private bool _panning;
        private Point _lastMousePos;

        // Cached render bitmap
        private Bitmap? _cache;
        private bool _cacheDirty = true;

        public MapPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.Black;
        }

        public void LoadMap(Map map)
        {
            CurrentMap = map;
            _originX = 0;
            _originY = 0;
            _tileSize = 2f;
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cacheDirty = true;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (CurrentMap == null)
            {
                e.Graphics.Clear(Color.Black);
                using var font = new Font("Segoe UI", 12f);
                e.Graphics.DrawString("No map loaded — use File > Set Data Path",
                    font, Brushes.Gray, 20, 20);
                return;
            }

            if (_cacheDirty || _cache == null ||
                _cache.Width != Width || _cache.Height != Height)
            {
                RebuildCache();
            }

            e.Graphics.DrawImage(_cache!, 0, 0);
        }

        private void RebuildCache()
        {
            if (CurrentMap == null || Width <= 0 || Height <= 0) return;

            _cache?.Dispose();
            _cache = new Bitmap(Width, Height, PixelFormat.Format32bppRgb);

            int ts = Math.Max(1, (int)_tileSize);
            int tilesX = (int)Math.Ceiling(Width  / (float)ts) + 2;
            int tilesY = (int)Math.Ceiling(Height / (float)ts) + 2;

            int startX = (int)_originX;
            int startY = (int)_originY;

            var tileMatrix = CurrentMap.Tiles;

            using var g = Graphics.FromImage(_cache);
            g.Clear(Color.Black);

            for (int ty = 0; ty < tilesY; ty++)
            {
                for (int tx = 0; tx < tilesX; tx++)
                {
                    int mx = startX + tx;
                    int my = startY + ty;

                    if (mx < 0 || my < 0 || mx >= CurrentMap.Width || my >= CurrentMap.Height)
                        continue;

                    // Land tile
                    var landTile = tileMatrix.GetLandTile(mx, my);
                    ushort landId = (ushort)(landTile.ID & 0x3FFF);
                    int radarColor16 = RadarCol.GetColorData(landId);
                    var color = Color565To32(radarColor16);

                    int px = (int)((tx - (_originX - startX)) * _tileSize);
                    int py = (int)((ty - (_originY - startY)) * _tileSize);

                    using var brush = new SolidBrush(color);
                    g.FillRectangle(brush, px, py, ts, ts);

                    // Static tiles (top-most visible)
                    var statics = tileMatrix.GetStaticTiles(mx, my);
                    if (statics.Length > 0)
                    {
                        // Use highest static's radar color
                        int highZ = int.MinValue;
                        int staticId = 0;
                        foreach (var s in statics)
                        {
                            if (s.Z > highZ) { highZ = s.Z; staticId = s.ID; }
                        }
                        int sc = RadarCol.GetColorData(staticId + 0x4000);
                        using var sb = new SolidBrush(Color565To32(sc));
                        g.FillRectangle(sb, px, py, ts, ts);
                    }
                }
            }

            _cacheDirty = false;
        }

        private static Color Color565To32(int c16)
        {
            int r = (c16 >> 10) & 0x1F;
            int g2 = (c16 >> 5) & 0x1F;
            int b = c16 & 0x1F;
            return Color.FromArgb(
                (r << 3) | (r >> 2),
                (g2 << 3) | (g2 >> 2),
                (b << 3) | (b >> 2));
        }

        // ── Mouse: Pan ─────────────────────────────────────────────────────────
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
            {
                _panning = true;
                _lastMousePos = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _panning = false;
            Cursor = Cursors.Default;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_panning && CurrentMap != null)
            {
                float dx = (e.X - _lastMousePos.X) / _tileSize;
                float dy = (e.Y - _lastMousePos.Y) / _tileSize;
                _originX = Math.Clamp(_originX - dx, 0, CurrentMap.Width  - 1);
                _originY = Math.Clamp(_originY - dy, 0, CurrentMap.Height - 1);
                _lastMousePos = e.Location;
                InvalidateCache();
            }

            if (CurrentMap != null)
            {
                int tileX = (int)(_originX + e.X / _tileSize);
                int tileY = (int)(_originY + e.Y / _tileSize);
                CoordinateChanged?.Invoke(this, new CoordinateEventArgs { X = tileX, Y = tileY });
            }
        }

        // ── Mouse: Zoom ────────────────────────────────────────────────────────
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (CurrentMap == null) return;

            float tileXUnderCursor = _originX + e.X / _tileSize;
            float tileYUnderCursor = _originY + e.Y / _tileSize;

            float factor = e.Delta > 0 ? 1.25f : 0.8f;
            _tileSize = Math.Clamp(_tileSize * factor, MinTileSize, MaxTileSize);

            // Re-anchor so the tile under the cursor stays under the cursor
            _originX = Math.Clamp(tileXUnderCursor - e.X / _tileSize, 0, CurrentMap.Width  - 1);
            _originY = Math.Clamp(tileYUnderCursor - e.Y / _tileSize, 0, CurrentMap.Height - 1);

            InvalidateCache();
        }
    }
}
