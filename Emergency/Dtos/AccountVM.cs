using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Emergency.Dtos
{
    public class LoginVM
    {
        [Required(ErrorMessage = "لا يمكن أن يكون الحقل فارغا")]
        [Display(Name = "اسم المستخدم")]
        public string Name { get; set; }

        [Required(ErrorMessage = "لا يمكن أن يكون الحقل فارغا")]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "الاحتفاظ بتسجيل الدخول على هذا الجهاز")]
        public bool RememberMe { get; set; }
    }

    public class RegisterVM
    {
        [Display(Name = "اسم المستخدم")]
        [Required(ErrorMessage = "الحقل مطلوب")]
        [MaxLength(50)]
        [Remote("CheckUserName", "Account", ErrorMessage = "الاسم موجود مسبقا", HttpMethod = "POST")]
        public string Name { get; set; }

        [Display(Name = "الاسم الفعلي")]
        [Required(ErrorMessage = "الحقل مطلوب")]
        public string FullName { get; set; }

        [Display(Name = "كلمة المرور")]
        [Required(ErrorMessage = "الحقل مطلوب")]
        public string Password { get; set; }

        [Display(Name = "تأكيد كلمة المرور")]
        [Required(ErrorMessage = "الحقل مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقتين")]
        public string Confirm { get; set; }
    }

    public class UpdateFullNameVM
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "الاسم الكامل")]
        [Required(ErrorMessage = "الحقل مطلوب")]
        public string FullName { get; set; }
    }
}