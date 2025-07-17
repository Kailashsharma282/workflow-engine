namespace WorkflowEngine.Models;

public class WorkflowInstance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid DefinitionId { get; set; }
    public required string CurrentStateId { get; set; }
    public List<HistoryItem> History { get; set; } = new();
}

public class HistoryItem
{
    public required string ActionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}