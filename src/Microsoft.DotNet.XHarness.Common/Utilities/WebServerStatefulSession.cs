// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Per-process secret for localhost stateful endpoints when the client cannot present a loopback <c>Origin</c> (attacker: local processes without browser Origin).
/// </summary>
public static class WebServerStatefulSession
{
    public const string HeaderName = "X-XHarness-Stateful-Session";

    public const string QueryParameterName = "xharness_session";

    public const string EnvironmentVariableName = "XHARNESS_STATEFUL_SESSION";

    public const string CookieName = "xharness_stateful_session";

    /// <summary>32-byte token, Base64Url-encoded (no padding).</summary>
    public static string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return ToBase64Url(bytes);
    }

    private static string ToBase64Url(ReadOnlySpan<byte> data)
    {
        string b64 = Convert.ToBase64String(data);
        return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Constant-time comparison for UTF-8 header/query values.</summary>
    public static bool FixedTimeEquals(string? a, string? b)
    {
        if (a is null || b is null)
        {
            return false;
        }

        byte[] ba = Encoding.UTF8.GetBytes(a);
        byte[] bb = Encoding.UTF8.GetBytes(b);
        if (ba.Length != bb.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
