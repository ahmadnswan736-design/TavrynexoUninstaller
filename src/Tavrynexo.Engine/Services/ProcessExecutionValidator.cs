using System.Diagnostics;
using Microsoft.Win32;
using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public class ExecutionPlan
{
    public string ExecutablePath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ValidationFailureReason { get; set; }
    public bool IsMsiInstaller { get; set; }
}

public static class ProcessExecutionValidator
{
    public static ExecutionPlan BuildSecureExecutionPlan(InstalledApplication app, bool preferQuiet)
    {
        var plan = new ExecutionPlan();

        if (app.AppType == ApplicationType.WindowsInstallerMsi && !string.IsNullOrWhiteSpace(app.ProductCodeGuid))
        {
            plan.IsMsiInstaller = true;
            plan.ExecutablePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
            plan.Arguments = preferQuiet ? $"/x {app.ProductCodeGuid} /qn /norestart" : $"/x {app.ProductCodeGuid}";
            plan.IsValid = true;
            return plan;
        }

        string rawCommand = (preferQuiet && !string.IsNullOrWhiteSpace(app.QuietUninstallString))
            ? app.QuietUninstallString : app.UninstallString;

        if (string.IsNullOrWhiteSpace(rawCommand))
        {
            plan.IsValid = false;
            plan.ValidationFailureReason = "No registered uninstaller command found in Windows Registry.";
            return plan;
        }

        string expanded = Environment.ExpandEnvironmentVariables(rawCommand.Trim());
        var (exe, args) = ParseExecutableAndArguments(expanded);

        if (string.IsNullOrWhiteSpace(exe))
        {
            plan.IsValid = false;
            plan.ValidationFailureReason = "Unable to resolve target executable from command.";
            return plan;
        }

        string fileName = Path.GetFileName(exe);
        if (fileName.Equals("msiexec.exe", StringComparison.OrdinalIgnoreCase) || fileName.Equals("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            plan.IsMsiInstaller = true;
            plan.ExecutablePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
            plan.Arguments = args;
            plan.IsValid = true;
            return plan;
        }

        if (!File.Exists(exe))
        {
            plan.IsValid = false;
            plan.ValidationFailureReason = $"Uninstaller executable does not exist at: '{exe}'";
            return plan;
        }

        plan.ExecutablePath = exe;
        plan.Arguments = args;
        plan.IsValid = true;
        return plan;
    }

    private static (string ExePath, string Arguments) ParseExecutableAndArguments(string commandLine)
    {
        string trimmed = commandLine.Trim();
        if (trimmed.StartsWith("\""))
        {
            int closingQuote = trimmed.IndexOf('"', 1);
            if (closingQuote != -1)
            {
                return (trimmed.Substring(1, closingQuote - 1).Trim(), trimmed.Substring(closingQuote + 1).Trim());
            }
        }

        int lastIndex = 0;
        while ((lastIndex = trimmed.IndexOf(' ', lastIndex + 1)) != -1)
        {
            string candidateExe = trimmed.Substring(0, lastIndex);
            if (File.Exists(candidateExe))
            {
                return (candidateExe, trimmed.Substring(lastIndex + 1).Trim());
            }
        }

        if (File.Exists(trimmed)) return (trimmed, string.Empty);
        int firstSpace = trimmed.IndexOf(' ');
        if (firstSpace != -1) return (trimmed.Substring(0, firstSpace), trimmed.Substring(firstSpace + 1).Trim());
        return (trimmed, string.Empty);
    }

    public static bool VerifyUninstallationCompleted(InstalledApplication app)
    {
        if (!string.IsNullOrWhiteSpace(app.RegistryKeyPath))
        {
            try
            {
                int slash = app.RegistryKeyPath.IndexOf('\\');
                if (slash != -1)
                {
                    string root = app.RegistryKeyPath.Substring(0, slash);
                    string rest = app.RegistryKeyPath.Substring(slash + 1);
                    var hive = root.Contains("CURRENT_USER") ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;
                    using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                    using var test = baseKey.OpenSubKey(rest);
                    if (test != null) return false;
                }
            }
            catch { }
        }
        if (!string.IsNullOrWhiteSpace(app.InstallLocation) && Directory.Exists(app.InstallLocation))
        {
            if (Directory.GetFiles(app.InstallLocation, "*.exe", SearchOption.TopDirectoryOnly).Length > 0)
                return false;
        }
        return true;
    }
}
