using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
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
}
