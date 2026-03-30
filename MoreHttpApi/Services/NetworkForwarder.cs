using System.Net.Sockets;

namespace MoreHttpApi.Services;

public static class NetworkForwarder
{
    static TcpListener? _listener;
    static CancellationTokenSource? _cts;

    public static void Start(ushort listenPort, ushort targetPort)
    {
        Stop();

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, listenPort);
        _listener.Start();

        Debug.Log($"[MoreHttpApi] Network forwarder listening on 0.0.0.0:{listenPort} → localhost:{targetPort}");

        Task.Run(() => AcceptLoop(_listener, targetPort, _cts.Token));
    }

    public static void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        _listener = null;
        _cts = null;
    }

    static async Task AcceptLoop(TcpListener listener, ushort targetPort, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => ForwardConnection(client, targetPort, ct));
            }
            catch when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MoreHttpApi] Forwarder accept error: {ex.Message}");
            }
        }
    }

    static async Task ForwardConnection(TcpClient client, ushort targetPort, CancellationToken ct)
    {
        using var _ = client;
        using var target = new TcpClient();

        try
        {
            await target.ConnectAsync(IPAddress.Loopback, targetPort);
        }
        catch
        {
            return;
        }

        using var clientStream = client.GetStream();
        using var targetStream = target.GetStream();

        var toTarget = CopyAsync(clientStream, targetStream, ct);
        var toClient = CopyAsync(targetStream, clientStream, ct);

        await Task.WhenAny(toTarget, toClient);
    }

    static async Task CopyAsync(NetworkStream from, NetworkStream to, CancellationToken ct)
    {
        var buffer = new byte[8192];
        try
        {
            int read;
            while ((read = await from.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await to.WriteAsync(buffer, 0, read, ct);
                await to.FlushAsync(ct);
            }
        }
        catch
        {
        }
    }
}
