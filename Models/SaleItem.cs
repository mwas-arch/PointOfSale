using System.ComponentModel.DataAnnotations.Schema;

namespace PointOfSale.Models
{
	public class SaleItem
	{
		public int Id { get; set; }

		public int SaleId { get; set; }
		[ForeignKey("SaleId")]
		public Sale Sale { get; set; }

		public int ProductId { get; set; }
		[ForeignKey("ProductId")]
		public Product Product { get; set; }

		public int Quantity { get; set; }

		public decimal UnitPrice { get; set; }
	}

}
