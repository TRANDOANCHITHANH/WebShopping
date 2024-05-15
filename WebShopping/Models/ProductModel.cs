using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebShopping.Repository.Validation;

namespace WebShopping.Models
{
	public class ProductModel
	{
		
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập tên sản phẩm")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu mô tả tên sản phẩm")]
		public string Description { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
		public decimal Price { get; set; }
	
		public string Slug { get; set; }
		[Required,Range(1,int.MaxValue,ErrorMessage ="Chọn một thương hiệu")]
		public int BrandId { get; set; }
		[Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
		public int CategoryId { get; set; }
		public CategoryModel Category { get; set; }
		public BrandModel Brand { get; set; }
		public string Image { get; set; } = "noimage.jpg";
		[NotMapped]
		[FileExtension]
		public IFormFile ImageUpload { get; set; }
	}
}
