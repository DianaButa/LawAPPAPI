using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawProject.Database;
using LawProject.DTO;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.ContractService
{
  public class ContractService : IContractService
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _env;

    public ContractService(ApplicationDbContext dbContext, IWebHostEnvironment env)
    {
      _dbContext = dbContext;
      _env = env;
    }

    public async Task<byte[]> GenerateDocumentsAsync(GenerateContractDto dto)
    {
      // 1. Găsește clientul în funcție de tipul clientului și ID-ul clientului
      string clientName = string.Empty;
      string adresaClient = string.Empty;
      string cnp = string.Empty;
      string cui = string.Empty;

      switch (dto.ClientType)
      {
        case "PF":
          var clientPF = await _dbContext.ClientPFs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPF == null)
            throw new ArgumentException($"Clientul fizic cu ID {dto.ClientId} nu a fost găsit.");

          clientName = $"{clientPF.FirstName} {clientPF.LastName}";
          adresaClient = clientPF.Address;
          cnp = clientPF.CNP;
          break;

        case "PJ":
          var clientPJ = await _dbContext.ClientPJs.FirstOrDefaultAsync(c => c.Id == dto.ClientId);
          if (clientPJ == null)
            throw new ArgumentException($"Clientul juridic cu ID {dto.ClientId} nu a fost găsit.");

          clientName = clientPJ.CompanyName;
          adresaClient = clientPJ.Address;
          cui = clientPJ.CUI;
          break;

        default:
          throw new ArgumentException($"ClientType invalid: {dto.ClientType}. Se acceptă doar PF sau PJ.");
      }

      // 2. Încarcă șablonul de contract
      // Calea relativă la root-ul proiectului (folderul unde e fișierul .csproj)
      string webRootPath = _env.ContentRootPath; // root-ul proiectului pe Azure
      string templateFilePath = Path.Combine(webRootPath, "Contract", "CONTRACT.docx");

      if (!File.Exists(templateFilePath))
      {
        throw new FileNotFoundException($"Șablonul de contract nu a fost găsit la calea {templateFilePath}");
      }

      // 3. Copiază șablonul într-un nou fișier pentru modificare
      string newFileName = $"Contract_{clientName}.docx";
      string newFilePath = Path.Combine(webRootPath, "Contract", newFileName);
      File.Copy(templateFilePath, newFilePath, true);

      // 4. Înlocuiește placeholderii din document cu datele clientului
      using (WordprocessingDocument doc = WordprocessingDocument.Open(newFilePath, true))
      {
        Body body = doc.MainDocumentPart.Document.Body;

        // Înlocuiește placeholderii cu datele clientului
        ReplacePlaceholders(body, clientName, adresaClient, cnp, cui, dto.Onorariu, dto.Scadenta, dto.Obiect);


        doc.MainDocumentPart.Document.Save();
      }

      // 5. Citește fișierul generat și returnează-l ca byte array
      byte[] fileBytes = File.ReadAllBytes(newFilePath);
      return fileBytes;
    }


    private void ReplacePlaceholders(Body body, string clientName, string adresaClient, string cnp, string cui, string onorariu, string scadenta, string obiect)
    {
      foreach (var para in body.Elements<Paragraph>())
      {
        foreach (var run in para.Elements<Run>())
        {
          foreach (var text in run.Elements<Text>())
          {
            // Înlocuim placeholder-ele din document
            text.Text = text.Text.Replace("<<client>>", clientName);
            text.Text = text.Text.Replace("<<address>>", adresaClient);
            text.Text = text.Text.Replace("<<cnp>>", cnp);  // Pentru PF
            text.Text = text.Text.Replace("<<cui>>", cui);  // Pentru PJ
            text.Text = text.Text.Replace("<<onorariu>>", onorariu);
            text.Text = text.Text.Replace("<<scadenta>>", scadenta);
            text.Text = text.Text.Replace("<<obiect>>", obiect);
          }
        }
      }
    }

  }
}
    

