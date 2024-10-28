using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebShopping.Areas.Admin.Repository;
using WebShopping.Models;
using WebShopping.Models.ViewModels;
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
    }
}
