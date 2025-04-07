using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.ClientService
{
  public class ClientService : IClientService
  {
    private readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<ClientPFDto>> GetAllPFAsync()
    {
      var clients = await _context.ClientPFs.ToListAsync();  // Adaptează la contextul tău
      return clients.Select(client => new ClientPFDto
      {
        Id = client.Id,
        FirstName = client.FirstName,
        LastName = client.LastName,
        CNP = client.CNP,
        Email = client.Email,
        Address = client.Address,
        PhoneNumber = client.PhoneNumber
      });
    }

    // Obține toate persoanele juridice
    public async Task<IEnumerable<ClientPJDto>> GetAllPJAsync()
    {
      var clients = await _context.ClientPJs.ToListAsync();  // Adaptează la contextul tău
      return clients.Select(client => new ClientPJDto
      {
        Id = client.Id,
        CompanyName = client.CompanyName,
        CUI = client.CUI,
        Email = client.Email,
        Address = client.Address,
        PhoneNumber = client.PhoneNumber
      });
    }

    // Adăugare client persoană fizică
    public async Task AddClientPF(ClientPFDto clientDto)
    {
      var newClient = new ClientPF
      {
        FirstName = clientDto.FirstName,
        LastName = clientDto.LastName,
        CNP = clientDto.CNP,
        Email = clientDto.Email,
        Address = clientDto.Address,
        PhoneNumber = clientDto.PhoneNumber
      };

      _context.ClientPFs.Add(newClient);  // Adaptează la contextul tău
      await _context.SaveChangesAsync();
    }

    // Adăugare client persoană juridică
    public async Task AddClientPJ(ClientPJDto clientDto)
    {
      var newClient = new ClientPJ
      {
        CompanyName = clientDto.CompanyName,
        CUI = clientDto.CUI,
        Email = clientDto.Email,
        Address = clientDto.Address,
        PhoneNumber = clientDto.PhoneNumber
      };

      _context.ClientPJs.Add(newClient);  // Adaptează la contextul tău
      await _context.SaveChangesAsync();
    }
  }
}
