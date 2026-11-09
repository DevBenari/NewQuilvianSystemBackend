using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan tindakan aktual sesi — <c>data-dictionary.md</c> bagian 2.13.</summary>
    /// <remarks>
    /// <c>PatientProcedureId</c> unik bila terisi, sehingga satu tindakan bertarif tidak pernah
    /// dirujuk dua intervensi (pencegah tagihan ganda).
    /// </remarks>
    public class RhbSessionInterventionConfiguration : IEntityTypeConfiguration<RhbSessionIntervention>
    {
        public void Configure(EntityTypeBuilder<RhbSessionIntervention> builder)
        {
            builder.ToTable("RhbSessionIntervention", "public", t =>
            {
                t.HasCheckConstraint("CK_RhbSessionIntervention_Quantity", "\"Quantity\" BETWEEN 1 AND 99");
                t.HasCheckConstraint(
                    "CK_RhbSessionIntervention_Duration",
                    "\"DurationMinutes\" IS NULL OR \"DurationMinutes\" BETWEEN 1 AND 600");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActualParametersText).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Quantity).HasDefaultValue(1);

            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.ProcedureId);
            builder.HasIndex(x => x.PatientProcedureId)
                .IsUnique()
                .HasFilter("\"PatientProcedureId\" IS NOT NULL");

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProgramIntervention)
                .WithMany()
                .HasForeignKey(x => x.ProgramInterventionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstProcedure>()
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<TrxPatientProcedure>()
                .WithMany()
                .HasForeignKey(x => x.PatientProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
