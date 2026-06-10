using System;
using System.Linq;
using System.Threading.Tasks;
using FinanceApp.Controllers;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinanceApp.Tests
{
    public class ReportsControllerTests
    {
        private const string User = "report-user";

        // The regression this guards: income recorded under a non-income
        // category (e.g. Type=Income, Category=Other) used to be counted
        // as an expense because Reports classified by Category.

        [Fact]
        public async Task IncomeVsExpense_ClassifiesByTransactionType_NotCategory()
        {
            using var context = TestHelpers.CreateContext();
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(User, 1000m, TransactionType.Income, TransactionCategory.Other),
                TestHelpers.NewTransaction(User, 200m, TransactionType.Expense, TransactionCategory.Food));
            await context.SaveChangesAsync();

            var controller = new ReportsController(context, TestHelpers.MockUserManager(User).Object);

            var result = await controller.IncomeVsExpense();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncomeVsExpenseViewModel>(view.Model);
            Assert.Equal(1000m, model.TotalIncome);
            Assert.Equal(200m, model.TotalExpenses);
        }

        [Fact]
        public async Task MonthlyBudget_ClassifiesByTransactionType_NotCategory()
        {
            using var context = TestHelpers.CreateContext();
            var january = new DateTime(2023, 1, 10);
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(User, 1000m, TransactionType.Income, TransactionCategory.Other, january),
                TestHelpers.NewTransaction(User, 300m, TransactionType.Expense, TransactionCategory.Rent, january));
            await context.SaveChangesAsync();

            var controller = new ReportsController(context, TestHelpers.MockUserManager(User).Object);

            var result = await controller.MonthlyBudget();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyBudgetViewModel>(view.Model);
            var month = Assert.Single(model.MonthlyTotals);
            Assert.Equal(1000m, month.Income);
            Assert.Equal(300m, month.Expenses);
        }

        [Fact]
        public async Task CategoryBreakdown_IncludesOnlyExpenses()
        {
            using var context = TestHelpers.CreateContext();
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(User, 1000m, TransactionType.Income, TransactionCategory.Other),
                TestHelpers.NewTransaction(User, 150m, TransactionType.Expense, TransactionCategory.Food),
                TestHelpers.NewTransaction(User, 50m, TransactionType.Expense, TransactionCategory.Food));
            await context.SaveChangesAsync();

            var controller = new ReportsController(context, TestHelpers.MockUserManager(User).Object);

            var result = await controller.CategoryBreakdown();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CategoryBreakdownViewModel>(view.Model);
            var foodTotal = Assert.Single(model.CategoryTotals);
            Assert.Equal(TransactionCategory.Food, foodTotal.Category);
            Assert.Equal(200m, foodTotal.Total);
        }

        [Fact]
        public async Task Reports_ExcludeOtherUsersTransactions()
        {
            using var context = TestHelpers.CreateContext();
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(User, 100m, TransactionType.Income, TransactionCategory.Salary),
                TestHelpers.NewTransaction("someone-else", 5000m, TransactionType.Income, TransactionCategory.Salary));
            await context.SaveChangesAsync();

            var controller = new ReportsController(context, TestHelpers.MockUserManager(User).Object);

            var result = await controller.IncomeVsExpense();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncomeVsExpenseViewModel>(view.Model);
            Assert.Equal(100m, model.TotalIncome);
        }
    }
}
