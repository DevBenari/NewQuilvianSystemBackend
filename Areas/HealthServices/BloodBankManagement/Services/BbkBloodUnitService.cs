using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik pembacaan kantong darah operasional (<c>BD-AGG-03</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pada slice <c>BE-BD-004</c> berkas ini hanya membaca.</b> Kantong lahir dari penerimaan
    /// milik <see cref="BbkProviderRequestService"/> — tidak ada jalan lain untuk menambah stok
    /// (<c>VAL-BD-015</c>). Tindakan atas kantong — penyimpanan, alokasi, bukti kecocokan,
    /// pemberian, koreksi, dan penyelesaian <c>PendingReview</c> — lahir di sini bersama task
    /// pemiliknya masing-masing (<c>BE-BD-015</c>, <c>BE-BD-006</c>..<c>BE-BD-010</c>).
    /// </para>
    /// <para>
    /// <b>Nomor kantong PMI sensitif.</b> Ia dikembalikan kepada pengguna yang berhak membaca, tetapi
    /// tidak pernah ditulis ke log.
    /// </para>
    /// </remarks>
    public class BbkBloodUnitService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public BbkBloodUnitService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<BloodUnitListDto>> GetPagedAsync(
            string? search,
            BbkBloodUnitStatus? unitStatus,
            bool? isExcess,
            Guid? providerRequestId,
            Guid? bloodComponentId,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (unitStatus.HasValue)
                query = query.Where(x => x.UnitStatus == unitStatus.Value);

            if (isExcess.HasValue)
                query = query.Where(x => x.IsExcess == isExcess.Value);

            if (providerRequestId.HasValue)
                query = query.Where(x => x.ProviderRequestId == providerRequestId.Value);

            if (bloodComponentId.HasValue)
                query = query.Where(x => x.BloodComponentId == bloodComponentId.Value);

            // Pencarian menyasar nomor kantong, nomor permintaan, nama pasien, dan rekam medis.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PmiBagNumber.ToLower().Contains(keyword) ||
                    (x.ProviderRequest != null && x.ProviderRequest.RequestNumber.ToLower().Contains(keyword)) ||
                    (x.ProviderRequest != null && x.ProviderRequest.Patient != null &&
                     x.ProviderRequest.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.ProviderRequest != null && x.ProviderRequest.Patient != null &&
                     x.ProviderRequest.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodUnitListDto
                {
                    Id = x.Id,
                    PmiBagNumber = x.PmiBagNumber,
                    BloodComponentId = x.BloodComponentId,
                    BloodComponentCode = x.BloodComponent != null ? x.BloodComponent.ComponentCode : null,
                    BloodComponentName = x.BloodComponent != null ? x.BloodComponent.ComponentName : null,
                    UnitStatus = x.UnitStatus,
                    IsExcess = x.IsExcess,
                    ProviderRequestId = x.ProviderRequestId,
                    RequestNumber = x.ProviderRequest != null ? x.ProviderRequest.RequestNumber : null,
                    PatientId = x.ProviderRequest != null ? x.ProviderRequest.PatientId : null,
                    PatientName = x.ProviderRequest != null && x.ProviderRequest.Patient != null
                        ? x.ProviderRequest.Patient.FullName
                        : null,
                    MedicalRecordNumber = x.ProviderRequest != null && x.ProviderRequest.Patient != null
                        ? x.ProviderRequest.Patient.MedicalRecordNumber
                        : null,
                    ReceiptId = x.ReceiptId,
                    ReceivedAt = x.Receipt != null ? x.Receipt.ReceivedAt : null,
                    Version = x.Version,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
                item.UnitStatusLabel = BbkDisplayLabels.Of(item.UnitStatus);

            return new PagedResult<BloodUnitListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<BloodUnitSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new { x.UnitStatus, x.IsExcess })
                .ToListAsync(cancellationToken);

            return new BloodUnitSummaryResponse
            {
                TotalUnit = rows.Count,
                ReceivedUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Received),
                StoredUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Stored),
                AvailableUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Available),
                AllocatedUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Allocated),
                IssuedUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Issued),
                PendingReviewUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.PendingReview),
                ReallocatedUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.Reallocated),
                ReturnedToProviderUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.ReturnedToProvider),
                NotUsableUnit = rows.Count(x => x.UnitStatus == BbkBloodUnitStatus.NotUsable),
                ExcessUnit = rows.Count(x => x.IsExcess)
            };
        }

        public async Task<BloodUnitDetailDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .Include(x => x.BloodComponent)
                .Include(x => x.Receipt)
                .Include(x => x.ProviderRequest)
                    .ThenInclude(x => x!.Patient)
                .Include(x => x.ProviderRequest)
                    .ThenInclude(x => x!.BloodOrder)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return null;

            return new BloodUnitDetailDto
            {
                Id = entity.Id,
                PmiBagNumber = entity.PmiBagNumber,
                BloodComponentId = entity.BloodComponentId,
                BloodComponentCode = entity.BloodComponent?.ComponentCode,
                BloodComponentName = entity.BloodComponent?.ComponentName,
                UnitStatus = entity.UnitStatus,
                UnitStatusLabel = BbkDisplayLabels.Of(entity.UnitStatus),
                IsExcess = entity.IsExcess,
                ProviderRequestId = entity.ProviderRequestId,
                RequestNumber = entity.ProviderRequest?.RequestNumber,
                RequestStatus = entity.ProviderRequest?.RequestStatus,
                RequestStatusLabel = entity.ProviderRequest != null
                    ? BbkDisplayLabels.Of(entity.ProviderRequest.RequestStatus)
                    : null,
                BloodOrderId = entity.ProviderRequest?.BloodOrderId,
                OrderNumber = entity.ProviderRequest?.BloodOrder?.OrderNumber,
                PatientId = entity.ProviderRequest?.PatientId,
                PatientName = entity.ProviderRequest?.Patient?.FullName,
                MedicalRecordNumber = entity.ProviderRequest?.Patient?.MedicalRecordNumber,
                ReceiptId = entity.ReceiptId,
                ReceiptSequence = entity.Receipt?.Sequence,
                ReceivedAt = entity.Receipt?.ReceivedAt,
                ReceivedByUserId = entity.Receipt?.ReceivedByUserId,
                IssuedToPatientId = entity.IssuedToPatientId,
                IssuedAt = entity.IssuedAt,
                IssuedByUserId = entity.IssuedByUserId,
                IssuedViaEmergency = entity.IssuedViaEmergency,
                Version = entity.Version,
                Transitions = await ReadTransitionsAsync(entity.Id, cancellationToken),

                // Tindakan pertama atas kantong Received adalah penyimpanan (BE-BD-015). Sampai
                // tindakan itu ada, tidak ada aksi yang dapat ditawarkan — dan menawarkan aksi
                // tanpa endpoint justru menyesatkan layar.
                AvailableActions = new List<string>(),
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        /// <summary>
        /// Riwayat perpindahan status satu kantong, terlama lebih dulu. Memulangkan <c>null</c>
        /// bila kantongnya tidak ada.
        /// </summary>
        public async Task<List<BloodBankTransitionDto>?> GetStatusHistoryAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var exists = await BaseQuery().AnyAsync(x => x.Id == id, cancellationToken);

            return exists ? await ReadTransitionsAsync(id, cancellationToken) : null;
        }

        public static BloodUnitFilterMetadataResponse BuildFilterMetadata() => new()
        {
            DefaultFilter = new BloodUnitDefaultFilterResponse(),
            UnitStatusOptions = Enum.GetValues<BbkBloodUnitStatus>()
                .Select(x => new BloodBankOptionItemResponse
                {
                    Value = (int)x,
                    Label = BbkDisplayLabels.Of(x)
                })
                .ToList(),
            SortOptions = new List<BloodBankSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Waktu diterima dicatat" },
                new() { Value = "pmiBagNumber", Label = "Nomor kantong PMI" },
                new() { Value = "unitStatus", Label = "Status kantong" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        private async Task<List<BloodBankTransitionDto>> ReadTransitionsAsync(
            Guid unitId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkTransitionHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.Scope == BbkTransitionScopes.BloodUnit &&
                    x.EntityId == unitId &&
                    !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.OccurredAt)
                .Select(x => new BloodBankTransitionDto
                {
                    Id = x.Id,
                    Action = x.Action,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ReasonCode = x.ReasonCode,
                    ReasonNote = x.ReasonNote,
                    ActorUserId = x.ActorUserId,
                    OccurredAt = x.OccurredAt,
                    CorrelationId = x.CorrelationId
                })
                .ToListAsync(cancellationToken);

        private IQueryable<BbkBloodUnit> BaseQuery()
            => _dbContext.Set<BbkBloodUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static IQueryable<BbkBloodUnit> ApplySort(
            IQueryable<BbkBloodUnit> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "pmibagnumber" => descending
                    ? query.OrderByDescending(x => x.PmiBagNumber)
                    : query.OrderBy(x => x.PmiBagNumber),
                "unitstatus" => descending
                    ? query.OrderByDescending(x => x.UnitStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.UnitStatus).ThenBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }
    }
}
