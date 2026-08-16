using DeepSize.Core.Logic.Events;
using DeepSize.Core.Model;

namespace DeepSize.Core.Logic;

/// <summary>
/// Scans a directory for files and directories.
/// </summary>
public interface IFileSystemScanner
{
    /// <summary>
    /// Scans the given path for files and directories.
    /// </summary>
    /// <param name="path">Absolute path of scan target directory</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IEnumerable<ScanEvent> Scan(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans the given path for files and directories.
    /// </summary>
    /// <param name="path">Absolute path of scan target directory</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public ScanResult ScanToResult(string path, CancellationToken cancellationToken = default);
}

/// <summary>
/// Scans a directory for files and directories.
/// </summary>
public class FileSystemScanner : IFileSystemScanner
{
    public IEnumerable<ScanEvent> Scan(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ScanError? dirScanError = null;
        IEnumerable<string> directories = [];
        try
        {
            directories = Directory.EnumerateDirectories(path);
        }
        catch (UnauthorizedAccessException exception)
        {
            dirScanError = new ScanError(path, exception.Message);
        }
        catch (IOException exception)
        {
            dirScanError = new ScanError(path, exception.Message);
        }

        if (dirScanError is null)
        {
            foreach (var directory in directories)
            {
                yield return new DirectoryStarted(directory);
                foreach (var dirScanEvent in Scan(directory, cancellationToken))
                {
                    yield return dirScanEvent;
                }

                yield return new DirectoryCompleted();
            }
        }
        else
        {
            yield return new ScanError(path, dirScanError.Message);
        }

        ScanError? fileScanError = null;
        IEnumerable<string> files = [];
        try
        {
            files = Directory.EnumerateFiles(path);
        }
        catch (UnauthorizedAccessException exception)
        {
            fileScanError = new ScanError(path, exception.Message);
        }
        catch (IOException exception)
        {
            fileScanError = new ScanError(path, exception.Message);
        }

        if (fileScanError is null)
        {
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                yield return new FileFound(file, fileInfo.Length);
            }
        }
        else
        {
            yield return new ScanError(path, fileScanError.Message);
        }
    }

    public ScanResult ScanToResult(
        string path,
        CancellationToken cancellationToken = default)
    {
        var root = new DirectoryNode
        {
            Name = path
        };
        var result = new ScanResult { Path = path, Root = root };
        
        var builder = new ScanResultBuilder();
        builder.Initialize(result);
        
        foreach(var scanEvent in Scan(path, cancellationToken))
        {
            builder.Apply(result, scanEvent);
        }
        
        return result;
    }
}