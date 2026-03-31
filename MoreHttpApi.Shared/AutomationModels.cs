namespace MoreHttpApi.Shared;

public record HttpAutomationNode(
    Guid EntityId,
    string? Name,
    string ComponentType,
    bool IsOutputActive,
    Dictionary<string, object?> Properties
);

public record HttpAutomationInfo(
    HttpAutomationNode[] Nodes
);
