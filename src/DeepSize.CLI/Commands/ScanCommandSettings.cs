using System.ComponentModel;
using Spectre.Console.Cli;

namespace DeepSize.CLI.Commands;

public class ScanCommandSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("The path of the directory to scan.")]
    [DefaultValue("./")]
    public string Path { get; set; } = "./";
    
    [CommandOption("--plain")]
    [Description("Prints the result in plain text.")]
    public bool IsPlain { get; set; }
    
    [CommandOption("--no-progress")]
    [Description("Disables the progress view.")]
    public bool IsNoProgress { get; set; }
    
    [CommandOption("-d|--depth <depth>")]
    [Description("Maximum depth of the directory tree to print.")]
    public int? MaxDepth { get; set; }
}