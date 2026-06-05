using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstProcedureConfiguration : IEntityTypeConfiguration<MstProcedure>
    {
        public void Configure(EntityTypeBuilder<MstProcedure> entity)
        {
            entity.ToTable("MstProcedure", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProcedureCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProcedureName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ProcedureGroupName)
                .HasMaxLength(100);

            entity.Property(x => x.ProcedureCategoryName)
                .HasMaxLength(100);

            entity.Property(x => x.ProcedureType)
                .HasMaxLength(50)
                .HasDefaultValue("General")
                .IsRequired();

            entity.Property(x => x.IsDoctorAction)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNursingAction)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSurgery)
                .HasDefaultValue(false);

            entity.Property(x => x.IsLaboratory)
                .HasDefaultValue(false);

            entity.Property(x => x.IsRadiology)
                .HasDefaultValue(false);

            entity.Property(x => x.IsTherapy)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedDoctor)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCoveredByInsuranceDefault)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForOutpatient)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForInpatient)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAvailableForEmergency)
                .HasDefaultValue(true);

            entity.Property(x => x.EstimatedDurationMinutes)
                .HasDefaultValue(0);

            entity.Property(x => x.ExternalProcedureCode)
                .HasMaxLength(50);

            entity.Property(x => x.IntegrationCode)
                .HasMaxLength(50);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.ClinicalNoteTemplate)
                .HasMaxLength(500);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasIndex(x => x.ProcedureCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ProcedureName);

            entity.HasIndex(x => x.ProcedureGroupName);

            entity.HasIndex(x => x.ProcedureCategoryName);

            entity.HasIndex(x => x.ProcedureType);

            entity.HasIndex(x => x.ExternalProcedureCode)
                .HasFilter("\"ExternalProcedureCode\" IS NOT NULL");

            entity.HasIndex(x => x.IntegrationCode)
                .HasFilter("\"IntegrationCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.IsDoctorAction,
                x.IsNursingAction,
                x.IsSurgery,
                x.IsLaboratory,
                x.IsRadiology,
                x.IsTherapy,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsAvailableForOutpatient,
                x.IsAvailableForInpatient,
                x.IsAvailableForEmergency,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsCoveredByInsuranceDefault,
                x.IsNeedApproval,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ProcedureType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}