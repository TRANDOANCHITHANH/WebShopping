using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    public class OrderController : Controller
    {
        private readonly DataContext _datacontext;
        public OrderController(DataContext context)
        {
            _datacontext = context;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _datacontext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailOrder = await _datacontext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == ordercode)
                .OrderByDescending(od => od.Id)
                .ToListAsync();

            // Trả về view với danh sách OrderDetails có liên quan đến ordercode
            return View(DetailOrder);
        }
        [Route("UpdateOrder")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _datacontext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            order.Status = status;
            try
            {
                await _datacontext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }catch (Exception ex) {
                return StatusCode(500, "An error");
                 }
        }
    }
}
