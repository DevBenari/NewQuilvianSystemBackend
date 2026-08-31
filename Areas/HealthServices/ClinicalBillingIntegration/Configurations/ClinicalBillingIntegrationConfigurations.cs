using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Configurations
{
    public class TrxClinicalMilestoneFactConfiguration : IEntityTypeConfiguration<TrxClinicalMilestoneFact>
    {
        public void Configure(EntityTypeBuilder<TrxClinicalMilestoneFact> builder)
        {
            builder.ToTable("TrxClinicalMilestoneFact", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SourceContext).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.MilestoneKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.DispatchStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 6);
            builder.Property(x => x.Unit).HasMaxLength(50);
            // Sengaja text, bukan jsonb. Ledger ini adalah bukti apa yang benar-benar dikirim
            // ke Billing, dan sidik jari permintaan dihitung dari untaian karakternya. PostgreSQL
            // memformat ulang nilai jsonb ketika dibaca kembali, sehingga menyimpannya sebagai
            // jsonb membuat baris ini tidak lagi dapat dipakai untuk mengirim ulang secara
            // idempotent — persis kegagalan yang ingin dicegah RJ-BIL-BE-002.
            builder.Property(x => x.TariffSnapshot).HasColumnType("text");
            builder.Property(x => x.RuleSnapshot).HasColumnType("text");
            builder.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            builder.Property(x => x.PayloadFingerprint).HasMaxLength(64).IsRequired();
            builder.Property(x => x.BillingOutcomeCode).HasMaxLength(100);
            builder.Property(x => x.BillingOutcomeMessage).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Satu revisi hanya boleh terbit sekali. Index ini yang membuat retry producer
            // tidak dapat menerbitkan revisi kembar walaupun dua request berjalan bersamaan.
            builder.HasIndex(x => new
                {
                    x.SourceContext,
                    x.MilestoneFactId,
                    x.MilestoneFactVersion,
                    x.EffectType
                })
                .IsUnique();

            // Kunci idempotency bersifat global agar tidak ada dua revisi berbeda yang
            // memakai kunci sama ketika dikirim ke Billing.
            builder.HasIndex(x => x.IdempotencyKey).IsUnique();

            // Penelusuran identitas stabil: mencari revisi terakhir milik satu sumber klinis.
            builder.HasIndex(x => new
                {
                    x.SourceContext,
                    x.SourceAggregateId,
                    x.SourceItemId,
                    x.EffectType
                })
                .HasFilter("\"IsDelete\" = false")
                .AreNullsDistinct(false);

            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.DispatchStatus);

            builder.HasOne<TrxPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
