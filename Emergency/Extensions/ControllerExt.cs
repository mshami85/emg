using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Emergency
{
    public enum JobStatus
    {
        [Description("النتيحة غير معروفة")]
        UNKNOWN = 0,

        [Description("تمت العملية بنجاح")]
        SUCCESS = 1,

        [Description("فشلت العملية")]
        FAIL = -1
    }

    public static class ControllerExtensions
    {
        public static void SetState(this Controller instance, JobStatus status, string? message = null)
        {
            instance.TempData["XStatus"] = status;
            instance.TempData["XMessage"] = message ?? status.ToDescription();
        }

        public static void LoadState(this Controller instance)
        {
            instance.ViewData["XStatus"] = instance.TempData["XStatus"];
            instance.ViewData["XMessage"] = instance.TempData["XMessage"];
        }
    }

}
