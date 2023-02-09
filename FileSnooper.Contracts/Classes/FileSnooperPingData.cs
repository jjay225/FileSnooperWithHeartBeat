using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace FileSnooper.Contracts.Classes
{
    public class FileSnooperPingData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string Identifier { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime TimeSent { get; set; }
    }
}
