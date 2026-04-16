// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Formats exceptions for user-facing logs without full stack traces (reduces noise and log-injection via newlines in Message).
/// </summary>
public static class ExceptionLogFormatter
{
    /// <summary>
    /// Returns <c>Type: message</c> on a single logical line (newlines collapsed to spaces).
    /// </summary>
    public static string FormatSummary(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        string type = ex.GetType().FullName ?? ex.GetType().Name;
        string message = ex.Message ?? string.Empty;
        message = message.Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal);

        return $"{type}: {message}";
    }
}
