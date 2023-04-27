using FinanceApp.Data;
using FinanceApp.Enums;
using FinanceApp.Hubs;
using FinanceApp.Models;
using FinanceApp.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace FinanceApp.Controllers
{
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<FinanceAppHub> _hubContext;



        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<FinanceAppHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;

        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Load the dashboard view model for the given user
            var dashboardViewModel = await LoadDashboardViewModelAsync(userId);

            return View(dashboardViewModel);
        }

        // Helper method to calculate net worth based on transactions and financial goals
        private decimal CalculateNetWorth(List<Transaction> transactions, List<FinancialGoal> financialGoals)
        {
            decimal totalAssets = 0;
            decimal totalLiabilities = 0;

            foreach (var transaction in transactions)
            {
                if (transaction.Category == TransactionCategory.Income)
                {
                    totalAssets += transaction.Amount;
                }
                else if (transaction.Category == TransactionCategory.Expense)
                {
                    totalLiabilities += transaction.Amount;
                }
            }

            foreach (var financialGoal in financialGoals)
            {
                if (financialGoal.Status == GoalStatus.Completed)
                {
                    totalAssets += financialGoal.CurrentAmount;
                }
            }

            return totalAssets - totalLiabilities;
        }
        private async Task<DashboardViewModel> LoadDashboardViewModelAsync(string userId)
        {
            // Fetch transactions, financial goals, and monthly budgets for the current user
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= DateTime.Now.AddMonths(-12))
                .ToListAsync();

            var financialGoals = await _context.FinancialGoals
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var monthlyBudgets = await _context.MonthlyBudgets
                 .Where(m => m.UserId == userId)
                 .ToListAsync();

            // Fetch IncomeVsExpense data for the current user
            var incomeVsExpenses = await _context.IncomeVsExpenses
                .Where(ive => ive.UserId == userId)
                .ToListAsync();

            // Calculate the category totals for expenses
            var categoryTotals = transactions
                .Where(t => t.Category == TransactionCategory.Expense)
                .GroupBy(t => t.SubType)
                .Select(g => new CategoryTotal
                {
                    SubType = g.Key,
                    Total = g.Sum(t => t.Amount),
                }).ToList();

            // Fetch NetWorth data for the current user
            var netWorthData = await _context.NetWorths
                .Where(nw => nw.UserId == userId)
                .OrderBy(nw => nw.Date)
                .ToListAsync();

            // Initialize DashboardViewModel
            var dashboardViewModel = new DashboardViewModel
            {
                Transactions = transactions,
                FinancialGoals = financialGoals,
                IncomeVsExpenses = incomeVsExpenses,
                MonthlyBudgets = monthlyBudgets,
                NetWorthData = netWorthData,
                CategoryTotals = categoryTotals
            };

            // Set up IncomeVsExpense-related properties
            dashboardViewModel.IncomeVsExpenseLabels = Enum.GetNames(typeof(TransactionSubType)).ToList();
            dashboardViewModel.IncomeVsExpenseAmounts = new Dictionary<string, List<decimal>> {
                { "Income", new List<decimal>(new decimal[dashboardViewModel.IncomeVsExpenseLabels.Count]) },
                { "Expense", new List<decimal>(new decimal[dashboardViewModel.IncomeVsExpenseLabels.Count]) }
    };

            foreach (var item in incomeVsExpenses)
            {
                int index = (int)item.SubType;
                if (item.Category == TransactionCategory.Income)
                {
                    dashboardViewModel.IncomeVsExpenseAmounts["Income"][index] += item.Amount;
                }
                else
                {
                    dashboardViewModel.IncomeVsExpenseAmounts["Expense"][index] += item.Amount;
                }
            }

            // Initialize MonthlyBudget-related properties
            dashboardViewModel.MonthlyBudgetLabels = Enum.GetNames(typeof(TransactionSubType)).ToList();
            dashboardViewModel.MonthlyBudgetAmounts = new List<decimal>();

            foreach (var category in Enum.GetValues(typeof(TransactionSubType)).Cast<TransactionSubType>())
            {
                var budgetAmount = monthlyBudgets
                    .Where(m => m.Category == category)
                    .Sum(m => m.Amount);

                dashboardViewModel.MonthlyBudgetAmounts.Add(budgetAmount);
            }

            // Initialize MonthlyBudgetViewModel
            var monthlyBudgetViewModel = new MonthlyBudgetViewModel();
            monthlyBudgetViewModel.MonthlyTotals = new List<MonthlyTotal>();

            DateTime startDate;
            if (transactions.Any())
            {
                startDate = transactions.Min(t => t.Date);
            }
            else
            {
                startDate = DateTime.Now;

            }

            var endDate = DateTime.Now;

            // Calculate monthly income and expenses
            for (var date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                var monthlyIncome = transactions
                    .Where(t => t.Category == TransactionCategory.Income && t.Date.Year == date.Year && t.Date.Month == date.Month)
                    .Sum(t => t.Amount);

                var monthlyExpenses = transactions
                    .Where(t => t.Category == TransactionCategory.Expense && t.Date.Year == date.Year && t.Date.Month == date.Month)
                    .Sum(t => t.Amount);

                monthlyBudgetViewModel.MonthlyTotals.Add(new MonthlyTotal
                {
                    Year = date.Year,
                    Month = date.Month,
                    Income = monthlyIncome,
                    Expenses = monthlyExpenses
                });
            }

            dashboardViewModel.MonthlyBudgetOverview = monthlyBudgetViewModel;

            return dashboardViewModel;

        }

        [HttpGet]
        public async Task<IActionResult> GetIncomeVsExpenseData()
        {
            var userId = _userManager.GetUserId(User);
            var dashboardViewModel = await LoadDashboardViewModelAsync(userId);

            var result = new
            {
                labels = dashboardViewModel.IncomeVsExpenseLabels,
                amounts = new
                {
                    Income = dashboardViewModel.IncomeVsExpenseAmounts["Income"],
                    Expense = dashboardViewModel.IncomeVsExpenseAmounts["Expense"]
                }
            };
            // Trigger the SendIncomeVsExpenseData method
            
           // await _hubContext.Clients.User(userId).SendAsync("ReceiveIncomeVsExpenseData");


            return Json(result);
        }


        private async Task<(List<string> labels, Dictionary<string, List<decimal>> amounts)> LoadIncomeVsExpenseDataAsync(string userId)
        {
            // Fetch IncomeVsExpense data for the current user
            var incomeVsExpenses = await _context.IncomeVsExpenses
                .Where(ive => ive.UserId == userId)
                .ToListAsync();

            var incomeVsExpenseLabels = Enum.GetNames(typeof(TransactionSubType)).ToList();
            var incomeVsExpenseAmounts = new Dictionary<string, List<decimal>>
            {
                { "Income", new List<decimal>(new decimal[incomeVsExpenseLabels.Count]) },
                { "Expense", new List<decimal>(new decimal[incomeVsExpenseLabels.Count]) }
            };

            foreach (var item in incomeVsExpenses)
            {
                int index = (int)item.SubType;
                if (item.Category == TransactionCategory.Income)
                {
                    incomeVsExpenseAmounts["Income"][index] += item.Amount;
                }
                else
                {
                    incomeVsExpenseAmounts["Expense"][index] += item.Amount;
                }
            }

            return (incomeVsExpenseLabels, incomeVsExpenseAmounts);
        }



    }
}





