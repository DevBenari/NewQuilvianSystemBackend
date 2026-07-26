using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class WfpTrainingRecordConfiguration : IEntityTypeConfiguration<WfpTrainingRecord>
    {
        public void Configure(EntityTypeBuilder<WfpTrainingRecord> entity)
        {
            entity.ToTable("WfpTrainingRecord", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreditPoint).HasPrecision(18, 2);
            entity.Property(x => x.IsVerified).HasDefaultValue(false);
            entity.Property(x => x.IsMandatory).HasDefaultValue(false);
            entity.Property(x => x.IsExternalTraining).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.TrainingRecords)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingCatalog)
                .WithMany()
                .HasForeignKey(x => x.TrainingCatalogId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingCategory)
                .WithMany()
                .HasForeignKey(x => x.TrainingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MandatoryTrainingRule)
                .WithMany()
                .HasForeignKey(x => x.MandatoryTrainingRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany()
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkforceProfileId, x.StartDate, x.TrainingType });

            entity.HasIndex(x => x.CertificateNumber)
                .HasFilter("\"CertificateNumber\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.TrainingParticipantId)
                .IsUnique()
                .HasFilter("\"TrainingParticipantId\" IS NOT NULL AND \"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpTrainingRecord> entity)
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
