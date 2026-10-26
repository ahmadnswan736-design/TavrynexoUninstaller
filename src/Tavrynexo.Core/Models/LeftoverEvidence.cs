using Tavrynexo.Core.Enums;

namespace Tavrynexo.Core.Models;

public enum ConfidenceRating
{
    HighCertainty,     // 90-100%: أدلة قاطعة ومطابقة كاملة لبيانات الناشر أو InProcServer
    ModerateCertainty, // 70-89%: دليل قوي من مجلد فرعي للناشر
    Uncertain          // <70%: أدلة غير كافية، يُترك غير محدد افتراضياً لحماية النظام
}

public enum EvidenceType
{
    ExactBinaryTargetMatch,
    ClsidInProcServerMatch,
    PublisherAndAppFolderMatch,
    RegistryValueReferenceMatch,
    MsiComponentGuidMatch,
    AppSpecificCleaningRule,
    HeuristicNameSimilarity
}

public class LeftoverEvidence
{
    public EvidenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class DetailedLeftoverItem : LeftoverItem
{
    public ConfidenceRating Rating { get; set; } = ConfidenceRating.HighCertainty;
    public List<LeftoverEvidence> Evidences { get; set; } = new();
    public string ExplanationWhyFlagged { get; set; } = string.Empty;
    public bool IsSharedDependencyRisk { get; set; }

    public string RatingText => Rating switch
    {
        ConfidenceRating.HighCertainty => "High Certainty (Safe to purge)",
        ConfidenceRating.ModerateCertainty => "Moderate (Review suggested)",
        _ => "Uncertain (Unchecked by default)"
    };
}
