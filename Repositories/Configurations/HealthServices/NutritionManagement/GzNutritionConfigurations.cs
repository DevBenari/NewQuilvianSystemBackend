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

public class GzDietTypeConfiguration : IEntityTypeConfiguration<GzDietType>
{
    public void Configure(EntityTypeBuilder<GzDietType> builder)
    {
        builder.ToTable("GzDietType", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DietTypeCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DietTypeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.DietTypeCode).IsUnique();
    }
}

public class GzFoodFormConfiguration : IEntityTypeConfiguration<GzFoodForm>
{
    public void Configure(EntityTypeBuilder<GzFoodForm> builder)
    {
        builder.ToTable("GzFoodForm", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FoodFormCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FoodFormName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.FoodFormCode).IsUnique();
    }
}

public class GzMealScheduleConfiguration : IEntityTypeConfiguration<GzMealSchedule>
{
    public void Configure(EntityTypeBuilder<GzMealSchedule> builder)
    {
        builder.ToTable("GzMealSchedule", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MealScheduleCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MealScheduleName).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.MealScheduleCode).IsUnique();
    }
}

public class GzPatientDietConfiguration : IEntityTypeConfiguration<GzPatientDiet>
{
    public void Configure(EntityTypeBuilder<GzPatientDiet> builder)
    {
        builder.ToTable("GzPatientDiet", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Instruction).HasMaxLength(1000);
        builder.Property(x => x.ChangeReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => new { x.EncounterId, x.StartAt });

        // Satu KUNJUNGAN hanya boleh punya satu diet aktif pada satu waktu. Dikunci pada
        // encounter, bukan pasien, supaya pasien yang dirawat dua kali tidak saling
        // menghalangi. Ditegakkan di basis data agar dapur tidak pernah menerima dua
        // perintah berbeda, bahkan ketika dua petugas menyimpan bersamaan.
        builder.HasIndex(x => x.EncounterId)
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)GzPatientDietStatus.Active} AND \"IsDelete\" = false");

        builder.HasOne(x => x.NutritionOrder).WithMany().HasForeignKey(x => x.NutritionOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DietType).WithMany().HasForeignKey(x => x.DietTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FoodForm).WithMany().HasForeignKey(x => x.FoodFormId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrescribedByWorkforce).WithMany()
            .HasForeignKey(x => x.PrescribedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GzProductionBatchConfiguration : IEntityTypeConfiguration<GzProductionBatch>
{
    public void Configure(EntityTypeBuilder<GzProductionBatch> builder)
    {
        builder.ToTable("GzProductionBatch", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CancelReason).HasMaxLength(1000);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => x.BatchNumber).IsUnique();
        builder.HasIndex(x => new { x.ServiceDate, x.Status });

        // Satu tanggal dan satu jadwal makan hanya boleh punya satu batch yang tidak
        // dibatalkan. Tanpa ini, dua petugas dapat membuat dua batch untuk waktu makan
        // yang sama dan dapur memasak dua kali.
        builder.HasIndex(x => new { x.ServiceDate, x.MealScheduleId })
            .IsUnique()
            .HasFilter($"\"Status\" <> {(int)GzProductionBatchStatus.Cancelled} AND \"IsDelete\" = false");

        builder.HasOne(x => x.MealSchedule).WithMany()
            .HasForeignKey(x => x.MealScheduleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GzProductionBatchDetailConfiguration : IEntityTypeConfiguration<GzProductionBatchDetail>
{
    public void Configure(EntityTypeBuilder<GzProductionBatchDetail> builder)
    {
        builder.ToTable("GzProductionBatchDetail", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PatientNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MedicalRecordNumberSnapshot).HasMaxLength(50);
        builder.Property(x => x.RoomNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.BedNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.DoctorNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.DietTypeNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FoodFormNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InstructionSnapshot).HasMaxLength(1000);

        // Satu pasien hanya muncul sekali pada satu batch.
        builder.HasIndex(x => new { x.ProductionBatchId, x.EncounterId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasOne(x => x.ProductionBatch).WithMany(x => x.Details)
            .HasForeignKey(x => x.ProductionBatchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany()
            .HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany()
            .HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PatientDiet).WithMany()
            .HasForeignKey(x => x.PatientDietId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GzMealDeliveryConfiguration : IEntityTypeConfiguration<GzMealDelivery>
{
    public void Configure(EntityTypeBuilder<GzMealDelivery> builder)
    {
        builder.ToTable("GzMealDelivery", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Note).HasMaxLength(1000);

        // Satu porsi hanya diserahkan satu kali. Tanpa ini, penekanan tombol dua kali
        // menghasilkan dua catatan penyerahan dan rekap sisa makanan menjadi keliru.
        builder.HasIndex(x => x.ProductionBatchDetailId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

        builder.HasOne(x => x.ProductionBatchDetail).WithMany(x => x.Deliveries)
            .HasForeignKey(x => x.ProductionBatchDetailId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DeliveredByWorkforce).WithMany()
            .HasForeignKey(x => x.DeliveredByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}
