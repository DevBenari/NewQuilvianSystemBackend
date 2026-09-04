using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Configurations
{
    public class BilFolioConfiguration : IEntityTypeConfiguration<BilFolio>
    {
        public void Configure(EntityTypeBuilder<BilFolio> builder)
        {
            builder.ToTable("BilFolio", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).IsConcurrencyToken();
            builder.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasOne<TrxPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.ChargeLines)
                .WithOne(x => x.Folio)
                .HasForeignKey(x => x.FolioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class BilChargeLineConfiguration : IEntityTypeConfiguration<BilChargeLine>
    {
        public void Configure(EntityTypeBuilder<BilChargeLine> builder)
        {
            builder.ToTable("BilChargeLine", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SourceContext).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.CalculationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Currency).HasMaxLength(3);
            builder.Property(x => x.GrossAmount).HasPrecision(18, 2);
            builder.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            builder.Property(x => x.ReviewReasonCode).HasMaxLength(100);
            builder.Property(x => x.Version).IsConcurrencyToken();
            builder.HasIndex(x => new
                {
                    x.SourceContext,
                    x.SourceAggregateId,
                    x.SourceItemId,
                    x.MilestoneFactId,
                    x.EffectType
                })
                .IsUnique()
                .AreNullsDistinct(false);
            builder.HasIndex(x => x.FolioId);
            builder.HasMany(x => x.Components)
                .WithOne(x => x.ChargeLine)
                .HasForeignKey(x => x.ChargeLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class BilChargeComponentConfiguration : IEntityTypeConfiguration<BilChargeComponent>
    {
        public void Configure(EntityTypeBuilder<BilChargeComponent> builder)
        {
            builder.ToTable("BilChargeComponent", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ComponentKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 6);
            builder.Property(x => x.Unit).HasMaxLength(50);
            builder.Property(x => x.TariffSnapshot).HasColumnType("jsonb");
            builder.Property(x => x.RuleSnapshot).HasColumnType("jsonb");
            builder.Property(x => x.RoundingSnapshot).HasColumnType("jsonb");
            builder.Property(x => x.CalculatedAmount).HasPrecision(18, 2);
            builder.HasIndex(x => new { x.ChargeLineId, x.ComponentKey }).IsUnique();
        }
    }

    public class BilProcessingEffectConfiguration : IEntityTypeConfiguration<BilProcessingEffect>
    {
        public void Configure(EntityTypeBuilder<BilProcessingEffect> builder)
        {
            builder.ToTable("BilProcessingEffect", "public");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Consumer).HasMaxLength(100).IsRequired();
            builder.Property(x => x.OperationType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            builder.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
            builder.Property(x => x.SourceContext).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Outcome).HasConversion<int>().IsRequired();
            builder.Property(x => x.CalculationStatus).HasConversion<int>();
            builder.Property(x => x.ErrorCode).HasMaxLength(100);
            builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
            builder.HasIndex(x => new { x.Consumer, x.OperationType, x.IdempotencyKey })
                .IsUnique();
            builder.HasIndex(x => new
                {
                    x.SourceContext,
                    x.MilestoneFactId,
                    x.MilestoneFactVersion,
                    x.EffectType
                })
                .IsUnique();
            builder.HasOne<BilFolio>()
                .WithMany()
                .HasForeignKey(x => x.FolioId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<BilChargeLine>()
                .WithMany()
                .HasForeignKey(x => x.ChargeLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
