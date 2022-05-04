# Telegram Scrapper

Execute steps in PowerShell console.

in order to get `.\TelegramScraper.exe` run `dotnet publish`

or you can use `dotnet run` from project root instead 

## 1 step: Login
```
> .\TelegramScraper.exe login
Enter your phone number:
+380635385207
Enter verification code:
64977
Success!!
```

or

```
> dotnet run login
Enter your phone number:
+380635385207
Enter verification code:
64977
Success!!
```

## 2 step: Load users
```
.\TelegramScraper.exe load-users --chat-name 'My chat name'
```
or from source code root
```
dotnet run load-users --chat-name 'My chat name'
```

Wait, it can take few minutes...

## 3 step: Load messages
```
.\TelegramScraper.exe load-msg --chat-name 'My chat name'
```
or from source code root
```
dotnet run load-msg --chat-name 'My chat name'
```

Wait, it can take few minutes...

## 4 step: Generate users list report
```
.\TelegramScraper.exe generate-users-list
```
or from source code root
```
dotnet run generate-users-list
```

See result in users.csv, in same directory where TelegramScraper.exe is located


## 5 step: Generate users list report
```
.\TelegramScraper.exe generate-msg-list
```
or from source code root
```
dotnet run generate-msg-list
```

See result in messages.csv, in same directory where TelegramScraper.exe is located

## 6 step: Upload csv files to google sheets or import in Excel to view results