using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookRentalShop.Data;
using BookRentalShop.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace BookRentalShop.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString, string category, decimal? minPrice, decimal? maxPrice, bool showAvailable = false)
        {
            var books = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) || b.Author.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(category))
            {
                books = books.Where(b => b.Category.Name == category);
            }

            if (minPrice.HasValue)
            {
                books = books.Where(b => b.Price >= minPrice);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.Price <= maxPrice);
            }

            if (showAvailable)
            {
                books = books.Where(b => b.AvailableCopies > 0);
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "Name", "Name");
            ViewData["SearchString"] = searchString;
            ViewData["SelectedCategory"] = category;
            ViewData["MinPrice"] = minPrice ?? 0;

            var booksList = await books.ToListAsync();
            if (booksList.Count == 0)
            {
                ViewData["NoBooksMessage"] = "No books with given criteria.";
            }
            else
            {
                ViewData["MaxPrice"] = maxPrice ?? booksList.Max(b => b.Price);
            }

            ViewData["ShowAvailable"] = showAvailable;

            return View(booksList);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
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

        [Authorize]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Borrow(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null || book.AvailableCopies < 1)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var borrowedBooks = await _context.BorrowedBooks
                .Where(b => b.UserId == userId && b.BookId == id)
                .FirstOrDefaultAsync();

            if (borrowedBooks != null && borrowedBooks.Quantity >= 1)
            {
                TempData["borrowErrorMessage"] = "You have already borrowed this book.";
                return RedirectToAction(nameof(Index));
            }

            if (borrowedBooks == null)
            {
                borrowedBooks = new BorrowedBooks
                {
                    BookId = book.BookId,
                    UserId = userId,
                    BorrowDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(14),
                    Quantity = 1
                };
                _context.Add(borrowedBooks);
            }
            else
            {
                borrowedBooks.Quantity++;
                _context.Update(borrowedBooks);
            }

            book.AvailableCopies--;
            _context.Update(book);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Book successfully borrowed!";
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowedBook = await _context.BorrowedBooks
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.BorrowedBookId == id);
            if (borrowedBook == null)
            {
                return NotFound();
            }

            borrowedBook.CalculateOverdueFee();
            if (borrowedBook.OverdueFee > 0)
            {
                var overdueFee = new OverdueFee
                {
                    UserId = borrowedBook.UserId,
                    Amount = borrowedBook.OverdueFee,
                    CreatedDate = DateTime.Now
                };
                _context.OverdueFees.Add(overdueFee);
            }

            borrowedBook.Quantity--;
            if (borrowedBook.Quantity == 0)
            {
                _context.BorrowedBooks.Remove(borrowedBook);
            }
            else
            {
                _context.Update(borrowedBook);
            }

            borrowedBook.Book.AvailableCopies++;
            _context.Update(borrowedBook.Book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Cart()
        {
            var userId = _userManager.GetUserId(User);

            var borrowedBooks = await _context.BorrowedBooks
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            decimal currentOverdueFees = 0;
            foreach (var borrowedBook in borrowedBooks)
            {
                borrowedBook.CalculateOverdueFee();
                currentOverdueFees += borrowedBook.OverdueFee;
            }

            var pastOverdueFees = await _context.OverdueFees
                .Where(f => f.UserId == userId)
                .SumAsync(f => f.Amount);

            var totalOverdueFees = currentOverdueFees + pastOverdueFees;

            ViewData["TotalOverdueFees"] = totalOverdueFees;

            return View(borrowedBooks);
        }

        // GET: Books/CreateBook
        [Authorize(Roles = "Admin")]
        public IActionResult CreateBook()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Books/CreateBook
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook([Bind("BookId,Title,Author,Description,Price,Year,AvailableCopies,CategoryId")] Book book)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/EditBook/5
        [Authorize(Roles = "Admin")]
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

        // POST: Books/EditBook/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int id, [Bind("BookId,Title,Author,Description,Price,Year,AvailableCopies,CategoryId")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }
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
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/DeleteBook/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/DeleteBook/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBookConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}