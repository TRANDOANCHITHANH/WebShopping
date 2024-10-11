using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
  /*  [Route("Admin/Role")]*/
 /*   [Authorize(Roles = "Admin")]*/
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
		private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(DataContext dataContext,RoleManager<IdentityRole> roleManager)
        {
			_roleManager = roleManager;
            _dataContext = dataContext;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderByDescending(p => p.Id).ToListAsync());
        }
        [Route("Create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IdentityRole role)
        {

			if (!_roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
			}
            return Redirect("Index");
		}
        [Route("Delete")]
		[HttpGet]
		public async Task<IActionResult> Delete(string Id)
		{
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound(); 
            }
            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                return NotFound();
            }
            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["Success"] = "Role deleted successfully";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while deleting the role");
            }
            return Redirect("Index");
		}
        
		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string Id)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return NotFound();
			}
            var role = await _roleManager.FindByIdAsync(Id);
			return View(role);
		}
       
		[HttpPost]
		[Route("Edit")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id,IdentityRole model)
		{
			
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
				var role = await _roleManager.FindByIdAsync(id);
				if (role == null)
				{
					return NotFound();
				}
                role.Name = model.Name;
				try
				{
					await _roleManager.UpdateAsync(role);
					TempData["Success"] = "Role updated successfully";
                    return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "An error occurred while updating the role");
				}
			}
			return View(model ?? new IdentityRole { Id = id });
			
		}
	}
}
