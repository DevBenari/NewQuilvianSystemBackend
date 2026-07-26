using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class TrxTrainingCertificateConfiguration : IEntityTypeConfiguration<TrxTrainingCertificate>
    {
        public void Configure(EntityTypeBuilder<TrxTrainingCertificate> entity)
        {
            entity.ToTable("TrxTrainingCertificate", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssuedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiredDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsVerified).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.TrainingParticipant)
                .WithMany(x => x.Certificates)
                .HasForeignKey(x => x.TrainingParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingResult)
                .WithMany()
                .HasForeignKey(x => x.TrainingResultId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CertificationType)
                .WithMany()
                .HasForeignKey(x => x.CertificationTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TrainingRecord)
                .WithMany()
                .HasForeignKey(x => x.TrainingRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.CertificateNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.IssuedDate });

            entity.HasIndex(x => x.TrainingResultId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTrainingCertificate> entity)
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
