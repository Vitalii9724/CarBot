using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestCarBot.Models
{
    public class UserData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TelegramUserId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PassportNumber { get; set; }
        public string? CarNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CarModel { get; set; }

        public UserStateEnum State { get; set; } = UserStateEnum.AwaitingStart;

        public ICollection<DocumentData> Documents { get; set; } = new List<DocumentData>();
        public ICollection<InsurancePolicy> Policies { get; set; } = new List<InsurancePolicy>();
    }
}