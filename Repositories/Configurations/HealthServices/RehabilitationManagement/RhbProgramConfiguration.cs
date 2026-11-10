using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan program terapi — <c>data-dictionary.md</c> bagian 2.8.</summary>
    /// <remarks>
    /// <b><c>UX_RhbProgram_ActivePerDiscipline</c></b> menjaga satu versi aktif
    /// (<c>ProgramStatus = 2</c>) per episode per disiplin. <b><c>UX_RhbProgram_Version</c></b>
    /// mencegah dua versi bernomor sama pada disiplin yang sama.
    /// </remarks>
    public class RhbProgramConfiguration : IEntityTypeConfiguration<RhbProgram>
    {
        public void Configure(EntityTypeBuilder<RhbProgram> builder)
        {
            builder.ToTable("RhbProgram", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProgramStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ProgramVersion).HasDefaultValue(1);
            builder.Property(x => x.FrequencyText).HasMaxLength(200);
            builder.Property(x => x.PrecautionText).HasMaxLength(1000);
            builder.Property(x => x.ModificationReason).HasMaxLength(500);
            builder.Property(x => x.StopReason).HasMaxLength(500);
            builder.Property(x => x.NeedsReview).HasDefaultValue(false);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.EpisodeId, x.DisciplineId })
                .IsUnique()
                .HasFilter("\"ProgramStatus\" = 2 AND \"IsDelete\" = false")
                .HasDatabaseName("UX_RhbProgram_ActivePerDiscipline");
            builder.HasIndex(x => new { x.EpisodeId, x.DisciplineId, x.ProgramVersion })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("UX_RhbProgram_Version");
            builder.HasIndex(x => x.ProgramStatus);
            builder.HasIndex(x => x.EvaluationDueDate);
            builder.HasIndex(x => x.SupersedesProgramId);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Programs)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SupersedesProgram)
                .WithMany()
                .HasForeignKey(x => x.SupersedesProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.AuthoredEncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
