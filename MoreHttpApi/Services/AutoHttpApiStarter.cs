namespace MoreHttpApi.Services;

[BindSingleton]
public class AutoHttpApiStarter(HttpApi api, MSettings s) : IPostLoadableSingleton
{
    
    public void PostLoad()
    {
        if (!s.AutoStartApi.Value) { return; }

        var port = (ushort)s.AutoStartPort.Value;
        api.SetPort(port);
        api.Start();

        if (!s.AllowNetworkAccess.Value) { return; }

        try
        {
            RebindToAllInterfaces(api, port);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MoreHttpApi] Failed to enable network access: {ex.Message}");
            Debug.LogWarning($"[MoreHttpApi] Run as admin once, or run: netsh http add urlacl url=http://+:{port}/ user=Everyone");
        }
    }

    static void RebindToAllInterfaces(HttpApi api, ushort port)
    {
        var listenerField = api.GetType()
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .FirstOrDefault(f => f.FieldType == typeof(HttpListener));

        if (listenerField == null)
        {
            Debug.LogWarning("[MoreHttpApi] Could not find HttpListener field on HttpApi — network access not available.");
            return;
        }

        var listener = (HttpListener)listenerField.GetValue(api);
        if (listener == null || !listener.IsListening)
        {
            Debug.LogWarning("[MoreHttpApi] HttpListener is not running — network access not available.");
            return;
        }

        listener.Stop();
        listener.Prefixes.Clear();
        listener.Prefixes.Add($"http://+:{port}/");
        listener.Start();

        Debug.Log($"[MoreHttpApi] Network access enabled — listening on http://+:{port}/");
    }

}
