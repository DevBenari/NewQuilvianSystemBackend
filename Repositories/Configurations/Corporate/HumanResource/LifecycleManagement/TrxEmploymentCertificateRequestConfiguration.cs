using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxEmploymentCertificateRequestConfiguration : IEntityTypeConfiguration<TrxEmploymentCertificateRequest>
    {
        public void Configure(EntityTypeBuilder<TrxEmploymentCertificateRequest> builder)
        {
            builder.ToTable("TrxEmploymentCertificateRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestDate).HasColumnType("date");
            builder.Property(x => x.CertificateType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LanguageCode).HasMaxLength(30);
            builder.Property(x => x.Purpose).HasMaxLength(500);
            builder.Property(x => x.DeliveryMethod).HasMaxLength(50);
            builder.Property(x => x.RequestStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DocumentPath).HasMaxLength(500);
            builder.Property(x => x.OriginalFileName).HasMaxLength(250);
            builder.Property(x => x.ContentType).HasMaxLength(150);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.IssuedByUser).WithMany().HasForeignKey(x => x.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.RequestDate });
        }
    }
}
