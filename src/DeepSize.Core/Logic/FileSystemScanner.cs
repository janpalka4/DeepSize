using DeepSize.Core.Logic.Events;
using DeepSize.Core.Model;

namespace DeepSize.Core.Logic;

public interface IFileSystemScanner
{
    IAsyncEnumerable<ScanEvent> ScanAsync(
        string path,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Scans a directory for files and directories.
/// </summary>
public class Scanner
{
    private readonly string _path;
    
    /// <summary>
    /// Scans the given path for files and directories.
    /// </summary>
    /// <param name="path">Absolute path to the scanned directory</param>
    public Scanner(string path)
    {
        _path = path;
    }

    public IAsyncEnumerable<> ScanAsync()
    {
        
    }
}