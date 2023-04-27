using Microsoft.AspNetCore.Mvc;
using FinanceApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FinanceApp.Enums;

namespace FinanceApp.Controllers
{

    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> IncomeVsExpense()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
            .Where(t => t.UserId == currentUser.Id)
            .ToListAsync();

            // Process data
            decimal totalIncome = transactions
                .Where(t => t.Category == TransactionCategory.Income)
                .Sum(t => t.Amount);

            decimal totalExpenses = transactions
                .Where(t => t.Category == TransactionCategory.Expense)
                .Sum(t => t.Amount);

            // Pass data to the view
            var viewModel = new IncomeVsExpenseViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses
            };

            return View(viewModel);
        }
        public async Task<IActionResult> CategoryBreakdown()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }


            // Retrieve transactions data
            var transactions = await _context.IncomeVsExpenses
            .Where(t => t.UserId == currentUser.Id)
            .ToListAsync();

            // Group transactions by category and calculate the sum of each category
            var categoryTotals = transactions
               .Where(t => t.Category == TransactionCategory.Expense)
               .GroupBy(t => t.SubType)
               .Select(g => new CategoryTotal
               {
                   SubType = g.Key,
                   Total = g.Sum(t => t.Amount),
               }).ToList();


            // Prepare the data model for the view
            var model = new CategoryBreakdownViewModel
            {
                CategoryTotals = categoryTotals,
            };

            return View(model);
        }
        public async Task<IActionResult> MonthlyBudget()
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();

            // Group transactions by month and calculate the sum of income and expenses for each month
            var monthlyTotals = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new MonthlyTotal
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Income = g.Where(t => t.Category == TransactionCategory.Income)
                             .Sum(t => t.Amount),
                    Expenses = g.Where(t => t.Category == TransactionCategory.Expense)
                               .Sum(t => t.Amount),
                }).ToList();

            // Prepare the data model for the view
            var model = new MonthlyBudgetViewModel
            {
                MonthlyTotals = monthlyTotals,
            };

            return View(model);
        }
        public async Task<IActionResult> TransactionsReport()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var transactions = await _context.Transactions
                                    .Where(t => t.UserId == userId)
                                    .ToListAsync();
            var viewModel = new TransactionsReportViewModel
            {
                Transactions = transactions,
                PreferredCurrency = user.PreferredCurrency // Assuming you have a PreferredCurrency property in ApplicationUser
            };
            return View(viewModel);
        }

        public async Task<IActionResult> DebtPayoff()
        {


            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();
            // Process the data for the report
            return View(transactions);
        }
        public async Task<IActionResult> SavingsProgress()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();
            // Process the data for the report
            return View(transactions);
        }
        public async Task<IActionResult> NetWorthReport()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();
            // Process the data for the report
            return View(transactions);
        }
        public async Task<IActionResult> Custom()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found.");
            }

            // Retrieve data
            var transactions = await _context.Transactions
                .Where(t => t.UserId == currentUser.Id)
                .ToListAsync();
            // Process the data for the report
            return View(transactions);
        }
    }
}


