using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace LesApi.Models
{
    public class User
    {
        [BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("lastname")]
        public string lastname { get; set; }

        [BsonElement("email")]
        public string email { get; set; }

        [BsonElement("password")]
        public string? password { get; set; }

        [BsonElement("role")]
        public string? role { get; set; }

        [BsonElement("titre")]
        public string? titre { get; set; }

        [BsonElement("typePieceIdentity")]
        public string? typePieceIdentity { get; set; }

        [BsonElement("nidentity")]
        public string? nidentity { get; set; }

        [BsonElement("gsm")]
        public string gsm { get; set; }

        [BsonElement("paysEmission")]
        public string? paysEmission { get; set; }

        [BsonElement("numeroPieceIdentite")]
        public string? numeroPieceIdentite { get; set; }

        [BsonElement("dateExpirationPiece")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? dateExpirationPiece { get; set; }

        [BsonElement("validitePieceIdentite")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? validitePieceIdentite { get; set; }

        [BsonElement("dateNaissance")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? dateNaissance { get; set; }

        [BsonElement("datePremierTransfert")]
        public DateTime? datePremierTransfert { get; set; }

        [BsonElement("profession")]
        public string? profession { get; set; }

        [BsonElement("paysNationalite")]
        public string? paysNationalite { get; set; }

        [BsonElement("paysAdresse")]
        public string?paysAdresse { get; set; }

        [BsonElement("adresseLegale")]
        public string? adresseLegale { get; set; }

        [BsonElement("ville")]
        public string? ville { get; set; }

        [BsonElement("estSupprimer")]
        public bool? estSupprimer { get; set; }

        [BsonElement("surListeNoire")]
        public bool? surListeNoire { get; set; }

        [BsonElement("montant")]
        public double? montant { get; set; }

        [BsonElement("montantTransfertAnnuel")]
        public double? montantTransfertAnnuel { get; set; }

        [BsonElement("beneficiaires")]
        public List<string>? beneficiaires { get; set; } = new List<string>();

        [BsonElement("_class")]
        public string? _class { get; set; }

    }
}
