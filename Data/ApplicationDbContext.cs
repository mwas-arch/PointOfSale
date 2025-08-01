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
		public DbSet<Sale> Sales { get; set; }
		public DbSet<SaleItem> SaleItems { get; set; }



		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Product>()
				.Property(p => p.CostPrice)
				.HasColumnType("decimal(18,2)");

			modelBuilder.Entity<Product>()
				.Property(p => p.SellingPrice)
				.HasColumnType("decimal(18,2)");

			

			modelBuilder.Entity<SaleItem>()
				.Property(s => s.UnitPrice)
				.HasPrecision(18, 2); // 18 total digits, 2 after decimal

			modelBuilder.Entity<Sale>()
		.HasMany(s => s.SaleItems)
		.WithOne(si => si.Sale)
		.HasForeignKey(si => si.SaleId)
		.OnDelete(DeleteBehavior.Cascade);

			// ✅ Also add this to ensure SaleItem → Product is valid
			modelBuilder.Entity<SaleItem>()
				.HasOne(si => si.Product)
				.WithMany()
				.HasForeignKey(si => si.ProductId);




		}

	}
}
