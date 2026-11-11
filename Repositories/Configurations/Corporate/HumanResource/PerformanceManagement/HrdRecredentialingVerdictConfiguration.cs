using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdRecredentialingVerdictConfiguration : IEntityTypeConfiguration<HrdRecredentialingVerdict>
    {
        public void Configure(EntityTypeBuilder<HrdRecredentialingVerdict> entity)
        {
            entity.ToTable("HrdRecredentialingVerdict", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DoctorCodeSnapshot).HasMaxLength(50);
            entity.Property(x => x.DoctorNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.Verdict).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CommitteeNotes).HasMaxLength(2000);
            entity.Property(x => x.SignatureHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");

            entity.HasMany(x => x.SyncLogs)
                .WithOne(x => x.Verdict)
                .HasForeignKey(x => x.VerdictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.DecidedAt });
            entity.HasIndex(x => x.Verdict);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdRecredentialingVerdict> entity)
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
