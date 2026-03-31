using System.Reflection;

namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class AutomationHandler(
    EntityRegistry entityRegistry,
    IDayNightCycle dayNightCycle,
    ILoc t
) : IMoreHttpApiHandler
{
    static readonly BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    static readonly HashSet<Type> SerializableTypes =
    [
        typeof(bool), typeof(int), typeof(float), typeof(double), typeof(long),
        typeof(string), typeof(Guid), typeof(short), typeof(byte),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte), typeof(decimal)
    ];

    static readonly HashSet<string> AutomationTypeNames =
    [
        "DepthSensor", "FlowSensor", "ContaminationSensor",
        "WeatherStation", "Chronometer", "Timer",
        "Gate", "Memory", "Relay",
        "ResourceCounter", "PopulationCounter", "ScienceCounter", "PowerMeter",
        "Indicator", "Speaker", "Detonator",
        "Lever", "HttpLever",
        "Automatable"
    ];

    public string Endpoint => "automation";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetAutomationAsync),
            _ => false,
        };

    async Task<HttpAutomationInfo> GetAutomationAsync()
    {
        var nodes = new List<HttpAutomationNode>();

        foreach (var entity in entityRegistry.Entities)
        {
            foreach (var comp in entity.AllComponents)
            {
                var typeName = comp.GetType().Name;
                if (!AutomationTypeNames.Contains(typeName)) continue;

                if (typeName == "Automatable" && TryGetBool(comp, "IsAutomated") != true)
                    continue;

                var props = new Dictionary<string, object?>();
                bool isOutputActive = false;

                try
                {
                    isOutputActive = TryGetBool(comp, "IsActive") ?? TryGetBool(comp, "IsOn") ?? false;
                    ExtractAllProperties(comp, props);
                    ExtractConnections(comp, props);

                    if (typeName == "Timer")
                        ExtractTimerIntervals(comp, props);
                }
                catch (Exception ex)
                {
                    props["_error"] = ex.Message;
                }

                nodes.Add(new HttpAutomationNode(
                    entity.EntityId,
                    entity.GetName(t),
                    typeName,
                    isOutputActive,
                    props
                ));
            }
        }

        return new([.. nodes]);
    }

    static void ExtractAllProperties(object comp, Dictionary<string, object?> props)
    {
        var type = comp.GetType();

        foreach (var prop in type.GetProperties(AllInstance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (!IsSerializable(prop.PropertyType)) continue;

            try
            {
                var val = prop.GetValue(comp);
                props[prop.Name] = val?.GetType().IsEnum == true ? val.ToString() : val;
            }
            catch { }
        }

        foreach (var field in type.GetFields(AllInstance))
        {
            if (field.Name.Contains("BackingField")) continue;
            if (props.ContainsKey(field.Name)) continue;
            if (!IsSerializable(field.FieldType)) continue;

            try
            {
                var name = field.Name.TrimStart('_');
                if (name.Length == 0 || props.ContainsKey(name)) continue;
                var val = field.GetValue(comp);
                props[name] = val?.GetType().IsEnum == true ? val.ToString() : val;
            }
            catch { }
        }
    }

    void ExtractTimerIntervals(object comp, Dictionary<string, object?> props)
    {
        var type = comp.GetType();

        var intervalA = type.GetProperty("TimerIntervalA", AllInstance)?.GetValue(comp);
        var intervalB = type.GetProperty("TimerIntervalB", AllInstance)?.GetValue(comp);

        if (intervalA != null)
            ExtractInterval(intervalA, "IntervalA", props);
        if (intervalB != null)
            ExtractInterval(intervalB, "IntervalB", props);
    }

    void ExtractInterval(object interval, string prefix, Dictionary<string, object?> props)
    {
        var iType = interval.GetType();

        var typeProp = iType.GetProperty("Type", AllInstance);
        var ticksProp = iType.GetProperty("Ticks", AllInstance);
        var hoursField = iType.GetField("_hours", AllInstance);

        var intervalType = typeProp?.GetValue(interval);
        var ticks = ticksProp?.GetValue(interval);
        var hours = hoursField?.GetValue(interval);

        props[$"{prefix}.Type"] = intervalType?.GetType().IsEnum == true ? intervalType.ToString() : intervalType;
        props[$"{prefix}.Ticks"] = ticks;

        if (hours is float h && h > 0)
        {
            props[$"{prefix}.Hours"] = h;
        }
        else if (ticks is int tickCount && tickCount > 0)
        {
            try
            {
                props[$"{prefix}.Hours"] = dayNightCycle.TicksToHours(tickCount);
            }
            catch { }
        }
    }

    static void ExtractConnections(object comp, Dictionary<string, object?> props)
    {
        var type = comp.GetType();

        foreach (var field in type.GetFields(AllInstance))
        {
            if (field.FieldType.Name != "AutomatorConnection") continue;

            try
            {
                var conn = field.GetValue(comp);
                if (conn == null) continue;

                var transmitterProp = conn.GetType().GetProperty("Transmitter", AllInstance);
                var transmitter = transmitterProp?.GetValue(conn);

                Guid? entityId = null;
                if (transmitter != null)
                {
                    var getComp = transmitter.GetType().GetMethod("GetComponent", AllInstance)
                        ?.MakeGenericMethod(typeof(EntityComponent));
                    var entityComp = getComp?.Invoke(transmitter, null) as EntityComponent;
                    entityId = entityComp?.EntityId;
                }

                var fieldName = field.Name.TrimStart('_');
                var key = fieldName.Length > 0 ? $"_conn_{fieldName}" : "_conn_input";
                props[key] = entityId?.ToString();
            }
            catch { }
        }
    }

    static bool IsSerializable(Type type)
    {
        if (SerializableTypes.Contains(type) || type.IsEnum) return true;
        var underlying = Nullable.GetUnderlyingType(type);
        return underlying != null && (SerializableTypes.Contains(underlying) || underlying.IsEnum);
    }

    static bool? TryGetBool(object comp, string name)
    {
        try
        {
            var type = comp.GetType();
            var prop = type.GetProperty(name, AllInstance);
            if (prop?.PropertyType == typeof(bool))
                return (bool)prop.GetValue(comp);
            var field = type.GetField(name, AllInstance);
            if (field?.FieldType == typeof(bool))
                return (bool)field.GetValue(comp);
        }
        catch { }
        return null;
    }
}
