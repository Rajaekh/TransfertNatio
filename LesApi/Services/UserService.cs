using LesApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace LesApi.Services
{
    public class UserService : IUser
    {
        private readonly IBeneficiaire _beneficiaire;
        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<Transfert> _transfert;
        public UserService(ITransfertDatabaseSettings settings, IMongoClient mongoClient, IBeneficiaire beneficiaire)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _beneficiaire = beneficiaire;
            _user = database.GetCollection<User>(settings.UsersCollectionName);
            _transfert = database.GetCollection<Transfert>(settings.TransfertCollectionName);
        }

        public User AddUser(User user)
        {
            _user.InsertOne(user);
            return user;
        }

        public List<Beneficiaire> GetUserBeneficiaire(string userId)
        {
            List<Beneficiaire> beneficiaires = new List<Beneficiaire>();
            var user = _user.Find(u => u.Id == userId).FirstOrDefault();

            if (user != null && user.Beneficiaires!= null)
            {
                foreach (var beneficiaireId in user.Beneficiaires)
                {
                    beneficiaires.Add(_beneficiaire.GetBeneficiaireById(beneficiaireId));
                }
            }

            return beneficiaires;
        }

        public User GetUserById(string userId)
        {
            if (userId== null || !IsValidObjectIdFormat(userId))
            {
                return null;
            }
            return _user.Find(u => u.Id == userId).FirstOrDefault();
        }

        public User GetUserByGSM(string gsm)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Gsm, gsm);
            return _user.Find(filter).FirstOrDefault();
        }


        public User EditUser(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var result = _user.ReplaceOne(filter, user);

            if (result.ModifiedCount > 0)
            {
                return user;
            }
            else
            {
                return null;
            }
        }
        //pour savoir la date du 1 ere transfert:
        public DateTime GetDatePremierTransfert(string idClient)
        {
            var datePremierTransfert = _transfert
                .Find(t => t.IdClient == idClient && t.DataeTransfert != null)
                .SortBy(t => t.DataeTransfert)
                .Limit(1)
                .Project(t => t.DataeTransfert)
                .FirstOrDefault();

            return datePremierTransfert;
        }

        public User GetUserByIdentity(string Nidentity)
        {
            return _user.Find(u => u.nidentity == Nidentity).FirstOrDefault();
        }

        public List<User> GetAllUser()
        {
            // Utilisez la méthode Find pour obtenir tous les transferts dans la collection
            var users = _user.Find(_ => true);

            // Convertissez les documents en liste de transferts
            List<User> UsersList= users.ToList();

            return UsersList;
        }
        public static bool IsValidObjectIdFormat(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        public User deleteUser(string id)
        {
            if (!IsValidObjectIdFormat(id))
            {
                // Gérer le cas où l'ID n'est pas au bon format ObjectId
                // Vous pouvez lever une exception, enregistrer une erreur, ou autre.
                throw new ArgumentException("Invalid ObjectId format");
            }

            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var userToDelete = _user.FindOneAndDelete(filter);

            if (userToDelete != null)
            {
                // L'utilisateur a été supprimé avec succès
                return userToDelete;
            }
            else
            {
                return null;
            }
        }


    }
}
