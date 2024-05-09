using Microsoft.EntityFrameworkCore;
using WebShopping.Models;

namespace WebShopping.Repository
{
	public class SeedData
	{
		public static void SeedingData(DataContext _context)
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel laptop = new CategoryModel{ Name="Laptop",Slug="laptop",Description="Laptop is Large Brand",Status=1 };
				CategoryModel pc = new CategoryModel{ Name="Pc",Slug="pc",Description="PC is Large Brand",Status=1 };
				BrandModel dell = new BrandModel { Name = "Dell", Slug = "dell", Description = "Dell is Large Brand", Status = 1 };
				BrandModel asus = new BrandModel { Name = "Asus", Slug = "asus", Description = "Asus is Large Brand", Status = 1 };
				_context.Products.AddRange(
					new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is Large Brand", Image = "1.jpg", Category = laptop,Price=1200,Brand= dell},
					new ProductModel { Name = "Asus zenbook", Slug = "asuszenbook", Description = "Asus zenbook is Large Brand", Image = "2.jpg", Category = laptop,Price=1200,Brand=asus},
					new ProductModel { Name = "Dell Vostro 3KL", Slug = "dellvostro3kl", Description = "Dell Vostro 3KL is Large Brand", Image = "3.jpg", Category = pc,Price=1200,Brand=dell}

				);
				_context.SaveChanges();	
			}
		}
	}
}
