﻿using System.ComponentModel.DataAnnotations;

namespace WebShopping.Repository.Validation
{
	public class FileExtensionAttribute: ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if(value is IFormFile file)
			{
				var extension = Path.GetExtension(file.FileName);
				string[] extensions = { "jpg", "png", "jpeg" };
				bool result = extension.Any(x=>extension.EndsWith(x));
				if(!result)
				{
					return new ValidationResult("Allowed extensions are jpg or png or jpeg");
				}
			}
			return ValidationResult.Success;
			
		}
	}
}
