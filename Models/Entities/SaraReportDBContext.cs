using Microsoft.EntityFrameworkCore;
using SaraReportAPI.Models.Entities.Views;

namespace SaraReportAPI.Models.Entities {
    public partial class SaraReportDBContext : DbContext
    {
        public SaraReportDBContext()
        {
        }

        public SaraReportDBContext(DbContextOptions<SaraReportDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Records> Records { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<RosterMeta4> RosterMeta4 { get; set; }
        public virtual DbSet<UserGroupsOrNames> UserGroupsOrNames { get; set; }
        public virtual DbSet<UserPermissions> UserPermissions { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
//                optionsBuilder.UseSqlServer("Data Source=CobaqDev003;Initial Catalog=SaraReportDB; Integrated Security=true;");
//            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Records>(entity =>
            {
                entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Reports>(entity =>
            {
                entity.Property(e => e.EndDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StartDate).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<RosterMeta4>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(d => d.RosterMeta4)
                    .HasForeignKey<Users>(d => d.EmployeeNumber);

                entity.HasOne(d => d.EmployeeSup)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.EmployeeNumberSup)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.ToView("RosterMeta4");
            });

            modelBuilder.Entity<UserPermissions>(entity =>
            {
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.RoleID)
                    .HasConstraintName("FK_UserPermissions_Roles");

                entity.HasOne(d => d.UserGroupsOrName)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.UserGroupsOrNameID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserPermissions_UserGroupsOrNames");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.Property(e => e.DateFirstLogin).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateLastLogin).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.RosterMeta4)
                .WithOne(p => p.User)
                .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
