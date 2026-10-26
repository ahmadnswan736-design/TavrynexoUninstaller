namespace Tavrynexo.Core.Enums;

public enum ApplicationType
{
    Win32Registry,
    WindowsInstallerMsi,
    UniversalWindowsPlatformAppx,
    PortableOrManualDirectory,
    SystemComponent
}

public enum LeftoverType
{
    File,
    Folder,
    RegistryKey,
    RegistryValue,
    FileSystemDirectory,
    FileSystemFile,
    WindowsService,
    Service,
    ScheduledTask,
    ShortcutOrStartMenuEntry,
    BrowserExtension,
    ComRegistrationClsid,
    Driver,
    AppData,
    TempFile
}

public enum SafetyLevel
{
    SafeToPurge = 0,
    Safe = 0,
    ReviewRecommended = 1,
    Caution = 1,
    SystemProtected = 2,
    HighRisk = 2,
    Danger
}

public enum ScanDepth
{
    Quick,
    Standard,
    DeepThorough
}

public enum UninstallStage
{
    Idle,
    CreatingSystemRestorePoint,
    CreatingRegistryBackup,
    ExecutingNativeUninstaller,
    ScanningLeftovers,
    ReviewingLeftovers,
    PurgingResidue,
    Completed,
    Failed
}

public enum ConfidenceLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Certain = 3,
    Definite = 3
}

public enum InstallType
{
    PerMachine,
    PerUser,
    System,
    Portable,
    MSI,
    InnoSetup,
    NSIS,
    Unknown
}
