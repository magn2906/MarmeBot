using System.Reflection;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using MarmeBot.Handlers.Events;
using MarmeBot.Services;
using MarmeBot.Utilities;

var discord = new DiscordClient(new DiscordConfiguration
{
    Token = EnvironmentHandler.GetVariable("BotToken"),
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.All
});

var services = new ServiceCollection();

services.AddLogging(configure =>
{
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Information); 
});

services.AddSingleton<IBannedWordService, BannedWordService>();
services.AddSingleton<MessagedCreatedEventHandler>();

#pragma warning disable ASP0000
var builtServices = services.BuildServiceProvider();
#pragma warning restore ASP0000

// Event handlers
var messagedCreatedEventHandler = builtServices.GetRequiredService<MessagedCreatedEventHandler>();
discord.MessageCreated += messagedCreatedEventHandler.OnMessageCreated;

// Slash commands
var slash = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = builtServices
});

slash.RegisterCommands(Assembly.GetExecutingAssembly());

var registeredCommands = slash.RegisteredCommands;

foreach (var registeredCommand in registeredCommands)
{
    Console.WriteLine(registeredCommand.Key);
    Console.WriteLine(registeredCommand.Value);
    foreach (var slashCommand in registeredCommand.Value)
    {
        Console.WriteLine(slashCommand.Name);
        Console.WriteLine(slashCommand.Description);
    }
}

await discord.ConnectAsync();
await Task.Delay(-1);