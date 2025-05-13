using LawProject.DTO;
using Newtonsoft.Json;
using System.Text.Json;

namespace LawProject.Service.ICCJ
{
  public class IccjService : IIccjService
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<IccjService> _logger;

    public IccjService(HttpClient httpClient, ILogger<IccjService> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    // Căutare dosar folosind numărul dosarului
    // Căutare dosar folosind numărul dosarului
    public async Task<List<AllFilesDto>> CautareDosareAsync(string nrDosar, string? obiectDosar = null, string? numeParte = null, DateTime? dataStart = null, DateTime? dataEnd = null)
    {
      if (string.IsNullOrWhiteSpace(nrDosar))
        throw new ArgumentException("Numărul dosarului este obligatoriu.");

      var url = $"https://www.scj.ro/api/api/CautareDosare?nr={Uri.EscapeDataString(nrDosar)}";

      // Adăugăm parametrii opționali pentru căutare
      if (!string.IsNullOrEmpty(obiectDosar))
        url += $"&obiect={Uri.EscapeDataString(obiectDosar)}";

      if (!string.IsNullOrEmpty(numeParte))
        url += $"&parte={Uri.EscapeDataString(numeParte)}";

      if (dataStart.HasValue)
        url += $"&dataStart={dataStart.Value:yyyy-MM-dd}";

      if (dataEnd.HasValue)
        url += $"&dataEnd={dataEnd.Value:yyyy-MM-dd}";

      try
      {
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
          var json = await response.Content.ReadAsStringAsync();
          var dosare = JsonConvert.DeserializeObject<List<AllFilesDto>>(json);
          return dosare ?? new List<AllFilesDto>();
        }
        else
        {
          _logger.LogWarning("Request failed with status code: " + response.StatusCode);
          return new List<AllFilesDto>();
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred while fetching dosare.");
        return new List<AllFilesDto>();
      }
    }

    // Mapează AllFilesDto în FileDetailsDto
    public FileDetailsDto MapToFileDetailsDto(AllFilesDto dosar)
    {
      return new FileDetailsDto
      {
        Numar = dosar.Numar,
        NumarVechi = dosar.NumarVechi,
        Data = dosar.Data,
        Institutie = dosar.Institutie,
        Moneda = dosar.Moneda,  // Poți ajusta acest câmp dacă e necesar
        ObiectDosar = dosar.ObiectDosar,
        Departament = dosar.Departament,
        CategorieCaz = dosar.CategorieCaz,
        StadiuProcesual = dosar.StadiuProcesual,
        Parti = dosar.Parti?.Select(p => new ParteDTO
        {
          Nume = p.Nume,
          CalitateParte = p.CalitateParte
        }).ToList(),
        Termene = dosar.Termene?.Select(s => new TermeneDTO
        {
     
          Data = s.Data,
          Ora = s.Ora,
          Solutie = s.Solutie,
          SolutieSumar = s.SolutieSumar,
        }).ToList(),
        CaiAtac = dosar.CaiAtac?.Select(c => new CaleAtacDTO
        {
          DataDeclarare = c.DataDeclarare,
          ParteDeclaratoare = c.ParteDeclaratoare,
          TipCaleAtac = c.TipCaleAtac
        }).ToList()
      };
    }
  }

}


