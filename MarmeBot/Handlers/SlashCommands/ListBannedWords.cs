using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MarmeBot.Services;

namespace MarmeBot.Handlers.SlashCommands;

public class ListBannedWords : ApplicationCommandModule
{
    private readonly IBannedWordService _bannedWordService;
    
    public ListBannedWords(IBannedWordService bannedWordService)
    {
        _bannedWordService = bannedWordService;
    }

    [SlashCommand("list-banned-words", "Lists all banned words")]
    public async Task ListBannedWordsCommand(InteractionContext ctx)
    {
        var bannedWords = _bannedWordService.GetBannedWordsByGuildAsync(ctx.Guild.Id.ToString());
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder
        {
            Content = "Banned words: \n" + string.Join("\n", bannedWords)
        });
    }
}