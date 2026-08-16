namespace DeepSize.Core.Model;

/// <summary>
/// Represents a node in the file tree.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// The name of the node (e.g. "file.txt")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The parent directory node of the node.
    /// </summary>
    public DirectoryNode? Parent { get; internal set; }
    
    public long Size { get; set; }
    
    /// <summary>
    /// Gets the relative path of the node to the root.
    /// </summary>
    /// <returns></returns>
    public string GetPath() =>
        Parent is null ? Name : $"{Parent.GetPath()}/{Name}";
}