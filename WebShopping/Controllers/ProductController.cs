using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Models.ViewModels;
using WebShopping.Repository;

namespace WebShopping.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		public ProductController(DataContext context)
		{
			_dataContext = context;
		}

		public IActionResult Index()
		{
			return View();
		}
		public async Task<IActionResult> Details(int Id)
		{
			if (Id == null)
			{
				return RedirectToAction("Index");
			}
			var productsById = _dataContext.Products.Include(p => p.Rating).Where(p => p.Id == Id).FirstOrDefault();
			var relatedProducts = await _dataContext.Products.
				Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id).Take(4).ToListAsync();
			ViewBag.RelatedProducts = relatedProducts;
			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productsById,
			};
			return View(viewModel);
		}
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (ModelState.IsValid)
			{
				var ratingEntity = new RatingModel
				{
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star
				};
				_dataContext.Ratings.Add(ratingEntity);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm đánh giá thành công";
				return Redirect(Request.Headers["Referer"]);
			}
			else
			{
				TempData["error"] = "Model đang có lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					errors.Add(value.ToString());
				}
				string errorMessage = string.Join(", ", errors);
				return RedirectToAction("Detail", new { id = rating.ProductId });
			}

		}
		public async Task<IActionResult> Search(string searchTerm)
		{
			var products = await _dataContext.Products.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToListAsync();
			ViewBag.Keyword = searchTerm;
			return View(products);
		}
	}
}
