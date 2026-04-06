namespace MoreHttpApi.Handlers;

public class PingHandler : IBackgroundHttpApiHandler
{
    public string Endpoint => "ping";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
    {
        await context.WriteText("", 204);
        return true;
    }
}
