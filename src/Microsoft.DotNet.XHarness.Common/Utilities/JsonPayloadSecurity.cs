// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Bounds JSON text from external tools before <see cref="System.Text.Json.JsonDocument.Parse(string)"/> to mitigate memory exhaustion.
/// </summary>
public static class JsonPayloadSecurity
{
    /// <summary>Default upper bound for a single parse of tool output (e.g. simctl JSON).</summary>
    public const int DefaultMaxJsonChars = 50_000_000;

    public static void ThrowIfJsonStringTooLong(string json, string paramName, int maxChars = DefaultMaxJsonChars)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        if (json.Length > maxChars)
        {
            throw new ArgumentException($"JSON payload exceeds maximum length ({maxChars} characters).", paramName);
        }
    }
}
