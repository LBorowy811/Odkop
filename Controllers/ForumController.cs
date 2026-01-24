using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Odkop.Data;
using Odkop.Models;

namespace Odkop.Controllers
{
    public class ForumStatsViewModel
    {
        public int TopicCount { get; set; }
        public int PostCount { get; set; }
    }

    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumController> _logger;

        public ForumController(ApplicationDbContext context, ILogger<ForumController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Strona główna forum - lista kategorii i forów
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Forums)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Pobierz statystyki dla każdego forum
            var forumStats = await _context.Forums
                .Select(f => new
                {
                    ForumId = f.Id,
                    TopicCount = f.Topics.Count,
                    PostCount = f.Topics.SelectMany(t => t.Posts).Count()
                })
                .ToDictionaryAsync(
                    x => x.ForumId,
                    x => new ForumStatsViewModel { TopicCount = x.TopicCount, PostCount = x.PostCount }
                );

            ViewBag.ForumStats = forumStats;
            ViewBag.User = HttpContext.Session.GetString("User");
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            return View(categories);
        }

        // Lista wątków w danym forum
        public async Task<IActionResult> Forum(int id, string? search)
        {
            var forum = await _context.Forums
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (forum == null) return NotFound();

            var user = HttpContext.Session.GetString("User");
            var isLoggedIn = !string.IsNullOrEmpty(user);

            // Sprawdź uprawnienia do oglądania
            if (!forum.AllowAnonymousView && !isLoggedIn)
            {
                TempData["Error"] = "Musisz być zalogowany, aby zobaczyć to forum.";
                return RedirectToAction("Login", "Account");
            }

            var topics = _context.Topics
                .Where(t => t.ForumId == id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                topics = topics.Where(t => t.Title.Contains(search));
            }

            var topicsList = await topics
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.LastPostAt ?? t.Created)
                .ToListAsync();

            ViewBag.Forum = forum;
            ViewBag.User = user;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.Search = search;
            ViewBag.CanPost = isLoggedIn || forum.AllowAnonymousPost;

            return View(topicsList);
        }

        // Wyświetlanie wątku z postami
        public async Task<IActionResult> Topic(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.Forum)
                .ThenInclude(f => f!.Category)
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null) return NotFound();

            var user = HttpContext.Session.GetString("User");
            var isLoggedIn = !string.IsNullOrEmpty(user);

            // Sprawdź uprawnienia do oglądania
            if (topic.Forum != null && !topic.Forum.AllowAnonymousView && !isLoggedIn)
            {
                TempData["Error"] = "Musisz być zalogowany, aby zobaczyć ten wątek.";
                return RedirectToAction("Login", "Account");
            }

            // Zwiększ licznik wyświetleń
            topic.ViewCount++;
            await _context.SaveChangesAsync();

            var posts = await _context.Posts
                .Where(p => p.TopicId == id)
                .OrderBy(p => p.Created)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            ViewBag.CanPost = isLoggedIn || (topic.Forum?.AllowAnonymousPost ?? false);
            ViewBag.IsLocked = topic.IsLocked;

            return View((topic, posts));
        }

        // Tworzenie nowego wątku - GET
        public async Task<IActionResult> CreateTopic(int forumId)
        {
            var forum = await _context.Forums.FindAsync(forumId);
            if (forum == null) return NotFound();

            var user = HttpContext.Session.GetString("User");
            var isLoggedIn = !string.IsNullOrEmpty(user);

            if (!isLoggedIn && !forum.AllowAnonymousPost)
            {
                TempData["Error"] = "Musisz być zalogowany, aby utworzyć wątek.";
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Forum = forum;
            ViewBag.User = user;
            return View();
        }

        // Tworzenie nowego wątku - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTopic(int forumId, string title, string content)
        {
            var forum = await _context.Forums.FindAsync(forumId);
            if (forum == null) return NotFound();

            var username = HttpContext.Session.GetString("User");
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = !string.IsNullOrEmpty(username);

            if (!isLoggedIn && !forum.AllowAnonymousPost)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Tytuł i treść są wymagane.";
                ViewBag.Forum = forum;
                ViewBag.User = username;
                return View();
            }

            var topic = new Topic
            {
                Title = title,
                ForumId = forumId,
                AuthorId = userId,
                AuthorName = username ?? "Anonim",
                Created = DateTime.Now,
                LastPostAt = DateTime.Now,
                PostCount = 1
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            // Dodaj pierwszy post
            var post = new Post
            {
                TopicId = topic.Id,
                AuthorId = userId,
                AuthorName = username ?? "Anonim",
                Content = content,
                Created = DateTime.Now
            };

            _context.Posts.Add(post);

            // Zaktualizuj licznik postów użytkownika
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.PostCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Utworzono wątek: {Title} przez {Author}", title, topic.AuthorName);
            return RedirectToAction("Topic", new { id = topic.Id });
        }

        // Dodawanie odpowiedzi do wątku - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(int topicId, string content)
        {
            var topic = await _context.Topics
                .Include(t => t.Forum)
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null) return NotFound();

            if (topic.IsLocked)
            {
                TempData["Error"] = "Ten wątek jest zamknięty.";
                return RedirectToAction("Topic", new { id = topicId });
            }

            var username = HttpContext.Session.GetString("User");
            var userId = HttpContext.Session.GetInt32("UserId");
            var isLoggedIn = !string.IsNullOrEmpty(username);

            if (!isLoggedIn && !(topic.Forum?.AllowAnonymousPost ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Treść jest wymagana.";
                return RedirectToAction("Topic", new { id = topicId });
            }

            var post = new Post
            {
                TopicId = topicId,
                AuthorId = userId,
                AuthorName = username ?? "Anonim",
                Content = content,
                Created = DateTime.Now
            };

            _context.Posts.Add(post);

            // Zaktualizuj statystyki wątku
            topic.PostCount++;
            topic.LastPostAt = DateTime.Now;

            // Zaktualizuj licznik postów użytkownika
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    user.PostCount++;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Topic", new { id = topicId });
        }

        // Edycja posta - GET
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Topic)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var username = HttpContext.Session.GetString("User");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Sprawdź czy użytkownik może edytować
            bool canEdit = post.AuthorName == username || userRole == "Admin" || userRole == "Moderator";
            if (!canEdit)
            {
                TempData["Error"] = "Nie masz uprawnień do edycji tego posta.";
                return RedirectToAction("Topic", new { id = post.TopicId });
            }

            return View(post);
        }

        // Edycja posta - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, string content)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var username = HttpContext.Session.GetString("User");
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            bool canEdit = post.AuthorName == username || userRole == "Admin" || userRole == "Moderator";
            if (!canEdit)
            {
                TempData["Error"] = "Nie masz uprawnień do edycji tego posta.";
                return RedirectToAction("Topic", new { id = post.TopicId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Treść jest wymagana.";
                return View(post);
            }

            post.Content = content;
            post.EditedAt = DateTime.Now;
            post.EditedById = userId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Topic", new { id = post.TopicId });
        }

        // Usuwanie posta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var username = HttpContext.Session.GetString("User");
            var userRole = HttpContext.Session.GetString("UserRole");

            bool canDelete = post.AuthorName == username || userRole == "Admin" || userRole == "Moderator";
            if (!canDelete)
            {
                TempData["Error"] = "Nie masz uprawnień do usunięcia tego posta.";
                return RedirectToAction("Topic", new { id = post.TopicId });
            }

            var topicId = post.TopicId;
            _context.Posts.Remove(post);

            // Zaktualizuj licznik postów w wątku
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic != null)
            {
                topic.PostCount--;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Topic", new { id = topicId });
        }

        // Wyszukiwanie
        public async Task<IActionResult> Search(string query, int? forumId)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<Post>());
            }

            var posts = _context.Posts
                .Include(p => p.Topic)
                .ThenInclude(t => t!.Forum)
                .AsQueryable();

            if (forumId.HasValue)
            {
                posts = posts.Where(p => p.Topic != null && p.Topic.ForumId == forumId.Value);
            }

            var queryLower = query.ToLower();
            var results = await posts
                .Where(p => p.Content.ToLower().Contains(queryLower))
                .OrderByDescending(p => p.Created)
                .Take(50)
                .ToListAsync();

            ViewBag.Query = query;
            ViewBag.ForumId = forumId;
            ViewBag.Forums = await _context.Forums.ToListAsync();

            return View(results);
        }
    }
}
