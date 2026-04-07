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
    }

}
