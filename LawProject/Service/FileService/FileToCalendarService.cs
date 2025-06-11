using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using LawProject.Service.EmailService;
using LawProject.Service.ICCJ;
using LawProject.Service.Notifications;
using Microsoft.EntityFrameworkCore;


namespace LawProject.Service.FileService
{
  public class FileToCalendarService
  {
 
    private readonly ILogger<FileToCalendarService> _logger;
    private readonly IFileManagementService _fileManagementService;
    private readonly MyQueryService _queryService;
  

    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IIccjService _ccjService;

    public FileToCalendarService(
    
        ILogger<FileToCalendarService> logger,
        IFileManagementService fileManagementService,
        MyQueryService queryService,
        IEmailService emailService,
        ApplicationDbContext context,
        IIccjService iccjService,
        IConfiguration configuration,
        INotificationService notificationService)
    {
     
      _logger = logger;
      _fileManagementService = fileManagementService;
      _queryService = queryService;
      _emailService = emailService;
      _context = context;
      _ccjService = iccjService;
      _configuration = configuration;
      _notificationService = notificationService;
    }




    // Procesarea unui singur dosar Just (pentru verificare imediată la adăugare)
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


    // Procesarea unui dosar pentru ICCJ (pentru verificare imediată la adăugare)
    public async Task ProcessSingleFileIccjAsync(string fileNumber)
    {
      var iccjDosarDetailsList = await _ccjService.CautareDosareAsync(fileNumber);

      if (iccjDosarDetailsList == null || !iccjDosarDetailsList.Any())
      {
        _logger.LogWarning($"No case details found in ICCJ for file number: {fileNumber}");
        return;
      }

 
      await ProcessDosarIccjAsync(iccjDosarDetailsList);
    }






    // Procesarea detaliilor unui dosar ICCJ
    // Procesarea ședințelor ICCJ și adăugarea lor în calendar
    public async Task ProcessDosarIccjAsync(dynamic iccjDosarDetailsList)
    {
      foreach (var dosar in iccjDosarDetailsList)
      {
        _logger.LogInformation($"Processing ICCJ case: {dosar.Numar}");

        // Verificăm dacă dosarul există în baza de date locală
        var dosarDb = await _fileManagementService.GetFileByNumberAsync((string)dosar.Numar);
        if (dosarDb == null)
        {
          _logger.LogWarning($"File number {dosar.Numar} not found in local database.");
          continue;
        }

        string color = "#E0E0E0";

        if (dosarDb.LawyerId != null)
        {
          int lawyerId = dosarDb.LawyerId.Value;

          var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == lawyerId);

          // Dacă avocatul există și are o culoare validă, o folosim
          if (lawyer != null && !string.IsNullOrWhiteSpace(lawyer.Color))
          {
            color = lawyer.Color;
            _logger.LogInformation($"Lawyer Color for case {dosar.Numar}: {color}");
          }
          else
          {
            _logger.LogWarning($"Lawyer for case {dosar.Numar} does not have a color set.");
          }
        }

        var termeneList = dosar.Termene as IEnumerable<dynamic>;
        if (termeneList == null || !termeneList.Any())
        {
          _logger.LogWarning($"No hearings found for case: {dosar.Numar}");
          continue;
        }

        // Filtrăm termenele viitoare
        var upcomingTermene = termeneList
      .Where(termen => termen.Data >= DateTime.Today)
      .ToList();


        if (!upcomingTermene.Any())
        {
          _logger.LogInformation($"No upcoming hearings for case number: {dosar.Numar}");
          continue;
        }

        bool hasChanges = false;
        var changes = new List<string>();

        foreach (var termen in upcomingTermene)
        {
          if (!(termen.Ora is string Ora) || string.IsNullOrEmpty(Ora))
          {
            _logger.LogWarning($"Invalid hearing time for ICCJ case {dosar.Numar}");
            continue;
          }

          // Calculăm ora de start și ora de final pentru termen
          DateTime startTime = termen.Data is string ? DateTime.Parse(termen.Data) : (DateTime)termen.Data;
          TimeSpan timeSpan = TimeSpan.Parse(Ora);
          startTime = startTime.Add(timeSpan);
          DateTime endTime = startTime.AddHours(1); // durata default de 1 oră

          // Verificăm dacă există deja un eveniment programat
          var existingEvent = await _fileManagementService.GetScheduledEventAsync((string)dosar.Numar, startTime);
          string description = $" Ora: {Ora}, Sursa: {dosar.Source}";

          if (existingEvent != null)
          {
            if (existingEvent.Description != description)
            {
              _logger.LogInformation($"ICCJ hearing changed for case {dosar.Numar} on {startTime}");

              existingEvent.Description = description;
             
              await _fileManagementService.UpdateScheduledEventAsync(existingEvent);

              changes.Add($"Updated ICCJ hearing for case {dosar.Numar} on {startTime}");
              hasChanges = true;
            }
          }
          else
          {
            var scheduledEvent = new ScheduledEvent
            {
              FileNumber = dosar.Numar,
              StartTime = startTime,
              ClientId = dosarDb.ClientId,
              ClientType = dosarDb.ClientType,
              Source = dosarDb.Source,
              LawyerName=dosarDb.LawyerName,
              TipDosar = dosarDb.TipDosar.ToLower(),
              ClientName = dosarDb.ClientName,
              Description = description,
              Color = color
            };

            await _fileManagementService.AddScheduledEventAsync(scheduledEvent);
            _logger.LogInformation($"New ICCJ hearing added for case {dosar.Numar} on {startTime}");

            changes.Add($"New ICCJ hearing added for case {dosar.Numar} on {startTime}");
            hasChanges = true;
          }
        }

        if (hasChanges)
        {
          string subject = $"Modificări detectate pentru dosarul ICCJ {dosar.Numar}";
          string message = string.Join("<br>", changes);

          var email = "diana.c.farcas@gmail.com";
          var name = "Cases App";
          await _emailService.SendNotificatonEmail(email, name, dosar.Numar.ToString());

          var notification = new Notification
          {
            Title = $"Modificări pentru dosarul ICCJ {dosar.Numar}",
            Message = $"S-au detectat modificări pentru dosarul ICCJ {dosar.Numar}",
            Timestamp = DateTime.UtcNow,
            Type = "hearing_changes",
            FileNumber = dosar.Numar,
            Source=dosarDb.Source,
            IsRead = false,
            Details = string.Join("\n", changes),
            UserId = 1
          };

          await _notificationService.CreateNotificationAsync(notification);
          _logger.LogInformation($"In-app notification created for ICCJ case {dosar.Numar}");
        }
      }
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

        string color = "#E0E0E0";



        if (dosarDb.LawyerId != null)
        {
          int lawyerId = dosarDb.LawyerId;  // Presupunem că LawyerId este un int
          var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == lawyerId);
        
        // Dacă avocatul există și are o culoare validă, o folosim
        if (lawyer != null && !string.IsNullOrWhiteSpace(lawyer.Color))
          {
            color = lawyer.Color;
            _logger.LogInformation($"Lawyer Color for case {dosar.numar}: {color}");
          }
          else
          {
            _logger.LogWarning($"Lawyer for case {dosar.numar} does not have a color set.");
          }
        }


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
              Source=dosarDb.Source,
              ClientName = dosarDb.ClientName,
              ClientId= dosarDb.ClientId,
              LawyerName=dosarDb.LawyerName,
              ClientType= dosarDb.ClientType,
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
          string message = $"Client: {dosarDb.ClientName}<br>Avocat: {dosarDb.LawyerName}<br><br>" +
                      string.Join("<br>", changes);

          // Obținem emailul avocatului din dosarDb
          var email = dosarDb.LawyerEmail;

          string name = "Doseanu-Law";

          if (dosarDb.LawyerId != null)
          {
            int lawyerId = Convert.ToInt32(dosarDb.LawyerId); 
            var lawyer = await _context.Lawyers.FirstOrDefaultAsync(l => l.Id == lawyerId);
            if (lawyer != null && !string.IsNullOrWhiteSpace(lawyer.Email))
            {
              email = lawyer.Email;
              name = lawyer.LawyerName;
            }
          }


          _logger.LogInformation($"Sending email to {dosarDb.LawyerEmail} for case {dosar.numar}");



          // Trimitere notificare email
          await _emailService.SendNotificatonEmail(email, name, dosar.numar.ToString());
          _logger.LogInformation($"Notification email sent for case {dosar.numar} to {email}");


          // Create in-app notification
          var notification = new Notification
          {
            Title = $"Modificări pentru dosarul {dosar.numar}",
            Message = $"S-au detectat modificări pentru dosarul {dosar.numar} ,avocat {dosarDb.LawyerName}, client {dosarDb.ClientName}",
            Timestamp = DateTime.UtcNow,
            Type = "hearing_changes",
            FileNumber = dosar.numar,
            LawyerName= dosarDb.LawyerName,
            Source=dosarDb.Source,
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

