using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebShopping.Areas.Admin.Repository;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly IEmailSender _emailSender;
		public CheckoutController(IEmailSender emailSender, DataContext context)
		{
			_dataContext = context;
			_emailSender = emailSender;
		}
		public async Task<IActionResult> Checkout()
		{
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel
                {
                    OrderCode = orderCode,
                    UserName = userEmail,
                    Status = 1,
                    CreateDate = DateTime.Now
                };
                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

                decimal totalAmount = 0;
                string productListHtml = "<ul>";

                foreach (var cartItem in cartItems)
                {
                    var orderDetails = new OrderDetails
                    {
                        UserName = userEmail,
                        OrderCode = orderCode,
                        ProductId = cartItem.ProductId,
                        Price = cartItem.Price,
                        Quantity = cartItem.Quantity
                    };
                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();

                    // Tính tổng giá trị đơn hàng
                    totalAmount += cartItem.Price * cartItem.Quantity;

                    // Thêm sản phẩm vào danh sách hiển thị trong email
                    productListHtml += $"<li>{cartItem.ProductName} - Giá: {cartItem.Price} x Số lượng: {cartItem.Quantity} = {cartItem.Price * cartItem.Quantity}</li>";
                }

                productListHtml += "</ul>";

                HttpContext.Session.Remove("Cart");

                // Nội dung HTML của email
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

                // Thông tin người nhận và tiêu đề email
                var receiver = userEmail;
                var subject = "Đặt hàng thành công";

                // Gửi email với nội dung HTML
                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng";
                return RedirectToAction("Index", "Cart");
            }
            return View();
		}
	}
}
