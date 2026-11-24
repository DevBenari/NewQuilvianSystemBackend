using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.PayrollManagement
{
    public class PrlOpeningTaxBalanceConfiguration : IEntityTypeConfiguration<PrlOpeningTaxBalance>
    {
        public void Configure(EntityTypeBuilder<PrlOpeningTaxBalance> entity)
        {
            // Nama dan ekspresi constraint/indeks SAMA PERSIS dengan DDL yang disetujui pemilik (PAY-BE-FIX-015 bagian 9.1). Semua nama
            // paling banyak 57 karakter (batas PostgreSQL 63); nama otomatis EF yang terlalu panjang dipotong dan tidak akan cocok dengan database.
            entity.ToTable("PrlOpeningTaxBalance", "public", table =>
            {
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_Status",
                    "\"Status\" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','SUPERSEDED')");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_EmployerSource",
                    "\"EmployerSource\" IN ('SAME_EMPLOYER_PRE_CUTOVER','PREVIOUS_EMPLOYER')");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_TaxYear", "\"TaxYear\" BETWEEN 2000 AND 2100");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_Version", "\"Version\" >= 1");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_CoveredMonths",
                    "cardinality(\"CoveredMonths\") BETWEEN 1 AND 12 AND \"CoveredMonths\" <@ ARRAY[1,2,3,4,5,6,7,8,9,10,11,12]::integer[]");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_Amounts",
                    "\"TaxableGross\" >= 0 AND \"EmployerPremium\" >= 0 AND \"PensionDeduction\" >= 0 AND \"TaxWithheld\" >= 0");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_ApprovedHasDecision",
                    "\"Status\" <> 'APPROVED' OR (\"ApprovedAt\" IS NOT NULL AND \"ApprovedByUserId\" IS NOT NULL)");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_RejectedHasDecision",
                    "\"Status\" <> 'REJECTED' OR (\"RejectedAt\" IS NOT NULL AND \"RejectedByUserId\" IS NOT NULL AND length(btrim(coalesce(\"DecisionNote\", ''))) > 0)");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_ApprovedHasSource",
                    "\"Status\" <> 'APPROVED' OR length(btrim(coalesce(\"SourceReference\", ''))) > 0");
                table.HasCheckConstraint("CK_PrlOpeningTaxBalance_MakerChecker",
                    "\"ApprovedByUserId\" IS NULL OR \"SubmittedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"SubmittedByUserId\"");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CoveredMonths).HasColumnType("integer[]").IsRequired();
            entity.Property(x => x.EmployerSource).HasMaxLength(40).IsRequired();
            entity.Property(x => x.TaxableGross).HasPrecision(18, 2);
            entity.Property(x => x.EmployerPremium).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.PensionDeduction).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.TaxWithheld).HasPrecision(18, 2);
            entity.Property(x => x.SourceReference).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("DRAFT").IsRequired();
            entity.Property(x => x.Version).HasDefaultValue(1);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DecisionNote).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId)
                .HasConstraintName("FK_PrlOpeningTaxBalance_WorkforceProfile").OnDelete(DeleteBehavior.Restrict);

            // Satu nomor versi per pegawai, tahun, dan sumber.
            entity.HasIndex(x => new { x.WorkforceProfileId, x.TaxYear, x.EmployerSource, x.Version })
                .IsUnique().HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("UX_PrlOpeningTaxBalance_Profile_Year_Source_Version");

            // Hanya satu versi APPROVED per pegawai, tahun, dan sumber (versi lama menjadi SUPERSEDED).
            entity.HasIndex(x => new { x.WorkforceProfileId, x.TaxYear, x.EmployerSource })
                .IsUnique().HasFilter("\"Status\" = 'APPROVED' AND \"IsDelete\" = false")
                .HasDatabaseName("UX_PrlOpeningTaxBalance_OneApprovedPerSource");

            // Mesin hitung memuat saldo APPROVED banyak pegawai sekaligus untuk satu tahun pajak.
            entity.HasIndex(x => new { x.WorkforceProfileId, x.TaxYear, x.Status, x.IsDelete })
                .HasDatabaseName("IX_PrlOpeningTaxBalance_WorkforceProfileId_TaxYear_Status");
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
