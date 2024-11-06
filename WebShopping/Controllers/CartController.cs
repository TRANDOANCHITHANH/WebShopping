using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
			decimal shippingPrice = 0;
			if (shippingPriceCookie != null)
			{
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
			}
			var coupon_code = Request.Cookies["CouponTitle"];

			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price) + shippingPrice,
				ShippingCost = shippingPrice,
				CouponCode = coupon_code
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
		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string phuong, string tinh, string quan)
		{
			var existingShipping = await _dataContext.Shippings.FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
			decimal shippingPrice = existingShipping?.Price ?? 35000; // Default to 35000 if not found

			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);

			try
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true // Only secure in production
				};
				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return StatusCode(500, "Error saving shipping information.");
			}

			// Return JSON with success and shippingPrice
			return Json(new { success = true, shippingPrice });
		}
		[HttpGet]
		[Route("Cart/DeleteShipping")]
		public IActionResult DeleteShipping()
		{
			Response.Cookies.Delete("ShippingPrice");
			return RedirectToAction("Index", "Cart");
		}
		[HttpPost]
		[Route("Cart/GetCoupon")]
		public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
		{
			var validCoupon = await _dataContext.Coupons.FirstOrDefaultAsync(x => x.Name == coupon_value);
			string couponTitle = validCoupon.Name + " | " + validCoupon?.Description;
			if (couponTitle != null)
			{
				TimeSpan remainingTitle = validCoupon.DateExpired - DateTime.Now;
				int daysRemaining = remainingTitle.Days;
				if (daysRemaining >= 0)
				{
					try
					{
						var cookieOptions = new CookieOptions
						{
							HttpOnly = true,
							Expires = DateTimeOffset.UtcNow.AddMinutes(30),
							Secure = true,
							SameSite = SameSiteMode.Strict
						};
						Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
						return Ok(new { success = true, message = "Coupon applied successfully" });
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
						return Ok(new { success = false, message = ex.ToString() });
					}
				}
				else
				{
					return Ok(new { success = false, message = "Coupon not existed" });
				}
			}
			return Json(new { CouponTitle = couponTitle });
		}
	}
}
