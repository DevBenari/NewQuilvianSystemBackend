using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan disiplin rehab — <c>data-dictionary.md</c> bagian 2.18.</summary>
    public class RhbDisciplineConfiguration : IEntityTypeConfiguration<RhbDiscipline>
    {
        public void Configure(EntityTypeBuilder<RhbDiscipline> builder)
        {
            builder.ToTable("RhbDiscipline", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisciplineCode).HasMaxLength(40).IsRequired();
            builder.Property(x => x.DisciplineName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(false);

            builder.HasIndex(x => x.DisciplineCode).IsUnique();
            builder.HasIndex(x => x.ProfessionId);

            builder.HasOne<MstProfession>()
                .WithMany()
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
