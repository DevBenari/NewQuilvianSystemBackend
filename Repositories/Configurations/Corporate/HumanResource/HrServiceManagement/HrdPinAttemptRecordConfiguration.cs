using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class HrdPinAttemptRecordConfiguration : IEntityTypeConfiguration<HrdPinAttemptRecord>
    {
        public void Configure(EntityTypeBuilder<HrdPinAttemptRecord> entity)
        {
            entity.ToTable("HrdPinAttemptRecord", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttemptKey).HasMaxLength(200).IsRequired();
            entity.Property(x => x.VerificationHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ClientIp).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LockedUntilUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.LastAttemptUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => x.AttemptKey)
                .IsUnique()
                .HasDatabaseName("uq_pin_attempt_record_key");
            entity.HasIndex(x => x.LastAttemptUtc);
        }
    }
}
