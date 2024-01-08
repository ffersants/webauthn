using Microsoft.EntityFrameworkCore;
using NetDevPack.Fido2.EntityFramework.Store.Model;
using NetDevPack.Fido2.EntityFramework.Store.Store;

namespace Data.Context
{
    public class AppDbContext : DbContext, IFido2Context
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<StoredCredentialDetail> Fido2StoredCredential { get; set; }

        // public DbSet<StoredCredentialDetail> Fido2StoredCredential { get; set; }
    }
}
