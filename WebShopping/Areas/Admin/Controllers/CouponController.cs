using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopping.Models;
using WebShopping.Repository;

namespace WebShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Coupon")]
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;
        public CouponController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var couponList = await _dataContext.Coupons.ToListAsync();
            ViewBag.CouponList = couponList;
            return View();
        }
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)
        {
            if (ModelState.IsValid)
            {
                _dataContext.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm coupon thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Error";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n ", errors);
                return BadRequest(errorMessage);
            }
            return View();
        }
    }
}