using DeepSize.CLI.Render;
using DeepSize.Core.Logic;
using DeepSize.Core.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DeepSize.CLI.Commands;

public class ScanCommand : Command<ScanCommandSettings>
{
    protected override int Execute(CommandContext context, ScanCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var executeLive = !(settings.IsPlain || settings.IsNoProgress);
            
            if (settings.IsPlain)
                AnsiConsole.Console.Profile.Capabilities.ColorSystem = ColorSystem.NoColors;

            if(executeLive)
                ExecuteLive(settings, cancellationToken);
            else
                ExecutePlain(settings, cancellationToken);
        }
        catch(Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
        
        return 0;
    }

    private void ExecuteLive(ScanCommandSettings settings,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(settings.Path);
        var startTime = DateTime.Now;
        var result = new ScanResult
        {
            Path = fullPath,
            Root = new DirectoryNode
            {
                Name = fullPath
            }
        };

        TimeSpan totalElapsed;
        var scanningPhrase = ScanConsolePresenter.GetRandomPhrase();
        var spinner = new LiveSpinner(Spinner.Known.Dots, Style.Parse("gray bold"), new Text(scanningPhrase));
        var liveLayout = ScanConsolePresenter.BuildLiveLayout(result, TimeSpan.Zero, spinner);


        var scanner = new FileSystemScanner();
        var builder = new ScanResultBuilder();
        builder.Initialize(result);

        AnsiConsole.Live(liveLayout)
            .AutoClear(true)
            .Start(ctx =>
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
                        var newLayout = ScanConsolePresenter.BuildLiveLayout(result, totalElapsed, spinner);
                        ctx.UpdateTarget(newLayout);
                        lastUpdate = DateTime.Now;
                    }
                }
            });

        ScanConsolePresenter.PrintResultTree(result, settings.MaxDepth, cancellationToken);
        
        totalElapsed = DateTime.Now - startTime;
        var finalStatus = new Markup("[yellow]✓ Completed[/]");
        var finalLayout = ScanConsolePresenter.BuildLiveLayout(result, totalElapsed, finalStatus);
        
        AnsiConsole.Write(finalLayout);
    }

    private void ExecutePlain(ScanCommandSettings settings, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(settings.Path);
        var scanner = new FileSystemScanner();
        var result = scanner.ScanToResult(fullPath, cancellationToken);
        
        ScanConsolePresenter.PrintResultTree(result, settings.MaxDepth, cancellationToken);
    }
}