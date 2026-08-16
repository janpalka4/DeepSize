namespace DeepSize.Core.Logic.Events;

public sealed record FileFound(string Path, long Size) : ScanEvent;