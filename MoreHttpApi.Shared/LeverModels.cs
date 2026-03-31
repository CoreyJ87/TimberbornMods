namespace MoreHttpApi.Shared;

public record HttpLever(
    Guid EntityId,
    string? Name,
    bool IsOn,
    bool IsSpringReturn,
    bool IsPinned
);

public record HttpLeverList(
    HttpLever[] Levers
);
