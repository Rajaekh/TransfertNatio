using LesApi.Models;
using LesApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficairesController : ControllerBase
    {
        private readonly IBeneficiaire _beneficiaire;
        public BeneficairesController(IBeneficiaire beneficiaire)
        {
            _beneficiaire = beneficiaire;

        }
        [HttpGet("{gsm}")]
        public ActionResult<Beneficaire> Get(string gsm)
        {

            var beneficiaire = _beneficiaire.GetBeneficiaireByGSM(gsm);
            if (beneficiaire == null)
            {
                return NotFound($"Client with GSM={gsm} not found");
            }
            return Ok(beneficiaire);
        }



        [HttpPost("{username}")]

        public async Task<ActionResult<Beneficaire>> Post([FromBody] Beneficaire beneficiaire, string username)

        {
            // Vérifier si le numéro de téléphone est déjà utilisé
            var existingBeneficiaire = _beneficiaire.GetBeneficiaireByGSM(beneficiaire.numeroGsm);
            if (existingBeneficiaire != null)
            {
                // Le numéro de téléphone est déjà utilisé, renvoyer une réponse d'erreur
                return Conflict("Le numéro de téléphone doit être unique.");
            }

            // Appeler la méthode asynchrone pour ajouter le bénéficiaire à distance
            // Remplacez par le vrai nom d'utilisateur
            var addedBeneficiaire = await _beneficiaire.AddBeneficiaireAsync(beneficiaire, username);

            if (addedBeneficiaire != null)
            {
                // La requête distante a réussi, vous pouvez traiter la réponse ou simplement la renvoyer
                return Ok(addedBeneficiaire);
            }
            else
            {
                // Il y a eu une erreur lors de la requête distante, vous pouvez renvoyer une réponse d'erreur appropriée
                return StatusCode(500, "Une erreur s'est produite lors de l'ajout du bénéficiaire à distance.");
            }
        }
        [HttpGet("{phone}/{username}")]
        public async Task<ActionResult<List<Beneficaire>>> GetBeneficiairesByPhoneAndUsername(string phone, string username)
        {
            Console.WriteLine("***************************************");
            Console.WriteLine("***************************************");
            List<Beneficaire> beneficiaires = await _beneficiaire.GetBeneficiairesByPhoneAndUsernameAsync(phone, username);

            return Ok(beneficiaires);
        }


    }

}

