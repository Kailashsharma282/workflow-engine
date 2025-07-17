using WorkflowEngine.Models;
using System.Diagnostics;

namespace WorkflowEngine.Services;

public interface IWorkflowService
{
    WorkflowDefinition CreateDefinition(WorkflowDefinition definition);
    WorkflowDefinition? GetDefinition(Guid id);
    IEnumerable<WorkflowDefinition> GetAllDefinitions();
    WorkflowInstance CreateInstance(Guid definitionId);
    WorkflowInstance? GetInstance(Guid id);
    IEnumerable<WorkflowInstance> GetAllInstances();
    WorkflowInstance ExecuteAction(Guid instanceId, string actionId);
}

public class WorkflowService : IWorkflowService
{
    private readonly Dictionary<Guid, WorkflowDefinition> _definitions = new();
    private readonly Dictionary<Guid, WorkflowInstance> _instances = new();

    public WorkflowDefinition CreateDefinition(WorkflowDefinition definition)
    {
        // Validate definition
        if (_definitions.Values.Any(d => d.Name == definition.Name))
        {
            throw new ArgumentException("Workflow definition name must be unique");
        }

        if (definition.States.Count(s => s.IsInitial) != 1)
        {
            throw new ArgumentException("Workflow must have exactly one initial state");
        }

        if (definition.States.All(s => !s.IsInitial))
        {
            throw new ArgumentException("Workflow must have at least one initial state");
        }

        if (definition.States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
        {
            throw new ArgumentException("Duplicate state IDs detected");
        }

        if (definition.Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))
        {
            throw new ArgumentException("Duplicate action IDs detected");
        }

        _definitions[definition.Id] = definition;
        return definition;
    }

    public WorkflowDefinition? GetDefinition(Guid id)
    {
        return _definitions.TryGetValue(id, out var definition) ? definition : null;
    }

    public IEnumerable<WorkflowDefinition> GetAllDefinitions()
    {
        return _definitions.Values;
    }

    public WorkflowInstance CreateInstance(Guid definitionId)
    {
        var definition = GetDefinition(definitionId) ??
            throw new KeyNotFoundException($"Workflow definition {definitionId} not found");

        var initialState = definition.States.Single(s => s.IsInitial);

        var instance = new WorkflowInstance
        {
            DefinitionId = definitionId,
            CurrentStateId = initialState.Id
        };

        _instances[instance.Id] = instance;
        return instance;
    }

    public WorkflowInstance? GetInstance(Guid id)
    {
        return _instances.TryGetValue(id, out var instance) ? instance : null;
    }

    public IEnumerable<WorkflowInstance> GetAllInstances()
    {
        return _instances.Values;
    }

    public WorkflowInstance ExecuteAction(Guid instanceId, string actionId)
    {
        var instance = GetInstance(instanceId) ??
            throw new KeyNotFoundException($"Instance {instanceId} not found");

        var definition = GetDefinition(instance.DefinitionId) ??
            throw new InvalidOperationException($"Definition for instance {instanceId} not found");

        var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId) ??
            throw new InvalidOperationException($"Current state {instance.CurrentStateId} not found in definition");

        if (currentState.IsFinal)
        {
            throw new InvalidOperationException("Cannot execute actions on a final state");
        }

        var action = definition.Actions.FirstOrDefault(a => a.Id == actionId) ??
            throw new KeyNotFoundException($"Action {actionId} not found in definition");

        if (!action.Enabled)
        {
            throw new InvalidOperationException($"Action {actionId} is disabled");
        }

        if (!action.FromStates.Contains(instance.CurrentStateId))
        {
            throw new InvalidOperationException(
                $"Action {actionId} cannot be executed from state {instance.CurrentStateId}");
        }

        var targetState = definition.States.FirstOrDefault(s => s.Id == action.ToState) ??
            throw new InvalidOperationException($"Target state {action.ToState} not found");

        // Execute the transition
        instance.CurrentStateId = targetState.Id;
        instance.History.Add(new HistoryItem
        {
            ActionId = actionId,
            Timestamp = DateTime.UtcNow
        });

        return instance;
    }
}