using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class HrdPlataranSehatSkpRecordConfiguration : IEntityTypeConfiguration<HrdPlataranSehatSkpRecord>
    {
        public void Configure(EntityTypeBuilder<HrdPlataranSehatSkpRecord> builder)
        {
            builder.ToTable("HrdPlataranSehatSkpRecord", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ActivityName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SkpCategory).HasMaxLength(50).HasDefaultValue("Pembelajaran").IsRequired();
            builder.Property(x => x.SkpPoints).HasColumnType("decimal(6,2)").HasDefaultValue(1.00m);
            builder.Property(x => x.ActivityDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.OrganizerName).HasMaxLength(100);
            builder.Property(x => x.PlataranSehatCertificateId).HasMaxLength(100);
            builder.Property(x => x.IsVerifiedByPlataranSehat).HasDefaultValue(false);
            builder.Property(x => x.PlataranSehatVerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsSyncedToOppe).HasDefaultValue(false);
            builder.Property(x => x.OppeSyncedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.EvidenceDocumentUrl).HasMaxLength(1000);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkforceProfileId);
        }
    }
}
