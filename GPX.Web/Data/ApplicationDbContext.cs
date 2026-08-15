using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// [GPX-DOC-v1] ================================================================================
// DbContext de EF Core que extiende IdentityDbContext con las entidades ApplicationUser, AppProfile,
// AppModule y AppProfileModule.
// ================================================================================================

namespace GPX.Web.Data {
    /// <summary>
    /// Clase ApplicationDbContext. DbContext de EF Core que extiende IdentityDbContext con las entidades
    /// ApplicationUser, AppProfile, AppModule y AppProfileModule.
    /// </summary>
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
        public DbSet<AppProfile> Profiles => Set<AppProfile>();
        public DbSet<AppModule> Modules => Set<AppModule>();
        public DbSet<AppProfileModule> ProfileModules => Set<AppProfileModule>();

        /// <summary>
        /// On Model Creating.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity => {
                entity.Property(record => record.FullName)
                    .HasMaxLength(200);

                entity.HasOne(record => record.Profile)
                    .WithMany(profile => profile.Users)
                    .HasForeignKey(record => record.ProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppProfile>(entity => {
                entity.ToTable("AppProfiles");
                entity.Property(record => record.Name)
                    .HasMaxLength(100);
                entity.Property(record => record.Description)
                    .HasMaxLength(250);
                entity.HasIndex(record => record.Name)
                    .IsUnique();
            });

            builder.Entity<AppModule>(entity => {
                entity.ToTable("AppModules");
                entity.Property(record => record.Code)
                    .HasMaxLength(50);
                entity.Property(record => record.Name)
                    .HasMaxLength(100);
                entity.Property(record => record.Route)
                    .HasMaxLength(150);
                entity.Property(record => record.Description)
                    .HasMaxLength(250);
                entity.Property(record => record.IconCssClass)
                    .HasMaxLength(150);
                entity.Property(record => record.ParentCode)
                    .HasMaxLength(50);
                entity.Property(record => record.ParentName)
                    .HasMaxLength(100);
                entity.Property(record => record.ParentIconCssClass)
                    .HasMaxLength(150);
                entity.HasIndex(record => record.Code)
                    .IsUnique();
                entity.HasIndex(record => record.Route)
                    .IsUnique();
            });

            builder.Entity<AppProfileModule>(entity => {
                entity.ToTable("AppProfileModules");
                entity.HasKey(record => new { record.ProfileId, record.ModuleId });
                entity.HasOne(record => record.Profile)
                    .WithMany(profile => profile.ProfileModules)
                    .HasForeignKey(record => record.ProfileId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(record => record.Module)
                    .WithMany(module => module.ProfileModules)
                    .HasForeignKey(record => record.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
