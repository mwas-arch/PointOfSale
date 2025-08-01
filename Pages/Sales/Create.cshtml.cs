using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PointOfSale.Data;
using PointOfSale.Models;

[Authorize(Roles = "SuperAdmin,SalesPerson,StoreOwner")]
public class CreateModel : PageModel
{
	private readonly ApplicationDbContext _context;
	private readonly UserManager<IdentityUser> _userManager;

	public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
	{
		_context = context;
		_userManager = userManager;
	}
	
	[BindProperty(SupportsGet = true)]	
	public string SearchTerm { get; set; }

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
				p.Name.Contains(SearchTerm) || p.Description.Contains(SearchTerm));
		}

		Products = await query.ToListAsync();
		return Page();
	}

	public async Task<IActionResult> OnPostAsync()
	{
		// Load Products for redisplay if needed
		Products = await _context.Products.ToListAsync();

		//if (!ModelState.IsValid)
		//{
		//	ModelState.AddModelError("", "Form is invalid.");
		//	return Page();
		//}

		if (string.IsNullOrWhiteSpace(CartItemsJson))
		{
			ModelState.AddModelError("", "Cart is empty.");
			return Page();
		}

		List<CartItemDto> cartItems;
		try
		{
			cartItems = JsonConvert.DeserializeObject<List<CartItemDto>>(CartItemsJson);
		}
		catch
		{
			ModelState.AddModelError("", "Invalid cart data.");
			return Page();
		}

		if (cartItems == null || !cartItems.Any())
		{
			ModelState.AddModelError("", "No items in cart.");
			return Page();
		}

		var user = await _userManager.GetUserAsync(User);
		var sale = new Sale
		{
			UserId = user?.Id,
			CustomerName = CustomerName,
			CustomerPhone = CustomerPhone,
			SaleDate = DateTime.Now,
			SaleItems = new List<SaleItem>()
		};

		foreach (var item in cartItems)
		{
			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
			if (product == null)
			{
				ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
				return Page();
			}

			if (product.Stock < item.Quantity)
			{
				TempData["Message"] = $"Out of stock: {product.Name} (only {product.Stock} left)";
				return RedirectToPage("/Sales/Create");
			}


			// Reduce stock
			product.Stock -= item.Quantity;
			_context.Products.Update(product);

			// Add sale item
			sale.SaleItems.Add(new SaleItem
			{
				ProductId = product.Id,
				Quantity = item.Quantity,
				UnitPrice = item.UnitPrice
			});
		}

		_context.Sales.Add(sale);
		await _context.SaveChangesAsync();

		TempData["Message"] = "Sale completed and stock updated successfully.";
		return RedirectToPage("/Sales/Create");
	}

	public class CartItemDto
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
	}
}
