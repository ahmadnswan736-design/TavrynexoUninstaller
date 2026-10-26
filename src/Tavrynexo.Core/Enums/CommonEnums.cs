namespace Tavrynexo.Core.Enums
{
    public enum LeftoverType
    {
        File,
        Folder,
        RegistryKey,
        RegistryValue,
        Service,
        ScheduledTask,
        Driver,
        AppData,
        TempFile
    }

    public enum SafetyLevel
    {
        Safe,
        ReviewRecommended,
        Danger,
        SystemProtected
    }

    public enum ConfidenceLevel
    {
        Low,
        Medium,
        High,
        Definite
    }

    public enum InstallType
    {
        PerMachine,
        PerUser,
        MSI,
        InnoSetup,
        NSIS,
        Unknown
    }
}
