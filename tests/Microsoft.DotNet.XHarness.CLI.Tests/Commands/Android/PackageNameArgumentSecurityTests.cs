using System;
using Microsoft.DotNet.XHarness.CLI.CommandArguments.Android;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands.Android;

public class PackageNameArgumentSecurityTests
{
    [Theory]
    [InlineData("com.example.app")]
    [InlineData("a.b")]
    [InlineData("com.example.app_1")]
    [InlineData("Com.Example.App")]
    public void Validate_AllowsSafeAndroidPackageNames(string value)
    {
        var arg = new PackageNameArgument();
        arg.Action(value);
        arg.Validate();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("-com.example.app")]
    [InlineData("com")]
    [InlineData("com..example")]
    [InlineData("com.example.")]
    [InlineData("com.example.app;rm -rf /")]
    [InlineData("com.example.app/Other")]
    [InlineData("com.example.app Other")]
    [InlineData("com.example.app\nx")]
    public void Validate_RejectsUnsafeAndroidPackageNames(string value)
    {
        var arg = new PackageNameArgument();
        arg.Action(value);
        Assert.Throws<ArgumentException>(() => arg.Validate());
    }
}

