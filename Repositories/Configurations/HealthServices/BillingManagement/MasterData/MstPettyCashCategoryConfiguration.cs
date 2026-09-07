using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstPettyCashCategoryConfiguration : IEntityTypeConfiguration<MstPettyCashCategory>
{
    public void Configure(EntityTypeBuilder<MstPettyCashCategory> entity)
    {
        entity.ToTable("MstPettyCashCategory", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.CategoryCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(300);
        entity.Property(x => x.IsActive).HasDefaultValue(true);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.CategoryCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_MstPettyCashCategory_CategoryCode");

        // Data induk awal — 02-backend-architecture.md § "Rencana data master awal".
        // Daftar final dikonfirmasi Finance sebelum diaktifkan di produksi.
        var seedTime = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);
        entity.HasData(
            Seed(new Guid("c8d2e6b3-4a15-4f79-8b21-000000000001"), "TRANSPORT", "Transport", "Ongkos transport kurir dan perjalanan dinas singkat", seedTime),
            Seed(new Guid("c8d2e6b3-4a15-4f79-8b21-000000000002"), "OPERASIONAL", "Operasional", "Keperluan operasional harian rumah sakit", seedTime),
            Seed(new Guid("c8d2e6b3-4a15-4f79-8b21-000000000003"), "KONSUMSI", "Konsumsi", "Konsumsi rapat dan kegiatan internal", seedTime),
            Seed(new Guid("c8d2e6b3-4a15-4f79-8b21-000000000004"), "MAINTENANCE", "Maintenance", "Perbaikan kecil sarana dan prasarana", seedTime),
            Seed(new Guid("c8d2e6b3-4a15-4f79-8b21-000000000005"), "ATK", "Alat Tulis Kantor", "Pembelian alat tulis kantor", seedTime));
    }

    private static MstPettyCashCategory Seed(Guid id, string code, string name, string description, DateTime createdAt) => new()
    {
        Id = id,
        CategoryCode = code,
        CategoryName = name,
        Description = description,
        IsActive = true,
        CreateDateTime = createdAt,
        CreateBy = Guid.Empty,
        UpdateBy = Guid.Empty,
        DeleteBy = Guid.Empty,
        CancelBy = Guid.Empty,
        IsDelete = false,
        IsCancel = false
    };
}
