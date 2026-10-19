using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;

public enum NewbornPaymentInheritanceStatus
{
    Success,
    NotFound,
    Conflict,
    Invalid
}

public sealed record NewbornPaymentInheritanceResult(
    NewbornPaymentInheritanceStatus Status,
    string? ErrorMessage,
    RegPatientEncounterGuarantor? Source)
{
    public bool IsSuccess => Status == NewbornPaymentInheritanceStatus.Success && Source != null;
}

/// <summary>
/// Memvalidasi sumber penjamin ibu dan membentuk snapshot baru milik encounter bayi.
/// Kartu pasien ibu tidak pernah dipasang sebagai FK kartu milik bayi.
/// </summary>
public sealed class NewbornEncounterPaymentInheritanceService(
    ApplicationDbContext dbContext,
    PatientEncounterNumberService numberService)
{
    public async Task<NewbornPaymentInheritanceResult> ResolveAsync(
        Guid babyPatientId,
        Guid sourcePaymentId,
        DateTime? expectedSourceAt,
        bool lockSource,
        CancellationToken cancellationToken = default)
    {
        if (sourcePaymentId == Guid.Empty || !expectedSourceAt.HasValue)
        {
            return new(NewbornPaymentInheritanceStatus.Invalid,
                "Sumber penjamin ibu dan versi datanya wajib diisi.", null);
        }

        if (lockSource && dbContext.Database.IsNpgsql())
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM public.\"RegPatientEncounterGuarantor\" WHERE \"Id\" = {sourcePaymentId} FOR SHARE",
                cancellationToken);
        }

        var baby = await dbContext.Set<MstPatient>()
            .AsNoTracking()
            .Where(x => x.Id == babyPatientId && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.IsNewborn, x.MotherPatientId, x.DeliveryCaseId })
            .FirstOrDefaultAsync(cancellationToken);

        if (baby == null || !baby.IsNewborn || !baby.MotherPatientId.HasValue || !baby.DeliveryCaseId.HasValue)
        {
            return new(NewbornPaymentInheritanceStatus.Invalid,
                "Pasien bayi belum memiliki hubungan ibu dan kasus persalinan yang valid.", null);
        }

        var source = await dbContext.Set<RegPatientEncounterGuarantor>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == sourcePaymentId &&
                x.IsActive &&
                !x.IsDelete &&
                !x.IsCancel,
                cancellationToken);

        if (source == null)
        {
            return new(NewbornPaymentInheritanceStatus.NotFound,
                "Sumber penjamin encounter ibu tidak ditemukan atau tidak aktif.", null);
        }

        if (source.PatientId != baby.MotherPatientId.Value)
        {
            return new(NewbornPaymentInheritanceStatus.Invalid,
                "Sumber penjamin tidak berasal dari ibu bayi yang dipilih.", null);
        }

        var deliveryCaseValid = await dbContext.Set<OprCase>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == baby.DeliveryCaseId.Value &&
                x.PatientId == baby.MotherPatientId.Value &&
                x.EncounterId == source.EncounterId &&
                x.SurgicalServiceType == OprSurgicalServiceType.Obstetric &&
                (x.Status == OprCaseStatus.InProgress || x.Status == OprCaseStatus.Completed) &&
                !x.IsDelete &&
                !x.IsCancel,
                cancellationToken);

        if (!deliveryCaseValid)
        {
            return new(NewbornPaymentInheritanceStatus.Invalid,
                "Sumber penjamin tidak sesuai dengan encounter kasus persalinan ibu.", null);
        }

        var sourceVersion = EnsureUtc(source.UpdateDateTime ?? source.CreateDateTime);
        var expectedVersion = EnsureUtc(expectedSourceAt.Value);
        if (Math.Abs((sourceVersion - expectedVersion).TotalMilliseconds) > 1)
        {
            return new(NewbornPaymentInheritanceStatus.Conflict,
                "Penjamin ibu telah berubah. Muat ulang data ibu sebelum melanjutkan admisi bayi.", null);
        }

        return new(NewbornPaymentInheritanceStatus.Success, null, source);
    }

    public async Task<RegPatientEncounterGuarantor> CreateSnapshotAsync(
        Guid encounterId,
        Guid babyPatientId,
        RegPatientEncounterGuarantor source,
        DateTime now,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        return new RegPatientEncounterGuarantor
        {
            Id = Guid.NewGuid(),
            PaymentSourceNumber = await numberService.AllocatePaymentSourceNumberAsync(cancellationToken),
            EncounterId = encounterId,
            PatientId = babyPatientId,
            PaymentType = source.PaymentType,
            PaymentMethodId = source.PaymentMethodId,
            // Kartu asuransi/perusahaan adalah milik ibu, sehingga tidak boleh menjadi FK bayi.
            PatientInsuranceId = null,
            InsuranceProviderId = source.InsuranceProviderId,
            PatientCompanyGuarantorId = null,
            CompanyGuarantorId = source.CompanyGuarantorId,
            PaymentSourceNameSnapshot = source.PaymentSourceNameSnapshot,
            PolicyNumberSnapshot = source.PolicyNumberSnapshot,
            CardNumberSnapshot = source.CardNumberSnapshot,
            MemberNumberSnapshot = source.MemberNumberSnapshot,
            PlanNameSnapshot = source.PlanNameSnapshot,
            ClassNameSnapshot = source.ClassNameSnapshot,
            BenefitPlanCodeSnapshot = source.BenefitPlanCodeSnapshot,
            CompanyGuarantorCodeSnapshot = source.CompanyGuarantorCodeSnapshot,
            EmployeeNumberSnapshot = source.EmployeeNumberSnapshot,
            EmployeeNameSnapshot = source.EmployeeNameSnapshot,
            EffectiveStartDateSnapshot = source.EffectiveStartDateSnapshot,
            EffectiveEndDateSnapshot = source.EffectiveEndDateSnapshot,
            IsEligible = source.IsEligible,
            IsPolicyActive = source.IsPolicyActive,
            Notes = source.Notes,
            EmployeeIdSnapshot = source.EmployeeIdSnapshot,
            FamilyMemberIdSnapshot = source.FamilyMemberIdSnapshot,
            BenefitRelationshipSnapshot = source.BenefitRelationshipSnapshot,
            Priority = source.Priority,
            IsPrimary = source.IsPrimary,
            InheritedFromPaymentSourceId = source.Id,
            InheritedFromPaymentSourceAt = EnsureUtc(source.UpdateDateTime ?? source.CreateDateTime),
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId,
            IsDelete = false,
            IsCancel = false
        };
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
