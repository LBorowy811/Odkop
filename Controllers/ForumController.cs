using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Odkop.Data;
using Odkop.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Odkop.Controllers
{
    public class ForumController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForumController> _logger;

        public ForumController(ApplicationDbContext context, ILogger<ForumController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string search)
        {
            var topics = _context.Topics.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                topics = topics.Where(t => t.Title.Contains(search));

            var list = await topics.OrderByDescending(t => t.Created).ToListAsync();
            return View(list);
        }

        public IActionResult CreateTopic()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("User")))
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTopic(Topic model)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                model.Author = user;
                model.Created = DateTime.Now;

                _context.Topics.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Dodano temat: {Title} przez {Author}", model.Title, model.Author);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> TopicDetails(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            var posts = await _context.Posts
                .Where(p => p.TopicId == id)
                .OrderBy(p => p.Created)
                .ToListAsync();

            ViewBag.User = HttpContext.Session.GetString("User");
            return View((topic, posts));
        }

        public IActionResult CreatePost(int topicId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("User")))
                return RedirectToAction("Login", "Account");

            ViewBag.TopicId = topicId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(int topicId, string title, string content)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Tytuł i treść posta są wymagane.";
                return RedirectToAction("CreatePost", new { topicId });
            }

            var post = new Post
            {
                TopicId = topicId,
                Author = user,
                Title = title,
                Content = content,
                Created = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("TopicDetails", new { id = topicId });
        }

        public async Task<IActionResult> PostDetails(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        public async Task<IActionResult> SearchPosts(int topicId, string query)
        {
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic == null) return NotFound();

            var posts = _context.Posts
                .Where(p => p.TopicId == topicId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                posts = posts.Where(p => p.Title.ToLower().Contains(query) || p.Content.ToLower().Contains(query));
            }

            var list = await posts.OrderBy(p => p.Created).ToListAsync();
            ViewBag.User = HttpContext.Session.GetString("User");
            ViewBag.Query = query;

            return View("TopicDetails", (topic, list));
        }
    }
}
