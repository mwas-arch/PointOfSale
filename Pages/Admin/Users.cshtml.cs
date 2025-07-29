using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PointOfSale.Pages.Admin
{
	[Authorize(Roles = "SuperAdmin")] // Optional: Restrict to SuperAdmin
	public class UsersModel : PageModel
	{
		private readonly UserManager<IdentityUser> _userManager;

		public UsersModel(UserManager<IdentityUser> userManager)
		{
			_userManager = userManager;
		}

		public List<UserWithRoles> Users { get; set; } = new();

		public class UserWithRoles
		{
			public string Id { get; set; }
			public string Email { get; set; }
			public List<string> Roles { get; set; } = new();
		}

		public async Task OnGetAsync()
		{
			var allUsers = _userManager.Users.ToList();

			foreach (var user in allUsers)
			{
				var roles = await _userManager.GetRolesAsync(user);
				Users.Add(new UserWithRoles
				{
					Id = user.Id,
					Email = user.Email,
					Roles = roles.ToList()
				});
			}
		}
	}
}
