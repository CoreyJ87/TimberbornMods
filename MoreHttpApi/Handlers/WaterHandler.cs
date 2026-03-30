namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class WaterHandler(
    EntityRegistry entityRegistry,
    ILoc t
) : IMoreHttpApiHandler
{
    public string Endpoint => "water";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetAllStreamGaugesAsync),
            1 => await context.HandleAsync(() => GetStreamGaugeAsync(parsedRequestPath)),
            _ => false,
        };

    async Task<HttpWaterInfo> GetAllStreamGaugesAsync()
    {
        var gauges = new List<HttpStreamGauge>();

        foreach (var entity in entityRegistry.Entities)
        {
            var gauge = entity.GetComponent<StreamGauge>();
            if (!gauge) continue;

            gauges.Add(ToHttpStreamGauge(entity, gauge));
        }

        return new([.. gauges]);
    }

    async Task<HttpStreamGauge> GetStreamGaugeAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
        {
            throw new StatusCodeException(400, "Invalid entity ID");
        }

        var entity = entityRegistry.GetEntity(id);
        if (!entity)
        {
            throw new StatusCodeException(404, "Entity not found");
        }

        var gauge = entity.GetComponent<StreamGauge>();
        if (!gauge)
        {
            throw new StatusCodeException(400, "Entity is not a stream gauge");
        }

        return ToHttpStreamGauge(entity, gauge);
    }

    HttpStreamGauge ToHttpStreamGauge(EntityComponent entity, StreamGauge gauge)
    {
        var name = entity.GetName(t);
        return new(
            entity.EntityId,
            name,
            gauge.WaterLevel,
            gauge.HighestWaterLevel,
            null
        );
    }
}
