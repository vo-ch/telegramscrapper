using Microsoft.EntityFrameworkCore;
using TdLib;
using TelegramScraper.Data.Entities;

namespace TelegramScraper.Data;

public class TsDbContext : DbContext
{
    public DbSet<ChatMemberEntity> ChatMembers { get; set; }
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    public string DbPath { get; }

    public TsDbContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "ts.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}