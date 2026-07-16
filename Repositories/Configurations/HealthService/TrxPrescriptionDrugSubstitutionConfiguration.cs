using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPrescriptionDrugSubstitutionConfiguration
        : IEntityTypeConfiguration<TrxPrescriptionDrugSubstitution>
    {
        public void Configure(EntityTypeBuilder<TrxPrescriptionDrugSubstitution> entity)
        {
            entity.ToTable("TrxPrescriptionDrugSubstitution", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PrescriptionId).IsRequired();
            entity.Property(x => x.PrescriptionItemId).IsRequired(false);
            entity.Property(x => x.PrescriptionCompoundItemId).IsRequired(false);
            entity.Property(x => x.OriginalDrugId).IsRequired();
            entity.Property(x => x.SubstituteDrugId).IsRequired();

            entity.Property(x => x.ReasonCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ReasonNote)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.ProposedByPharmacistId).IsRequired();
            entity.Property(x => x.ProposedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(PrescriptionSubstitutionApprovalStatus.Proposed)
                .IsRequired();

            entity.Property(x => x.ApprovedByDoctorId).IsRequired(false);
            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DoctorApprovalNote)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureAuditColumns(entity);

            entity.HasOne(x => x.Prescription)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<TrxPrescriptionCompoundItem>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionCompoundItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.OriginalDrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.SubstituteDrugId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ProposedByPharmacistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ApprovedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.PrescriptionId,
                x.ApprovalStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => x.PrescriptionItemId);
            entity.HasIndex(x => x.PrescriptionCompoundItemId);
            entity.HasIndex(x => x.OriginalDrugId);
            entity.HasIndex(x => x.SubstituteDrugId);
            entity.HasIndex(x => x.ProposedAt);
        }

        private static void ConfigureAuditColumns(
            EntityTypeBuilder<TrxPrescriptionDrugSubstitution> entity)
        {
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
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
