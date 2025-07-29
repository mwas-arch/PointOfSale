using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PointOfSale.Pages.Admin
{
	[Authorize(Roles = "SuperAdmin")]
	public class ManageRolesModel : PageModel
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public ManageRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public class UserRoleViewModel
		{
			public string Id { get; set; }
			public string Email { get; set; }
			public List<string> Roles { get; set; } = new();
		}

		public List<UserRoleViewModel> Users { get; set; } = new();
		public List<string> AllRoles { get; set; } = new();

		[BindProperty]
		public string SelectedUserId { get; set; }

		[BindProperty]
		public string SelectedRole { get; set; }

		public string Message { get; set; }

		public async Task OnGetAsync()
		{
			var allUsers = _userManager.Users.ToList();
			AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();

			foreach (var user in allUsers)
			{
				var roles = await _userManager.GetRolesAsync(user);
				Users.Add(new UserRoleViewModel
				{
					Id = user.Id,
					Email = user.Email,
					Roles = roles.ToList()
				});
			}
		}

		//remove role from user
		public async Task<IActionResult> OnPostRemoveRoleAsync()
		{
			var user = await _userManager.FindByIdAsync(SelectedUserId);

			if (user == null || string.IsNullOrEmpty(SelectedRole))
			{
				TempData["Message"] = "? Invalid user or role.";
				return RedirectToPage();
			}

			// ?? Prevent removing your own SuperAdmin role
			var currentUserId = _userManager.GetUserId(User);
			if (user.Id == currentUserId && SelectedRole == "SuperAdmin")
			{
				TempData["Message"] = "? You cannot remove your own SuperAdmin role.";
				return RedirectToPage();
			}

			var result = await _userManager.RemoveFromRoleAsync(user, SelectedRole);

			if (result.Succeeded)
			{
				TempData["Message"] = $"? Removed role '{SelectedRole}' from {user.Email}.";
			}
			else
			{
				TempData["Message"] = $"? Failed to remove role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
			}

			return RedirectToPage();
		}



		public async Task<IActionResult> OnPostAssignRoleAsync()
		{
			var user = await _userManager.FindByIdAsync(SelectedUserId);
			var currentUserId = _userManager.GetUserId(User);

			if (user != null && !string.IsNullOrEmpty(SelectedRole))
			{
				// ? Prevent demoting yourself from SuperAdmin
				if (user.Id == currentUserId && SelectedRole != "SuperAdmin")
				{
					TempData["Message"] = "? You cannot remove your own SuperAdmin privileges.";
					return RedirectToPage();
				}

				// ? Add role if user doesn't have it already
				if (!await _userManager.IsInRoleAsync(user, SelectedRole))
				{
					await _userManager.AddToRoleAsync(user, SelectedRole);
					TempData["Message"] = $"? Role '{SelectedRole}' assigned to {user.Email}.";
				}
				else
				{
					TempData["Message"] = $"{user.Email} is already in the role '{SelectedRole}'.";
				}
			}

			return RedirectToPage();
		}
	}
}


