using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        [Required]
        public string Author { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;

    }
}
