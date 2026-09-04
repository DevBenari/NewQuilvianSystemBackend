using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstAdministrationFeePolicyConfiguration : IEntityTypeConfiguration<MstAdministrationFeePolicy>
{
    public void Configure(EntityTypeBuilder<MstAdministrationFeePolicy> entity)
    {
        entity.ToTable("MstAdministrationFeePolicy", "public", table =>
        {
            table.HasCheckConstraint("CK_MstAdministrationFeePolicy_Amount", "\"Amount\" >= 0");
            table.HasCheckConstraint("CK_MstAdministrationFeePolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
            table.HasCheckConstraint("CK_MstAdministrationFeePolicy_NotDiscountable", "\"Discountable\" = false");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.ServiceType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.OncePerPatientLocalDay).IsRequired();
        entity.Property(x => x.Discountable).HasDefaultValue(false);
        entity.Property(x => x.IsActive).HasDefaultValue(false);
        entity.Property(x => x.EffectiveFrom).HasColumnType("timestamp with time zone");
        entity.Property(x => x.EffectiveTo).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDelete\" = false");
        entity.HasIndex(x => new { x.ServiceType, x.EffectiveFrom, x.EffectiveTo, x.IsActive, x.IsDelete });

        var seedTime = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc);
        var effectiveFrom = new DateTimeOffset(seedTime);
        entity.HasData(
            Draft(new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b801"), "ADM-RAJAL-DRAFT", "Draft biaya administrasi rawat jalan", "RAJAL", 10, seedTime, effectiveFrom),
            Draft(new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b802"), "ADM-IGD-DRAFT", "Draft biaya administrasi IGD", "IGD", 10, seedTime, effectiveFrom),
            Draft(new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b803"), "ADM-OTC-DRAFT", "Draft biaya administrasi OTC", "OTC", 10, seedTime, effectiveFrom),
            Draft(new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b804"), "ADM-RANAP-DRAFT", "Draft biaya administrasi rawat inap", "RANAP", 100, seedTime, effectiveFrom));
    }

    private static MstAdministrationFeePolicy Draft(Guid id, string code, string name, string serviceType, int priority, DateTime createdAt, DateTimeOffset effectiveFrom) => new()
    {
        Id = id,
        Code = code,
        Name = name,
        ServiceType = serviceType,
        Amount = 0,
        OncePerPatientLocalDay = serviceType != "RANAP",
        ReplacementPriority = priority,
        Coverable = false,
        Discountable = false,
        EffectiveFrom = effectiveFrom,
        EffectiveTo = effectiveFrom.AddSeconds(1),
        IsActive = false,
        CreateDateTime = createdAt,
        CreateBy = Guid.Empty,
        UpdateBy = Guid.Empty,
        DeleteBy = Guid.Empty,
        CancelBy = Guid.Empty,
        IsDelete = false,
        IsCancel = false
    };
}
