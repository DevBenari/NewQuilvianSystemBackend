using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstDoctorServiceRuleConfiguration : IEntityTypeConfiguration<MstDoctorServiceRule>
    {
        public void Configure(EntityTypeBuilder<MstDoctorServiceRule> entity)
        {
            entity.ToTable("MstDoctorServiceRule", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RuleCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RuleName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.RuleType)
                .HasConversion<int>()
                .HasDefaultValue(DoctorServiceRuleType.GeneralService)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.TariffCategoryId)
                .IsRequired(false);

            entity.Property(x => x.TariffId)
                .IsRequired(false);

            entity.Property(x => x.ProcedureId)
                .IsRequired(false);

            entity.Property(x => x.PatientClassId)
                .IsRequired(false);

            entity.Property(x => x.IsAllowWalkIn)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowAppointment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowKioskRegistration)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowTelemedicine)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedReferral)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPrimaryForClinic)
                .HasDefaultValue(false);

            entity.Property(x => x.IsDefaultForClinic)
                .HasDefaultValue(false);

            entity.Property(x => x.DailyQuotaLimit)
                .HasDefaultValue(0);

            entity.Property(x => x.PriorityLevel)
                .HasDefaultValue(0);

            entity.Property(x => x.RuleStatus)
                .HasConversion<int>()
                .HasDefaultValue(DoctorServiceRuleStatus.Active)
                .IsRequired();

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

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

            entity.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TariffCategory)
                .WithMany()
                .HasForeignKey(x => x.TariffCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.RuleCode)
                .IsUnique();

            entity.HasIndex(x => x.RuleName);

            entity.HasIndex(x => x.RuleType);

            entity.HasIndex(x => x.RuleStatus);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.TariffCategoryId);

            entity.HasIndex(x => x.TariffId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.PatientClassId);

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ServiceUnitId,
                x.ClinicId,
                x.RuleType,
                x.RuleStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.TariffCategoryId,
                x.TariffId,
                x.PatientClassId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.TariffCategoryId,
                x.TariffId,
                x.PatientClassId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ServiceUnitId,
                x.ClinicId,
                x.TariffCategoryId,
                x.TariffId,
                x.ProcedureId,
                x.PatientClassId
            })
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.IsAllowWalkIn,
                x.IsAllowAppointment,
                x.IsAllowKioskRegistration,
                x.IsAllowTelemedicine,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsNeedReferral,
                x.IsNeedApproval,
                x.RuleStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsPrimaryForClinic,
                x.IsDefaultForClinic,
                x.PriorityLevel,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
