using LawProject.DTO;

namespace LawProject.Service.ContractService
{
  public interface IContractService
  {
    Task<byte[]> GenerateDocumentsAsync(GenerateContractDto dto);
  }
}
