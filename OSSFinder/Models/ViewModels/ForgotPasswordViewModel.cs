using System.ComponentModel.DataAnnotations;

namespace OSSFinder.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}