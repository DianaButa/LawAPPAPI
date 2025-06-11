using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.AccountService;
using LawProject.Service.ClientService;
using LawProject.Service.DailyEventService;
using LawProject.Service.FileService;
using LawProject.Service.InvoiceSerices;
using LawProject.Service.RaportService;
using LawProject.Service.TaskService;
using Microsoft.EntityFrameworkCore;


namespace LawProject.Service.CheltuieliService
{
  public class CheltuieliService :ICheltuieliService
  {

    private readonly ApplicationDbContext _context;
    private readonly ILogger<CheltuieliService> _logger;
    private readonly IAccountService _acountService;

    public CheltuieliService(ApplicationDbContext context, ILogger<CheltuieliService> logger, IAccountService accountService)
    {
      _context = context;
      _logger = logger;
      _acountService = accountService;

    }

    public async Task AddCheltuieliAsync(CheltuieliDto dto)
    {
      // Obține utilizatorul pe baza ID-ului
      var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == dto.UserId);

      if (user == null)
      {
        _logger.LogWarning($"Userul cu ID {dto.UserId} nu a fost găsit.");
        throw new ArgumentException($"Userul cu ID {dto.UserId} nu a fost găsit.");
      }

      var cheltuieli = new Cheltuieli
      {
        UserId = user.Id,           
        UserName = user.UserName,  

        Categorie = dto.Categorie,
        Titlu = dto.Titlu,
        Data = dto.Data,             
        Valoare = dto.Valoare,
        Moneda = dto.Moneda,
        Descriere = dto.Descriere
      };

      _context.Cheltuieli.Add(cheltuieli);
      await _context.SaveChangesAsync();
    }


    public async Task<IEnumerable<CheltuieliDto>> GetAllCheltuieliAsync()
    {
      var cheltuieli = await _context.Cheltuieli.ToListAsync();
      return cheltuieli.Select(cheltuieli => new CheltuieliDto
      {
        Id = cheltuieli.Id,
        Titlu=cheltuieli.Titlu,
        Data = cheltuieli.Data,
        Descriere=cheltuieli.Descriere,
        UserName=cheltuieli.UserName,
        UserId=cheltuieli.UserId,
        Categorie=cheltuieli.Categorie,
        Valoare= cheltuieli.Valoare,
        Moneda=cheltuieli.Moneda,
   
      });
    }
  }
}
