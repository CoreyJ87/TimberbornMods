using System.Net.Sockets;
using System.Threading;

namespace MoreHttpApi.Services;

public static class NetworkForwarder
{
    static TcpListener _listener;
    static volatile bool _running;

    public static void Start(int listenPort, int targetPort)
    {
        Stop();

        Debug.Log($"[MoreHttpApi] Starting network forwarder on port {listenPort} → localhost:{targetPort}...");

        _listener = new TcpListener(IPAddress.Any, listenPort);
        _listener.Start();
        _running = true;

        Debug.Log($"[MoreHttpApi] Network forwarder ACTIVE on 0.0.0.0:{listenPort}");

        var thread = new Thread(() => AcceptLoop(listenPort, targetPort));
        thread.IsBackground = true;
        thread.Name = "MoreHttpApi-NetworkForwarder";
        thread.Start();
    }

    public static void Stop()
    {
        _running = false;
        try { _listener?.Stop(); } catch { }
        _listener = null;
    }

    static void AcceptLoop(int listenPort, int targetPort)
    {
        Debug.Log($"[MoreHttpApi] Accept loop started on port {listenPort}");
        while (_running)
        {
            TcpClient client = null;
            try
            {
                client = _listener.AcceptTcpClient();
                Debug.Log($"[MoreHttpApi] Accepted connection from {client.Client.RemoteEndPoint}");
                var c = client;
                new Thread(() => Forward(c, targetPort)) { IsBackground = true }.Start();
            }
            catch (SocketException) when (!_running)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MoreHttpApi] Accept error: {ex.GetType().Name}: {ex.Message}");
            }
        }
        Debug.Log("[MoreHttpApi] Accept loop exited");
    }

    static void Forward(TcpClient client, int targetPort)
    {
        TcpClient target = null;
        try
        {
            target = new TcpClient();
            target.Connect(IPAddress.Loopback, targetPort);

            var clientStream = client.GetStream();
            var targetStream = target.GetStream();

            var toTarget = new Thread(() => CopyStream(clientStream, targetStream, "client→game")) { IsBackground = true };
            var toClient = new Thread(() => CopyStream(targetStream, clientStream, "game→client")) { IsBackground = true };

            toTarget.Start();
            toClient.Start();

            toTarget.Join();
            toClient.Join();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MoreHttpApi] Forward error: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            try { client?.Close(); } catch { }
            try { target?.Close(); } catch { }
        }
    }

    static void CopyStream(NetworkStream from, NetworkStream to, string direction)
    {
        var buffer = new byte[8192];
        try
        {
            int read;
            while ((read = from.Read(buffer, 0, buffer.Length)) > 0)
            {
                to.Write(buffer, 0, read);
                to.Flush();
            }
        }
        catch
        {
        }
    }
}
