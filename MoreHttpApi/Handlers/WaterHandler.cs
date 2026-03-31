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
            0 => await context.HandleAsync(GetAllWaterDataAsync),
            1 => await context.HandleAsync(() => GetStreamGaugeAsync(parsedRequestPath)),
            _ => false,
        };

    async Task<HttpWaterInfo> GetAllWaterDataAsync()
    {
        var gauges = new List<HttpStreamGauge>();
        var sensors = new List<HttpDepthSensor>();

        foreach (var entity in entityRegistry.Entities)
        {
            var gauge = entity.GetComponent<StreamGauge>();
            if (gauge)
                gauges.Add(ToHttpStreamGauge(entity, gauge));

            var depthSensor = entity.GetComponent<DepthSensor>();
            if (depthSensor)
                sensors.Add(ToHttpDepthSensor(entity, depthSensor));
        }

        return new([.. gauges], [.. sensors]);
    }

    async Task<HttpStreamGauge> GetStreamGaugeAsync(ParsedRequestPath parsedRequestPath)
    {
        if (!Guid.TryParse(parsedRequestPath.RemainingSegment[0], out var id))
            throw new StatusCodeException(400, "Invalid entity ID");

        var entity = entityRegistry.GetEntity(id);
        if (!entity)
            throw new StatusCodeException(404, "Entity not found");

        var gauge = entity.GetComponent<StreamGauge>();
        if (!gauge)
            throw new StatusCodeException(400, "Entity is not a stream gauge");

        return ToHttpStreamGauge(entity, gauge);
    }

    HttpStreamGauge ToHttpStreamGauge(EntityComponent entity, StreamGauge gauge)
        => new(entity.EntityId, entity.GetName(t), gauge.WaterLevel, gauge.HighestWaterLevel, null);

    HttpDepthSensor ToHttpDepthSensor(EntityComponent entity, DepthSensor sensor)
    {
        float? depth = TryGetFloat(sensor, "CurrentDepth") ?? TryGetFloat(sensor, "_currentDepth");
        float? threshold = TryGetFloat(sensor, "Threshold") ?? TryGetFloat(sensor, "_threshold");
        string? mode = TryGetString(sensor, "ThresholdMode") ?? TryGetString(sensor, "_thresholdMode");
        bool active = TryGetBool(sensor, "IsActive") ?? TryGetBool(sensor, "IsOn") ?? false;

        return new(entity.EntityId, entity.GetName(t), depth, threshold, mode, active);
    }

    static float? TryGetFloat(object obj, string name)
    {
        try
        {
            var type = obj.GetType();
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return Convert.ToSingle(prop.GetValue(obj));
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return Convert.ToSingle(field.GetValue(obj));
        }
        catch { }
        return null;
    }

    static bool? TryGetBool(object obj, string name)
    {
        try
        {
            var type = obj.GetType();
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop?.PropertyType == typeof(bool)) return (bool)prop.GetValue(obj);
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field?.FieldType == typeof(bool)) return (bool)field.GetValue(obj);
        }
        catch { }
        return null;
    }

    static string? TryGetString(object obj, string name)
    {
        try
        {
            var type = obj.GetType();
            var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return prop.GetValue(obj)?.ToString();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(obj)?.ToString();
        }
        catch { }
        return null;
    }
}
