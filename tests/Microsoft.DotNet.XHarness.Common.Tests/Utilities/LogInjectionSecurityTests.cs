using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class LogInjectionSecurityTests
{
    [Fact]
    public void Sanitize_StripsAnsiEscapes_AndNeutralizesMarkers()
    {
        var input = "\x1b[2J\x1b[H" + Microsoft.DotNet.XHarness.Common.RunSummaryEmitter.JsonStartMarker + "\rOK";
        var s = LogInjectionSecurity.Sanitize(input);
        Assert.True(s.IndexOf('\u001B') < 0, $"Found ESC at index {s.IndexOf('\u001B')}");
        Assert.DoesNotContain(Microsoft.DotNet.XHarness.Common.RunSummaryEmitter.JsonStartMarker, s);
        Assert.Contains("[XHARNESS_RESULT_START]", s);
        Assert.Contains("\\r", s);
    }

    [Fact]
    public void Sanitize_StripsSgrColorSequence_BeforeMarker()
    {
        var raw = "\u001b[31mred" + Microsoft.DotNet.XHarness.Common.RunSummaryEmitter.JsonStartMarker;
        var s = LogInjectionSecurity.Sanitize(raw);
        Assert.Equal(-1, s.IndexOf('\u001b'));
        Assert.DoesNotContain(Microsoft.DotNet.XHarness.Common.RunSummaryEmitter.JsonStartMarker, s);
    }
}

