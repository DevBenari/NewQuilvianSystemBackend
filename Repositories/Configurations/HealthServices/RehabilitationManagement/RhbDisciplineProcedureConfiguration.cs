using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan tindakan per disiplin — <c>data-dictionary.md</c> bagian 2.19.</summary>
    public class RhbDisciplineProcedureConfiguration : IEntityTypeConfiguration<RhbDisciplineProcedure>
    {
        public void Configure(EntityTypeBuilder<RhbDisciplineProcedure> builder)
        {
            builder.ToTable("RhbDisciplineProcedure", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => new { x.DisciplineId, x.ProcedureId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.ProcedureId);

            builder.HasOne(x => x.Discipline)
                .WithMany(x => x.Procedures)
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstProcedure>()
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
