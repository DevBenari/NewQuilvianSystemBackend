using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Metode pengadaan — kamus data pemilihan-penyedia bagian 2. Kode unik di antara baris aktif; service menyimpan
    /// kode dalam huruf besar (pola master Pengadaan). CHECK menjaga batas HPS dan minimum penawaran; aturan "rentang
    /// metode aktif tidak tumpang tindih" melibatkan beberapa baris sehingga dijaga service master (PBJ-DEC-095).
    /// </summary>
    public class MstProcurementMethodConfiguration : IEntityTypeConfiguration<MstProcurementMethod>
    {
        public void Configure(EntityTypeBuilder<MstProcurementMethod> builder)
        {
            builder.ToTable("MstProcurementMethod", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_MstProcurementMethod_MinHpsAmount",
                    "\"MinHpsAmount\" IS NULL OR \"MinHpsAmount\" >= 0");
                table.HasCheckConstraint(
                    "CK_MstProcurementMethod_MaxHpsAmount",
                    "\"MaxHpsAmount\" IS NULL OR \"MaxHpsAmount\" > 0");
                table.HasCheckConstraint(
                    "CK_MstProcurementMethod_HpsRange",
                    "\"MinHpsAmount\" IS NULL OR \"MaxHpsAmount\" IS NULL OR \"MinHpsAmount\" < \"MaxHpsAmount\"");
                table.HasCheckConstraint(
                    "CK_MstProcurementMethod_MinimumQuotationCount",
                    "\"MinimumQuotationCount\" BETWEEN 1 AND 10");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MethodCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.MethodName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.MinHpsAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.MaxHpsAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasIndex(x => x.MethodCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
