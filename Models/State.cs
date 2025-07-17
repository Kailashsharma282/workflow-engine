namespace WorkflowEngine.Models;

public class State
{
    public required string Id { get; set; }
    public string? Description { get; set; }
    public bool IsInitial { get; set; }
    public bool IsFinal { get; set; }
    public bool Enabled { get; set; } = true;
}