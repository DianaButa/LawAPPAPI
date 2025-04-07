using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;  
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.Lawyer
{
  public class LawyerService : ILawyerService
  {
    private readonly ApplicationDbContext _context;

    public LawyerService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<LawyerDto>> GetAllLawyersAsync()
    {
      var lawyers = await _context.Lawyers
          .Select(l => new LawyerDto
          {
            Id = l.Id,
            LawyerName = l.LawyerName,
            Color = l.Color 
          })
          .ToListAsync();

      return lawyers;
    }


    public async Task<LawyerDto> AddLawyerAsync(LawyerDto lawyerDto)
    {
      // Validăm culoarea avocatului (dacă vrei să o validezi ca fiind un cod de culoare valid)
      if (string.IsNullOrEmpty(lawyerDto.Color))
      {
        throw new ArgumentException("Culoarea avocatului este obligatorie.");
      }

      var lawyer = new Models.Lawyer
      {
        LawyerName = lawyerDto.LawyerName,
        Color = lawyerDto.Color
      };

      _context.Lawyers.Add(lawyer);
      await _context.SaveChangesAsync();

      lawyerDto.Id = lawyer.Id;
      return lawyerDto;
    }

  }
}
