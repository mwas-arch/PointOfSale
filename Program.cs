using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Data;


namespace PointOfSale
{
	public class Program
	{
		
		public static async Task Main(string[] args) // ? Make Main async
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
								   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddRoles<IdentityRole>() // ? Add role support
				.AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddRazorPages();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapRazorPages();

			// ? Seed roles here
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				await CreateRolesAsync(services);
			}

			app.Run();
		}

		// ? Role creation method
		public static async Task CreateRolesAsync(IServiceProvider serviceProvider)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

			string[] roleNames = { "SuperAdmin", "StoreOwner", "SalesPerson" };

			foreach (var roleName in roleNames)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}

			// ? Assign SuperAdmin role to the first user
			var firstUser = userManager.Users.FirstOrDefault();
			if (firstUser != null && !(await userManager.IsInRoleAsync(firstUser, "SuperAdmin")))
			{
				await userManager.AddToRoleAsync(firstUser, "SuperAdmin");
			}
		}

	}
}

