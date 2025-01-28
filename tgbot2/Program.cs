using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Linq;
using static Dropbox.Api.Sharing.ListFileMembersIndividualResult;
using static Program;
using System.Net.Http;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using Model;
using Dropbox.Api.Files;
using Telegram.Bot.Types.ReplyMarkups;



class Program
{
    private static readonly string botToken = "";
    private static TelegramBotClient botClient;
    private static Dictionary<long, Meme> userMemes = new Dictionary<long, Meme>();
    private static Dictionary<long, bool> userAccessStatus = new Dictionary<long, bool>();
    private const string password = "пароль";
    private static List<string> allTags = new List<string>();


    static async Task Main(string[] args)
    {
        botClient = new TelegramBotClient(botToken);

        var me = await botClient.GetMe();
        Console.WriteLine($"Bot ID: {me.Id}, Bot name: {me.FirstName}");

        using (var cts = new CancellationTokenSource())
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                cancellationToken: cts.Token
            );
            await SetBotCommandsAsync(botClient, cts.Token);
            Console.WriteLine("Бот запущен. Нажмите Ctrl+C для выхода из программы.");
            await Task.Delay(-1);
        }
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message == null)
            return;

        var message = update.Message;
        long userId = message.Chat.Id;

        if (!userMemes.ContainsKey(userId))
            userMemes[userId] = new Meme();

        if (!userAccessStatus.ContainsKey(userId))
            userAccessStatus[userId] = false;

        var userMeme = userMemes[userId];
        BL logic = new BL();

        Console.WriteLine($"User {message.Chat.FirstName}: Current state is {userMeme.State}");

        if (message.Type == MessageType.Text && userMeme.State == State.Idle)
        {
            string command = message.Text;
            Console.WriteLine($"Получено сообщение от {message.Chat.FirstName}: {message.Text}");

            if (command.ToLower() == "/start")
            {
                await botClient.SendMessage(message.Chat.Id, "дарова это бот для мемов карины дорожки", cancellationToken: cancellationToken);
            }
            else if (command.ToLower() == "/help")
            {
                await botClient.SendMessage(message.Chat.Id, "Для загрузки мемов введите пароль и отправьте фото\n" +
                    "список команд:\n" +
                    "/gmbt [теги через пробел] - получить мемы по тегам\n" +
                    "/gmbd [описание] - получить мемы по описанию\n" +
                    "/getallmemes - получить все мемы\n" +
                    "/getalltags - получить все теги\n" +
                    "/password [пароль] - получить доступ для загрузки мемов\n" +
                    "/help - помощь", cancellationToken: cancellationToken);
            }
            else if (command.StartsWith("/gmbt "))
            {
                await MemesByTags(botClient, logic, message, command);
            }
            else if (command.StartsWith("/gmbd "))
            {
                await MemesByDescription(botClient, logic, message, command);
            }
            else if (command.ToLower() == "/getallmemes")
            {
                foreach (Meme meme in logic.GetAllMemes())
                {
                    Console.WriteLine($"{Convert.ToString(meme.Id)}, {meme.Description}, {string.Join(" ", meme.Tags)}");
                    await botClient.SendPhoto(
                    chatId: userId,
                    photo: meme.ImageId,
                    caption: $"📄 *Описание*: {meme.Description}\n🏷️ *Теги*: {string.Join(", ", meme.Tags)}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                }
            }
            else if (command.ToLower() == "/getalltags")
            {
                string tags = string.Join("\n", allTags);
                await botClient.SendMessage(message.Chat.Id, "все теги:" + tags, cancellationToken: cancellationToken);
            }
            else if (command.StartsWith("/password "))
            {
                string commandstarts = "/password ";
                string pass = command.Substring(commandstarts.Length);
                if (pass == password)
                {
                    Console.WriteLine($"User {message.Chat.FirstName}: правильно ввел пароль");
                    userAccessStatus[userId] = true;

                    await botClient.SendMessage(
                        chatId: userId,
                        text: "ура проверка пройдена!! отправьте фото",
                        cancellationToken: cancellationToken
                    );
                }
                else if (pass != password)
                {
                    await botClient.SendMessage(
                        chatId: userId,
                        text: "неверный пароль",
                        cancellationToken: cancellationToken
                    );
                    Console.WriteLine($"User {message.Chat.FirstName}: неправильно ввел пароль");
                }
            }
            else if (command.ToLower() == "/help2")
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Выберите команду:",
                    replyMarkup: new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Получить мемы по тегам", "/getmemesbytags "),
                            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Получить все мемы", "/getallmemes")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Старт", "/start")
                        }
                    }),
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(message.Chat.Id, "нипанимаю", cancellationToken: cancellationToken);
            }
        }
        if (message.Type == MessageType.Photo && userMeme.State == State.Idle && userAccessStatus[userId])
        {
            var photoList = message.Photo;
            var fileId = photoList[photoList.Count() - 1].FileId;

            userMeme.ImageId = fileId;
            userMeme.State = State.AwaitingTags;

            await botClient.SendMessage(
                chatId: userId,
                text: "фото сохранено! отправьте теги",
                cancellationToken: cancellationToken
            );
        }
        else if (userMeme.State == State.AwaitingTags && message.Type == MessageType.Text && userAccessStatus[userId])
        {
            userMeme.Tags = message.Text;
            foreach (string tag in message.Text.Split(' '))
            {
                if (!allTags.Contains( tag )) allTags.Add( tag );
            }

            userMeme.State = State.AwaitingDescription;

            await botClient.SendMessage(
                chatId: userId,
                text: "теги сохранены! теперь отправьте описание для изображения",
                cancellationToken: cancellationToken
            );
        }
        else if (userMeme.State == State.AwaitingDescription && message.Type == MessageType.Text && userAccessStatus[userId])
        {
            userMeme.Description = message.Text;
            userMeme.State = State.Idle;
            logic.AddMeme(userMeme.Tags, userMeme.Description, userMeme.ImageId);

            await botClient.SendMessage(
                chatId: userId,
                text: "описание сохранено! спасибо, вот информация о вашем изображении:",
                cancellationToken: cancellationToken
            );

            await botClient.SendPhoto(
                    chatId: userId,
                    photo: userMeme.ImageId,
                    caption: $"📄 *Описание*: {userMeme.Description}\n🏷️ *Теги*: {userMeme.Tags}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
        }
        else if (message.Type == MessageType.Photo && userMeme.State == State.Idle && !userAccessStatus[userId])
        {
            await botClient.SendMessage(
                chatId: userId,
                text: "введите пароль с помощью /password [пароль]",
                cancellationToken: cancellationToken
            );
        }
    }
    private static async Task SetBotCommandsAsync(ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        var commands = new List<BotCommand>
    {
        new BotCommand { Command = "start", Description = "начало работы с ботом" },
        new BotCommand { Command = "getallmemes", Description = "получить все мемы" },
        new BotCommand { Command = "help", Description = "список комманд" },
        new BotCommand { Command = "getalltags", Description = "получить все теги" }
    };

        await botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
    }
    private static async Task MemesByTags(ITelegramBotClient botClient, BL logic, Message message, string command)
    {
        string commandstarts = "/gmbt ";
        string tags = command.Substring(commandstarts.Length);
        var memes = logic.GetMemesByTags(tags);

        foreach (var meme in memes)
        {
                await botClient.SendPhoto(message.Chat.Id, meme.ImageId, caption: "вот мем с тегами: " + string.Join(", ", tags));
        }
    }
    private static async Task MemesByDescription(ITelegramBotClient botClient, BL logic, Message message, string command)
    {
        string commandstarts = "/gmbd ";
        string desc = command.Substring(commandstarts.Length);
        var memes = logic.GetMemesByDescription(desc);

        foreach (var meme in memes)
        {
            await botClient.SendPhoto(message.Chat.Id, meme.ImageId, caption: "вот мем с описанием: " + string.Join(", ", desc));
        }
    }
    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Произошла ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}
