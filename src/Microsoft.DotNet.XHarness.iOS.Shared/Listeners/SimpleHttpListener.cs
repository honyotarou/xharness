// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.Common.Networking;
using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Listeners;

public class SimpleHttpListener : SimpleListener
{
    public const int MaxRequestBodyBytes = TcpStreamLimits.MaxTestLogStreamBytes;
    public static readonly TimeSpan ReadTimeout = TimeSpan.FromMinutes(2);

    private readonly bool _autoExit;
    private HttpListener _server;
    private bool _connected_once;

    public int Port { get; private set; }

    public SimpleHttpListener(ILog log, IFileBackedLog testLog, bool autoExit) : base(log, testLog)
    {
        _autoExit = autoExit;
    }

    public override int InitializeAndGetPort()
    {
        _server = new HttpListener();

        if (Port != 0)
        {
            throw new NotImplementedException();
        }

        // Bind to an OS-assigned ephemeral port to avoid predictable selection / port hijacking races.
        string prefixHost = TcpListenerAddressResolver.GetHttpListenerPrefixHost();
        using var tcp = new TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        var newPort = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();

        _server.Prefixes.Clear();
        _server.Prefixes.Add("http://" + prefixHost + ":" + newPort + "/");
        _server.Start();
        Port = newPort;

        return Port;
    }

    protected override void Stop() => _server.Stop();

    protected override void Start()
    {
        bool processed;

        try
        {
            Log.WriteLine("Test log server listening on: {0}:{1}", Address, Port);
            do
            {
                var context = _server.GetContext();
                processed = Processing(context);
            } while (!_autoExit || !processed);
        }
        catch (Exception e)
        {
            if (e is not SocketException se || se.SocketErrorCode != SocketError.Interrupted)
            {
                Console.WriteLine("[{0}] : {1}", DateTime.Now, e);
            }
        }
        finally
        {
            try
            {
                _server.Stop();
            }
            finally
            {
                Finished();
            }
        }
    }

    private bool Processing(HttpListenerContext context)
    {
        var finished = false;

        var request = context.Request;
        var response = "OK";

        string data = string.Empty;
        if (request.HasEntityBody)
        {
            // Bound in-memory read (attacker: hostile device sends a huge body).
            if (request.InputStream.CanTimeout)
            {
                request.InputStream.ReadTimeout = (int)ReadTimeout.TotalMilliseconds;
            }
            data = StreamReadLimits.ReadToEndWithByteLimit(request.InputStream, MaxRequestBodyBytes);
        }

        switch (request.RawUrl)
        {
            case "/Start":
                if (!_connected_once)
                {
                    _connected_once = true;
                    Connected(request.RemoteEndPoint.ToString());
                }
                break;
            case "/Finish":
                if (!finished)
                {
                    TestLog.Write(data);
                    TestLog.Flush();
                    finished = true;
                }
                break;
            default:
                Log.WriteLine("Unknown upload url: {0}", request.RawUrl);
                response = "Unknown upload url";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
        }

        var buf = System.Text.Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = buf.Length;
        context.Response.OutputStream.Write(buf, 0, buf.Length);
        context.Response.OutputStream.Close();
        context.Response.Close();

        return finished;
    }

}

