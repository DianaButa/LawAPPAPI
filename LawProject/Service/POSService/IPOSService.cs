using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.POSService
{
  public interface IPOSService

  {
    Task<POS> GetPOSByNumarIncasareAsync(string numarIncasare);

    Task<POSResponseDto> GenereazaIncasareAsync(POSCreateDto dto);

    Task<IEnumerable<POSResponseDto>> GetAllPOSsAsync();


  }
}
