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
        private const int ReasonCodeMaxLength = 30;

        /// <summary>Nama tindakan pada <c>BbkTransitionHistory</c>.</summary>
        private const string StoreAction = "Store";
        private const string MakeAvailableAction = "MakeAvailable";
        private const string HoldForReviewAction = "HoldForReview";
        private const string AllocateAction = "Allocate";
        private const string CancelAllocationAction = "CancelAllocation";
        private const string IssueAction = "Issue";
        private const string EmergencyIssueAction = "EmergencyIssue";

        /// <summary>Nama aksi pada <c>AvailableActions</c>.</summary>
        public const string AssignStorageLocationActionName = "AssignStorageLocation";
        public const string MoveStorageLocationActionName = "MoveStorageLocation";
        public const string AllocateActionName = "Allocate";
        public const string CancelAllocationActionName = "CancelAllocation";
        public const string RecordCompatibilityEvidenceActionName = "RecordCompatibilityEvidence";
        public const string IssueActionName = "Issue";
        public const string EmergencyIssueActionName = "EmergencyIssue";

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

        // Pesan kanonis matriks validasi §3, persis — alokasi dan pembatalannya (BE-BD-006).
        private const string Val018cMessage =
            "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain.";

        private const string Val023Message =
            "Kantong sudah diberikan. Pembatalan tidak dapat dilakukan; gunakan catatan koreksi bila " +
            "pencatatannya keliru.";

        private const string Val033Message =
            "Kantong ini menunggu keputusan dan tidak dapat langsung dialokasikan. " +
            "Selesaikan statusnya lebih dulu.";

        private const string Val016Message =
            "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.";

        // Pesan validasi pemberian normal â€” BE-BD-007.
        private const string Val017Message =
            "Kantong harus dialokasikan ke order pasien sebelum diberikan.";

        private const string Val018Message =
            "Bukti pemeriksaan kecocokan belum tercatat. Darah tidak dapat diberikan.";

        private const string Val019Message =
            "Bukti kecocokan yang ada bukan untuk pasien ini. " +
            "Catat bukti kecocokan terhadap pasien tujuan.";

        private const string Val020Message =
            "Bukti kecocokan sudah lewat masa berlaku. Diperlukan bukti kecocokan yang baru.";

        private const string Val020bMessage =
            "Masa berlaku bukti kecocokan untuk komponen ini belum ditetapkan. " +
            "Pemberian ditahan sampai dikonfigurasi.";

        private const string Val065Message =
            "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif dan belum dapat diberikan. " +
            "Pindahkan dulu ke lokasi yang aktif.";

        private const string Val079Message =
            "Hasil pemeriksaan kecocokan menyatakan kantong ini tidak cocok untuk pasien tersebut. " +
            "Kantong tidak dapat diberikan.";

        // Pesan validasi pemberian jalur darurat — BE-BD-008.
        private const string Val021Message =
            "Jalur darurat hanya untuk peran berwenang dan wajib mengisi alasan.";

        private const string Val066Message =
            "Pemberian darurat wajib menyebutkan apa yang dilewati: bukti kecocokan, " +
            "lokasi penyimpanan yang tidak aktif, atau keduanya.";

        private const string Val070Message =
            "Sebutkan keadaan yang membuat pemberian ini harus dilakukan sekarang.";

        private const string Val071Message =
            "Sebutkan Anda menerbitkan otorisasi ini sebagai Dokter Bank Darah atau sebagai " +
            "dokter penanggung jawab pasien.";

        private const string EmergencyReasonCategoryMismatchMessage =
            "Alasan yang dipilih bukan alasan jalur darurat.";

        private const string CheckedAtRequiredMessage =
            "Waktu pemeriksaan kecocokan wajib diisi.";

        private const string CheckedAtUtcMessage =
            "Waktu pemeriksaan kecocokan wajib dikirim dalam UTC.";

        private const string CheckedAtFutureMessage =
            "Waktu pemeriksaan kecocokan tidak boleh berada di masa depan.";

        private const string InvalidCompatibilityResultMessage =
            "Hasil pemeriksaan kecocokan tidak dikenali.";

        private const string NoActiveAllocationForEvidenceMessage =
            "Kantong harus memiliki alokasi aktif sebelum bukti kecocokan dapat dicatat.";

        private const string OrderLineRequiredMessage =
            "Baris kebutuhan order wajib dipilih.";

        private const string OrderLineNotFoundMessage =
            "Baris kebutuhan order tidak ditemukan atau sudah dihapus. Pilih baris dari order yang aktif.";

        private const string NoActiveAllocationMessage =
            "Kantong ini tidak sedang dialokasikan, sehingga tidak ada alokasi yang dapat dibatalkan.";

        /// <summary>
        /// Alasan pembatalan alokasi wajib berkategori <c>AllocationCancellation</c>
        /// (<c>DEC-BD-029</c>). Bunyinya sejalan dengan penolakan kategori pada pembatalan order.
        /// </summary>
        private const string ReasonCategoryMismatchMessage =
            "Alasan yang dipilih bukan alasan pembatalan alokasi kantong. Pilih alasan dari " +
            "kategori pembatalan alokasi.";

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

        /// <summary>
        /// Status order yang masih menerima alokasi kantong (matriks §3, syarat "order aktif").
        /// </summary>
        /// <remarks>
        /// <c>PartiallyFulfilled</c> ikut di sini karena kebutuhannya memang belum selesai —
        /// itulah keadaan normal order yang darahnya datang bertahap. <c>FullyFulfilled</c>,
        /// <c>Cancelled</c>, dan <c>Expired</c> tidak: order yang sudah terpenuhi tidak
        /// membutuhkan kantong tambahan, dan dua sisanya sudah berhenti berlaku.
        /// </remarks>
        private static readonly BbkBloodOrderStatus[] AllocatableOrderStatuses =
        {
            BbkBloodOrderStatus.Active,
            BbkBloodOrderStatus.PartiallyFulfilled
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly BbkEncounterStatusReader _encounterStatusReader;

        public BbkBloodUnitService(
            ApplicationDbContext dbContext,
            BbkEncounterStatusReader encounterStatusReader)
        {
            _dbContext = dbContext;
            _encounterStatusReader = encounterStatusReader;
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
            CancellationToken cancellationToken = default,
            bool? emergencyPendingEvidence = null)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (unitStatus.HasValue)
                query = query.Where(x => x.UnitStatus == unitStatus.Value);

            // Daftar kerja #3 (api-contract baris 100): kantong yang diberikan lewat jalur darurat
            // dan melewati gerbang bukti kecocokan, sehingga buktinya masih harus disusulkan.
            // Pemberian darurat yang hanya melewati gerbang lokasi tetap menunjuk bukti yang sah
            // dan karena itu tidak masuk daftar ini.
            if (emergencyPendingEvidence.HasValue)
            {
                query = emergencyPendingEvidence.Value
                    ? query.Where(x =>
                        x.IssuedViaEmergency && x.CompatibilityEvidenceIdUsed == null)
                    : query.Where(x =>
                        !x.IssuedViaEmergency || x.CompatibilityEvidenceIdUsed != null);
            }

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

            // Riwayat alokasi dibaca sekali lalu dipakai dua kali: seluruh barisnya, dan baris yang
            // sedang berlaku. Membacanya dua kali hanya menambah satu perjalanan ke database.
            var allocations = await ReadAllocationsAsync(entity.Id, cancellationToken);
            var compatibilityEvidences = await ReadCompatibilityEvidencesAsync(
                entity.Id,
                cancellationToken);
            var emergencyAuthorizations = await ReadEmergencyAuthorizationsAsync(
                entity.Id,
                cancellationToken);

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
                Allocations = allocations,
                CurrentAllocation = allocations
                    .FirstOrDefault(x => x.AllocationStatus == BbkAllocationStatus.Active),
                CompatibilityEvidences = compatibilityEvidences,
                CompatibilityEvidenceIdUsed = entity.CompatibilityEvidenceIdUsed,
                EmergencyAuthorizations = emergencyAuthorizations,
                AvailableActions = AvailableActionsFor(
                    entity.UnitStatus,
                    entity.CurrentPlacementId,
                    entity.IsExcess),
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
        // Alokasi kantong — BE-BD-006
        // =================================================================

        /// <summary>
        /// Mengikat satu kantong <c>Available</c> pada satu baris kebutuhan order:
        /// <c>Available</c> → <c>Allocated</c> (matriks §3, <c>DEC-BD-003</c>, <c>DEC-BD-007</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Gerbang penyimpanan dipakai, bukan ditiru.</b> Pertanyaan "sudah tersimpan dan
        /// lokasinya sedang aktif?" dijawab <see cref="EvaluateAllocationGateAsync"/> yang lahir
        /// pada <c>BE-BD-015</c> — kontrak <c>v4</c> <c>02-backend-architecture.md</c> §F.4
        /// menetapkan gerbang yang sama dipakai <c>allocate</c> dan <c>reallocate</c>. Menyalin
        /// logikanya ke sini akan membuat dua penjaga yang kelak berselisih diam-diam, dan
        /// penonaktifan satu kulkas hanya akan menutup separuh jalur.
        /// </para>
        ///
        /// <para>
        /// <b>Urutan pemeriksaan disengaja dan tidak boleh ditukar.</b> Gerbang penyimpanan
        /// dinilai <b>lebih dulu</b>, baru kantong yang menunggu keputusan. Kantong berlebih lahir
        /// <c>Received</c> juga, dan <c>INV-BD-025</c> menetapkan tonggak penyimpanan sebagai
        /// syarat pertama — sehingga kantong berlebih yang belum disimpan ditolak
        /// <c>VAL-BD-063</c> karena memang belum disimpan, bukan <c>VAL-BD-033</c>. Sesudah
        /// disimpan, kantong berlebih berstatus <c>PendingReview</c> dengan lokasi aktif, gerbang
        /// terbuka, dan penolakan yang benar barulah <c>VAL-BD-033</c> (<c>AC-BD-033</c>).
        /// </para>
        ///
        /// <para>
        /// <b>Satu alokasi aktif dijaga dua lapis, dan lapis keduanya yang menentukan.</b>
        /// Pembacaan "belum ada alokasi aktif" hanya menyaring kesalahan biasa; dua permintaan yang
        /// datang pada milidetik yang sama sama-sama lolos pembacaan itu. Yang menolak permintaan
        /// kedua adalah index unik terfilter <c>IX_BbkBloodUnitAllocation_ActiveUnit</c> ditambah
        /// token <see cref="BbkBloodUnit.Version"/>, dan penolakannya diterjemahkan menjadi
        /// <c>409 VAL-BD-018c</c> oleh <see cref="TrySaveAsync"/> — bukan menjadi galat PostgreSQL
        /// yang bocor ke pengguna (<c>VAL-BD-018c</c>).
        /// </para>
        ///
        /// <para>
        /// <b>Satu transaksi.</b> Baris alokasi, perpindahan status kantong, kenaikan token, dan
        /// baris riwayat tersimpan bersama atau tidak tersimpan sama sekali.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitResult> AllocateAsync(
            Guid id,
            AllocateUnitRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            if (request.BloodOrderLineId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, OrderLineRequiredMessage);

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            // Gerbang penyimpanan BE-BD-015, dipakai apa adanya: VAL-BD-063 dan VAL-BD-064.
            var gate = await EvaluateAllocationGateAsync(id, cancellationToken);

            if (gate == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (!gate.IsOpen)
                return Failed(BloodUnitOutcome.NotAllowedByState, gate.Message);

            // VAL-BD-033: kantong berlebih dan kantong yang menunggu keputusan tidak dapat
            // dialokasikan langsung; jalurnya penyelesaian PendingReview milik BE-BD-009.
            if (unit.IsExcess || unit.UnitStatus == BbkBloodUnitStatus.PendingReview)
                return Failed(BloodUnitOutcome.NotAllowedByState, Val033Message);

            // VAL-BD-018c dari sisi status: kantong yang sudah terikat tidak dapat diikat lagi.
            if (unit.UnitStatus == BbkBloodUnitStatus.Allocated)
                return Failed(BloodUnitOutcome.AllocationConflict, Val018cMessage);

            if (unit.UnitStatus != BbkBloodUnitStatus.Available)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    $"Kantong berstatus {BbkDisplayLabels.Of(unit.UnitStatus)} tidak dapat dialokasikan.");
            }

            var lineFailure = await CheckOrderLineAsync(request.BloodOrderLineId, cancellationToken);

            if (lineFailure != null)
                return lineFailure;

            // Penyaring kesalahan biasa. Penjaga sebenarnya ada di database — lihat catatan method.
            //
            // Saringannya SENGAJA hanya status, tanpa IsDelete, supaya cocok persis dengan predikat
            // index terfilter: WHERE "AllocationStatus" = 0. Index itu tidak menyebut IsDelete, jadi
            // saringan aplikasi yang menambahkan !IsDelete akan menyimpulkan "belum ada alokasi
            // aktif" untuk baris yang tetap menempati slot index — dan pembacaan itu berselisih
            // dengan penjaga yang sebenarnya berlaku.
            var hasActiveAllocation = await _dbContext.Set<BbkBloodUnitAllocation>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.BloodUnitId == id &&
                         x.AllocationStatus == BbkAllocationStatus.Active,
                    cancellationToken);

            if (hasActiveAllocation)
                return Failed(BloodUnitOutcome.AllocationConflict, Val018cMessage);

            var now = DateTime.UtcNow;

            var allocation = new BbkBloodUnitAllocation
            {
                Id = Guid.NewGuid(),
                BloodUnitId = unit.Id,
                BloodOrderLineId = request.BloodOrderLineId,
                AllocationStatus = BbkAllocationStatus.Active,
                AllocatedByUserId = actorUserId,
                AllocatedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkBloodUnitAllocation>().Add(allocation);

            var fromStatus = unit.UnitStatus;

            unit.UnitStatus = BbkBloodUnitStatus.Allocated;
            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            // Alokasi tidak memakai alasan terkendali, sehingga reasonCode dibiarkan bawaan.
            AppendTransition(
                unit.Id,
                AllocateAction,
                fromStatus,
                BbkBloodUnitStatus.Allocated,
                reasonNote: null,
                actorUserId,
                occurredAt: now,
                recordedAt: now,
                correlationId: allocation.Id);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodUnitOutcome.AllocationConflict, Val018cMessage);

            return Succeeded(unit, "Kantong berhasil dialokasikan ke baris kebutuhan order.");
        }

        /// <summary>
        /// Membatalkan alokasi yang keliru sebelum kantong diberikan: <c>Allocated</c> →
        /// <c>Available</c>, atau <c>PendingReview</c> bila order asalnya sudah berakhir
        /// (matriks §3, <c>DEC-BD-029</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Baris alokasinya tidak dihapus.</b> Ia berpindah ke <c>Cancelled</c> lalu menyimpan
        /// pelaku, waktu, kode alasan, dan <b>salinan teks</b> alasannya (<c>ARCH-BD-POS-03</c>,
        /// <c>INV-BD-035</c>). Pertanyaan audit "kantong ini pernah disiapkan untuk siapa saja"
        /// hanya dapat dijawab bila percobaan yang dibatalkan pun tersimpan.
        /// </para>
        ///
        /// <para>
        /// <b>Kembali ke <c>Available</c>, tidak pernah ke <c>Stored</c> maupun <c>Received</c></b>
        /// — tonggak penempatan hanya dilewati sekali (matriks §3 dan daftar perpindahan yang
        /// dilarang). Kantongnya memang masih berada di kulkas yang sama; pembatalan alokasi tidak
        /// memindahkan apa pun secara fisik.
        /// </para>
        ///
        /// <para>
        /// <b>Ke mana kantong kembali ditentukan order asalnya, bukan pilihan petugas</b>
        /// (<c>AC-BD-043</c> dan <c>AC-BD-044</c>). Bila order asal masih berjalan, kantong kembali
        /// menjadi stok yang boleh dialokasikan. Bila order asal sudah <c>Cancelled</c>,
        /// <c>Expired</c>, <c>FullyFulfilled</c>, atau kunjungan pasiennya sudah berakhir menurut
        /// <see cref="BbkEncounterStatusReader"/>, kantong <b>tidak</b> kembali ke stok bebas
        /// melainkan masuk <c>PendingReview</c> — ia perlu keputusan manusia, karena pasien yang
        /// menjadi alasan kantong itu diminta sudah tidak menunggunya lagi.
        /// </para>
        ///
        /// <para>
        /// <b>Contoh dua arah.</b> Kantong dialokasikan ke baris order Pasien A yang masih dirawat,
        /// lalu petugas sadar salah kantong → dibatalkan, kantong kembali <c>Available</c> dan
        /// dapat langsung dipakai pasien lain (<c>AC-BD-043</c>). Kantong lain dialokasikan ke
        /// Pasien B, lalu Pasien B pulang dan kunjungannya ditutup → pembatalan membawa kantong ke
        /// <c>PendingReview</c>, bukan <c>Available</c> (<c>AC-BD-044</c>).
        /// </para>
        ///
        /// <para>
        /// <b>Kantong yang sudah diberikan tidak dapat dibatalkan</b> (<c>VAL-BD-023</c>,
        /// <c>AC-BD-046</c>). Pemberian bersifat terminal; jalur perbaikannya catatan koreksi
        /// <c>BE-BD-010</c>, bukan pembatalan alokasi. Perpindahan <c>Issued</c> →
        /// <c>Available</c> tidak pernah terjadi lewat jalur mana pun.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitResult> CancelAllocationAsync(
            Guid id,
            CancelWithReasonRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            // VAL-BD-016 (400): alasan wajib dipilih dari daftar terkendali, tidak diketik bebas.
            //
            // Dinormalkan menjadi string non-nullable, mengikuti NormalizeCode pada
            // BbkBloodOrderService: nilai kosong menjadi string.Empty, bukan null. Bentuk itu dipakai
            // di dalam lambda EF di bawah, dan local non-nullable menutup satu-satunya pertanyaan
            // aliran nullable pada jalur ini.
            var reasonCode = NormalizeReasonCode(request.ReasonCode);

            if (reasonCode.Length == 0)
                return Failed(BloodUnitOutcome.Invalid, Val016Message);

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            // VAL-BD-023: pemberian terminal. Diperiksa sebelum alasan DICARI DI MASTER, sehingga
            // kantong yang sudah diberikan ditolak dengan sebab yang benar walaupun kode alasannya
            // sah. Permintaan yang kode alasannya kosong sama sekali tetap ditolak lebih dulu oleh
            // VAL-BD-016 di atas, karena body seperti itu tidak sah bagi tindakan apa pun.
            if (unit.UnitStatus == BbkBloodUnitStatus.Issued)
                return Failed(BloodUnitOutcome.NotAllowedByState, Val023Message);

            if (unit.UnitStatus != BbkBloodUnitStatus.Allocated)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    $"Kantong berstatus {BbkDisplayLabels.Of(unit.UnitStatus)} tidak sedang dialokasikan, " +
                    "sehingga tidak ada alokasi yang dapat dibatalkan.");
            }

            var reason = await _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete && x.IsActive && x.ReasonCode.ToUpper() == reasonCode,
                    cancellationToken);

            if (reason == null)
                return Failed(BloodUnitOutcome.Invalid, Val016Message);

            // Kategori alasan menentukan maknanya. Alasan pembatalan order atau jalur darurat tidak
            // boleh dipakai membatalkan alokasi, walaupun keduanya sah sebagai alasan.
            if (BloodBankReasonCategories.Normalize(reason.ReasonCategory)
                != BloodBankReasonCategories.AllocationCancellation)
            {
                return Failed(BloodUnitOutcome.NotAllowedByState, ReasonCategoryMismatchMessage);
            }

            // Sama seperti pada alokasi, saringannya hanya status — mencerminkan predikat index
            // terfilter. Kalau baris yang menempati slot index tidak terbaca di sini, kantong akan
            // berstatus Allocated tanpa satu pun alokasi yang dapat dibatalkan, dan tidak ada jalan
            // keluar dari keadaan itu lewat API.
            var allocation = await _dbContext.Set<BbkBloodUnitAllocation>()
                .FirstOrDefaultAsync(
                    x => x.BloodUnitId == id &&
                         x.AllocationStatus == BbkAllocationStatus.Active,
                    cancellationToken);

            if (allocation == null)
                return Failed(BloodUnitOutcome.NotAllowedByState, NoActiveAllocationMessage);

            var originStillActive = await IsAllocationOriginActiveAsync(
                allocation.BloodOrderLineId,
                cancellationToken);

            var finalStatus = originStillActive
                ? BbkBloodUnitStatus.Available
                : BbkBloodUnitStatus.PendingReview;

            var now = DateTime.UtcNow;

            allocation.AllocationStatus = BbkAllocationStatus.Cancelled;
            allocation.CancelReasonCode = reason.ReasonCode;
            allocation.CancelReasonNote = reason.ReasonText;
            allocation.CancelledByUserId = actorUserId;
            allocation.CancelledAt = now;
            allocation.UpdateDateTime = now;
            allocation.UpdateBy = actorUserId;

            unit.UnitStatus = finalStatus;
            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            AppendTransition(
                unit.Id,
                CancelAllocationAction,
                BbkBloodUnitStatus.Allocated,
                finalStatus,
                reasonNote: reason.ReasonText,
                actorUserId,
                occurredAt: now,
                recordedAt: now,
                correlationId: allocation.Id,
                reasonCode: reason.ReasonCode);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(
                unit,
                originStillActive
                    ? "Alokasi kantong berhasil dibatalkan. Kantong kembali tersedia."
                    : "Alokasi kantong berhasil dibatalkan. Order asal sudah berakhir, sehingga kantong " +
                      "masuk daftar menunggu keputusan.");
        }

        // =================================================================
        // Bukti kecocokan dan pemberian â€” BE-BD-007
        // =================================================================

        /// <summary>
        /// Mencatat hasil pemeriksaan kecocokan untuk pasien yang menjadi tujuan
        /// alokasi aktif kantong.
        /// </summary>
        public async Task<BloodUnitResult> RecordCompatibilityEvidenceAsync(
            Guid id,
            RecordEvidenceRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            if (!request.EvidenceResult.HasValue ||
                !Enum.IsDefined(request.EvidenceResult.Value))
            {
                return Failed(
                    BloodUnitOutcome.Invalid,
                    InvalidCompatibilityResultMessage);
            }

            if (request.CheckedAt == default)
            {
                return Failed(
                    BloodUnitOutcome.Invalid,
                    CheckedAtRequiredMessage);
            }

            if (request.CheckedAt.Kind != DateTimeKind.Utc)
            {
                return Failed(
                    BloodUnitOutcome.Invalid,
                    CheckedAtUtcMessage);
            }

            var now = DateTime.UtcNow;

            if (request.CheckedAt > now)
            {
                return Failed(
                    BloodUnitOutcome.Invalid,
                    CheckedAtFutureMessage);
            }

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
            {
                return Failed(
                    BloodUnitOutcome.VersionConflict,
                    ConcurrencyMessage);
            }

            if (unit.UnitStatus != BbkBloodUnitStatus.Allocated)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    NoActiveAllocationForEvidenceMessage);
            }

            var allocationTarget = await _dbContext
                .Set<BbkBloodUnitAllocation>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == id &&
                    x.AllocationStatus == BbkAllocationStatus.Active)
                .Select(x => new
                {
                    PatientId =
                        x.BloodOrderLine != null &&
                        x.BloodOrderLine.BloodOrder != null
                            ? (Guid?)x.BloodOrderLine.BloodOrder.PatientId
                            : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (allocationTarget == null || !allocationTarget.PatientId.HasValue)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    NoActiveAllocationForEvidenceMessage);
            }

            var evidence = new BbkCompatibilityEvidence
            {
                Id = Guid.NewGuid(),
                BloodUnitId = unit.Id,
                PatientId = allocationTarget.PatientId.Value,
                EvidenceResult = request.EvidenceResult.Value,
                ValidatedByUserId = actorUserId,
                CheckedAt = request.CheckedAt,
                IsSuperseded = false,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkCompatibilityEvidence>().Add(evidence);

            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            if (!await TrySaveAsync(cancellationToken))
            {
                return Failed(
                    BloodUnitOutcome.VersionConflict,
                    ConcurrencyMessage);
            }

            return Succeeded(
                unit,
                request.EvidenceResult.Value == BbkCompatibilityResult.Compatible
                    ? "Bukti kecocokan berhasil dicatat dengan hasil cocok."
                    : "Bukti kecocokan berhasil dicatat dengan hasil tidak cocok.");
        }

        /// <summary>
        /// Menilai seluruh gerbang pemberian normal berdasarkan keadaan terkini.
        /// </summary>
        public async Task<BloodUnitIssuanceGateResult?> EvaluateIssuanceGateAsync(
            Guid unitId,
            CancellationToken cancellationToken = default)
        {
            var unit = await BaseQuery()
                .Where(x => x.Id == unitId)
                .Select(x => new
                {
                    x.UnitStatus,
                    x.CurrentPlacementId,
                    IsLocationActive =
                        x.CurrentPlacement != null &&
                        x.CurrentPlacement.StorageLocation != null &&
                        x.CurrentPlacement.StorageLocation.IsActive &&
                        !x.CurrentPlacement.StorageLocation.IsDelete &&
                        !x.CurrentPlacement.StorageLocation.IsCancel,
                    ValidityHours =
                        x.BloodComponent != null
                            ? x.BloodComponent.CompatibilityEvidenceValidityHours
                            : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (unit == null)
                return null;

            if (unit.UnitStatus != BbkBloodUnitStatus.Allocated)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-017",
                    Val017Message);
            }

            if (!unit.CurrentPlacementId.HasValue || !unit.IsLocationActive)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-065",
                    Val065Message);
            }

            var activeAllocation = await _dbContext
                .Set<BbkBloodUnitAllocation>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == unitId &&
                    x.AllocationStatus == BbkAllocationStatus.Active)
                .Select(x => new
                {
                    PatientId =
                        x.BloodOrderLine != null &&
                        x.BloodOrderLine.BloodOrder != null
                            ? (Guid?)x.BloodOrderLine.BloodOrder.PatientId
                            : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (activeAllocation == null || !activeAllocation.PatientId.HasValue)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-017",
                    Val017Message);
            }

            var patientId = activeAllocation.PatientId.Value;

            return await EvaluateCompatibilityEvidenceGateAsync(
                unitId,
                patientId,
                unit.ValidityHours,
                cancellationToken);
        }

        /// <summary>
        /// Menilai gerbang bukti kecocokan saja, terlepas dari status dan lokasi kantong.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Dipakai bersama oleh gerbang pemberian normal (<c>BE-BD-007</c>) dan penilaian bypass
        /// jalur darurat (<c>BE-BD-008</c>). Keduanya wajib membaca aturan yang sama: kalau
        /// aturannya disalin, jalur darurat dapat menyatakan gerbang bukti "terbuka" pada keadaan
        /// yang justru ditahan jalur normal, dan penanda daruratnya menjadi keterangan palsu
        /// (<c>INV-BD-030</c>).
        /// </para>
        /// <para>
        /// Urutan penilaiannya tidak diubah dari <c>BE-BD-007</c>:
        /// <c>VAL-BD-018</c> → <c>VAL-BD-019</c> → <c>VAL-BD-020b</c> → <c>VAL-BD-079</c> →
        /// <c>VAL-BD-020</c>.
        /// </para>
        /// </remarks>
        private async Task<BloodUnitIssuanceGateResult> EvaluateCompatibilityEvidenceGateAsync(
            Guid unitId,
            Guid patientId,
            int? validityHours,
            CancellationToken cancellationToken)
        {
            var evidences = await _dbContext
                .Set<BbkCompatibilityEvidence>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == unitId &&
                    !x.IsDelete &&
                    !x.IsSuperseded)
                .OrderByDescending(x => x.CheckedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.EvidenceResult,
                    x.CheckedAt
                })
                .ToListAsync(cancellationToken);

            if (evidences.Count == 0)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-018",
                    Val018Message,
                    patientId);
            }

            var latestPatientEvidence = evidences
                .FirstOrDefault(x => x.PatientId == patientId);

            if (latestPatientEvidence == null)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-019",
                    Val019Message,
                    patientId);
            }

            if (!validityHours.HasValue || validityHours.Value <= 0)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-020b",
                    Val020bMessage,
                    patientId);
            }

            if (latestPatientEvidence.EvidenceResult == BbkCompatibilityResult.Incompatible)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-079",
                    Val079Message,
                    patientId,
                    latestPatientEvidence.Id);
            }

            DateTime validUntil;

            try
            {
                validUntil = latestPatientEvidence.CheckedAt.AddHours(validityHours.Value);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-020",
                    Val020Message,
                    patientId,
                    latestPatientEvidence.Id);
            }

            if (validUntil <= DateTime.UtcNow)
            {
                return new BloodUnitIssuanceGateResult(
                    false,
                    "VAL-BD-020",
                    Val020Message,
                    patientId,
                    latestPatientEvidence.Id);
            }

            return new BloodUnitIssuanceGateResult(
                true,
                null,
                "Gerbang pemberian terbuka.",
                patientId,
                latestPatientEvidence.Id);
        }

        /// <summary>
        /// Memberikan kantong kepada pasien melalui jalur normal.
        /// Pemberian bersifat terminal.
        /// </summary>
        public async Task<BloodUnitResult> IssueAsync(
            Guid id,
            IssueUnitRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
            {
                return Failed(
                    BloodUnitOutcome.VersionConflict,
                    ConcurrencyMessage);
            }

            var gate = await EvaluateIssuanceGateAsync(id, cancellationToken);

            if (gate == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (!gate.IsOpen ||
                !gate.PatientId.HasValue ||
                !gate.CompatibilityEvidenceId.HasValue)
            {
                // Kode gerbang dibawa apa adanya ke controller. Gerbang sudah menentukan
                // VAL-BD-017/018/019/020/020b/065/079; menghitungnya ulang di controller akan
                // melahirkan sumber kebenaran kedua yang bisa menyimpang dari gerbang.
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    gate.Message,
                    gate.ValidationCode);
            }

            var now = DateTime.UtcNow;
            var fromStatus = unit.UnitStatus;

            unit.UnitStatus = BbkBloodUnitStatus.Issued;
            unit.IssuedToPatientId = gate.PatientId.Value;
            unit.IssuedAt = now;
            unit.IssuedByUserId = actorUserId;
            unit.CompatibilityEvidenceIdUsed = gate.CompatibilityEvidenceId.Value;
            unit.IssuedViaEmergency = false;
            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            AppendTransition(
                unit.Id,
                IssueAction,
                fromStatus,
                BbkBloodUnitStatus.Issued,
                reasonNote: null,
                actorUserId,
                occurredAt: now,
                recordedAt: now,
                correlationId: gate.CompatibilityEvidenceId.Value);

            if (!await TrySaveAsync(cancellationToken))
            {
                return Failed(
                    BloodUnitOutcome.VersionConflict,
                    ConcurrencyMessage);
            }

            return Succeeded(
                unit,
                "Kantong berhasil diberikan kepada pasien.");
        }

        // =================================================================
        // Pemberian jalur darurat — BE-BD-008
        // =================================================================

        /// <summary>
        /// Menilai kedua gerbang yang dapat dilewati otorisasi darurat, masing-masing terpisah.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Gerbang pemberian normal berhenti pada penolakan pertama, sehingga ia tidak dapat
        /// menjawab "apakah gerbang bukti juga tertutup" ketika lokasinya sudah tertutup lebih
        /// dulu. Jalur darurat justru menuntut jawaban atas <b>keduanya</b>, karena otorisasinya
        /// wajib menyatakan gerbang mana yang dilewati (<c>INV-BD-030</c>).
        /// </para>
        /// <para>
        /// Aturan buktinya tetap dibaca dari
        /// <see cref="EvaluateCompatibilityEvidenceGateAsync"/> yang sama dengan jalur normal,
        /// bukan disalin.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitEmergencyBypassState?> EvaluateEmergencyBypassAsync(
            Guid unitId,
            CancellationToken cancellationToken = default)
        {
            var unit = await BaseQuery()
                .Where(x => x.Id == unitId)
                .Select(x => new
                {
                    x.UnitStatus,
                    x.CurrentPlacementId,
                    IsLocationActive =
                        x.CurrentPlacement != null &&
                        x.CurrentPlacement.StorageLocation != null &&
                        x.CurrentPlacement.StorageLocation.IsActive &&
                        !x.CurrentPlacement.StorageLocation.IsDelete &&
                        !x.CurrentPlacement.StorageLocation.IsCancel,
                    ValidityHours =
                        x.BloodComponent != null
                            ? x.BloodComponent.CompatibilityEvidenceValidityHours
                            : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (unit == null)
                return null;

            var activeAllocation = await _dbContext
                .Set<BbkBloodUnitAllocation>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == unitId &&
                    x.AllocationStatus == BbkAllocationStatus.Active)
                .Select(x => new
                {
                    PatientId =
                        x.BloodOrderLine != null &&
                        x.BloodOrderLine.BloodOrder != null
                            ? (Guid?)x.BloodOrderLine.BloodOrder.PatientId
                            : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Lokasi dinilai apa adanya. Kantong tanpa penempatan sama sekali dihitung tertutup,
            // sama seperti pada gerbang normal: yang belum pernah disimpan tidak boleh diberikan
            // hanya karena jalurnya darurat.
            var locationGateClosed =
                !unit.CurrentPlacementId.HasValue || !unit.IsLocationActive;

            if (activeAllocation == null || !activeAllocation.PatientId.HasValue)
            {
                return new BloodUnitEmergencyBypassState(
                    EvidenceGateClosed: true,
                    LocationGateClosed: locationGateClosed,
                    PatientId: null,
                    ValidCompatibilityEvidenceId: null);
            }

            var patientId = activeAllocation.PatientId.Value;

            var evidenceGate = await EvaluateCompatibilityEvidenceGateAsync(
                unitId,
                patientId,
                unit.ValidityHours,
                cancellationToken);

            return new BloodUnitEmergencyBypassState(
                EvidenceGateClosed: !evidenceGate.IsOpen,
                LocationGateClosed: locationGateClosed,
                PatientId: patientId,
                ValidCompatibilityEvidenceId:
                    evidenceGate.IsOpen ? evidenceGate.CompatibilityEvidenceId : null);
        }

        /// <summary>
        /// Memberikan kantong lewat jalur darurat, melewati gerbang bukti kecocokan dan/atau
        /// gerbang lokasi penyimpanan aktif. Pemberian bersifat terminal.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Jalur ini tidak melewati status.</b> Kantong tetap wajib berstatus
        /// <c>Allocated</c> dan tetap wajib punya alokasi aktif — keadaan darurat memperbolehkan
        /// melewati bukti dan lokasi, bukan memberikan darah yang tidak ditujukan kepada siapa pun
        /// (<c>state-transition</c> §3 baris jalur darurat).
        /// </para>
        /// <para>
        /// <b>Kewenangan penerbit dijaga hak akses, bukan dihitung ulang di sini.</b>
        /// <c>BloodUnit : EmergencyIssue</c> menahan pelaku yang bukan Dokter BDRS maupun DPJP
        /// (<c>VAL-BD-072</c>). Peran yang dinyatakan penerbit <b>direkam, tidak diverifikasi</b>:
        /// <c>validation-matrix</c> menegaskan Quilvian tidak memeriksa kebenaran penugasan DPJP
        /// karena Bank Darah bukan pemilik data penugasan itu.
        /// </para>
        /// </remarks>
        public async Task<BloodUnitResult> EmergencyIssueAsync(
            Guid id,
            EmergencyIssueRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodUnitOutcome.Invalid, ActorUnknownMessage);

            // VAL-BD-021 memegang alasan darurat yang kosong atau tidak sah (DEC-BD-050).
            // Kewenangan penerbit bukan urusan kode ini: pelaku yang tidak memegang
            // BloodUnit : EmergencyIssue sudah ditahan hak akses dengan VAL-BD-072 sebelum action
            // ini dijalankan, sehingga tidak ada pemeriksaan kewenangan kedua di sini yang dapat
            // menyimpang dari yang pertama.
            var reasonCode = request.ReasonCode?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(reasonCode))
                return Failed(BloodUnitOutcome.Forbidden, Val021Message, "VAL-BD-021");

            if (reasonCode.Length > ReasonCodeMaxLength)
                return Failed(BloodUnitOutcome.Forbidden, Val021Message, "VAL-BD-021");

            // VAL-BD-071 sebelum VAL-BD-070 dan VAL-BD-066: ketiganya soal kelengkapan otorisasi,
            // dan petugas perlu tahu bagian mana yang kurang, bukan satu pesan gabungan.
            if (!request.AuthorizerRole.HasValue ||
                !Enum.IsDefined(request.AuthorizerRole.Value))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val071Message,
                    "VAL-BD-071");
            }

            var conditionNote = request.EmergencyConditionNote?.Trim();

            if (string.IsNullOrWhiteSpace(conditionNote))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val070Message,
                    "VAL-BD-070");
            }

            if (conditionNote.Length > NoteMaxLength)
                return Failed(BloodUnitOutcome.Invalid, NoteTooLongMessage);

            if (!request.BypassScope.HasValue ||
                !Enum.IsDefined(request.BypassScope.Value))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val066Message,
                    "VAL-BD-066");
            }

            var unit = await TrackedAsync(id, cancellationToken);

            if (unit == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != unit.Version)
                return Failed(BloodUnitOutcome.VersionConflict, ConcurrencyMessage);

            if (unit.UnitStatus != BbkBloodUnitStatus.Allocated)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val017Message,
                    "VAL-BD-017");
            }

            var reason = await _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete && x.IsActive && x.ReasonCode.ToUpper() == reasonCode,
                    cancellationToken);

            if (reason == null)
                return Failed(BloodUnitOutcome.Forbidden, Val021Message, "VAL-BD-021");

            if (BloodBankReasonCategories.Normalize(reason.ReasonCategory)
                != BloodBankReasonCategories.Emergency)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    EmergencyReasonCategoryMismatchMessage);
            }

            var bypass = await EvaluateEmergencyBypassAsync(id, cancellationToken);

            if (bypass == null)
                return Failed(BloodUnitOutcome.NotFound, NotFoundMessage);

            if (!bypass.PatientId.HasValue)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val017Message,
                    "VAL-BD-017");
            }

            // VAL-BD-066 bagian "tidak sesuai keadaan kantong". Penanda darurat yang menyebut
            // gerbang yang sebenarnya tidak tertutup adalah keterangan palsu pada rekam klinis,
            // dan penanda yang tidak menyebut gerbang yang benar-benar dilewati meninggalkan
            // pemberian tanpa keterangan (INV-BD-030).
            var declared = request.BypassScope.Value;

            var declaredEvidence =
                declared == BbkEmergencyBypassScope.CompatibilityEvidence ||
                declared == BbkEmergencyBypassScope.Both;

            var declaredLocation =
                declared == BbkEmergencyBypassScope.InactiveStorageLocation ||
                declared == BbkEmergencyBypassScope.Both;

            if (declaredEvidence != bypass.EvidenceGateClosed ||
                declaredLocation != bypass.LocationGateClosed)
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    Val066Message,
                    "VAL-BD-066");
            }

            var now = DateTime.UtcNow;
            var fromStatus = unit.UnitStatus;
            var patientId = bypass.PatientId.Value;

            var authorization = new BbkEmergencyAuthorization
            {
                Id = Guid.NewGuid(),
                BloodUnitId = unit.Id,
                PatientId = patientId,
                AuthorizedByUserId = actorUserId,
                AuthorizedAt = now,
                ReasonCode = reason.ReasonCode,
                ReasonNote = reason.ReasonText,
                BypassScope = declared,
                AuthorizerRole = request.AuthorizerRole.Value,
                EmergencyConditionNote = conditionNote,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkEmergencyAuthorization>().Add(authorization);

            unit.UnitStatus = BbkBloodUnitStatus.Issued;
            unit.IssuedToPatientId = patientId;
            unit.IssuedAt = now;
            unit.IssuedByUserId = actorUserId;
            unit.IssuedViaEmergency = true;

            // Ketika gerbang bukti TIDAK dilewati, buktinya memang berlaku dan wajib ikut
            // tercatat: pemberian darurat karena lokasi nonaktif tetap punya bukti kecocokan yang
            // sah, dan menghapus jejaknya akan membuat kantong ini terbaca seolah diberikan tanpa
            // bukti sama sekali.
            unit.CompatibilityEvidenceIdUsed = bypass.ValidCompatibilityEvidenceId;

            unit.Version++;
            unit.UpdateDateTime = now;
            unit.UpdateBy = actorUserId;

            AppendTransition(
                unit.Id,
                EmergencyIssueAction,
                fromStatus,
                BbkBloodUnitStatus.Issued,
                reasonNote: reason.ReasonText,
                actorUserId,
                occurredAt: now,
                recordedAt: now,
                correlationId: authorization.Id,
                reasonCode: reason.ReasonCode);

            if (!await TrySaveAsync(cancellationToken))
            {
                return Failed(
                    BloodUnitOutcome.VersionConflict,
                    ConcurrencyMessage);
            }

            return Succeeded(
                unit,
                "Kantong berhasil diberikan lewat jalur darurat. Otorisasi darurat tercatat.");
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

        /// <remarks>
        /// <para>
        /// <b>Daftar ini kelayakan, bukan izin.</b> Ia menjawab tombol apa yang masuk akal
        /// ditampilkan, bukan siapa yang boleh menekannya — hak akses tetap ditegakkan
        /// <c>[AccessPermission]</c> pada controller, dan setiap aksi menilai ulang syaratnya
        /// sendiri saat dijalankan.
        /// </para>
        /// <para>
        /// <b>Aksi alokasi ditawarkan sejak <c>BE-BD-006</c>.</b> <c>Allocate</c> muncul hanya pada
        /// kantong <c>Available</c> yang <b>bukan</b> kantong berlebih, karena kantong berlebih
        /// ditolak <c>VAL-BD-033</c>. Keaktifan lokasi <b>tidak</b> diperiksa di sini: gerbang
        /// <c>VAL-BD-064</c> dinilai saat tindakan dicoba, dan menyembunyikan tombolnya akan
        /// membuat petugas tidak pernah melihat sebab penolakannya.
        /// </para>
        /// </remarks>
        private static List<string> AvailableActionsFor(
            BbkBloodUnitStatus status,
            Guid? currentPlacementId,
            bool isExcess)
        {
            if (!currentPlacementId.HasValue && status == BbkBloodUnitStatus.Received)
                return new List<string> { AssignStorageLocationActionName };

            var actions = new List<string>();

            if (currentPlacementId.HasValue && StillInStockStatuses.Contains(status))
                actions.Add(MoveStorageLocationActionName);

            if (currentPlacementId.HasValue && status == BbkBloodUnitStatus.Available && !isExcess)
                actions.Add(AllocateActionName);

            if (status == BbkBloodUnitStatus.Allocated)
            {
                actions.Add(CancelAllocationActionName);
                actions.Add(RecordCompatibilityEvidenceActionName);
                actions.Add(IssueActionName);

                // Jalur darurat layak dicoba pada setiap kantong Allocated, termasuk yang
                // lokasinya nonaktif. Keaktifan lokasi dan kelengkapan bukti TIDAK diperiksa di
                // sini: gerbangnya dinilai saat tindakan dilakukan, dan menyembunyikan tombolnya
                // justru menutup satu-satunya jalan keluar yang disediakan DEC-BD-038.
                actions.Add(EmergencyIssueActionName);
            }

            return actions;
        }

        /// <summary>
        /// Memeriksa baris kebutuhan tujuan alokasi: barisnya ada, ordernya ada, ordernya masih
        /// berjalan, dan kunjungan pasiennya belum berakhir (matriks §3 baris <c>Available</c> →
        /// <c>Allocated</c>, syarat "order aktif").
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Dua sumber keaktifan, keduanya wajib.</b> Status order menjawab "apakah ordernya
        /// masih diminta"; <see cref="BbkEncounterStatusReader"/> menjawab "apakah pasiennya masih
        /// di tempat" (<c>DEC-BD-014</c>). Order yang masih berstatus <c>Active</c> tetapi
        /// pasiennya sudah pulang <b>bukan</b> order aktif — dan mengalokasikan kantong ke situ
        /// berarti menahan kantong untuk pasien yang tidak akan menerimanya.
        /// </para>
        /// <para>
        /// <b>Kodenya belum ada di matriks validasi.</b> Kontrak <c>v4</c> menyebut syarat "order
        /// aktif" pada matriks perpindahan status, tetapi tidak memberi kode <c>VAL-BD-*</c> untuk
        /// penolakannya, dan <c>api-contract</c> hanya mencantumkan <c>409 VAL-BD-018c</c> serta
        /// <c>422 VAL-BD-033/063/064</c> pada endpoint ini. Karena itu penolakan di sini memakai
        /// pesan bisnis yang jelas tanpa mengarang kode baru; delta-nya dicatat pada laporan task
        /// <c>BE-BD-006</c> untuk diputuskan pemilik kontrak.
        /// </para>
        /// </remarks>
        private async Task<BloodUnitResult?> CheckOrderLineAsync(
            Guid bloodOrderLineId,
            CancellationToken cancellationToken)
        {
            var line = await _dbContext.Set<BbkBloodOrderLine>()
                .AsNoTracking()
                .Where(x => x.Id == bloodOrderLineId && !x.IsDelete && !x.IsCancel)
                .Select(x => new
                {
                    x.BloodOrderId,
                    OrderExists = x.BloodOrder != null && !x.BloodOrder.IsDelete,
                    OrderStatus = x.BloodOrder != null ? x.BloodOrder.OrderStatus : (BbkBloodOrderStatus?)null,
                    EncounterId = x.BloodOrder != null ? x.BloodOrder.EncounterId : (Guid?)null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (line == null || !line.OrderExists || !line.OrderStatus.HasValue || !line.EncounterId.HasValue)
                return Failed(BloodUnitOutcome.Invalid, OrderLineNotFoundMessage);

            if (!AllocatableOrderStatuses.Contains(line.OrderStatus.Value))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    $"Order asal baris kebutuhan ini berstatus {BbkDisplayLabels.Of(line.OrderStatus.Value)} " +
                    "dan tidak lagi menerima alokasi kantong.");
            }

            if (await _encounterStatusReader.IsEncounterClosedAsync(line.EncounterId.Value, cancellationToken))
            {
                return Failed(
                    BloodUnitOutcome.NotAllowedByState,
                    "Kunjungan pasien pada order asal sudah berakhir, sehingga kantong tidak dapat " +
                    "dialokasikan ke baris kebutuhan ini.");
            }

            return null;
        }

        /// <summary>
        /// Menjawab satu pertanyaan bagi pembatalan alokasi: apakah order asal baris kebutuhan ini
        /// masih berjalan? Jawabannya menentukan kantong kembali <c>Available</c> atau masuk
        /// <c>PendingReview</c> (<c>AC-BD-043</c> lawan <c>AC-BD-044</c>).
        /// </summary>
        /// <remarks>
        /// <b>Berat sebelah ke arah aman.</b> Baris kebutuhan atau order yang tidak dapat dibaca
        /// dijawab "tidak aktif", sehingga kantong masuk <c>PendingReview</c> dan menunggu
        /// keputusan manusia — bukan kembali ke stok bebas atas dasar data yang tidak terbaca.
        /// Pilihan yang sama dipakai <see cref="BbkEncounterStatusReader"/> untuk kunjungan yang
        /// tidak ditemukan.
        /// </remarks>
        private async Task<bool> IsAllocationOriginActiveAsync(
            Guid bloodOrderLineId,
            CancellationToken cancellationToken)
        {
            var origin = await _dbContext.Set<BbkBloodOrderLine>()
                .AsNoTracking()
                .Where(x => x.Id == bloodOrderLineId && !x.IsDelete)
                .Select(x => new
                {
                    OrderStatus = x.BloodOrder != null && !x.BloodOrder.IsDelete
                        ? x.BloodOrder.OrderStatus
                        : (BbkBloodOrderStatus?)null,
                    EncounterId = x.BloodOrder != null ? x.BloodOrder.EncounterId : (Guid?)null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (origin == null || !origin.OrderStatus.HasValue || !origin.EncounterId.HasValue)
                return false;

            if (!AllocatableOrderStatuses.Contains(origin.OrderStatus.Value))
                return false;

            return !await _encounterStatusReader.IsEncounterClosedAsync(
                origin.EncounterId.Value,
                cancellationToken);
        }

        /// <summary>
        /// Menormalkan kode alasan dari klien: dipangkas, dijadikan huruf besar, dan kosong menjadi
        /// <see cref="string.Empty"/> alih-alih <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Bentuknya sengaja sama dengan <c>NormalizeCode</c> pada <c>BbkBloodOrderService</c>
        /// (<c>BE-BD-003</c>) supaya kedua jalur alasan terkendali Bank Darah memperlakukan masukan
        /// klien dengan cara yang sama persis.
        /// </remarks>
        private static string NormalizeReasonCode(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        /// <summary>
        /// Riwayat alokasi satu kantong, terlama lebih dulu. Baris yang sudah dibatalkan
        /// <b>tetap</b> terbaca (<c>ARCH-BD-POS-03</c>).
        /// </summary>
        private async Task<List<BloodUnitAllocationDto>> ReadAllocationsAsync(
            Guid unitId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkBloodUnitAllocation>()
                .AsNoTracking()
                .Where(x => x.BloodUnitId == unitId && !x.IsDelete)
                .OrderBy(x => x.AllocatedAt)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new BloodUnitAllocationDto
                {
                    Id = x.Id,
                    BloodUnitId = x.BloodUnitId,
                    BloodOrderLineId = x.BloodOrderLineId,
                    BloodOrderId = x.BloodOrderLine != null ? x.BloodOrderLine.BloodOrderId : null,
                    OrderNumber = x.BloodOrderLine != null && x.BloodOrderLine.BloodOrder != null
                        ? x.BloodOrderLine.BloodOrder.OrderNumber
                        : null,
                    LineSequence = x.BloodOrderLine != null ? x.BloodOrderLine.Sequence : null,
                    BloodComponentId = x.BloodOrderLine != null ? x.BloodOrderLine.BloodComponentId : null,
                    BloodComponentCode = x.BloodOrderLine != null && x.BloodOrderLine.BloodComponent != null
                        ? x.BloodOrderLine.BloodComponent.ComponentCode
                        : null,
                    BloodComponentName = x.BloodOrderLine != null && x.BloodOrderLine.BloodComponent != null
                        ? x.BloodOrderLine.BloodComponent.ComponentName
                        : null,
                    PatientId = x.BloodOrderLine != null && x.BloodOrderLine.BloodOrder != null
                        ? x.BloodOrderLine.BloodOrder.PatientId
                        : null,
                    PatientName = x.BloodOrderLine != null &&
                                  x.BloodOrderLine.BloodOrder != null &&
                                  x.BloodOrderLine.BloodOrder.Patient != null
                        ? x.BloodOrderLine.BloodOrder.Patient.FullName
                        : null,
                    AllocationStatus = x.AllocationStatus,
                    AllocationStatusLabel = x.AllocationStatus == BbkAllocationStatus.Active
                        ? "Aktif"
                        : "Dibatalkan",
                    AllocatedByUserId = x.AllocatedByUserId,
                    AllocatedAt = x.AllocatedAt,
                    CancelReasonCode = x.CancelReasonCode,
                    CancelReasonNote = x.CancelReasonNote,
                    CancelledByUserId = x.CancelledByUserId,
                    CancelledAt = x.CancelledAt
                })
                .ToListAsync(cancellationToken);

        /// <summary>
        /// Riwayat seluruh bukti kecocokan satu kantong, terbaru lebih dulu.
        /// Bukti incompatible maupun superseded tetap disimpan dan dibaca.
        /// </summary>
        /// <summary>
        /// Otorisasi darurat satu kantong, terbaru lebih dulu. Melekat permanen; tidak ada jalur
        /// bisnis yang menghapusnya (<c>INV-BD-032</c>).
        /// </summary>
        private async Task<List<EmergencyAuthorizationDto>> ReadEmergencyAuthorizationsAsync(
            Guid unitId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkEmergencyAuthorization>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == unitId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.AuthorizedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new EmergencyAuthorizationDto
                {
                    Id = x.Id,
                    BloodUnitId = x.BloodUnitId,
                    PatientId = x.PatientId,
                    AuthorizedByUserId = x.AuthorizedByUserId,
                    AuthorizedAt = x.AuthorizedAt,
                    ReasonCode = x.ReasonCode,
                    ReasonNote = x.ReasonNote,
                    BypassScope = x.BypassScope,
                    BypassScopeLabel =
                        x.BypassScope == BbkEmergencyBypassScope.CompatibilityEvidence
                            ? "Melewati bukti kecocokan"
                            : x.BypassScope == BbkEmergencyBypassScope.InactiveStorageLocation
                                ? "Melewati lokasi penyimpanan tidak aktif"
                                : "Melewati bukti kecocokan dan lokasi penyimpanan tidak aktif",
                    AuthorizerRole = x.AuthorizerRole,
                    AuthorizerRoleLabel =
                        x.AuthorizerRole == BbkEmergencyAuthorizerRole.BloodBankDoctor
                            ? "Dokter Bank Darah"
                            : "Dokter penanggung jawab pasien",
                    EmergencyConditionNote = x.EmergencyConditionNote,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

        private async Task<List<CompatibilityEvidenceDto>> ReadCompatibilityEvidencesAsync(
            Guid unitId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkCompatibilityEvidence>()
                .AsNoTracking()
                .Where(x =>
                    x.BloodUnitId == unitId &&
                    !x.IsDelete)
                .OrderByDescending(x => x.CheckedAt)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new CompatibilityEvidenceDto
                {
                    Id = x.Id,
                    BloodUnitId = x.BloodUnitId,
                    PatientId = x.PatientId,
                    EvidenceResult = x.EvidenceResult,
                    EvidenceResultLabel =
                        x.EvidenceResult == BbkCompatibilityResult.Compatible
                            ? "Cocok"
                            : "Tidak cocok",
                    ValidatedByUserId = x.ValidatedByUserId,
                    CheckedAt = x.CheckedAt,
                    IsSuperseded = x.IsSuperseded,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

        /// <remarks>
        /// <paramref name="reasonCode"/> ditambahkan pada <c>BE-BD-006</c>: pembatalan alokasi wajib
        /// menyimpan kode alasan terkendali <b>beserta</b> salinan teksnya, sedangkan perpindahan
        /// penyimpanan <c>BE-BD-015</c> memang tidak memakai alasan terkendali sama sekali. Nilai
        /// bawaan <c>null</c> menjaga pemanggil lama tetap berperilaku sama persis.
        /// </remarks>
        private void AppendTransition(
            Guid unitId,
            string action,
            BbkBloodUnitStatus fromStatus,
            BbkBloodUnitStatus toStatus,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt,
            DateTime recordedAt,
            Guid correlationId,
            string? reasonCode = null)
        {
            _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
            {
                Id = Guid.NewGuid(),
                Scope = BbkTransitionScopes.BloodUnit,
                EntityId = unitId,
                Action = action,
                FromStatus = fromStatus.ToString(),
                ToStatus = toStatus.ToString(),
                ReasonCode = reasonCode,
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
            catch (DbUpdateException ex) when (IsSingleRowGuardViolation(ex))
            {
                _dbContext.ChangeTracker.Clear();

                return false;
            }
        }

        /// <summary>
        /// Mengenali pelanggaran salah satu dari dua index unik terfilter Bank Darah: penempatan
        /// berlaku (<c>BE-BD-015</c>) dan alokasi aktif (<c>BE-BD-006</c>).
        /// </summary>
        /// <remarks>
        /// <b>Hanya kedua index itu yang ditangkap.</b> Pelanggaran unik lain — misalnya nomor
        /// kantong PMI ganda — sengaja dibiarkan naik sebagai galat, karena sebabnya berbeda dan
        /// menerjemahkannya menjadi <c>409</c> akan menyembunyikan kesalahan data yang sebenarnya.
        /// Nama constraint dibandingkan persis, dan tidak ada teks galat PostgreSQL, nama
        /// constraint, maupun SQL yang diteruskan ke pengguna.
        /// </remarks>
        private static bool IsSingleRowGuardViolation(DbUpdateException exception)
            => exception.InnerException is PostgresException postgres &&
               postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
               (string.Equals(
                    postgres.ConstraintName,
                    BbkBloodUnitPlacementConfiguration.CurrentUnitIndexName,
                    StringComparison.Ordinal) ||
                string.Equals(
                    postgres.ConstraintName,
                    BbkBloodUnitAllocationConfiguration.ActiveUnitIndexName,
                    StringComparison.Ordinal));

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

        /// <remarks>
        /// <paramref name="validationCode"/> bersifat opsional supaya seluruh penolakan yang
        /// memang tidak punya kode pada <c>validation-matrix</c> tetap memanggil helper ini
        /// dengan dua argumen seperti sebelumnya.
        /// </remarks>
        private static BloodUnitResult Failed(
            BloodUnitOutcome outcome,
            string message,
            string? validationCode = null)
            => new(outcome, null, message, validationCode);
    }

    /// <summary>Hasil satu tindakan atas kantong darah. Dipetakan ke HTTP status oleh controller.</summary>
    /// <param name="ValidationCode">
    /// Kode <c>VAL-BD-*</c> pada <c>contracts/validation-matrix.md</c> ketika penolakannya memang
    /// punya kode. <c>null</c> berarti penolakan itu tidak bernomor, dan controller tidak menulis
    /// apa pun ke slot <c>errors</c> — persis perilaku sebelum kode ini dibawa keluar.
    /// </param>
    public sealed record BloodUnitResult(
        BloodUnitOutcome Outcome,
        BbkBloodUnit? Entity,
        string Message,
        string? ValidationCode = null);

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
        VersionConflict = 4,

        /// <summary>
        /// Kantong sudah punya alokasi aktif — <c>VAL-BD-018c</c>, <c>409</c>. Dipisahkan dari
        /// <see cref="VersionConflict"/> karena sebabnya berbeda: bukan datanya sudah berubah,
        /// melainkan kantongnya sudah terikat pada baris kebutuhan lain.
        /// </summary>
        AllocationConflict = 5,

        /// <summary>
        /// Pelaku tidak berwenang atas tindakan ini — <c>403</c>. Dipisahkan dari
        /// <see cref="Invalid"/> karena sebabnya bukan bentuk isian yang salah melainkan
        /// kewenangan; <c>400</c> akan menyesatkan pembaca log. Dipakai <c>VAL-BD-021</c>
        /// pada jalur darurat (<c>BE-BD-008</c>).
        /// </summary>
        Forbidden = 6
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
