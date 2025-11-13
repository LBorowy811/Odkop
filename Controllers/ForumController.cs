using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Odkop.Models;
using Odkop.Services;

namespace Odkop.Controllers
{
    public class ForumController : Controller
    {
        private readonly ForumService _forumService;
        private readonly ILogger<ForumController> _logger;

        public ForumController(ForumService forumService, ILogger<ForumController> logger)
        {
            _forumService = forumService;
            _logger = logger;
        }

        public IActionResult Index(string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return View(_forumService.SearchTopics(search));

            return View(_forumService.GetAllTopics());
        }

        public IActionResult CreateTopic()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("User")))
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult CreateTopic(Topic model)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                model.Author = user;
                _forumService.AddTopic(model);
                _logger.LogInformation("Dodano temat: {Title} przez {Author}", model.Title, model.Author);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult TopicDetails(int id)
        {
            var topic = _forumService.GetTopic(id);
            if (topic == null) return NotFound();

            var posts = _forumService.GetPostsForTopic(id);
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
        public IActionResult CreatePost(int topicId, string title, string content)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user)) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Tytuł i treść posta są wymagane.";
                return RedirectToAction("CreatePost", new { topicId });
            }

            _forumService.AddPost(new Post
            {
                TopicId = topicId,
                Author = user,
                Title = title,
                Content = content
            });

            return RedirectToAction("TopicDetails", new { id = topicId });
        }
        public IActionResult PostDetails(int id)
        {
            var post = _forumService.GetPost(id);
            if (post == null) return NotFound();
            return View(post);
        }
        public IActionResult SearchPosts(int topicId, string query)
        {
            var topic = _forumService.GetTopic(topicId);
            if (topic == null) return NotFound();

            var posts = _forumService.GetPostsForTopic(topicId);

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                posts = posts
                    .Where(p => (p.Title != null && p.Title.ToLower().Contains(query))
                             || (p.Content != null && p.Content.ToLower().Contains(query)))
                    .ToList();
            }

            ViewBag.User = HttpContext.Session.GetString("User");
            ViewBag.Query = query;

            return View("TopicDetails", (topic, posts));
        }
    }
}