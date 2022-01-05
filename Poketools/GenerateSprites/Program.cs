using System;
using System.IO;
using System.Linq;
using System.Threading;
using MainStreaming.Library.µFfmpeg;
using MainStreaming.Library.µFfmpeg.Builders;

const int maxPokeHor = 31;
const int maxPokeVer = 21;
const int pokeHeight = 100;
const int pokeWidth = 96;

var ffmpeg = MicroFfmpeg.Ffmpeg();

var pokemonSheet = new FileInfo("sprites/front.png");
var pokeTxt = new FileInfo("sprites/pokemonnames.txt");
var palette_source = new FileInfo("sprites/palette_source.png");
using var stream = new StreamReader(pokeTxt.OpenRead());
var pokemonNamesList = (await stream.ReadToEndAsync()).Split("\n").Select(s => s.Replace("\r", "")).ToArray();

var palette = new FileInfo("palette.png");

var ffmpegArgs = new FfMpegArguments()
    .Header("-y")
    .Input(palette_source.FullName)
    .VideoFilter(new []{("palettegen", new (string key, string value)[]
    {
        ("max_colors", "4"),
        ("transparency_color", "white")
    })})
    .Output("palette.png");

Console.WriteLine(ffmpegArgs.Build());
await ffmpeg.Execute(ffmpegArgs.Build(), CancellationToken.None);
var directory = new DirectoryInfo("extracted");
directory.Create();
var k = 0;
for (var i = 0; i < maxPokeVer; i++)
for (var j = 0; j < maxPokeHor; j++, k++)
{
    var old = new FileInfo($"sprites/old/front/{pokemonNamesList[k]}.png");
    var probe = await MainStreaming.Library.µFfmpeg.Commons.Convert.GetFastInfo(old, CancellationToken.None);
    ffmpegArgs = new FfMpegArguments()
        .Header("-y")
        .Input(pokemonSheet.FullName)
        .Input(palette.FullName)
        .FilterComplex($"crop=96:100:{j * pokeWidth}:{i * pokeHeight}[0];[0]hue=s=0[1];[1]paletteuse[2];[2]crop=w={probe.Streams.First().Width}:h={probe.Streams.First().Height}[3]")
        .Map("[3]")
        .Output($@"{directory.FullName}\{pokemonNamesList[k]}.png");

    Console.WriteLine(ffmpegArgs.Build());
    await ffmpeg.Execute(ffmpegArgs.Build(), CancellationToken.None);
}
