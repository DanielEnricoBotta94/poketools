using System;
using System.IO;
using System.Linq;
using System.Threading;
using ImageMagick;
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

await ffmpeg.Execute(ffmpegArgs.Build(), CancellationToken.None);
var directory = new DirectoryInfo("extracted");
directory.Create();
var k = 0;
for (var i = 0; i < maxPokeVer; i++)
for (var j = 0; j < maxPokeHor; j++, k++)
{
    var old = new FileInfo($"sprites/old/front/{pokemonNamesList[k]}.png");
    var magick = new MagickImage(old.FullName);
   // var (top, bottom, left, right) = GetDistanceBetweenBorderAndSprite(magick);
    
    var probe = await MainStreaming.Library.µFfmpeg.Commons.Convert.GetFastInfo(old, CancellationToken.None);
    ffmpegArgs = new FfMpegArguments()
        .Header("-y")
        .Input(pokemonSheet.FullName)
        .Input(palette.FullName)
        .Option("-frames:v 1")
        .FilterComplex($"color=white,format=rgb24[c];[c][0]scale2ref[c][i];[c][i]overlay[S];[S]crop=96:100:{j * pokeWidth}:{i * pokeHeight}[1];[1]hue=s=0[2];[2]paletteuse[3];[3]crop=w={probe.Streams.First().Width}:h={probe.Streams.First().Height}[4]")
        .Map("[4]")
        .Output($@"{directory.FullName}\{pokemonNamesList[k]}.png");

    Console.WriteLine(ffmpegArgs.Build());
    await ffmpeg.Execute(ffmpegArgs.Build(), CancellationToken.None);
}

return;
//(int top, int bottom, int left, int right) GetDistanceBetweenBorderAndSprite(MagickImage image)
//{
//    var pixels = image.GetPixels()
//    for (var i = 0; i < image.Width; i++)
//    {
//      //  image
//    }
//}