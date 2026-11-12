using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdFppeTriggerEventConfiguration : IEntityTypeConfiguration<HrdFppeTriggerEvent>
    {
        public void Configure(EntityTypeBuilder<HrdFppeTriggerEvent> entity)
        {
            entity.ToTable("HrdFppeTriggerEvent", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DoctorNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.DoctorCodeSnapshot).HasMaxLength(50);
            entity.Property(x => x.SpecializationSnapshot).HasMaxLength(150);
            entity.Property(x => x.TriggerType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.MetricEvidence).HasMaxLength(2000);
            entity.Property(x => x.RecommendedAction).HasMaxLength(500);
            entity.Property(x => x.IncidentDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DetectedAt).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.TriggerType });
            entity.HasIndex(x => x.DetectedAt);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdFppeTriggerEvent> entity)
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
