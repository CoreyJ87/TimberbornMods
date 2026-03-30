namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class PowerHandler(
    EntityRegistry entityRegistry,
    ILoc t
) : IMoreHttpApiHandler
{
    public string Endpoint => "power";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetPowerAsync),
            _ => false,
        };

    async Task<HttpPowerInfo> GetPowerAsync()
    {
        List<HttpMechanicalNode> nodes = [];
        int totalSupply = 0;
        int totalDemand = 0;

        foreach (var entity in entityRegistry.Entities)
        {
            var node = entity.GetComponent<MechanicalNode>();
            if (!node) continue;

            var active = node.Active;
            var output = node.PowerOutput;
            var input = node.PowerInput;

            if (active)
            {
                totalSupply += output;
                totalDemand += input;
            }

            var name = entity.GetName(t);
            nodes.Add(new HttpMechanicalNode(
                entity.EntityId,
                name,
                output,
                input,
                active
            ));
        }

        return new([new HttpPowerGrid(totalSupply, totalDemand, [.. nodes])]);
    }
}
