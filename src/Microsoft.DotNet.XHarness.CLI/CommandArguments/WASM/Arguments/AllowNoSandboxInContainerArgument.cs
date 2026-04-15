// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.XHarness.CLI.CommandArguments.Wasm;

internal sealed class AllowNoSandboxInContainerArgument : SwitchArgument
{
    public AllowNoSandboxInContainerArgument()
        : base("allow-no-sandbox-in-container", "Allow adding Chrome's --no-sandbox automatically when running inside a container (unsafe; prefer keeping the sandbox enabled).", false)
    {
    }
}

