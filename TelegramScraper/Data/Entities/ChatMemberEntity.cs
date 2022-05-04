using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelegramScraper.Data.Entities;

public class ChatMemberEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public long Id { get; set; }
    
    public long? InviterUserId { get; set; }
    
    public int? JoinedChatDate { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? PhoneNumber { get; set; }
    
    public string? Bio { get; set; }
    
    public long? AddedByChatMemberId { get; set; }
    
    public long? DeletedByChatMemberId { get; set; }
}