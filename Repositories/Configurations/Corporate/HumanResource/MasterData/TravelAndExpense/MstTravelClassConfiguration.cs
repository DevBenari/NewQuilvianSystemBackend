using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelClassConfiguration : IEntityTypeConfiguration<MstTravelClass>
    {
        public void Configure(EntityTypeBuilder<MstTravelClass> entity)
        {
            entity.ToTable("MstTravelClass", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelClassCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TravelClassName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.TransportMode).HasMaxLength(50).HasDefaultValue("Air").IsRequired();
            entity.Property(x => x.ClassLevel).HasMaxLength(50);
            entity.Property(x => x.IsDomesticAllowed).HasDefaultValue(true);
            entity.Property(x => x.IsInternationalAllowed).HasDefaultValue(false);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.TravelClassCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TravelClassName);
            entity.HasIndex(x => new { x.TransportMode, x.ClassLevel, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsDefault, x.SortOrder, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);
        }
    }
}
