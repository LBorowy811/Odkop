using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Odkop.Data;
using Odkop.Models;

namespace Odkop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // Panel główny
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.ForumCount = await _context.Forums.CountAsync();
            ViewBag.TopicCount = await _context.Topics.CountAsync();
            ViewBag.PostCount = await _context.Posts.CountAsync();
            ViewBag.AnnouncementCount = await _context.Announcements.CountAsync();
            ViewBag.BannedWordCount = await _context.BannedWords.CountAsync();

            return View();
        }

        // ZARZĄDZANIE KATEGORIAMI
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var categories = await _context.Categories
                .Include(c => c.Forums)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kategoria została utworzona.";
                return RedirectToAction("Categories");
            }

            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            if (ModelState.IsValid)
            {
                category.Name = model.Name;
                category.Description = model.Description;
                category.DisplayOrder = model.DisplayOrder;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Kategoria została zaktualizowana.";
                return RedirectToAction("Categories");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kategoria została usunięta.";
            }

            return RedirectToAction("Categories");
        }

        // ZARZĄDZANIE FORAMI
        public async Task<IActionResult> Forums()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var forums = await _context.Forums
                .Include(f => f.Category)
                .OrderBy(f => f.Category!.DisplayOrder)
                .ThenBy(f => f.DisplayOrder)
                .ToListAsync();

            return View(forums);
        }

        public async Task<IActionResult> CreateForum()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForum(Forum forum)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Forums.Add(forum);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Forum zostało utworzone.";
                return RedirectToAction("Forums");
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(forum);
        }

        public async Task<IActionResult> EditForum(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var forum = await _context.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(forum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForum(int id, Forum model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var forum = await _context.Forums.FindAsync(id);
            if (forum == null) return NotFound();

            if (ModelState.IsValid)
            {
                forum.Name = model.Name;
                forum.Description = model.Description;
                forum.CategoryId = model.CategoryId;
                forum.DisplayOrder = model.DisplayOrder;
                forum.AllowAnonymousView = model.AllowAnonymousView;
                forum.AllowAnonymousPost = model.AllowAnonymousPost;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Forum zostało zaktualizowane.";
                return RedirectToAction("Forums");
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForum(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var forum = await _context.Forums.FindAsync(id);
            if (forum != null)
            {
                _context.Forums.Remove(forum);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Forum zostało usunięte.";
            }

            return RedirectToAction("Forums");
        }

        // ZARZĄDZANIE UŻYTKOWNIKAMI
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var users = await _context.Users
                .OrderByDescending(u => u.RegisteredAt)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserRole role)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rola użytkownika została zmieniona.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Nie pozwól usunąć samego siebie
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (user.Id == currentUserId)
                {
                    TempData["Error"] = "Nie możesz usunąć własnego konta.";
                    return RedirectToAction("Users");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Użytkownik został usunięty.";
            }

            return RedirectToAction("Users");
        }

        // Przypinanie/Zamykanie wątków
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePinTopic(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                topic.IsPinned = !topic.IsPinned;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Topic", "Forum", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockTopic(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                topic.IsLocked = !topic.IsLocked;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Topic", "Forum", new { id });
        }

        // ZARZĄDZANIE OGŁOSZENIAMI
        public async Task<IActionResult> Announcements()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        public IActionResult CreateAnnouncement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(Announcement announcement)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                announcement.AuthorId = HttpContext.Session.GetInt32("UserId");
                announcement.AuthorName = HttpContext.Session.GetString("User") ?? "Admin";
                announcement.CreatedAt = DateTime.Now;

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ogłoszenie zostało utworzone.";
                return RedirectToAction("Announcements");
            }

            return View(announcement);
        }

        public async Task<IActionResult> EditAnnouncement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnnouncement(int id, Announcement model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            if (ModelState.IsValid)
            {
                announcement.Title = model.Title;
                announcement.Content = model.Content;
                announcement.ExpiresAt = model.ExpiresAt;
                announcement.IsActive = model.IsActive;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Ogłoszenie zostało zaktualizowane.";
                return RedirectToAction("Announcements");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ogłoszenie zostało usunięte.";
            }

            return RedirectToAction("Announcements");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAnnouncement(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.IsActive = !announcement.IsActive;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Announcements");
        }

        // ZARZĄDZANIE SŁOWAMI ZAKAZANYMI
        public async Task<IActionResult> BannedWords()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var words = await _context.BannedWords
                .OrderBy(w => w.Word)
                .ToListAsync();

            return View(words);
        }

        public IActionResult CreateBannedWord()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBannedWord(string word, string? reason)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["Error"] = "Słowo jest wymagane.";
                return View(new BannedWord());
            }

            // Sprawdź czy słowo już istnieje
            var exists = await _context.BannedWords
                .AnyAsync(w => w.Word.ToLower() == word.ToLower());

            if (exists)
            {
                TempData["Error"] = "To słowo już istnieje w słowniku.";
                return View(new BannedWord { Word = word, Reason = reason });
            }

            var bannedWord = new BannedWord
            {
                Word = word,
                Reason = reason,
                CreatedAt = DateTime.Now
            };

            _context.BannedWords.Add(bannedWord);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Słowo zostało dodane do słownika.";
            return RedirectToAction("BannedWords");
        }

        public async Task<IActionResult> EditBannedWord(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var word = await _context.BannedWords.FindAsync(id);
            if (word == null) return NotFound();

            return View(word);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBannedWord(int id, BannedWord model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var word = await _context.BannedWords.FindAsync(id);
            if (word == null) return NotFound();

            if (ModelState.IsValid)
            {
                word.Word = model.Word;
                word.Reason = model.Reason;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Słowo zostało zaktualizowane.";
                return RedirectToAction("BannedWords");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBannedWord(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var word = await _context.BannedWords.FindAsync(id);
            if (word != null)
            {
                _context.BannedWords.Remove(word);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Słowo zostało usunięte ze słownika.";
            }

            return RedirectToAction("BannedWords");
        }
    }
}
