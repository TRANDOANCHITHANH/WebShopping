﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext context)
		{
			_dataContext = context;
		}

		public async Task<IActionResult> Index(string Slug = "")
		{
			BrandModel brand = _dataContext.Brands.Where(b => b.Slug == Slug).FirstOrDefault();
			if (brand == null)
			{
				return RedirectToAction("Index");
			}
			var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
			return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
