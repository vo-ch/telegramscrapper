using TdLib;
using TelegramScraper.Data;
using TelegramScraper.Data.Entities;

namespace TelegramScraper.Services;

public class TelegramService
{
    private const int DefaultAppId = 14760668;
    private const string DefaultAppHash = "57d30080a8df5d5ad840d3ec0b82ab2d";
    private const string InfoFilePath = "telegram_app_info.txt";
    public TelegramService(TdClient tdLib, TsDbContext db)
    {
        Client = tdLib;
        Db = db;
    }

    private TsDbContext Db { get; init; }

    private TdClient Client { get; init; }

    public async Task Login()
    {
        // await using (StreamWriter file = new(InfoFilePath))
        // {
        //     Console.WriteLine($"Enter your telegram appId (leave empty for default {DefaultAppId}): ");
        //     string appIdString = Console.ReadLine() ?? $"{DefaultAppId}";
        //     await file.WriteLineAsync(appIdString);
        //     
        //     Console.WriteLine($"Enter your telegram appHash (leave empty for default {DefaultAppHash}): ");
        //     string appHash = Console.ReadLine() ?? $"{DefaultAppHash}";
        //     await file.WriteLineAsync(appHash);
        // }
        
        await Init();
        try
        {
            Console.WriteLine("Enter your phone number: ");
            string phone = Console.ReadLine() ?? "";
            await Client.SetAuthenticationPhoneNumberAsync(phone);
            
            Console.WriteLine("Enter verification code: ");
            string code = Console.ReadLine() ?? "";
            
            if (!string.IsNullOrEmpty(code)) await Client.CheckAuthenticationCodeAsync(code);
            Console.WriteLine("Success!!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }

    public async Task<TdApi.Chat?> TryFindChat(string name)
    {
        await Init();
        
        TdApi.Chats chats = await Client.GetChatsAsync(limit: 400);
        bool isFound = false;
        foreach (var chatId in chats.ChatIds)
        {
             TdApi.Chat chat = await Client.GetChatAsync(chatId);
             if (chat.Title == name && chat.Type is TdApi.ChatType.ChatTypeSupergroup supergroup)
             {
                 Console.WriteLine($"Found chat id: {chat.Id}; supergroup id: {supergroup.SupergroupId} ; datatype: {chat.DataType}");
                 isFound = true;

                 return chat;
             }
        }
        if (!isFound) Console.WriteLine($"Chat '{name}' was not found");

        return null;
    }

    public async Task LoadUsersData(string chatName)
    {
        TdApi.Chat? chat = await TryFindChat(chatName);
        Console.WriteLine($"Getting members from {chat?.Id}");
        var sg = chat?.Type as TdApi.ChatType.ChatTypeSupergroup;

        int offset = 0;
        int limit = 100;
        int total = 0;

        do
        {
            
            var members = await Client.GetSupergroupMembersAsync(supergroupId: sg?.SupergroupId ?? 0, offset: offset, limit: limit);
            total = members.TotalCount;
            Console.WriteLine($"Total {total} offset {offset}");
            int i = 0;
            // TdApi.ChatMembers members = await Client.GetSupergroupMembersAsync(supergroupId: chatId, limit: 500);
            foreach (TdApi.ChatMember chatMember in members.Members)
            {
                i++;
                if (chatMember.MemberId is TdApi.MessageSender.MessageSenderUser user)
                {
                    var userDetails = await Client.GetUserAsync(user.UserId);
                    var userFull = await Client.GetUserFullInfoAsync(user.UserId);
                    Console.WriteLine($"{i}; Id: {user.UserId}; Member: {userDetails.Username} {userDetails.FirstName} {userDetails.LastName};  invited by: {chatMember.InviterUserId}");

                    var userToSave = new ChatMemberEntity()
                    {
                        Id = user.UserId,
                        InviterUserId = chatMember.InviterUserId,
                        JoinedChatDate = chatMember.JoinedChatDate,
                        FirstName = userDetails.FirstName,
                        LastName = userDetails.LastName,
                        Username = userDetails.Username,
                        PhoneNumber = userDetails.PhoneNumber,
                        Bio = userFull.Bio,
                    };

                    Db.ChatMembers.Add(userToSave);
                    await Db.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"{i}; Id: {chatMember.MemberId}; ");
                }
            }
            
            offset += 100;
        } while (total > offset);
        
        Console.WriteLine("Success!!");

    }

    public async Task LoadMessages(string chatName)
    {
        
        TdApi.Chat? chat = await TryFindChat(chatName);
        Console.WriteLine($"Getting messages from {chat?.Id}");

        long lastMsgId = 0;
        int limit = 100;
        int totalRequestLoaded = 0;
        int total = 0;

        do
        {
            var result = await Client.GetChatHistoryAsync(chatId: chat?.Id ?? 0, fromMessageId: lastMsgId, limit: limit);
            total = result.TotalCount;
            totalRequestLoaded = result.Messages_.Length;
            Console.WriteLine($"Total {total} per request: {totalRequestLoaded}");

            foreach (var message in result.Messages_)
            {
                lastMsgId = message.Id;

                await ProcessMessage(message);
            }

            await Db.SaveChangesAsync();

        } while (totalRequestLoaded > 0);
    }

    private async Task ProcessMessage(TdApi.Message msg)
    {
        switch (msg.Content)
        {
            case TdApi.MessageContent.MessageAnimatedEmoji content:
                await ProcessMsgAnimatedEmogi(msg, content);
                break;
            case TdApi.MessageContent.MessageAnimation content:
                await ProcessMsgAnimation(msg, content);
                break;
            case TdApi.MessageContent.MessageAudio content:
                await ProcessMsgAudio(msg, content);
                break;
            case TdApi.MessageContent.MessageChatAddMembers content:
                await ProcessMsgChatAddMembers(msg, content);
                break;
            case TdApi.MessageContent.MessageChatChangePhoto content:
                await ProcessMsgChatChangePhote(msg, content);
                break;
            case TdApi.MessageContent.MessageChatChangeTitle content:
                await ProcessMsgChatChangeTitle(msg, content);
                break;
            case TdApi.MessageContent.MessageChatDeleteMember content:
                await ProcessMsgChatDeleteMember(msg, content);
                break;
            case TdApi.MessageContent.MessageChatJoinByLink content:
                await ProcessMsgChatJoinByLink(msg, content);
                break;
            case TdApi.MessageContent.MessageChatJoinByRequest content:
                await ProcessMsgChatJoinByRequest(msg, content);
                break;
            case TdApi.MessageContent.MessageContact content:
                await ProcessMsgContact(msg, content);
                break;
            case TdApi.MessageContent.MessageContactRegistered content:
                await ProcessMsgContactRegistered(msg, content);
                break;
            case TdApi.MessageContent.MessagePhoto content:
                await ProcessMsgPhoto(msg, content);
                break;
            case TdApi.MessageContent.MessagePinMessage content:
                await ProcessMsgPinMessage(msg, content);
                break;
            case TdApi.MessageContent.MessagePoll content:
                await ProcessMsgPoll(msg, content);
                break;
            case TdApi.MessageContent.MessageScreenshotTaken content:
                await ProcessMsgScreenshotTaken(msg, content);
                break;
            case TdApi.MessageContent.MessageSticker content:
                await ProcessMsgSticker(msg, content);
                break;
            case TdApi.MessageContent.MessageText content:
                await ProcessMsgText(msg, content);
                break;
            case TdApi.MessageContent.MessageVideo content:
                await ProcessMsgVideo(msg, content);
                break;
            case TdApi.MessageContent.MessageVideoNote content:
                await ProcessMsgVideoNote(msg, content);
                break;
            case TdApi.MessageContent.MessageVoiceNote content:
                await ProcessMsgVoiceNote(msg, content);
                break;
            default:
                await ProcessMessageDefault(msg);
                break;
        }
    }

    private void LogMsg(TdApi.Message msg, string content)
    {
        long senderId = GetSender(msg.SenderId);
        Console.WriteLine($"Id: {msg.Id} ; senderId: {senderId} ; date: {new DateTime(msg.Date)}");
        Console.WriteLine($"Content: {content}");
    }

    private ChatMessageEntity BuildDefaultChatMessage(TdApi.Message msg)
    {
        long senderId = GetSender(msg.SenderId);
        
        return new ChatMessageEntity()
        {
            Id = msg.Id,
            UserId = senderId,
            Date = msg.Date,
            AuthorSignature = msg.AuthorSignature,
            ContentType = msg.Content.ToString(),
            ContentDataType = msg.Content.DataType,
            DataType = msg.DataType,
            EditDate = msg.EditDate,
            ForvardedFromMsgId = msg.ForwardInfo?.FromMessageId,
            ForvardedFromChatId = msg.ForwardInfo?.FromChatId,
            ReplyToMsgId = msg.ReplyToMessageId,
        };
    }
    
    private long GetSender(TdApi.MessageSender sender)
    {
        if (sender is TdApi.MessageSender.MessageSenderUser senderUser)
        {
            return senderUser.UserId;
        }
        if (sender is TdApi.MessageSender.MessageSenderChat senderChat)
        {
            return senderChat.ChatId;
        }
        Console.WriteLine("!!ERROR: sender type is not recognized");
        return 0;
    }

    private async Task ProcessMessageDefault(TdApi.Message msg)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!content_not_supported_yet!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgVoiceNote(TdApi.Message msg, TdApi.MessageContent.MessageVoiceNote content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = content?.Caption?.Text ?? "!!No_content!!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgVideoNote(TdApi.Message msg, TdApi.MessageContent.MessageVideoNote content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!content_omitted_in_text_format!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgVideo(TdApi.Message msg, TdApi.MessageContent.MessageVideo content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Video!; !caption!:{content?.Caption?.Text}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgText(TdApi.Message msg, TdApi.MessageContent.MessageText content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = content?.Text?.Text ?? "!!No_content!!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgSticker(TdApi.Message msg, TdApi.MessageContent.MessageSticker content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!sticker!: {content?.Sticker?.Emoji} !from_set_id!: {content?.Sticker?.SetId}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgScreenshotTaken(TdApi.Message msg, TdApi.MessageContent.MessageScreenshotTaken content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!Screenshot_taken!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgPoll(TdApi.Message msg, TdApi.MessageContent.MessagePoll content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);

        entity.Content =
        $"!Poll!: {content?.Poll?.Question}; !Options!: {string.Join(" ||| ", content?.Poll?.Options.Select(o => o.Text).ToArray() ?? new string[] { })}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgPinMessage(TdApi.Message msg, TdApi.MessageContent.MessagePinMessage content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = content?.MessageId.ToString() ?? "!!No_content!!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgPhoto(TdApi.Message msg, TdApi.MessageContent.MessagePhoto content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Photo!. Caption: {content?.Caption?.Text}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgContactRegistered(TdApi.Message msg, TdApi.MessageContent.MessageContactRegistered content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!Contact_registered!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgContact(TdApi.Message msg, TdApi.MessageContent.MessageContact content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Contact!: {content?.Contact?.UserId}; {content?.Contact?.PhoneNumber}; {content?.Contact?.FirstName} {content?.Contact?.LastName}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatJoinByRequest(TdApi.Message msg, TdApi.MessageContent.MessageChatJoinByRequest content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!Join_by_request!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatJoinByLink(TdApi.Message msg, TdApi.MessageContent.MessageChatJoinByLink content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!Join_by_chat_link!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatDeleteMember(TdApi.Message msg, TdApi.MessageContent.MessageChatDeleteMember content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);

        var user = await Db.ChatMembers.FindAsync(content.UserId);
        if (user is null)
        {
            Db.ChatMembers.Add(new ChatMemberEntity()
            {
                Id = content.UserId,
                DeletedByChatMemberId = entity.UserId,
            });
        }
        
        entity.Content = $"!Deleted_member!: {content?.UserId}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatChangeTitle(TdApi.Message msg, TdApi.MessageContent.MessageChatChangeTitle content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Changed_title!: {content?.Title}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatChangePhote(TdApi.Message msg, TdApi.MessageContent.MessageChatChangePhoto content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = "!changed_photo!";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgChatAddMembers(TdApi.Message msg, TdApi.MessageContent.MessageChatAddMembers content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);

        foreach (long addedUserId in content.MemberUserIds)
        {
            ChatMemberEntity? addedUser = await Db.ChatMembers.FindAsync(addedUserId);
            if (addedUser is not null)
            {
                addedUser.AddedByChatMemberId = entity.UserId;
            }
        }
        
        entity.Content = $"!Added_users!: {string.Join(", ", content.MemberUserIds)}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgAudio(TdApi.Message msg, TdApi.MessageContent.MessageAudio content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Audio!; !caption!: {content?.Caption?.Text}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgAnimation(TdApi.Message msg, TdApi.MessageContent.MessageAnimation content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Animation! caption: {content?.Caption?.Text}; filename: {content?.Animation?.FileName}; duration: {content?.Animation?.Duration}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task ProcessMsgAnimatedEmogi(TdApi.Message msg, TdApi.MessageContent.MessageAnimatedEmoji content)
    {
        ChatMessageEntity entity = BuildDefaultChatMessage(msg);
        
        entity.Content = $"!Animated_emoji!: {content?.Emoji}";
        Db.ChatMessages.Add(entity);
        
        LogMsg(msg, entity.Content);
    }

    private async Task Init()
    {
        // string[] lines = await File.ReadAllLinesAsync(InfoFilePath);
        //
        // string appIdString = lines[0];
        // string appHash = lines[1];
        // int appId;
        //
        // if (!int.TryParse(appIdString, out appId))
        // {
        //     Console.WriteLine($"Incorrect appId {appIdString}");
        //
        //     throw new Exception($"Incorrect appId {appIdString}");
        // }
        
        await Client.SetTdlibParametersAsync(new TdApi.TdlibParameters()
        {
            ApiId = DefaultAppId,
            ApiHash = DefaultAppHash,
            SystemLanguageCode = "ua",
            DeviceModel = "windows11",
            ApplicationVersion = "0.0.1",
        });
        
        await Client.CheckDatabaseEncryptionKeyAsync();
    }
}