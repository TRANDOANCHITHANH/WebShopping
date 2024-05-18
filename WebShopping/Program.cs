using Microsoft.EntityFrameworkCore;
using WebShopping.Repository;

namespace WebShopping
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDb"]);
            }
            );
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.IsEssential = true;  
            });
            var app = builder.Build();
            app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");
            app.UseSession();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
			app.MapControllerRoute(
			   name: "areas",
			pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
				);

            app.MapControllerRoute(
               name: "category",
            pattern: "/category/{Slug?}",
            defaults: new {controller="Category",action="Index"}
                ) ;

            app.MapControllerRoute(
              name: "brand",
           pattern: "/brand/{Slug?}",
           defaults: new { controller = "Brand", action = "Index" }
               );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
			

			var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
            SeedData.SeedingData(context);
            app.Run();
        }
    }
}
