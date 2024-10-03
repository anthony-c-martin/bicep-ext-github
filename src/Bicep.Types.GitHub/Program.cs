// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommandLine;

namespace Bicep.Types.Github;

public static class Program
{
    internal class Options
    {
        [Option("outdir", Required = true, HelpText = "The path to output types to")]
        public string? Outdir { get; set; }
    }

    public static void Main(string[] args)
        => Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(Main);

    private static void Main(Options options)
    {
        var types = TypeGenerator.GenerateTypes();
        var outdir = Path.GetFullPath(options.Outdir!);

        Directory.CreateDirectory(outdir);
        foreach (var kvp in types)
        {
            var filePath = Path.Combine(outdir, kvp.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            File.WriteAllText(filePath, kvp.Value);
        }
    }
}