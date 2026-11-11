using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdOfficialDutyRequestConfiguration : IEntityTypeConfiguration<HrdOfficialDutyRequest>
    {
        public void Configure(EntityTypeBuilder<HrdOfficialDutyRequest> builder)
        {
            builder.ToTable("HrdOfficialDutyRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.DutyCategory).HasMaxLength(50).IsRequired();
            builder.Property(x => x.AssignmentOrderNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.StartAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.EndAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.Destination).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Purpose).IsRequired();
            builder.Property(x => x.Latitude).HasColumnType("decimal(10,8)");
            builder.Property(x => x.Longitude).HasColumnType("decimal(11,8)");
            builder.Property(x => x.AttachmentUrl).HasMaxLength(500);
            builder.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Draft").IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectionReason).HasMaxLength(500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByWorkforce)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByWorkforceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.AssignmentOrderNumber);
        }
    }
}
