using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Configurations
{
    public class MstRadModalityConfiguration : IEntityTypeConfiguration<MstRadModality>
    {
        public void Configure(EntityTypeBuilder<MstRadModality> builder)
        {
            builder.ToTable("MstRadModality", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ModalityCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ModalityName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => x.ModalityCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.IsActive);
        }
    }

    public class MstRadSafetyRequirementConfiguration
        : IEntityTypeConfiguration<MstRadSafetyRequirement>
    {
        public void Configure(EntityTypeBuilder<MstRadSafetyRequirement> builder)
        {
            builder.ToTable("MstRadSafetyRequirement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequirementCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Category).HasMaxLength(50);
            builder.Property(x => x.SourceNote).HasMaxLength(500);

            builder.HasIndex(x => x.RequirementCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.Category, x.SortOrder });
        }
    }

    public class MstRadModalitySafetyRuleConfiguration
        : IEntityTypeConfiguration<MstRadModalitySafetyRule>
    {
        public void Configure(EntityTypeBuilder<MstRadModalitySafetyRule> builder)
        {
            builder.ToTable("MstRadModalitySafetyRule", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Note).HasMaxLength(1000);

            // Satu butir keselamatan hanya boleh punya satu aturan aktif untuk kombinasi
            // modalitas dan pemeriksaan yang sama. Tanpa penjaga ini, dua baris yang saling
            // bertentangan — satu wajib, satu tidak — dapat hidup berdampingan, dan yang
            // menang tinggal soal urutan baris.
            builder.HasIndex(x => new { x.ModalityId, x.ProcedureId, x.SafetyRequirementId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"IsActive\" = true");

            builder.HasIndex(x => new { x.ModalityId, x.IsActive });

            builder.HasOne(x => x.Modality)
                .WithMany(x => x.SafetyRules)
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SafetyRequirement)
                .WithMany()
                .HasForeignKey(x => x.SafetyRequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RadOrderConfiguration : IEntityTypeConfiguration<RadOrder>
    {
        public void Configure(EntityTypeBuilder<RadOrder> builder)
        {
            builder.ToTable("RadOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.ClinicalIndication).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.OrderStatus);
            builder.HasIndex(x => new { x.ModalityId, x.OrderStatus });

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Modality)
                .WithMany()
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Studies)
                .WithOne(x => x.RadOrder)
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RadStudyConfiguration : IEntityTypeConfiguration<RadStudy>
    {
        public void Configure(EntityTypeBuilder<RadStudy> builder)
        {
            builder.ToTable("RadStudy", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StudyNumber).HasMaxLength(64).IsRequired();
            builder.Property(x => x.StudyStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.AbortCause).HasConversion<int>();
            builder.Property(x => x.AbortReason).HasMaxLength(1000);
            builder.Property(x => x.PerformedPortionNote).HasMaxLength(1000);
            builder.Property(x => x.RepeatCause).HasConversion<int>();
            builder.Property(x => x.RepeatReason).HasMaxLength(1000);
            builder.Property(x => x.QualityNote).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.ExternalStudyUid).HasMaxLength(128);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Nomor study wajib unik. Ditegakkan database, bukan hanya di service, agar dua
            // permintaan bersamaan tidak dapat menghasilkan nomor kembar.
            builder.HasIndex(x => x.StudyNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.RadOrderId, x.StudySequence });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.StudyStatus);
            builder.HasIndex(x => x.RepeatOfStudyId);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Modality)
                .WithMany()
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rantai pengulangan. Restrict memastikan study yang gagal tidak dapat dihapus
            // selama masih menjadi asal-usul study penggantinya — tanpa itu, jumlah paparan
            // yang sebenarnya bisa hilang dari catatan.
            builder.HasOne(x => x.RepeatOfStudy)
                .WithMany()
                .HasForeignKey(x => x.RepeatOfStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SafetyChecks)
                .WithOne(x => x.RadStudy)
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Consumptions)
                .WithOne(x => x.RadStudy)
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RadStudySafetyCheckConfiguration : IEntityTypeConfiguration<RadStudySafetyCheck>
    {
        public void Configure(EntityTypeBuilder<RadStudySafetyCheck> builder)
        {
            builder.ToTable("RadStudySafetyCheck", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequirementCodeSnapshot).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequirementNameSnapshot).HasMaxLength(200).IsRequired();
            builder.Property(x => x.CheckState).HasConversion<int>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Satu butir hanya boleh muncul sekali pada satu study. Butir yang sama tercatat
            // dua kali membuat "sudah dijawab" bergantung pada baris mana yang dibaca.
            builder.HasIndex(x => new { x.RadStudyId, x.SafetyRequirementId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.RadStudyId, x.CheckState });

            builder.HasOne(x => x.SafetyRequirement)
                .WithMany()
                .HasForeignKey(x => x.SafetyRequirementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RadAcquisitionConsumptionConfiguration
        : IEntityTypeConfiguration<RadAcquisitionConsumption>
    {
        public void Configure(EntityTypeBuilder<RadAcquisitionConsumption> builder)
        {
            builder.ToTable("RadAcquisitionConsumption", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Quantity).HasPrecision(18, 4);
            builder.Property(x => x.Unit).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasIndex(x => new { x.RadStudyId, x.ItemType });
        }
    }

    public class RadTransitionHistoryConfiguration : IEntityTypeConfiguration<RadTransitionHistory>
    {
        public void Configure(EntityTypeBuilder<RadTransitionHistory> builder)
        {
            builder.ToTable("RadTransitionHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Scope).HasConversion<int>().IsRequired();
            builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(50);
            builder.Property(x => x.ToStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(50);
            builder.Property(x => x.ReasonNote).HasMaxLength(1000);

            builder.HasIndex(x => new { x.RadOrderId, x.OccurredAt });
            builder.HasIndex(x => new { x.RadStudyId, x.OccurredAt });
            builder.HasIndex(x => x.EncounterId);

            builder.HasOne(x => x.RadOrder)
                .WithMany()
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadStudy)
                .WithMany()
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
