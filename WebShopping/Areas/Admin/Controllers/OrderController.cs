using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
	[Area("Admin")]
	
	public class OrderController : Controller
	{
		private readonly DataContext _datacontext;
		public OrderController(DataContext context)
		{
			_datacontext = context;
		}
		public async Task<IActionResult> Index()
		{
			return View(await _datacontext.Orders.OrderByDescending(p=>p.Id).ToListAsync());
		}
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
			var DetailOrder = await _datacontext.OrderDetails.Include(od=>od.Product).Where(od=>od.OrderCode==ordercode).ToListAsync();
            return View(await _datacontext.OrderDetails.OrderByDescending(p => p.Id).ToListAsync());
        }
    }
}
