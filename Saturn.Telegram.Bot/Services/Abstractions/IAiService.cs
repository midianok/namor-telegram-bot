using OpenAI.Chat;

namespace Saturn.Bot.Service.Services.Abstractions;

public interface IAiService
{
    Task<string> CompleteChatAsync(IList<ChatMessage> messages, CancellationToken ct = default);
}
