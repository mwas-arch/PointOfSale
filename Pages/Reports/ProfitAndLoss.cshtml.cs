using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PointOfSale.Data;
using PointOfSale.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PointOfSale.Pages.Reports
{
	public class ProfitAndLossModel : PageModel
	{
		private readonly ApplicationDbContext _context;

		public ProfitAndLossModel(ApplicationDbContext context)
		{
			_context = context;
		}

		[BindProperty]
		public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);

		[BindProperty]
		public DateTime ToDate { get; set; } = DateTime.Today;

		public List<ProfitLossItem> ReportItems { get; set; } = new();
		public decimal TotalRevenue { get; set; }
		public decimal TotalCost { get; set; }
		public decimal TotalProfit { get; set; }

		public async Task<IActionResult> OnPostAsync()
		{
			await GenerateReportAsync();
			return Page();
		}

		private async Task GenerateReportAsync()
		{
			ReportItems.Clear();
			TotalRevenue = 0;
			TotalCost = 0;
			TotalProfit = 0;

			var sales = await _context.Sales
				.Where(s => s.SaleDate >= FromDate && s.SaleDate <= ToDate)
				.Include(s => s.SaleItems)
				.ThenInclude(si => si.Product)
				.ToListAsync();

			foreach (var sale in sales)
			{
				foreach (var item in sale.SaleItems)
				{
					var buyingPrice = item.Product.CostPrice;
					var sellingPrice = item.UnitPrice;
					var quantity = item.Quantity;

					var cost = buyingPrice * quantity;
					var revenue = sellingPrice * quantity;
					var profit = revenue - cost;

					ReportItems.Add(new ProfitLossItem
					{
						SaleDate = sale.SaleDate,
						ProductName = item.Product.Name,
						Quantity = quantity,
						BuyingPrice = buyingPrice,
						SellingPrice = sellingPrice,
						Revenue = revenue,
						Cost = cost,
						Profit = profit
					});

					TotalRevenue += revenue;
					TotalCost += cost;
					TotalProfit += profit;
				}
			}
		}

		public async Task<IActionResult> OnPostExportCsvAsync()
		{
			await GenerateReportAsync();

			var csv = new StringWriter();

			// Header row
			csv.WriteLine("Date,Product,Qty,Buying Price,Selling Price,Revenue,Cost,Profit");

			foreach (var item in ReportItems)
			{
				csv.WriteLine($"{item.SaleDate:yyyy-MM-dd},{item.ProductName},{item.Quantity},{item.BuyingPrice},{item.SellingPrice},{item.Revenue},{item.Cost},{item.Profit}");
			}

			var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
			var stream = new MemoryStream(bytes);

			return File(stream, "text/csv", "ProfitAndLoss.csv");
		}



		public async Task<IActionResult> OnPostExportPdfAsync()
		{
			QuestPDF.Settings.License = LicenseType.Community;

			await GenerateReportAsync();

			var stream = new MemoryStream();

			var doc = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Size(PageSizes.A4);
					page.Margin(30);
					page.Content().Column(col =>
					{
						col.Item().Text($"Profit and Loss Report ({FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd})")
							.Bold().FontSize(16).AlignCenter();

						col.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.ConstantColumn(60);
								columns.RelativeColumn();
								columns.ConstantColumn(40);
								columns.ConstantColumn(60);
								columns.ConstantColumn(60);
								columns.ConstantColumn(60);
								columns.ConstantColumn(60);
								columns.ConstantColumn(60);
							});

							table.Header(header =>
							{
								header.Cell().Text("Date").Bold();
								header.Cell().Text("Product").Bold();
								header.Cell().Text("Qty").Bold();
								header.Cell().Text("Buy").Bold();
								header.Cell().Text("Sell").Bold();
								header.Cell().Text("Revenue").Bold();
								header.Cell().Text("Cost").Bold();
								header.Cell().Text("Profit").Bold();
							});

							foreach (var item in ReportItems)
							{
								table.Cell().Text(item.SaleDate.ToShortDateString());
								table.Cell().Text(item.ProductName);
								table.Cell().Text(item.Quantity.ToString());
								table.Cell().Text(item.BuyingPrice.ToString("F2"));
								table.Cell().Text(item.SellingPrice.ToString("F2"));
								table.Cell().Text(item.Revenue.ToString("F2"));
								table.Cell().Text(item.Cost.ToString("F2"));
								table.Cell().Text(item.Profit.ToString("F2"));
							}
						});

						col.Item().PaddingTop(20).Text($"Total Revenue: {TotalRevenue:F2} Ksh");
						col.Item().Text($"Total Cost: {TotalCost:F2} Ksh");
						col.Item().Text($"Total Profit: {TotalProfit:F2} Ksh");
					});
				});
			});

			doc.GeneratePdf(stream);
			stream.Position = 0;

			return File(stream, "application/pdf", "ProfitAndLoss.pdf");
		}

		public class ProfitLossItem
		{
			public DateTime SaleDate { get; set; }
			public string ProductName { get; set; }
			public int Quantity { get; set; }
			public decimal BuyingPrice { get; set; }
			public decimal SellingPrice { get; set; }
			public decimal Revenue { get; set; }
			public decimal Cost { get; set; }
			public decimal Profit { get; set; }
		}
	}
}
