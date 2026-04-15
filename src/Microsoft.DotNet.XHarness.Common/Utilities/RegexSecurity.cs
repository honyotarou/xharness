// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Builds <see cref="Regex"/> instances with a bounded match time to reduce ReDoS risk on attacker-controlled or config-driven patterns.
/// </summary>
public static class RegexSecurity
{
    /// <summary>Default upper bound for a single <see cref="Regex.Match(string)"/> against long lines.</summary>
    public static readonly TimeSpan DefaultMatchTimeout = TimeSpan.FromSeconds(1);

    public static Regex Create(string pattern, RegexOptions options = RegexOptions.None)
    {
        // Do not force Compiled: it can conflict with NonBacktracking on newer runtimes.
        // Callers can opt into Compiled explicitly if they want it.
        return new Regex(pattern, options, DefaultMatchTimeout);
    }
}
