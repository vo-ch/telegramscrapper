namespace TelegramScraper.Models;

public class UsersListReportRecord
{
    public long Id { get; set; }
    
    public long? InviterUserId { get; set; }
    
    public string? JoinedChatDate { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? PhoneNumber { get; set; }
    
    public string? Bio { get; set; }
    
    public long? AddedByUserId { get; set; }
    public string? AddedByUser { get; set; }
    
    public long? DeletedByUserId { get; set; }
    public string? DeletedByUser { get; set; }

    public int? JoinedChatDateTimestamp { get; set; }

    public int? MessagesTotal { get; set; }
}