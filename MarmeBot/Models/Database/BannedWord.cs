using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarmeBot.Models.Database;

public class BannedWord
{
    [BsonId] 
    public ObjectId  Id { get; set; } = ObjectId.GenerateNewId();
    public required string Word { get; set; }
    public required string GuildId { get; set; }
}