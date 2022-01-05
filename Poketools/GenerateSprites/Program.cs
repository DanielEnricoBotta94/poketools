using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MainStreaming.Library.µFfmpeg;
using MainStreaming.Library.µFfmpeg.Builders;
using MainStreaming.Library.µFfmpeg.Models.Interfaces;

async Task GeneratePokemonSingleSprites(string[] pokemonNamesList, int maxPokeHor, int maxPokeVer,
    FileInfo? pokemonSheet, int pokeWidth, int pokeHeight, IFfMpegProcess? ffMpegProcess)
{
    var directory = new DirectoryInfo("extracted");
    directory.Create();
    var k = 0;
    for (var i = 0; i < maxPokeHor; i++)
    {
        for (var j = 0; j < maxPokeVer; j++, k++)
        {

            var ffmpeg_args = new FfMpegArguments()
                .Header("-y")
                .Input(pokemonSheet.FullName)
                .FilterComplex($"crop=96:100:{j * pokeWidth}:{i * pokeHeight}[0];[0]hue=s=0[1]")
                .Map("[1]")
                .Output($@"{directory.FullName}\{pokemonNamesList[k].Replace("\r", "")}.png");

            Console.WriteLine(ffmpeg_args.Build());
            await ffMpegProcess.Execute(ffmpeg_args.Build(), CancellationToken.None);
        }
    }
}

const int max_poke_hor = 31;
const int max_poke_ver = 21;
const int poke_height = 100;
const int poke_width = 96;

var ffmpeg = MicroFfmpeg.Ffmpeg();

var pokemon_sheet = new FileInfo("sprites/front.png");
var poke_txt = new FileInfo("sprites/pokemonnames.txt");
using var stream = new StreamReader(poke_txt.OpenRead());
var pokemon_names_list = (await stream.ReadToEndAsync()).Split("\n");

await GeneratePokemonSingleSprites(pokemon_names_list, max_poke_hor, max_poke_ver, pokemon_sheet, poke_width, poke_height, ffmpeg);


var pokesprites = new DirectoryInfo("extracted")
    .GetFiles()
    .Where(w => Path.GetExtension(w.FullName).Equals(".png", StringComparison.InvariantCultureIgnoreCase))
    .ToArray();

Console.WriteLine("hi");