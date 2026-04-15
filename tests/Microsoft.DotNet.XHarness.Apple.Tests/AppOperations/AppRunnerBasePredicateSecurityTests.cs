using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.Apple;
using Microsoft.DotNet.XHarness.Common.Execution;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.iOS.Shared.Execution;
using Microsoft.DotNet.XHarness.iOS.Shared.Logging;
using Microsoft.DotNet.XHarness.iOS.Shared.Utilities;
using Moq;
using Xunit;

namespace Microsoft.DotNet.XHarness.Apple.Tests.AppOperations;

public class AppRunnerBasePredicateSecurityTests
{
    [Fact]
    public async Task CaptureSimulatorLog_EscapesSingleQuotesInPredicate()
    {
        var pm = new Mock<IMlaunchProcessManager>(MockBehavior.Strict);
        var captureFactory = new Mock<ICaptureLogFactory>();
        var logs = new Mock<ILogs>();
        var mainLog = new Mock<IFileBackedLog>();
        var helpers = new Mock<IHelpers>();

        logs
            .Setup(l => l.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>()))
            .Returns(Mock.Of<IFileBackedLog>(l => l.FullPath == "/tmp/fake.log"));

        pm.Setup(p => p.ExecuteXcodeCommandAsync(
                "simctl",
                It.Is<string[]>(a => string.Join(" ", a).Contains("--predicate", System.StringComparison.Ordinal) &&
                                     string.Join(" ", a).Contains("senderImagePath contains 'test\\' OR 1==1 OR senderImagePath contains \\'", System.StringComparison.Ordinal)),
                It.IsAny<ILog>(),
                It.IsAny<ILog>(),
                It.IsAny<ILog>(),
                It.IsAny<System.TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new ProcessExecutionResult { ExitCode = 0 }));

        var runner = new AppRunnerBaseProbe(pm.Object, captureFactory.Object, logs.Object, mainLog.Object, helpers.Object);
        var sim = Mock.Of<Microsoft.DotNet.XHarness.iOS.Shared.Hardware.ISimulatorDevice>(s =>
            s.UDID == "udid" &&
            s.Name == "sim" &&
            s.Boot(It.IsAny<ILog>(), It.IsAny<CancellationToken>()) == Task.FromResult(true));

        await runner.CallCaptureSimulatorLog(sim, "test' OR 1==1 OR senderImagePath contains '", CancellationToken.None);

        pm.VerifyAll();
    }

    private sealed class AppRunnerBaseProbe : AppRunnerBase
    {
        public AppRunnerBaseProbe(IMlaunchProcessManager processManager, ICaptureLogFactory captureLogFactory, ILogs logs, IFileBackedLog mainLog, IHelpers helpers)
            : base(processManager, captureLogFactory, logs, mainLog, helpers)
        {
        }

        public Task<CancellationTokenSource> CallCaptureSimulatorLog(Microsoft.DotNet.XHarness.iOS.Shared.Hardware.ISimulatorDevice simulator, string appName, CancellationToken ct)
        {
            var info = new Microsoft.DotNet.XHarness.iOS.Shared.AppBundleInformation(appName, "id", "/tmp", "/tmp", false);
            return CaptureSimulatorLog(simulator, info, ct);
        }
    }
}

