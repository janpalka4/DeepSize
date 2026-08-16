namespace DeepSize.Core.Logic.Events;

public sealed record DirectoryStarted(string Path) : ScanEvent;