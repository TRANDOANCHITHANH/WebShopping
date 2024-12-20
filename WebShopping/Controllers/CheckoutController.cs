﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebShopping.Areas.Admin.Repository;
using WebShopping.Models;
using WebShopping.Models.Momo;
using WebShopping.Models.ViewModels;
using WebShopping.Repository;
using WebShopping.Services.Momo;

namespace WebShopping.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		private IMomoService _momoService;
		public CheckoutController(IEmailSender emailSender, DataContext context)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}
		public async Task<IActionResult> Checkout(string OrderId)
		{
			var userEmail = User.FindFirstValue(ClaimTypes.Email);
			if (userEmail == null)
			{
				return RedirectToAction("Login", "Account");
			}
			else
			{
				var orderCode = Guid.NewGuid().ToString();
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;
				if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}
				var coupon_code = Request.Cookies["CouponTitle"];
				var orderItem = new OrderModel
				{
					OrderCode = orderCode,
					ShippingCost = shippingPrice,
					UserName = userEmail,
					Status = 1,
					CreateDate = DateTime.Now,
					CouponCode = coupon_code,
					PaymentMethod = "COD"
				};
				_dataContext.Add(orderItem);
				_dataContext.SaveChanges();

				List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

				decimal totalAmount = 0;
				string productListHtml = "<ul>";

				foreach (var cart in cartItems)
				{
					var orderDetails = new OrderDetails();
					orderDetails.UserName = userEmail;
					orderDetails.OrderCode = orderCode;
					orderDetails.ProductId = cart.ProductId;
					orderDetails.Price = cart.Price;
					orderDetails.Quantity = cart.Quantity;
					var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
					product.Quantity -= cart.Quantity;
					product.Sold += cart.Quantity;
					_dataContext.Add(orderDetails);
					_dataContext.SaveChanges();
					totalAmount += cart.Price * cart.Quantity;
					productListHtml += $"<li>{cart.ProductName} - Giá: {cart.Price} x Số lượng: {cart.Quantity} = {cart.Price * cart.Quantity}</li>";
				}

				productListHtml += "</ul>";

				HttpContext.Session.Remove("Cart");
				string message = $@"
            <h2>Cảm ơn quý khách đã đặt hàng tại cửa hàng của chúng tôi!</h2>
            <p>Đơn hàng của bạn đã được đặt thành công. Dưới đây là chi tiết đơn hàng:</p>
            <p><strong>Mã đơn hàng:</strong> {orderCode}</p>
            <p><strong>Sản phẩm:</strong></p>
            {productListHtml}
            <p><strong>Tổng giá trị đơn hàng:</strong> {totalAmount.ToString("C")} VND</p>
            <p>Chúng tôi sẽ liên hệ với bạn khi đơn hàng được xử lý. Xin cảm ơn quý khách!</p>
            <br/>
            <p>Trân trọng,</p>
            <p>Cửa hàng của chúng tôi</p>";


				var receiver = userEmail;
				var subject = "Đặt hàng thành công";

				await _emailSender.SendEmailAsync(receiver, subject, message);

				TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
				return RedirectToAction("Index", "Cart");
			}
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
		{
			var requestQuery = HttpContext.Request.Query;

			// Kiểm tra resultCode
			if (!requestQuery.TryGetValue("resultCode", out var resultCode) || resultCode != "0")
			{
				TempData["success"] = "Đã hủy giao dịch MoMo";
				return RedirectToAction("Index", "Cart");
			}

			// Kiểm tra các tham số khác
			if (!requestQuery.TryGetValue("orderId", out var orderId) ||
				!requestQuery.TryGetValue("Amount", out var amountValue) ||
				!decimal.TryParse(amountValue, out var amount) ||
				!requestQuery.TryGetValue("orderInfo", out var orderInfo))
			{
				throw new ArgumentException("Missing or invalid query parameters from MoMo callback.");
			}

			// Lưu giao dịch vào database
			var newMomoInsert = new MomoInfoModel
			{
				OrderId = orderId,
				FullName = User.FindFirstValue(ClaimTypes.Email),
				Amount = amount,
				OrderInfo = orderInfo,
				DatePaid = DateTime.Now
			};

			_dataContext.Add(newMomoInsert);
			await _dataContext.SaveChangesAsync();
			await Checkout(requestQuery["orderId"]);
			var response = _momoService.PaymentExecuteAsync(requestQuery);
			return View(response);
		}
	}
}
