using DeepSize.CLI.Render;
using DeepSize.Core.Logic;
using DeepSize.Core.Model;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace DeepSize.CLI.Commands;

public class ScanAsyncCommand : Command<ScanCommandSettings>
{
    private static readonly string[] ScanningPhrases =
    [
        "Mapping your disk...",
        "Measuring the digital universe...",
        "Hunting for large files...",
        "Scanning...",
        "Exploring directories...",
        "Counting bytes...",
        "Mapping the filesystem...",
        "Looking for space hogs...",
        "Digging through directories..."
    ];

    private async Task ExecuteLive(ScanCommandSettings settings,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(settings.Path);
        var startTime = DateTime.Now;

        TimeSpan totalElapsed;
        var randomPhraseIndex = new Random().Next(ScanningPhrases.Length);
        var scanningPhrase = ScanningPhrases[randomPhraseIndex];
        var panelTitle = fullPath;
        var spinner = new LiveSpinner(Spinner.Known.Dots, Style.Parse("gray bold"), new Text(scanningPhrase));
        var padder = new Padder(new Text(""), new Padding(0, 1, 0, 0));
        var liveLayout = new Rows(
            new Panel(panelTitle),
            padder,
            spinner
        );

        var result = new ScanResult
        {
            Path = fullPath,
            Root = new DirectoryNode
            {
                Name = fullPath
            }
        };

        var scanner = new FileSystemScanner();
        var builder = new ScanResultBuilder();
        builder.Initialize(result);

        AnsiConsole.Live(liveLayout)
            .AutoClear(true)
            .Start(ctx =>
            {
                try
                {
                    var updateInterval = TimeSpan.FromMilliseconds(100);
                    var lastUpdate = DateTime.Now;
                    foreach (var scanEvent in scanner.Scan(fullPath, cancellationToken))
                    {
                        builder.Apply(result, scanEvent);

                        var elapsed = DateTime.Now - lastUpdate;
                        if (elapsed > updateInterval)
                        {
                            totalElapsed = DateTime.Now - startTime;
                            var newLayout = MakeNewLiveLayout(totalElapsed,
                                result.TotalFiles,
                                result.TotalDirectories,
                                result.Root.Size,
                                spinner);
                            ctx.UpdateTarget(newLayout);
                            lastUpdate = DateTime.Now;
                        }
                    }

                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    return Task.FromException(exception);
                }
            });

        PrintResult(result, settings.MaxDepth);
        totalElapsed = DateTime.Now - startTime;
        var finalLayout = MakeNewLiveLayout(totalElapsed,
            result.TotalFiles,
            result.TotalDirectories,
            result.Root.Size,
            new Markup("[yellow]✓ Completed[/]"));
        AnsiConsole.Write(finalLayout);
    }

    private async Task ExecutePlain(ScanCommandSettings settings, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(settings.Path);
        var scanner = new FileSystemScanner();
        var result = await scanner.ScanToResult(fullPath, cancellationToken);
        PrintResult(result, settings.MaxDepth);
    }

    private void PrintResult(ScanResult result, int? maxDepth)
    {
        var tree = new Tree(result.Path);
        AddTreeNode(result.Root,tree, maxDepth: maxDepth ?? int.MaxValue);
        
        AnsiConsole.Write(tree);
    }
    
    private IRenderable MakeNewLiveLayout(TimeSpan totalElapsed, long totalFiles, long totalDirectories, long totalSize, IRenderable statusRenderable)
    {
        return new Rows(
            new Panel(new Rows(
                new Markup($"Total files:         [gray]{totalFiles:N0}[/]"),
                new Markup($"Total directories:   [gray]{totalDirectories:N0}[/]"),
                new Markup($"Total size:          [cyan]{Utils.FormatBytes(totalSize)}[/]"),
                new Padder(new Text(""), new Padding(0, 1, 0, 0)),
                new Markup(
                    $"Elapsed:             [gray]{Utils.TimeSpanToReadableString(totalElapsed)}[/]")
            )),
            statusRenderable);
    }

    /// <summary>
    /// Creates a tree node from the given node that represents the node in the file tree.
    /// </summary>
    /// <param name="node">Filesystem tree node</param>
    /// <param name="parent"></param>
    /// <param name="depth">Current depth</param>
    /// <param name="maxDepth">Maximum depth of tree structure</param>
    /// <returns></returns>
    private void AddTreeNode(Node node, IHasTreeNodes parent, int depth = 0, int maxDepth = int.MaxValue)
    {
        if(depth >= maxDepth)
            return;
        
        switch (node)
        {
            case FileNode fileNode:
                parent.AddNode($"[gray]{Markup.Escape(fileNode.Name)} ([/][cyan]{Markup.Escape(Utils.FormatBytes(fileNode.Size))}[/][gray])[/]");
                break;
            case DirectoryNode directoryNode:
                var treeNode = parent.AddNode($"[blue]{Markup.Escape(directoryNode.Name)} ([/][cyan]{Markup.Escape(Utils.FormatBytes(directoryNode.Size))}[/][gray])[/]");
                var newDepth = depth + 1;
                directoryNode.Children.OrderByDescending(x => x.Size).ToList().ForEach(child => AddTreeNode(child, treeNode, newDepth, maxDepth));
                break;
            case ErrorNode errorNode:
                parent.AddNode($"[red]{Markup.Escape(errorNode.Name)}: {Markup.Escape(errorNode.Message)}[/]");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(node), node, "Value is not a valid node type.");
        }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, ScanCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var executeLive = !(settings.IsPlain || settings.IsNoProgress);
            if (settings.IsPlain)
                AnsiConsole.Profile.Capabilities.ColorSystem = ColorSystem.NoColors;
            
            if(executeLive)
                await ExecuteLive(settings, cancellationToken);
            else
                await ExecutePlain(settings, cancellationToken);
        }
        catch(Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
        
        return 0;
    }
}