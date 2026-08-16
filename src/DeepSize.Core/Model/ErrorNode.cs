namespace DeepSize.Core.Model;

public class ErrorNode : Node
{
    public string Message { get; set; }
    
    public ErrorNode(string message)
    {
        Message = message;
    }
}