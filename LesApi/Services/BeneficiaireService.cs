using LesApi.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Text;

namespace LesApi.Services
{
    public class BeneficiaireService : IBeneficiaire
    {
        private IMongoCollection<Beneficaire> _beneficiaire;

        public BeneficiaireService(ITransfertDatabaseSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _beneficiaire = database.GetCollection<Beneficaire>(settings.BeneficiaireCollectionName);
        }

        public Beneficaire AddBeneficiaire(Beneficaire beneficiaire)
        {
            _beneficiaire.InsertOne(beneficiaire);
            return beneficiaire;
        }

        public Beneficaire GetBeneficiaireByGSM(string gsm)
        {
            return _beneficiaire.Find(Beneficiaire => Beneficiaire.numeroGsm == gsm).FirstOrDefault();
        }

        public Beneficaire GetBeneficiaireById(string IdBeneficiaire)
        {
            if (IdBeneficiaire.Length != 24)
            {
                // Gérer le cas où la chaîne n'a pas la longueur attendue
                return null; // Ou une autre valeur par défaut, selon votre logique
            }

            return _beneficiaire.Find(Beneficiaire => Beneficiaire.id == IdBeneficiaire).FirstOrDefault();
        }

        public async Task<Beneficaire> AddBeneficiaireAsync(Beneficaire beneficiaire,string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app"; // gateway 8080
                string endpoint = $"/USER-SERVICE/users/addBeneficiaire-to-client/{username}";
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";

                // Ajouter l'en-tête d'autorisation
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                // Convertir l'objet utilisateur en chaîne JSON
                string userJson = JsonConvert.SerializeObject(beneficiaire);

                // Créer le contenu de la requête avec le corps JSON
                HttpContent content = new StringContent(userJson, Encoding.UTF8, "application/json");

                // Envoyer la requête POST
                HttpResponseMessage response = await client.PostAsync(apiUrl + endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<Beneficaire>(result); // Convertir la chaîne JSON en objet User
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }



        public async Task<List<Beneficaire>> GetBeneficiairesByPhoneAndUsernameAsync(string phone, string username)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://1b18-105-158-110-135.ngrok-free.app";
                string endpoint = $"/USER-SERVICE/beneficiaires/beneficiaires/getByPhoneAndUsername/{phone}/{username}"; // Ajout du paramètre username à la chaîne de requête
                string accessToken = "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJhZG1pbkBhZG1pbi5jb20iLCJpYXQiOjE3MDQ1ODAwODEsImV4cCI6NjE2NjYwOTU2MDB9.n5GBa1iHS9qeL9co8GFhLV2Zq8q3e2m2T5QSt_Y5P2E";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Substring(7));

                HttpResponseMessage response = await client.GetAsync(apiUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return JsonConvert.DeserializeObject<List<Beneficaire>>(result);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }
            }
        }



    }
}
