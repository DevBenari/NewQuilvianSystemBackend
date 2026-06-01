using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpTrainingRecordConfiguration : IEntityTypeConfiguration<WfpTrainingRecord>
    {
        public void Configure(EntityTypeBuilder<WfpTrainingRecord> entity)
        {
            entity.ToTable("WfpTrainingRecord", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequirementCode)
                .HasMaxLength(100);

            entity.Property(x => x.TrainingType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.TrainingName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Organizer)
                .HasMaxLength(200);

            entity.Property(x => x.Location)
                .HasMaxLength(200);

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.CertificateNumber)
                .HasMaxLength(100);

            entity.Property(x => x.CreditPoint)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.TrainingRecords)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.RequirementCode,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.TrainingType,
                x.StartDate,
                x.IsDelete
            });
        }
    }
}
