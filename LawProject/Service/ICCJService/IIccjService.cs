using LawProject.DTO;

namespace LawProject.Service.ICCJ
{
  public interface IIccjService
  {

    Task<List<AllFilesDto>> CautareDosareAsync(string nrDosar, string? obiectDosar = null, string? numeParte = null, DateTime? dataStart = null, DateTime? dataEnd = null);
  }


  }

