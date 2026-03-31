using HttpLever = MoreHttpApi.Shared.HttpLever;

namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class LeverHandler(
    EntityRegistry entityRegistry,
    ILoc t
) : IMoreHttpApiHandler
{
    public string Endpoint => "levers";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        var method = context.Request.HttpMethod.ToUpperInvariant();

        return parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetAllLeversAsync),
            1 when method == "GET" => await context.HandleAsync(() => GetLeverAsync(parsedRequestPath)),
            1 when method == "POST" => await context.HandleAsync(() => ToggleLeverAsync(parsedRequestPath)),
            _ => false,
        };
    }

    async Task<HttpLeverList> GetAllLeversAsync()
    {
        var levers = new List<HttpLever>();

        foreach (var entity in entityRegistry.Entities)
        {
            var lever = entity.GetComponent<Lever>();
            if (!lever) continue;

            levers.Add(ToHttpLever(entity, lever));
        }

        return new([.. levers]);
    }

    async Task<HttpLever> GetLeverAsync(ParsedRequestPath parsedRequestPath)
    {
        var (entity, lever) = FindLever(parsedRequestPath);
        return ToHttpLever(entity, lever);
    }

    async Task<HttpLever> ToggleLeverAsync(ParsedRequestPath parsedRequestPath)
    {
        var (entity, lever) = FindLever(parsedRequestPath);
        lever.Toggle();
        return ToHttpLever(entity, lever);
    }

    (EntityComponent entity, Lever lever) FindLever(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
            throw new StatusCodeException(400, "Invalid entity ID");

        var entity = entityRegistry.GetEntity(id);
        if (!entity)
            throw new StatusCodeException(404, "Entity not found");

        var lever = entity.GetComponent<Lever>();
        if (!lever)
            throw new StatusCodeException(400, "Entity is not a lever");

        return (entity, lever);
    }

    HttpLever ToHttpLever(EntityComponent entity, Lever lever)
    {
        return new(
            entity.EntityId,
            entity.GetName(t),
            lever.IsOn,
            lever.IsSpringReturn,
            lever.IsPinned
        );
    }
}
