using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tomlyn;
using Tomlyn.Model;

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

        var error = TryGetRegistryToken(out var token);
        console.WriteLine($"error: {error}\n{token}");

        if (error != 0)
            return error;

        if (string.IsNullOrEmpty(token))
            return -1;

        var model = config.Exists
            ? Toml.ToModel(await File.ReadAllTextAsync(config.FullName, cancellationToken))
            : new TomlTable();
        console.WriteLine($"before: {Toml.FromModel(model)}");



        /// basically doing the following assignment: `model["npmAuth"][registry]["token"] = token`
        if (!model.ContainsKey("npmAuth"))
            model.Add("npmAuth", new TomlTable());
        var npmAuthTable = (TomlTable)model["npmAuth"];

        if (!npmAuthTable.ContainsKey(registry.ToString()))
            npmAuthTable.Add(registry.ToString(), new TomlTable());
        var registryTable = (TomlTable)npmAuthTable[registry.ToString()];

        if (registryTable.ContainsKey("token"))
            registryTable["token"] = token;
        else
            registryTable.Add("token", token);

        console.WriteLine($"after: {Toml.FromModel(model)}");
        await File.WriteAllTextAsync(
            config.FullName,
            Toml.FromModel(model),
            Encoding.Default,
            cancellationToken
        );

        return 0;
    }

    static int TryGetRegistryToken(out string token)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = @"gcloud";
        process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
        process.StartInfo.Arguments = @"auth print-access-token";

        object stdoutBuilderLocker = new object();
        object stderrBuilderLocker = new object();
        StringBuilder stdoutBuilder = new();
        StringBuilder stderrBuilder = new();
        string errorLog = string.Empty;

        process.OutputDataReceived += new DataReceivedEventHandler(
            (object sender, DataReceivedEventArgs args) =>
            {
                lock (stdoutBuilderLocker)
                {
                    stdoutBuilder.Append(args.Data?.Trim() ?? string.Empty);
                }
            }
        );

        process.ErrorDataReceived += new DataReceivedEventHandler(
            (object sender, DataReceivedEventArgs args) =>
            {
                var dataTrimmed = args.Data?.Trim() ?? string.Empty;
                lock (stderrBuilderLocker)
                {
                    stderrBuilder.Append(args.Data?.Trim() ?? string.Empty);
                }
            }
        );

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        var timeOut = 60;
        if (process.WaitForExit(timeOut * 1000))
        {
            Console.WriteLine(
                $"process {process.Id} has terminated with exit code {process.ExitCode}"
            );
        }
        else
        {
            Console.Error.WriteLine(
                $"process {process.Id} has timed out and will now be terminated"
            );
            process.Kill(entireProcessTree: true);
            Console.Error.WriteLine(
                $"process {process.Id} has been terminated with exit code {process.ExitCode}"
            );
        }

        if (stdoutBuilder.Length > 0)
            Console.WriteLine(stdoutBuilder.ToString());

        if (stderrBuilder.Length > 0)
            Console.Error.WriteLine(stderrBuilder.ToString());

        token = stdoutBuilder.ToString();
        return process.ExitCode;
    }
}
