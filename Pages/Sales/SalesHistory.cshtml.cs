using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;
using PointOfSale.Models;

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
			.Include(s => s.User)
			.Include(s => s.SaleItems)
			.ToListAsync();
	}
}
