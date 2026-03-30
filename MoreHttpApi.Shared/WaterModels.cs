namespace MoreHttpApi.Shared;

public record HttpStreamGauge(
    Guid EntityId,
    string? Name,
    float WaterLevel,
    float HighestWaterLevel,
    float? LowestWaterLevel
);

public record HttpWaterInfo(
    HttpStreamGauge[] StreamGauges
);
