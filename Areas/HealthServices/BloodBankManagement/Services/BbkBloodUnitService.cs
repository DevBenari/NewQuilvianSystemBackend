using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik pembacaan dan tindakan atas kantong darah operasional (<c>BD-AGG-03</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kantong lahir dari penerimaan</b> milik <see cref="BbkProviderRequestService"/> — tidak ada
    /// jalan lain untuk menambah stok (<c>VAL-BD-015</c>).
    /// </para>
    /// <para>
    /// <b>Sejak <c>BE-BD-015</c> berkas ini juga menyimpan dan memindahkan kantong</b>, beserta
    /// gerbang alokasi yang menjawab satu pertanyaan penyimpanan (<see cref="EvaluateAllocationGateAsync"/>).
    /// Tindakan lain — alokasi, bukti kecocokan, pemberian, koreksi, penyelesaian
    /// <c>PendingReview</c> — lahir di sini bersama task pemiliknya (<c>BE-BD-006</c>..<c>BE-BD-010</c>).
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
        private const int NoteMaxLength = 500;

        /// <summary>Nama tindakan pada <c>BbkTransitionHistory</c>.</summary>
        private const string StoreAction = "Store";
        private const string MakeAvailableAction = "MakeAvailable";
        private const string HoldForReviewAction = "HoldForReview";

        /// <summary>Nama aksi pada <c>AvailableActions</c>.</summary>
        public const string AssignStorageLocationActionName = "AssignStorageLocation";
        public const string MoveStorageLocationActionName = "MoveStorageLocation";

        private const string NotFoundMessage = "Kantong darah tidak ditemukan atau sudah dihapus.";

        private const string ActorUnknownMessage =
            "Petugas pelaku tidak dikenali. Masuk kembali lalu ulangi tindakan ini.";

        private const string ConcurrencyMessage =
            "Kantong ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini.";

        private const string LocationRequiredMessage = "Lokasi penyimpanan wajib dipilih.";

        private const string LocationNotFoundMessage =
            "Lokasi penyimpanan tidak ditemukan atau sudah dihapus. Pilih lokasi dari daftar yang aktif.";

        private const string NoteTooLongMessage = "Keterangan paling banyak 500 karakter.";

        /// <summary>
        /// Keadaan <i>fail-closed</i> (<c>INV-BD-025</c>): tanpa satu pun lokasi aktif, tidak ada
        /// kantong yang dapat disimpan. Bunyinya diturunkan dari <c>FE-BD-014</c>.
        /// </summary>
        private const string NoActiveLocationMessage =
            "Belum ada lokasi penyimpanan darah yang aktif. Tambahkan atau aktifkan minimal satu lokasi " +
            "pada Setup Bank Darah sebelum kantong dapat disimpan.";

        // Pesan kanonis matriks validasi §4b, persis.
        private const string Val060Message =
            "Lokasi penyimpanan itu sudah tidak aktif dan tidak dapat dipilih. Pilih lokasi lain yang masih aktif.";

        private const string Val061Message =
            "Kantong ini sudah punya lokasi penyimpanan. Gunakan perpindahan lokasi bila ingin memindahkannya.";

        private const string Val062Message =
            "Kantong ini belum punya lokasi penyimpanan. Tetapkan lokasinya lebih dulu.";

        private const string Val063Message =
            "Kantong belum disimpan pada lokasi penyimpanan, sehingga belum dapat dialokasikan. " +
            "Tetapkan lokasi penyimpanannya lebih dulu.";

        private const string Val064Message =
            "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif. " +
            "Pindahkan dulu ke lokasi yang aktif sebelum dialokasikan.";

        /// <summary>Alasan sistem memasukkan kantong ke <c>PendingReview</c> (<c>DEC-BD-025</c>).</summary>
        private const string ExcessHoldNote = "Kiriman melebihi permintaan.";

        /// <summary>Alasan sistem memasukkan kantong ke <c>PendingReview</c> (<c>DEC-BD-020</c>).</summary>
        private const string ClosedEncounterHoldNote =
            "Permintaan asal sudah ditutup karena kunjungan pasien berakhir.";

        /// <summary>
        /// Status kantong yang masih berada di stok — boleh dipindahkan, dan terhitung tertahan
        /// ketika lokasinya dinonaktifkan (<c>state-transition-matrix.md</c> §3).
        /// </summary>
        /// <remarks>
        /// <see cref="BbkBloodUnitStatus.Received"/> tidak termasuk karena belum punya lokasi.
        /// <see cref="BbkBloodUnitStatus.Issued"/>, <see cref="BbkBloodUnitStatus.ReturnedToProvider"/>,
        /// dan <see cref="BbkBloodUnitStatus.NotUsable"/> adalah status akhir — kantongnya sudah keluar.
        /// </remarks>
        public static readonly BbkBloodUnitStatus[] StillInStockStatuses =
        {
            BbkBloodUnitStatus.Stored,
            BbkBloodUnitStatus.Available,
            BbkBloodUnitStatus.Allocated,
            BbkBloodUnitStatus.PendingReview,
            BbkBloodUnitStatus.Reallocated
        };

        private readonly ApplicationDbContext _dbContext;

        public BbkBloodUnitService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

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
                    CurrentPlacementId = x.CurrentPlacementId,
                    CurrentStorageLocationId = x.CurrentPlacement != null
                        ? x.CurrentPlacement.StorageLocationId
                        : null,
                    CurrentStorageLocationCode = x.CurrentPlacement != null && x.CurrentPlacement.StorageLocation != null
                        ? x.CurrentPlacement.StorageLocation.StorageLocationCode
                        : null,
                    CurrentStorageLocationName = x.CurrentPlacement != null && x.CurrentPlacement.StorageLocation != null
                        ? x.CurrentPlacement.StorageLocation.StorageLocationName
                        : null,
                    IsCurrentStorageLocationActive = x.CurrentPlacement != null && x.CurrentPlacement.StorageLocation != null
                        ? x.CurrentPlacement.StorageLocation.IsActive && !x.CurrentPlacement.StorageLocation.IsDelete
                        : null,
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
                .Include(x => x.CurrentPlacement)
                    .ThenInclude(x => x!.StorageLocation)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return null;

            var location = entity.CurrentPlacement?.StorageLocation;

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
                CurrentPlacementId = entity.CurrentPlacementId,
                CurrentStorageLocationId = entity.CurrentPlacement?.StorageLocationId,
                CurrentStorageLocationCode = location?.StorageLocationCode,
                CurrentStorageLocationName = location?.StorageLocationName,
                IsCurrentStorageLocationActive = location != null ? location.IsActive && !location.IsDelete : null,
                CurrentPlacedAt = entity.CurrentPlacement?.PlacedAt,
                Version = entity.Version,
                Transitions = await ReadTransitionsAsync(entity.Id, cancellationToken),
                AvailableActions = AvailableActionsFor(entity.UnitStatus, entity.CurrentPlacementId),
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

        /// <summary>
        /// Riwayat penempatan satu kantong, terlama lebih dulu. Memulangkan <c>null</c> bila
        /// kantongnya tidak ada; daftar kosong bila kantong belum pernah disimpan.
        /// </summary>
        /// <remarks>
        /// Lokasi yang sudah dinonaktifkan maupun dihapus <b>tetap</b> terbaca pada riwayat —
        /// penyaringan lokasi aktif hanya berlaku bagi tujuan penempatan baru.
        /// </remarks>
        public async Task<List<BloodUnitPlacementDto>?> GetPlacementsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var exists = await BaseQuery().AnyAsync(x => x.Id == id, cancellationToken);

            if (!exists)
                return null;

            return await _dbContext.Set<BbkBloodUnitPlacement>()
                .AsNoTracking()
                .Where(x => x.BloodUnitId == id && !x.IsDelete)
                .OrderBy(x => x.PlacedAt)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new BloodUnitPlacementDto
                {
                    Id = x.Id,
                    BloodUnitId = x.BloodUnitId,
                    StorageLocationId = x.StorageLocationId,
                    StorageLocationCode = x.StorageLocation != null ? x.StorageLocation.StorageLocationCode : null,
                    StorageLocationName = x.StorageLocation != null ? x.StorageLocation.StorageLocationName : null,
                    IsStorageLocationActive = x.StorageLocation != null &&
                                              x.StorageLocation.IsActive &&
                                              !x.StorageLocation.IsDelete,
                    PreviousPlacementId = x.PreviousPlacementId,
                    PreviousStorageLocationId = x.PreviousPlacement != null
                        ? x.PreviousPlacement.StorageLocationId
                        : null,
                    PreviousStorageLocationCode = x.PreviousPlacement != null && x.PreviousPlacement.StorageLocation != null
                        ? x.PreviousPlacement.StorageLocation.StorageLocationCode
                        : null,
                    PreviousStorageLocationName = x.PreviousPlacement != null && x.PreviousPlacement.StorageLocation != null
                        ? x.PreviousPlacement.StorageLocation.StorageLocationName
                        : null,
                    PlacedAt = x.PlacedAt,
                    PlacedByUserId = x.PlacedByUserId,
                    IsCurrent = x.IsCurrent,
                    Note = x.Note
                })
                .ToListAsync(cancellationToken);
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

        // =================================================================
        // Gerbang alokasi — ARCH-BD-POS-06
        // =================================================================

        /// <summary>
        /// Menjawab satu pertanyaan: kantong ini sudah melewati <c>Stored</c> <b>dan</b> penempatan
        /// terakhirnya menunjuk lokasi yang sedang aktif? Memulangkan <c>null</c> bila kantongnya
        /// tidak ada.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Gerbang ini hanya membaca.</b> Ia tidak mengalokasikan apa pun; pemakainya adalah
        /// endpoint <c>allocate</c> dan <c>reallocate</c> yang lahir pada <c>BE-BD-006</c> dan
        /// <c>BE-BD-009</c> (<c>02-backend-architecture.md</c> §F.4). Syarat alokasi lain — order aktif,
        /// tidak ada alokasi aktif lain — milik task itu, bukan pertanyaan penyimpanan.
        /// </para>
        /// <para>
        /// <b>Dinilai saat ditanya, tidak pernah dari salinan.</b> Keaktifan lokasi dibaca dari
        /// master pada detik ini (<c>INV-BD-028</c>). Menonaktifkan satu kulkas karena itu cukup satu
        /// <c>UPDATE</c> pada satu baris master, dan gerbang seluruh kantong di dalamnya tertutup pada
        /// saat yang sama — tanpa job, tanpa penyuntingan kantong.
        /// </para>
        /// <para>
        /// <b>Contoh.</b> Kantong baru datang pukul 08.00 → tertutup <c>VAL-BD-063</c>. Ditaruh di
        /// Kulkas Besar pukul 08.10 → terbuka. Kulkas Besar dinonaktifkan pukul 13.00 → tertutup
        /// <c>VAL-BD-064</c>. Dipindahkan ke Kulkas Kecil pukul 13.20 → terbuka kembali.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitAllocationGateResult?> EvaluateAllocationGateAsync(
            Guid unitId,
            CancellationToken cancellationToken = default)
        {
            var unit = await BaseQuery()
                .Where(x => x.Id == unitId)
                .Select(x => new
                {
                    x.UnitStatus,
                    x.CurrentPlacementId,
                    IsLocationActive = x.CurrentPlacement != null &&
                                       x.CurrentPlacement.StorageLocation != null &&
                                       x.CurrentPlacement.StorageLocation.IsActive &&
                                       !x.CurrentPlacement.StorageLocation.IsDelete
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (unit == null)
                return null;

            if (unit.UnitStatus == BbkBloodUnitStatus.Received || !unit.CurrentPlacementId.HasValue)
                return new BloodUnitAllocationGateResult(false, "VAL-BD-063", Val063Message);

            if (!unit.IsLocationActive)
                return new BloodUnitAllocationGateResult(false, "VAL-BD-064", Val064Message);

            return new BloodUnitAllocationGateResult(
                true,
                null,
                "Kantong sudah tersimpan pada lokasi penyimpanan yang aktif.");
        }

        // =================================================================
        // Penyimpanan — BE-BD-015
        // =================================================================

        /// <summary>
        /// Menetapkan lokasi penyimpanan <b>pertama</b>: <c>Received</c> → <c>Stored</c> →
        /// <c>Available</c>, atau <c>PendingReview</c> bila kantongnya berlebih atau permintaan
        /// asalnya sudah <c>ClosedEncounter</c> (<c>DEC-BD-036</c>, matriks §3).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b><c>Stored</c> dilewati, bukan disinggahi.</b> Matriks menetapkan <c>Stored</c> →
        /// <c>Available</c> sebagai akibat penempatan tanpa tindakan manusia tambahan
        /// (<c>ARCH-BD-POS-04</c>). Karena itu kedua perpindahan dicatat pada riwayat, tetapi status
        /// yang tersimpan sesudah tindakan ini selalu <c>Available</c> atau <c>PendingReview</c>.
        /// </para>
        /// <para>
        /// <b>Kantong berlebih tetap wajib disimpan lebih dulu</b> sebelum menunggu keputusan — ia
        /// fisik ada di suatu kulkas, dan riwayat harus tahu yang mana (matriks §3 baris
        /// <c>Stored</c> → <c>PendingReview</c>).
        /// </para>
        /// </remarks>
        public async Task<BloodUnitResult> AssignStorageLocationAsync(
            Guid id,
            AssignStorageLocationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var inputFailure = ValidateInput(request.StorageLocationId, request.Note, actorUserId);

            if (inputFailure != null)
                return inputFailure;

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            // VAL-BD-061: penetapan pertama hanya sekali. Perpindahan punya endpoint sendiri.
            if (unit.CurrentPlacementId.HasValue)
                return Failed(BloodUnitOutcome.NotAllowedByState, Val061Message);

            if (unit.UnitStatus != BbkBloodUnitStatus.Received)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    $"Kantong berstatus {BbkDisplayLabels.Of(unit.UnitStatus)} tidak dapat ditetapkan lokasi penyimpanannya.");
            }

            var destinationFailure = await CheckDestinationAsync(request.StorageLocationId, cancellationToken);

            if (destinationFailure != null)
                return destinationFailure;

            var requestStatus = await _dbContext.Set<BbkProviderRequest>()
                .AsNoTracking()
                .Where(x => x.Id == unit.ProviderRequestId)
                .Select(x => (BbkProviderRequestStatus?)x.RequestStatus)
                .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;

            var placement = NewPlacement(unit.Id, request.StorageLocationId, previousPlacementId: null, request.Note, actorUserId, now);

            _dbContext.Set<BbkBloodUnitPlacement>().Add(placement);

            // Sebab masuk PendingReview, bila ada. Kelebihan lebih dulu disebut karena ia melekat
            // pada kantong itu sendiri; penutupan permintaan melekat pada asalnya.
            var holdNote = unit.IsExcess
                ? ExcessHoldNote
                : requestStatus == BbkProviderRequestStatus.ClosedEncounter
                    ? ClosedEncounterHoldNote
                    : null;

            var finalStatus = holdNote != null ? BbkBloodUnitStatus.PendingReview : BbkBloodUnitStatus.Available;

            unit.UnitStatus = finalStatus;
            unit.CurrentPlacementId = placement.Id;
            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            AppendTransition(
                unit.Id,
                StoreAction,
                BbkBloodUnitStatus.Received,
                BbkBloodUnitStatus.Stored,
                reasonNote: null,
                actorUserId,
                occurredAt: now,
                recordedAt: now,
                correlationId: placement.Id);

            // Perpindahan kedua terjadi pada detik yang sama sebagai akibat, bukan tindakan baru.
            // Satu tick pada waktu pencatatan menjaga urutan bacanya tetap Store lebih dulu.
            AppendTransition(
                unit.Id,
                holdNote != null ? HoldForReviewAction : MakeAvailableAction,
                BbkBloodUnitStatus.Stored,
                finalStatus,
                reasonNote: holdNote,
                actorUserId,
                occurredAt: now,
                recordedAt: now.AddTicks(1),
                correlationId: placement.Id);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(
                unit,
                holdNote != null
                    ? "Kantong berhasil disimpan dan masuk daftar menunggu keputusan."
                    : "Kantong berhasil disimpan dan kini tersedia.");
        }

        /// <summary>
        /// Memindahkan kantong ke lokasi lain. <b>Status kantong tidak berubah</b>, dan penempatan
        /// lama tetap tersimpan (<c>INV-BD-026</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Satu transaksi, dua langkah, dikawal <c>Version</c>.</b> Langkah pertama memadamkan
        /// penempatan lama dan menaikkan <c>Version</c> kantong; langkah kedua menambah penempatan baru
        /// dan memindahkan <c>CurrentPlacementId</c>. Urutan itu disengaja: index unik terfilter
        /// menolak dua penempatan berlaku sekaligus, sehingga penempatan lama harus padam lebih dulu.
        /// Dua petugas yang memindahkan kantong yang sama hampir bersamaan berebut baris kantong
        /// yang sama; yang kalah menemukan <c>Version</c> sudah berubah dan menerima <c>409</c>,
        /// sementara transaksinya batal seluruhnya.
        /// </para>
        /// <para>
        /// <b>Berlaku juga ketika lokasi asal sudah nonaktif</b> — inilah jalan keluar kantong dari
        /// kulkas yang rusak (<c>DEC-BD-037</c>). Yang wajib aktif hanya lokasi tujuan
        /// (<c>INV-BD-027</c>).
        /// </para>
        /// <para>
        /// <b>Yang tidak tersentuh:</b> status, penerimaan asal, permintaan asal, dan data pemberian
        /// kantong. Pada kantong <c>Allocated</c>, alokasi, pasien tujuan, dan bukti kecocokannya tetap
        /// utuh karena berkas-berkas itu bukan bagian dari tindakan ini.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitResult> MoveStorageLocationAsync(
            Guid id,
            MoveStorageLocationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var inputFailure = ValidateInput(request.StorageLocationId, request.Note, actorUserId);

            if (inputFailure != null)
                return inputFailure;

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            // VAL-BD-062: perpindahan menuntut penempatan pertama lebih dulu.
            if (!unit.CurrentPlacementId.HasValue)
                return Failed(BloodUnitOutcome.NotAllowedByState, Val062Message);

            if (!StillInStockStatuses.Contains(unit.UnitStatus))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    $"Kantong berstatus {BbkDisplayLabels.Of(unit.UnitStatus)} sudah keluar dari stok dan tidak dapat dipindahkan.");
            }

            var destinationFailure = await CheckDestinationAsync(request.StorageLocationId, cancellationToken);

            if (destinationFailure != null)
                return destinationFailure;

            var current = await _dbContext.Set<BbkBloodUnitPlacement>()
                .FirstOrDefaultAsync(x => x.Id == unit.CurrentPlacementId.Value, cancellationToken);

            if (current == null)
                return Failed(BloodUnitOutcome.NotAllowedByState, Val062Message);

            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            current.IsCurrent = false;
            current.UpdateDateTime = now;
            current.UpdateBy = actorUserId;

            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            var placement = NewPlacement(unit.Id, request.StorageLocationId, current.Id, request.Note, actorUserId, now);

            _dbContext.Set<BbkBloodUnitPlacement>().Add(placement);
            unit.CurrentPlacementId = placement.Id;

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            await transaction.CommitAsync(cancellationToken);

            return Succeeded(unit, "Kantong berhasil dipindahkan. Status kantong tidak berubah.");
        }

        // =================================================================
        // Penolong
        // =================================================================

        private static BloodUnitResult? ValidateInput(Guid storageLocationId, string? note, Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            if (storageLocationId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, LocationRequiredMessage);

            if (note != null && note.Trim().Length > NoteMaxLength)
                return Failed(BloodUnitOutcome.Invalid, NoteTooLongMessage);

            return null;
        }

        /// <summary>
        /// Memeriksa lokasi tujuan penempatan baru — penempatan pertama maupun perpindahan
        /// (<c>INV-BD-027</c>).
        /// </summary>
        /// <remarks>
        /// Bila lokasi yang dipilih tidak dapat dipakai <b>dan</b> tidak ada satu pun lokasi aktif,
        /// pesannya mengarahkan ke Setup: masalahnya bukan pilihan petugas, melainkan modul yang
        /// berhenti karena master kosong (<c>INV-BD-025</c>, <i>fail-closed</i>).
        /// </remarks>
        private async Task<BloodUnitResult?> CheckDestinationAsync(Guid storageLocationId, CancellationToken cancellationToken)
        {
            var location = await _dbContext.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .Where(x => x.Id == storageLocationId)
                .Select(x => new { x.IsActive, IsRemoved = x.IsDelete || x.IsCancel })
                .FirstOrDefaultAsync(cancellationToken);

            if (location != null && location.IsActive && !location.IsRemoved)
                return null;

            var anyActive = await _dbContext.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .AnyAsync(x => x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken);

            if (!anyActive)
                return Failed(BloodUnitOutcome.NotAllowedByState, NoActiveLocationMessage);

            if (location == null || location.IsRemoved)
                return Failed(BloodUnitOutcome.Invalid, LocationNotFoundMessage);

            return Failed(BloodUnitOutcome.NotAllowedByState, Val060Message);
        }

        private static BbkBloodUnitPlacement NewPlacement(
            Guid unitId,
            Guid storageLocationId,
            Guid? previousPlacementId,
            string? note,
            Guid actorUserId,
            DateTime now)
            => new()
            {
                Id = Guid.NewGuid(),
                BloodUnitId = unitId,
                StorageLocationId = storageLocationId,
                PreviousPlacementId = previousPlacementId,
                PlacedAt = now,
                PlacedByUserId = actorUserId,
                IsCurrent = true,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

        private static List<string> AvailableActionsFor(BbkBloodUnitStatus status, Guid? currentPlacementId)
        {
            if (!currentPlacementId.HasValue && status == BbkBloodUnitStatus.Received)
                return new List<string> { AssignStorageLocationActionName };

            if (currentPlacementId.HasValue && StillInStockStatuses.Contains(status))
                return new List<string> { MoveStorageLocationActionName };

            return new List<string>();
        }

        private void AppendTransition(
            Guid unitId,
            string action,
            BbkBloodUnitStatus fromStatus,
            BbkBloodUnitStatus toStatus,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt,
            DateTime recordedAt,
            Guid correlationId)
        {
            _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
            {
                Id = Guid.NewGuid(),
                Scope = BbkTransitionScopes.BloodUnit,
                EntityId = unitId,
                Action = action,
                FromStatus = fromStatus.ToString(),
                ToStatus = toStatus.ToString(),
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = correlationId,
                CreateDateTime = recordedAt,
                CreateBy = actorUserId
            });
        }

        /// <summary>
        /// Menyimpan perubahan dan menerjemahkan dua penjaga penempatan tunggal — token
        /// <c>Version</c> kantong dan index unik terfilter — menjadi hasil <c>409</c>.
        /// </summary>
        private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();

                return false;
            }
            catch (DbUpdateException ex) when (IsCurrentPlacementViolation(ex))
            {
                _dbContext.ChangeTracker.Clear();

                return false;
            }
        }

        private static bool IsCurrentPlacementViolation(DbUpdateException exception)
            => exception.InnerException is PostgresException postgres &&
               postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
               string.Equals(
                   postgres.ConstraintName,
                   BbkBloodUnitPlacementConfiguration.CurrentUnitIndexName,
                   StringComparison.Ordinal);

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

        private Task<BbkBloodUnit?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<BbkBloodUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

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

        private static BloodUnitResult Succeeded(BbkBloodUnit entity, string message)
            => new(BloodUnitOutcome.Success, entity, message);

        private static BloodUnitResult Failed(BloodUnitOutcome outcome, string message)
            => new(outcome, null, message);
    }

    /// <summary>Hasil satu tindakan atas kantong darah. Dipetakan ke HTTP status oleh controller.</summary>
    public sealed record BloodUnitResult(
        BloodUnitOutcome Outcome,
        BbkBloodUnit? Entity,
        string Message);

    /// <summary>Jenis hasil satu tindakan atas kantong darah.</summary>
    public enum BloodUnitOutcome
    {
        Success = 0,

        /// <summary>Kantong tidak ada atau sudah dihapus.</summary>
        NotFound = 1,

        /// <summary>Isian tidak sah — lokasi kosong atau tidak ada, keterangan terlalu panjang.</summary>
        Invalid = 2,

        /// <summary>
        /// Keadaan data tidak memenuhi syarat — termasuk <c>VAL-BD-060</c>, <c>VAL-BD-061</c>,
        /// <c>VAL-BD-062</c>, dan master lokasi yang kosong.
        /// </summary>
        NotAllowedByState = 3,

        /// <summary>Kantong sudah berubah di tangan orang lain.</summary>
        VersionConflict = 4
    }

    /// <summary>
    /// Jawaban gerbang alokasi dari sisi penyimpanan (<c>ARCH-BD-POS-06</c>).
    /// </summary>
    /// <param name="IsOpen">Kantong sudah tersimpan pada lokasi yang sedang aktif.</param>
    /// <param name="RuleCode"><c>VAL-BD-063</c> atau <c>VAL-BD-064</c> bila tertutup.</param>
    /// <param name="Message">Pesan kanonis matriks validasi bagi pengguna.</param>
    public sealed record BloodUnitAllocationGateResult(
        bool IsOpen,
        string? RuleCode,
        string Message);
}
