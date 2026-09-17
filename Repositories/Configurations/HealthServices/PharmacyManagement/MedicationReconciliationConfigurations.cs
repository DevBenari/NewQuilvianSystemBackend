using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

/// <summary>
/// Bentuk tabel obat bawaan pasien — <c>BE-RWI-101</c>, migration <c>R5</c>, kamus data 0.5
/// bagian 13.2.
/// </summary>
public class PhmMedicationReconciliationItemConfiguration : IEntityTypeConfiguration<PhmMedicationReconciliationItem>
{
    public void Configure(EntityTypeBuilder<PhmMedicationReconciliationItem> builder)
    {
        builder.ToTable("PhmMedicationReconciliationItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EncounterId).IsRequired();
        builder.Property(x => x.InpEpisodeId).IsRequired();
        builder.Property(x => x.PatientId).IsRequired();
        builder.Property(x => x.DrugId).IsRequired();
        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Dose).HasColumnType("numeric(12,4)");
        builder.Property(x => x.DrugFormSnapshot).HasMaxLength(100);
        builder.Property(x => x.FrequencyText).HasMaxLength(100);
        builder.Property(x => x.Route).HasConversion<int>().IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.RecordedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.RecordedByEmployeeId).IsRequired();
        builder.Property(x => x.RecordedByUserId).IsRequired();
        builder.Property(x => x.CurrentDecision)
            .HasConversion<int>()
            .HasDefaultValue(ReconciliationDecisionType.Pending)
            .IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.IsDelete).HasDefaultValue(false);
        builder.Property(x => x.IsCancel).HasDefaultValue(false);

        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InpEpisode).WithMany().HasForeignKey(x => x.InpEpisodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DoseUnitMeasurement).WithMany().HasForeignKey(x => x.DoseUnitMeasurementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecordedByEmployee).WithMany().HasForeignKey(x => x.RecordedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecordedByUser).WithMany().HasForeignKey(x => x.RecordedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EncounterId);
        builder.HasIndex(x => x.InpEpisodeId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.DrugId);
        builder.HasIndex(x => x.DoseUnitMeasurementId);
        builder.HasIndex(x => x.RecordedByEmployeeId);
        builder.HasIndex(x => x.RecordedByUserId);
        builder.HasIndex(x => x.RecordedAt);
        builder.HasIndex(x => x.CurrentDecision);

        // Simpan dua kali tidak melahirkan dua baris.
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");
    }
}

/// <summary>
/// Bentuk tabel riwayat keputusan rekonsiliasi — <c>BE-RWI-101</c>, migration <c>R5</c>, kamus
/// data 0.5 bagian 13.3.
/// </summary>
public class PhmMedicationReconciliationDecisionConfiguration : IEntityTypeConfiguration<PhmMedicationReconciliationDecision>
{
    public void Configure(EntityTypeBuilder<PhmMedicationReconciliationDecision> builder)
    {
        builder.ToTable("PhmMedicationReconciliationDecision", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReconciliationItemId).IsRequired();
        builder.Property(x => x.SequenceNumber).IsRequired();
        builder.Property(x => x.DecisionType).HasConversion<int>().IsRequired();
        builder.Property(x => x.DecisionNote).HasMaxLength(500);
        builder.Property(x => x.DecidedByDoctorId).IsRequired();
        builder.Property(x => x.DecidedByUserId).IsRequired();
        builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        builder.Property(x => x.IsDelete).HasDefaultValue(false);
        builder.Property(x => x.IsCancel).HasDefaultValue(false);

        builder.HasOne(x => x.ReconciliationItem)
            .WithMany(x => x.Decisions)
            .HasForeignKey(x => x.ReconciliationItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DecidedByDoctor).WithMany().HasForeignKey(x => x.DecidedByDoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ResultPrescription).WithMany().HasForeignKey(x => x.ResultPrescriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ResultPrescriptionItem).WithMany().HasForeignKey(x => x.ResultPrescriptionItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SupersedesDecision).WithMany().HasForeignKey(x => x.SupersedesDecisionId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ReconciliationItemId, x.SequenceNumber }).IsUnique();
        builder.HasIndex(x => x.DecidedByDoctorId);
        builder.HasIndex(x => x.DecidedByUserId);
        builder.HasIndex(x => x.ResultPrescriptionId);
        builder.HasIndex(x => x.ResultPrescriptionItemId);
        builder.HasIndex(x => x.SupersedesDecisionId);
    }
}
