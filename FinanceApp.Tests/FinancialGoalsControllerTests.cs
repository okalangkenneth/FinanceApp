using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FinanceApp.Controllers;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinanceApp.Tests
{
    public class FinancialGoalsControllerTests
    {
        private const string Owner = "owner-user";
        private const string Attacker = "attacker-user";

        private static UpdateFinancialGoalViewModel ViewModelFor(int id, string title)
        {
            return new UpdateFinancialGoalViewModel
            {
                Id = id,
                Title = title,
                Description = "updated",
                TargetAmount = 2000m,
                CurrentAmount = 500m,
                StartDate = new System.DateTime(2023, 1, 1),
                EndDate = new System.DateTime(2024, 1, 1),
                Status = FinanceApp.Models.GoalStatus.InProgress,
                Currency = FinanceApp.Models.Currency.USD
            };
        }

        [Fact]
        public async Task UpdateFinancialGoal_Post_ByNonOwner_ReturnsNotFound_AndDoesNotModify()
        {
            using var context = TestHelpers.CreateContext();
            var goal = TestHelpers.NewGoal(Owner, title: "original");
            context.FinancialGoals.Add(goal);
            await context.SaveChangesAsync();

            var controller = new FinancialGoalsController(context, TestHelpers.MockUserManager(Attacker).Object);

            var result = await controller.UpdateFinancialGoal(ViewModelFor(goal.Id, "hijacked"));

            Assert.IsType<NotFoundResult>(result);
            Assert.Equal("original", context.FinancialGoals.Single(g => g.Id == goal.Id).Title);
        }

        [Fact]
        public async Task UpdateFinancialGoal_Post_ByOwner_Updates()
        {
            using var context = TestHelpers.CreateContext();
            var goal = TestHelpers.NewGoal(Owner, title: "original");
            context.FinancialGoals.Add(goal);
            await context.SaveChangesAsync();

            var controller = new FinancialGoalsController(context, TestHelpers.MockUserManager(Owner).Object);

            var result = await controller.UpdateFinancialGoal(ViewModelFor(goal.Id, "renamed"));

            Assert.IsType<RedirectToActionResult>(result);
            var stored = context.FinancialGoals.Single(g => g.Id == goal.Id);
            Assert.Equal("renamed", stored.Title);
            Assert.Equal(Owner, stored.UserId);
        }

        [Fact]
        public async Task UpdateFinancialGoal_Get_ByNonOwner_ReturnsNotFound()
        {
            using var context = TestHelpers.CreateContext();
            var goal = TestHelpers.NewGoal(Owner);
            context.FinancialGoals.Add(goal);
            await context.SaveChangesAsync();

            var controller = new FinancialGoalsController(context, TestHelpers.MockUserManager(Attacker).Object);

            var result = await controller.UpdateFinancialGoal(goal.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ByNonOwner_ReturnsNotFound_GoalSurvives()
        {
            using var context = TestHelpers.CreateContext();
            var goal = TestHelpers.NewGoal(Owner);
            context.FinancialGoals.Add(goal);
            await context.SaveChangesAsync();

            var controller = new FinancialGoalsController(context, TestHelpers.MockUserManager(Attacker).Object);
            TestHelpers.GiveTempData(controller);

            var result = await controller.DeleteConfirmed(goal.Id);

            Assert.IsType<NotFoundResult>(result);
            Assert.Single(context.FinancialGoals);
        }

        [Fact]
        public async Task DeleteConfirmed_ByOwner_Deletes()
        {
            using var context = TestHelpers.CreateContext();
            var goal = TestHelpers.NewGoal(Owner);
            context.FinancialGoals.Add(goal);
            await context.SaveChangesAsync();

            var controller = new FinancialGoalsController(context, TestHelpers.MockUserManager(Owner).Object);
            TestHelpers.GiveTempData(controller);

            var result = await controller.DeleteConfirmed(goal.Id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(context.FinancialGoals);
        }

        [Fact]
        public void UpdateFinancialGoal_Post_HasAntiForgeryValidation()
        {
            var method = typeof(FinancialGoalsController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m => m.Name == "UpdateFinancialGoal"
                             && m.GetCustomAttribute<HttpPostAttribute>() != null);

            Assert.NotNull(method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>());
        }
    }
}
