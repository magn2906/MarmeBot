using System.Security.Authentication;
using MarmeBot.Models.Database;
using MarmeBot.Utilities;
using MongoDB.Driver;

namespace MarmeBot.Services;

public interface IBannedWordService
{
    Task AddBannedWordAsync(BannedWord word);
    Task RemoveBannedWordAsync(string word, string guildId);
    Task<IEnumerable<BannedWord>> GetBannedWordsByGuildAsync(string guildId);
}

public class BannedWordService : IBannedWordService
{
    private readonly IMongoCollection<BannedWord> _bannedWords;

    public BannedWordService(ILogger<BannedWordService> logger)
    {
        var connectionString = EnvironmentHandler.GetVariable("MongoConnectionString");
        logger.LogInformation("Connection string is empty: {isEmpty}", string.IsNullOrWhiteSpace(connectionString));

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12 // Ensure TLS 1.2 is used
        };

        var client = new MongoClient(settings);
        var database = client.GetDatabase("marme");
        _bannedWords = database.GetCollection<BannedWord>("BannedWords");
    }

    public async Task AddBannedWordAsync(BannedWord word)
    {
        await _bannedWords.InsertOneAsync(word);
    }

    public async Task RemoveBannedWordAsync(string word, string guildId)
    {
        var filter = Builders<BannedWord>.Filter.And(
            Builders<BannedWord>.Filter.Eq(w => w.GuildId, guildId),
            Builders<BannedWord>.Filter.Eq(w => w.Word, word));
        await _bannedWords.DeleteOneAsync(filter);
    }

    public async Task<IEnumerable<BannedWord>> GetBannedWordsByGuildAsync(string guildId)
    {
        var filter = Builders<BannedWord>.Filter.Eq(w => w.GuildId, guildId);
        var bannedWords = await _bannedWords.Find(filter).ToListAsync();
        return bannedWords;
    }
}