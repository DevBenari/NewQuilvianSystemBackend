using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxClinicalPrivilegeEmergencyOverrideLogConfiguration : IEntityTypeConfiguration<TrxClinicalPrivilegeEmergencyOverrideLog>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalPrivilegeEmergencyOverrideLog> builder)
        {
            builder.ToTable("TrxClinicalPrivilegeEmergencyOverrideLog", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PrivilegeItemCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ProcedureName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.OverrideReason).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.OverrideAtUtc).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.WitnessName).HasMaxLength(150);
            builder.Property(x => x.IsReviewedByKmkp).HasDefaultValue(false);
            builder.Property(x => x.ReviewedAtUtc).HasColumnType("timestamp with time zone");
            builder.Property(x => x.KmkpReviewNotes).HasMaxLength(2000);
            builder.Property(x => x.KmkpVerdict).HasMaxLength(50).HasDefaultValue("PendingReview");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WitnessUser)
                .WithMany()
                .HasForeignKey(x => x.WitnessUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReviewedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.PrivilegeItemCode);
        }
    }
}
