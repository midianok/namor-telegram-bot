using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Saturn.Bot.Service.Services;
using Saturn.Bot.Service.Services.Abstractions;
using Saturn.Telegram.Db.Repositories;
using Saturn.Telegram.Db.Repositories.Abstractions;
using Saturn.Telegram.Lib;
using Saturn.Telegram.Lib.Infrastructure;
using Saturn.Telegram.Lib.Infrastructure.Abstractions;
using Telegram.Bot;

namespace Saturn.Bot.Service.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection, ConfigurationManager configuration)
    {
        serviceCollection.AddSingleton<TelegramBotClient>(_ =>
        {
            var botToken = configuration.GetSectionOrThrow("BOT_TOKEN");
            return new TelegramBotClient(botToken);
        });

        serviceCollection.AddSingleton<ChatClient>(_ =>
        {
            var apiKey = configuration.GetSectionOrThrow("ATLAS_CLOUD_API_KEY");
            return new ChatClient("qwen/qwen3-vl-8b-instruct", new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri("https://api.atlascloud.com/v1") });
        });

        serviceCollection
            .AddSingleton<IAiService, AiService>()
            .AddSingleton<IChatCachedRepository, ChatCachedRepository>()
            .AddSingleton<IMessageRepository, MessageRepository>()
            .AddSingleton<IOperationCallRepository, OperationCallRepository>()
            .AddSingleton<ISaveMessageService, SaveMessageService>()
            .AddSingleton<OperationManager>()
            .AddHostedService<CacheInvalidationService>()
            .AddMemoryCache();

        return serviceCollection;
    }
}
