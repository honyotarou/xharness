// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Limits for <c>POST /test-results</c> (attacker: disk/memory DoS via huge uploads).
/// </summary>
public static class WebServerTestResultsUpload
{
    /// <summary>Maximum request body size for uploaded test result XML.</summary>
    public const long MaxRequestBodyBytes = 50L * 1024 * 1024;

    /// <summary>
    /// Copies <paramref name="source"/> to <paramref name="destination"/> until EOF or <paramref name="maxBytes"/> exceeded.
    /// </summary>
    /// <returns><see langword="true"/> if completed within limit; <see langword="false"/> if the next read would exceed <paramref name="maxBytes"/>.</returns>
    public static async Task<(bool WithinLimit, long BytesWritten)> TryCopyStreamToStreamWithLimitAsync(
        Stream source,
        Stream destination,
        long maxBytes,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[8192];
        long written = 0;
        while (true)
        {
            int read = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                return (true, written);
            }

            if (written + read > maxBytes)
            {
                return (false, written);
            }

            await destination.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
            written += read;
        }
    }
}
