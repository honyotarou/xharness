// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.CLI;
using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests;

public class ProgramLoggingTests
{
    [Fact]
    public void Main_HonorsXHarnessLogIssuedCommandEnvironmentVariable()
    {
        string? prev = Environment.GetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName);
        TextWriter originalOut = Console.Out;
        try
        {
            var whenUnset = new StringWriter();
            Console.SetOut(whenUnset);
            Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, null);
            Program.Main(new[] { "help" });
            Console.SetOut(originalOut);
            string unsetOut = whenUnset.ToString();
            Assert.Contains("XHarness command issued", unsetOut);

            var whenFalse = new StringWriter();
            Console.SetOut(whenFalse);
            Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, "false");
            Program.Main(new[] { "help" });
            Console.SetOut(originalOut);
            string falseOut = whenFalse.ToString();
            Assert.DoesNotContain("XHarness command issued", falseOut);
        }
        finally
        {
            Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, prev);
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void Main_RedactsSensitiveArgvWhenLoggingIssuedCommand()
    {
        string? prev = Environment.GetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName);
        TextWriter originalOut = Console.Out;
        try
        {
            Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, null);
            var sw = new StringWriter();
            Console.SetOut(sw);
            Program.Main(new[] { "android", "help", "--access-token=secret123" });
            Console.SetOut(originalOut);
            string output = sw.ToString();
            Assert.Contains("XHarness command issued", output);
            Assert.Contains("--access-token=[REDACTED]", output);
            Assert.DoesNotContain("secret123", output);
        }
        finally
        {
            Environment.SetEnvironmentVariable(XHarnessEnvironmentOptions.LogIssuedCommandVariableName, prev);
            Console.SetOut(originalOut);
        }
    }
}
