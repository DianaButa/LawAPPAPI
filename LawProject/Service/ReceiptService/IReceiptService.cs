using LawProject.DTO;
using LawProject.Models;

namespace LawProject.Service.ReceiptService
{
  public interface IReceiptService
  {
    Task<ReceiptResponseDto> GenereazaChitantaAsync(ReceiptCreateDto dto);


    Task<Receipt> GetReceiptByNumarChitantaAsync(string numarChitanta);

    Task<IEnumerable<ReceiptResponseDto>> GetAllReceiptsAsync();
  }
}
