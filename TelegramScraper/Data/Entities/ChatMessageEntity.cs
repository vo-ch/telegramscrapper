using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TdLib;

namespace TelegramScraper.Data.Entities;

public class ChatMessageEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Required]
    public long Id { get; set; }
    
    public long? UserId { get; set; }
    
    public string? Content { get; set; }

    public int? Date { get; set; }

    public string? AuthorSignature { get; set; }

    public string? ContentType { get; set; }

    public string? DataType { get; set; }

    public string? ContentDataType { get; set; }

    public int? EditDate { get; set; }

    public long? ForvardedFromChatId { get; set; }

    public long? ForvardedFromMsgId { get; set; }

    public long? ReplyToMsgId { get; set; }
}