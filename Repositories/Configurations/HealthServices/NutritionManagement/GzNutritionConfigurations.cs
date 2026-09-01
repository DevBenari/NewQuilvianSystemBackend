using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NutritionManagement;

public class GzNutritionOrderConfiguration : IEntityTypeConfiguration<GzNutritionOrder>
{
    public void Configure(EntityTypeBuilder<GzNutritionOrder> builder)
    {
        builder.ToTable("GzNutritionOrder", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReasonForReferral).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ClosingNote).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => new { x.PatientId, x.RequestedAt });
        builder.HasIndex(x => x.Status);

        // Satu episode rawat inap hanya boleh punya satu order yang masih berjalan
        // (`GIZ002`). Indeks tersaring ini menegakkannya di basis data, bukan hanya di
        // service — sehingga dua permintaan yang tiba bersamaan tetap tidak dapat lolos
        // berdua.
        builder.HasIndex(x => x.EncounterId)
            .IsUnique()
            .HasFilter($"\"Status\" IN ({(int)GzOrderStatus.Requested}, {(int)GzOrderStatus.InProgress}) AND \"IsDelete\" = false");

        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequesterDoctor).WithMany().HasForeignKey(x => x.RequesterDoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssignedWorkforce).WithMany().HasForeignKey(x => x.AssignedWorkforceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GzNutritionCareRecordConfiguration : IEntityTypeConfiguration<GzNutritionCareRecord>
{
    public void Configure(EntityTypeBuilder<GzNutritionCareRecord> builder)
    {
        builder.ToTable("GzNutritionCareRecord", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssessmentNote).HasMaxLength(2000);
        builder.Property(x => x.DiagnosisNote).HasMaxLength(1000);
        builder.Property(x => x.InterventionNote).HasMaxLength(2000);
        builder.Property(x => x.DietPrescription).HasMaxLength(500);
        builder.Property(x => x.IntakeRecallNote).HasMaxLength(2000);
        builder.Property(x => x.EvaluationNote).HasMaxLength(2000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => new { x.NutritionOrderId, x.VisitSequence }).IsUnique();
        builder.HasIndex(x => x.VisitAt);

        builder.HasOne(x => x.NutritionOrder).WithMany(x => x.CareRecords)
            .HasForeignKey(x => x.NutritionOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecordedByWorkforce).WithMany()
            .HasForeignKey(x => x.RecordedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.NutritionDiagnosis).WithMany()
            .HasForeignKey(x => x.NutritionDiagnosisId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProgressNote).WithMany()
            .HasForeignKey(x => x.ProgressNoteId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GzNutritionOrderHistoryConfiguration : IEntityTypeConfiguration<GzNutritionOrderHistory>
{
    public void Configure(EntityTypeBuilder<GzNutritionOrderHistory> builder)
    {
        builder.ToTable("GzNutritionOrderHistory", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);

        builder.HasIndex(x => new { x.NutritionOrderId, x.OccurredAt });

        // Kunci idempotensi. Satu kunci hanya boleh dipakai satu kali per jenis aksi,
        // sehingga permintaan yang terkirim dua kali tidak menghasilkan dua tindakan.
        builder.HasIndex(x => new { x.Action, x.CorrelationId })
            .IsUnique()
            .HasFilter("\"CorrelationId\" IS NOT NULL AND \"IsDelete\" = false");

        builder.HasOne(x => x.NutritionOrder).WithMany(x => x.Histories)
            .HasForeignKey(x => x.NutritionOrderId).OnDelete(DeleteBehavior.Restrict);
    }
}
