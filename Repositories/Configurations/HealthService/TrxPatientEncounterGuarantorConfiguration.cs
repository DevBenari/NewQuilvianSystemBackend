using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxPatientEncounterGuarantorConfiguration : IEntityTypeConfiguration<TrxPatientEncounterGuarantor>
    {
        public void Configure(EntityTypeBuilder<TrxPatientEncounterGuarantor> entity)
        {
            entity.ToTable("TrxPatientEncounterGuarantor", "public");

            entity.HasKey(x => x.Id);

            // =========================
            // PROPERTIES
            // =========================

            entity.Property(x => x.EncounterGuarantorNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EncounterId)
                .IsRequired();

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.GuarantorType)
                .HasConversion<int>()
                .HasDefaultValue(PatientEncounterGuarantorType.PatientCash)
                .IsRequired();

            entity.Property(x => x.GuarantorRole)
                .HasConversion<int>()
                .HasDefaultValue(PatientEncounterGuarantorRole.Primary)
                .IsRequired();

            entity.Property(x => x.GuarantorStatus)
                .HasConversion<int>()
                .HasDefaultValue(PatientEncounterGuarantorStatus.Draft)
                .IsRequired();

            entity.Property(x => x.CheckMethod)
                .HasConversion<int>()
                .HasDefaultValue(PatientEncounterGuarantorCheckMethod.None)
                .IsRequired();

            entity.Property(x => x.CoveragePriority)
                .HasDefaultValue(1)
                .IsRequired();

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(true);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            // =========================
            // REFERENCES
            // =========================

            entity.Property(x => x.PaymentMethodId)
                .IsRequired(false);

            entity.Property(x => x.PatientInsuranceId)
                .IsRequired(false);

            entity.Property(x => x.InsuranceProviderId)
                .IsRequired(false);

            entity.Property(x => x.CompanyGuarantorId)
                .IsRequired(false);

            entity.Property(x => x.PatientCompanyGuarantorId)
                .IsRequired(false);

            entity.Property(x => x.PatientMembershipId)
                .IsRequired(false);

            // =========================
            // SNAPSHOT
            // =========================

            entity.Property(x => x.GuarantorNameSnapshot)
                .HasMaxLength(250);

            entity.Property(x => x.PolicyNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.CardNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.MemberNumberSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.PlanNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.ClassNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.BenefitPlanCodeSnapshot)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveStartDateSnapshot)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDateSnapshot)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            // =========================
            // ELIGIBILITY / VERIFICATION
            // =========================

            entity.Property(x => x.IsEligibilityRequired)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEligible)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.VerifiedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.VerifiedByUserId)
                .IsRequired(false);

            entity.Property(x => x.EligibilityReferenceNumber)
                .HasMaxLength(250);

            entity.Property(x => x.EligibilityCheckedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.VerificationReferenceNumber)
                .HasMaxLength(250);

            entity.Property(x => x.VerificationOfficerName)
                .HasMaxLength(250);

            entity.Property(x => x.VerificationNote)
                .HasMaxLength(500);

            entity.Property(x => x.IsNeedApproval)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedGuaranteeLetter)
                .HasDefaultValue(false);

            entity.Property(x => x.IsNeedReferralLetter)
                .HasDefaultValue(false);

            entity.Property(x => x.IsAllowExcessPaymentByPatient)
                .HasDefaultValue(true);

            // =========================
            // DECIMAL VALUES
            // =========================

            entity.Property(x => x.CoveragePercent)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.AnnualLimitAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.RemainingLimitAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.UsedLimitAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.RoomLimitPerDayAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.DeductibleAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.CoPaymentPercent)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.CoPaymentAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.EstimatedCoveredAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            entity.Property(x => x.EstimatedPatientPayAmount)
                .HasPrecision(18, 2)
                .IsRequired(false);

            // =========================
            // RISK / SPECIAL CONDITION
            // =========================

            entity.Property(x => x.IsPolicyActive)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPremiumPaid)
                .IsRequired(false);

            entity.Property(x => x.IsCardActive)
                .IsRequired(false);

            entity.Property(x => x.IsInWaitingPeriod)
                .IsRequired(false);

            entity.Property(x => x.WaitingPeriodUntilDate)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.HasSpecialExclusion)
                .IsRequired(false);

            entity.Property(x => x.SpecialExclusionNote)
                .HasMaxLength(500);

            entity.Property(x => x.HasPreviousClaim)
                .IsRequired(false);

            entity.Property(x => x.PreviousClaimNote)
                .HasMaxLength(500);

            entity.Property(x => x.BenefitSnapshotJson)
                .HasMaxLength(4000);

            entity.Property(x => x.ManualCheckResultJson)
                .HasMaxLength(4000);

            // =========================
            // CANCEL / NOTES
            // =========================

            entity.Property(x => x.CancelledAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelledByUserId)
                .IsRequired(false);

            entity.Property(x => x.CancelReason)
                .HasMaxLength(250);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            // =========================
            // IDENTITY MODEL
            // =========================

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

            // =========================
            // RELATIONS
            // =========================

            entity.HasOne(x => x.Encounter)
                .WithMany(x => x.EncounterGuarantors)
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PaymentMethod)
                .WithMany()
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientInsurance)
                .WithMany()
                .HasForeignKey(x => x.PatientInsuranceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InsuranceProvider)
                .WithMany()
                .HasForeignKey(x => x.InsuranceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientCompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.PatientCompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PatientMembership)
                .WithMany()
                .HasForeignKey(x => x.PatientMembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // INDEXES
            // =========================

            entity.HasIndex(x => x.EncounterGuarantorNumber)
                .IsUnique();

            entity.HasIndex(x => x.EncounterId);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.PaymentMethodId);

            entity.HasIndex(x => x.PatientInsuranceId);

            entity.HasIndex(x => x.InsuranceProviderId);

            entity.HasIndex(x => x.CompanyGuarantorId);

            entity.HasIndex(x => x.PatientCompanyGuarantorId);

            entity.HasIndex(x => x.PatientMembershipId);

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.CoveragePriority,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EncounterId,
                x.IsPrimary,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.GuarantorType,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.GuarantorType,
                x.GuarantorStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsEligibilityRequired,
                x.IsEligible,
                x.GuarantorStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsNeedApproval,
                x.IsNeedGuaranteeLetter,
                x.IsNeedReferralLetter
            });

            entity.HasIndex(x => new
            {
                x.EligibilityReferenceNumber,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PolicyNumberSnapshot,
                x.CardNumberSnapshot,
                x.MemberNumberSnapshot
            });
        }
    }
}