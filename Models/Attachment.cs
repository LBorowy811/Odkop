namespace Odkop.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
