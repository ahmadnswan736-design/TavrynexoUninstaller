using Tavrynexo.Core.Enums;
using Tavrynexo.Core.Models;
using Tavrynexo.Core.Security;
using Tavrynexo.Engine.Services;

namespace Tavrynexo.Engine.CoreEngines;

public interface IResidueDetectionEngine
{
    Task<IReadOnlyList<DetailedLeftoverItem>> EvaluateResidueAsync(InstalledApplication app, ScanDepth depth = ScanDepth.Standard, IProgress<int>? progress = null, CancellationToken cancellationToken = default);
}

public class ResidueDetectionEngine : IResidueDetectionEngine
{
    private readonly MultiFactorLeftoverScanner _scanner;

    public ResidueDetectionEngine(MultiFactorLeftoverScanner? scanner = null)
    {
        _scanner = scanner ?? new MultiFactorLeftoverScanner();
    }

    public async Task<IReadOnlyList<DetailedLeftoverItem>> EvaluateResidueAsync(InstalledApplication app, ScanDepth depth = ScanDepth.Standard, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var rawItems = await _scanner.PerformMultiFactorScanAsync(app, progress, cancellationToken);
        var tunedItems = new List<DetailedLeftoverItem>();

        foreach (var item in rawItems)
        {
            // تقييد تأثير تشابه النصوص الصرف إلى 15% فقط
            int textSimilarityOnly = item.Evidences.Where(e => e.Type == EvidenceType.HeuristicNameSimilarity).Sum(e => e.Weight);
            int multiFactorScore = item.Evidences.Where(e => e.Type != EvidenceType.HeuristicNameSimilarity).Sum(e => e.Weight);
            int finalCompositeScore = multiFactorScore + Math.Min(textSimilarityOnly, 15);
            item.ConfidenceScore = Math.Min(finalCompositeScore, 100);

            if (item.SafetyLevel == SafetyLevel.SystemProtected)
            {
                item.Rating = ConfidenceRating.Uncertain;
                item.IsSelectedForRemoval = false;
                item.ExplanationWhyFlagged = "Protected system path. Modification forbidden.";
            }
            else if (item.ConfidenceScore >= 80)
            {
                item.Rating = ConfidenceRating.HighCertainty;
                item.IsSelectedForRemoval = true;
            }
            else
            {
                // إذا كانت الأدلة غير كافية (<80): تترك دون تحديد افتراضياً
                item.Rating = ConfidenceRating.Uncertain;
                item.IsSelectedForRemoval = false;
                item.ExplanationWhyFlagged = $"Inconclusive evidence ({item.ConfidenceScore}%). Unselected by default for safety.";
            }
            tunedItems.Add(item);
        }
        return tunedItems;
    }
}
