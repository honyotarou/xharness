// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;

namespace Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;

internal class BrowserLocationArgument : StringArgument
{
    public BrowserLocationArgument() : base("browser-path=", "Path to the browser to be used. This must correspond to the browser specified with -b")
    {
    }

    public override void Action(string argumentValue)
    {
        var rooted = RootPath(argumentValue);
        HostPathSecurity.ThrowIfUnsafeHostPath(rooted, nameof(BrowserLocationArgument));
        base.Action(rooted);
    }
}
