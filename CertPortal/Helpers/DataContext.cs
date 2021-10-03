using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CertPortal.Entities;

namespace CertPortal.Helpers
{
    public partial class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountCertificate> AccountCertificates { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        public DbSet<InstitutionStudent> InstitutionStudents { get; set; }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InstitutionStudent>(entity =>
            {
           

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.InstitutionStudent)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("institutionstudents_accounts_Id_fk");

                entity.HasOne(d => d.Institution)
                    .WithMany(p => p.InstitutionStudent)
                    .HasForeignKey(d => d.InstitutionId)
                    .HasConstraintName("institutionstudents_institutions_Id_fk");
            });
            OnModelCreatingPartial(modelBuilder);
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}