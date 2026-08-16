using DeepSize.Core.Model;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DeepSize.CLI.Render;

public static class ScanConsolePresenter
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

    public static string GetRandomPhrase()
    {
        return ScanningPhrases[Random.Shared.Next(ScanningPhrases.Length)];
    }

    /// <summary>
    /// Makes a live layout for the scanning progress.
    /// </summary>
    public static IRenderable BuildLiveLayout(ScanResult result, TimeSpan elapsed, IRenderable statusRenderable)
    {
        return new Rows(
            new Panel(new Rows(
                new Markup($"Total files:         [gray]{result.TotalFiles:N0}[/]"),
                new Markup($"Total directories:   [gray]{result.TotalDirectories:N0}[/]"),
                new Markup($"Total size:          [cyan]{Utils.FormatBytes(result.Root.Size)}[/]"),
                new Padder(new Text(""), new Padding(0, 1, 0, 0)),
                new Markup($"Elapsed:             [gray]{Utils.TimeSpanToReadableString(elapsed)}[/]")
            )),
            statusRenderable);
    }

    /// <summary>
    /// Prints the final result tree to the console.
    /// </summary>
    public static void PrintResultTree(ScanResult result, int? maxDepth, CancellationToken cancellationToken = default)
    {
        var tree = new Tree(result.Path);
        AddTreeNode(result.Root, tree, depth: 0, maxDepth: maxDepth ?? int.MaxValue, cancellationToken: cancellationToken);
        AnsiConsole.Write(tree);
    }

    private static void AddTreeNode(Node node, IHasTreeNodes parent, int depth, int maxDepth, CancellationToken cancellationToken = default)
    {
        if (depth >= maxDepth) return;
        
        cancellationToken.ThrowIfCancellationRequested();
        
        switch (node)
        {
            case FileNode fileNode:
                parent.AddNode($"[gray]{Markup.Escape(fileNode.Name)} ([/][cyan]{Markup.Escape(Utils.FormatBytes(fileNode.Size))}[/][gray])[/]");
                break;
            case DirectoryNode directoryNode:
                var treeNode = parent.AddNode($"[blue]{Markup.Escape(directoryNode.Name)} ([/][cyan]{Markup.Escape(Utils.FormatBytes(directoryNode.Size))}[/][gray])[/]");
                var newDepth = depth + 1;
                
                var sortedChildren = directoryNode.Children.OrderByDescending(x => x.Size);
                foreach (var child in sortedChildren)
                {
                    AddTreeNode(child, treeNode, newDepth, maxDepth);
                }
                break;
            case ErrorNode errorNode:
                parent.AddNode($"[red]{Markup.Escape(errorNode.Name)}: {Markup.Escape(errorNode.Message)}[/]");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(node), node, "Value is not a valid node type.");
        }
    }
}