using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.RaportService
{
  public class RaportService : IRaportService
  {
    private readonly ApplicationDbContext _context;

    public RaportService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<int> CreateRaportAsync(RaportCreateDto dto)
    {
      // Obține avocatul
      var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == dto.LawyerId);
      if (lawyer == null)
        throw new ArgumentException($"Avocatul cu ID {dto.LawyerId} nu a fost găsit.");

      // Inițializare date client
      string clientName = null;

      if (dto.ClientId.HasValue && !string.IsNullOrEmpty(dto.ClientType))
      {
        var clientType = dto.ClientType.ToUpper();

        switch (clientType)
        {
          case "PF":
            var clientPF = await _context.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value);
            if (clientPF == null)
              throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId.Value} nu a fost găsit.");

            clientName = $"{clientPF.FirstName} {clientPF.LastName}";
            break;

          case "PJ":
            var clientPJ = await _context.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId.Value);
            if (clientPJ == null)
              throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId.Value} nu a fost găsit.");

            clientName = clientPJ.CompanyName;
            break;

          default:
            throw new ArgumentException($"Tipul clientului este invalid: {dto.ClientType}. Se acceptă doar 'PF' sau 'PJ'.");
        }
      }

      // Creare obiect Raport
      var raport = new Raport
      {
        LawyerId = dto.LawyerId,
        LawyerName = lawyer.LawyerName, 
        DataRaport = DateTime.Now,
        ClientId = dto.ClientId,
        ClientType = dto.ClientType,
        ClientName = clientName,
        FileId = dto.FileId,
        FileNumber = dto.FileNumber,
        OreDeplasare = dto.OreDeplasare,
        OreStudiu = dto.OreStudiu,
        TaskuriLucrate = dto.Taskuri?.Select(t => new RaportTask
        {
          WorkTaskId = t.WorkTaskId,
          OreLucrate = t.OreLucrate
        }).ToList()
      };

      _context.Rapoarte.Add(raport);
      await _context.SaveChangesAsync();

      return raport.Id;
    }


    public async Task<List<Raport>> GetAllRapoarteAsync()
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .ToListAsync();
    }

    public async Task<Raport?> GetRaportByIdAsync(int id)
    {
      return await _context.Rapoarte
          .Include(r => r.TaskuriLucrate)
          .ThenInclude(rt => rt.WorkTask)
          .FirstOrDefaultAsync(r => r.Id == id);
    }
  }
}

