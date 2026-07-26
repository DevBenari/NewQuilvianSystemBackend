using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class TrxHrServiceRequestAttachmentConfiguration : IEntityTypeConfiguration<TrxHrServiceRequestAttachment>
    {
        public void Configure(EntityTypeBuilder<TrxHrServiceRequestAttachment> entity)
        {
            entity.ToTable("TrxHrServiceRequestAttachment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsEmployeeVisible).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.HrServiceRequest).WithMany(x => x.Attachments).HasForeignKey(x => x.HrServiceRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.HrServiceRequestComment).WithMany(x => x.Attachments).HasForeignKey(x => x.HrServiceRequestCommentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UploadedByWorkforceProfile).WithMany().HasForeignKey(x => x.UploadedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.HrServiceRequestId, x.UploadedAt });
            entity.HasIndex(x => x.FileChecksum);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxHrServiceRequestAttachment> entity)
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
