namespace DeepSize.Core.Model;

public class DirectoryNode : Node
{
    public List<Node> Children { get; init; } = [];

    public void AddChild(Node node)
    {
        Children.Add(node);
        node.Parent = this;
        if(node is FileNode)
            AddSize(node.Size);
    }
    
    /// <summary>
    /// Adds the size to the node and all its parents.
    /// </summary>
    /// <param name="size"></param>
    public void AddSize(long size)
    {
        Size += size;
        Parent?.AddSize(size);
    }
}