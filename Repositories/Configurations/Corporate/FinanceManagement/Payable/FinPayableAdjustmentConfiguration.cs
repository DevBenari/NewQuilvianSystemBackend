using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Payable;

public sealed class FinPayableAdjustmentConfiguration : IEntityTypeConfiguration<FinPayableAdjustment>
{
    public void Configure(EntityTypeBuilder<FinPayableAdjustment> entity)
    {
        entity.ToTable("FinPayableAdjustment", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPayableAdjustment_PayableType", "\"PayableType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
            table.HasCheckConstraint("CK_FinPayableAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
            table.HasCheckConstraint("CK_FinPayableAdjustment_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
            table.HasCheckConstraint("CK_FinPayableAdjustment_Amount", "\"Amount\" > 0");
            // Maker-checker: pengaju tidak boleh menyetujui permohonannya sendiri (FIN-DEC-012).
            table.HasCheckConstraint("CK_FinPayableAdjustment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
            // FIN-DES-016: tepat satu FK terisi — pola num_nonnulls yang sama dengan
            // CK_CliAssessmentInstrumentResponse_OneOwner dan CK_..._TemplateVersionId (precedent lain di repo).
            table.HasCheckConstraint("CK_FinPayableAdjustment_ExactlyOnePayable",
                "num_nonnulls(\"SupplierPayableId\", \"MedicalServicePayableId\") = 1");
            table.HasCheckConstraint("CK_FinPayableAdjustment_PayableTypeMatch",
                "(\"PayableType\" = 'SUPPLIER' AND \"SupplierPayableId\" IS NOT NULL) OR (\"PayableType\" = 'MEDICAL_SERVICE' AND \"MedicalServicePayableId\" IS NOT NULL)");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.PayableType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinPayableAdjustmentStatuses.Requested);
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.SupplierPayable)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.SupplierPayableId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.MedicalServicePayable)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.MedicalServicePayableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AdjustmentNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinPayableAdjustment_Number");
    }
}
