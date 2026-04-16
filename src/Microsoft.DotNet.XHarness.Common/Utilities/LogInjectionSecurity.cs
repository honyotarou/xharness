// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.RegularExpressions;
using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Sanitizes untrusted log content to mitigate terminal control sequences and marker forgery (attacker: CI log injection).
/// </summary>
public static class LogInjectionSecurity
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Strip ANSI control sequences (CSI/OSC) and normalize CR to avoid overwrite tricks.
        string s = StripAnsi(input)
            .Replace("\u001B", string.Empty, StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal);

        // Prevent forging of result markers in untrusted output.
        s = s.Replace(Common.RunSummaryEmitter.JsonStartMarker, "[XHARNESS_RESULT_START]", StringComparison.Ordinal)
             .Replace(Common.RunSummaryEmitter.JsonEndMarker, "[XHARNESS_RESULT_END]", StringComparison.Ordinal);

        // Defense-in-depth: drop any remaining C0 controls (e.g. ESC variants missed by StripAnsi, or odd logger state).
        return StripC0ControlsExceptNewlineTab(s);
    }

    private static string StripC0ControlsExceptNewlineTab(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        var sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
        {
            if (c < 0x20 && c != '\n' && c != '\r' && c != '\t')
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static string StripAnsi(string input)
    {
        // Fast path
        if (input.IndexOf('\u001B') < 0)
        {
            return input;
        }

        var sb = new System.Text.StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c != '\u001B')
            {
                sb.Append(c);
                continue;
            }

            // ESC sequence
            if (i + 1 >= input.Length)
            {
                break;
            }

            char next = input[i + 1];
            if (next == '[')
            {
                // CSI: ESC [ ... final byte in @-~
                i += 2;
                for (; i < input.Length; i++)
                {
                    char ch = input[i];
                    if (ch >= '@' && ch <= '~')
                    {
                        break;
                    }
                }
            }
            else if (next == ']')
            {
                // OSC: ESC ] ... BEL or ESC \
                i += 2;
                for (; i < input.Length; i++)
                {
                    char ch = input[i];
                    if (ch == '\u0007')
                    {
                        break;
                    }
                    if (ch == '\u001B' && i + 1 < input.Length && input[i + 1] == '\\')
                    {
                        i += 1;
                        break;
                    }
                }
            }
            else
            {
                // Unknown escape: drop ESC and next char
                i += 1;
            }
        }

        return sb.ToString();
    }
}

