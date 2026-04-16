using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.DotNet.XHarness.Common;
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

    [Fact]
    public void ParseInstrumentationResult_SetsFilePullFailed_WhenResultXmlPullThrows()
    {
        var adbPath = Path.GetTempFileName();
        try
        {
            var pm = new Mock<IAdbProcessManager>();
            pm.SetupProperty(m => m.DeviceSerial, string.Empty);
            pm.Setup(m => m.Run(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<TimeSpan>()))
                .Throws(new IOException("simulated adb failure"));

            var adb = new AdbRunner(Mock.Of<ILogger>(), pm.Object, adbPath);
            var runner = new InstrumentationRunner(Mock.Of<ILogger>(), adb);

            var parseMi = typeof(InstrumentationRunner).GetMethod(
                "ParseInstrumentationResult",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(parseMi);

            const string stdout =
                "INSTRUMENTATION_RESULT: test-results-path=/data/local/tmp/out.xml\n" +
                "INSTRUMENTATION_RESULT: return-code=0\n";

            string outDir = Path.Combine(Path.GetTempPath(), "xh-instr-parse-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outDir);
            try
            {
                var produced = new List<DiagnosticsFile>();
                var tuple = ((int? ExitCode, bool Crashed, bool FilePullFailed))parseMi!.Invoke(
                    runner,
                    new object[] { "com.example.app", outDir, stdout, produced })!;

                Assert.True(tuple.FilePullFailed);
            }
            finally
            {
                try
                {
                    Directory.Delete(outDir, true);
                }
                catch
                {
                }
            }
        }
        finally
        {
            if (File.Exists(adbPath))
            {
                File.Delete(adbPath);
            }
        }
    }
}

