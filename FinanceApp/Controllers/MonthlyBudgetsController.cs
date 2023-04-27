using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using FinanceApp.Enums;

namespace FinanceApp.Controllers
{
    public class MonthlyBudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MonthlyBudgetsController> _logger;



        public MonthlyBudgetsController(UserManager<ApplicationUser> userManager,ApplicationDbContext context, ILogger<MonthlyBudgetsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: MonthlyBudgets
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewBag.AmountSortParm = String.IsNullOrEmpty(sortOrder) ? "amount_desc" : "";
            ViewBag.CategorySortParm = sortOrder == "category" ? "category_desc" : "category";

            var monthlyBudgets = from m in _context.MonthlyBudgets.Include(m => m.ApplicationUser) select m;

            switch (sortOrder)
            {
                case "amount_desc":
                    monthlyBudgets = monthlyBudgets.OrderByDescending(m => m.Amount);
                    break;
                case "category":
                    monthlyBudgets = monthlyBudgets.OrderBy(m => m.Category);
                    break;
                case "category_desc":
                    monthlyBudgets = monthlyBudgets.OrderByDescending(m => m.Category);
                    break;
                default:
                    monthlyBudgets = monthlyBudgets.OrderBy(m => m.Amount);
                    break;
            }

            return View(await monthlyBudgets.ToListAsync());
        }






        // GET: MonthlyBudgets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBudget = await _context.MonthlyBudgets
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monthlyBudget == null)
            {
                return NotFound();
            }

            return View(monthlyBudget);
        }

       
        // GET: MonthlyBudgets/Create
        public IActionResult Create()
        {
            // You can replace the list of categories below with the actual categories you want to use.
            var categories = new List<string> { "Rent", "Groceries", "Utilities", "Entertainment", "Transportation", "Savings","Other" };

            ViewData["Categories"] = new SelectList(categories);
            return View();
        }
        // POST: MonthlyBudgets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: MonthlyBudgets/Create
        // POST: MonthlyBudgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Category,Amount")] MonthlyBudget monthlyBudget)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Check if there's an existing record with the same category for the user
                var existingRecord = await _context.MonthlyBudgets
                    .FirstOrDefaultAsync(m => m.UserId == user.Id && m.Category == monthlyBudget.Category);

                if (existingRecord != null)
                {
                    // Update the existing record
                    existingRecord.Amount += monthlyBudget.Amount;
                    _context.Update(existingRecord);
                }
                else
                {
                    // Create a new record
                    monthlyBudget.UserId = user.Id;
                    _context.Add(monthlyBudget);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", monthlyBudget.UserId);
            return View(monthlyBudget);
        }

        // GET: MonthlyBudgets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBudget = await _context.MonthlyBudgets.FindAsync(id);
            if (monthlyBudget == null)
            {
                return NotFound();
            }

            if (monthlyBudget.Category == 0) // Check if Category is not set
            {
                monthlyBudget.Category = (TransactionSubType)(-1); // Set Category to -1 (Assuming -1 corresponds to 'Choose a category')
            }

            if (monthlyBudget.Amount == 0) // Check if Amount is not set
            {
                monthlyBudget.Amount = 0.00m; // Set Amount to 0.00
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", monthlyBudget.UserId);
            return View(monthlyBudget);
        }



        // POST: MonthlyBudgets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Category,Amount")] MonthlyBudget monthlyBudget)
        {
            if (id != monthlyBudget.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monthlyBudget);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonthlyBudgetExists(monthlyBudget.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", monthlyBudget.UserId);
            return View(monthlyBudget);
        }

        // GET: MonthlyBudgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBudget = await _context.MonthlyBudgets
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monthlyBudget == null)
            {
                return NotFound();
            }

            return View(monthlyBudget);
        }

        // POST: MonthlyBudgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monthlyBudget = await _context.MonthlyBudgets.FindAsync(id);
            _context.MonthlyBudgets.Remove(monthlyBudget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonthlyBudgetExists(int id)
        {
            return _context.MonthlyBudgets.Any(e => e.Id == id);
        }


        // Add this method to the MonthlyBudgetsController
        [HttpGet]
        public async Task<IActionResult> GetRecordById(int id)
        {
            var record = await _context.MonthlyBudgets.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            return Json(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAll(IFormCollection form)
        {
            var userRecords = new List<MonthlyBudget>();

            for (int i = 0; i < form.Keys.Count(k => k.Contains("Id")); i++)
            {
                var budget = new MonthlyBudget
                {
                    Id = int.Parse(form[$"userRecords[{i}].Id"]),
                    UserId = form[$"userRecords[{i}].UserId"],
                    Category = (TransactionSubType)Enum.Parse(typeof(TransactionSubType), form[$"userRecords[{i}].Category"]),
                    Amount = decimal.Parse(form[$"userRecords[{i}].Amount"])
                };
                userRecords.Add(budget);
            }

            if (ModelState.IsValid)
            {
                foreach (var budget in userRecords)
                {
                    _context.Update(budget);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If the ModelState is not valid, you might want to repopulate the ViewData and return the EditAll view.
            // You may need to adjust this according to your specific needs.
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userRecords.FirstOrDefault()?.UserId);
            ViewData["UserRecords"] = userRecords;
            return View("EditAll", userRecords.FirstOrDefault());
        }
    }
}
