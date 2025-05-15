using LawProject.Models;

namespace LawProject.DTO
{
  public class FisaClientDetaliataDto
  {
    public int ClientId { get; set; }
    public string ClientType { get; set; }
    public string ClientName { get; set; }

    public IEnumerable<MyFile> Files { get; set; }
    public IEnumerable<DailyEventsDto> DailyEvents { get; set; }
    public IEnumerable<WorkTask> ClosedTasks { get; set; }
    public IEnumerable<Raport> Rapoarte { get; set; }



    // Opțional - sumarizări
    public int NrDosare => Files?.Count() ?? 0;
    public double TotalOreDailyEvents => DailyEvents?.Sum(e => double.TryParse(e.AllocatedHours?.ToString(), out var h) ? h : 0) ?? 0;
    public double TotalOreTasks => ClosedTasks?.Sum(t => t.HoursWorked ?? 0) ?? 0;

    public double TotalOreDeplasare =>
        Rapoarte?.Sum(r => r.OreDeplasare) ?? 0.0;

    public double TotalOreStudiu =>
        Rapoarte?.Sum(r => r.OreStudiu) ?? 0.0;


    public double TotalOreTaskuriDinRapoarte =>
        Rapoarte?.SelectMany(r => r.TaskuriLucrate)
                .Sum(t => t.WorkTask?.HoursWorked ?? 0) ?? 0;

    public double TotalOreRaport =>
        TotalOreDeplasare + TotalOreStudiu + TotalOreTaskuriDinRapoarte;
  }
}
