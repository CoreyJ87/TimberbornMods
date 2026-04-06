namespace MoreHttpApi.Handlers;

public interface IMoreHttpApiHandler
{
    string Endpoint { get; }
    Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath);
}

/// <summary>
/// Marker interface for handlers that don't need Unity's main thread.
/// These run entirely on a background thread, avoiding frame hitches.
/// </summary>
public interface IBackgroundHttpApiHandler : IMoreHttpApiHandler;
