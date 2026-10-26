using Tavrynexo.Core.Enums;

namespace Tavrynexo.Core.Models;

public enum ForensicInferenceType
{
    ConfirmedBinaryMatch,
    ServiceExecutableAssociation,
    ScheduledTaskTargetAssociation,
    ShellExtensionClsidAssociation,
    RegistryPathInference,
    AppDataFolderInference
}

public class ForensicEvidenceLead
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public ForensicInferenceType InferenceType { get; set; }
    public LeftoverType ItemType { get; set; }
    public string TargetPath { get; set; } = string.Empty;
    public int ConfidenceScore { get; set; }
    public bool IsInferredOnly { get; set; } // تمييز الاستنتاج عن الرصد المؤكد
    public string EvidenceJustification { get; set; } = string.Empty;
    public bool IsSelectedForManualPurge { get; set; } = false; // غير محدد للحذف التلقائي نهائياً
    public SafetyLevel SafetyLevel { get; set; } = Tavrynexo.Core.Enums.SafetyLevel.ReviewRecommended;
}

public class ForensicAnalysisReport
{
    public string TargetAppName { get; set; } = string.Empty;
    public string TargetPublisher { get; set; } = string.Empty;
    public DateTime AnalysisTimeUtc { get; set; } = DateTime.UtcNow;
    public List<ForensicEvidenceLead> Findings { get; set; } = new();

    public List<string> AnalysisLimitations { get; set; } = new()
    {
        "Inferred correlations are probabilistic leads based on executable path references and publisher naming.",
        "Shared runtime assemblies cannot be confirmed without uninstaller manifests.",
        "Automatic batch deletion is strictly disabled to prevent unintended OS modification."
    };
}
