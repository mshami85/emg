using System.ComponentModel.DataAnnotations;

namespace Emergency.Dtos
{
    public class MobileModel
    {
        [Required, MaxLength(100)]
        public string Owner { get; set; }

        [Required, MaxLength(50)]
        public string AndroidId { get; set; }

        [Required, MaxLength(50)]
        public string Secret { get; set; }

    }
}
