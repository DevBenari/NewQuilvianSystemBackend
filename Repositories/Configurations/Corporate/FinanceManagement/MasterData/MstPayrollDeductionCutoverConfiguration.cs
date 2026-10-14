using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstPayrollDeductionCutoverConfiguration : IEntityTypeConfiguration<MstPayrollDeductionCutover>
{
    public void Configure(EntityTypeBuilder<MstPayrollDeductionCutover> entity)
    {
        entity.ToTable("MstPayrollDeductionCutover", "public", table =>
        {
            table.HasCheckConstraint("CK_MstPayrollDeductionCutover_Period", "\"AutomaticFromPeriod\" ~ '^\\d{4}-\\d{2}$'");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.AutomaticFromPeriod).HasMaxLength(7).IsRequired();
        entity.Property(x => x.ChangeReason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Hanya satu baris aktif — dijaga unique index terfilter
        entity.HasIndex(x => x.IsActive)
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_MstPayrollDeductionCutover_Active");
    }
}

