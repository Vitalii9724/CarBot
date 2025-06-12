using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestCarBot.Models
{
    public class DocumentData
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public long TelegramUserId { get; set; }

        public string FileId { get; set; } = null!;
        public string DocumentType { get; set; } = null!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public UserData? User { get; set; }
    }
}