using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModuleManager.Models;

namespace ModuleManager.Data;

public class ApplicationDbContext
    : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }

    public DbSet<Module> Modules { get; set; }
    public DbSet<Component> Components { get; set; }
    public DbSet<Process> Processes { get; set; }
    public DbSet<Template> Templates { get; set; }
}
