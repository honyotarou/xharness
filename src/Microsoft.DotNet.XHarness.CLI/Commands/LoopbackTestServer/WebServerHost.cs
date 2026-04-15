// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
namespace Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;

/// <summary>
/// Owns generic-host construction and listening URLs for the loopback test server.
/// </summary>
internal static class WebServerHost
{
    public static Task<ServerURLs> Start(
        IWebServerArguments arguments,
        ILogger logger,
        CancellationToken token,
        Func<WebSocket, Task>? onConsoleConnected = null)
    {
        var options = WebServerOptions.FromArguments(arguments);
        options.OnConsoleConnected = onConsoleConnected;
        return Start(options, logger, token);
    }

    public static async Task<ServerURLs> Start(WebServerOptions webServerOptions, ILogger logger, CancellationToken token)
    {
        webServerOptions.EnsureStatefulSessionSecretIfNeeded();

        var urls = webServerOptions.UseHttps
            ? new[] { "http://127.0.0.1:0", "https://127.0.0.1:0" }
            : new[] { "http://127.0.0.1:0" };

        var builder = new HostBuilder()
            .ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<LoopbackTestServerStartup>()
                    .UseKestrel(WebServerKestrelLimits.ApplyTestResultsUploadCap)
                    .UseUrls(urls);
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole().AddFilter(null, LogLevel.Warning);
            })
            .ConfigureServices((ctx, services) =>
            {
                if (webServerOptions.UseCors)
                {
                    services.AddCors(o => o.AddPolicy("LocalhostCors", corsBuilder =>
                    {
                        corsBuilder.SetIsOriginAllowed(LocalhostStatefulEndpointGate.IsCorsOriginAllowed)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("*");
                    }));
                }

                services.AddRouting();
                services.AddLogging();
                services.AddSingleton(logger);
                services.Configure<WebServerOptions>(ctx.Configuration);
                services.Configure<WebServerOptions>(options => webServerOptions.CopyTo(options));
            });

        if (webServerOptions.ContentRoot != null)
        {
            builder.UseContentRoot(webServerOptions.ContentRoot);
        }

        var host = builder.Build();

        await host.StartAsync(token);

        var server = host.Services.GetService<IServer>();
        var addressFeature = server?.Features.Get<IServerAddressesFeature>();
        var ipAddress = addressFeature?
            .Addresses
            .Where(a => a.StartsWith("http:", StringComparison.Ordinal))
            .Select(a => new Uri(a))
            .Select(uri => $"{uri.Host}:{uri.Port}")
            .FirstOrDefault();
        var ipAddressSecure = addressFeature?
            .Addresses
            .Where(a => a.StartsWith("https:", StringComparison.Ordinal))
            .Select(a => new Uri(a))
            .Select(uri => $"{uri.Host}:{uri.Port}")
            .FirstOrDefault();

        if (ipAddress == null || (webServerOptions.UseHttps && ipAddressSecure == null))
        {
            throw new InvalidOperationException("Failed to determine web server's IP address or port");
        }

        return new ServerURLs(ipAddress, ipAddressSecure, webServerOptions.StatefulSessionToken);
    }
}
