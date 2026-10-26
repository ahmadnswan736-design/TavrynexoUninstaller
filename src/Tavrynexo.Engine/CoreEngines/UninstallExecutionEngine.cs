using System.Diagnostics;
using Microsoft.Win32;
using Tavrynexo.Core.Models;
using Tavrynexo.Engine.Services;

namespace Tavrynexo.Engine.CoreEngines;

public enum ExecutionOutcome
{
    VerifiedSuccess,
    RebootRequiredToComplete,
    ProcessExitedNormallyVerificationUncertain,
    CancelledByUser,
    ExecutionFailed
}

public class ExecutionResult
{
    public ExecutionOutcome Outcome { get; set; }
    public int ExitCode { get; set; }
    public string DiagnosticMessage { get; set; } = string.Empty;
    public bool IsRebootPending { get; set; }
}

public interface IUninstallExecutionEngine
{
    Task<ExecutionResult> ExecuteUninstallAsync(InstalledApplication app, bool preferQuiet = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    bool CheckPendingSystemReboot();
}

public class UninstallExecutionEngine : IUninstallExecutionEngine
{
    public async Task<ExecutionResult> ExecuteUninstallAsync(InstalledApplication app, bool preferQuiet = false, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var result = new ExecutionResult();
        var plan = ProcessExecutionValidator.BuildSecureExecutionPlan(app, preferQuiet);
        if (!plan.IsValid)
        {
            result.Outcome = ExecutionOutcome.ExecutionFailed;
            result.DiagnosticMessage = plan.ValidationFailureReason ?? "Command validation failed.";
            return result;
        }

        var psi = new ProcessStartInfo
        {
            FileName = plan.ExecutablePath,
            Arguments = plan.Arguments,
            UseShellExecute = true,
            Verb = "runas",
            WindowStyle = preferQuiet ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
            {
                result.Outcome = ExecutionOutcome.ExecutionFailed;
                result.DiagnosticMessage = "Failed to launch uninstaller.";
                return result;
            }

            await process.WaitForExitAsync(cancellationToken);
            result.ExitCode = process.ExitCode;
            result.DiagnosticMessage = UninstallJob.TranslateExitCode(process.ExitCode, plan.IsMsiInstaller);
            result.IsRebootPending = CheckPendingSystemReboot() || process.ExitCode == 3010;

            if (result.ExitCode == 1602) result.Outcome = ExecutionOutcome.CancelledByUser;
            else if (result.IsRebootPending) result.Outcome = ExecutionOutcome.RebootRequiredToComplete;
            else if (result.ExitCode == 0) result.Outcome = ExecutionOutcome.VerifiedSuccess;
            else result.Outcome = ExecutionOutcome.ExecutionFailed;

            return result;
        }
        catch (OperationCanceledException)
        {
            result.Outcome = ExecutionOutcome.CancelledByUser;
            result.DiagnosticMessage = "Cancelled by user.";
            return result;
        }
        catch (Exception ex)
        {
            result.Outcome = ExecutionOutcome.ExecutionFailed;
            result.DiagnosticMessage = ex.Message;
            return result;
        }
    }

    public bool CheckPendingSystemReboot()
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            using var smKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager");
            if (smKey?.GetValue("PendingFileRenameOperations") != null) return true;
        }
        catch { }
        return false;
    }
}
