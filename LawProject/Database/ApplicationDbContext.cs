using LawProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LawProject.Database
{
  public class ApplicationDbContext: DbContext
  {

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }

    public DbSet<MyFile> Files { get; set; }

    public DbSet<Notes> Notes { get; set; }

    public DbSet<Lawyer> Lawyers { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<ScheduledEvent> ScheduledEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Notes>()
          .HasOne(n => n.Dosar)
          .WithMany(d => d.Notes)
          .HasForeignKey(n => n.DosarId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
