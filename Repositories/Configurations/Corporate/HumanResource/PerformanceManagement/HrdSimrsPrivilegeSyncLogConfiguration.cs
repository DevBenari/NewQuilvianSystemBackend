using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdSimrsPrivilegeSyncLogConfiguration : IEntityTypeConfiguration<HrdSimrsPrivilegeSyncLog>
    {
        public void Configure(EntityTypeBuilder<HrdSimrsPrivilegeSyncLog> entity)
        {
            entity.ToTable("HrdSimrsPrivilegeSyncLog", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DoctorCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000);
            entity.Property(x => x.DispatchedAt).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => new { x.VerdictId, x.DispatchedAt });
            entity.HasIndex(x => x.DispatchedAt);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdSimrsPrivilegeSyncLog> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
