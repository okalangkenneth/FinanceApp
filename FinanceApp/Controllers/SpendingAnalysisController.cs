using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.Services.SpendingAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    [Authorize]
    public class SpendingAnalysisController : Controller
    {
        private readonly ISpendingAnalysisService _spendingAnalysisService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<SpendingAnalysisController> _logger;

        public SpendingAnalysisController(
            ISpendingAnalysisService spendingAnalysisService,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<SpendingAnalysisController> logger)
        {
            _spendingAnalysisService = spendingAnalysisService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        private string CreatePromptFromTransactions(List<Transaction> transactions)
        {
            StringBuilder promptBuilder = new StringBuilder("Analyze the spending habits of a user with the following transactions:\n");

            foreach (var transaction in transactions)
            {
                promptBuilder.AppendLine($"{transaction.Date.ToShortDateString()} - {transaction.Category} - {transaction.Amount} {transaction.Currency}");
            }

            promptBuilder.Append("\nInsights:");
            return promptBuilder.ToString();
        }

        [EnableRateLimiting("ai-analysis")]
        public async Task<IActionResult> Analyze()
        {
            List<Transaction> transactions = await GetTransactionsForUser();

            string prompt = CreatePromptFromTransactions(transactions);

            try
            {
                string analysisResult = await _spendingAnalysisService.AnalyzeSpendingHabitsAsync(prompt, HttpContext.RequestAborted);

                ViewBag.AnalysisResult = analysisResult;
                return View("Analyze");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing spending habits.");
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing spending habits.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Error");
            }
        }

        // Private: this is a query helper, not an endpoint — as a public
        // method it was routable and served the user's raw transaction list.
        private async Task<List<Transaction>> GetTransactionsForUser()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Transactions
                                 .Where(t => t.UserId == userId)
                                 .ToListAsync();
        }

        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message ?? "An error occurred while processing your request. Please try again later.";
            return View();
        }

        [EnableRateLimiting("ai-analysis")]
        public async Task<IActionResult> Recommendations()
        {
            string prompt = "Provide some recommendations for better financial management.";

            try
            {
                string recommendations = await _spendingAnalysisService.GenerateRecommendationsAsync(prompt, HttpContext.RequestAborted);

                ViewBag.Recommendations = recommendations;
                return View("Recommendations");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while generating recommendations.");
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating recommendations.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction("Error");
            }
        }
    }
}
