using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    /*[Authorize(Roles = "Admin")]*/
    public class UserController : Controller
    {

        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;
		public UserController(UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager,DataContext dataContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
			_dataContext = dataContext;	
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new { User = u, RoleName = r.Name }
                                        ).ToListAsync();
            return View(usersWithRoles);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Role = new SelectList(roles, "Id", "Name");

            return View(new AppUserModel());
        }
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Create")]
		public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				
				var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
				if (createUserResult.Succeeded)
				{
					
					var createUser = await _userManager.FindByEmailAsync(user.Email);

					if (createUser != null)
					{
						
						var roleData = await _roleManager.FindByIdAsync(user.RoleId);

						if (roleData != null)
						{
							
							var addToRoleResult = await _userManager.AddToRoleAsync(user, roleData.Name);
							if (addToRoleResult.Succeeded)
							{
								return RedirectToAction("Index", "User");
							}
							else
							{
								
								foreach (var error in addToRoleResult.Errors)
								{
									ModelState.AddModelError(string.Empty, error.Description);
								}
							}
						}
						else
						{
							ModelState.AddModelError(string.Empty, "Role not found.");
						}
					}
				}
				else
				{
					// Thêm các lỗi từ việc tạo người dùng
					foreach (var error in createUserResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}

				
				return View(user);
			}
			else
			{
				TempData["error"] = "Model có vài thứ đang lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}

				// Trả về form create user, không phải Index
				return View(user);
			}
		}
        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
			
			var userRole = await _userManager.GetRolesAsync(user);
			if (userRole.Any())
			{
		
				var currentRole = roles.FirstOrDefault(r => r.Name == userRole.FirstOrDefault());
				if (currentRole != null)
				{
					user.RoleId = currentRole.Id;
				}
			}
			return View(user);
        }
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserModel user, string id)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingUser.Email = user.Email;
                existingUser.UserName = user.UserName;
                existingUser.PhoneNumber = user.PhoneNumber;

                var updateUserResult = await _userManager.UpdateAsync(existingUser);

                if (updateUserResult.Succeeded)
                {
                   
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    var newRole = await _roleManager.FindByIdAsync(user.RoleId);

                    if (newRole != null)
                    {
                       
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        }

                        var addToRoleResult = await _userManager.AddToRoleAsync(existingUser, newRole.Name);

                        if (addToRoleResult.Succeeded)
                        {
                            TempData["success"] = "Update user thành công";
                            return RedirectToAction("Index", "User");
                        }
                        else
                        {
                            foreach (var error in addToRoleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Role = new SelectList(roles, "Id", "Name");
            TempData["error"] = "Model validation failed";
            return View(existingUser);
        }

        [HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var deleteResult = await _userManager.DeleteAsync(user);
			if (!deleteResult.Succeeded)
			{
				return View("Error");
			}
			TempData["success"] = "User đã được xóa thành công";
			return RedirectToAction("Index");
		}
	}
}