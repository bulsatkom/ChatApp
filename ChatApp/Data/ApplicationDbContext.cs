using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
            this.Database.Migrate();
        }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Receiver> Receivers { get; set; }

    }
}
