using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LesApi.Models;
using transfertService.Models;

namespace LesApi.Services
{
    public interface IUser
    {
        Task<User> GetUserByGSM(string GSM);
        List<User> GetAllUsers(HttpRequest request);

        User GetUserById(string IdUser);
        Task<User> AddUserAsync(User user);

        DateTime GetDatePremierTransfert(string idClient);
        Task<List<Beneficaire>> GetBeneficiaireAsync(string username);

        User EditUser(User user);
        Task<User> EditUserAsync(User user, string username);
        Task<User> GetUserByIdentityAsync(string numeroPieceIdentite);

        User deleteUser(string id);
    }
}
