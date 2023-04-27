using FinanceApp.Data;
using FinanceApp.Enums;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager,
    ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> LogOutAndRedirect()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Get transactions and financial goals for the current user
            var transactions = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            var financialGoals = await _context.FinancialGoals
                .Where(g => g.UserId == user.Id)
                .ToListAsync();

            // Calculate category spending
            var categorySpending = Enum.GetValues(typeof(TransactionCategory))
                .Cast<TransactionCategory>()
                .Select(category => new CategorySpending
                {
                    Category = category,
                    Amount = transactions
                        .Where(t => t.Category == TransactionCategory.Expense && t.Category == category)
                        .Sum(t => t.Amount)
                })
                .ToList();

            // Calculate total income, expenses, and balance
            decimal income = transactions.Where(t => t.Category == TransactionCategory.Income).Sum(t => t.Amount);
            decimal expenses = transactions.Where(t => t.Category == TransactionCategory.Expense).Sum(t => t.Amount);
            decimal balance = income - expenses;

            ViewBag.Income = income;
            ViewBag.Expenses = expenses;
            ViewBag.Balance = balance;

            var model = new DashboardViewModel
            {
                Transactions = transactions,
                FinancialGoals = financialGoals,
                CategorySpending = categorySpending
            };

            return View(model);


        }
    }
}
