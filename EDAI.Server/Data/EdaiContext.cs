using EDAI.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EDAI.Server.Data;

public class EdaiContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Essay> Essays { get; set; }
    public DbSet<Score> Scores { get; set; }
    
    public EdaiContext() { }
    
    public EdaiContext(DbContextOptions<EdaiContext> options) : base(options) { }
}