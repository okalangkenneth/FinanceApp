using FinanceApp.Models;
using Xunit;

namespace FinanceApp.Tests
{
    public class FinancialGoalTests
    {
        // Audit item 29: Progress divided by TargetAmount with no zero guard,
        // so a zero-target goal threw DivideByZeroException on render.

        [Fact]
        public void Progress_ZeroTarget_ReturnsZeroInsteadOfThrowing()
        {
            var goal = new FinancialGoal { TargetAmount = 0m, CurrentAmount = 500m };

            Assert.Equal(0m, goal.Progress);
        }

        [Theory]
        [InlineData(2500, 10000, 25)]
        [InlineData(10000, 10000, 100)]
        [InlineData(0, 10000, 0)]
        public void Progress_NonZeroTarget_ReturnsPercentage(decimal current, decimal target, decimal expected)
        {
            var goal = new FinancialGoal { TargetAmount = target, CurrentAmount = current };

            Assert.Equal(expected, goal.Progress);
        }
    }
}
