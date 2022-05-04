// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using TdLib;
using TdLib.Bindings;using TelegramScraper.Data;
using TelegramScraper.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using TsDbContext db = new TsDbContext();

await db.Database.EnsureCreatedAsync();

var bindings = Interop.AutoDetectBindings();
            bindings.SetLogVerbosityLevel(0);
using TdClient tdClient = new TdClient(bindings);

TelegramService telegram = new TelegramService(tdClient, db);
ReportService reportService = new ReportService(db);

// Root
// -----------------------------------------
var rootCommand = new RootCommand("Get info from telegram chat");
rootCommand.SetHandler(() => { Console.WriteLine("Hello"); });

// Login
// -----------------------------------------
var loginCommand = new Command("login", "Login to telegram");
loginCommand.SetHandler(async () =>
{
    await telegram.Login();
});
rootCommand.AddCommand(loginCommand);

// Find chat 
// -----------------------------------------
var chatNameOption = new Option<string>( name: "--chat-name", description: "Chat name")
{
    IsRequired = true,
};

var findChatIdCommand = new Command("find-chat-id", "Find chat by name");
findChatIdCommand.SetHandler(async (string name) =>
{
    await telegram.TryFindChat(name);
}, chatNameOption);
findChatIdCommand.AddOption(chatNameOption);
rootCommand.AddCommand(findChatIdCommand);

// Load chat members
// -----------------------------------------
var loadUsersCommand = new Command("load-users", "Load users by chat name to db");
loadUsersCommand.SetHandler(async (string name) =>
{
    await telegram.LoadUsersData(name);
}, chatNameOption);
loadUsersCommand.AddOption(chatNameOption);
rootCommand.AddCommand(loadUsersCommand);

// Load chat members
// -----------------------------------------
var loadMessagesCommand = new Command("load-msg", "Load messages by chat name to db");
loadMessagesCommand.SetHandler(async (string name) =>
{
    await telegram.LoadMessages(name);
}, chatNameOption);
loadMessagesCommand.AddOption(chatNameOption);
rootCommand.AddCommand(loadMessagesCommand);

//----------------------------------------
// Load chat members
// -----------------------------------------

var deleteDbCommand = new Command("delete-db", "Delete existing local DB");
deleteDbCommand.SetHandler(async () =>
{
    await db.Database.EnsureDeletedAsync();
    Console.WriteLine("DB successfully deleted");
});
rootCommand.AddCommand(deleteDbCommand);

//----------------------------------------
// Generate users list report
// -----------------------------------------

var generateUsersListCommand = new Command("generate-users-list", "Generates users list report in csv");
generateUsersListCommand.SetHandler(async () =>
{
    await reportService.GenerateUsersList();
});
rootCommand.AddCommand(generateUsersListCommand);

//----------------------------------------
// Generate users list report
// -----------------------------------------

var generateMsgListCommand = new Command("generate-msg-list", "Generates messages list report in csv");
generateMsgListCommand.SetHandler(async () =>
{
    await reportService.GenerateUserMessagesReport();
});
rootCommand.AddCommand(generateMsgListCommand);

//----------------------------------------
return await rootCommand.InvokeAsync(args);