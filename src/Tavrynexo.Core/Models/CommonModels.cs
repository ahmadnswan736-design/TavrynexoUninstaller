using System;
using Tavrynexo.Core.Enums;

namespace Tavrynexo.Core.Models
{
    public class InstalledApplication
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DisplayName { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string DisplayVersion { get; set; } = string.Empty;
        public string InstallDate { get; set; } = string.Empty;
        public string InstallLocation { get; set; } = string.Empty;
        public string UninstallString { get; set; } = string.Empty;
        public string QuietUninstallString { get; set; } = string.Empty;
        public long EstimatedSizeKb { get; set; }
        public string IconPath { get; set; } = string.Empty;
        public bool Is64Bit { get; set; } = true;
        public InstallType InstallationType { get; set; } = InstallType.PerMachine;
    }

    public class LeftoverItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PathOrKey { get; set; } = string.Empty;
        public LeftoverType Type { get; set; }
        public SafetyLevel Safety { get; set; }
        public ConfidenceLevel Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
    }
}
