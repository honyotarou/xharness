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
            parts[i] = RedactSingleArgument(args[i]);
        }

        return string.Join(' ', parts);
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
