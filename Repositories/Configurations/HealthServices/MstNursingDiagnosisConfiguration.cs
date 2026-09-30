using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstNursingDiagnosisConfiguration : IEntityTypeConfiguration<MstNursingDiagnosis>
    {
        public void Configure(EntityTypeBuilder<MstNursingDiagnosis> entity)
        {
            entity.ToTable("MstNursingDiagnosis", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.GroupId)
                .IsRequired(false);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.SubCategory)
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(x => x.Definition)
                .IsRequired(false);

            entity.Property(x => x.TerminologySystem)
                .HasMaxLength(50)
                .HasDefaultValue("SDKI")
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

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasIndex(x => x.Name);

            entity.HasOne(x => x.Group)
                .WithMany(x => x.Diagnoses)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.Etiologies)
                .WithOne(x => x.NursingDiagnosis)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Outcomes)
                .WithOne(x => x.NursingDiagnosis)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Interventions)
                .WithOne(x => x.NursingDiagnosis)
                .HasForeignKey(x => x.NursingDiagnosisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
