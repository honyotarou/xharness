// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using Microsoft.DotNet.XHarness.CLI.Android;
using Microsoft.DotNet.XHarness.CLI.AndroidHeadless;
using Microsoft.DotNet.XHarness.CLI.Commands;
using Microsoft.DotNet.XHarness.CLI.Commands.Apple;
using Microsoft.DotNet.XHarness.CLI.Commands.Wasm;
using Microsoft.DotNet.XHarness.CLI.Commands.Wasi;
using Mono.Options;

namespace Microsoft.DotNet.XHarness.CLI;

/// <summary>
/// Central registration of top-level <see cref="CommandSet"/> entries (encapsulates platform availability).
/// </summary>
internal static class XHarnessRootCommandRegistry
{
    public static CommandSet CreateRootCommandSet()
    {
#pragma warning disable IDE0028 // Simplify collection initialization for DEBUG
        var commandSet = new CommandSet("xharness");
#pragma warning restore IDE0028 // Simplify collection initialization for DEBUG

#if !DEBUG
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                commandSet.Add(new AppleCommandSet());
            }
#else
        commandSet.Add(new AppleCommandSet());
#endif

        commandSet.Add(new AndroidCommandSet());
        commandSet.Add(new AndroidHeadlessCommandSet());
        commandSet.Add(new WasmCommandSet());
        commandSet.Add(new WasiCommandSet());
        commandSet.Add(new XHarnessHelpCommand());
        commandSet.Add(new XHarnessVersionCommand());

        return commandSet;
    }
}
