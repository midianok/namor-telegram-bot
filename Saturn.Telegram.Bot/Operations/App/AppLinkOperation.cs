using Saturn.Telegram.Lib.Operation;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Saturn.Bot.Service.Operations.App;

public class AppLinkOperation : IOperation
{
    private readonly TelegramBotClient _botClient;

    public AppLinkOperation(TelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public bool Validate(Message msg, UpdateType type) =>
        string.Equals(msg.Text?.Trim(), "бот", StringComparison.OrdinalIgnoreCase);

    public async Task OnMessageAsync(Message msg, UpdateType type)
    {
        var keyboard = new InlineKeyboardMarkup(
            InlineKeyboardButton.WithUrl("Открыть", $"https://t.me/rt5263bot/app?startapp={msg.Chat.Id}"));

        await _botClient.SendMessage(msg.Chat, "НаморApp", replyMarkup: keyboard);
    }
}
