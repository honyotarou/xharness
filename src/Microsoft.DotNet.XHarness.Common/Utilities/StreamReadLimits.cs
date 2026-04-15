// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Bounds in-memory reads from untrusted streams (attacker: hostile peer sends huge body).
/// </summary>
public static class StreamReadLimits
{
    public static string ReadToEndWithByteLimit(Stream stream, int maxBytes, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes));
        }

        encoding ??= Encoding.UTF8;

        using var ms = new MemoryStream(capacity: Math.Min(maxBytes, 8192));
        var buffer = new byte[8192];
        int total = 0;
        while (true)
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                break;
            }

            int allowed = Microsoft.DotNet.XHarness.Common.Networking.TcpStreamLimits.AllowedReadLength(total, read, maxBytes);
            if (allowed == 0)
            {
                throw new InvalidOperationException("Stream exceeds maximum size.");
            }

            ms.Write(buffer, 0, allowed);
            total += allowed;
            if (allowed < read)
            {
                throw new InvalidOperationException("Stream exceeds maximum size.");
            }
        }

        return encoding.GetString(ms.ToArray());
    }
}

