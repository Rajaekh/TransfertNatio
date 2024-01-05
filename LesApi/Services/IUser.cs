using LesApi.Models;

namespace LesApi.Services
{
    public interface IUser
    {
        User GetUserByGSM(string GSM);
        List<User> GetAllUser();
        User GetUserById(string IdUser);
        User AddUser(User user);
        public DateTime GetDatePremierTransfert(string idClient);
        List<Beneficiaire> GetUserBeneficiaire(string IdUser);
        User EditUser(User user);
        User GetUserByIdentity(string Nidentity);
        User deleteUser(string id);
    }
}
