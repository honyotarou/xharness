// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.DotNet.XHarness.Common.Logging;
using Microsoft.DotNet.XHarness.iOS.Shared.Execution;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Logging;

public interface IDeviceLogCapturer : IDisposable
{
    void StartCapture();
    void StopCapture();
}

public class DeviceLogCapturer : IDeviceLogCapturer
{
    private const string AllowSudoLogCollectEnv = "XHARNESS_ALLOW_SUDO_LOG_COLLECT";
    private readonly ILog _mainLog;
    private readonly ILog _deviceLog;
    private readonly string _deviceUdid;
    private readonly string _outputPath;
    private DateTime _startTime;

    public DeviceLogCapturer(ILog mainLog, ILog deviceLog, string deviceUdid)
    {
        _mainLog = mainLog ?? throw new ArgumentNullException(nameof(mainLog));
        _deviceLog = deviceLog ?? throw new ArgumentNullException(nameof(deviceLog));
        _deviceUdid = deviceUdid ?? throw new ArgumentNullException(nameof(deviceUdid));

        // User-private directory (avoids world-writable shared /tmp on some Unix installs).
        string baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "xharness",
            "device_logs");
        Directory.CreateDirectory(baseDir);
        _outputPath = Path.Combine(baseDir, $"device_logs_{Guid.NewGuid():N}.logarchive");
    }

    public void StartCapture()
    {
        _startTime = DateTime.Now;
        _deviceLog.WriteLine($"Device log capture started at {_startTime:yyyy-MM-dd HH:mm:ss}");
    }

    public void StopCapture()
    {
        _deviceLog.WriteLine($"Device log capture stopped at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        string startTimeStr = _startTime.ToString("yyyy-MM-dd HH:mm:ss");

        // Collect logs. Use a timeout to avoid hanging indefinitely if the device
        // becomes unresponsive (e.g. tvOS devices with broken log streaming).
        const int processTimeoutMs = 120_000; // 2 minutes

        bool allowSudo =
            string.Equals(Environment.GetEnvironmentVariable(AllowSudoLogCollectEnv)?.Trim(), "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Environment.GetEnvironmentVariable(AllowSudoLogCollectEnv)?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        if (!allowSudo)
        {
            _mainLog.WriteLine($"Skipping device log collection because it requires sudo. Set {AllowSudoLogCollectEnv}=1 to enable.");
            CleanupOutputPath();
            return;
        }

        _deviceLog.WriteLine($"Collecting logs: sudo log collect --device-udid <udid> --start \"{startTimeStr}\" --output \"{_outputPath}\"");

        using Process collectProcess = new Process();
        collectProcess.StartInfo.FileName = "sudo";
        collectProcess.StartInfo.UseShellExecute = false;
        collectProcess.StartInfo.ArgumentList.Add("log");
        collectProcess.StartInfo.ArgumentList.Add("collect");
        collectProcess.StartInfo.ArgumentList.Add("--device-udid");
        collectProcess.StartInfo.ArgumentList.Add(_deviceUdid);
        collectProcess.StartInfo.ArgumentList.Add("--start");
        collectProcess.StartInfo.ArgumentList.Add(startTimeStr);
        collectProcess.StartInfo.ArgumentList.Add("--output");
        collectProcess.StartInfo.ArgumentList.Add(_outputPath);
        collectProcess.StartInfo.RedirectStandardOutput = true;
        collectProcess.StartInfo.RedirectStandardError = true;

        StringBuilder collectOutput = new StringBuilder();
        StringBuilder collectErrors = new StringBuilder();

        collectProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                collectOutput.AppendLine(e.Data);
        };

        collectProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                collectErrors.AppendLine(e.Data);
        };

        collectProcess.Start();
        collectProcess.BeginOutputReadLine();
        collectProcess.BeginErrorReadLine();

        if (!collectProcess.WaitForExit(processTimeoutMs))
        {
            _mainLog.WriteLine($"Device log collection timed out after {processTimeoutMs / 1000}s. Killing process and skipping log reading.");
            try { collectProcess.Kill(entireProcessTree: true); } catch { /* best effort */ }
            CleanupOutputPath();
            return;
        }

        // Ensure all asynchronous output/error reads have completed before consuming buffers
        collectProcess.WaitForExit();

        if (collectErrors.Length > 0)
        {
            _mainLog.WriteLine($"Errors during log collection: {collectErrors}");

            if (collectProcess.ExitCode != 0)
            {
                _deviceLog.WriteLine($"Log collection failed with exit code {collectProcess.ExitCode}. Skipping log reading.");
                CleanupOutputPath();
                return;
            }
        }

        // Read the collected logs
        _deviceLog.WriteLine($"Reading logs: log show \"{_outputPath}\"");

        using Process readProcess = new Process();
        readProcess.StartInfo.FileName = "log";
        readProcess.StartInfo.UseShellExecute = false;
        readProcess.StartInfo.ArgumentList.Add("show");
        readProcess.StartInfo.ArgumentList.Add(_outputPath);
        readProcess.StartInfo.RedirectStandardOutput = true;
        readProcess.StartInfo.RedirectStandardError = true;

        StringBuilder output = new StringBuilder();
        StringBuilder errors = new StringBuilder();

        readProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                output.AppendLine(e.Data);
        };

        readProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                errors.AppendLine(e.Data);
        };

        readProcess.Start();
        readProcess.BeginOutputReadLine();
        readProcess.BeginErrorReadLine();

        if (!readProcess.WaitForExit(processTimeoutMs))
        {
            _mainLog.WriteLine($"Device log reading timed out after {processTimeoutMs / 1000}s. Killing process.");
            try { readProcess.Kill(entireProcessTree: true); } catch { /* best effort */ }
        }
        else
        {
            // Ensure all asynchronous output/error reads have completed before consuming buffers
            readProcess.WaitForExit();

            if (output.Length > 0)
            {
                lock (_deviceLog)
                {
                    _deviceLog.WriteLine(output.ToString());
                }
            }

            if (errors.Length > 0)
            {
                _mainLog.WriteLine($"Errors while reading device logs: {errors}");
            }
        }

        CleanupOutputPath();
    }

    private void CleanupOutputPath()
    {
        if (Directory.Exists(_outputPath))
        {
            Directory.Delete(_outputPath, true);
        }
    }

    public void Dispose() => StopCapture();
}

