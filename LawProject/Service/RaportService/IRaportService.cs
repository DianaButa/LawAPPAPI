using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.RaportService
{
  public interface IRaportService
  {
    Task<int> CreateRaportAsync(RaportCreateDto dto);
    Task<List<RaportDto>> GetAllRapoarteAsync();
    Task<Raport?> GetRaportByIdAsync(int id);

    Task<List<RaportGeneralDto>> GetRapoarteGeneraleAsync();

    Task<List<RaportDto>> GetRapoarteByLawyerIdAsync(int lawyerId);

    Task<List<RaportGeneralDto>> GetRapoarteGeneraleByLawyerAsync(int lawyerId);
    //Task<List<Raport>> GetRapoarteByClientAsync(int clientId, string clientType);
    Task<Raport?> GetRaportByFileNumberAsync(string fileNumber);
   
  }
}
