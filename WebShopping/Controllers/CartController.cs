using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Models.ViewModels;
using WebShopping.Repository;

namespace WebShopping.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
			};
			return View(cartVM);
		}
		public IActionResult Checkout()
		{
			return View();
		}
		public async Task<IActionResult> Add(int Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
			CartItemModel cartIM = cartItems.Where(c => c.ProductId == Id).FirstOrDefault();
			if (cartIM == null)
			{
				cartItems.Add(new CartItemModel(product));

			}
			else
			{
				cartIM.Quantity += 1;
			}
			HttpContext.Session.SetJson("Cart", cartItems);
			TempData["success"] = "Thêm giỏ hàng thành công";
			return Redirect(Request.Headers["Referer"].ToString());
		}
		public async Task<IActionResult> Decrease(int Id)
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartIM = cartItems.Where(c => c.ProductId == Id).FirstOrDefault();
			if (cartIM.Quantity > 1)
			{
				--cartIM.Quantity;
			}
			else
			{
				cartItems.RemoveAll(p => p.ProductId == Id);
			}
			if (cartItems.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItems);
			}
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Increase(int Id)
		{
			ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			CartItemModel cartIM = cartItems.Where(c => c.ProductId == Id).FirstOrDefault();
			if (cartIM.Quantity >= 1 && product.Quantity > cartIM.Quantity)
			{
				++cartIM.Quantity;
				TempData["success"] = "Tăng số lượng sản phẩm thành công";
			}
			else
			{
				cartIM.Quantity = product.Quantity;
				TempData["success"] = "Số lượng tối đa của sản phẩm. Thêm giỏ hàng thành công";
			}
			if (cartItems.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItems);
			}
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Remove(int Id)
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			cartItems.RemoveAll(p => p.ProductId == Id);
			if (cartItems.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cartItems);
			}
			TempData["success"] = "Remove product of cart Successfully";
			return RedirectToAction("Index");
		}
		public async Task<IActionResult> Clear()
		{
			HttpContext.Session.Remove("Cart");
			TempData["success"] = "Clear all product of cart Successfully";
			return RedirectToAction("Index");
		}
	}
}
