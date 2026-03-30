namespace MoreHttpApi.Shared;

public record HttpMechanicalNode(
    Guid EntityId,
    string? Name,
    int PowerOutput,
    int PowerInput,
    bool IsActive
);

public record HttpPowerGrid(
    int CurrentPowerSupply,
    int CurrentPowerDemand,
    HttpMechanicalNode[] Nodes
);

public record HttpPowerInfo(
    HttpPowerGrid[] Grids
);
