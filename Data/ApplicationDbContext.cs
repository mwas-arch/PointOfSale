using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Models; // Add this if not already present

namespace PointOfSale.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		// ✅ DbSet must go inside the class register
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// ✅ Set precision for decimal
			modelBuilder.Entity<Product>()
						.Property(p => p.Price)
						.HasPrecision(18, 2);
		}
	}
}
