using System;
using System.IO;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Security;

public class RepositoryScriptHardeningTests
{
    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "eng", "common")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root (eng/common).");
    }

    [Fact]
    public void ArcadeInitToolsNative_DoesNotUseEvalOnUntrustedData()
    {
        var root = GetRepoRoot();
        var text = File.ReadAllText(Path.Combine(root, "eng", "common", "init-tools-native.sh"));
        Assert.DoesNotContain("eval \"${native_assets", text);
    }

    [Fact]
    public void InternalFeedOperations_DoesNotFetchFromMasterBranch()
    {
        var root = GetRepoRoot();
        var text = File.ReadAllText(Path.Combine(root, "eng", "common", "internal-feed-operations.sh"));
        Assert.DoesNotContain("/master/", text);
        Assert.Contains("raw.githubusercontent.com/microsoft/artifacts-credprovider/", text);
    }
}

