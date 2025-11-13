using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Topic
    {
        public int Id { get; set; }
        [Required, StringLength(100, ErrorMessage ="Tytuł nie może być dłuższy niż 100 znaków.")]
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;

    }
}
