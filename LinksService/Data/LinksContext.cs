using LinksService.Models;
using Microsoft.EntityFrameworkCore;

namespace LinksService.Data;

public class LinksContext : DbContext
{
    public LinksContext(DbContextOptions<LinksContext> options) : base(options) { }

    public DbSet<Link> Links { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique constraint on Category.Name
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Index on Category.Order for sorting
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Order);

        // Configure Tags as comma-separated string
        var listComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<Link>()
            .Property(e => e.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(listComparer);

        // Link-Category relationship
        modelBuilder.Entity<Link>()
            .HasOne(l => l.Category)
            .WithMany()
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Index on CategoryId for filtering
        modelBuilder.Entity<Link>()
            .HasIndex(e => e.CategoryId);

        // Index on Order for sorting
        modelBuilder.Entity<Link>()
            .HasIndex(e => e.Order);

        // Unique username for admin
        modelBuilder.Entity<AdminUser>()
            .HasIndex(e => e.Username)
            .IsUnique();
    }
}
