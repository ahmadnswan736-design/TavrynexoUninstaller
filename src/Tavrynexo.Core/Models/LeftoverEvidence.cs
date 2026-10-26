using Tavrynexo.Core.Enums;

namespace Tavrynexo.Core.Models;

public enum ConfidenceRating
{
    HighCertainty,
    ModerateCertainty,
    Uncertain
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
    public int ConfidenceScore { get; set; }
    public SafetyLevel SafetyLevel { get => Safety; set => Safety = value; }
    public bool IsSelectedForRemoval { get => IsSelected; set => IsSelected = value; }

    public string RatingText => Rating switch
    {
        ConfidenceRating.HighCertainty => "High Certainty (Safe to purge)",
        ConfidenceRating.ModerateCertainty => "Moderate (Review suggested)",
        _ => "Uncertain (Unchecked by default)"
    };
}
