using DinkToPdf;
using FinanceApp.Data;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public ExportController(ApplicationDbContext context, ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
        }

        public IActionResult ExportTransactionsReportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Retrieve data for the report
            var transactions = _context.Transactions.ToList();
            var viewModel = new TransactionsReportViewModel
            {
                Transactions = transactions
            };

            // Create an Excel package
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TransactionsReport");

            // Add header row
            worksheet.Cells[1, 1].Value = "Date";
            worksheet.Cells[1, 2].Value = "Description";
            worksheet.Cells[1, 3].Value = "Amount";
            worksheet.Cells[1, 4].Value = "Type";
            worksheet.Cells[1, 5].Value = "Category";

            // Add data rows
            int row = 2;
            foreach (var transaction in viewModel.Transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.Date;
                worksheet.Cells[row, 2].Value = transaction.Description;
                worksheet.Cells[row, 3].Value = transaction.Amount;
                worksheet.Cells[row, 4].Value = transaction.Type.ToString();
                worksheet.Cells[row, 5].Value = transaction.Category.ToString();
                row++;
            }

            // Set the content type and file name
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "TransactionsReport.xlsx";

            // Return the Excel file as an attachment
            return File(package.GetAsByteArray(), contentType, fileName);
        }


        public async Task<IActionResult> ExportTransactionsReportPdfAsync()
        {
            // Retrieve data for the report
            var transactions = _context.Transactions.ToList();
            var viewModel = new TransactionsReportViewModel
            {
                Transactions = transactions
            };

            // Render the view as an HTML string
            string viewHtml = await RenderViewAsString("~/Views/Reports/TransactionsReport.cshtml", viewModel);



            // Convert the HTML string to a PDF
            var pdfConverter = new SynchronizedConverter(new PdfTools());
            var pdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4
        },
                Objects = { new ObjectSettings { HtmlContent = viewHtml } }
            };
            byte[] pdfBytes = pdfConverter.Convert(pdfDocument);

            // Return the PDF as an attachment
            string fileName = "TransactionsReport.pdf";
            string contentType = "application/pdf";
            return File(pdfBytes, contentType, fileName);
        }
        private async Task<string> RenderViewAsString(string viewName, object model)
        {
            ViewData.Model = model;
            using var writer = new StringWriter();
            var viewResult = _viewEngine.GetView(null, viewName, false);
            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Could not find view: {viewName}");
            }
            var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, writer, new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }




    }
}
