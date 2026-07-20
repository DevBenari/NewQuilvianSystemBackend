using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientProcedureConfiguration : IEntityTypeConfiguration<TrxPatientProcedure>
    {
        public void Configure(EntityTypeBuilder<TrxPatientProcedure> entity)
        {
            entity.ToTable("TrxPatientProcedure", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.ConsultationId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.DoctorId)
                .IsRequired();

            entity.Property(x => x.ServiceUnitId)
                .IsRequired(false);

            entity.Property(x => x.ClinicId)
                .IsRequired(false);

            entity.Property(x => x.ProcedureId)
                .IsRequired();

            entity.Property(x => x.TariffId)
                .IsRequired(false);

            entity.Property(x => x.InsuranceTariffId)
                .IsRequired(false);

            entity.Property(x => x.InsuranceCoverageRuleId)
                .IsRequired(false);

            entity.Property(x => x.ProcedureCodeSnapshot)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProcedureNameSnapshot)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.ProcedureTypeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.ProcedureCategoryNameSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.ProcedureMasterType)
                .HasMaxLength(50)
                .HasDefaultValue("Master")
                .IsRequired();

            entity.Property(x => x.IsFromMasterProcedure)
                .HasDefaultValue(true);

            entity.Property(x => x.IsPrimaryProcedure)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEmergencyProcedure)
                .HasDefaultValue(false);

            entity.Property(x => x.IsSurgeryRelated)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPackageProcedure)
                .HasDefaultValue(false);

            entity.Property(x => x.PatientTypeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.PaymentTypeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.PatientClassNameSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.InsuranceProviderNameSnapshot)
                .HasMaxLength(200);

            entity.Property(x => x.BenefitPlanNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.InsuranceTariffCodeSnapshot)
                .HasMaxLength(50);

            entity.Property(x => x.InsuranceTariffNameSnapshot)
                .HasMaxLength(250);

            entity.Property(x => x.ProcedureSource)
                .HasConversion<int>()
                .HasDefaultValue(PatientProcedureSource.DoctorOrder)
                .IsRequired();

            entity.Property(x => x.ProcedureStatus)
                .HasConversion<int>()
                .HasDefaultValue(PatientProcedureStatus.Planned)
                .IsRequired();

            entity.Property(x => x.ProcedureDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.PlannedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ScheduledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.Quantity)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.UnitNameSnapshot)
                .HasMaxLength(50);

            entity.Property(x => x.UnitPrice)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.TotalPrice)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.HospitalPriceSnapshot)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.InsuranceContractPrice)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.IsFreeOfCharge)
                .HasDefaultValue(false);

            entity.Property(x => x.FreeOfChargeReason)
                .HasMaxLength(250);

            entity.Property(x => x.IsBillable)
                .HasDefaultValue(true);

            entity.Property(x => x.IsCoveredByInsurance)
                .HasDefaultValue(false);

            entity.Property(x => x.CoverageStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Unknown")
                .IsRequired();

            entity.Property(x => x.CoveragePercent)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.CoveredAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.PatientPayAmount)
                .HasColumnType("numeric(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.CoverageNote)
                .HasMaxLength(250);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsApproved)
                .HasDefaultValue(false);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ApprovalNote)
                .HasMaxLength(250);

            entity.Property(x => x.IsExecuted)
                .HasDefaultValue(false);

            entity.Property(x => x.ExecutedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ExecutedByUserId)
                .IsRequired(false);

            entity.Property(x => x.PerformedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.PerformedByUserId)
                .IsRequired(false);

            entity.Property(x => x.ClinicalNote)
                .HasMaxLength(1000);

            entity.Property(x => x.ResultNote)
                .HasMaxLength(1000);

            entity.Property(x => x.InstructionNote)
                .HasMaxLength(500);

            entity.Property(x => x.DispositionNote)
                .HasMaxLength(500);

            entity.Property(x => x.ComplicationNote)
                .HasMaxLength(500);

            entity.Property(x => x.FollowUpInstruction)
                .HasMaxLength(500);

            entity.Property(x => x.BillingItemId)
                .IsRequired(false);

            entity.Property(x => x.IsBillingGenerated)
                .HasDefaultValue(false);

            entity.Property(x => x.BillingGeneratedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
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

            entity.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Consultation)
                .WithMany()
                .HasForeignKey(x => x.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

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

            entity.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceTariff)
                .WithMany()
                .HasForeignKey(x => x.InsuranceTariffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceCoverageRule)
                .WithMany()
                .HasForeignKey(x => x.InsuranceCoverageRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ExecutedByUser)
                .WithMany()
                .HasForeignKey(x => x.ExecutedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformedByUser)
                .WithMany()
                .HasForeignKey(x => x.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.ConsultationId);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.DoctorId);

            entity.HasIndex(x => x.ServiceUnitId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => x.ProcedureId);

            entity.HasIndex(x => x.TariffId);

            entity.HasIndex(x => x.InsuranceTariffId);

            entity.HasIndex(x => x.InsuranceCoverageRuleId);

            entity.HasIndex(x => x.ProcedureCodeSnapshot);

            entity.HasIndex(x => x.ProcedureNameSnapshot);

            // Proteksi terakhir terhadap request bersamaan dari dua tab/device.
            // Record yang sudah dibatalkan dibuat IsActive=false sehingga tindakan
            // yang sama tetap dapat dipilih kembali secara sah.
            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.ProcedureId
            })
                .IsUnique()
                .HasDatabaseName("UX_TrxPatientProcedure_Consultation_Procedure_Active")
                .HasFilter("\"IsDelete\" = FALSE AND \"IsActive\" = TRUE");

            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.ProcedureCodeSnapshot,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ConsultationId,
                x.IsPrimaryProcedure,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.ProcedureStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.ProcedureDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DoctorId,
                x.ProcedureDateTime,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ProcedureStatus,
                x.IsBillable,
                x.IsBillingGenerated,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsFreeOfCharge,
                x.IsBillable,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.CoverageStatus,
                x.IsCoveredByInsurance,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsNeedApproval,
                x.IsApproved,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsExecuted,
                x.ExecutedAt,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PerformedAt,
                x.PerformedByUserId,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ServiceUnitId,
                x.ClinicId,
                x.ProcedureDateTime,
                x.ProcedureStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ProcedureMasterType,
                x.IsFromMasterProcedure,
                x.IsPrimaryProcedure,
                x.IsEmergencyProcedure,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsSurgeryRelated,
                x.IsPackageProcedure,
                x.IsDelete
            });

            entity.HasIndex(x => x.ApprovedByUserId);

            entity.HasIndex(x => x.ExecutedByUserId);

            entity.HasIndex(x => x.PerformedByUserId);

            entity.HasIndex(x => x.CancelledByUserId);
        }
    }
}