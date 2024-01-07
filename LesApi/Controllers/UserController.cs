using LesApi.Models;
using LesApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Text;
using transfertService.Models;

namespace LesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUser _user;
        public UserController(IUser user)
        {
            _user = user;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            // Appeler la méthode du service
            var request = HttpContext.Request;
            List<User> users = _user.GetAllUsers(request);

            if (users != null)
            {
                // Faites quelque chose avec la liste d'utilisateurs (par exemple, retournez-la dans la réponse HTTP)
                return Ok(users);
            }
            else
            {
                // Gérez le cas où la récupération des utilisateurs a échoué
                return BadRequest("Failed to retrieve users from the external API or database.");
            }
        }

        [HttpGet("{phone}")]
        public async Task<ActionResult<User>> Get(string phone)
        {
            try
            {
                var user = await _user.GetUserByGSM(phone);

                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound(); // Ou BadRequest selon votre logique
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }



        // recuperer user by Identity:
        [HttpGet("UserByIdentity/{numeroPieceIdentite}")]
       
        public async Task<ActionResult<User>> GetUserByIdentity(string numeroPieceIdentite)
        {
            try
            {
                var user = await _user.GetUserByIdentityAsync(numeroPieceIdentite);

                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound(); // Ou BadRequest selon votre logique
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("beneficiaire/{username}")]
        public async Task<ActionResult<List<Beneficiaire>>> GetBeneficiaire(string username)
        {
            try
            {
                var beneficiaires = await _user.GetBeneficiaireAsync(username);

                if (beneficiaires != null)
                {
                    return Ok(beneficiaires);
                }
                else
                {
                    return BadRequest("Failed to retrieve beneficiaires from the external API or database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // POST api/<ClientController>
        [HttpPost]
        public async Task<ActionResult<User>> Post([FromBody] User user)
        {
          
                var addedUser = await _user.AddUserAsync(user);

                if (addedUser != null)
                {
                    return Ok(addedUser);
                }
                else
                {
                    return BadRequest("Failed to add the user through the external API.");
                }
            
        }
        //var existingUser = _user.GetUserById(id);

        //if (existingUser == null)
        //{
        //    return NotFound($"User with ID={id} not found");
        //}
        //_user.EditUser(user);
        //return Ok(_user.GetUserByGSM(user.Gsm));
        [HttpPut("{username}")]
        public async Task<ActionResult<User>> EditUser(string username, [FromBody] User user)
        {
            try
            {
                var editedUser = await _user.EditUserAsync(user,username);

                if (editedUser != null)
                {
                    return Ok(editedUser);
                }
                else
                {
                    return NotFound(); // Ou BadRequest() selon la logique de gestion d'erreur que vous préférez
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error"); // Gestion des exceptions
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<User> DeleteUser(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
               var deletedUser = _user.deleteUser(id);
                return Ok(deletedUser);
            }

        [HttpGet("userbyId/{id}")]
        public ActionResult<User> GetUserById(string id)
        {
            var user = _user.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID={id} not found");
            }
            return Ok(user);
        }



    }
}
