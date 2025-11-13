using Odkop.Models;
using System.Text.Json;

namespace Odkop.Services
{
    public class ForumService
    {
        private readonly string topicsFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "topics.json");
        private readonly string postsFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "posts.json");

        public ForumService()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(topicsFile)!);
            Directory.CreateDirectory(Path.GetDirectoryName(postsFile)!);

            if (!File.Exists(topicsFile)) File.WriteAllText(topicsFile, "[]");
            if (!File.Exists(postsFile)) File.WriteAllText(postsFile, "[]");
        }

        private List<Topic> LoadTopics() => JsonSerializer.Deserialize<List<Topic>>(File.ReadAllText(topicsFile)) ?? new();
        private void SaveTopics(List<Topic> topics) => File.WriteAllText(topicsFile, JsonSerializer.Serialize(topics, new JsonSerializerOptions { WriteIndented = true }));

        private List<Post> LoadPosts() => JsonSerializer.Deserialize<List<Post>>(File.ReadAllText(postsFile)) ?? new();
        private void SavePosts(List<Post> posts) => File.WriteAllText(postsFile, JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true }));

        public List<Topic> GetAllTopics() => LoadTopics().OrderByDescending(t => t.Created).ToList();
        public Topic? GetTopic(int id) => LoadTopics().FirstOrDefault(t => t.Id == id);
        public void AddTopic(Topic topic)
        {
            var topics = LoadTopics();
            topic.Id = topics.Any() ? topics.Max(t => t.Id) + 1 : 1;
            topics.Add(topic);
            SaveTopics(topics);
        }

        public List<Post> GetPostsForTopic(int topicId) => LoadPosts().Where(p => p.TopicId == topicId).OrderBy(p => p.Created).ToList();
        public Post? GetPost(int id) => LoadPosts().FirstOrDefault(p => p.Id == id);
        public void AddPost(Post post)
        {
            var posts = LoadPosts();
            post.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
            posts.Add(post);
            SavePosts(posts);
        }

        public List<Topic> SearchTopics(string query) =>
            LoadTopics().Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Post> SearchPosts(string query) =>
            LoadPosts().Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Content.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}