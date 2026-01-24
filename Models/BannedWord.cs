using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class BannedWord
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Słowo jest wymagane")]
        [StringLength(100, ErrorMessage = "Słowo nie może być dłuższe niż 100 znaków")]
        public string Word { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Reason { get; set; }
    }
}
