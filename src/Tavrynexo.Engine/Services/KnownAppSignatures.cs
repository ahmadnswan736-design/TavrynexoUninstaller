using Tavrynexo.Core.Models;

namespace Tavrynexo.Engine.Services;

public static class KnownAppSignatures
{
    public static List<LeftoverEvidence> CheckKnownRules(string appName, string publisher, string testPath)
    {
        var evidences = new List<LeftoverEvidence>();
        string lowerPath = testPath.ToLowerInvariant();
        string lowerApp = appName.ToLowerInvariant();

        if (lowerPath.Contains("gpucache") || lowerPath.Contains("code cache") || lowerPath.Contains("ebwebview"))
        {
            if (lowerPath.Contains(lowerApp))
            {
                evidences.Add(new LeftoverEvidence
                {
                    Type = EvidenceType.AppSpecificCleaningRule,
                    Description = "Matches Electron/Chromium runtime cache directory signature for target application.",
                    Weight = 30
                });
            }
        }
        if (lowerPath.Contains("appdata\\locallow") && lowerPath.Contains(lowerApp))
        {
            evidences.Add(new LeftoverEvidence
            {
                Type = EvidenceType.AppSpecificCleaningRule,
                Description = "Matches persistent LocalLow storage location used by modern media/game engines.",
                Weight = 35
            });
        }
        return evidences;
    }
}
