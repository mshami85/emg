using Emergency.Classes;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Emergency.Dtos
{
    public class DeviceModel
    {
        [Required(ErrorMessage = "الحقل مطلوب"), MaxLength(100)]
        [Display(Name = "معرف الجهاز Android Id")]
        [Remote("CheckAndroidId", "Device", AdditionalFields = "Id", ErrorMessage = "الرقم مسجل مسبقا", HttpMethod = "POST")]
        public string AndroidId { get; set; }

        [Required(ErrorMessage = "الحقل مطلوب"), MaxLength(100)]
        [Display(Name = "اسم المالك")]
        public string Owner { get; set; }

        [Required(ErrorMessage = "الحقل مطلوب"), MaxLength(100)]
        [Display(Name = "رقم التحقق")]
        public string SecureCode { get; set; } = KeyUtil.GenerateKey();

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }

    public class DeviceViewModel : DeviceModel
    {
        public int Id { get; set; }
    }
}