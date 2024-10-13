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
		public async Task<IActionResult> Create(AppUserModel user, string role)
		{
			if (ModelState.IsValid)
			{
				// Tạo người dùng với mật khẩu (chưa nên dùng PasswordHash)
				var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
				if (createUserResult.Succeeded)
				{
					// Lấy lại người dùng vừa tạo để có thông tin về UserId
					var createUser = await _userManager.FindByEmailAsync(user.Email);

					if (createUser != null)
					{
						// Tìm vai trò bằng tên (hoặc Id nếu cần)
						var roleData = await _roleManager.FindByIdAsync(user.RoleId);

						if (roleData != null)
						{
							// Gán người dùng vào vai trò
							var addToRoleResult = await _userManager.AddToRoleAsync(user, roleData.Name);
							if (addToRoleResult.Succeeded)
							{
								return RedirectToAction("Index", "User");
							}
							else
							{
								// Thêm các lỗi từ việc gán vai trò
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

				// Nếu có lỗi, trả về view với model để hiển thị lại form
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

	}
}