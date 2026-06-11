using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Ultima;

namespace UO.MapExporter
{
    /// <summary>
    /// Exports a UO map as a flat PNG tile grid or a single full-resolution PNG.
    /// </summary>
    public static class TileExporter
    {
        /// <summary>Exports the map as a grid of fixed-size PNG tiles.</summary>
        public static void ExportGrid(ExportOptions opts, IProgress<int> progress, CancellationToken ct)
        {
            var map   = opts.Map;
            int ts    = opts.TileSize;
            int scale = opts.Scale;
            int fullW = map.Width  * scale;
            int fullH = map.Height * scale;

            int colCount = (int)Math.Ceiling(fullW / (float)ts);
            int rowCount = (int)Math.Ceiling(fullH / (float)ts);
            int total    = colCount * rowCount;
            int done     = 0;

            for (int row = 0; row < rowCount; row++)
            {
                ct.ThrowIfCancellationRequested();

                int pxY = row * ts;
                int pxH = Math.Min(ts, fullH - pxY);
                int mapY = pxY / scale;
                int mapH = (int)Math.Ceiling(pxH / (float)scale) + 1;

                for (int col = 0; col < colCount; col++)
                {
                    int pxX = col * ts;
                    int pxW = Math.Min(ts, fullW - pxX);
                    int mapX = pxX / scale;
                    int mapW = (int)Math.Ceiling(pxW / (float)scale) + 1;

                    using var block = MapExporterCore.RenderBlock(map, mapX, mapY, mapW, mapH, scale, opts.RenderStatics);
                    using var tile  = new Bitmap(pxW, pxH, PixelFormat.Format32bppArgb);
                    using var g     = Graphics.FromImage(tile);
                    g.DrawImage(block, 0, 0, pxW, pxH);

                    string fname = $"tile_{row:D4}_{col:D4}.png";
                    tile.Save(Path.Combine(opts.OutputPath, fname), ImageFormat.Png);

                    done++;
                    progress.Report(done * 100 / total);
                }
            }
        }

        /// <summary>Exports the entire map as one large PNG. Beware: Felucca is 7168x4096 at scale=1.</summary>
        public static void ExportFull(ExportOptions opts, IProgress<int> progress, CancellationToken ct)
        {
            var map   = opts.Map;
            int scale = opts.Scale;

            progress.Report(5);
            ct.ThrowIfCancellationRequested();

            using var bmp = MapExporterCore.RenderBlock(
                map, 0, 0, map.Width, map.Height, scale, opts.RenderStatics);

            progress.Report(90);
            ct.ThrowIfCancellationRequested();

            string fname = $"{map.ToString()!.ToLower()}_full.png";
            bmp.Save(Path.Combine(opts.OutputPath, fname), ImageFormat.Png);
            progress.Report(100);
        }
    }
}
