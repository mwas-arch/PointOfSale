using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PointOfSale.Models
{
	public class Sale
	{
		public int Id { get; set; }

		public string UserId { get; set; }
		[ForeignKey("UserId")]
		public IdentityUser User { get; set; }

		public string CustomerName { get; set; }
		public string CustomerPhone { get; set; }

		public DateTime SaleDate { get; set; }

		public List<SaleItem> SaleItems { get; set; } = new();

	}


}
