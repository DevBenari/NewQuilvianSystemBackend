using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxRecredentialingApplicationConfiguration : IEntityTypeConfiguration<TrxRecredentialingApplication>
    {
        public void Configure(EntityTypeBuilder<TrxRecredentialingApplication> entity)
        {
            entity.ToTable("TrxRecredentialingApplication", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DueDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.SubmittedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CompletedDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ComplianceSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.ChangeSummaryJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PreviousCredentialingApplication)
                .WithMany()
                .HasForeignKey(x => x.PreviousCredentialingApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CurrentCredentialLicense)
                .WithMany()
                .HasForeignKey(x => x.CurrentCredentialLicenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CurrentCertification)
                .WithMany()
                .HasForeignKey(x => x.CurrentCertificationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CurrentClinicalPrivilege)
                .WithMany()
                .HasForeignKey(x => x.CurrentClinicalPrivilegeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SubmittedByUser)
                .WithMany()
                .HasForeignKey(x => x.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RecredentialingNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.WorkforceProfileId, x.RecredentialingStatus, x.DueDate });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxRecredentialingApplication> entity)
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
