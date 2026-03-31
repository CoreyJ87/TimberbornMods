namespace MoreHttpApi.Shared;

public record HttpStreamGauge(
    Guid EntityId,
    string? Name,
    float WaterLevel,
    float HighestWaterLevel,
    float? LowestWaterLevel
);

public record HttpDepthSensor(
    Guid EntityId,
    string? Name,
    float? CurrentDepth,
    float? Threshold,
    string? ThresholdMode,
    bool IsActive
);

public record HttpWaterInfo(
    HttpStreamGauge[] StreamGauges,
    HttpDepthSensor[] DepthSensors
);
