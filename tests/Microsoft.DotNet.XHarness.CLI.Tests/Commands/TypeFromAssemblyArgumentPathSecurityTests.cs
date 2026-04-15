// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.CLI.CommandArguments;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands;

/// <summary>
/// TDD: web-server middleware assembly paths must not contain traversal segments.
/// </summary>
public class TypeFromAssemblyArgumentPathSecurityTests
{
    [Fact]
    public void WebServerMiddlewareArgument_Validate_RejectsParentDirectoryInPath()
    {
        string temp = Path.Combine(Path.GetTempPath(), "xharness-mw-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string dllPath = Path.Combine(temp, "dummy.dll");
            File.WriteAllText(dllPath, "");
            string sneaky = Path.Combine(temp, "..", Path.GetFileName(temp), "dummy.dll");

            var arg = new WebServerMiddlewareArgument();
            arg.Action($"{sneaky},DoesNotMatter");

            var ex = Assert.Throws<ArgumentException>(() => arg.Validate());
            Assert.Contains("parent-directory", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try
            {
                Directory.Delete(temp, recursive: true);
            }
            catch
            {
            }
        }
    }
}
