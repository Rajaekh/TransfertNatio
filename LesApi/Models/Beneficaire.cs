using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LesApi.Models
{
    public class Beneficaire
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }

        [BsonElement("nom")]
        public string nom { get; set; }

        [BsonElement("prenom")]
        public string prenom { get; set; }

        [BsonElement("numeroGsm")]
        public string numeroGsm { get; set; }

        [BsonElement("pieceIdentity")]
        public string pieceIdentity { get; set; }

            [BsonElement("_class")]
        public string? _class { get; set; }
    }
}
