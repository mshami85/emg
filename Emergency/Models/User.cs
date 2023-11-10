using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Emergency.Models
{
    public class UserRoles
    {
        public const string WEB_USER = "web_user";
        public const string MOBILE = "mobile";
    }

    public class User : IdentityUser<int>
    {
        [MaxLength(100)]
        public string FullName { get; set; }

        public bool Enabled { get; set; }

        public virtual ICollection<AdminMessage> Messages { get; set; } = new List<AdminMessage>();
    }
}
