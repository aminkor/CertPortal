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
        public DbSet<RoleInstitution> RoleInstitutions { get; set; }

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

            modelBuilder.Entity<Account>()
                .HasMany(c => c.Certificates)
                .WithOne(e => e.Account).OnDelete(DeleteBehavior.SetNull);
            ;
            
            modelBuilder.Entity<Certificate>()
                .HasOne(e => e.Account)
                .WithMany(c => c.Certificates);
            
            
            modelBuilder.Entity<Certificate>()
                .HasOne(e => e.Institution)
                .WithMany(c => c.Certificates);
            
            modelBuilder.Entity<Institution>()
                .HasMany(c => c.Certificates)
                .WithOne(e => e.Institution).OnDelete(DeleteBehavior.SetNull);
            ;
            
            modelBuilder.Entity<Institution>()
                .HasMany(c => c.Students)
                .WithOne(e => e.Institution).OnDelete(DeleteBehavior.SetNull);
;
            
            OnModelCreatingPartial(modelBuilder);
        }
        
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}