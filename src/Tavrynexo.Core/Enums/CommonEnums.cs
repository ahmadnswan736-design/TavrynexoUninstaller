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
    RegistryKey,
    RegistryValue,
    FileSystemDirectory,
    FileSystemFile,
    WindowsService,
    ScheduledTask,
    ShortcutOrStartMenuEntry,
    BrowserExtension,
    ComRegistrationClsid
}

public enum SafetyLevel
{
    SafeToPurge = 0,
    Safe = 0,
    ReviewRecommended = 1,
    Caution = 1,
    SystemProtected = 2,
    HighRisk = 2
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
