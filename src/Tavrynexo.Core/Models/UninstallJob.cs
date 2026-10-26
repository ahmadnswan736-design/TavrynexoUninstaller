namespace Tavrynexo.Core.Models;

public enum JobStatus
{
    Queued,
    CreatingRestorePoint,
    ExecutingUninstaller,
    ScanningResidue,
    PurgingResidue,
    VerifyingOutcome,
    CompletedSuccess,
    CompletedWithWarnings,
    Failed,
    Cancelled
}

public class UninstallJob
{
    public string JobId { get; set; } = Guid.NewGuid().ToString("N");
    public InstalledApplication Application { get; set; } = null!;
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public int? ExitCode { get; set; }
    public string DiagnosticMessage { get; set; } = "Waiting in queue...";
    public bool QuietModeRequested { get; set; }
    public bool DeepResidueScanRequested { get; set; } = true;
    public bool IsVerifiedUninstalled { get; set; }
    public int ResiduePurgedCount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public static string TranslateExitCode(int code, bool isMsi)
    {
        return code switch
        {
            0 => "Success (Clean exit code 0)",
            3010 => "Success, restart required to complete removal (ERROR_SUCCESS_REBOOT_REQUIRED)",
            1602 => "User cancelled uninstallation (ERROR_INSTALL_USEREXIT)",
            1603 => "Fatal error occurred during uninstallation (ERROR_INSTALL_FAILURE)",
            1618 => "Another installation is currently in progress (ERROR_INSTALL_ALREADY_RUNNING)",
            1625 => "Uninstallation blocked by system policy (ERROR_INSTALL_PACKAGE_REJECTED)",
            1605 => "Action only valid for currently installed products (ERROR_UNKNOWN_PRODUCT)",
            _ => isMsi ? $"MSI Exit Code {code}" : $"Process Exit Code {code}"
        };
    }
}
