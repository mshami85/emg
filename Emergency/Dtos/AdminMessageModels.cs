using Emergency.Models;
using System.ComponentModel.DataAnnotations;

namespace Emergency.Dtos
{
    public class AdminMessageModel
    {
        [Required(ErrorMessage = "يجب إدخال نص للرسالة")]
        [MaxLength(100)]
        public string? Text { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
    }

    public class AdminMessageViewModel
    {
        public int Id { get; set; }
        public DateTime SendTime { get; set; }
        public string? Text { get; set; }
        public GeoLocation Location { get; set; } = new();
        public string Sender { get; set; }

        public static AdminMessageViewModel CopyFrom(AdminMessage m)
        {
            var vm = new AdminMessageViewModel
            {
                Text = m.Text,
                Id = m.Id,
                Sender = m.Sender?.FullName ?? "Admin",
                SendTime = m.SendTime,
            };
            vm.Location.LatLongFrom(m.Location.Latitude, m.Location.Longitude);
            return vm;
        }
    }
}
