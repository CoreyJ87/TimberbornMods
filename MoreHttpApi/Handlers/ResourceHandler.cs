namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class ResourceHandler(
    EntityRegistry entityRegistry
) : IMoreHttpApiHandler
{
    public string Endpoint => "resources";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetResourcesAsync),
            _ => false,
        };

    async Task<HttpResourceInfo> GetResourcesAsync()
    {
        Dictionary<string, int> totals = [];
        var inventories = new List<Inventory>();

        foreach (var entity in entityRegistry.Entities)
        {
            inventories.Clear();
            entity.GetComponents(inventories);

            foreach (var inventory in inventories)
            {
                foreach (var stock in inventory.Stock)
                {
                    var goodId = stock.GoodId;
                    var amount = stock.Amount;
                    if (amount <= 0) continue;

                    totals.TryGetValue(goodId, out var current);
                    totals[goodId] = current + amount;
                }
            }
        }

        var totalGoods = totals
            .Select(kv => new HttpGoodSummary(kv.Key, kv.Value))
            .OrderByDescending(g => g.Amount)
            .ToArray();

        return new(totalGoods, []);
    }
}
