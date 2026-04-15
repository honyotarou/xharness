using System.Reflection;
using Microsoft.DotNet.XHarness.Android;
using Microsoft.DotNet.XHarness.Android.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.DotNet.XHarness.Android.Tests;

public class InstrumentationRunnerOutputSecurityTests
{
    [Fact]
    public void ParseInstrumentationOutputs_ReturnCodePrefersFailureWhenDuplicated()
    {
        var adbPath = System.IO.Path.GetTempFileName();
        try
        {
            var adb = new AdbRunner(Mock.Of<ILogger>(), processManager: Mock.Of<IAdbProcessManager>(), adbExePath: adbPath);
            var runner = new InstrumentationRunner(Mock.Of<ILogger>(), adb);

            var mi = typeof(InstrumentationRunner).GetMethod("ParseInstrumentationOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(mi);

            string stdout =
                "INSTRUMENTATION_RESULT: return-code=1\n" +
                "INSTRUMENTATION_RESULT: return-code=0\n";

            var dict = (System.Collections.Generic.IReadOnlyDictionary<string, string>)mi!.Invoke(runner, new object[] { stdout })!;
            Assert.Equal("1", dict["return-code"]);
        }
        finally
        {
            if (System.IO.File.Exists(adbPath))
                System.IO.File.Delete(adbPath);
        }
    }

    [Fact]
    public void ParseInstrumentationOutputs_NegativeFailureIsNotDowngradedToZero()
    {
        var adbPath = System.IO.Path.GetTempFileName();
        try
        {
            var adb = new AdbRunner(Mock.Of<ILogger>(), processManager: Mock.Of<IAdbProcessManager>(), adbExePath: adbPath);
            var runner = new InstrumentationRunner(Mock.Of<ILogger>(), adb);

            var mi = typeof(InstrumentationRunner).GetMethod("ParseInstrumentationOutputs", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(mi);

            string stdout =
                "INSTRUMENTATION_RESULT: return-code=-1\n" +
                "INSTRUMENTATION_RESULT: return-code=0\n";

            var dict = (System.Collections.Generic.IReadOnlyDictionary<string, string>)mi!.Invoke(runner, new object[] { stdout })!;
            Assert.Equal("-1", dict["return-code"]);
        }
        finally
        {
            if (System.IO.File.Exists(adbPath))
                System.IO.File.Delete(adbPath);
        }
    }
}

