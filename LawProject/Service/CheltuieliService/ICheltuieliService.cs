using LawProject.DTO;

namespace LawProject.Service.CheltuieliService
{
  public interface ICheltuieliService
  {
    Task AddCheltuieliAsync(CheltuieliDto dto);
    Task<IEnumerable<CheltuieliDto>> GetAllCheltuieliAsync();
  }
}
