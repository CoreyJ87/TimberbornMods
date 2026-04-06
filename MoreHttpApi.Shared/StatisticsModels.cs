namespace MoreHttpApi.Shared;

public record HttpStatistic(
    string Id,
    int Value
);

public record HttpStatisticsInfo(
    HttpStatistic[] Statistics
);
