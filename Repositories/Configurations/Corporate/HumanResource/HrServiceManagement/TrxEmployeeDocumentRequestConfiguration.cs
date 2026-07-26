using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class TrxEmployeeDocumentRequestConfiguration : IEntityTypeConfiguration<TrxEmployeeDocumentRequest>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeDocumentRequest> entity)
        {
            entity.ToTable("TrxEmployeeDocumentRequest", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NeededByDate).HasColumnType("date");
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.NumberOfCopies).HasDefaultValue(1);
            entity.Property(x => x.RequestedDataJson).HasColumnType("jsonb");
            entity.Property(x => x.IsEmployeeDownloadAllowed).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeDocumentType).WithMany(x => x.DocumentRequests).HasForeignKey(x => x.EmployeeDocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedForWorkforceProfile).WithMany().HasForeignKey(x => x.RequestedForWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedForEmployee).WithMany().HasForeignKey(x => x.RequestedForEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByUser).WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HrServiceRequest).WithMany(x => x.EmployeeDocumentRequests).HasForeignKey(x => x.HrServiceRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowInstance).WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser).WithMany().HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DocumentRequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.RequestedForWorkforceProfileId, x.RequestStatus, x.RequestedAt });
            entity.HasIndex(x => new { x.EmployeeDocumentTypeId, x.RequestStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeDocumentRequest> entity)
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
