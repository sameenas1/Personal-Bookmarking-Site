using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookmarkSite.Data;
using BookmarkSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BookmarkSite.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookmarksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookmarks
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 2)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Bookmarks.Where(b => b.UserId == userId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(b => b.Title.Contains(search) || b.Url.Contains(search));

            var totalItems = await query.CountAsync();
            var bookmarks = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;

            return View(bookmarks);
        }

        // GET: Bookmarks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bookmarks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Url")] Bookmark bookmark)
        {
            var userId = _userManager.GetUserId(User);
            var count = await _context.Bookmarks.CountAsync(b => b.UserId == userId);

            if (count >= 15) // Increased limit for testing pagination
            {
                ModelState.AddModelError("", "You can only add up to 15 bookmarks.");
                return View(bookmark);
            }

            if (ModelState.IsValid)
            {
                bookmark.UserId = userId;
                bookmark.CreatedAt = System.DateTime.Now;
                _context.Add(bookmark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookmark);
        }

        // GET: Bookmarks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookmark = await _context.Bookmarks.FindAsync(id);
            if (bookmark == null || bookmark.UserId != _userManager.GetUserId(User))
                return NotFound();

            return View(bookmark);
        }

        // POST: Bookmarks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Url")] Bookmark bookmark)
        {
            if (id != bookmark.Id) return NotFound();

            var existing = await _context.Bookmarks.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (existing == null || existing.UserId != _userManager.GetUserId(User))
                return NotFound();

            bookmark.UserId = existing.UserId;
            bookmark.CreatedAt = existing.CreatedAt;

            if (ModelState.IsValid)
            {
                _context.Update(bookmark);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bookmark);
        }

        // GET: Bookmarks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == _userManager.GetUserId(User));
            if (bookmark == null) return NotFound();

            return View(bookmark);
        }

        // POST: Bookmarks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookmark = await _context.Bookmarks.FindAsync(id);
            if (bookmark != null && bookmark.UserId == _userManager.GetUserId(User))
            {
                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
