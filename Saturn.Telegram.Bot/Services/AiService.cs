using System.ClientModel;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Saturn.Bot.Service.Services.Abstractions;
using Saturn.Telegram.Lib.Exceptions;

namespace Saturn.Bot.Service.Services;

public class AiService : IAiService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AiService> _logger;

    public AiService(ChatClient chatClient, ILogger<AiService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<string> CompleteChatAsync(IList<ChatMessage> messages, CancellationToken ct = default)
    {
        try
        {
            var result = await _chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            return result.Value.Content.FirstOrDefault()?.Text ?? throw new AiEmptyResponseException();
        }
        catch (ClientResultException ex) when (ex.Status == 400)
        {
            _logger.LogError("xAI content moderation rejection (400 Bad Request)");
            throw new AiContentModerationException();
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogError("xAI balance exhausted (429 Too Many Requests)");
            throw new AiBudgetExhaustedException();
        }
    }
}
