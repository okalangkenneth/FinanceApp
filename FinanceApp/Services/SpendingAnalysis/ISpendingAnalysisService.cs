using System.Threading;
using System.Threading.Tasks;

namespace FinanceApp.Services.SpendingAnalysis
{
    public interface ISpendingAnalysisService
    {
        Task<string> AnalyzeSpendingHabitsAsync(string prompt, CancellationToken cancellationToken = default);

        Task<string> GenerateRecommendationsAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
