using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Route("Admin/Product")]
    public class ProductController : Controller
	{
		private readonly DataContext _datacontext;
		private readonly IWebHostEnvironment _environment;
		public ProductController(DataContext context,IWebHostEnvironment _webHostEnvironment) 
		{  
			_datacontext = context;
			_environment = _webHostEnvironment;
		}
        [Route("Index")]
        public async Task<IActionResult> Index(int pg=1)
		{
            List<ProductModel> products = _datacontext.Products
			.Include(p => p.Category)  
			.Include(p => p.Brand)     
			.ToList();
            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = products.Count;
            var paper = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;
            var data = products.Skip(recSkip).Take(paper.PageSize).ToList();
            ViewBag.Paper = paper;

            return View(data);

        }
		[HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
			ViewBag.Categories = new SelectList(_datacontext.Categories,"Id","Name");
			ViewBag.Brands = new SelectList(_datacontext.Brands,"Id","Name");
            return View();
        }
		[HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductModel productModel)
		{
			ViewBag.Categories = new SelectList(_datacontext.Categories, "Id", "Name", productModel.CategoryId);
			ViewBag.Brands = new SelectList(_datacontext.Brands, "Id", "Name",productModel.BrandId);
			if(ModelState.IsValid)
			{
				productModel.Slug = productModel.Name.Replace(" ", "-");
				var slug = await _datacontext.Products.FirstOrDefaultAsync(p=>p.Slug==productModel.Slug);
				if(slug!=null)
				{
					ModelState.AddModelError("", "Sản phẩm đã có trong database");
					return View(productModel);
				}
				if(productModel.ImageUpload !=null)
					{
						string uploadsDir = Path.Combine(_environment.WebRootPath,"media/products");
						string imageName = Guid.NewGuid().ToString() + "_" + productModel.ImageUpload.FileName;
						string filePath = Path.Combine(uploadsDir, imageName);
						FileStream fs = new FileStream(filePath,FileMode.Create);
						await productModel.ImageUpload.CopyToAsync(fs);
						fs.Close();
						productModel.Image = imageName;
					}
				
				_datacontext.Add(productModel);
				await _datacontext.SaveChangesAsync();
				TempData["success"] = "Thêm sản phẩm thành công";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang bị lỗi";
				List<string> errors = new List<string>();
				foreach(var value in ModelState.Values)
				{
					foreach(var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			
			return View(productModel);

		}
		[HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
		{
			ProductModel product = await _datacontext.Products.FindAsync(Id);
			ViewBag.Categories = new SelectList(_datacontext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_datacontext.Brands, "Id", "Name", product.BrandId);
			return View(product);
		}
		[HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int Id, ProductModel productModel)
		{
			ViewBag.Categories = new SelectList(_datacontext.Categories, "Id", "Name", productModel.CategoryId);
			ViewBag.Brands = new SelectList(_datacontext.Brands, "Id", "Name", productModel.BrandId);
			var existed = _datacontext.Products.Find(productModel.Id); 
			if (ModelState.IsValid)
			{
				productModel.Slug = productModel.Name.Replace(" ", "-");
				var slug = await _datacontext.Products.FirstOrDefaultAsync(p => p.Slug == productModel.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm đã có trong database");
					return View(productModel);
				}
				if (productModel.ImageUpload != null)
				{
				
					string uploadsDir = Path.Combine(_environment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + productModel.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);
					string oldFilePath = Path.Combine(uploadsDir, productModel.Image);
					try
					{
						if (System.IO.File.Exists(oldFilePath))
						{
							System.IO.File.Delete(oldFilePath);

						}
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "An error occured while deleting the product image");
					}
					FileStream fs = new FileStream(filePath, FileMode.Create);
					await productModel.ImageUpload.CopyToAsync(fs);
					fs.Close();
					existed.Image = imageName;

					
				}
				existed.Name = productModel.Name;			
				existed.Description = productModel.Description;
				existed.Price = productModel.Price;
				existed.CategoryId = productModel.CategoryId;
				existed.BrandId = productModel.BrandId;
				_datacontext.Update(existed);
				await _datacontext.SaveChangesAsync();
				TempData["success"] = "Cập nhật sản phẩm thành công";
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

			return View(productModel);

		}
        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
		{
			ProductModel product = await _datacontext.Products.FindAsync(Id);
			if (!string.Equals(product.Image, "noname.jpg"))
			{
				string uploadsDir = Path.Combine(_environment.WebRootPath, "media/products");
				
				string oldfilePath = Path.Combine(uploadsDir,product.Image);
				if(System.IO.File.Exists(oldfilePath))
				{
					System.IO.File.Delete(oldfilePath);
				}
			}
			_datacontext.Products.Remove(product);
			await _datacontext.SaveChangesAsync();
			TempData["error"] = "Sản phẩm đã được xóa";
			return RedirectToAction("Index");
		}
	}
}
