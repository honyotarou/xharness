// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.CLI;

public class XHarnessEnvironmentOptionsTests
{
    [Fact]
    public void ShouldLogIssuedCommandLine_RoundTripsEnvironmentVariable()
    {
        string prev = System.Environment.GetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName);
        try
        {
            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, null);
            Assert.True(XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine());

            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, "false");
            Assert.False(XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine());

            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, "0");
            Assert.False(XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine());

            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, "true");
            Assert.True(XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine());

            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, "1");
            Assert.True(XHarnessEnvironmentOptions.ShouldLogIssuedCommandLine());
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, prev);
        }
    }
}
