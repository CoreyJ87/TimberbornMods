using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MoreHttpApi.Services;

public static class NetworkForwarder
{
    static TcpListener? _listener;
    static volatile bool _running;

    public static void Start(int listenPort, int targetPort)
    {
        Stop();

        Debug.Log($"[MoreHttpApi] Starting network forwarder on port {listenPort} -> localhost:{targetPort}...");

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
            TcpClient? client = null;
            try
            {
                client = _listener!.AcceptTcpClient();
                Debug.Log($"[MoreHttpApi] Accepted connection from {client.Client.RemoteEndPoint}");
                var c = client;
                new Thread(() => HandleConnection(c, targetPort)) { IsBackground = true }.Start();
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

    static void HandleConnection(TcpClient client, int targetPort)
    {
        try
        {
            var stream = client.GetStream();

            var requestLine = ReadLine(stream);
            if (string.IsNullOrEmpty(requestLine)) return;

            var parts = requestLine.Split(' ');
            if (parts.Length < 2) return;
            var method = parts[0];
            var path = parts[1];

            var headers = new System.Collections.Generic.Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
            string line;
            while (!string.IsNullOrEmpty(line = ReadLine(stream)))
            {
                var colonIdx = line.IndexOf(':');
                if (colonIdx > 0)
                    headers[line.Substring(0, colonIdx).Trim()] = line.Substring(colonIdx + 1).Trim();
            }

            byte[]? body = null;
            if (headers.TryGetValue("Content-Length", out var cl)
                && int.TryParse(cl, out var contentLength) && contentLength > 0)
            {
                body = new byte[contentLength];
                int totalRead = 0;
                while (totalRead < contentLength)
                {
                    int read = stream.Read(body, totalRead, contentLength - totalRead);
                    if (read <= 0) break;
                    totalRead += read;
                }
            }

            var targetUrl = $"http://localhost:{targetPort}{path}";
            var request = (HttpWebRequest)WebRequest.Create(targetUrl);
            request.Method = method;
            request.Proxy = null;

            foreach (var h in headers)
            {
                switch (h.Key.ToLower())
                {
                    case "host":
                    case "content-length":
                    case "transfer-encoding":
                    case "connection":
                        continue;
                    case "accept":
                        request.Accept = h.Value;
                        break;
                    case "content-type":
                        request.ContentType = h.Value;
                        break;
                    case "user-agent":
                        request.UserAgent = h.Value;
                        break;
                    default:
                        try { request.Headers[h.Key] = h.Value; } catch { }
                        break;
                }
            }

            if (body != null && body.Length > 0)
            {
                request.ContentLength = body.Length;
                using var reqStream = request.GetRequestStream();
                reqStream.Write(body, 0, body.Length);
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex) when (wex.Response is HttpWebResponse errorResponse)
            {
                response = errorResponse;
            }

            using (response)
            {
                byte[] responseBody;
                using (var responseStream = response.GetResponseStream())
                using (var ms = new MemoryStream())
                {
                    responseStream?.CopyTo(ms);
                    responseBody = ms.ToArray();
                }

                var sb = new StringBuilder();
                sb.Append($"HTTP/1.1 {(int)response.StatusCode} {response.StatusDescription}\r\n");

                foreach (string key in response.Headers)
                {
                    if (string.Equals(key, "Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (string.Equals(key, "Content-Length", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sb.Append($"{key}: {response.Headers[key]}\r\n");
                }

                sb.Append($"Content-Length: {responseBody.Length}\r\n");
                sb.Append("Connection: close\r\n");
                sb.Append("\r\n");

                var headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
                stream.Write(headerBytes, 0, headerBytes.Length);

                if (responseBody.Length > 0)
                    stream.Write(responseBody, 0, responseBody.Length);

                stream.Flush();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MoreHttpApi] Forward error: {ex.GetType().Name}: {ex.Message}");
            try
            {
                var error = Encoding.UTF8.GetBytes("HTTP/1.1 502 Bad Gateway\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                client.GetStream().Write(error, 0, error.Length);
            }
            catch { }
        }
        finally
        {
            try { client?.Close(); } catch { }
        }
    }

    static string ReadLine(NetworkStream stream)
    {
        var sb = new StringBuilder();
        while (true)
        {
            int b = stream.ReadByte();
            if (b < 0) break;
            if (b == '\r')
            {
                int next = stream.ReadByte();
                if (next == '\n') break;
                if (next >= 0) sb.Append((char)next);
                continue;
            }
            if (b == '\n') break;
            sb.Append((char)b);
        }
        return sb.ToString();
    }
}
