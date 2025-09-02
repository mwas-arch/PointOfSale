using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;
using PointOfSale.Models;

[Authorize(Roles = "SuperAdmin,SalesPerson,StoreOwner")]
public class ReceiptModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public ReceiptModel(ApplicationDbContext context)
	{
		_context = context;
	}

	public Sale Sale { get; set; }

	public async Task<IActionResult> OnGetAsync(int id)
	{
		Sale = await _context.Sales
			.Include(s => s.SaleItems)
				.ThenInclude(si => si.Product)
			.FirstOrDefaultAsync(s => s.Id == id);

		if (Sale == null)
			return NotFound();

		return Page();
	}
}
