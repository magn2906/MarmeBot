using System.Buffers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MarmeBot.Services;
using MarmeBot.Models.Database;

namespace MarmeBot.Handlers.Events;

public class MessageCreatedEventHandler
{
    private readonly IBannedWordService _bannedWordService;

    public MessageCreatedEventHandler(IBannedWordService bannedWordService) =>
        _bannedWordService = bannedWordService;

    public async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
        {
            return;
        }

        var messageContent = e.Message.Content;

        var bannedWords = await _bannedWordService.GetBannedWordsByGuildAsync(e.Guild.Id.ToString());

        var isBannedWordContained = CheckForBannedWords(messageContent, bannedWords, out var bannedWord);

        if (isBannedWordContained)
        {
            try
            {
                await e.Message.DeleteAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            var sanitizedBanned = string.Concat(bannedWord.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
            var pattern = $@"\b{string.Join(@"\s*", Regex.Escape(sanitizedBanned).ToCharArray())}\b";

            var encapsulatedMessage = Regex.Replace(
                messageContent,
                pattern,
                $"||{bannedWord}||",
                RegexOptions.IgnoreCase
            );

            await e.Message.RespondAsync($"{e.Author.Username}: \"{encapsulatedMessage}\" contains a banned word");
        }
    }

    private static bool CheckForBannedWords(string messageContent,
        IEnumerable<BannedWord> bannedWords, out string bannedWord)
    {
        var messageSpan = messageContent.AsSpan();

        // Remove whitespaces and convert message content to uppercase
        var contentBuffer = ArrayPool<char>.Shared.Rent(messageSpan.Length);
        var sanitizedContentLength = 0;
        foreach (var c in messageSpan)
        {
            if (!char.IsWhiteSpace(c))
            {
                contentBuffer[sanitizedContentLength++] = char.ToUpperInvariant(c);
            }
        }

        var sanitizedContentSpan = new ReadOnlySpan<char>(contentBuffer, 0, sanitizedContentLength);

        // Process each banned word
        foreach (var bw in bannedWords)
        {
            var sanitizedBw = string.Concat(bw.Word.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
            if (sanitizedContentSpan.IndexOf(sanitizedBw.AsSpan()) < 0)
            {
                continue;
            }

            bannedWord = bw.Word; // return original banned word, as written in database
            ArrayPool<char>.Shared.Return(contentBuffer);
            return true;
        }

        ArrayPool<char>.Shared.Return(contentBuffer);
        bannedWord = string.Empty;
        return false;
    }
}