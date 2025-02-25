using System.Security.Authentication;
using MarmeBot.Models.Database;
using MarmeBot.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MarmeBot.Services;

public interface IBannedWordService
{
    Task AddBannedWordAsync(BannedWord word);
    Task RemoveBannedWordAsync(string word);
    IEnumerable<string> GetAllBannedWords();
}

public class BannedWordService : IBannedWordService
{
    private readonly IMongoCollection<BannedWord> _bannedWords;
    private readonly List<string> _bannedWordCache = new();
    private readonly ILogger<BannedWordService> _logger;

    public BannedWordService(ILogger<BannedWordService> logger)
    {
        _logger = logger;

        var connectionString = EnvironmentHandler.GetVariable("MongoConnectionString");
        _logger.LogInformation("Connection string is empty: {isEmpty}", string.IsNullOrWhiteSpace(connectionString));


        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12 // Ensure TLS 1.2 is used
        };

        var client = new MongoClient(settings);
        var database = client.GetDatabase("marme");
        _bannedWords = database.GetCollection<BannedWord>("BannedWords");

        InitializeBannedWordCache().Wait();
    }

    private async Task InitializeBannedWordCache()
    {
        var words = await _bannedWords.Find(new BsonDocument()).ToListAsync();
        _bannedWordCache.AddRange(words.Select(w => w.Word));
    }

    public async Task AddBannedWordAsync(BannedWord word)
    {
        await _bannedWords.InsertOneAsync(word);
        _bannedWordCache.Add(word.Word);
    }

    public async Task RemoveBannedWordAsync(string word)
    {
        var filter = Builders<BannedWord>.Filter.Eq(w => w.Word, word);
        await _bannedWords.DeleteOneAsync(filter);
        _bannedWordCache.Remove(word);
    }

    public IEnumerable<string> GetAllBannedWords()
    {
        return _bannedWordCache.AsReadOnly();
    }
}