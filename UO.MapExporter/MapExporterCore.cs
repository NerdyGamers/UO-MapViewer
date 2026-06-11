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
        /// Renders a rectangular block of UO tiles into a Bitmap.
        /// </summary>
        public static Bitmap RenderBlock(
            Map map,
            int startX, int startY,
            int blockW, int blockH,
            int scale,
            bool renderStatics)
        {
            int px = blockW * scale;
            int py = blockH * scale;
            var bmp = new Bitmap(px, py, PixelFormat.Format32bppArgb);

            var tileMatrix = map.Tiles;

            unsafe
            {
                var data = bmp.LockBits(new Rectangle(0, 0, px, py), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                int stride = data.Stride / 4;
                var ptr = (int*)data.Scan0;

                for (int ty = 0; ty < blockH; ty++)
                {
                    for (int tx = 0; tx < blockW; tx++)
                    {
                        int mx = startX + tx;
                        int my = startY + ty;

                        int color = Color.Black.ToArgb();

                        if (mx >= 0 && my >= 0 && mx < map.Width && my < map.Height)
                        {
                            var land = tileMatrix.GetLandTile(mx, my);
                            ushort id = (ushort)(land.ID & 0x3FFF);
                            color = Radar565ToArgb(RadarCol.GetColorData(id));

                            if (renderStatics)
                            {
                                var statics = tileMatrix.GetStaticTiles(mx, my);
                                int highZ = int.MinValue;
                                int sid = -1;
                                foreach (var s in statics)
                                    if (s.Z > highZ) { highZ = s.Z; sid = s.ID; }

                                if (sid >= 0)
                                    color = Radar565ToArgb(RadarCol.GetColorData(sid + 0x4000));
                            }
                        }

                        for (int sy = 0; sy < scale; sy++)
                            for (int sx = 0; sx < scale; sx++)
                                ptr[(ty * scale + sy) * stride + tx * scale + sx] = color;
                    }
                }

                bmp.UnlockBits(data);
            }

            return bmp;
        }

        private static int Radar565ToArgb(int c16)
        {
            int r = (c16 >> 10) & 0x1F;
            int g = (c16 >> 5)  & 0x1F;
            int b = c16         & 0x1F;
            return (unchecked((int)0xFF000000))
                 | ((r << 3 | r >> 2) << 16)
                 | ((g << 3 | g >> 2) << 8)
                 |  (b << 3 | b >> 2);
        }
    }
}
