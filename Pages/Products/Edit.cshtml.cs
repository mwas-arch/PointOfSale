using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;
using PointOfSale.Models;
using System.Threading.Tasks;

namespace PointOfSale.Pages_Products
{
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public EditModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public Product Product { get; set; } = default!;

		public async Task<IActionResult> OnGetAsync(int? id)
		{
			if (id == null)
				return NotFound();

			Product = await _context.Products.FindAsync(id);

			if (Product == null)
				return NotFound();

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
				return Page();

			// Load existing product from database
			var productInDb = await _context.Products.FindAsync(Product.Id);
			if (productInDb == null)
				return NotFound();

			// Update fields
			productInDb.Name = Product.Name;
			productInDb.CostPrice = Product.CostPrice;
			productInDb.SellingPrice = Product.SellingPrice;
			productInDb.Stock = Product.Stock;
			productInDb.Category = Product.Category;

			await _context.SaveChangesAsync();

			return RedirectToPage("./Index");
		}


		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.Id == id);
		}
	}
}
