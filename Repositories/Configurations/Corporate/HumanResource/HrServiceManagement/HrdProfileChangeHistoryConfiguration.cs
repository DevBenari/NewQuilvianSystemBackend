using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class HrdProfileChangeHistoryConfiguration : IEntityTypeConfiguration<HrdProfileChangeHistory>
    {
        public void Configure(EntityTypeBuilder<HrdProfileChangeHistory> entity)
        {
            entity.ToTable("HrdProfileChangeHistory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AppliedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(x => x.OldValuesJson).HasColumnType("jsonb");
            entity.Property(x => x.NewValuesJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.HrServiceRequest)
                .WithMany()
                .HasForeignKey(x => x.HrServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.HrServiceRequestId);
            entity.HasIndex(x => new { x.EmployeeId, x.ComponentModified, x.AppliedAtUtc });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdProfileChangeHistory> entity)
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
