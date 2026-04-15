// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.DotNet.XHarness.Common;
using Microsoft.DotNet.XHarness.iOS.Shared.Logging;
using Microsoft.DotNet.XHarness.iOS.Shared.XmlResults;
using Moq;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.XmlResults;

public class XmlResultParserPathSecurityTests
{
    private readonly XmlResultParser _parser = new();

    [Theory]
    [InlineData("/tmp/../etc/passwd")]
    [InlineData("..\\secret")]
    public void IsValidXml_RejectsUnsafePath(string path)
    {
        Assert.Throws<ArgumentException>(() => _parser.IsValidXml(path, out _));
    }

    [Fact]
    public void CleanXml_RejectsUnsafePaths()
    {
        Assert.Throws<ArgumentException>(() => _parser.CleanXml("/tmp/../a", "/tmp/out"));
        Assert.Throws<ArgumentException>(() => _parser.CleanXml("/tmp/a", "/tmp/../out"));
    }

    [Fact]
    public void ParseResults_RejectsUnsafeSourcePath()
    {
        Assert.Throws<ArgumentException>(() => _parser.ParseResults("/tmp/../x.xml", XmlResultJargon.NUnitV3, default(string)));
    }

    [Fact]
    public void ParseResults_RejectsUnsafeHumanReadablePath()
    {
        Assert.Throws<ArgumentException>(() => _parser.ParseResults("/tmp/a.xml", XmlResultJargon.NUnitV3, "/tmp/../hr.txt"));
    }

    [Fact]
    public void GenerateTestReport_RejectsUnsafeResultsPath()
    {
        using var w = new StringWriter();
        Assert.Throws<ArgumentException>(() => _parser.GenerateTestReport(w, "/tmp/../x.xml", XmlResultJargon.NUnitV2));
    }

    [Fact]
    public void UpdateMissingData_RejectsUnsafePaths()
    {
        Assert.Throws<ArgumentException>(() => _parser.UpdateMissingData("/tmp/../a", "/tmp/b", "app", Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => _parser.UpdateMissingData("/tmp/a", "/tmp/../b", "app", Array.Empty<string>()));
    }

    [Fact]
    public void GetXmlFilePath_RejectsUnsafePath()
    {
        Assert.Throws<ArgumentException>(() => _parser.GetXmlFilePath("/tmp/../r.xml", XmlResultJargon.NUnitV2));
    }

    [Fact]
    public void GetVSTSFilename_RejectsUnsafePath()
    {
        Assert.Throws<ArgumentException>(() => XmlResultParser.GetVSTSFilename("/tmp/../v.xml"));
    }

    [Fact]
    public void GenerateFailure_RejectsUnsafeStderrPath()
    {
        var logs = new Mock<ILogs>();
        Assert.Throws<ArgumentException>(() => _parser.GenerateFailure(
            logs.Object,
            "src",
            "app",
            null,
            "t",
            "m",
            "/tmp/../stderr.txt",
            XmlResultJargon.NUnitV2));
        logs.VerifyNoOtherCalls();
    }
}
