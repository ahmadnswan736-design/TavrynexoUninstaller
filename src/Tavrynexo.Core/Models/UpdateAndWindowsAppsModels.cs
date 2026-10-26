namespace Tavrynexo.Core.Models;

public enum DigitalSignatureStatus
{
    NotChecked,
    ValidTrustedPublisher,
    InvalidSignature,
    UntrustedRootOrRevoked,
    Unsigned
}

public class SoftwareUpdateItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ApplicationName { get; set; } = string.Empty;
    public string InstalledVersion { get; set; } = string.Empty;
    public string AvailableVersion { get; set; } = string.Empty;
    public string SourceCatalog { get; set; } = "Official Windows Package Manager (Winget)";
    public string PackageIdentifier { get; set; } = string.Empty;
    public DigitalSignatureStatus SignatureStatus { get; set; } = DigitalSignatureStatus.ValidTrustedPublisher;
    public bool IsSelectedForUpdate { get; set; } = true;
}

public class WindowsAppPackageItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DisplayName { get; set; } = string.Empty;
    public string PackageFullName { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public bool IsRemovable { get; set; } = true;
    public bool IsProtectedSystemComponent { get; set; }
    public string RestrictionReason { get; set; } = string.Empty;

    public string ActionAvailabilityText => IsProtectedSystemComponent
        ? "Protected System Component (Removal Restricted)"
        : "Removable User Application";
}
