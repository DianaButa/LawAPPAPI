
using LawProject.Enums;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LawProject.Database
{
  public class ApplicationDbContext : DbContext
  {

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


    public DbSet<Raport> Rapoarte { get; set; }
    public DbSet<Cheltuieli> Cheltuieli { get; set; }
    public DbSet<Contract> Contracts { get; set; }

    public DbSet<DailyEvents> DailyEvents { get; set; }
    public DbSet<RaportTask> RaportTaskuri { get; set; }

    public DbSet<ClientPF> ClientPFs { get; set; }

    public DbSet<ClientPJ> ClientPJs { get; set; }

    public DbSet<MyFile> Files { get; set; }

    public DbSet<WorkTask> Tasks { get; set; }

    public DbSet<Notes> Notes { get; set; }

    public DbSet<Invoice> Invoices { get; set; }

    public DbSet<Receipt> Receipts { get; set; }

    public DbSet<POS> POSs { get; set; }
    public DbSet<Lawyer> Lawyers { get; set; }
    public DbSet<EventA> EventsA { get; set; }

    public DbSet<EventC> EventsC { get; set; }


    public DbSet<Notification> Notifications { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<ScheduledEvent> ScheduledEvents { get; set; }

    public DbSet<ClientPFFile> ClientPFFiles { get; set; }
    public DbSet<ClientPJFile> ClientPJFiles { get; set; }
    public DbSet<LawyerFile> LawyerFiles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Configurarea relației many-to-many între ClientPF și MyFile
      modelBuilder.Entity<ClientPFFile>()
          .HasKey(cp => new { cp.ClientPFId, cp.MyFileId });


      modelBuilder.Entity<ClientPFFile>()
          .HasOne(cp => cp.ClientPF)
          .WithMany(c => c.ClientPFFiles)
          .HasForeignKey(cp => cp.ClientPFId);

      modelBuilder.Entity<ClientPFFile>()
          .HasOne(cp => cp.MyFile)
          .WithMany(m => m.ClientPFFiles)
          .HasForeignKey(cp => cp.MyFileId);

      // Configurarea relației many-to-many între ClientPJ și MyFile
      modelBuilder.Entity<ClientPJFile>()
          .HasKey(cj => new { cj.ClientPJId, cj.MyFileId });

      modelBuilder.Entity<ClientPJFile>()
          .HasOne(cj => cj.ClientPJ)
          .WithMany(c => c.ClientPJFiles)
          .HasForeignKey(cj => cj.ClientPJId);

      modelBuilder.Entity<ClientPJFile>()
          .HasOne(cj => cj.MyFile)
          .WithMany(m => m.ClientPJFiles)
          .HasForeignKey(cj => cj.MyFileId);


      modelBuilder.Entity<LawyerFile>()
               .HasKey(lf => new { lf.LawyerId, lf.FileId });

      modelBuilder.Entity<LawyerFile>()
          .HasOne(lf => lf.Lawyer)
          .WithMany(l => l.LawyerFiles)
          .HasForeignKey(lf => lf.LawyerId)
          .OnDelete(DeleteBehavior.NoAction); 

      modelBuilder.Entity<LawyerFile>()
          .HasOne(lf => lf.File)
          .WithMany(f => f.LawyerFiles)
          .HasForeignKey(lf => lf.FileId)
          .OnDelete(DeleteBehavior.NoAction);

      modelBuilder.Entity<Raport>()
            .HasMany(r => r.TaskuriLucrate)
            .WithOne(rt => rt.Raport)
            .HasForeignKey(rt => rt.RaportId)
            .OnDelete(DeleteBehavior.Cascade);

      //modelBuilder.Entity<EventA>()
      //.Property(e => e.ClientType)
      //.HasConversion(
      //    v => v.ToString(),  // Enum -> String
      //    v => Enum.Parse<ClientTypeEnum>(v)  // String -> Enum
      //);

      //// Definirea relațiilor
      //modelBuilder.Entity<EventA>()
      //    .HasOne(e => e.Lawyer)
      //    .WithMany(l => l.EventsA)
      //    .HasForeignKey(e => e.LawyerId);

      //modelBuilder.Entity<EventA>()
      //    .HasOne(e => e.ClientPF)
      //    .WithMany(c => c.EventsA)
      //    .HasForeignKey(e => e.ClientPFId)
      //    .OnDelete(DeleteBehavior.SetNull);

      //modelBuilder.Entity<EventA>()
      //    .HasOne(e => e.ClientPJ)
      //    .WithMany(c => c.EventsA)
      //    .HasForeignKey(e => e.ClientPJId)
      //    .OnDelete(DeleteBehavior.SetNull);
    }
  }
  }


