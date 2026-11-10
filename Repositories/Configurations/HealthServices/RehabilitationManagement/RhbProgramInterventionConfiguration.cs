using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan intervensi rencana — <c>data-dictionary.md</c> bagian 2.9.</summary>
    public class RhbProgramInterventionConfiguration : IEntityTypeConfiguration<RhbProgramIntervention>
    {
        public void Configure(EntityTypeBuilder<RhbProgramIntervention> builder)
        {
            builder.ToTable("RhbProgramIntervention", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ParametersText).HasMaxLength(1000).IsRequired();

            builder.HasIndex(x => x.ProgramId);
            builder.HasIndex(x => x.ProcedureId);

            builder.HasOne(x => x.Program)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstProcedure>()
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
