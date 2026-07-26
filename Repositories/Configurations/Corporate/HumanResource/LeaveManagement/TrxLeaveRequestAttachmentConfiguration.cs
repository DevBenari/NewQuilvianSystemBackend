using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LeaveManagement
{
    public class TrxLeaveRequestAttachmentConfiguration : IEntityTypeConfiguration<TrxLeaveRequestAttachment>
    {
        public void Configure(EntityTypeBuilder<TrxLeaveRequestAttachment> entity)
        {
            entity.ToTable("TrxLeaveRequestAttachment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AttachmentType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(150);
            entity.Property(x => x.FileHash).HasMaxLength(128);
            entity.Property(x => x.VerificationStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerificationNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
            entity.HasOne(x => x.LeaveRequest).WithMany(x => x.Attachments).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.LeaveRequestId, x.AttachmentType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.VerificationStatus, x.IsRequiredDocument, x.IsDelete });
        }
    }
}
