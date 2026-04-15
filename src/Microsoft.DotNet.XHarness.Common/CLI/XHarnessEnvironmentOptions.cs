// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.CLI;

/// <summary>
/// Optional behaviors controlled via environment variables (CI / security).
/// </summary>
public static class XHarnessEnvironmentOptions
{
    /// <summary>
    /// When false or 0, the CLI does not print the full argv line (avoids leaking secrets passed as flags).
    /// Default is true for backward compatibility.
    /// </summary>
    public const string LogIssuedCommandVariableName = "XHARNESS_LOG_ISSUED_COMMAND";

    public static bool ShouldLogIssuedCommandLine()
    {
        string? v = Environment.GetEnvironmentVariable(LogIssuedCommandVariableName);
        if (string.IsNullOrEmpty(v))
        {
            return true;
        }

        if (bool.TryParse(v, out bool b))
        {
            return b;
        }

        if (string.Equals(v, "0", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(v, "1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return true;
    }
}
