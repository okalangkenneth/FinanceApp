using FinanceApp.Data;
using FinanceApp.Enums;
using FinanceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class IncomeVsExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IncomeVsExpensesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: IncomeVsExpenses
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewBag.AmountSortParm = String.IsNullOrEmpty(sortOrder) ? "amount_desc" : "";
            ViewBag.CategorySortParm = sortOrder == "category" ? "category_desc" : "category";
            ViewBag.SubTypeSortParm = sortOrder == "subtype" ? "subtype_desc" : "subtype";

            var incomeVsExpenses = from i in _context.IncomeVsExpenses.Include(i => i.ApplicationUser) select i;

            switch (sortOrder)
            {
                case "amount_desc":
                    incomeVsExpenses = incomeVsExpenses.OrderByDescending(i => i.Amount);
                    break;
                case "category":
                    incomeVsExpenses = incomeVsExpenses.OrderBy(i => i.Category);
                    break;
                case "category_desc":
                    incomeVsExpenses = incomeVsExpenses.OrderByDescending(i => i.Category);
                    break;
                case "subtype":
                    incomeVsExpenses = incomeVsExpenses.OrderBy(i => i.SubType);
                    break;
                case "subtype_desc":
                    incomeVsExpenses = incomeVsExpenses.OrderByDescending(i => i.SubType);
                    break;
                default:
                    incomeVsExpenses = incomeVsExpenses.OrderBy(i => i.Amount);
                    break;
            }

            return View(await incomeVsExpenses.ToListAsync());
        }


        // GET: IncomeVsExpenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomeVsExpense = await _context.IncomeVsExpenses
                .Include(i => i.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (incomeVsExpense == null)
            {
                return NotFound();
            }

            return View(incomeVsExpense);
        }

        // GET: IncomeVsExpenses/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: IncomeVsExpenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Type,Category,SubType")] IncomeVsExpense incomeVsExpense)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Check if an existing IncomeVsExpense item has the same SubType
                    var existingItem = await _context.IncomeVsExpenses
                        .FirstOrDefaultAsync(i => i.UserId == user.Id && i.SubType == incomeVsExpense.SubType);

                    if (existingItem != null)
                    {
                        // Update the Amount of the existing item
                        existingItem.Amount += incomeVsExpense.Amount;
                        _context.Update(existingItem);
                    }
                    else
                    {
                        // Create a new item if no existing item with the same SubType is found
                        incomeVsExpense.UserId = user.Id;
                        _context.Add(incomeVsExpense);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", incomeVsExpense.UserId);
            return View(incomeVsExpense);
        }

        // GET: IncomeVsExpenses/Edit
        [HttpGet("Edit/{userId}")]
        public async Task<IActionResult> Edit(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var incomeVsExpenses = await _context.IncomeVsExpenses.Where(x => x.UserId == userId).ToListAsync();

            if (incomeVsExpenses == null || incomeVsExpenses.Count == 0)
            {
                return NotFound();
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userId);
            ViewData["UserRecords"] = incomeVsExpenses;

            // Return the view with the first IncomeVsExpense object in the list
            return View(incomeVsExpenses.FirstOrDefault());
        }

        // POST: IncomeVsExpenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Category,SubType")] IncomeVsExpense incomeVsExpense)
        {
            if (id != incomeVsExpense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original IncomeVsExpense object from the database
                    var originalIncomeVsExpense = await _context.IncomeVsExpenses.FindAsync(id);

                    if (originalIncomeVsExpense == null)
                    {
                        return NotFound();
                    }

                    // Update the Amount and SubType properties
                   
                    originalIncomeVsExpense.Amount = incomeVsExpense.Amount;
                    originalIncomeVsExpense.Category = incomeVsExpense.Category;
                    originalIncomeVsExpense.SubType = incomeVsExpense.SubType;

                    _context.Update(originalIncomeVsExpense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeVsExpenseExists(incomeVsExpense.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", incomeVsExpense.UserId);
            return View(incomeVsExpense);
        }

        // GET: IncomeVsExpenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var incomeVsExpense = await _context.IncomeVsExpenses
                .Include(i => i.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (incomeVsExpense == null)
            {
                return NotFound();
            }

            return View(incomeVsExpense);
        }

        // POST: IncomeVsExpenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var incomeVsExpense = await _context.IncomeVsExpenses.FindAsync(id);
            _context.IncomeVsExpenses.Remove(incomeVsExpense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncomeVsExpenseExists(int id)
        {
            return _context.IncomeVsExpenses.Any(e => e.Id == id);
        }
        public decimal GetTotalAmountForCategory(TransactionCategory category)
        {
            return _context.IncomeVsExpenses.Where(x => x.Category == category).Sum(x => x.Amount);
        }

        public decimal GetTotalAmountForSubType(TransactionSubType subType)
        {
            return _context.IncomeVsExpenses.Where(x => x.SubType == subType).Sum(x => x.Amount);
        }
        [HttpGet]
        public async Task<IActionResult> GetRecordById(int id)
        {
            var record = await _context.IncomeVsExpenses.FindAsync(id);

            if (record == null)
            {
                return NotFound();
            }

            var subTypeString = Enum.GetName(typeof(TransactionSubType), record.SubType);
            return Json(new { id = record.Id, amount = record.Amount, subType = subTypeString });
        }




    }
}
