﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize]
	public class BrandController : Controller
    {
        private readonly DataContext _datacontext;
        public BrandController(DataContext context)
        {
            _datacontext = context;

        }
        public async Task<IActionResult> Index()
        {
            return View(await _datacontext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
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
		public async Task<IActionResult> Edit(int Id)
		{
			BrandModel brand = await _datacontext.Brands.FindAsync(Id);

			return View(brand);
		}
		[HttpPost]
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