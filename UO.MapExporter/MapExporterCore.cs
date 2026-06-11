using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Ultima;

namespace UO.MapExporter
{
    /// <summary>
    /// Core rendering + export logic. Shared by the CLI and the WinForms ExportDialog.
    /// Delegates rendering to Map.GetImage() which uses the correct RadarCol.Colors pipeline.
    /// </summary>
    public static class MapExporterCore
    {
        public static void Export(ExportOptions opts, IProgress<int> progress, CancellationToken ct)
        {
            Directory.CreateDirectory(opts.OutputPath);

            switch (opts.Mode)
            {
                case ExportMode.DeepZoom:
                    DeepZoomExporter.Export(opts, progress, ct);
                    break;
                case ExportMode.TileGrid:
                    TileExporter.ExportGrid(opts, progress, ct);
                    break;
                case ExportMode.FullPng:
                    TileExporter.ExportFull(opts, progress, ct);
                    break;
            }
        }

        /// <summary>
        /// Renders a rectangular region of UO tiles into a 32bpp Bitmap.
        /// Coordinates are in tile units; the result is (blockW * scale) x (blockH * scale) pixels.
        /// Delegates to Map.GetImage() which uses Ultima's native RadarCol.Colors pipeline.
        /// </summary>
        public static Bitmap RenderBlock(
            Map map,
            int startX, int startY,
            int blockW, int blockH,
            int scale,
            bool renderStatics)
        {
            // Map.GetImage() takes 8x8-block coordinates and renders each block as 8x8 pixels.
            // Convert tile coords to block coords by dividing by 8, then request enough blocks
            // to cover the desired tile region.
            int blockStartX = startX / 8;
            int blockStartY = startY / 8;
            int blocksWide  = (blockW + 7) / 8;
            int blocksTall  = (blockH + 7) / 8;

            // GetImage returns a 16bpp RGB555 bitmap; convert to 32bpp for downstream use.
            using var raw = map.GetImage(blockStartX, blockStartY, blocksWide, blocksTall, renderStatics);

            int srcW = raw.Width;
            int srcH = raw.Height;

            // Trim to exact tile region (GetImage may render extra tiles at the edges).
            int tilePixW = Math.Min(blockW, srcW);
            int tilePixH = Math.Min(blockH, srcH);

            int outW = tilePixW * scale;
            int outH = tilePixH * scale;

            var result = new Bitmap(outW, outH, PixelFormat.Format32bppArgb);

            using var g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode   = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(raw, new Rectangle(0, 0, outW, outH),
                             new Rectangle(0, 0, tilePixW, tilePixH),
                             GraphicsUnit.Pixel);

            return result;
        }
    }
}
