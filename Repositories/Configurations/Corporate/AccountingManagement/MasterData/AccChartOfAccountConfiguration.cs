using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.MasterData
{
    public class AccChartOfAccountConfiguration : IEntityTypeConfiguration<AccChartOfAccount>
    {
        public void Configure(EntityTypeBuilder<AccChartOfAccount> entity)
        {
            entity.ToTable("AccChartOfAccount", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.AccountCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.AccountName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.AccountLevel)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.AccountType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.NormalBalance)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.IsPostable)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            // MstLegalEntity dirujuk saja dan tidak disalin. Sengaja memakai WithMany() tanpa
            // navigasi balik supaya entity milik Human Resource tidak perlu disentuh.
            entity.HasOne(x => x.LegalEntity)
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ParentAccount)
                .WithMany(x => x.ChildAccounts)
                .HasForeignKey(x => x.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kode akun unik per badan hukum, bukan unik global (ACC-DEC-037).
            entity.HasIndex(x => new { x.LegalEntityId, x.AccountCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.LegalEntityId, x.AccountName });

            entity.HasIndex(x => x.ParentAccountId);

            entity.HasIndex(x => x.AccountType);
        }
    }
}
