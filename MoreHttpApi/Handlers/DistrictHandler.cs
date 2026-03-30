namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class DistrictHandler(
    BeaverPopulation beavers,
    EventBus eb,
    ILoc t
) : IMoreHttpApiHandler, ILoadableSingleton
{
    public string Endpoint => "districts";

    readonly HashSet<Bot> bots = [];

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnCharacterCreated(CharacterCreatedEvent e)
    {
        var b = e.Character.GetComponent<Bot>();
        if (b) bots.Add(b);
    }

    [OnEvent]
    public void OnCharacterKilled(CharacterKilledEvent e)
    {
        var b = e.Character.GetComponent<Bot>();
        if (b is not null) bots.Remove(b);
    }

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => await context.HandleAsync(GetDistrictsAsync);

    async Task<HttpDistrictInfo> GetDistrictsAsync()
    {
        Dictionary<Guid, (string Name, int Adults, int Children, int Bots)> districts = [];

        void EnsureDistrict(Citizen citizen)
        {
            if (!citizen.HasAssignedDistrict) return;
            var dc = citizen.AssignedDistrict.GetEntity();
            var id = dc.EntityId;
            if (!districts.ContainsKey(id))
            {
                districts[id] = (dc.GetName(t), 0, 0, 0);
            }
        }

        void IncrementDistrict(Citizen citizen, CharacterType type)
        {
            if (!citizen.HasAssignedDistrict) return;
            var id = citizen.AssignedDistrict.GetEntity().EntityId;
            if (!districts.TryGetValue(id, out var data)) return;

            districts[id] = type switch
            {
                CharacterType.Adult => data with { Adults = data.Adults + 1 },
                CharacterType.Child => data with { Children = data.Children + 1 },
                CharacterType.Bot => data with { Bots = data.Bots + 1 },
                _ => data,
            };
        }

        foreach (var adult in beavers._beaverCollection.Adults)
        {
            var citizen = adult.GetComponent<Citizen>();
            if (!citizen) continue;
            EnsureDistrict(citizen);
            IncrementDistrict(citizen, CharacterType.Adult);
        }

        foreach (var child in beavers._beaverCollection.Children)
        {
            var citizen = child.GetComponent<Citizen>();
            if (!citizen) continue;
            EnsureDistrict(citizen);
            IncrementDistrict(citizen, CharacterType.Child);
        }

        foreach (var bot in bots)
        {
            var citizen = bot.GetComponent<Citizen>();
            if (!citizen) continue;
            EnsureDistrict(citizen);
            IncrementDistrict(citizen, CharacterType.Bot);
        }

        var result = districts
            .Select(kv => new HttpDistrict(kv.Key, kv.Value.Name, kv.Value.Adults, kv.Value.Children, kv.Value.Bots))
            .ToArray();

        return new(result);
    }
}
