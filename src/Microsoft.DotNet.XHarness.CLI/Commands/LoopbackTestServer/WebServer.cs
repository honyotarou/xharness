// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;
using Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;
using Microsoft.Extensions.Logging;

namespace Microsoft.DotNet.XHarness.CLI.Commands;

/// <summary>
/// Entry point for starting the loopback WASM test server. Host and pipeline live in this folder.
/// </summary>
public static class WebServer
{
    internal static Task<ServerURLs> Start(
        IWebServerArguments arguments,
        ILogger logger,
        CancellationToken token,
        Func<WebSocket, Task>? onConsoleConnected = null) =>
        WebServerHost.Start(arguments, logger, token, onConsoleConnected);

    internal static Task<ServerURLs> Start(WebServerOptions options, ILogger logger, CancellationToken token) =>
        WebServerHost.Start(options, logger, token);
}
