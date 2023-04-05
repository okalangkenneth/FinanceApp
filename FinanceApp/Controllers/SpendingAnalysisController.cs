using FinanceApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FinanceApp.Models;
using FinanceApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Exceptions;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace FinanceApp.Controllers
{
    public class SpendingAnalysisController : Controller
    {
        private readonly OpenAIService _openAIService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _config;


        public SpendingAnalysisController(OpenAIService openAIService, ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger, IConfiguration config)
        {
            _openAIService = openAIService;
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _config = config;
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

        public async Task<IActionResult> Analyze()
        {
            // Fetch the transactions data
            List<Transaction> transactions = await GetTransactionsForUser();

            // Prepare the prompt (input data) for the GPT model
            string prompt = CreatePromptFromTransactions(transactions);

            // Get the API key and endpoint from the configuration
            string apiKey = _config.GetValue<string>("OpenAI:ApiKey");
            string endpoint = _config.GetValue<string>("OpenAI:Endpoint");

            try
            {
                // Call the OpenAIService to analyze the spending habits
                string analysisResult = await _openAIService.AnalyzeSpendingHabitsAsync(prompt, apiKey, endpoint);

                // Use the analysisResult to display insights to the user
                ViewBag.AnalysisResult = analysisResult;
                return View("Analyze"); // Return the Analyze view
            }
            catch (CreditExhaustedException ex)
            {
                _logger.LogError(ex, "An error occurred while analyzing spending habits.");
                return RedirectToAction("Error", new { message = "Your subscription has reached its limit or payment is required. Please upgrade your plan or contact support." });
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

        public async Task<List<Transaction>> GetTransactionsForUser()
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


        public async Task<IActionResult> Recommendations()
        {
            // Prepare the prompt (input data) for the GPT model
            string prompt = "Provide some recommendations for better financial management.";

            // Get the API key and endpoint from the configuration
            string apiKey = _config.GetValue<string>("OpenAI:ApiKey");
            string endpoint = _config.GetValue<string>("OpenAI:Endpoint");

            try
            {
                // Call the OpenAIService to generate recommendations
                string recommendations = await _openAIService.GenerateRecommendationsAsync(prompt, apiKey, endpoint);

                // Use the recommendations to display to the user
                ViewBag.Recommendations = recommendations;
                return View("Recommendations"); // Return the Recommendations view
            }
            catch (CreditExhaustedException ex)
            {
                _logger.LogError(ex, "An error occurred while generating recommendations.");
                return RedirectToAction("Error", new { message = "Your subscription has reached its limit or payment is required. Please upgrade your plan or contact support." });
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


