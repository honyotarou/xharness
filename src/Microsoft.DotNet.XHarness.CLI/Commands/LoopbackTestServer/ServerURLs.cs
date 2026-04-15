// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace Microsoft.DotNet.XHarness.CLI.Commands;

/// <summary>
/// Bound addresses returned after the loopback test server starts.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record ServerURLs(string Http, string? Https, string? StatefulSessionToken = null)
{
    private string DebuggerDisplay => ToString();

    public override string ToString()
    {
        var token = string.IsNullOrEmpty(StatefulSessionToken) ? null : "[REDACTED]";
        return $"ServerURLs {{ Http = {Http}, Https = {Https}, StatefulSessionToken = {token} }}";
    }
}
