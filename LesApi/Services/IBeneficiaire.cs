using LesApi.Models;

namespace LesApi.Services
{
    public interface IBeneficiaire
    {
        Beneficaire GetBeneficiaireById(string IdBeneficiaire);
        Beneficaire AddBeneficiaire(Beneficaire beneficiaire);
        Beneficaire GetBeneficiaireByGSM(string gsm);
        Task<Beneficaire> AddBeneficiaireAsync(Beneficaire beneficiaire,string username);
        Task<List<Beneficaire>> GetBeneficiairesByPhoneAndUsernameAsync(String phone, String username);

    }
}
