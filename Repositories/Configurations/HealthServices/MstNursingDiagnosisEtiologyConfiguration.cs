using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstNursingDiagnosisEtiologyConfiguration : IEntityTypeConfiguration<MstNursingDiagnosisEtiology>
    {
        public void Configure(EntityTypeBuilder<MstNursingDiagnosisEtiology> entity)
        {
            entity.ToTable("MstNursingDiagnosisEtiology", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NursingDiagnosisId)
                .IsRequired();

            entity.Property(x => x.EtiologyName)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired(false);

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

            entity.HasIndex(x => x.NursingDiagnosisId);

            entity.HasOne(x => x.NursingDiagnosis)
                .WithMany(x => x.Etiologies)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
