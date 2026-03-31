namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class AutomationHandler(
    EntityRegistry entityRegistry,
    ILoc t
) : IMoreHttpApiHandler
{
    static readonly HashSet<string> AutomationTypeNames =
    [
        "DepthSensor", "FlowSensor", "ContaminationSensor",
        "WeatherStation", "Chronometer", "Timer",
        "Gate", "Memory", "Relay",
        "ResourceCounter", "PopulationCounter", "ScienceCounter", "PowerMeter",
        "Indicator", "Speaker", "Detonator",
        "Lever", "HttpLever"
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
            foreach (var comp in entity.GetComponents<BaseComponent>())
            {
                var typeName = comp.GetType().Name;
                if (!AutomationTypeNames.Contains(typeName)) continue;

                var props = new Dictionary<string, object?>();
                bool isOutputActive = false;

                try
                {
                    isOutputActive = TryGetBool(comp, "IsActive") ?? TryGetBool(comp, "IsOn") ?? false;
                    ExtractProperties(comp, typeName, props);
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

    static void ExtractProperties(BaseComponent comp, string typeName, Dictionary<string, object?> props)
    {
        switch (typeName)
        {
            case "DepthSensor":
                TryAdd(props, comp, "CurrentDepth");
                TryAdd(props, comp, "Threshold");
                TryAdd(props, comp, "ThresholdMode");
                break;
            case "FlowSensor":
                TryAdd(props, comp, "CurrentFlow");
                TryAdd(props, comp, "Threshold");
                TryAdd(props, comp, "ThresholdMode");
                break;
            case "ContaminationSensor":
                TryAdd(props, comp, "IsContaminated");
                break;
            case "WeatherStation":
                TryAdd(props, comp, "IsHazardousWeather");
                break;
            case "Timer":
                TryAdd(props, comp, "IsOn");
                TryAdd(props, comp, "OnDuration");
                TryAdd(props, comp, "OffDuration");
                TryAdd(props, comp, "TimeRemaining");
                break;
            case "Chronometer":
                TryAdd(props, comp, "StartHour");
                TryAdd(props, comp, "EndHour");
                break;
            case "Gate":
                TryAdd(props, comp, "GateMode");
                TryAdd(props, comp, "IsInverted");
                break;
            case "Memory":
                TryAdd(props, comp, "IsOn");
                break;
            case "Relay":
                TryAdd(props, comp, "IsOn");
                TryAdd(props, comp, "Channel");
                break;
            case "Lever":
            case "HttpLever":
                TryAdd(props, comp, "IsOn");
                TryAdd(props, comp, "IsSpringReturn");
                TryAdd(props, comp, "IsPinned");
                break;
            case "ResourceCounter":
                TryAdd(props, comp, "GoodId");
                TryAdd(props, comp, "Threshold");
                TryAdd(props, comp, "CurrentCount");
                break;
            case "PopulationCounter":
                TryAdd(props, comp, "Threshold");
                TryAdd(props, comp, "CurrentCount");
                break;
            case "PowerMeter":
                TryAdd(props, comp, "Threshold");
                TryAdd(props, comp, "CurrentPower");
                break;
            case "Indicator":
                TryAdd(props, comp, "IsOn");
                break;
        }
    }

    static void TryAdd(Dictionary<string, object?> props, object comp, string propertyName)
    {
        try
        {
            var type = comp.GetType();
            var prop = type.GetProperty(propertyName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop != null)
            {
                props[propertyName] = prop.GetValue(comp);
                return;
            }
            var field = type.GetField(propertyName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                props[propertyName] = field.GetValue(comp);
            }
        }
        catch
        {
        }
    }

    static bool? TryGetBool(object comp, string name)
    {
        try
        {
            var type = comp.GetType();
            var prop = type.GetProperty(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (prop?.PropertyType == typeof(bool))
                return (bool)prop.GetValue(comp);
            var field = type.GetField(name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (field?.FieldType == typeof(bool))
                return (bool)field.GetValue(comp);
        }
        catch { }
        return null;
    }
}
