using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CertPortal.Entities;

namespace CertPortal.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountCertificate> AccountCertificates { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }

        private readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to sqlite database
            options.UseMySql(Configuration.GetConnectionString("CertPortalDB"));
        }
    }
}