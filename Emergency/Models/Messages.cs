using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emergency.Models
{
    public enum MobileStatus
    {
        [Description("أنا بخير")]
        IAM_OK,
        [Description("أحتاج مساعدة")]
        NEED_HELP,
        [Description("حالة طوارئ")]
        EMERGENCY
    }

    [Owned]
    public class GeoLocation
    {
        [MaxLength(50)]
        public string? Latitude { get; set; }
        [MaxLength(50)]
        public string? Longitude { get; set; }

        public void LatLongFrom(string? lat, string? lon)
        {
            Latitude = lat;
            Longitude = lon;
        }
        public bool HasValue() => !string.IsNullOrWhiteSpace(Longitude) || !string.IsNullOrWhiteSpace(Latitude);
    }

    [Table("AdminMessages")]
    public class AdminMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime SendTime { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string? Text { get; set; }
        public GeoLocation Location { get; set; } = new();
        public virtual User Sender { get; set; }
        public virtual ICollection<MessageDelivery> Deliveries { get; set; } = new List<MessageDelivery>();
    }

    [Table("MessagesDelivery")]
    public class MessageDelivery
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime? DeliverTime { get; set; }
        public virtual AdminMessage Message { get; set; }
        public virtual Mobile Mobile { get; set; }
    }

    [Table("MobileMessages")]
    public class MobileMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime SendTime { get; set; }
        public GeoLocation Location { get; set; } = new();
        public MobileStatus Status { get; set; }
        [MaxLength(100)]
        public string? Text { get; set; }
        public virtual Mobile Mobile { get; set; }
        public bool Shown { get; set; }
    }

    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
    }
}
