// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;

namespace Microsoft.DotNet.XHarness.CLI;

/// <summary>
/// Decides whether stdout should stay machine-parseable (no extra xharness banners).
/// </summary>
internal static class CommandLineSensitiveOutput
{
    /// <summary>
    /// Returns true when the command outputs data suitable for parsing and we should keep the output clean.
    /// </summary>
    public static bool IsSensitive(string[] args)
    {
        if (args.Length > 0 && args[0] == "version")
        {
            return true;
        }

        if (args.Length < 2 || args.Contains("--help") || args.Contains("-h"))
        {
            return false;
        }

        var platform = args[0];
        var command = args[1];

        return platform switch
        {
            "apple" => command switch
            {
                "device" => true,
                "state" => args.Contains("--json"),
                "mlaunch" => true,
                _ => false,
            },
            "android" => command switch
            {
                "device" => true,
                "state" => args.Contains("--json"),
                "adb" => true,
                _ => false,
            },
            "android-headless" => command switch
            {
                "device" => true,
                "state" => args.Contains("--json"),
                "adb" => true,
                _ => false,
            },
            _ => false,
        };
    }
}
