using System.ComponentModel.DataAnnotations;

namespace PointOfSale.Models
{
	public class Product
	{
		[Key] // EF Core recognizes this as the primary key
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Range(0.01, 100000)]
		public decimal CostPrice { get; set; }
		[Range(0.01, 100000)]
		public decimal SellingPrice { get; set; }

		[Range(0, 100000)]
		public int Stock { get; set; }

		public string? Category { get; set; }
		public string? Description { get; set; }
	}
}
