namespace DeepSize.Core.Logic.Events;

public sealed record ScanError(string Path, string Message) : ScanEvent;