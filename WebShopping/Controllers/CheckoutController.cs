using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		public CheckoutController( DataContext context)
		{
			_dataContext = context;

		}
		public async Task<IActionResult> Checkout()
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else {
			var orderCode = Guid.NewGuid().ToString();
				var orderItem = new OrderModel();
				orderItem.OrderCode = orderCode;
				orderItem.UserName = userEmail;
				orderItem.Status = 1;
				orderItem.CreateDate = DateTime.Now;
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();
				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
				foreach (var cartItem in cartItems)
				{
					var orderDetails = new OrderDetails();
					orderDetails.UserName = userEmail;
					orderDetails.OrderCode = orderCode;
					orderDetails.ProductId = cartItem.ProductId;
					orderDetails.Price = cartItem.Price;
					orderDetails.Quantity = cartItem.Quantity;
					_dataContext.Add(orderDetails);
					_dataContext.SaveChanges();
				}
				HttpContext.Session.Remove("Cart");
				TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
				return RedirectToAction("Index","Cart");
			}
			return View();
		}
	}
}
