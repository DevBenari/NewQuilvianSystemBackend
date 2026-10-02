using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan permintaan darah ke PMI beserta penerimaannya.
    /// Controller tidak menyentuh <c>ApplicationDbContext</c> sendiri (<c>QBE-SVC-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>1. Nomor permintaan tidak pernah dihitung dari data.</b> <c>RequestNumber</c>
    /// diterbitkan <see cref="NumberSeriesAllocator"/> pada deret <c>BBK_PROVIDER_REQUEST</c>
    /// (<c>QBE-CODE-002/003/006</c>), dan seluruh penolakan diselesaikan lebih dulu supaya deret
    /// tidak berlubang oleh permintaan yang memang tidak sah (<c>INV-PLT-002</c>).
    /// </para>
    ///
    /// <para>
    /// <b>2. Satu order, paling banyak satu permintaan yang masih berjalan</b>
    /// (<c>BD-XINV-02</c>, <c>DEC-BD-008</c>). Diperiksa di bawah kunci penasihat per order dan
    /// dijaga index unik terfilter di database.
    /// </para>
    ///
    /// <para>
    /// <b>3. Sisa permintaan tidak pernah negatif, dan penerimaan tidak pernah ditolak karena
    /// kelebihan</b> (<c>BD-XINV-03</c>, <c>INV-BD-017</c>, <c>DEC-BD-025</c>). Jumlah diminta
    /// diturunkan dari baris order asal <b>per komponen</b>. Setiap kantong yang datang dihitung
    /// terhadap komponennya sendiri: selama komponen itu masih punya sisa, kantongnya terhitung
    /// memenuhi permintaan; begitu sisanya nol, kantong berikutnya ditandai <c>IsExcess</c>.
    /// Penerimaan diantrekan per permintaan dan menaikkan token <c>Version</c>, sehingga dua
    /// penerimaan yang hampir bersamaan tidak dapat sama-sama merasa masih di dalam kuota.
    /// </para>
    ///
    /// <para>
    /// <b>4. Kantong lahir <c>Received</c></b> — belum tersimpan, belum dapat dialokasikan
    /// (<c>DEC-BD-036</c>). Kantong berlebih pun lahir <c>Received</c>: matriks perpindahan status
    /// menuntutnya disimpan lebih dulu sebelum masuk <c>PendingReview</c>, dan perpindahan itu milik
    /// task penyimpanan <c>BE-BD-015</c>.
    /// </para>
    ///
    /// <para>
    /// <b>5. Kewenangan bukan urusan berkas ini.</b> Butir hak akses ditegakkan pada controller dan
    /// diberikan admin lewat layar Akses Role. Tidak ada pemeriksaan nama peran, nama jabatan,
    /// maupun <c>UserType</c> di sini.
    /// </para>
    /// </remarks>
    public class BbkProviderRequestService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        /// <summary>Penanda deret nomor permintaan darah ke PMI pada provider bersama.</summary>
        /// <remarks>
        /// Awalan, jumlah digit, dan kebijakan pengulangan milik Bank Darah (<c>QBE-CODE-005</c>,
        /// <c>DEC-PLT-005</c>); mesin alokasinya milik Platform. Deret baru memakai <c>NEVER</c>
        /// (<c>DEC-PLT-004</c>), mengikuti deret order <c>BBK_BLOOD_ORDER</c>.
        /// </remarks>
        private const string RequestSequenceKey = "BBK_PROVIDER_REQUEST";
        private const string RequestNumberPrefix = "PMI";
        private const int RequestSequenceDigits = 8;

        /// <summary>Nama tindakan pada <c>BbkTransitionHistory</c>.</summary>
        private const string CreateAction = "Create";
        private const string ReceiveAction = "Receive";
        private const string CancelAction = "Cancel";
        private const string CloseEncounterAction = "CloseEncounter";

        /// <summary>Nama index fisik yang dipetakan menjadi hasil bisnis, bukan galat server.</summary>
        private const string ActiveRequestIndexName = "IX_BbkProviderRequest_BloodOrderId_Active";
        private const string PmiBagNumberIndexName = "IX_BbkBloodUnit_PmiBagNumber";

        /// <summary>Toleransi selisih jam perangkat untuk waktu penerimaan fisik.</summary>
        private static readonly TimeSpan ReceivedAtFutureTolerance = TimeSpan.FromMinutes(5);

        private const string NotFoundMessage = "Permintaan darah ke PMI tidak ditemukan atau sudah dihapus.";

        private const string ActorUnknownMessage =
            "Petugas pelaku tidak dikenali. Masuk kembali lalu ulangi tindakan ini.";

        private const string ConcurrencyMessage =
            "Permintaan ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini.";

        /// <summary>Pesan <c>VAL-BD-006</c>, persis seperti matriks validasi.</summary>
        private const string DuplicateRequestMessage =
            "Sudah ada permintaan darah yang masih berjalan untuk kebutuhan ini. " +
            "Tidak boleh dibuat permintaan baru.";

        /// <summary>Pesan <c>VAL-BD-007</c>, persis seperti matriks validasi.</summary>
        private const string EmptyQuantityMessage = "Jumlah kantong yang diminta wajib diisi.";

        /// <summary>Pesan <c>VAL-BD-014</c> — peringatan, bukan penolakan.</summary>
        private const string ExcessWarningMessage =
            "Kiriman melebihi permintaan. Kantong tetap dicatat diterima dan masuk daftar menunggu keputusan.";

        /// <summary>Pesan <c>VAL-BD-003</c>, persis seperti matriks validasi.</summary>
        private const string ComponentNotInCatalogMessage =
            "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas.";

        /// <summary>Pesan <c>VAL-BD-016</c>, persis seperti matriks validasi.</summary>
        private const string UncontrolledReasonMessage =
            "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.";

        private const string DuplicateBagNumberMessage =
            "Nomor kantong PMI yang diinput sudah pernah tercatat diterima. Periksa kembali nomor kantongnya.";

        /// <summary>Salinan teks pada riwayat kantong berlebih — frasa <c>AC-BD-032</c>.</summary>
        private const string ExcessTransitionNote = "Kiriman melebihi permintaan.";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly BbkEncounterStatusReader _encounterStatusReader;

        public BbkProviderRequestService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            BbkEncounterStatusReader encounterStatusReader)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _encounterStatusReader = encounterStatusReader;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        public async Task<PagedResult<ProviderRequestListDto>> GetPagedAsync(
            string? search,
            Guid? patientId,
            Guid? bloodOrderId,
            BbkProviderRequestStatus? requestStatus,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (patientId.HasValue)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (bloodOrderId.HasValue)
                query = query.Where(x => x.BloodOrderId == bloodOrderId.Value);

            if (requestStatus.HasValue)
                query = query.Where(x => x.RequestStatus == requestStatus.Value);

            // Pencarian menyasar nomor permintaan, nomor order, nama pasien, dan nomor rekam medis.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    (x.BloodOrder != null && x.BloodOrder.OrderNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ProviderRequestListDto
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    BloodOrderId = x.BloodOrderId,
                    OrderNumber = x.BloodOrder != null ? x.BloodOrder.OrderNumber : null,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    RequestStatus = x.RequestStatus,
                    TotalRequestedQuantity = x.BloodOrder!.Lines
                        .Where(l => !l.IsDelete)
                        .Sum(l => (int?)l.RequestedQuantity) ?? 0,
                    TotalReceivedQuantity = x.Units.Count(u => !u.IsDelete),
                    TotalExcessQuantity = x.Units.Count(u => !u.IsDelete && u.IsExcess),
                    Version = x.Version,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.RequestStatusLabel = BbkDisplayLabels.Of(item.RequestStatus);

                // Kantong yang terhitung per komponen tidak pernah melebihi jumlah diminta
                // komponennya, sehingga selisih total ini sama dengan jumlah sisa per komponen.
                item.TotalOutstandingQuantity = Math.Max(
                    0,
                    item.TotalRequestedQuantity - (item.TotalReceivedQuantity - item.TotalExcessQuantity));
            }

            return new PagedResult<ProviderRequestListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<ProviderRequestSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new
                {
                    x.RequestStatus,
                    HasExcess = x.Units.Any(u => !u.IsDelete && u.IsExcess)
                })
                .ToListAsync(cancellationToken);

            return new ProviderRequestSummaryResponse
            {
                TotalRequest = rows.Count,
                RequestedRequest = rows.Count(x => x.RequestStatus == BbkProviderRequestStatus.Requested),
                PartiallyFulfilledRequest = rows.Count(x => x.RequestStatus == BbkProviderRequestStatus.PartiallyFulfilled),
                FulfilledRequest = rows.Count(x => x.RequestStatus == BbkProviderRequestStatus.Fulfilled),
                CancelledRequest = rows.Count(x => x.RequestStatus == BbkProviderRequestStatus.Cancelled),
                ClosedEncounterRequest = rows.Count(x => x.RequestStatus == BbkProviderRequestStatus.ClosedEncounter),
                RequestWithExcess = rows.Count(x => x.HasExcess)
            };
        }

        public async Task<ProviderRequestDetailDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await DetailQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return null;

            var detail = ToDetail(entity);

            detail.Transitions = await ReadTransitionsAsync(id, cancellationToken);
            detail.ClosedEncounterAt = detail.Transitions
                .LastOrDefault(x => x.Action == CloseEncounterAction)?
                .OccurredAt;

            return detail;
        }

        /// <summary>
        /// Riwayat perpindahan status satu permintaan, terlama lebih dulu. Memulangkan
        /// <c>null</c> bila permintaannya tidak ada.
        /// </summary>
        public async Task<List<BloodBankTransitionDto>?> GetStatusHistoryAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var exists = await BaseQuery().AnyAsync(x => x.Id == id, cancellationToken);

            return exists ? await ReadTransitionsAsync(id, cancellationToken) : null;
        }

        // =================================================================
        // Perubahan
        // =================================================================

        /// <summary>
        /// Membuat permintaan darah ke PMI dari satu order yang masih aktif
        /// (<c>state-transition-matrix.md</c> §2).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>"Order aktif" dibaca dua arah</b>: status order <c>Active</c> atau
        /// <c>PartiallyFulfilled</c>, <b>dan</b> kunjungan asalnya belum berakhir menurut
        /// <see cref="BbkEncounterStatusReader"/>. Order yang kunjungannya sudah berakhir akan
        /// kedaluwarsa (<c>DEC-BD-014</c>); meminta darah untuknya berarti memesan kantong yang
        /// tidak lagi punya pasien untuk diberikan — tafsiran yang sama dengan penolakan order baru
        /// pada kunjungan berakhir di <c>BE-BD-003</c>.
        /// </para>
        /// <para>
        /// <b>"Kebutuhan yang sama" pada <c>VAL-BD-006</c> dibaca per order.</b> Permintaan
        /// diturunkan dari satu order dan jumlahnya dari baris order itu, sehingga dua permintaan
        /// berjalan untuk satu order berarti meminta kebutuhan yang sama dua kali. Dua order
        /// berbeda — termasuk order ganda yang dilanjutkan dengan alasan tertulis — adalah dua
        /// kebutuhan yang berbeda.
        /// </para>
        /// </remarks>
        public async Task<ProviderRequestResult> CreateAsync(
            CreateProviderRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(ProviderRequestOutcome.Invalid, ActorUnknownMessage);

            if (request.BloodOrderId == Guid.Empty)
                return Failed(ProviderRequestOutcome.Invalid, "Order darah asal permintaan wajib dipilih.");

            var order = await _dbContext.Set<BbkBloodOrder>()
                .AsNoTracking()
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                .FirstOrDefaultAsync(
                    x => x.Id == request.BloodOrderId && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

            if (order == null)
                return Failed(ProviderRequestOutcome.Invalid, "Order darah asal tidak ditemukan atau sudah dihapus.");

            if (!IsOrderOpen(order.OrderStatus))
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    $"Order darah berstatus {BbkDisplayLabels.Of(order.OrderStatus)} tidak dapat dibuatkan permintaan ke PMI.");
            }

            // VAL-BD-007: jumlah yang diminta diturunkan dari baris order. Order tanpa kebutuhan
            // yang dapat diminta tidak menghasilkan permintaan kosong.
            if (order.Lines.Sum(x => x.RequestedQuantity) <= 0)
                return Failed(ProviderRequestOutcome.Invalid, EmptyQuantityMessage);

            if (await _encounterStatusReader.IsEncounterClosedAsync(order.EncounterId, cancellationToken))
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    "Kunjungan asal order ini sudah berakhir, sehingga tidak dapat dibuatkan permintaan darah ke PMI.");
            }

            // Satu transaksi dari pemeriksaan permintaan berjalan sampai permintaan tersimpan,
            // dijaga kunci penasihat per order. Tanpa kunci ini dua petugas yang menyimpan
            // permintaan untuk order yang sama hampir bersamaan sama-sama lolos BD-XINV-02.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquireAdvisoryLockAsync($"BBK_PROVIDER_REQUEST_{order.Id:N}", cancellationToken);

            if (await HasActiveRequestAsync(order.Id, cancellationToken))
                return Failed(ProviderRequestOutcome.DuplicateRequest, DuplicateRequestMessage);

            var now = DateTime.UtcNow;

            // Nomor diminta paling akhir: seluruh penolakan sudah selesai di atas (INV-PLT-002).
            string requestNumber;

            try
            {
                requestNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: RequestSequenceKey,
                        Prefix: RequestNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: RequestSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return Failed(
                    ProviderRequestOutcome.Invalid,
                    "Nomor permintaan darah gagal diterbitkan. Hubungi administrator sistem.");
            }

            var entity = new BbkProviderRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = requestNumber,
                BloodOrderId = order.Id,
                PatientId = order.PatientId,
                RequestStatus = BbkProviderRequestStatus.Requested,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkProviderRequest>().Add(entity);

            AppendTransition(
                BbkTransitionScopes.ProviderRequest,
                entity.Id,
                CreateAction,
                fromStatus: null,
                toStatus: BbkProviderRequestStatus.Requested.ToString(),
                reasonCode: null,
                reasonNote: null,
                actorUserId,
                occurredAt: now,
                correlationId: null);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex, ActiveRequestIndexName))
            {
                // Penjaga fisik BD-XINV-02 menangkap permintaan kedua yang lolos di luar kunci.
                _dbContext.ChangeTracker.Clear();

                return Failed(ProviderRequestOutcome.DuplicateRequest, DuplicateRequestMessage);
            }

            await transaction.CommitAsync(cancellationToken);

            return Succeeded(entity, "Permintaan darah ke PMI berhasil dibuat.");
        }

        /// <summary>
        /// Mencatat satu kedatangan fisik kantong, termasuk yang melebihi jumlah diminta
        /// (<c>DEC-BD-025</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Contoh berangka.</b> Order meminta PRC 2 kantong. PMI mengirim 3 PRC sekaligus: dua
        /// kantong pertama terhitung memenuhi permintaan, kantong ketiga ditandai berlebih,
        /// permintaan menjadi <c>Fulfilled</c> dengan sisa <b>0 — bukan −1</b>, dan ketiga kantong
        /// tercatat membawa rujukan permintaan asalnya (<c>AC-BD-031</c>).
        /// </para>
        /// <para>
        /// <b>Status yang menerima kantong</b> mengikuti matriks perpindahan status §2:
        /// <c>Requested</c>, <c>PartiallyFulfilled</c>, dan <c>ClosedEncounter</c>. Permintaan
        /// <c>ClosedEncounter</c> tetap mencatat kantong susulan tetapi tidak kembali aktif
        /// (<c>AC-BD-023</c>). <c>Fulfilled</c> dan <c>Cancelled</c> terminal.
        /// </para>
        /// </remarks>
        public async Task<ProviderRequestResult> RecordReceiptAsync(
            Guid id,
            RecordReceiptRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(ProviderRequestOutcome.Invalid, ActorUnknownMessage);

            if (request.Units == null || request.Units.Count == 0)
                return Failed(ProviderRequestOutcome.Invalid, "Penerimaan wajib memuat minimal satu kantong.");

            var units = request.Units
                .Select(x => (BagNumber: x.PmiBagNumber?.Trim() ?? string.Empty, x.BloodComponentId))
                .ToList();

            if (units.Any(x => x.BagNumber.Length == 0))
                return Failed(ProviderRequestOutcome.Invalid, "Nomor kantong PMI wajib diisi untuk setiap kantong.");

            if (units.Any(x => x.BagNumber.Length > 50))
                return Failed(ProviderRequestOutcome.Invalid, "Nomor kantong PMI paling panjang 50 karakter.");

            if (units.Any(x => x.BloodComponentId == Guid.Empty))
                return Failed(ProviderRequestOutcome.Invalid, ComponentNotInCatalogMessage);

            if (units.Select(x => x.BagNumber).Distinct(StringComparer.Ordinal).Count() != units.Count)
            {
                return Failed(
                    ProviderRequestOutcome.DuplicateBagNumber,
                    "Satu nomor kantong PMI tercantum lebih dari sekali pada penerimaan ini.");
            }

            var now = DateTime.UtcNow;

            // Npgsql menuntut DateTime berjenis UTC. Waktu dari layar dinormalkan dulu.
            var receivedAt = request.ReceivedAt.HasValue
                ? DateTime.SpecifyKind(request.ReceivedAt.Value.ToUniversalTime(), DateTimeKind.Utc)
                : now;

            if (receivedAt > now.Add(ReceivedAtFutureTolerance))
                return Failed(ProviderRequestOutcome.Invalid, "Waktu penerimaan fisik tidak boleh melewati waktu sekarang.");

            var componentIds = units.Select(x => x.BloodComponentId).Distinct().ToList();

            var knownComponentCount = await _dbContext.Set<MstBloodComponent>()
                .AsNoTracking()
                .CountAsync(x => componentIds.Contains(x.Id) && !x.IsDelete, cancellationToken);

            if (knownComponentCount != componentIds.Count)
                return Failed(ProviderRequestOutcome.Invalid, ComponentNotInCatalogMessage);

            // Penerimaan diantrekan per permintaan, lalu seluruh hitungan dibaca ulang sesudah
            // kunci didapat — sehingga penerimaan kedua selalu melihat kantong penerimaan pertama.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquireAdvisoryLockAsync($"BBK_PROVIDER_REQUEST_RECEIPT_{id:N}", cancellationToken);

            var entity = await _dbContext.Set<BbkProviderRequest>()
                .Include(x => x.BloodOrder)
                    .ThenInclude(x => x!.Lines)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

            if (entity == null)
                return Failed(ProviderRequestOutcome.NotFound, NotFoundMessage);

            if (!CanReceive(entity.RequestStatus))
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    $"Permintaan berstatus {BbkDisplayLabels.Of(entity.RequestStatus)} tidak dapat menerima kantong lagi.");
            }

            var bagNumbers = units.Select(x => x.BagNumber).ToList();

            if (await _dbContext.Set<BbkBloodUnit>().AsNoTracking().AnyAsync(x => bagNumbers.Contains(x.PmiBagNumber), cancellationToken))
                return Failed(ProviderRequestOutcome.DuplicateBagNumber, DuplicateBagNumberMessage);

            var requested = (entity.BloodOrder?.Lines ?? new List<BbkBloodOrderLine>())
                .Where(x => !x.IsDelete)
                .GroupBy(x => x.BloodComponentId)
                .ToDictionary(x => x.Key, x => x.Sum(l => l.RequestedQuantity));

            var counted = await _dbContext.Set<BbkBloodUnit>()
                .AsNoTracking()
                .Where(x => x.ProviderRequestId == entity.Id && !x.IsDelete && !x.IsExcess)
                .GroupBy(x => x.BloodComponentId)
                .Select(x => new { ComponentId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.ComponentId, x => x.Count, cancellationToken);

            // Urutan kedatangan di dalam satu permintaan. Dihitung di bawah kunci penasihat per
            // permintaan dan dijaga index unik (ProviderRequestId, Sequence) — bukan nomor bisnis.
            var sequence = await _dbContext.Set<BbkBloodUnitReceipt>()
                .CountAsync(x => x.ProviderRequestId == entity.Id, cancellationToken) + 1;

            var receipt = new BbkBloodUnitReceipt
            {
                Id = Guid.NewGuid(),
                ProviderRequestId = entity.Id,
                ReceivedQuantity = units.Count,
                ReceivedAt = receivedAt,
                ReceivedByUserId = actorUserId,
                Sequence = sequence,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkBloodUnitReceipt>().Add(receipt);

            var excessCount = 0;

            foreach (var unit in units)
            {
                requested.TryGetValue(unit.BloodComponentId, out var requestedQuantity);
                counted.TryGetValue(unit.BloodComponentId, out var countedQuantity);

                // Selama komponen ini masih punya sisa, kantongnya terhitung memenuhi permintaan.
                // Komponen yang tidak diminta sama sekali punya jumlah diminta nol, sehingga
                // seluruh kantongnya berlebih — dicatat, tidak ditolak.
                var isExcess = countedQuantity >= requestedQuantity;

                if (isExcess)
                    excessCount++;
                else
                    counted[unit.BloodComponentId] = countedQuantity + 1;

                var bloodUnit = new BbkBloodUnit
                {
                    Id = Guid.NewGuid(),
                    PmiBagNumber = unit.BagNumber,
                    ProviderRequestId = entity.Id,
                    ReceiptId = receipt.Id,
                    BloodComponentId = unit.BloodComponentId,
                    IsExcess = isExcess,
                    UnitStatus = BbkBloodUnitStatus.Received,
                    Version = 0,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<BbkBloodUnit>().Add(bloodUnit);

                AppendTransition(
                    BbkTransitionScopes.BloodUnit,
                    bloodUnit.Id,
                    ReceiveAction,
                    fromStatus: null,
                    toStatus: BbkBloodUnitStatus.Received.ToString(),
                    reasonCode: null,
                    reasonNote: isExcess ? ExcessTransitionNote : null,
                    actorUserId,
                    occurredAt: receivedAt,
                    correlationId: receipt.Id);
            }

            var totalRequested = requested.Values.Sum();
            var totalCounted = requested.Keys.Sum(x => counted.TryGetValue(x, out var value) ? value : 0);

            var fromStatus = entity.RequestStatus;
            var toStatus = StatusAfterReceipt(fromStatus, totalRequested, totalCounted);

            entity.RequestStatus = toStatus;
            entity.Version++;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (toStatus != fromStatus)
            {
                AppendTransition(
                    BbkTransitionScopes.ProviderRequest,
                    entity.Id,
                    ReceiveAction,
                    fromStatus: fromStatus.ToString(),
                    toStatus: toStatus.ToString(),
                    reasonCode: null,
                    reasonNote: null,
                    actorUserId,
                    occurredAt: receivedAt,
                    correlationId: receipt.Id);
            }

            var saveOutcome = await SaveWithGuardsAsync(cancellationToken);

            if (saveOutcome != ProviderRequestOutcome.Success)
            {
                return Failed(
                    saveOutcome,
                    saveOutcome == ProviderRequestOutcome.DuplicateBagNumber
                        ? DuplicateBagNumberMessage
                        : ConcurrencyMessage);
            }

            await transaction.CommitAsync(cancellationToken);

            return new ProviderRequestResult(
                ProviderRequestOutcome.Success,
                entity,
                excessCount > 0 ? ExcessWarningMessage : "Penerimaan kantong berhasil dicatat.",
                excessCount);
        }

        /// <summary>Membatalkan permintaan dengan alasan terkendali (<c>VAL-BD-016</c>).</summary>
        /// <remarks>
        /// Matriks perpindahan status hanya menuntut alasan <b>terkendali</b> — dipilih dari
        /// <c>MstBloodBankReason</c> yang aktif — dan tidak menetapkan kategori alasan tertentu
        /// untuk pembatalan permintaan. Karena itu tidak ada penyaringan kategori di sini;
        /// menambahkannya berarti mengarang aturan yang belum diputuskan. Kantong yang sudah
        /// diterima tidak tersentuh pembatalan.
        /// </remarks>
        public async Task<ProviderRequestResult> CancelAsync(
            Guid id,
            CancelProviderRequestRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(ProviderRequestOutcome.Invalid, ActorUnknownMessage);

            var reasonCode = NormalizeCode(request.ReasonCode);

            if (string.IsNullOrWhiteSpace(reasonCode))
                return Failed(ProviderRequestOutcome.Invalid, UncontrolledReasonMessage);

            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(ProviderRequestOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != entity.Version)
                return Failed(ProviderRequestOutcome.VersionConflict, ConcurrencyMessage);

            if (!IsOpen(entity.RequestStatus))
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    $"Permintaan berstatus {BbkDisplayLabels.Of(entity.RequestStatus)} tidak dapat dibatalkan.");
            }

            var reason = await _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete && x.IsActive && x.ReasonCode.ToUpper() == reasonCode,
                    cancellationToken);

            if (reason == null)
                return Failed(ProviderRequestOutcome.Invalid, UncontrolledReasonMessage);

            var now = DateTime.UtcNow;
            var fromStatus = entity.RequestStatus;

            entity.RequestStatus = BbkProviderRequestStatus.Cancelled;
            entity.Version++;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            AppendTransition(
                BbkTransitionScopes.ProviderRequest,
                entity.Id,
                CancelAction,
                fromStatus: fromStatus.ToString(),
                toStatus: BbkProviderRequestStatus.Cancelled.ToString(),
                reasonCode: reason.ReasonCode,
                reasonNote: reason.ReasonText,
                actorUserId,
                occurredAt: now,
                correlationId: null);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(ProviderRequestOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(entity, "Permintaan darah ke PMI berhasil dibatalkan.");
        }

        /// <summary>
        /// Menutup administratif permintaan yang masih kurang karena kunjungan asalnya berakhir
        /// (<c>DEC-BD-020</c>).
        /// </summary>
        /// <remarks>
        /// <b>Tidak punya endpoint</b>, sesuai kontrak: penutupan dipicu sistem dari sinyal
        /// kunjungan, bukan oleh seseorang yang menekan tombol. Riwayat penerimaannya tetap utuh,
        /// dan kantong yang masih datang sesudahnya tetap dapat dicatat (<c>AC-BD-022</c>,
        /// <c>AC-BD-023</c>). Pemicu terjadwalnya berada di luar cakupan <c>BE-BD-004</c>.
        /// </remarks>
        public async Task<ProviderRequestResult> CloseForEncounterEndAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<BbkProviderRequest>()
                .Include(x => x.BloodOrder)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

            if (entity == null || entity.BloodOrder == null)
                return Failed(ProviderRequestOutcome.NotFound, NotFoundMessage);

            if (!IsOpen(entity.RequestStatus))
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    $"Permintaan berstatus {BbkDisplayLabels.Of(entity.RequestStatus)} tidak dapat ditutup karena kunjungan berakhir.");
            }

            var closure = await _encounterStatusReader.ReadAsync(entity.BloodOrder.EncounterId, cancellationToken);

            if (!closure.IsClosed)
            {
                return Failed(
                    ProviderRequestOutcome.NotAllowedByState,
                    "Kunjungan asal permintaan ini masih berjalan, sehingga permintaannya belum ditutup.");
            }

            var now = DateTime.UtcNow;
            var fromStatus = entity.RequestStatus;

            entity.RequestStatus = BbkProviderRequestStatus.ClosedEncounter;
            entity.Version++;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            // OccurredAt adalah waktu kunjungan benar-benar berakhir, bukan waktu pemicu berjalan.
            AppendTransition(
                BbkTransitionScopes.ProviderRequest,
                entity.Id,
                CloseEncounterAction,
                fromStatus: fromStatus.ToString(),
                toStatus: BbkProviderRequestStatus.ClosedEncounter.ToString(),
                reasonCode: null,
                reasonNote: $"Kunjungan berakhir — sinyal {closure.Signal}.",
                actorUserId,
                occurredAt: closure.ClosedAt ?? now,
                correlationId: null);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(ProviderRequestOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(
                entity,
                "Permintaan darah ke PMI ditutup karena kunjungan asalnya berakhir. Riwayat penerimaannya tetap utuh.");
        }

        // =================================================================
        // Riwayat perpindahan status
        // =================================================================

        /// <summary>Menambahkan satu baris riwayat. Append-only.</summary>
        private void AppendTransition(
            string scope,
            Guid entityId,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt,
            Guid? correlationId)
        {
            _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
            {
                Id = Guid.NewGuid(),
                Scope = scope,
                EntityId = entityId,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = Truncate(reasonNote, 500),
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CorrelationId = correlationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        private async Task<List<BloodBankTransitionDto>> ReadTransitionsAsync(
            Guid requestId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkTransitionHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.Scope == BbkTransitionScopes.ProviderRequest &&
                    x.EntityId == requestId &&
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

        // =================================================================
        // Pemetaan dan metadata
        // =================================================================

        /// <summary>
        /// Menyusun detail beserta pemenuhan per komponen. Jumlah diminta dari baris order;
        /// jumlah diterima dan berlebih dari kantong yang benar-benar lahir.
        /// </summary>
        public static ProviderRequestDetailDto ToDetail(BbkProviderRequest entity)
        {
            var lines = entity.BloodOrder?.Lines
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .ToList() ?? new List<BbkBloodOrderLine>();

            var receipts = entity.Receipts
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .ToList();

            var units = receipts
                .SelectMany(x => x.Units.Where(u => !u.IsDelete))
                .ToList();

            var componentIds = lines.Select(x => x.BloodComponentId)
                .Concat(units.Select(x => x.BloodComponentId))
                .Distinct()
                .ToList();

            var components = componentIds
                .Select(componentId =>
                {
                    var line = lines.FirstOrDefault(x => x.BloodComponentId == componentId);
                    var unit = units.FirstOrDefault(x => x.BloodComponentId == componentId);
                    var catalog = line?.BloodComponent ?? unit?.BloodComponent;

                    var requestedQuantity = lines.Where(x => x.BloodComponentId == componentId).Sum(x => x.RequestedQuantity);
                    var receivedQuantity = units.Count(x => x.BloodComponentId == componentId && !x.IsExcess);
                    var excessQuantity = units.Count(x => x.BloodComponentId == componentId && x.IsExcess);

                    return new ProviderRequestComponentDto
                    {
                        BloodComponentId = componentId,
                        BloodComponentCode = catalog?.ComponentCode,
                        BloodComponentName = catalog?.ComponentName,
                        RequestedQuantity = requestedQuantity,
                        ReceivedQuantity = receivedQuantity,
                        ExcessQuantity = excessQuantity,
                        OutstandingQuantity = Math.Max(0, requestedQuantity - receivedQuantity)
                    };
                })
                .ToList();

            return new ProviderRequestDetailDto
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                BloodOrderId = entity.BloodOrderId,
                OrderNumber = entity.BloodOrder?.OrderNumber,
                OrderStatus = entity.BloodOrder?.OrderStatus,
                OrderStatusLabel = entity.BloodOrder != null ? BbkDisplayLabels.Of(entity.BloodOrder.OrderStatus) : null,
                EncounterId = entity.BloodOrder?.EncounterId,
                EncounterNumber = entity.BloodOrder?.Encounter?.EncounterNumber,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber,
                RequestStatus = entity.RequestStatus,
                RequestStatusLabel = BbkDisplayLabels.Of(entity.RequestStatus),
                TotalRequestedQuantity = components.Sum(x => x.RequestedQuantity),
                TotalReceivedQuantity = units.Count,
                TotalExcessQuantity = units.Count(x => x.IsExcess),
                TotalOutstandingQuantity = components.Sum(x => x.OutstandingQuantity),
                Version = entity.Version,
                Components = components,
                Receipts = receipts
                    .Select(x => new ProviderReceiptDto
                    {
                        Id = x.Id,
                        Sequence = x.Sequence,
                        ReceivedQuantity = x.ReceivedQuantity,
                        ExcessQuantity = x.Units.Count(u => !u.IsDelete && u.IsExcess),
                        ReceivedAt = x.ReceivedAt,
                        ReceivedByUserId = x.ReceivedByUserId,
                        Units = x.Units
                            .Where(u => !u.IsDelete)
                            .OrderBy(u => u.CreateDateTime)
                            .Select(u => new ProviderReceiptUnitDto
                            {
                                Id = u.Id,
                                PmiBagNumber = u.PmiBagNumber,
                                BloodComponentId = u.BloodComponentId,
                                BloodComponentCode = u.BloodComponent?.ComponentCode,
                                BloodComponentName = u.BloodComponent?.ComponentName,
                                IsExcess = u.IsExcess,
                                UnitStatus = u.UnitStatus,
                                UnitStatusLabel = BbkDisplayLabels.Of(u.UnitStatus)
                            })
                            .ToList()
                    })
                    .ToList(),
                AvailableActions = AvailableActionsOf(entity.RequestStatus),
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        /// <summary>
        /// Aksi yang layak dicoba pada keadaan sekarang. <b>Kelayakan status saja</b> — bukan
        /// kewenangan.
        /// </summary>
        private static List<string> AvailableActionsOf(BbkProviderRequestStatus status)
        {
            var actions = new List<string>();

            if (CanReceive(status))
                actions.Add("Receive");

            if (IsOpen(status))
                actions.Add("Cancel");

            return actions;
        }

        public static ProviderRequestFilterMetadataResponse BuildFilterMetadata() => new()
        {
            DefaultFilter = new ProviderRequestDefaultFilterResponse(),
            RequestStatusOptions = Enum.GetValues<BbkProviderRequestStatus>()
                .Select(x => new BloodBankOptionItemResponse
                {
                    Value = (int)x,
                    Label = BbkDisplayLabels.Of(x)
                })
                .ToList(),
            SortOptions = new List<BloodBankSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Waktu dibuat" },
                new() { Value = "requestNumber", Label = "Nomor permintaan" },
                new() { Value = "requestStatus", Label = "Status permintaan" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        // =================================================================
        // Penolong
        // =================================================================

        /// <summary>Permintaan yang masih berjalan: dapat dibatalkan dan menahan permintaan kedua.</summary>
        private static bool IsOpen(BbkProviderRequestStatus status)
            => status is BbkProviderRequestStatus.Requested or BbkProviderRequestStatus.PartiallyFulfilled;

        /// <summary>Status yang masih menerima kantong, sesuai matriks perpindahan status §2.</summary>
        private static bool CanReceive(BbkProviderRequestStatus status)
            => IsOpen(status) || status == BbkProviderRequestStatus.ClosedEncounter;

        private static bool IsOrderOpen(BbkBloodOrderStatus status)
            => status is BbkBloodOrderStatus.Active or BbkBloodOrderStatus.PartiallyFulfilled;

        /// <summary>
        /// Status permintaan sesudah satu penerimaan, diturunkan dari kantong yang terhitung.
        /// </summary>
        /// <remarks>
        /// <c>ClosedEncounter</c> tidak pernah kembali aktif. Selain itu: seluruh kebutuhan
        /// terpenuhi → <c>Fulfilled</c>; sebagian → <c>PartiallyFulfilled</c>; belum satu pun
        /// kantong yang terhitung — misalnya kiriman hanya berisi komponen yang tidak diminta —
        /// status tidak berubah.
        /// </remarks>
        private static BbkProviderRequestStatus StatusAfterReceipt(
            BbkProviderRequestStatus current,
            int totalRequested,
            int totalCounted)
        {
            if (current == BbkProviderRequestStatus.ClosedEncounter)
                return current;

            if (totalRequested > 0 && totalCounted >= totalRequested)
                return BbkProviderRequestStatus.Fulfilled;

            return totalCounted > 0 ? BbkProviderRequestStatus.PartiallyFulfilled : current;
        }

        private Task<bool> HasActiveRequestAsync(Guid bloodOrderId, CancellationToken cancellationToken)
            => _dbContext.Set<BbkProviderRequest>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.BloodOrderId == bloodOrderId &&
                         !x.IsDelete &&
                         (x.RequestStatus == BbkProviderRequestStatus.Requested ||
                          x.RequestStatus == BbkProviderRequestStatus.PartiallyFulfilled),
                    cancellationToken);

        /// <summary>
        /// Mengantrekan tulisan yang berebut kunci yang sama. Polanya sama dengan
        /// <c>BbkBloodOrderService</c>: <c>pg_advisory_xact_lock</c> hidup selama transaksi dan lepas
        /// sendiri ketika transaksi selesai. Di luar PostgreSQL — pengujian InMemory — langkah ini
        /// dilewati, sehingga perilaku berebut hanya dapat dibuktikan terhadap PostgreSQL.
        /// </summary>
        private async Task AcquireAdvisoryLockAsync(string lockKey, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        /// <summary>
        /// Menyimpan perubahan dan menerjemahkan bentrok token <c>Version</c> menjadi hasil
        /// <c>409</c> alih-alih galat server.
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
        }

        /// <summary>
        /// Menyimpan penerimaan dengan dua penjaga: token <c>Version</c> permintaan dan index unik
        /// nomor kantong PMI. Keduanya dipetakan menjadi hasil bisnis; transaksi yang tidak
        /// di-commit dibatalkan seluruhnya, termasuk kantong dan baris riwayatnya.
        /// </summary>
        private async Task<ProviderRequestOutcome> SaveWithGuardsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return ProviderRequestOutcome.Success;
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();

                return ProviderRequestOutcome.VersionConflict;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex, PmiBagNumberIndexName))
            {
                _dbContext.ChangeTracker.Clear();

                return ProviderRequestOutcome.DuplicateBagNumber;
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception, string indexName)
            => exception.InnerException is PostgresException postgres &&
               postgres.SqlState == PostgresErrorCodes.UniqueViolation &&
               string.Equals(postgres.ConstraintName, indexName, StringComparison.Ordinal);

        private IQueryable<BbkProviderRequest> BaseQuery()
            => _dbContext.Set<BbkProviderRequest>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private IQueryable<BbkProviderRequest> DetailQuery()
            => BaseQuery()
                .Include(x => x.Patient)
                .Include(x => x.BloodOrder)
                    .ThenInclude(x => x!.Encounter)
                .Include(x => x.BloodOrder)
                    .ThenInclude(x => x!.Lines.Where(l => !l.IsDelete))
                        .ThenInclude(x => x.BloodComponent)
                .Include(x => x.Receipts.Where(r => !r.IsDelete))
                    .ThenInclude(x => x.Units.Where(u => !u.IsDelete))
                        .ThenInclude(x => x.BloodComponent)
                .AsSplitQuery();

        private Task<BbkProviderRequest?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<BbkProviderRequest>()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static IQueryable<BbkProviderRequest> ApplySort(
            IQueryable<BbkProviderRequest> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "requestnumber" => descending
                    ? query.OrderByDescending(x => x.RequestNumber)
                    : query.OrderBy(x => x.RequestNumber),
                "requeststatus" => descending
                    ? query.OrderByDescending(x => x.RequestStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.RequestStatus).ThenBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static string NormalizeCode(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        private static string? Truncate(string? value, int maxLength)
            => value != null && value.Length > maxLength ? value[..maxLength] : value;

        private static ProviderRequestResult Succeeded(BbkProviderRequest entity, string message)
            => new(ProviderRequestOutcome.Success, entity, message, 0);

        private static ProviderRequestResult Failed(ProviderRequestOutcome outcome, string message)
            => new(outcome, null, message, 0);
    }

    /// <summary>
    /// Hasil satu tindakan pada permintaan darah ke PMI. Dipetakan ke HTTP status oleh controller.
    /// </summary>
    /// <param name="Outcome">Jenis hasilnya.</param>
    /// <param name="Entity">Permintaan yang terpengaruh, bila tindakannya berhasil.</param>
    /// <param name="Message">Pesan bagi pengguna, dalam Bahasa Indonesia.</param>
    /// <param name="ExcessUnitCount">
    /// Jumlah kantong berlebih pada penerimaan ini. Lebih dari nol berarti balasan membawa
    /// peringatan <c>VAL-BD-014</c> — tetap <c>200</c>, karena kelebihan tidak pernah ditolak.
    /// </param>
    public sealed record ProviderRequestResult(
        ProviderRequestOutcome Outcome,
        BbkProviderRequest? Entity,
        string Message,
        int ExcessUnitCount);

    /// <summary>Jenis hasil satu tindakan pada permintaan darah ke PMI.</summary>
    public enum ProviderRequestOutcome
    {
        Success = 0,

        /// <summary>Permintaan tidak ada atau sudah dihapus.</summary>
        NotFound = 1,

        /// <summary>Isian permintaan tidak sah.</summary>
        Invalid = 2,

        /// <summary>Sudah ada permintaan berjalan untuk order yang sama (<c>VAL-BD-006</c>).</summary>
        DuplicateRequest = 3,

        /// <summary>Keadaan datanya tidak memenuhi syarat tindakan ini.</summary>
        NotAllowedByState = 4,

        /// <summary>Permintaan sudah berubah di tangan orang lain.</summary>
        VersionConflict = 5,

        /// <summary>Nomor kantong PMI sudah pernah tercatat, atau tercantum dua kali.</summary>
        DuplicateBagNumber = 6
    }
}
