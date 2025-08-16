using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Data;

public class EdaiContext : IdentityDbContext<EDAIUser>
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Essay> Essays { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<IndexedContent> IndexedContents { get; set; }
    public DbSet<FeedbackComment> FeedbackComments { get; set; }
    public DbSet<EdaiDocument> Documents { get; set; }

    public DbSet<StudentClass> StudentClasses { get; set; }
    
    public DbSet<Organisation> Organisations { get; set; }
    public DbSet<StudentSummary> StudentSummaries { get; set; }
    
    public EdaiContext() { }
    
    public EdaiContext(DbContextOptions<EdaiContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Optional: configure the join table explicitly if needed
        modelBuilder.Entity<FeedbackComment>()
            .HasMany(s => s.RelatedTexts)
            .WithMany(c => c.FeedbackComments);
    }
}