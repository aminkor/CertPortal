using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CertPortal.Entities;

namespace CertPortal.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        
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