using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;
using PointOfSale.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "SuperAdmin,,StoreOwner")]

public class SalesHistoryModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public SalesHistoryModel(ApplicationDbContext context)
	{
		_context = context;
	}

	public List<Sale> Sales { get; set; }

	public async Task OnGetAsync()
	{
		Sales = await _context.Sales
			.Include(s => s.SaleItems)
				.ThenInclude(si => si.Product)
			.OrderByDescending(s => s.SaleDate)
			.ToListAsync();
	}
}
