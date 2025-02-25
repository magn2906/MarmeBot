using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MarmeBot.Models.Database;
using MarmeBot.Services;

namespace MarmeBot.Handlers.SlashCommands;

public class BanWord : ApplicationCommandModule
{
    private readonly IBannedWordService _bannedWordService;
    
    public BanWord(IBannedWordService bannedWordService)
    {
        _bannedWordService = bannedWordService;
    }
    
    [SlashCommand("ban-word", "Bans a word from being said in the server")]
    public async Task BanWordCommand(InteractionContext ctx, [Option("word", "Word to ban")]string word)
    {
        var wordToBan = new BannedWord
        {
            Word = word
        };
        await _bannedWordService.AddBannedWordAsync(wordToBan);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder
        {
            Content = $"Banned word \"{word}\""
        });
    }
}