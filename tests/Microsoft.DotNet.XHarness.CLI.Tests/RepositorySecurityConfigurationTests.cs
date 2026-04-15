// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests;

public class RepositorySecurityConfigurationTests
{
    [Fact]
    public void CommonVariablesYaml_ConfiguresXHarnessLogIssuedCommand()
    {
        string path = Path.Combine(RepositoryRootLocator.FindRepositoryRoot(), "eng", "common-variables.yml");
        Assert.True(File.Exists(path), $"Expected {path}");
        string yaml = File.ReadAllText(path);
        Assert.Contains("XHARNESS_LOG_ISSUED_COMMAND", yaml);
        Assert.Contains("false", yaml);
    }

    [Fact]
    public void E2eTestTemplate_ConfiguresHelixLogIssuedCommand()
    {
        string path = Path.Combine(RepositoryRootLocator.FindRepositoryRoot(), "eng", "e2e-test.yml");
        Assert.True(File.Exists(path), $"Expected {path}");
        string yaml = File.ReadAllText(path);
        Assert.Contains("XHARNESS_LOG_ISSUED_COMMAND", yaml);
        Assert.Contains("false", yaml);
    }

    private static class RepositoryRootLocator
    {
        public static string FindRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "eng", "common-variables.yml");
                if (File.Exists(candidate))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new InvalidOperationException("Repository root (eng/common-variables.yml) not found.");
        }
    }
}
