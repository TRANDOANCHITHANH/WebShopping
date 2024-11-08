using Microsoft.AspNetCore.Mvc;
using WebShopping.Models;
using WebShopping.Services.Momo;

namespace WebShopping.Controllers
{
	public class PaymentController : Controller
	{
		private IMomoService _momoService;

		public PaymentController(IMomoService momoService)
		{
			_momoService = momoService;
		}

		[HttpPost]
		public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentMomo(model);
			return Redirect(response.PayUrl);
		}

		[HttpGet]
		public IActionResult PaymentCallback()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return View(response);
		}
	}

}
