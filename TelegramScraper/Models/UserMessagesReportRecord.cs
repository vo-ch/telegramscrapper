namespace TelegramScraper.Models;

public class UserMessagesReportRecord
{
    public long Id { get; set; }
    
    public long? UserId { get; set; }
    
    public string? User { get; set; }
    
    public string? Content { get; set; }

    public string? Date { get; set; }

    public string? AuthorSignature { get; set; }

    public string? ContentType { get; set; }

    public string? DataType { get; set; }

    public string? ContentDataType { get; set; }

    public string? EditDate { get; set; }

    public long? ForvardedFromChatId { get; set; }

    public long? ForvardedFromMsgId { get; set; }

    public long? ReplyToMsgId { get; set; }

    public string? Username { get; set; }

    public int? DateTimestamp { get; set; }

    public string? Phone { get; set; }
}