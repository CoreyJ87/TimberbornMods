namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class WellbeingHandler(
    BeaverPopulation beavers
) : IMoreHttpApiHandler
{
    public string Endpoint => "wellbeing";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => await context.HandleAsync(GetWellbeingAsync);

    async Task<HttpWellbeingInfo> GetWellbeingAsync()
    {
        Dictionary<string, (float pointsSum, float maxPointsSum, float wellbeingSum, int enabledCount, int satisfiedCount, int total)> needs = [];

        float totalWellbeing = 0;
        int popCount = 0;

        void ProcessCharacter(BaseComponent comp)
        {
            var tracker = comp.GetComponent<WellbeingTracker>();
            if (!tracker) return;

            totalWellbeing += tracker.Wellbeing;
            popCount++;

            var needMan = comp.GetComponent<NeedManager>();
            if (!needMan) return;

            foreach (var need in needMan._needs.AllNeeds)
            {
                var id = need.NeedSpec.Id;
                if (!needs.ContainsKey(id))
                    needs[id] = (0, 0, 0, 0, 0, 0);

                var cur = needs[id];
                cur.total++;
                cur.pointsSum += need.Points;
                cur.maxPointsSum += need.NeedSpec.MaxPoints;
                cur.wellbeingSum += need.Wellbeing;
                if (need.Enabled) cur.enabledCount++;
                if (need.Points > 0) cur.satisfiedCount++;
                needs[id] = cur;
            }
        }

        foreach (var adult in beavers._beaverCollection.Adults)
            ProcessCharacter(adult);

        foreach (var child in beavers._beaverCollection.Children)
            ProcessCharacter(child);

        var avgWellbeing = popCount > 0 ? totalWellbeing / popCount : 0;

        var needSummaries = needs
            .Select(kv =>
            {
                var (pointsSum, maxPointsSum, wellbeingSum, enabledCount, satisfiedCount, total) = kv.Value;
                return new HttpNeedSummary(
                    kv.Key,
                    total > 0 ? pointsSum / total : 0,
                    total > 0 ? maxPointsSum / total : 0,
                    total > 0 ? wellbeingSum / total : 0,
                    enabledCount > 0,
                    satisfiedCount,
                    total
                );
            })
            .OrderByDescending(n => n.AvgWellbeingContribution)
            .ToArray();

        return new(avgWellbeing, popCount, needSummaries);
    }
}
