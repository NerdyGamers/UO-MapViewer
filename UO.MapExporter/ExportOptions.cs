using Ultima;

namespace UO.MapExporter
{
    public enum ExportMode { DeepZoom, TileGrid, FullPng }

    public class ExportOptions
    {
        public Map Map           { get; init; } = Map.Felucca;
        public string OutputPath { get; init; } = "./output";
        public ExportMode Mode   { get; init; } = ExportMode.DeepZoom;
        public int TileSize      { get; init; } = 256;
        public bool RenderStatics{ get; init; } = true;
        public int Scale         { get; init; } = 1;
    }
}
