using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan disiplin yang diminta — <c>data-dictionary.md</c> bagian 2.2.</summary>
    public class RhbRequestDisciplineConfiguration : IEntityTypeConfiguration<RhbRequestDiscipline>
    {
        public void Configure(EntityTypeBuilder<RhbRequestDiscipline> builder)
        {
            builder.ToTable("RhbRequestDiscipline", "public");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.RequestId, x.DisciplineId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.DisciplineId);

            builder.HasOne(x => x.Request)
                .WithMany(x => x.Disciplines)
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Discipline)
                .WithMany()
                .HasForeignKey(x => x.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
