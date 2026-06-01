using System.ClientModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Saturn.Bot.Service.Services.Abstractions;
using Saturn.Telegram.Lib.Exceptions;

namespace Saturn.Bot.Service.Services;

public class AiService : IAiService
{
    private readonly ChatClient _chatClient;
    private readonly ChatClient _visionChatClient;
    private readonly ILogger<AiService> _logger;

    public AiService(
        [FromKeyedServices("chat")] ChatClient chatClient,
        [FromKeyedServices("vision")] ChatClient visionChatClient,
        ILogger<AiService> logger)
    {
        _chatClient = chatClient;
        _visionChatClient = visionChatClient;
        _logger = logger;
    }

    private static bool HasImageContent(IList<ChatMessage> messages) =>
        messages.OfType<UserChatMessage>()
            .SelectMany(m => m.Content)
            .Any(p => p.Kind == ChatMessageContentPartKind.Image);

    private ChatClient SelectClient(IList<ChatMessage> messages) =>
        HasImageContent(messages) ? _visionChatClient : _chatClient;

    public async Task<string> CompleteChatAsync(IList<ChatMessage> messages, CancellationToken ct = default)
    {
        try
        {
            var result = await SelectClient(messages).CompleteChatAsync(messages, cancellationToken: ct);
            return result.Value.Content.FirstOrDefault()?.Text ?? throw new AiEmptyResponseException();
        }
        catch (ClientResultException ex) when (ex.Status == 400)
        {
            _logger.LogError("AtlasCloud content moderation rejection (400 Bad Request)");
            throw new AiContentModerationException();
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogError("AtlasCloud balance exhausted (429 Too Many Requests)");
            throw new AiBudgetExhaustedException();
        }
    }
}
