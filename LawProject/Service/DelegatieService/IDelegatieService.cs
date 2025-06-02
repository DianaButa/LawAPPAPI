using DocumentFormat.OpenXml.Wordprocessing;
using LawProject.DTO;

namespace LawProject.Service.DelegatieService
{
  public interface IDelegatieService
  {
    Task<byte[]> GenerateDocumentAsync(DelegatieDto dto);


  }
}
