using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebShopping.Models
{
	public class AppUserModel: IdentityUser
	{
		public string Occupation {  get; set; }
		public string RoleId { get; set; }
        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }
    }
}
