// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Xml;
using Microsoft.DotNet.XHarness.Common;
using Microsoft.DotNet.XHarness.iOS.Shared.XmlResults;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.XmlResults;

/// <summary>
/// Cycles 2–4 — attacker: poison CI with test-result XML containing a DTD (XXE / expansion surface).
/// Parsers must use <see cref="Microsoft.DotNet.XHarness.Common.Xml.SecureXmlReaderSettings"/>.
/// </summary>
public class UntrustedXmlResultParsingSecurityTests
{
    private const string XmlDoctype = "<?xml version=\"1.0\"?>\n<!DOCTYPE foo [<!ELEMENT foo ANY>]>\n";

    [Fact]
    public void TouchUnit_ParseXml_WithDoctype_ThrowsXmlException()
    {
        var xml = XmlDoctype
            + "<test-results total=\"0\" errors=\"0\" failures=\"0\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\"></test-results>";
        var parser = new TouchUnitResultParser();
        Assert.Throws<XmlException>(() => parser.ParseXml(new StringReader(xml), null));
    }

    [Fact]
    public void Trx_ParseXml_WithDoctype_ThrowsXmlException()
    {
        var xml = XmlDoctype
            + "<TestRun xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\"></TestRun>";
        var parser = new TrxResultParser();
        Assert.Throws<XmlException>(() => parser.ParseXml(new StringReader(xml), null));
    }

    [Fact]
    public void XUnit_ParseXml_WithDoctype_ThrowsXmlException()
    {
        var xml = XmlDoctype + "<assemblies></assemblies>";
        var parser = new XUnitResultParser();
        Assert.Throws<XmlException>(() => parser.ParseXml(new StringReader(xml), null));
    }

    [Fact]
    public void NUnitV3_ParseXml_WithDoctype_ThrowsXmlException()
    {
        var xml = XmlDoctype
            + "<test-run testcasecount=\"0\" passed=\"0\" failed=\"0\" inconclusive=\"0\" skipped=\"0\"></test-run>";
        var parser = new NUnitV3ResultParser();
        Assert.Throws<XmlException>(() => parser.ParseXml(new StringReader(xml), null));
    }

    [Fact]
    public void XmlResultParser_GenerateTestReport_WithDoctype_ThrowsXmlException()
    {
        var xml = XmlDoctype
            + "<test-results total=\"0\" errors=\"0\" failures=\"0\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\"></test-results>";
        var parser = new XmlResultParser();
        using var writer = new StringWriter();
        using var reader = new StringReader(xml);
        Assert.Throws<XmlException>(() => parser.GenerateTestReport(writer, reader, XmlResultJargon.NUnitV2));
    }
}
