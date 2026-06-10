using System;
using System.Security.Claims;
using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinanceApp.Tests
{
    public static class TestHelpers
    {
        public static ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        public static Mock<UserManager<ApplicationUser>> MockUserManager(string currentUserId)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var manager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            var user = new ApplicationUser { Id = currentUserId, PreferredCurrency = "USD" };
            manager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(currentUserId);
            manager.Setup(m => m.FindByIdAsync(currentUserId)).ReturnsAsync(user);
            manager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            return manager;
        }

        public static void GiveTempData(Controller controller)
        {
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        public static Transaction NewTransaction(
            string userId,
            decimal amount,
            TransactionType type,
            TransactionCategory category,
            DateTime? date = null)
        {
            return new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                Category = category,
                Date = date ?? new DateTime(2023, 1, 15),
                Description = "test transaction",
                PreferredCurrency = "USD",
                Currency = Currency.USD
            };
        }

        public static FinancialGoal NewGoal(string userId, string title = "goal", decimal target = 1000m)
        {
            return new FinancialGoal
            {
                UserId = userId,
                Title = title,
                Description = "test goal",
                TargetAmount = target,
                CurrentAmount = 100m,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2024, 1, 1),
                Status = GoalStatus.InProgress,
                Currency = Currency.USD
            };
        }
    }
}
