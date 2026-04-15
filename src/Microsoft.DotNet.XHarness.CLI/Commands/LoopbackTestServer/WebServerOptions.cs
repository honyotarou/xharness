// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;
using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;

/// <summary>
/// Immutable configuration surface for <see cref="WebServerHost" />; built from CLI arguments or tests.
/// </summary>
internal sealed class WebServerOptions
{
    public Func<WebSocket, Task>? OnConsoleConnected { get; set; }
    public IList<Type> EchoServerMiddlewares { get; set; } = new List<Type>();
    public bool UseCors { get; set; }
    public bool UseHttps { get; set; }
    public bool UseCrossOriginPolicy { get; set; }
    public bool UseDefaultFiles { get; set; }
    public bool WebServerUploadResults { get; set; }
    public string? OutputDirectory { get; set; }
    public string? ContentRoot { get; set; }

    /// <summary>Per-server secret for non-browser clients hitting stateful endpoints.</summary>
    public string? StatefulSessionToken { get; set; }

    /// <summary>
    /// Ensures a session secret exists when stateful endpoints are enabled.
    /// </summary>
    public void EnsureStatefulSessionSecretIfNeeded()
    {
        bool needsStateful = WebServerUploadResults || OnConsoleConnected != null;
        if (!needsStateful)
        {
            return;
        }

        if (string.IsNullOrEmpty(StatefulSessionToken))
        {
            StatefulSessionToken = WebServerStatefulSession.GenerateToken();
        }
    }

    public void CopyTo(WebServerOptions otherOptions)
    {
        otherOptions.OnConsoleConnected = OnConsoleConnected;
        otherOptions.EchoServerMiddlewares = EchoServerMiddlewares;
        otherOptions.UseCors = UseCors;
        otherOptions.UseHttps = UseHttps;
        otherOptions.UseCrossOriginPolicy = UseCrossOriginPolicy;
        otherOptions.UseDefaultFiles = UseDefaultFiles;
        otherOptions.WebServerUploadResults = WebServerUploadResults;
        otherOptions.OutputDirectory = OutputDirectory;
        otherOptions.ContentRoot = ContentRoot;
        otherOptions.StatefulSessionToken = StatefulSessionToken;
    }

    public static WebServerOptions FromArguments(IWebServerArguments arguments)
    {
        var options = new WebServerOptions
        {
            UseCors = arguments.WebServerUseCors,
            UseHttps = arguments.WebServerUseHttps,
            UseCrossOriginPolicy = arguments.WebServerUseCrossOriginPolicy,
            UseDefaultFiles = arguments.WebServerUseDefaultFiles,
            WebServerUploadResults = arguments.WebServerUploadResults,
            OutputDirectory = arguments.OutputDirectory,
        };
        foreach (var middlewareType in arguments.WebServerMiddlewarePathsAndTypes.GetLoadedTypes())
        {
            options.EchoServerMiddlewares.Add(middlewareType);
        }

        return options;
    }
}
