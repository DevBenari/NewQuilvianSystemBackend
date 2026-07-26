using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxAccessRevocationConfiguration : IEntityTypeConfiguration<TrxAccessRevocation>
    {
        public void Configure(EntityTypeBuilder<TrxAccessRevocation> builder)
        {
            builder.ToTable("TrxAccessRevocation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.SystemName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.AccessType).HasMaxLength(100);
            builder.Property(x => x.AccountIdentifier).HasMaxLength(200);
            builder.Property(x => x.RequestedRevocationAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RevocationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EvidencePath).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.ExitClearance).WithMany(x => x.AccessRevocations).HasForeignKey(x => x.ExitClearanceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RevokedByUser).WithMany().HasForeignKey(x => x.RevokedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.ExitClearanceId, x.SystemName, x.AccountIdentifier }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.RevocationStatus });
        }
    }
}
