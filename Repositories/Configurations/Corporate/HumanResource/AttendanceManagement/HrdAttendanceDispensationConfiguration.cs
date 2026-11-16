using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdAttendanceDispensationConfiguration : IEntityTypeConfiguration<HrdAttendanceDispensation>
    {
        public void Configure(EntityTypeBuilder<HrdAttendanceDispensation> builder)
        {
            builder.ToTable("HrdAttendanceDispensation", "public", table =>
                table.HasCheckConstraint("CK_HrdAttendanceDispensation_Range", "\"EndAt\" > \"StartAt\""));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AttendanceDate).HasColumnType("date");
            builder.Property(x => x.DispensationType).HasMaxLength(20).IsRequired();
            builder.Property(x => x.StartAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.EndAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Submitted").IsRequired();
            builder.Property(x => x.IsOnBehalf).HasDefaultValue(false);
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.RejectionReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApproverWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ApproverWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.AttendanceDate });
            builder.HasIndex(x => x.Status);
        }
    }
}
