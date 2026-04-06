namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class StatisticsHandler(
    IncrementalStatisticCollector statisticCollector
) : IMoreHttpApiHandler
{
    static readonly string[] AllStatisticIds =
    [
        StatisticIds.BeaversBorn,
        StatisticIds.BeaversExploded,
        StatisticIds.BotsManufactured,
        StatisticIds.ChippedTeeth,
        StatisticIds.DaysPassed,
        StatisticIds.DynamiteDetonated,
        StatisticIds.TailsPainted,
        StatisticIds.TreesCut,
        StatisticIds.WaterConsumed,
    ];

    public string Endpoint => "statistics";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetStatisticsAsync),
            _ => false,
        };

    async Task<HttpStatisticsInfo> GetStatisticsAsync()
    {
        var statistics = AllStatisticIds
            .Select(id => new HttpStatistic(id, statisticCollector.GetOrDefault(id)))
            .ToArray();

        return new(statistics);
    }
}
