using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MarmeBot.Services;

namespace MarmeBot.Handlers.SlashCommands;

public class UnbanWord : ApplicationCommandModule
{
    private readonly IBannedWordService _bannedWordService;
    
    public UnbanWord(IBannedWordService bannedWordService)
    {
        _bannedWordService = bannedWordService;
    }
    
    [SlashCommand("unban-word", "Unbans a word from being said in the server")]
    public async Task UnbanWordCommand(InteractionContext ctx, [Option("word", "Word to unban")]string word)
    {
        await _bannedWordService.RemoveBannedWordAsync(word);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder
        {
            Content = $"Unbanned word \"{word}\""
        });
    }
}