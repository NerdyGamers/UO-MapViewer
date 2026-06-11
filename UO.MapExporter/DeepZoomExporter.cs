using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using Ultima;

namespace UO.MapExporter
{
    /// <summary>
    /// Exports a UO facet as a Deep Zoom Image (DZI) + tile pyramid.
    /// Compatible with OpenSeadragon, Leaflet-IIIF, and similar viewers.
    /// </summary>
    public static class DeepZoomExporter
    {
        public static void Export(ExportOptions opts, IProgress<int> progress, CancellationToken ct)
        {
            var map    = opts.Map;
            int scale  = opts.Scale;
            int ts     = opts.TileSize;           // DZI tile size
            int fullW  = map.Width  * scale;
            int fullH  = map.Height * scale;

            // Determine max zoom level
            int maxLevel = 0;
            {
                int w = fullW, h = fullH;
                while (w > 1 || h > 1) { w = Math.Max(1, (w + 1) / 2); h = Math.Max(1, (h + 1) / 2); maxLevel++; }
            }

            string mapName   = map.ToString()!.ToLower();
            string dziPath   = Path.Combine(opts.OutputPath, $"{mapName}.dzi");
            string filesDir  = Path.Combine(opts.OutputPath, $"{mapName}_files");
            Directory.CreateDirectory(filesDir);

            // Write DZI descriptor
            WriteDzi(dziPath, fullW, fullH, ts, "png");

            // Render full-res level, then downsample
            // Strategy: render in horizontal strips to avoid a single huge Bitmap allocation
            int stripH = Math.Max(ts, 512);  // render 512+ px strips at a time

            // We'll build the max-level tiles directly from strip renders
            string levelDir = Path.Combine(filesDir, maxLevel.ToString());
            Directory.CreateDirectory(levelDir);

            int totalStrips = (int)Math.Ceiling(fullH / (float)stripH);

            // Track row of DZI tiles being accumulated
            // Each DZI tile is ts x ts. We buffer a full row of tiles.
            Bitmap?[] tileRowBuffer = new Bitmap[(int)Math.Ceiling(fullW / (float)ts)];

            int processedStrips = 0;

            for (int stripY = 0; stripY < fullH; stripY += stripH)
            {
                ct.ThrowIfCancellationRequested();

                int actualH = Math.Min(stripH, fullH - stripY);

                // Map tile coords
                int mapStartY = stripY / scale;
                int mapBlockH = (int)Math.Ceiling(actualH / (float)scale) + 1;

                var strip = MapExporterCore.RenderBlock(
                    map, 0, mapStartY,
                    map.Width, mapBlockH,
                    scale, opts.RenderStatics);

                // Crop to actual pixel height
                int renderH = Math.Min(strip.Height, actualH);

                // Slice into DZI tiles
                int tileColCount = (int)Math.Ceiling(fullW / (float)ts);

                for (int col = 0; col < tileColCount; col++)
                {
                    int tileX0 = col * ts;
                    int tileW  = Math.Min(ts, fullW - tileX0);

                    // How many full DZI tile rows does this strip span?
                    int tileRow0 = stripY / ts;
                    int tileRow1 = (stripY + renderH - 1) / ts;

                    for (int row = tileRow0; row <= tileRow1; row++)
                    {
                        int tileY0    = row * ts;
                        int srcYStart = Math.Max(tileY0, stripY) - stripY;
                        int srcYEnd   = Math.Min(tileY0 + ts, stripY + renderH) - stripY;
                        int dstYStart = Math.Max(tileY0, stripY) - tileY0;
                        int tileH     = Math.Min(ts, fullH - tileY0);

                        if (tileRowBuffer[col] == null || tileRowBuffer[col]!.Height != tileH)
                        {
                            tileRowBuffer[col]?.Dispose();
                            tileRowBuffer[col] = new Bitmap(tileW, tileH, PixelFormat.Format32bppArgb);
                        }

                        using var g = Graphics.FromImage(tileRowBuffer[col]!);
                        g.DrawImage(strip,
                            new Rectangle(0, dstYStart, tileW, srcYEnd - srcYStart),
                            new Rectangle(tileX0, srcYStart, tileW, srcYEnd - srcYStart),
                            GraphicsUnit.Pixel);

                        // Flush tile if complete
                        bool tileComplete = (tileY0 + tileH) <= (stripY + renderH);
                        if (tileComplete)
                        {
                            string tilePath = Path.Combine(levelDir, $"{col}_{row}.png");
                            tileRowBuffer[col]!.Save(tilePath, ImageFormat.Png);
                            tileRowBuffer[col]!.Dispose();
                            tileRowBuffer[col] = null;
                        }
                    }
                }

                strip.Dispose();
                processedStrips++;
                progress.Report((int)(processedStrips * 80f / totalStrips));
            }

            // Flush any remaining partial tiles
            for (int col = 0; col < tileRowBuffer.Length; col++)
            {
                if (tileRowBuffer[col] != null)
                {
                    int row = (fullH - 1) / ts;
                    string tilePath = Path.Combine(levelDir, $"{col}_{row}.png");
                    tileRowBuffer[col]!.Save(tilePath, ImageFormat.Png);
                    tileRowBuffer[col]!.Dispose();
                }
            }

            // Build lower zoom levels by downsampling
            BuildLowerLevels(filesDir, maxLevel, fullW, fullH, ts, progress, ct);
        }

        private static void BuildLowerLevels(
            string filesDir, int maxLevel,
            int fullW, int fullH, int ts,
            IProgress<int> progress, CancellationToken ct)
        {
            for (int level = maxLevel - 1; level >= 0; level--)
            {
                ct.ThrowIfCancellationRequested();

                int scale  = 1 << (maxLevel - level);
                int levelW = Math.Max(1, (int)Math.Ceiling(fullW / (float)scale));
                int levelH = Math.Max(1, (int)Math.Ceiling(fullH / (float)scale));

                string srcDir = Path.Combine(filesDir, (level + 1).ToString());
                string dstDir = Path.Combine(filesDir, level.ToString());
                Directory.CreateDirectory(dstDir);

                int colCount = (int)Math.Ceiling(levelW / (float)ts);
                int rowCount = (int)Math.Ceiling(levelH / (float)ts);

                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        int tileW = Math.Min(ts, levelW - col * ts);
                        int tileH = Math.Min(ts, levelH - row * ts);

                        using var tile = new Bitmap(tileW, tileH, PixelFormat.Format32bppArgb);
                        using var g    = Graphics.FromImage(tile);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        // Each tile at this level covers 2x2 tiles from the level above
                        for (int dy = 0; dy < 2; dy++)
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int srcCol = col * 2 + dx;
                            int srcRow = row * 2 + dy;
                            string srcPath = Path.Combine(srcDir, $"{srcCol}_{srcRow}.png");
                            if (!File.Exists(srcPath)) continue;

                            using var src = new Bitmap(srcPath);
                            int dstX = dx * (tileW / 2);
                            int dstY = dy * (tileH / 2);
                            int dstW = src.Width  / 2;
                            int dstH = src.Height / 2;
                            if (dstW < 1) dstW = 1;
                            if (dstH < 1) dstH = 1;
                            g.DrawImage(src, dstX, dstY, dstW, dstH);
                        }

                        tile.Save(Path.Combine(dstDir, $"{col}_{row}.png"), ImageFormat.Png);
                    }
                }

                int pct = 80 + (int)((maxLevel - level) * 20f / maxLevel);
                progress.Report(Math.Min(pct, 99));
            }

            progress.Report(100);
        }

        private static void WriteDzi(string path, int width, int height, int tileSize, string format)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine($"<Image xmlns=\"http://schemas.microsoft.com/deepzoom/2008\"");
            sb.AppendLine($"       Format=\"{format}\" TileSize=\"{tileSize}\" Overlap=\"0\">");
            sb.AppendLine($"  <Size Width=\"{width}\" Height=\"{height}\" />");
            sb.AppendLine("</Image>");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
