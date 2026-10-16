using Mp3ToWav;

const string Usage = """
    Usage: Mp3ToWav <input.mp3 | folder> [more inputs...] [-o <outputFolder>] [-f]

      input        One or more .mp3 files or folders containing .mp3 files.
      -o, --out    Output folder (default: next to each input file).
      -f, --force  Overwrite existing .wav files.

      For example: dotnet run -- C:\Music -o C:\Music\Wavs -f
      This program only searches the top level of the folder and will not touch subfolders.
    """;

var inputs = new List<string>();
string? outDir = null;
bool force = false;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-h" or "--help" or "/?":
            Console.WriteLine(Usage);
            return 0;
        case "-o" or "--out":
            if (++i >= args.Length) { Console.Error.WriteLine("Missing value for -o."); return 1; }
            outDir = args[i];
            break;
        case "-f" or "--force":
            force = true;
            break;
        default:
            inputs.Add(args[i]);
            break;
    }
}

if (inputs.Count == 0)
{
    Console.Error.WriteLine(Usage);
    return 1;
}

var files = new List<string>();
var failures = new List<(string Input, string Reason)>();
foreach (var input in inputs)
{
    if (Directory.Exists(input))
        files.AddRange(Directory.EnumerateFiles(input, "*.mp3", SearchOption.TopDirectoryOnly));
    else if (File.Exists(input))
    {
        if (string.Equals(Path.GetExtension(input), ".mp3", StringComparison.OrdinalIgnoreCase))
            files.Add(input);
        else
        {
            const string reason = "Unsupported file type (expected .mp3).";
            Console.Error.WriteLine($"{input}: {reason}");
            failures.Add((input, reason));
        }
    }
    else
    {
        const string reason = "Input not found.";
        failures.Add((input, reason));
        Console.Error.WriteLine($"Not found: {input}");
    }
}

if (outDir is not null)
    Directory.CreateDirectory(outDir);

foreach (var mp3 in files)
{
    string wav = Path.Combine(outDir ?? Path.GetDirectoryName(Path.GetFullPath(mp3))!,
                              Path.GetFileNameWithoutExtension(mp3) + ".wav");
    if (File.Exists(wav) && !force)
    {
        Console.WriteLine($"Skipped (exists, use -f): {wav}");
        continue;
    }

    try
    {
        Mp3Converter.Convert(mp3, wav);
        Console.WriteLine($"{mp3} -> {wav}");
    }
    catch (Exception ex)
    {
        failures.Add((mp3, ex.Message));
        Console.Error.WriteLine($"Failed: {mp3}: {ex.Message}");
        try { File.Delete(wav); } catch { }
    }
}

if (failures.Count > 0)
{
    Console.Error.WriteLine($"Failed inputs ({failures.Count}):");
    foreach (var (input, reason) in failures)
        Console.Error.WriteLine($"  {input}: {reason}");
}

return failures.Count == 0 && files.Count > 0 ? 0 : 1;
