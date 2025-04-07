using LawProject.Database;
using LawProject.Models;
using LawProject.Service.EmailService;
using LawProject.Service.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.FileService
{
  public class FileToCalendarService
  {
 
    private readonly ILogger<FileToCalendarService> _logger;
    private readonly FileManagementService _fileManagementService;
    private readonly MyQueryService _queryService;
    
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public FileToCalendarService(
    
        ILogger<FileToCalendarService> logger,
        FileManagementService fileManagementService,
        MyQueryService queryService,
        IEmailService emailService,
        ApplicationDbContext context,
        IConfiguration configuration,
        INotificationService notificationService)
    {
     
      _logger = logger;
      _fileManagementService = fileManagementService;
      _queryService = queryService;
      _emailService = emailService;
      _context = context;
      _configuration = configuration;
      _notificationService = notificationService;
    }




    // Procesarea unui singur dosar (pentru verificare imediată la adăugare)
    public async Task ProcessSingleFileAsync(string fileNumber)
    {
      var dosarDetails = await _queryService.CautareDosareAsync(fileNumber);

      if (dosarDetails == null || !dosarDetails.Any())
      {
        _logger.LogWarning($"No case details found for file number: {fileNumber}");
        return;
      }

      await ProcessDosarAsync(dosarDetails);
    }

    // Procesarea tuturor dosarelor din baza de date (pentru jobul periodic)
    public async Task ProcessAllFilesAsync()
    {
      var files = await _fileManagementService.GetAllFilesAsync();

      if (files == null || !files.Any())
      {
        _logger.LogWarning("No files found in the database.");
        return;
      }

      foreach (var file in files)
      {
        _logger.LogInformation($"Processing file number: {file.Numar}");

        var dosarDetails = await _queryService.CautareDosareAsync(file.Numar);
        if (dosarDetails != null && dosarDetails.Any())
        {
          await ProcessDosarAsync(dosarDetails);
        }
        else
        {
          _logger.LogWarning($"No case details found for file number: {file.Numar}");
        }
      }
    }

    // Procesarea ședințelor și adăugarea lor în calendar
    public async Task ProcessDosarAsync(dynamic dosarDetails)
    {
      foreach (var dosar in dosarDetails)
      {
        _logger.LogInformation($"Processing case: {dosar.numar}");

        // Verificăm dacă dosarul există în baza de date locală
        var dosarDb = await _fileManagementService.GetFileByNumberAsync(dosar.numar);
        if (dosarDb == null)
        {
          _logger.LogWarning($"File number {dosar.numar} not found in local database.");
          continue;
        }


        // Obținem avocatul asociat dosarului și culoarea acestuia
        // Asigură-te că `LawyerId` este de un tip statetic
        int lawyerId = Convert.ToInt32(dosarDb.LawyerId);

        var lawyer = await _context.Lawyers
                                    .Where(l => l.Id == lawyerId)
                                    .FirstOrDefaultAsync();


        string color = lawyer?.Color ?? "#E0E0E0"; // Dacă avocatul nu este găsit, folosim o culoare implicită



        var sedinteList = dosar.sedinte as IEnumerable<dynamic>;
        if (sedinteList == null || !sedinteList.Any())
        {
          _logger.LogWarning($"No hearings found for case: {dosar.numar}");
          continue;
        }

        var upcomingSedinte = sedinteList
            .Where(sedinta => sedinta.data != null &&
                              (sedinta.data is string ? DateTime.Parse(sedinta.data) : (DateTime)sedinta.data) >= DateTime.Today)
            .ToList();

        if (!upcomingSedinte.Any())
        {
          _logger.LogInformation($"No upcoming hearings for case number: {dosar.numar}");
          continue;
        }

        bool hasChanges = false;
        var changes = new List<string>();

        foreach (var sedinta in upcomingSedinte)
        {
          if (!(sedinta.ora is string ora) || string.IsNullOrEmpty(ora))
          {
            _logger.LogWarning($"Invalid hearing time for case {dosar.numar}");
            continue;
          }

          DateTime startTime = sedinta.data is string ? DateTime.Parse(sedinta.data) : (DateTime)sedinta.data;
          TimeSpan timeSpan = TimeSpan.Parse(ora);
          startTime = startTime.Add(timeSpan);
          DateTime endTime = startTime.AddHours(1);  // Setăm durata ședinței la o oră

          // Verificăm dacă ședința există deja în baza de date
          var existingEvent = await _fileManagementService.GetScheduledEventAsync(dosar.numar, startTime);
          if (existingEvent != null)
          {
            // Verificăm dacă există modificări ale ședinței
            if (existingEvent.Description != $"Complet: Ora: {ora}, {sedinta.complet},  Instituție: {dosar.institutie}")
            {
              _logger.LogInformation($"Hearing details changed for case {dosar.numar} on {startTime}");

              existingEvent.Description = $"Complet: Ora: {ora}, {sedinta.complet},  Instituție: {dosar.institutie}";
              await _fileManagementService.UpdateScheduledEventAsync(existingEvent);

              changes.Add($"Updated hearing for case {dosar.numar} on {startTime}");
              hasChanges = true;
            }
          }
          else
          {
            // Creăm un eveniment nou
            var scheduledEvent = new ScheduledEvent
            {
              FileNumber = dosar.numar,
              StartTime = startTime,
              TipDosar = dosarDb.TipDosar.ToLower(),
              ClientName = dosarDb.ClientName,
              Description = $"Complet: Ora: {ora}, {sedinta.complet},  Instituție: {dosar.institutie}",
              Color = color
            };

            await _fileManagementService.AddScheduledEventAsync(scheduledEvent);
            _logger.LogInformation($"Sedinta noua adaugata pentru dosarul {dosar.numar} on {startTime}");

            changes.Add($"Sedinta noua adaugata pentru dosarul {dosar.numar} on {startTime}");
            hasChanges = true;
          }
        }

        // Dacă există modificări, trimitem notificări
        if (hasChanges)
        {
          string subject = $"Modificări detectate pentru dosarul {dosar.numar}";
          string message = string.Join("<br>", changes);

          // Trimitere notificare email
          var email = "diana.c.farcas@gmail.com";  // Adresa poate fi preluată din baza de date a dosarului dacă este necesar
          var name = "Cases App";  // Poți adăuga numele utilizatorului sau alt detaliu specific
          await _emailService.SendNotificatonEmail(email, name, dosar.numar.ToString());
          _logger.LogInformation($"Notification email sent for case {dosar.numar}");

          // Create in-app notification
          var notification = new Notification
          {
            Title = $"Modificări pentru dosarul {dosar.numar}",
            Message = $"S-au detectat modificări pentru dosarul {dosar.numar}",
            Timestamp = DateTime.UtcNow,
            Type = "hearing_changes",
            FileNumber = dosar.numar,
            IsRead = false,
            Details = string.Join("\n", changes),
            UserId = 1 // You might want to get this from the current user context
          };

          await _notificationService.CreateNotificationAsync(notification);
          _logger.LogInformation($"In-app notification created for case {dosar.numar}");
        }
      }
    }


    public async Task ProcessAllLocalFilesAsync()
    {
      // Obține toate dosarele din baza de date locală
      var files = await _fileManagementService.GetAllFilesAsync();

      if (files == null || !files.Any())
      {
        _logger.LogWarning("No files found in the database.");
        return;
      }

      // Parcurge fiecare dosar și verifică ședințele
      foreach (var file in files)
      {
        _logger.LogInformation($"Processing file number: {file.Numar}");

        // Verificăm ședințele folosind QueryService (sau alt serviciu care preia ședințele)
        var dosarDetails = await _queryService.CautareDosareAsync(file.Numar);
        if (dosarDetails != null && dosarDetails.Any())
        {
          // Procesăm ședințele și le adăugăm în baza de date
          await ProcessDosarAsync(dosarDetails);
        }
        else
        {
          _logger.LogWarning($"No case details found for file number: {file.Numar}");
        }
      }
    }
  }
}

