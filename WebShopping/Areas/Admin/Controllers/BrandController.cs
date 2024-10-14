using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    /*[Authorize(Roles = "Admin")]*/
    public class BrandController : Controller
    {
        private readonly DataContext _datacontext;
        public BrandController(DataContext context)
        {
            _datacontext = context;

        }
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brands = _datacontext.Brands.ToList();
            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = brands.Count;
            var paper = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = brands.Skip(recSkip).Take(paper.PageSize).ToList();
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
        [Route("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _datacontext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Thương hiệu đã có trong database");
                    return View(brand);
                }
                _datacontext.Add(brand);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm thương hiệu thành công";
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

            return View(brand);

        }
		[HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
		{
			BrandModel brand = await _datacontext.Brands.FindAsync(Id);

			return View(brand);
		}
		[HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int Id, BrandModel brand)
		{

			if (ModelState.IsValid)
			{
				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _datacontext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã có trong database");
					return View(brand);
				}


				_datacontext.Update(brand);
				await _datacontext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thương hiệu thành công";
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

			return View(brand);

		}
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
		{
			BrandModel brand = await _datacontext.Brands.FindAsync(Id);

			_datacontext.Brands.Remove(brand);
			await _datacontext.SaveChangesAsync();
			TempData["error"] = "Thương hiệu đã được xóa";
			return RedirectToAction("Index");
		}
	}
}
