using LesApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using transfertService.Models;

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

        public async Task<User> AddUserAsync(User user)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = "/USER-SERVICE/users/admin/add-client";
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";

                // Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                // Convertir l'objet utilisateur en chaîne JSON
                string userJson = JsonConvert.SerializeObject(user);

                // Créer le contenu de la requête avec le corps JSON
                HttpContent content = new StringContent(userJson, Encoding.UTF8, "application/json");

                // Envoyer la requête POST
                HttpResponseMessage response = await client.PostAsync(apiUrl + endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<User>(result); // Convertir la chaîne JSON en objet User
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }
        public List<Beneficaire> GetUserBeneficiaire(string userId)
        {
            List<Beneficaire> beneficiaires = new List<Beneficaire>();
            var user = _user.Find(u => u.Id == userId).FirstOrDefault();

            if (user != null && user.beneficiaires != null)
            {
                foreach (var beneficiaireId in user.beneficiaires)
                {
                    beneficiaires.Add(_beneficiaire.GetBeneficiaireById(beneficiaireId));
                }
            }

            return beneficiaires;
        }

        public User GetUserById(string userId)
        {
            Console.WriteLine("****************************************");
            Console.WriteLine("BEFORE TEST");
            Console.WriteLine("************************************");
            if (userId == null || !IsValidObjectIdFormat(userId))
            {
                Console.WriteLine("****************************************");
                Console.WriteLine("user id is null");
                Console.WriteLine("****************************************");

                return null;
            }
            Console.WriteLine("****************************************");
            Console.WriteLine("****************************************");

            return _user.Find(u => u.Id == userId).FirstOrDefault();
        }




        public async Task<User> GetUserByGSM(string phone)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app";
                string endpoint = "/USER-SERVICE/users/agent/get-by-phone/" + phone;
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                HttpResponseMessage response = await client.GetAsync(apiUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<User>(result);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
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

        public async Task<User> GetUserByIdentityAsync(string Nidentity)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = "/USER-SERVICE/users/agent/get-by-piece-identity/" + Nidentity;
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";
                Console.WriteLine(accessToken);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                HttpResponseMessage response = await client.GetAsync(apiUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<User>(result); // Convertir la chaîne JSON en objet User
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }


        public List<User> GetAllUsers(HttpRequest request)
        {

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = "/USER-SERVICE/users/admin/allClients";
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";
                Console.WriteLine(accessToken);

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                HttpResponseMessage response = client.GetAsync(apiUrl + endpoint).Result;  // Utilisation de .Result pour obtenir le résultat synchronement.

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<List<User>>(result); // Convertir la chaîne JSON en liste d'objets User
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null; // Retournez null ou une liste vide, selon votre gestion d'erreur
                }
            }
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

        public async Task<List<Beneficaire>> GetBeneficiaireAsync(string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = "/USER-SERVICE/users/agent/get-allbeneficiaire-of-client/" + username;
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";
                Console.WriteLine(accessToken);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                HttpResponseMessage response = await client.GetAsync(apiUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<List<Beneficaire>>(result); // Convertir la chaîne JSON en liste d'objets Beneficiaire
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }

        public async Task<User> EditUserAsync(User user, string username)
        {
            try
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = $"/USER-SERVICE/users/update-info/{username}";
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";

                using (HttpClient client = new HttpClient())
                {
                    // Ajouter l'en-tête d'autorisation
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                    // Convertir l'objet utilisateur en chaîne JSON
                    string userJson = JsonConvert.SerializeObject(user);
                    Console.WriteLine(userJson);

                    // Créer le contenu de la requête avec le corps JSON
                    HttpContent content = new StringContent(userJson, Encoding.UTF8, "application/json");

                    // Envoyer la requête PUT
                    HttpResponseMessage response = await client.PostAsync(apiUrl + endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result);
                        return JsonConvert.DeserializeObject<User>(result); // Convertir la chaîne JSON en objet User
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

    }


}


