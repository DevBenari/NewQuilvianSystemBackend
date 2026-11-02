using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Daftar Pasien MCU, ringkasan per status, detail, dan riwayat order (<c>BE-MCU-010</c>,
    /// <c>MCU-API-v1</c> grup Mcu Order bagian baca). Baca saja; seluruh kueri <c>AsNoTracking</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>NIK tidak pernah utuh di daftar</b> (<c>MCU-FE-003</c>). Nilai mentah dibaca di dalam
    /// service, dimasking di sini, dan hanya bentuk masking yang keluar. NIK utuh hanya pada detail,
    /// yang dibuka per order oleh pengguna berizin.
    /// </para>
    /// <para>
    /// <b>Kelengkapan <i>n dari m</i></b> (<c>FR-MCU-037</c>): <i>m</i> seluruh komponen order,
    /// <i>n</i> yang sudah tuntas — selesai, atau dibatalkan resmi oleh dokter MCU. Saat
    /// <i>n</i> = <i>m</i>, order boleh berpindah ke <i>Menunggu Resume</i> (<c>MCU-STATE-v1</c>).
    /// </para>
    /// <para>
    /// <b>Relasi opsional dibaca lewat proyeksi bersyarat</b>, bukan <c>Include</c>, sehingga order
    /// peserta berstatus identitas sementara (tanpa pasien) atau tanpa dokter tidak lenyap dari
    /// daftar.
    /// </para>
    /// </remarks>
    public class McuOrderQueryService
    {
        public const string HistoryKindStatus = "Status";
        public const string HistoryKindDoctor = "Doctor";

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public McuOrderQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Metadata dan ringkasan
        // =====================================================================

        public async Task<McuOrderFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken)
        {
            var companyIds = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => !x.IsDelete && x.CompanyGuarantorId != null)
                .Select(x => x.CompanyGuarantorId!.Value)
                .Union(_dbContext.McuPackageCompanyPrices.AsNoTracking()
                    .Where(x => !x.IsDelete)
                    .Select(x => x.CompanyGuarantorId))
                .ToListAsync(cancellationToken);

            var companies = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                .Where(x => companyIds.Contains(x.Id))
                .OrderBy(x => x.CompanyGuarantorName)
                .Select(x => new McuOptionItemResponse { Value = x.Id.ToString(), Label = x.CompanyGuarantorName })
                .ToListAsync(cancellationToken);

            var doctorIds = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => !x.IsDelete && x.McuDoctorId != null)
                .Select(x => x.McuDoctorId!.Value)
                .Union(_dbContext.McuDailySchedules.AsNoTracking()
                    .Where(x => !x.IsDelete && x.DutyMcuDoctorId != null)
                    .Select(x => x.DutyMcuDoctorId!.Value))
                .ToListAsync(cancellationToken);

            var doctors = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .Where(x => doctorIds.Contains(x.Id))
                .OrderBy(x => x.FullName)
                .Select(x => new McuOptionItemResponse { Value = x.Id.ToString(), Label = x.FullName })
                .ToListAsync(cancellationToken);

            return new McuOrderFilterMetadataResponse
            {
                StatusOptions = McuLabels.EnumOptions(McuLabels.OrderStatusFlow, McuLabels.OrderStatus),
                SourceOptions = McuLabels.EnumOptions(Enum.GetValues<McuOrderSource>(), McuLabels.Source),
                CompanyOptions = companies,
                McuDoctorOptions = doctors,
                SortOptions =
                [
                    new() { Value = "orderNumber", Label = "No. Order" },
                    new() { Value = "participantName", Label = "Nama peserta" },
                    new() { Value = "examinationDate", Label = "Tanggal pemeriksaan" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100]
            };
        }

        public async Task<McuOrderSummaryResponse> GetSummaryAsync(McuOrderSummaryQuery query, CancellationToken cancellationToken)
        {
            var date = query.Date ?? DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());

            var counts = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => !x.IsDelete && x.ExaminationDate == date)
                .GroupBy(x => x.OrderStatus)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

            var statuses = McuLabels.OrderStatusFlow
                .Select(x => new McuOrderStatusCountResponse
                {
                    Status = x,
                    StatusLabel = McuLabels.OrderStatus(x),
                    Count = counts.GetValueOrDefault(x)
                })
                .ToList();

            return new McuOrderSummaryResponse
            {
                Date = date,
                Total = statuses.Sum(x => x.Count),
                Statuses = statuses
            };
        }

        // =====================================================================
        // Daftar Pasien MCU
        // =====================================================================

        public async Task<PagedResult<McuOrderListResponse>> GetOrdersAsync(McuOrderPagedQuery query, CancellationToken cancellationToken)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var rows = _dbContext.McuOrders.AsNoTracking().Where(x => !x.IsDelete);

            if (query.Date.HasValue)
                rows = rows.Where(x => x.ExaminationDate == query.Date.Value);
            if (query.Status.HasValue)
                rows = rows.Where(x => x.OrderStatus == query.Status.Value);
            if (query.Source.HasValue)
                rows = rows.Where(x => x.Source == query.Source.Value);
            if (query.CompanyGuarantorId.HasValue)
                rows = rows.Where(x => x.CompanyGuarantorId == query.CompanyGuarantorId.Value);
            if (query.McuDoctorId.HasValue)
                rows = rows.Where(x => x.McuDoctorId == query.McuDoctorId.Value);

            var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim().ToLower();
            if (search != null)
            {
                rows = rows.Where(x => x.ParticipantName.ToLower().Contains(search)
                    || x.OrderNumber.ToLower().Contains(search)
                    || (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(search)));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = query.SortBy?.Trim().ToLowerInvariant() switch
            {
                "participantname" => descending ? rows.OrderByDescending(x => x.ParticipantName) : rows.OrderBy(x => x.ParticipantName),
                "examinationdate" => descending
                    ? rows.OrderByDescending(x => x.ExaminationDate).ThenByDescending(x => x.OrderNumber)
                    : rows.OrderBy(x => x.ExaminationDate).ThenBy(x => x.OrderNumber),
                _ => descending ? rows.OrderByDescending(x => x.OrderNumber) : rows.OrderBy(x => x.OrderNumber)
            };

            var totalData = await rows.CountAsync(cancellationToken);

            var page = await rows
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.ExaminationDate,
                    x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    x.ParticipantName,
                    RawNik = x.ParticipantNik ?? (x.Patient != null ? x.Patient.IdentityNumber : null),
                    x.PackageNameSnapshot,
                    x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                    x.McuDoctorId,
                    McuDoctorName = x.McuDoctor != null ? x.McuDoctor.FullName : null,
                    x.OrderStatus,
                    x.Source,
                    x.Version
                })
                .ToListAsync(cancellationToken);

            var completion = await CompletionAsync(page.Select(x => x.Id).ToList(), cancellationToken);

            var items = page.Select(x =>
            {
                var (resolved, total) = completion.GetValueOrDefault(x.Id);
                return new McuOrderListResponse
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    ExaminationDate = x.ExaminationDate,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    ParticipantName = x.ParticipantName,
                    MaskedNik = McuLabels.MaskNik(x.RawNik),
                    PackageName = x.PackageNameSnapshot,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantorName,
                    McuDoctorId = x.McuDoctorId,
                    McuDoctorName = x.McuDoctorName,
                    CompletedComponentCount = resolved,
                    ComponentCount = total,
                    CompletionLabel = McuLabels.Completion(resolved, total),
                    OrderStatus = x.OrderStatus,
                    OrderStatusLabel = McuLabels.OrderStatus(x.OrderStatus),
                    Source = x.Source,
                    SourceLabel = McuLabels.Source(x.Source),
                    Version = x.Version
                };
            }).ToList();

            return new PagedResult<McuOrderListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =====================================================================
        // Detail dan riwayat
        // =====================================================================

        /// <summary>Komponen order beserta status dan tautan pesanan Lab/Rad (<c>GET orders/{id}/components</c>, detail order).</summary>
        public async Task<List<McuOrderComponentResponse>> GetComponentsAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var components = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => x.McuOrderId == orderId && !x.IsDelete)
                .OrderBy(x => x.ComponentType)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new McuOrderComponentResponse
                {
                    Id = x.Id,
                    ComponentName = x.ComponentNameSnapshot,
                    ComponentType = x.ComponentType,
                    IsPackageCovered = x.IsPackageCovered,
                    ComponentStatus = x.ComponentStatus,
                    ScheduledDate = x.ScheduledDate,
                    LabOrderId = x.LabOrderId,
                    LabOrderNumber = x.LabOrder != null ? x.LabOrder.OrderNumber : null,
                    RadOrderId = x.RadOrderId,
                    RadOrderNumber = x.RadOrder != null ? x.RadOrder.OrderNumber : null,
                    PerformingDoctorName = x.PerformingDoctor != null ? x.PerformingDoctor.FullName : null,
                    CompletedAt = x.CompletedAt,
                    CancelReason = x.CancelReason
                })
                .ToListAsync(cancellationToken);

            foreach (var component in components)
            {
                component.ComponentTypeLabel = McuPackageService.ComponentTypeLabel(component.ComponentType);
                component.ComponentStatusLabel = McuLabels.ComponentStatus(component.ComponentStatus);
            }

            return components;
        }

        public async Task<McuOrderDetailResponse?> GetOrderAsync(Guid id, CancellationToken cancellationToken)
        {
            var order = await _dbContext.McuOrders.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new McuOrderDetailResponse
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    OrderStatus = x.OrderStatus,
                    Version = x.Version,
                    PatientId = x.PatientId,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    ParticipantName = x.ParticipantName,
                    ParticipantNik = x.ParticipantNik ?? (x.Patient != null ? x.Patient.IdentityNumber : null),
                    ParticipantBirthDate = x.ParticipantBirthDate,
                    ParticipantPhone = x.ParticipantPhone,
                    PatientMatchMethod = x.PatientMatchMethod,
                    PackageId = x.PackageId,
                    PackageCode = x.PackageCodeSnapshot,
                    PackageName = x.PackageNameSnapshot,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : null,
                    AppliedTariffId = x.AppliedTariffId,
                    AppliedTariffName = x.AppliedTariff != null ? x.AppliedTariff.TariffName : null,
                    AppliedPrice = x.AppliedTariff != null ? (decimal?)x.AppliedTariff.NormalPrice : null,
                    ClearanceBasis = x.ClearanceBasis,
                    Source = x.Source,
                    SourceEncounterId = x.SourceEncounterId,
                    SourceEncounterNumber = x.SourceEncounter != null ? x.SourceEncounter.EncounterNumber : null,
                    RecommendingDoctorName = x.RecommendingDoctor != null ? x.RecommendingDoctor.FullName : null,
                    BookingImportId = x.BookingImportId,
                    ExaminationDate = x.ExaminationDate,
                    ArrivalConfirmedAt = x.ArrivalConfirmedAt,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    CheckInChannel = x.CheckInChannel,
                    CheckedInAt = x.CheckedInAt,
                    McuDoctorId = x.McuDoctorId,
                    McuDoctorName = x.McuDoctor != null ? x.McuDoctor.FullName : null,
                    ExaminationStartedAt = x.ExaminationStartedAt,
                    NoShowAt = x.NoShowAt,
                    CancelReason = x.CancelReason
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (order == null)
                return null;

            order.OrderStatusLabel = McuLabels.OrderStatus(order.OrderStatus);
            order.SourceLabel = McuLabels.Source(order.Source);
            order.PatientMatchMethodLabel = order.PatientMatchMethod.HasValue ? McuLabels.PatientMatchMethod(order.PatientMatchMethod.Value) : null;
            order.CheckInChannelLabel = order.CheckInChannel.HasValue ? McuLabels.CheckInChannel(order.CheckInChannel.Value) : null;

            // Dasar mulai hanya bermakna sesudah pemeriksaan dimulai (05-screen-brief.md 5.5).
            if (order.ExaminationStartedAt == null)
                order.ClearanceBasis = null;
            order.ClearanceBasisLabel = order.ClearanceBasis.HasValue ? McuLabels.ClearanceBasis(order.ClearanceBasis.Value) : null;

            order.Components = await GetComponentsAsync(id, cancellationToken);

            order.ComponentCount = order.Components.Count;
            order.CompletedComponentCount = order.Components.Count(x => McuLabels.IsComponentResolved(x.ComponentStatus));
            order.CompletionLabel = McuLabels.Completion(order.CompletedComponentCount, order.ComponentCount);

            var checks = await _dbContext.McuOrderRequirementChecks.AsNoTracking()
                .Where(x => x.McuOrderId == id && !x.IsDelete)
                .OrderBy(x => x.CheckedAt)
                .Select(x => new
                {
                    x.Id,
                    x.McuOrderComponentId,
                    ComponentName = x.McuOrderComponent != null ? x.McuOrderComponent.ComponentNameSnapshot : string.Empty,
                    x.RequirementTextSnapshot,
                    x.Outcome,
                    x.Note,
                    x.CheckedAt,
                    x.CheckedByUserId
                })
                .ToListAsync(cancellationToken);

            var users = await UserNamesAsync(checks.Select(x => (Guid?)x.CheckedByUserId), cancellationToken);

            order.RequirementChecks = checks.Select(x => new McuOrderRequirementCheckResponse
            {
                Id = x.Id,
                McuOrderComponentId = x.McuOrderComponentId,
                ComponentName = x.ComponentName,
                RequirementText = x.RequirementTextSnapshot,
                Outcome = x.Outcome,
                OutcomeLabel = McuLabels.RequirementOutcome(x.Outcome),
                Note = x.Note,
                CheckedAt = x.CheckedAt,
                CheckedByName = users.GetValueOrDefault(x.CheckedByUserId)
            }).ToList();

            return order;
        }

        /// <summary>Riwayat status dan penetapan dokter MCU, urut waktu terjadi.</summary>
        public async Task<McuResult<List<McuOrderHistoryResponse>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.McuOrders.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (!exists)
                return McuResult<List<McuOrderHistoryResponse>>.NotFound("Order MCU tidak ditemukan.");

            var statuses = await _dbContext.McuOrderStatusHistories.AsNoTracking()
                .Where(x => x.McuOrderId == id && !x.IsDelete)
                .Select(x => new { x.FromStatus, x.ToStatus, x.ChangedAt, x.ChangedByUserId, x.Reason })
                .ToListAsync(cancellationToken);

            var doctors = await _dbContext.McuOrderDoctorAssignments.AsNoTracking()
                .Where(x => x.McuOrderId == id && !x.IsDelete)
                .Select(x => new
                {
                    x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    x.AssignedAt,
                    x.AssignedByUserId,
                    x.Reason,
                    x.EndedAt
                })
                .ToListAsync(cancellationToken);

            var users = await UserNamesAsync(
                statuses.Select(x => x.ChangedByUserId).Concat(doctors.Select(x => (Guid?)x.AssignedByUserId)),
                cancellationToken);

            var history = statuses
                .Select(x => new McuOrderHistoryResponse
                {
                    Kind = HistoryKindStatus,
                    OccurredAt = x.ChangedAt,
                    FromStatus = x.FromStatus,
                    FromStatusLabel = x.FromStatus.HasValue ? McuLabels.OrderStatus(x.FromStatus.Value) : null,
                    ToStatus = x.ToStatus,
                    ToStatusLabel = McuLabels.OrderStatus(x.ToStatus),
                    ActorUserId = x.ChangedByUserId,
                    ActorName = x.ChangedByUserId.HasValue ? users.GetValueOrDefault(x.ChangedByUserId.Value) : null,
                    Reason = x.Reason
                })
                .Concat(doctors.Select(x => new McuOrderHistoryResponse
                {
                    Kind = HistoryKindDoctor,
                    OccurredAt = x.AssignedAt,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorName,
                    DoctorEndedAt = x.EndedAt,
                    ActorUserId = x.AssignedByUserId,
                    ActorName = users.GetValueOrDefault(x.AssignedByUserId),
                    Reason = x.Reason
                }))
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.Kind == HistoryKindStatus ? 0 : 1)
                .ToList();

            return McuResult<List<McuOrderHistoryResponse>>.Ok(history);
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private async Task<Dictionary<Guid, (int Resolved, int Total)>> CompletionAsync(
            List<Guid> orderIds,
            CancellationToken cancellationToken)
        {
            var components = await _dbContext.McuOrderComponents.AsNoTracking()
                .Where(x => orderIds.Contains(x.McuOrderId) && !x.IsDelete)
                .Select(x => new { x.McuOrderId, x.ComponentStatus })
                .ToListAsync(cancellationToken);

            return components
                .GroupBy(x => x.McuOrderId)
                .ToDictionary(
                    x => x.Key,
                    x => (x.Count(c => McuLabels.IsComponentResolved(c.ComponentStatus)), x.Count()));
        }

        private async Task<Dictionary<Guid, string>> UserNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        {
            var distinct = ids.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
            if (distinct.Count == 0)
                return new Dictionary<Guid, string>();

            return await _dbContext.Users.AsNoTracking()
                .Where(x => distinct.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }
    }
}
