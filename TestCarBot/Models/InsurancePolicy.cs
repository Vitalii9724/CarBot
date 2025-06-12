using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TestCarBot.Models;

public class InsurancePolicy
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("User")]
    public long TelegramUserId { get; set; }

    public string PolicyNumber { get; set; } = string.Empty; 
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public UserData? User { get; set; }
}