using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NutritionManagement;

/// <summary>
/// Nilai tetap untuk baris master yang disebut langsung oleh keputusan pemilik proses.
/// </summary>
/// <remarks>
/// Kunci dan stempel waktunya dipatok, bukan dibangkitkan saat migration berjalan. Nilai yang
/// berubah setiap kali dijalankan membuat <c>HasData</c> menganggap barisnya selalu berbeda,
/// sehingga setiap migration berikutnya akan menerbitkan pembaruan yang tidak ada gunanya.
/// </remarks>
internal static class GziSeed
{
    public static readonly DateTime At = new(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid DomainNi = new("9a1e0001-0000-4000-8000-000000000001");
    public static readonly Guid DomainNc = new("9a1e0001-0000-4000-8000-000000000002");
    public static readonly Guid DomainNb = new("9a1e0001-0000-4000-8000-000000000003");

    public static readonly Guid ParamEnergy = new("9a1e0002-0000-4000-8000-000000000001");
    public static readonly Guid ParamProtein = new("9a1e0002-0000-4000-8000-000000000002");
    public static readonly Guid ParamFat = new("9a1e0002-0000-4000-8000-000000000003");
    public static readonly Guid ParamCarbohydrate = new("9a1e0002-0000-4000-8000-000000000004");
    public static readonly Guid ParamFluid = new("9a1e0002-0000-4000-8000-000000000005");
}

public class GziNutritionDiagnosisDomainConfiguration : IEntityTypeConfiguration<GziNutritionDiagnosisDomain>
{
    public void Configure(EntityTypeBuilder<GziNutritionDiagnosisDomain> builder)
    {
        builder.ToTable("GziNutritionDiagnosisDomain", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DomainCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DomainName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasIndex(x => x.DomainCode).IsUnique();

        // Tiga domain ini disebut langsung oleh `GIZ-DEC-011`, jadi mengisinya menjalankan
        // keputusan — bukan menebak. Master diagnosisnya sendiri tetap dibiarkan kosong.
        builder.HasData(
            new GziNutritionDiagnosisDomain
            {
                Id = GziSeed.DomainNi,
                DomainCode = "NI",
                DomainName = "Nutrition Intake",
                Description = "Domain asupan gizi menurut IDNT.",
                SortOrder = 1,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionDiagnosisDomain
            {
                Id = GziSeed.DomainNc,
                DomainCode = "NC",
                DomainName = "Nutrition Clinical",
                Description = "Domain klinis gizi menurut IDNT.",
                SortOrder = 2,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionDiagnosisDomain
            {
                Id = GziSeed.DomainNb,
                DomainCode = "NB",
                DomainName = "Nutrition Behavioral-Environmental",
                Description = "Domain perilaku dan lingkungan menurut IDNT.",
                SortOrder = 3,
                IsActive = true,
                CreateDateTime = GziSeed.At
            });
    }
}

public class GziNutritionDiagnosisConfiguration : IEntityTypeConfiguration<GziNutritionDiagnosis>
{
    public void Configure(EntityTypeBuilder<GziNutritionDiagnosis> builder)
    {
        builder.ToTable("GziNutritionDiagnosis", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiagnosisCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DiagnosisName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Standard).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StandardVersion).HasMaxLength(30);

        builder.HasIndex(x => x.DiagnosisCode).IsUnique();
        builder.HasIndex(x => new { x.DiagnosisDomainId, x.SortOrder });

        builder.HasOne(x => x.DiagnosisDomain).WithMany(x => x.Diagnoses)
            .HasForeignKey(x => x.DiagnosisDomainId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ParentDiagnosis).WithMany(x => x.ChildDiagnoses)
            .HasForeignKey(x => x.ParentDiagnosisId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GziNutritionCareRecordDiagnosisConfiguration : IEntityTypeConfiguration<GziNutritionCareRecordDiagnosis>
{
    public void Configure(EntityTypeBuilder<GziNutritionCareRecordDiagnosis> builder)
    {
        builder.ToTable("GziNutritionCareRecordDiagnosis", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Note).HasMaxLength(1000);

        // Satu diagnosis hanya boleh ditegakkan sekali pada satu kunjungan.
        builder.HasIndex(x => new { x.CareRecordId, x.NutritionDiagnosisId })
            .IsUnique()
            .HasDatabaseName("IX_GziNutritionCareRecordDiagnosis_CareRecord_Diagnosis")
            .HasFilter("\"IsDelete\" = false");

        // Paling banyak satu diagnosis primer per kunjungan (`GIZ018`). Ditegakkan di basis
        // data, bukan hanya di service, sehingga dua permintaan yang tiba bersamaan tetap
        // tidak dapat lolos berdua.
        builder.HasIndex(x => x.CareRecordId)
            .IsUnique()
            .HasDatabaseName("IX_GziNutritionCareRecordDiagnosis_CareRecordId_Primary")
            .HasFilter("\"IsPrimary\" = true AND \"IsDelete\" = false");

        builder.HasOne(x => x.CareRecord).WithMany(x => x.Diagnoses)
            .HasForeignKey(x => x.CareRecordId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.NutritionDiagnosis).WithMany()
            .HasForeignKey(x => x.NutritionDiagnosisId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GziNutritionParameterConfiguration : IEntityTypeConfiguration<GziNutritionParameter>
{
    public void Configure(EntityTypeBuilder<GziNutritionParameter> builder)
    {
        builder.ToTable("GziNutritionParameter", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParameterCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ParameterName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitCode).HasMaxLength(30).IsRequired();

        builder.HasIndex(x => x.ParameterCode).IsUnique();

        // Lima parameter `GIZ-DEC-012`. Batas hanya diisi untuk energi, mengikuti aturan
        // `GIZ006` yang memang sudah disepakati; batas empat sisanya dibiarkan kosong karena
        // belum ditetapkan siapa pun, dan mengarangnya berarti menolak angka yang mungkin benar.
        builder.HasData(
            new GziNutritionParameter
            {
                Id = GziSeed.ParamEnergy,
                ParameterCode = "ENERGY",
                ParameterName = "Energi",
                UnitCode = "kkal/hari",
                ValueScale = 0,
                MinValue = 1m,
                MaxValue = 10000m,
                SortOrder = 1,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionParameter
            {
                Id = GziSeed.ParamProtein,
                ParameterCode = "PROTEIN",
                ParameterName = "Protein",
                UnitCode = "gram/hari",
                ValueScale = 1,
                SortOrder = 2,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionParameter
            {
                Id = GziSeed.ParamFat,
                ParameterCode = "FAT",
                ParameterName = "Lemak",
                UnitCode = "gram/hari",
                ValueScale = 1,
                SortOrder = 3,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionParameter
            {
                Id = GziSeed.ParamCarbohydrate,
                ParameterCode = "CARBOHYDRATE",
                ParameterName = "Karbohidrat",
                UnitCode = "gram/hari",
                ValueScale = 1,
                SortOrder = 4,
                IsActive = true,
                CreateDateTime = GziSeed.At
            },
            new GziNutritionParameter
            {
                Id = GziSeed.ParamFluid,
                ParameterCode = "FLUID",
                ParameterName = "Cairan",
                UnitCode = "ml/hari",
                ValueScale = 0,
                SortOrder = 5,
                IsActive = true,
                CreateDateTime = GziSeed.At
            });
    }
}

public class GziNutritionFormulaConfiguration : IEntityTypeConfiguration<GziNutritionFormula>
{
    public void Configure(EntityTypeBuilder<GziNutritionFormula> builder)
    {
        builder.ToTable("GziNutritionFormula", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FormulaCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FormulaName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FormulaVersion).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.SourceReference).HasMaxLength(500);
        builder.Property(x => x.ImplementationKey).HasMaxLength(100).IsRequired();

        // Satu rumus boleh punya beberapa versi; yang unik adalah pasangannya.
        builder.HasIndex(x => new { x.FormulaCode, x.FormulaVersion }).IsUnique();
    }
}

public class GziNutritionRequirementConfiguration : IEntityTypeConfiguration<GziNutritionRequirement>
{
    public void Configure(EntityTypeBuilder<GziNutritionRequirement> builder)
    {
        builder.ToTable("GziNutritionRequirement", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChangeReason).HasMaxLength(1000);
        builder.Property(x => x.CalculationInput).HasColumnType("jsonb");
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasIndex(x => new { x.NutritionOrderId, x.RevisionNumber }).IsUnique();

        // Satu order hanya boleh punya satu revisi yang berlaku (`GIZ020`). Tanpa indeks ini,
        // dua revisi yang disimpan bersamaan sama-sama mengaku berlaku, dan dapur memilih
        // salah satu tanpa cara mengetahui mana yang benar.
        builder.HasIndex(x => x.NutritionOrderId)
            .IsUnique()
            .HasDatabaseName("IX_GziNutritionRequirement_NutritionOrderId_Current")
            .HasFilter("\"IsCurrent\" = true AND \"IsDelete\" = false");

        builder.HasOne(x => x.NutritionOrder).WithMany()
            .HasForeignKey(x => x.NutritionOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CareRecord).WithMany()
            .HasForeignKey(x => x.CareRecordId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CalculationFormula).WithMany()
            .HasForeignKey(x => x.CalculationFormulaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DeterminedByWorkforce).WithMany()
            .HasForeignKey(x => x.DeterminedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GziNutritionRequirementItemConfiguration : IEntityTypeConfiguration<GziNutritionRequirementItem>
{
    public void Configure(EntityTypeBuilder<GziNutritionRequirementItem> builder)
    {
        builder.ToTable("GziNutritionRequirementItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdjustmentReason).HasMaxLength(1000);

        builder.HasIndex(x => new { x.NutritionRequirementId, x.NutritionParameterId })
            .IsUnique()
            .HasDatabaseName("IX_GziNutritionRequirementItem_Requirement_Parameter")
            .HasFilter("\"IsDelete\" = false");

        builder.HasOne(x => x.NutritionRequirement).WithMany(x => x.Items)
            .HasForeignKey(x => x.NutritionRequirementId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.NutritionParameter).WithMany()
            .HasForeignKey(x => x.NutritionParameterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AdjustedByWorkforce).WithMany()
            .HasForeignKey(x => x.AdjustedByWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}
