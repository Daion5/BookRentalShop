using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookRentalShop.Data;
using BookRentalShop.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookRentalShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Books
        public async Task<IActionResult> Books()
        {
            var books = await _context.Books.Include(b => b.Category).ToListAsync();
            return View(books);
        }

        // GET: Admin/Books/Create
        public IActionResult CreateBook()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook([Bind("Title,Author,Price,Year,Description,AvailableCopies,CategoryId")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Books));
            }
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Admin/Books/Edit/5
        public async Task<IActionResult> EditBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Admin/Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int id, [Bind("BookId,Title,Author,Price,Year,Description,AvailableCopies,CategoryId")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Books));
            }
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Admin/Books/Delete/5
        public async Task<IActionResult> DeleteBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBookConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Books));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Admin/Categories/Create
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([Bind("Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, [Bind("CategoryId,Name")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            if (_context.Books.Any(b => b.CategoryId == category.CategoryId))
            {
                ViewBag.ErrorMessage = "Nie można usunąć kategorii, ponieważ istnieją książki przypisane do tej kategorii.";
                return View("Error");
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        // GET: Admin/ManageFees
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageFees(string searchUser, DateTime? searchDate)
        {
            var overdueFees = _context.OverdueFees
                .Include(f => f.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchUser))
            {
                overdueFees = overdueFees.Where(f => f.User.UserName.Contains(searchUser));
            }

            if (searchDate.HasValue)
            {
                overdueFees = overdueFees.Where(f => f.CreatedDate.Date == searchDate.Value.Date);
            }

            return View(await overdueFees.ToListAsync());
        }


        // POST: Admin/DeleteFee/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFee(int id)
        {
            var fee = await _context.OverdueFees.FindAsync(id);
            if (fee != null)
            {
                _context.OverdueFees.Remove(fee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageFees));
        }
    }
}
