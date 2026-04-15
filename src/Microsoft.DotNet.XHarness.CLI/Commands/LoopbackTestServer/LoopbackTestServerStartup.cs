// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;

/// <summary>
/// ASP.NET Core pipeline for the xharness loopback WASM test server.
/// </summary>
internal sealed class LoopbackTestServerStartup
{
    private readonly IWebHostEnvironment _hostingEnvironment;

    public LoopbackTestServerStartup(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public void Configure(IApplicationBuilder app, IOptionsMonitor<WebServerOptions> optionsAccessor)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger>();
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".cjs"] = "text/javascript";
        provider.Mappings[".mjs"] = "text/javascript";

        foreach (var extn in new string[] { ".dll", ".pdb", ".dat", ".blat", ".webcil" })
        {
            provider.Mappings[extn] = "application/octet-stream";
        }

        // Whitelist common WASM / static assets (ServeUnknownFileTypes = false → unlisted extensions are not served).
        foreach (var pair in new (string Ext, string Mime)[]
        {
            (".json", "application/json"),
            (".css", "text/css"),
            (".map", "application/json"),
            (".svg", "image/svg+xml"),
            (".png", "image/png"),
            (".jpg", "image/jpeg"),
            (".jpeg", "image/jpeg"),
            (".gif", "image/gif"),
            (".webp", "image/webp"),
            (".ico", "image/x-icon"),
            (".txt", "text/plain"),
            (".woff", "font/woff"),
            (".woff2", "font/woff2"),
            (".ttf", "font/ttf"),
            (".br", "application/octet-stream"),
            (".gz", "application/gzip"),
        })
        {
            provider.Mappings[pair.Ext] = pair.Mime;
        }

        var options = optionsAccessor.CurrentValue;

        app.Use((context, next) =>
        {
            string path = context.Request.Path.Value ?? string.Empty;
            if (WebServerStaticContentSecurity.ShouldBlockRequestPath(path))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }

            return next();
        });

        if (options.UseCrossOriginPolicy)
        {
            app.Use((context, next) =>
            {
                context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
                context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
                return next();
            });
        }

        // CORS must run before static files so static assets also carry the policy headers.
        if (options.UseCors)
        {
            app.UseCors("LocalhostCors");
        }

        if (options.UseDefaultFiles)
        {
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath)
            });
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(_hostingEnvironment.ContentRootPath),
            ContentTypeProvider = provider,
            ServeUnknownFileTypes = false
        });

        if (!string.IsNullOrEmpty(options.StatefulSessionToken))
        {
            // Avoid leaking the token via query params; bind it to an HttpOnly cookie for browser clients.
            app.Use((context, next) =>
            {
                context.Response.Cookies.Append(
                    WebServerStatefulSession.CookieName,
                    options.StatefulSessionToken!,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                        Secure = options.UseHttps,
                        Path = "/",
                    });
                return next();
            });
        }

        app.UseWebSockets();
        if (options.OnConsoleConnected != null)
        {
            int activeConsoleConnections = 0;
            app.UseRouter(router =>
            {
                router.MapGet("/console", async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    if (!LocalhostStatefulEndpointGate.IsWebSocketRequestAllowed(context.Request, options.StatefulSessionToken))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    if (Interlocked.Increment(ref activeConsoleConnections) != 1)
                    {
                        Interlocked.Decrement(ref activeConsoleConnections);
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        return;
                    }

                    try
                    {
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await options.OnConsoleConnected(socket);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeConsoleConnections);
                    }
                });
            });
        }

        if (options.WebServerUploadResults)
        {
            app.UseRouter(router =>
            {
                router.MapPost("/test-results", async context =>
                {
                    if (!LocalhostStatefulEndpointGate.IsHttpRequestAllowed(context.Request, options.StatefulSessionToken))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return;
                    }

                    var xmlResultsFilePath = Path.Combine(options.OutputDirectory!, "testResults.xml");
                    HostPathSecurity.ThrowIfUnsafeHostPath(xmlResultsFilePath, nameof(options.OutputDirectory));

                    bool withinLimit;
                    long written;
                    await using (var fileStream = HostPathSecurity.OpenNewFileForWriteUnderBase(options.OutputDirectory!, xmlResultsFilePath, nameof(options.OutputDirectory)))
                    {
                        (withinLimit, written) = await WebServerTestResultsUpload.TryCopyStreamToStreamWithLimitAsync(
                            context.Request.Body,
                            fileStream,
                            WebServerTestResultsUpload.MaxRequestBodyBytes,
                            context.RequestAborted);
                    }

                    if (!withinLimit)
                    {
                        try
                        {
                            File.Delete(xmlResultsFilePath);
                        }
                        catch
                        {
                            // best effort
                        }

                        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                        return;
                    }

                    logger.LogInformation($"Stored {xmlResultsFilePath} results {written} bytes");
                });
            });
        }

        foreach (var middleware in options.EchoServerMiddlewares)
        {
            app.UseMiddleware(middleware);
            logger.LogInformation($"Loaded {middleware.FullName} middleware");
        }
    }
}
