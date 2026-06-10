using System.Linq;
using System.Threading.Tasks;
using FinanceApp.Controllers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinanceApp.Tests
{
    public class TransactionsControllerTests
    {
        private const string Owner = "owner-user";
        private const string Attacker = "attacker-user";

        [Fact]
        public async Task Edit_Get_ByNonOwner_ReturnsNotFound()
        {
            using var context = TestHelpers.CreateContext();
            var transaction = TestHelpers.NewTransaction(Owner, 100m, TransactionType.Expense, TransactionCategory.Food);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var controller = new TransactionsController(context, TestHelpers.MockUserManager(Attacker).Object);

            var result = await controller.Edit(transaction.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ByNonOwner_ReturnsNotFound_AndDoesNotModify()
        {
            using var context = TestHelpers.CreateContext();
            var transaction = TestHelpers.NewTransaction(Owner, 100m, TransactionType.Expense, TransactionCategory.Food);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var controller = new TransactionsController(context, TestHelpers.MockUserManager(Attacker).Object);
            var tampered = TestHelpers.NewTransaction(Attacker, 999m, TransactionType.Income, TransactionCategory.Other);
            tampered.Id = transaction.Id;

            var result = await controller.Edit(transaction.Id, tampered);

            Assert.IsType<NotFoundResult>(result);
            var stored = context.Transactions.Single(t => t.Id == transaction.Id);
            Assert.Equal(100m, stored.Amount);
            Assert.Equal(Owner, stored.UserId);
        }

        [Fact]
        public async Task Edit_Post_ByOwner_UpdatesFields_ButNeverUserId()
        {
            using var context = TestHelpers.CreateContext();
            var transaction = TestHelpers.NewTransaction(Owner, 100m, TransactionType.Expense, TransactionCategory.Food);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var controller = new TransactionsController(context, TestHelpers.MockUserManager(Owner).Object);
            var update = TestHelpers.NewTransaction(null, 250m, TransactionType.Expense, TransactionCategory.Health);
            update.Id = transaction.Id;

            var result = await controller.Edit(transaction.Id, update);

            Assert.IsType<RedirectToActionResult>(result);
            var stored = context.Transactions.Single(t => t.Id == transaction.Id);
            Assert.Equal(250m, stored.Amount);
            Assert.Equal(TransactionCategory.Health, stored.Category);
            Assert.Equal(Owner, stored.UserId);
        }

        [Fact]
        public async Task DeleteConfirmed_ByNonOwner_ReturnsNotFound_RowSurvives()
        {
            using var context = TestHelpers.CreateContext();
            var transaction = TestHelpers.NewTransaction(Owner, 100m, TransactionType.Expense, TransactionCategory.Food);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var controller = new TransactionsController(context, TestHelpers.MockUserManager(Attacker).Object);

            var result = await controller.DeleteConfirmed(transaction.Id);

            Assert.IsType<NotFoundResult>(result);
            Assert.Single(context.Transactions);
        }

        [Fact]
        public async Task DeleteConfirmed_ByOwner_DeletesRow()
        {
            using var context = TestHelpers.CreateContext();
            var transaction = TestHelpers.NewTransaction(Owner, 100m, TransactionType.Expense, TransactionCategory.Food);
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var controller = new TransactionsController(context, TestHelpers.MockUserManager(Owner).Object);

            var result = await controller.DeleteConfirmed(transaction.Id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(context.Transactions);
        }
    }
}
