namespace DeepSize.Core.Model;

public class ScanResult
{
    /// <summary>
    /// The root node of the file tree.
    /// </summary>
    public required DirectoryNode Root { get; init; }
    
    /// <summary>
    /// The absolute path of the directory being scanned.
    /// </summary>
    public string Path { get; init; } = string.Empty;
    
    /// <summary>
    /// The total number of all files and directories in the directory tree.
    /// </summary>
    public long TotalFiles { get; set; }
    
    /// <summary>
    /// The total number of directories in the directory tree.
    /// </summary>
    public long TotalDirectories { get; set; }
}