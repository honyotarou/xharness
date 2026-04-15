// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.CLI;

/// <summary>
/// Validates values passed to OS processes or shell-less argv to reject injection via control characters.
/// </summary>
public static class CliTokenValidator
{
    public static void ThrowIfContainsControlCharacters(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        foreach (char c in value)
        {
            if (char.IsControl(c) || c == '\u2028' || c == '\u2029')
            {
                throw new ArgumentException("The value must not contain control characters.", paramName);
            }
        }
    }
}
