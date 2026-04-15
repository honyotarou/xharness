using System;
using System.IO;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Security;

public class RepositoryNuGetHardeningTests
{
    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "NuGet.config")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root (NuGet.config).");
    }

    [Fact]
    public void NuGetConfig_HasPackageSourceMapping()
    {
        var root = GetRepoRoot();
        var text = File.ReadAllText(Path.Combine(root, "NuGet.config"));
        Assert.Contains("<packageSourceMapping>", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("packageSource key=\"dotnet-public\"", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Repo_HasSignCheckExclusionsFile()
    {
        var root = GetRepoRoot();
        Assert.True(File.Exists(Path.Combine(root, "eng", "SignCheckExclusionsFile.txt")));
    }
}

