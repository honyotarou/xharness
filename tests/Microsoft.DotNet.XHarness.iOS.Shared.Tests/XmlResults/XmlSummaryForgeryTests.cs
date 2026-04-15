using System.IO;
using Microsoft.DotNet.XHarness.iOS.Shared.XmlResults;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests.XmlResults;

public class XmlSummaryForgeryTests
{
    [Fact]
    public void NUnitV2_ForgedSummaryFailuresZero_ButTestCaseFailure_FailsClosed()
    {
        var xml =
            "<test-results total=\"1\" errors=\"0\" failures=\"0\" not-run=\"0\" inconclusive=\"0\" ignored=\"0\" skipped=\"0\" invalid=\"0\">" +
            "  <test-suite type=\"TestFixture\" name=\"Security\" time=\"0\">" +
            "    <results>" +
            "      <test-case name=\"AuthBypass\" result=\"Failure\"><failure><message>x</message><stack-trace>y</stack-trace></failure></test-case>" +
            "    </results>" +
            "  </test-suite>" +
            "</test-results>";

        var parser = new NUnitV2ResultParser();
        var (_, failed) = parser.ParseXml(new StringReader(xml), null);
        Assert.True(failed);
    }

    [Fact]
    public void NUnitV3_ForgedSummaryFailedZero_ButTestCaseFailed_FailsClosed()
    {
        var xml =
            "<test-run testcasecount=\"1\" passed=\"1\" failed=\"0\" inconclusive=\"0\" skipped=\"0\">" +
            "  <test-suite type=\"TestFixture\" fullname=\"Security\" time=\"0\">" +
            "    <test-case name=\"AuthBypass\" result=\"Failed\"><failure><message>x</message><stack-trace>y</stack-trace></failure></test-case>" +
            "  </test-suite>" +
            "</test-run>";

        var parser = new NUnitV3ResultParser();
        var (_, failed) = parser.ParseXml(new StringReader(xml), null);
        Assert.True(failed);
    }
}

