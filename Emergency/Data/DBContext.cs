using Emergency.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Emergency.Data
{
    public class DBContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLazyLoadingProxies();
        }

        public DbSet<Mobile> Mobiles { get; set; }
        public DbSet<AdminMessage> AdminMessages { get; set; }
        public DbSet<MobileMessage> MobileMessages { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<MessageDelivery> MessagesDelivery { get; set; }
    }
}