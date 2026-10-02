using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.ClinicalManagement
{
    /// <summary>
    /// Bentuk tabel rencana asuhan keperawatan — <c>BE-RWI-059</c>.
    /// </summary>
    public class CliNursingCarePlanConfiguration : IEntityTypeConfiguration<CliNursingCarePlan>
    {
        public void Configure(EntityTypeBuilder<CliNursingCarePlan> entity)
        {
            entity.ToTable("CliNursingCarePlan", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.InpEpisodeId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.OpenedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.OpenedByEmployeeId)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InpEpisode)
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OpenedByEmployee)
                .WithMany()
                .HasForeignKey(x => x.OpenedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Items)
                .WithOne(x => x.CarePlan)
                .HasForeignKey(x => x.CarePlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Satu perawatan tepat satu rencana asuhan - kamus data bagian 3. Penyaringnya baris
            // yang belum terhapus, supaya rencana yang pernah dihapus lunak tidak menahan
            // pembuatan rencana pengganti.
            entity.HasIndex(x => x.InpEpisodeId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.PatientId);
        }
    }
}
