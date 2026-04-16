// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.SupplyChain;

/// <summary>
/// Ensures the 1ES template resource stays documented for digest pinning (anonymous clones cannot resolve ADO tags).
/// </summary>
public class AzurePipelinesOneEsTemplateTests
{
    [Fact]
    public void RootAzurePipelines_DocumentsDigestPinningForOneEsTemplate()
    {
        string yaml = File.ReadAllText(Path.Combine(FindRepoRoot(), "azure-pipelines.yml"));
        Assert.Contains("1ESPipelineTemplates", yaml, StringComparison.Ordinal);
        Assert.Contains("git ls-remote", yaml, StringComparison.OrdinalIgnoreCase);
        Assert.Matches(new Regex(@"ref:\s*(refs/tags/release|[0-9a-f]{40})", RegexOptions.IgnoreCase), yaml);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "azure-pipelines.yml")) &&
                File.Exists(Path.Combine(dir.FullName, "XHarness.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repo root (azure-pipelines.yml + XHarness.slnx).");
    }
}
