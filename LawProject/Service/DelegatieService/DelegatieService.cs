using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawProject.Database;
using LawProject.DTO;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.DelegatieService
{
  public class DelegatieService : IDelegatieService
  {
    private readonly ApplicationDbContext _dbContext;

    public DelegatieService(ApplicationDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<byte[]> GenerateDocumentAsync(DelegatieDto dto)
    {
      // 1. Preluăm clientul după tip
      string clientName = string.Empty;

      switch (dto.ClientType)
      {
        case "PF":
          var clientPF = await _dbContext.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPF == null)
            throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId} nu a fost găsit.");
          clientName = $"{clientPF.FirstName} {clientPF.LastName}";
          break;

        case "PJ":
          var clientPJ = await _dbContext.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPJ == null)
            throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId} nu a fost găsit.");
          clientName = clientPJ.CompanyName;
          break;

        default:
          throw new ArgumentException($"ClientType invalid: {dto.ClientType}. Se acceptă doar PF sau PJ.");
      }

      // 2. Preluăm dosarul aferent ales
      var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == dto.FileId && f.ClientId == dto.ClientId);
      if (file == null)
        throw new ArgumentException($"Dosarul cu ID {dto.FileId} nu a fost găsit pentru clientul cu ID {dto.ClientId}.");

      string numarContract = file.NumarContract;  
      string numarDelegatie = file.Delegatie;

      // 3. Încarcă șablonul de delegație
      string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string templateFilePath = Path.Combine(baseDirectory, "Delegatie/DELEGATIE.docx");

      if (!File.Exists(templateFilePath))
      {
        throw new FileNotFoundException($"Șablonul de delegație nu a fost găsit la calea {templateFilePath}");
      }

      // 4. Copiază șablonul pentru modificare
      string newFileName = $"Delegatie_{clientName}.docx";
      string newFilePath = Path.Combine(baseDirectory, "Delegatie", newFileName);
      File.Copy(templateFilePath, newFilePath, true);

      // 5. Înlocuiește placeholderii din document cu datele delegației
      using (WordprocessingDocument doc = WordprocessingDocument.Open(newFilePath, true))
      {
        Body body = doc.MainDocumentPart.Document.Body;

        ReplaceDelegatiePlaceholders(body, clientName, numarContract, numarDelegatie, dto.Institutie, dto.Activitate);

        doc.MainDocumentPart.Document.Save();
      }

      // 6. Returnează fișierul generat ca byte[]
      byte[] fileBytes = File.ReadAllBytes(newFilePath);
      return fileBytes;
    }


    private void ReplaceDelegatiePlaceholders(Body body, string clientName, string numarContract, string numarDelegatie, string institutie, string activitate)
    {
      foreach (var para in body.Elements<Paragraph>())
      {
        // Construim textul complet al paragrafului
        string paraText = para.InnerText;

        if (paraText.Contains("<<client>>") || paraText.Contains("<<numarContract>>") || paraText.Contains("<<numarDelegatie>>") || paraText.Contains("<<institutie>>") || paraText.Contains("<<activitate>>"))
        {
          // Ștergem toate run-urile (ca să le reconstruim)
          para.RemoveAllChildren<Run>();

          // Construim un nou Run cu textul înlocuit
          string replacedText = paraText
              .Replace("<<client>>", clientName)
              .Replace("<<numarContract>>", numarContract)
              .Replace("<<numarDelegatie>>", numarDelegatie)
              .Replace("<<institutie>>", institutie)
              .Replace("<<activitate>>", activitate);

          para.AppendChild(new Run(new Text(replacedText)));
        }
      }
    
  }
}
}
