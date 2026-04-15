// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.DotNet.XHarness.CLI.Commands;
using Microsoft.DotNet.XHarness.Common.CLI;
using Mono.Options;

namespace Microsoft.DotNet.XHarness.CLI;

public static class Program
{
    /// <summary>
    /// The verbatim "--" argument used for pass-through args is removed by Mono.Options when parsing CommandSets,
    /// so in Program.cs, we temporarily replace it with this string and then recognize it back here.
    /// </summary>
    public const string VerbatimArgumentPlaceholder = "[[%verbatim_argument%]]";

    public static int Main(string[] args)
    {
        bool shouldOutput = !CommandLineSensitiveOutput.IsSensitive(args);

        if (shouldOutput && XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine())
        {
            Console.WriteLine(
                $"[{XHarnessVersionCommand.GetAssemblyVersion().ProductVersion}] " +
                "XHarness command issued: " + IssuedCommandRedaction.FormatArgumentsForLog(args));
        }

        if (args.Length > 0)
        {
#if !DEBUG
            if (args[0] == "apple" && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Otherwise the command would just not be found
                Console.Error.WriteLine("The 'apple' command is not available on non-OSX platforms!");
                return (int)ExitCode.INVALID_ARGUMENTS;
            }
#endif

            // Mono.Options wouldn't allow "--" so we will temporarily rename it and parse it ourselves later
            args = args.Select(a => a == "--" ? VerbatimArgumentPlaceholder : a).ToArray();
        }

        var commands = GetXHarnessCommandSet();
        int result = commands.Run(args);

        string? exitCodeName = null;
        if (args.Length > 0 && result != 0 && Enum.IsDefined(typeof(ExitCode), result))
        {
            exitCodeName = $" ({(ExitCode)result})";
        }

        if (shouldOutput)
        {
            Console.WriteLine($"XHarness exit code: {result}{exitCodeName}");
        }

        return result;
    }

    public static CommandSet GetXHarnessCommandSet() => XHarnessRootCommandRegistry.CreateRootCommandSet();
}
