using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using Ultima;
using UO.MapExporter;

var dataOpt   = new Option<string>("--datapath", "Path to UO client data directory") { IsRequired = true };
var mapOpt    = new Option<string>("--map",      () => "Felucca", "Facet name");
var modeOpt   = new Option<string>("--mode",     () => "deepzoom", "Export mode: deepzoom | tiles | full");
var outOpt    = new Option<string>("--out",      () => "./output",  "Output directory");
var tileOpt   = new Option<int>(   "--tilesize", () => 256,         "Tile size in pixels");
var staticsOpt= new Option<bool>(  "--statics",  () => true,        "Render statics layer");
var scaleOpt  = new Option<int>(   "--scale",    () => 1,           "Pixel scale multiplier");

var root = new RootCommand("UO Map Exporter — export UO maps to deep-zoom or PNG tiles")
{
    dataOpt, mapOpt, modeOpt, outOpt, tileOpt, staticsOpt, scaleOpt
};

root.SetHandler((string data, string mapName, string mode, string outPath, int tileSize, bool statics, int scale) =>
{
    Files.SetMulPath(data);

    var map = mapName.ToLower() switch
    {
        "trammel"  => Map.Trammel,
        "ilshenar" => Map.Ilshenar,
        "malas"    => Map.Malas,
        "tokuno"   => Map.Tokuno,
        "termur"   => Map.TerMur,
        _          => Map.Felucca
    };

    var exportMode = mode.ToLower() switch
    {
        "tiles" => ExportMode.TileGrid,
        "full"  => ExportMode.FullPng,
        _       => ExportMode.DeepZoom
    };

    var options = new ExportOptions
    {
        Map           = map,
        OutputPath    = outPath,
        Mode          = exportMode,
        TileSize      = tileSize,
        RenderStatics = statics,
        Scale         = scale
    };

    var progress = new Progress<int>(p =>
    {
        Console.Write($"\rProgress: {p}%   ");
    });

    Console.WriteLine($"Exporting {mapName} ({map.Width}x{map.Height}) → {mode.ToUpper()} → {outPath}");
    MapExporterCore.Export(options, progress, CancellationToken.None);
    Console.WriteLine("\nDone.");

}, dataOpt, mapOpt, modeOpt, outOpt, tileOpt, staticsOpt, scaleOpt);

return root.InvokeAsync(args).GetAwaiter().GetResult();
