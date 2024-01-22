using Microsoft.EntityFrameworkCore;


namespace Data.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Data.Models.StoredCredentialDetail> Fido2StoredCredential { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // public DbSet<StoredCredentialDetail> Fido2StoredCredential { get; set; }
    }
}
