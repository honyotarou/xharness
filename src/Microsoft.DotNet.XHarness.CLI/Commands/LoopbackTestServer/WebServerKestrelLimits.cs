// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;

internal static class WebServerKestrelLimits
{
    public static void ApplyTestResultsUploadCap(KestrelServerOptions options)
    {
        options.Limits.MaxRequestBodySize = WebServerTestResultsUpload.MaxRequestBodyBytes;

        // Loopback-only server, but apply conservative HTTP/2 limits to reduce abuse surface.
        options.Limits.Http2.MaxStreamsPerConnection = 16;
        options.Limits.Http2.MaxFrameSize = 16 * 1024; // bytes
        options.Limits.Http2.HeaderTableSize = 8 * 1024; // bytes
    }
}
