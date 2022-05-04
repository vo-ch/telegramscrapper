using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using TelegramScraper.Data;
using TelegramScraper.Data.Entities;
using TelegramScraper.Models;

namespace TelegramScraper.Services;

public class ReportService
{
    public ReportService(TsDbContext db)
    {
        Db = db;
    }

    private TsDbContext Db { get; init; }

    public async Task GenerateUsersList()
    {
        IEnumerable<ChatMemberEntity> members = await Db.ChatMembers.OrderBy(x => x.JoinedChatDate).ToListAsync();

        IList<UsersListReportRecord> users = members.Select(m => new UsersListReportRecord()
        {
            Id = m.Id,
            Bio = m.Bio,
            Username = m.Username,
            FirstName = m.FirstName,
            LastName = m.LastName,
            PhoneNumber = $"tel: {m.PhoneNumber}",
            InviterUserId = m.InviterUserId,
            JoinedChatDateTimestamp = m.JoinedChatDate, 
            JoinedChatDate = ToDateTimeText(m.JoinedChatDate),
            AddedByUserId = m.AddedByChatMemberId,
            DeletedByUserId = m.DeletedByChatMemberId,
        }).ToList();

        foreach (UsersListReportRecord user in users)
        {
            if (user.AddedByUserId is not null)
            {
                var addedBy = members.FirstOrDefault(x => x.Id == user.AddedByUserId);
                if (addedBy is not null)
                {
                    user.AddedByUser =
                    $"{addedBy.Username} || {addedBy.FirstName} {addedBy.LastName} || {addedBy.PhoneNumber}";
                }
            }
            if (user.DeletedByUserId is not null)
            {
                var deletedBy = members.FirstOrDefault(x => x.Id == user.DeletedByUserId);
                if (deletedBy is not null)
                {
                    user.DeletedByUser =
                    $"{deletedBy.Username} || {deletedBy.FirstName} {deletedBy.LastName} || {deletedBy.PhoneNumber}";
                }
            }

            user.MessagesTotal = await Db.ChatMessages.CountAsync(x => x.UserId == user.Id);

        }
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
        };

        await using var writer = new StreamWriter("users.csv");
        await using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(users);
        
        Console.WriteLine("Success!!");
    }

    public async Task GenerateUserMessagesReport()
    {
        var msgs = await Db.ChatMessages.OrderBy(x => x.Date).ToListAsync();
        var usrs = await Db.ChatMembers.ToListAsync();

        var records = msgs.Select(m =>
        {
            var usr = usrs.FirstOrDefault(u => u.Id == m.UserId);
            return new UserMessagesReportRecord()
            {
                Id = m.Id,
                Content = m.Content,
                ContentType = m.ContentType,
                ContentDataType = m.ContentDataType,
                DateTimestamp = m.Date,
                Date = ToDateTimeText(m.Date),
                EditDate = ToDateTimeText(m.EditDate),
                UserId = m.UserId,
                Username = usr?.Username,
                Phone = $"tel: {usr?.PhoneNumber}",
                User = usr is null ? "" : $"{usr.Username} || {usr.FirstName} {usr.LastName} || {usr.PhoneNumber}",
                AuthorSignature = m.AuthorSignature,
                DataType = m.DataType,
                ForvardedFromChatId = m.ForvardedFromChatId,
                ForvardedFromMsgId = m.ForvardedFromMsgId,
                ReplyToMsgId = m.ReplyToMsgId,
            };
        }).ToList();
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            ShouldQuote = args =>
            {
                if (string.IsNullOrEmpty(args.Field)) return false;

                return args.FieldType == typeof(string);
            }
        };

        await using var writer = new StreamWriter("messages.csv");
        await using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(records);
        
        Console.WriteLine("Success!!");
    }

    private string? ToDateTimeText(int? ts)
    {
        if (ts is null) return null;
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(ts ?? 0).ToLocalTime();
        return dateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss");
    }
}