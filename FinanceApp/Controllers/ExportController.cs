using ClosedXML.Excel;
using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExportController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult ExportTransactionsReportExcel()
        {
            // Retrieve data for the report — current user's rows only
            var userId = _userManager.GetUserId(User);
            var transactions = _context.Transactions
                .Where(t => t.UserId == userId)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TransactionsReport");

            // Add header row
            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Description";
            worksheet.Cell(1, 3).Value = "Amount";
            worksheet.Cell(1, 4).Value = "Type";
            worksheet.Cell(1, 5).Value = "Category";

            // Add data rows
            int row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cell(row, 1).Value = transaction.Date;
                worksheet.Cell(row, 2).Value = transaction.Description;
                worksheet.Cell(row, 3).Value = transaction.Amount;
                worksheet.Cell(row, 4).Value = transaction.Type.ToString();
                worksheet.Cell(row, 5).Value = transaction.Category.ToString();
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "TransactionsReport.xlsx";
            return File(stream.ToArray(), contentType, fileName);
        }

        public IActionResult ExportTransactionsReportPdf()
        {
            // Retrieve data for the report — current user's rows only
            var userId = _userManager.GetUserId(User);
            var transactions = _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Date)
                .ToList();

            byte[] pdfBytes = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(style => style.FontSize(10));

                    page.Header()
                        .Text("Transactions Report")
                        .FontSize(18)
                        .SemiBold();

                    page.Content().PaddingVertical(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);   // Date
                            columns.RelativeColumn(3);    // Description
                            columns.ConstantColumn(80);   // Amount
                            columns.ConstantColumn(60);   // Type
                            columns.ConstantColumn(90);   // Category
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Date");
                            header.Cell().Element(HeaderCell).Text("Description");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Amount");
                            header.Cell().Element(HeaderCell).Text("Type");
                            header.Cell().Element(HeaderCell).Text("Category");
                        });

                        foreach (var transaction in transactions)
                        {
                            table.Cell().Element(BodyCell).Text(transaction.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Element(BodyCell).Text(transaction.Description);
                            table.Cell().Element(BodyCell).AlignRight().Text(transaction.Amount.ToString("N2"));
                            table.Cell().Element(BodyCell).Text(transaction.Type.ToString());
                            table.Cell().Element(BodyCell).Text(transaction.Category.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "TransactionsReport.pdf");

            static IContainer HeaderCell(IContainer container) =>
                container.BorderBottom(1).PaddingVertical(4).DefaultTextStyle(style => style.SemiBold());

            static IContainer BodyCell(IContainer container) =>
                container.BorderBottom(0.5f).PaddingVertical(3);
        }
    }
}
