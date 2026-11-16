using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.AttendanceManagement
{
    public class HrdStudyAssignmentConfiguration : IEntityTypeConfiguration<HrdStudyAssignment>
    {
        public void Configure(EntityTypeBuilder<HrdStudyAssignment> builder)
        {
            builder.ToTable("HrdStudyAssignment", "public", table =>
                table.HasCheckConstraint("CK_HrdStudyAssignment_Range", "\"EndDate\" >= \"StartDate\""));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DecreeNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.InstitutionName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ProgramName).HasMaxLength(200);
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();
            builder.Property(x => x.LastChangeReason).HasMaxLength(500);

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

            builder.HasIndex(x => new { x.WorkforceProfileId, x.StartDate, x.EndDate });
        }
    }
}
