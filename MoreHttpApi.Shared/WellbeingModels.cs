namespace MoreHttpApi.Shared;

public record HttpNeedSummary(
    string Id,
    float AvgPoints,
    float MaxPoints,
    float AvgWellbeingContribution,
    bool Enabled,
    int SatisfiedCount,
    int TotalCount
);

public record HttpWellbeingInfo(
    float AverageWellbeing,
    int PopulationCount,
    HttpNeedSummary[] Needs
);
