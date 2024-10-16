using Microsoft.AspNetCore.Mvc;

namespace WebShopping.Controllers
{
	public class PaymentController : Controller
	{
		private readonly string _accessKey;
		private readonly string _secretKey;
		public IActionResult Index()
		{
			return View();
		}
		public PaymentController(IConfiguration configuration)
		{
			_accessKey = configuration[""];
			_secretKey = configuration[""];
		}
	}
}
