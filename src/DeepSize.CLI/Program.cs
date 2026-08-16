using System.Text;
using DeepSize.CLI.Commands;
using Spectre.Console.Cli;

if(!Console.IsOutputRedirected)
    Console.OutputEncoding = Encoding.UTF8;

var app = new CommandApp<ScanCommand>();
await app.RunAsync(args);

