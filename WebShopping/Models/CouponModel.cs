using System.ComponentModel.DataAnnotations;

namespace WebShopping.Models
{
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được để trống")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được để trống")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Yêu cầu không được để trống")]
		public int Quantity { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateExpired { get; set; }
		public int Status { get; set; }
	}
}
