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
        var counted = new HashSet<Inventory>();

        foreach (var entity in entityRegistry.Entities)
        {
            var stockpile = entity.GetComponent<Stockpile>();
            if (stockpile)
            {
                AddInventory(stockpile.Inventory, counted, totals);
                continue;
            }

            var manufactory = entity.GetComponent<Manufactory>();
            if (manufactory)
            {
                AddInventory(manufactory.Inventory, counted, totals);
                continue;
            }

            var outputInv = entity.GetComponent<SimpleOutputInventory>();
            if (outputInv)
            {
                AddInventory(outputInv.Inventory, counted, totals);
                continue;
            }

            var consumer = entity.GetComponent<GoodConsumingBuilding>();
            if (consumer)
            {
                AddInventory(consumer.Inventory, counted, totals);
                continue;
            }

            var breedingPod = entity.GetComponent<BreedingPod>();
            if (breedingPod)
            {
                AddInventory(breedingPod.Inventory, counted, totals);
                continue;
            }

            var wonderInv = entity.GetComponent<WonderInventory>();
            if (wonderInv)
            {
                AddInventory(wonderInv.Inventory, counted, totals);
            }
        }

        var totalGoods = totals
            .Select(kv => new HttpGoodSummary(kv.Key, kv.Value))
            .OrderByDescending(g => g.Amount)
            .ToArray();

        return new(totalGoods, []);
    }

    static void AddInventory(Inventory? inv, HashSet<Inventory> counted, Dictionary<string, int> totals)
    {
        if (inv == null || !counted.Add(inv)) return;

        foreach (var stock in inv.Stock)
        {
            if (stock.Amount <= 0) continue;
            totals.TryGetValue(stock.GoodId, out var current);
            totals[stock.GoodId] = current + stock.Amount;
        }
    }
}
