using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Emergency.Models
{
    [Table("Mobiles")]
    public class Mobile
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Owner { get; set; }

        [Required, MaxLength(50)]
        public string AndroidId { get; set; }

        [Required, MaxLength(50)]
        public string SecureCode { get; set; }

        public bool Enabled { get; set; }

        public string? Notes { get; set; }

        public DateTime? RegisterDate { get; set; }

        public virtual ICollection<MobileMessage> Messages { get; set; } = new List<MobileMessage>();
        public virtual ICollection<MessageDelivery> ReceivedMessages { get; set; } = new List<MessageDelivery>();
    }
}
