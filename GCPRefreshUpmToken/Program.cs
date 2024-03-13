using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

namespace GCPRefreshUpmToken;

#nullable enable

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(
            "Refreshes `.upmconfig.toml` with a new token from Google Cloud."
        )
        {
            new Option<Uri>(
                aliases: ["--registry", "-r"],
                description: "URL of the Google Cloud Artifact Registry"
            ),
            new Option<FileInfo>(aliases: ["--config", "-c"], description: "config file"),
        };
        rootCommand.Handler = CommandHandler.Create(Run);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task<int> Run(
        Uri registry,
        FileInfo config,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        console.WriteLine($"hello: {registry} {config}");
        return 1;
    }
}
