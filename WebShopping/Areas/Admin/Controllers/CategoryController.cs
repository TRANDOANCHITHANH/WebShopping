using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	/*[Authorize(Roles = "Admin")]*/
	[Route("Admin/Category")]
	public class CategoryController : Controller
	{
		private readonly DataContext _datacontext;
		public CategoryController(DataContext context)
		{
			_datacontext = context;
			
		}
		[Route("Index")]
		public async Task <IActionResult> Index(int pg = 1)
		{
			List<CategoryModel>category = _datacontext.Categories.ToList();
			const int pageSize = 10;
			if (pg < 1)
			{
				pg = 1;
			}
			int recsCount = category.Count;
			var paper = new Paginate(recsCount, pg,pageSize);
			int recSkip = (pg - 1) * pageSize;
			var data = category.Skip(recSkip).Take(paper.PageSize).ToList();
			ViewBag.Paper = paper;
			
			return View(data);
		}
        [HttpGet]
		[Route("Create")]
        public IActionResult Create()
        {
           
            return View();
        }
		[HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CategoryModel category)
		{
			
			if (ModelState.IsValid)
			{
				category.Slug = category.Name.Replace(" ", "-");
				var slug = await _datacontext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Danh mục đã có trong database");
					return View(category);
				}
				_datacontext.Add(category);
				await _datacontext.SaveChangesAsync();
				TempData["success"] = "Thêm danh mục thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}

			return View(category);

		}
		[HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
		{
			CategoryModel category = await _datacontext.Categories.FindAsync(Id);
			
			return View(category);
		}
		[HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int Id, CategoryModel category)
		{
		
			if (ModelState.IsValid)
			{
				category.Slug = category.Name.Replace(" ", "-");
				var slug = await _datacontext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Danh mục đã có trong database");
					return View(category);
				}
				

				_datacontext.Update(category);
				await _datacontext.SaveChangesAsync();
				TempData["success"] = "Cập nhật danh mục thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}

			return View(category);

		}
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
		{
			CategoryModel category = await _datacontext.Categories.FindAsync(Id);
			
			_datacontext.Categories.Remove(category);
			await _datacontext.SaveChangesAsync();
			TempData["error"] = "Danh mục đã được xóa";
			return RedirectToAction("Index");
		}
	}
}
