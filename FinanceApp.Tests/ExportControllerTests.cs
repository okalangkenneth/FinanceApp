using System.IO;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FinanceApp.Controllers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using Xunit;

namespace FinanceApp.Tests
{
    public class ExportControllerTests
    {
        private const string CurrentUser = "current-user";
        private const string OtherUser = "other-user";

        static ExportControllerTests()
        {
            // Same Community tier the app sets in Program.cs
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [Fact]
        public void ExportController_RequiresAuthorization()
        {
            var attributes = typeof(ExportController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

            Assert.NotEmpty(attributes);
        }

        [Fact]
        public async Task ExcelExport_ContainsOnlyCurrentUsersTransactions()
        {
            using var context = TestHelpers.CreateContext();
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(CurrentUser, 100m, TransactionType.Expense, TransactionCategory.Food),
                TestHelpers.NewTransaction(CurrentUser, 200m, TransactionType.Income, TransactionCategory.Salary),
                TestHelpers.NewTransaction(OtherUser, 999m, TransactionType.Expense, TransactionCategory.Rent),
                TestHelpers.NewTransaction(OtherUser, 888m, TransactionType.Expense, TransactionCategory.Health));
            await context.SaveChangesAsync();

            var controller = new ExportController(context, TestHelpers.MockUserManager(CurrentUser).Object);

            var result = controller.ExportTransactionsReportExcel();

            var file = Assert.IsType<FileContentResult>(result);
            using var stream = new MemoryStream(file.FileContents);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet("TransactionsReport");

            // 1 header row + exactly the current user's 2 transactions
            Assert.Equal(3, worksheet.LastRowUsed().RowNumber());
            Assert.Equal(100m, worksheet.Cell(2, 3).GetValue<decimal>());
            Assert.Equal(200m, worksheet.Cell(3, 3).GetValue<decimal>());
        }

        [Fact]
        public async Task PdfExport_ProducesPdf_ForCurrentUser()
        {
            using var context = TestHelpers.CreateContext();
            context.Transactions.AddRange(
                TestHelpers.NewTransaction(CurrentUser, 100m, TransactionType.Expense, TransactionCategory.Food),
                TestHelpers.NewTransaction(OtherUser, 999m, TransactionType.Expense, TransactionCategory.Rent));
            await context.SaveChangesAsync();

            var controller = new ExportController(context, TestHelpers.MockUserManager(CurrentUser).Object);

            var result = controller.ExportTransactionsReportPdf();

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.True(file.FileContents.Length > 0);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(file.FileContents, 0, 4));
        }
    }
}
