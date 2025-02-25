using System.Buffers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MarmeBot.Services;

namespace MarmeBot.Handlers.Events;

public class MessagedCreatedEventHandler
{
    private readonly List<string> _sanitizedBannedWords;

    public MessagedCreatedEventHandler(IBannedWordService bannedWordService)
    {
        // Initialize and sanitize banned words
        _sanitizedBannedWords = bannedWordService.GetAllBannedWords()
            .Select(w => string.Concat(w.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant())
            .ToList();
    }

    public async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot)
        {
            return;
        }

        var messageContent = e.Message.Content;

        var isBannedWordContained = CheckForBannedWords(messageContent, _sanitizedBannedWords, out var bannedWord);

        if (isBannedWordContained)
        {
            await e.Message.DeleteAsync();

            // Build the regex pattern to match the banned word with optional whitespaces between letters
            var pattern = $@"\b{string.Join(@"\s*", Regex.Escape(bannedWord).ToCharArray())}\b";
            var encapsulatedMessage = Regex.Replace(
                messageContent,
                pattern,
                $"||{bannedWord}||",
                RegexOptions.IgnoreCase
            );

            await e.Message.RespondAsync($"{e.Author.Username}: \"{encapsulatedMessage}\" contains a banned word");
        }
    }

    private static bool CheckForBannedWords(string messageContent, IEnumerable<string> sanitizedBannedWords, out string bannedWord)
    {
        var messageSpan = messageContent.AsSpan();

        // Remove whitespaces and convert to uppercase
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

        foreach (var sanitizedWord in sanitizedBannedWords)
        {
            var sanitizedWordSpan = sanitizedWord.AsSpan();

            if (sanitizedContentSpan.IndexOf(sanitizedWordSpan) < 0)
            {
                continue;
            }
            
            bannedWord = sanitizedWord;

            ArrayPool<char>.Shared.Return(contentBuffer);

            return true;
        }

        ArrayPool<char>.Shared.Return(contentBuffer);

        bannedWord = string.Empty;
        return false;
    }
}
