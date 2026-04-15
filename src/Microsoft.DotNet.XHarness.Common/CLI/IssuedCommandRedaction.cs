// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.CLI;

/// <summary>
/// Redacts argv fragments before they are written to logs (attacker: shoulder-surf CI logs for secrets in flags).
/// </summary>
public static class IssuedCommandRedaction
{
    private static readonly string[] SensitiveKeySubstrings =
    {
        "token", "password", "secret", "auth", "bearer", "credential", "apikey",
        "key", "cert", "passphrase", "connection", "cookie",
    };

    /// <summary>
    /// Formats argv for logging, redacting values for arguments that look like <c>--name=value</c> when the name is sensitive.
    /// </summary>
    public static string FormatArgumentsForLog(string[]? args)
    {
        if (args == null || args.Length == 0)
        {
            return string.Empty;
        }

        var parts = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            string cur = args[i];
            parts[i] = RedactSingleArgument(cur);

            // Handle "--key value" (space-separated) form.
            // If current token is a sensitive flag name, redact the next token as its value.
            if (LooksLikeSensitiveKeyToken(cur) && i + 1 < args.Length)
            {
                parts[i + 1] = "[REDACTED]";
                i++; // skip next, we've already emitted it
            }
        }

        return string.Join(' ', parts);
    }

    private static bool LooksLikeSensitiveKeyToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // We only treat leading-dash tokens as keys. Non-dash tokens (positional args) are not keys.
        if (!token.StartsWith("-", StringComparison.Ordinal))
        {
            return false;
        }

        // If this token already contains an explicit "=value", it is not the "--key value" form.
        if (token.Contains('=', StringComparison.Ordinal))
        {
            return false;
        }

        // Remove leading '-' and stop at '=' if present.
        var span = token.AsSpan().TrimStart('-');
        int eq = span.IndexOf('=');
        if (eq >= 0)
        {
            span = span[..eq];
        }

        // Normalize things like "--foo:" or "--foo," in case a consumer formats flags oddly.
        while (!span.IsEmpty)
        {
            char last = span[^1];
            if (last is ':' or ',' or ';')
            {
                span = span[..^1];
                continue;
            }
            break;
        }
        if (span.IsEmpty)
        {
            return false;
        }

        string key = span.ToString();
        foreach (string needle in SensitiveKeySubstrings)
        {
            if (key.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string RedactSingleArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return arg;
        }

        int eq = arg.IndexOf('=');
        if (eq <= 0)
        {
            return arg;
        }

        string key = arg.AsSpan(0, eq).ToString().TrimStart('-');
        foreach (string needle in SensitiveKeySubstrings)
        {
            if (key.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return arg.Substring(0, eq + 1) + "[REDACTED]";
            }
        }

        return arg;
    }
}
