using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PointOfSale.Data;
using PointOfSale.Models;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "SuperAdmin,SalesPerson,StoreOwner")]
public class CreateModel : PageModel
{
	[BindProperty(SupportsGet = true)]
	public string SearchTerm { get; set; }

	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;

	public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
	{
		_context = context;
		_userManager = userManager;
	}

	public List<Product> Products { get; set; }

	[BindProperty]
	public string CustomerName { get; set; }

	[BindProperty]
	public string CustomerPhone { get; set; }

	[BindProperty]
	public string CartItemsJson { get; set; }

	public async Task<IActionResult> OnGetAsync()
	{
		var query = _context.Products.AsQueryable();

		if (!string.IsNullOrWhiteSpace(SearchTerm))
		{
			query = query.Where(p =>
				p.Name.Contains(SearchTerm) ||
				p.Description.Contains(SearchTerm));
		}

		Products = await query.ToListAsync();
		return Page();
	}


	public async Task<IActionResult> OnPostAsync()
	{
		// Fetch products again so the page can display them even after POST
		Products = await _context.Products.ToListAsync();

		if (!ModelState.IsValid) return Page();

		var cartItems = JsonConvert.DeserializeObject<List<SaleItem>>(CartItemsJson);
		if (cartItems == null || !cartItems.Any())
		{
			ModelState.AddModelError("", "Cart is empty.");
			return Page(); // Products will now not be null
		}

		var user = await _userManager.GetUserAsync(User);

		var sale = new Sale
		{
			UserId = user.Id,
			CustomerName = CustomerName,
			CustomerPhone = CustomerPhone,
			SaleDate = DateTime.Now,
			SaleItems = new List<SaleItem>()
		};

		foreach (var item in cartItems)
		{
			var product = await _context.Products.FindAsync(item.ProductId);
			if (product == null || product.Stock < item.Quantity)
			{
				ModelState.AddModelError("", $"Insufficient stock for {product?.Name ?? "Unknown Product"}.");
				return Page(); // Products will now not be null
			}

			product.Stock -= item.Quantity;

			sale.SaleItems.Add(new SaleItem
			{
				ProductId = item.ProductId,
				Quantity = item.Quantity,
				UnitPrice = item.UnitPrice
			});
		}

		_context.Sales.Add(sale);


		await _context.SaveChangesAsync();

		TempData["Message"] = "Sale saved successfully!";
		return RedirectToPage("/Sales/Create");

	}

}
