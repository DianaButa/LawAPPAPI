using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.RaportService
{
  public interface IRaportService
  {
    Task<int> CreateRaportAsync(RaportCreateDto dto);
    Task<List<Raport>> GetAllRapoarteAsync();
    Task<Raport?> GetRaportByIdAsync(int id);

    Task<List<RaportGeneralDto>> GetRapoarteGeneraleAsync(); 
  }
}
