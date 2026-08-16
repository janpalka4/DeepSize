using DeepSize.Core.Logic.Events;
using DeepSize.Core.Model;

namespace DeepSize.Core.Logic;

public interface IScanResultBuilder
{
    public void Initialize(ScanResult result);
    public void Apply(ScanResult result, ScanEvent scanEvent);
}

public sealed class ScanResultBuilder : IScanResultBuilder
{
    private readonly Stack<DirectoryNode> _directories = [];

    public void Initialize(ScanResult result)
    {
        _directories.Clear();
        _directories.Push(result.Root);
    }

    public void Apply(ScanResult result, ScanEvent scanEvent)
    {
        switch (scanEvent)
        {
            case DirectoryStarted directoryStarted:
                var parentDirectoryNode = _directories.Peek();
                var directoryNode = new DirectoryNode
                {
                    Name = Path.GetFileName(directoryStarted.Path)
                };
                parentDirectoryNode.AddChild(directoryNode);
                _directories.Push(directoryNode);
                result.TotalDirectories++;
                break;
            case DirectoryCompleted directoryCompleted:
                _directories.Pop();
                break;
            case FileFound fileFound:
                var fileParentDirectoryNode = _directories.Peek();
                var fileNode = new FileNode
                {
                    Name = Path.GetFileName(fileFound.Path),
                    Size = fileFound.Size
                };
                fileParentDirectoryNode.AddChild(fileNode);
                result.TotalFiles++;
                break;
            case ScanError scanError:
                var errorNode = new ErrorNode(scanError.Message);
                var errorParentDirectoryNode = _directories.Peek();
                errorParentDirectoryNode.AddChild(errorNode);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ScanEvent), scanEvent.GetType().Name, "Value is not a valid scan event type.");
        }
    }
}