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

        var networkPort = (ushort)s.NetworkPort.Value;
        try
        {
            NetworkForwarder.Start(networkPort, port);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MoreHttpApi] Failed to start network forwarder: {ex.Message}");
        }
    }

}
