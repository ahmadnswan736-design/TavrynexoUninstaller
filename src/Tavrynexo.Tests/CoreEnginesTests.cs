using Tavrynexo.Core.Models;
using Tavrynexo.Data.Database;
using Tavrynexo.Engine.CoreEngines;
using Xunit;

namespace Tavrynexo.Tests;

public class CoreEnginesTests
{
    [Fact]
    public async Task ResidueDetectionEngine_LowEvidenceItem_MarkedUncertainAndUnselected()
    {
        var engine = new ResidueDetectionEngine();
        var app = new InstalledApplication { DisplayName = "DummyAppXYZ", Publisher = "Vendor123" };
        var results = await engine.EvaluateResidueAsync(app);
        foreach (var item in results.Where(r => r.Rating == ConfidenceRating.Uncertain))
        {
            Assert.False(item.IsSelectedForRemoval);
        }
    }

    [Fact]
    public async Task UninstallExecutionEngine_NonExistentExecutable_ReturnsFailedOutcome()
    {
        var engine = new UninstallExecutionEngine();
        var app = new InstalledApplication { DisplayName = "OrphanApp", UninstallString = @"C:\NonExistent_404\unins.exe" };
        var result = await engine.ExecuteUninstallAsync(app);
        Assert.Equal(ExecutionOutcome.ExecutionFailed, result.Outcome);
    }
}
