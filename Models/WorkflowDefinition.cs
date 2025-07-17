namespace WorkflowEngine.Models;

public class WorkflowDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required List<State> States { get; set; }
    public required List<WorkflowAction> Actions { get; set; }
}