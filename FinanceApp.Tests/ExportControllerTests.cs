using System.IO;
using System.Threading.Tasks;
using FinanceApp.Controllers;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Moq;
using OfficeOpenXml;
using Xunit;

namespace FinanceApp.Tests
{
    public class ExportControllerTests
    {
        private const string CurrentUser = "current-user";
        private const string OtherUser = "other-user";

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

            var controller = new ExportController(
                context,
                Mock.Of<ICompositeViewEngine>(),
                TestHelpers.MockUserManager(CurrentUser).Object);

            var result = controller.ExportTransactionsReportExcel();

            var file = Assert.IsType<FileContentResult>(result);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new MemoryStream(file.FileContents));
            var worksheet = package.Workbook.Worksheets[0];

            // 1 header row + exactly the current user's 2 transactions
            Assert.Equal(3, worksheet.Dimension.End.Row);
            Assert.Equal(100m, worksheet.Cells[2, 3].GetValue<decimal>());
            Assert.Equal(200m, worksheet.Cells[3, 3].GetValue<decimal>());
        }
    }
}
