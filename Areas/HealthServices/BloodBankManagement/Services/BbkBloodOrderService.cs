using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan order darah. Controller tidak menyentuh
    /// <c>ApplicationDbContext</c> sendiri (<c>QBE-SVC-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lima aturan melekat di berkas ini, dan tiga yang pertama adalah aturan keselamatan —
    /// bukan preferensi arsitektur.
    /// </para>
    ///
    /// <para>
    /// <b>1. Nomor order tidak pernah dihitung dari data.</b> <c>OrderNumber</c> diterbitkan
    /// <see cref="NumberSeriesAllocator"/> pada deret <c>BBK_BLOOD_ORDER</c>. <c>Count+1</c>,
    /// <c>Max+1</c>, dan pemindaian nomor terbit dilarang (<c>QBE-CODE-002/003</c>,
    /// <c>QBE-CODE-006</c>). Konsekuensi yang disengaja: bila order gagal tersimpan setelah
    /// nomornya terbit, nomor itu <b>hangus selamanya</b> dan deret berlubang
    /// (<c>INV-PLT-002</c>). Lubang lebih murah daripada satu nomor yang menempel pada dua
    /// order.
    /// </para>
    ///
    /// <para>
    /// <b>2. Deteksi order ganda menahan, bukan menolak selamanya.</b> <c>BD-XINV-01</c>
    /// membandingkan pasien + kunjungan + komponen terhadap order yang <b>masih aktif</b>.
    /// Ketika cocok, pembuatan tertahan <c>422 VAL-BD-001</c> dan pemanggil dipersilakan
    /// melanjutkan lewat <see cref="ConfirmDuplicateAsync"/> dengan alasan tertulis
    /// (<c>ASM-BD-001</c>). Order yang kunjungannya sudah berakhir <b>tidak lagi menahan</b>
    /// apa pun (<c>AC-BD-004</c>).
    /// </para>
    ///
    /// <para>
    /// <b>3. Tidak ada pembatalan order tanpa audit</b> (<c>INV-BD-035</c>). Alasan wajib
    /// dipilih dari <c>MstBloodBankReason</c> (<c>VAL-BD-016</c>), kategorinya wajib salah satu
    /// dari kedua kategori pembatalan order, dan setiap pembatalan meninggalkan baris
    /// <c>BbkTransitionHistory</c> berisi pelaku, waktu, status asal, status tujuan, kode
    /// alasan, dan <b>salinan teks</b> alasannya.
    /// </para>
    ///
    /// <para>
    /// <b>4. Angka pemenuhan dihitung, tidak disimpan</b> (<c>BD-DOM-17</c>). Lihat catatan
    /// jujur pada <see cref="GetFulfillmentAsync"/> tentang batas perhitungan pada tahap
    /// pengiriman saat ini.
    /// </para>
    ///
    /// <para>
    /// <b>5. Kewenangan bukan urusan berkas ini.</b> Pemisahan <c>BloodOrder : Cancel</c> dari
    /// <c>BloodOrder : Update</c> (<c>DEC-BD-044</c>) ditegakkan butir hak akses pada
    /// controller, yang diberikan admin lewat layar Akses Role. Tidak ada satu pun pemeriksaan
    /// nama peran, nama jabatan, atau <c>UserType</c> di sini, dan tidak boleh ada. Yang
    /// diperiksa service adalah <b>kesesuaian kategori alasan dengan kewenangan yang melekat
    /// pada data</b> — dokter peminta order ini, atau bukan.
    /// </para>
    /// </remarks>
    public class BbkBloodOrderService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        /// <summary>Penanda deret nomor order darah pada provider bersama.</summary>
        /// <remarks>
        /// Awalan, jumlah digit, dan kebijakan pengulangan milik Bank Darah
        /// (<c>DEC-PLT-005</c>); mesin alokasinya milik Platform. Deret baru memakai
        /// <c>NEVER</c> (<c>DEC-PLT-004</c>), sehingga nomor tidak pernah diulang per tahun.
        /// </remarks>
        private const string OrderSequenceKey = "BBK_BLOOD_ORDER";
        private const string OrderNumberPrefix = "ORD";
        private const int OrderSequenceDigits = 8;

        /// <summary>Nama tindakan pada <c>BbkTransitionHistory</c> untuk scope order.</summary>
        private const string CreateAction = "Create";
        private const string ConfirmedDuplicateAction = "CreateConfirmedDuplicate";
        private const string CancelAction = "Cancel";
        private const string ExpireAction = "Expire";

        private const string NotFoundMessage = "Order darah tidak ditemukan atau sudah dihapus.";

        private const string ActorUnknownMessage =
            "Petugas pelaku tidak dikenali. Masuk kembali lalu ulangi tindakan ini.";

        private const string ConcurrencyMessage =
            "Order ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini.";

        /// <summary>Pesan <c>VAL-BD-083</c>, persis seperti matriks validasi.</summary>
        private const string CategoryMismatchMessage =
            "Pilih alasan pembatalan yang sesuai: pembatalan klinis oleh dokter peminta, " +
            "atau pembatalan operasional oleh petugas Bank Darah.";

        /// <summary>Pesan <c>VAL-BD-010</c>, persis seperti matriks validasi.</summary>
        private const string ManualIncompleteMessage =
            "Order manual wajib mengisi pasien, kunjungan, dokter peminta, unit asal, " +
            "dan petugas yang menginput.";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly BbkEncounterStatusReader _encounterStatusReader;

        public BbkBloodOrderService(
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

        public async Task<PagedResult<BloodOrderListDto>> GetPagedAsync(
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? serviceUnitId,
            BbkBloodOrderStatus? orderStatus,
            BbkOrderSource? orderSource,
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

            if (encounterId.HasValue)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (serviceUnitId.HasValue)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (orderStatus.HasValue)
                query = query.Where(x => x.OrderStatus == orderStatus.Value);

            if (orderSource.HasValue)
                query = query.Where(x => x.OrderSource == orderSource.Value);

            // Pencarian menyasar nomor order, nomor rekam medis, dan nama pasien: ketiganya
            // yang benar-benar dipegang petugas ketika mencari satu order.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.OrderNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodOrderListDto
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    RequestingDoctorId = x.RequestingDoctorId,
                    RequestingDoctorName = x.RequestingDoctor != null ? x.RequestingDoctor.FullName : null,
                    OrderSource = x.OrderSource,
                    OrderStatus = x.OrderStatus,
                    TotalRequestedQuantity = x.Lines
                        .Where(l => !l.IsDelete)
                        .Sum(l => (int?)l.RequestedQuantity) ?? 0,
                    IsDuplicateOverridden = _dbContext.Set<BbkTransitionHistory>().Any(h =>
                        h.Scope == BbkTransitionScopes.BloodOrder &&
                        h.EntityId == x.Id &&
                        h.Action == ConfirmedDuplicateAction &&
                        !h.IsDelete),
                    Version = x.Version,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.OrderStatusLabel = LabelOf(item.OrderStatus);
                item.OrderSourceLabel = LabelOf(item.OrderSource);
            }

            return new PagedResult<BloodOrderListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<BloodOrderSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new
                {
                    x.OrderStatus,
                    IsDuplicateOverridden = _dbContext.Set<BbkTransitionHistory>().Any(h =>
                        h.Scope == BbkTransitionScopes.BloodOrder &&
                        h.EntityId == x.Id &&
                        h.Action == ConfirmedDuplicateAction &&
                        !h.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return new BloodOrderSummaryResponse
            {
                TotalOrder = rows.Count,
                ActiveOrder = rows.Count(x => x.OrderStatus == BbkBloodOrderStatus.Active),
                PartiallyFulfilledOrder = rows.Count(x => x.OrderStatus == BbkBloodOrderStatus.PartiallyFulfilled),
                FullyFulfilledOrder = rows.Count(x => x.OrderStatus == BbkBloodOrderStatus.FullyFulfilled),
                CancelledOrder = rows.Count(x => x.OrderStatus == BbkBloodOrderStatus.Cancelled),
                ExpiredOrder = rows.Count(x => x.OrderStatus == BbkBloodOrderStatus.Expired),
                DuplicateOverriddenOrder = rows.Count(x => x.IsDuplicateOverridden)
            };
        }

        public async Task<BloodOrderDetailDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await DetailQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return null;

            var detail = ToDetail(entity);

            detail.Transitions = await ReadTransitionsAsync(id, cancellationToken);
            ApplyTransitionFacts(detail);
            detail.Fulfillment = BuildFulfillment(entity);

            return detail;
        }

        /// <summary>
        /// Ringkasan pemenuhan satu order: diminta, diberikan, sisa (<c>BD-DOM-17</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Batas yang jujur pada tahap pengiriman ini.</b> Pemberian kantong belum ada di
        /// source — ia lahir pada <c>BE-BD-006</c> (alokasi) dan <c>BE-BD-007</c> (pemberian),
        /// dan catatan koreksinya pada <c>BE-BD-010</c>. Karena itu jumlah yang diberikan pada
        /// slice <c>BE-BD-003</c> <b>selalu nol</b>, dan sisanya sama dengan yang diminta.
        /// </para>
        /// <para>
        /// Yang penting: angka itu <b>dihitung</b>, bukan dibaca dari kolom. Ketika pemberian
        /// dan koreksi tiba, yang berubah hanya sumber penjumlahannya di satu tempat ini —
        /// tidak ada kolom tersimpan yang perlu diisi ulang, dan tidak ada jalan bagi angka
        /// pemenuhan untuk berbeda dari kantong yang benar-benar keluar.
        /// </para>
        /// </remarks>
        public async Task<FulfillmentSummaryDto?> GetFulfillmentAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await DetailQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return entity == null ? null : BuildFulfillment(entity);
        }

        // =================================================================
        // Perubahan
        // =================================================================

        /// <summary>Membuat order darah elektronik dari unit pelayanan yang berwenang.</summary>
        public Task<BloodOrderResult> CreateAsync(
            CreateBloodOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
            => CreateInternalAsync(
                request,
                BbkOrderSource.Electronic,
                actorUserId,
                duplicateOverrideReason: null,
                cancellationToken);

        /// <summary>Membuat order darah manual yang diinput petugas Bank Darah.</summary>
        /// <remarks>
        /// <c>VAL-BD-010</c> ditegakkan di sini dengan menuntut kelengkapan seluruh rujukan
        /// <b>dan</b> pelaku input. Pelakunya tidak pernah diterima dari isian permintaan; ia
        /// diturunkan dari pengguna terautentikasi, sehingga tidak ada order manual yang dapat
        /// mengaku diinput orang lain.
        /// </remarks>
        public Task<BloodOrderResult> CreateManualAsync(
            CreateManualBloodOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
            => CreateInternalAsync(
                request,
                BbkOrderSource.Manual,
                actorUserId,
                duplicateOverrideReason: null,
                cancellationToken);

        /// <summary>
        /// Melanjutkan order yang tertahan deteksi ganda, dengan alasan tertulis
        /// (<c>ASM-BD-001</c>).
        /// </summary>
        public Task<BloodOrderResult> ConfirmDuplicateAsync(
            ConfirmDuplicateOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = request.DuplicateOverrideReason?.Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Task.FromResult(Failed(
                    BloodOrderOutcome.Invalid,
                    "Alasan melanjutkan order ganda wajib diisi."));
            }

            return CreateInternalAsync(
                request,
                request.IsManual ? BbkOrderSource.Manual : BbkOrderSource.Electronic,
                actorUserId,
                reason,
                cancellationToken);
        }

        /// <summary>
        /// Membatalkan order dengan alasan terkendali, oleh dokter peminta atau petugas BDRS
        /// (<c>DEC-BD-044</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Kategori alasan yang menentukan sebabnya pada rekam</b>, bukan nama peran pelaku.
        /// <c>OrderCancellationClinical</c> menyatakan kebutuhan klinis berubah dan hanya sah
        /// bagi <b>dokter peminta order ini</b>; <c>OrderCancellationOperational</c> menyatakan
        /// kekeliruan operasional dan wajib dipakai pelaku selain dokter peminta — dalam praktik
        /// petugas Bank Darah. Kesesuaian dua arah itulah yang dijaga <c>VAL-BD-083</c>: alasan
        /// klinis oleh petugas BDRS ditolak, <b>begitu pula sebaliknya</b>.
        /// </para>
        /// <para>
        /// <b>Bagaimana "dokter peminta" ditentukan tanpa hardcode peran.</b> Yang dibandingkan
        /// adalah kepemilikan data: apakah pengguna terautentikasi terhubung ke baris
        /// <c>MstDoctor</c> yang tercatat sebagai <c>RequestingDoctorId</c> pada order ini.
        /// Itu pemeriksaan kepemilikan, bukan pemeriksaan nama peran — dan memang begitulah
        /// aturan hak akses menuntutnya dibedakan.
        /// </para>
        /// </remarks>
        public async Task<BloodOrderResult> CancelAsync(
            Guid id,
            CancelBloodOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodOrderOutcome.Invalid, ActorUnknownMessage);

            var reasonCode = NormalizeCode(request.ReasonCode);

            if (string.IsNullOrWhiteSpace(reasonCode))
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.");
            }

            var order = await TrackedAsync(id, cancellationToken);

            if (order == null)
                return Failed(BloodOrderOutcome.NotFound, NotFoundMessage);

            if (request.Version.HasValue && request.Version.Value != order.Version)
            {
                return Failed(BloodOrderOutcome.VersionConflict, ConcurrencyMessage);
            }

            if (!IsCancellable(order.OrderStatus))
            {
                return Failed(
                    BloodOrderOutcome.NotAllowedByState,
                    order.OrderStatus == BbkBloodOrderStatus.Expired
                        ? "Order yang sudah kedaluwarsa tidak dapat dibuka kembali. Buat order baru pada kunjungan yang berjalan."
                        : $"Order berstatus {LabelOf(order.OrderStatus)} tidak dapat dibatalkan.");
            }

            // VAL-BD-016: alasan wajib benar-benar ada pada daftar terkendali dan masih aktif.
            var reason = await _dbContext.Set<MstBloodBankReason>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete && x.IsActive && x.ReasonCode.ToUpper() == reasonCode,
                    cancellationToken);

            if (reason == null)
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas.");
            }

            var category = BloodBankReasonCategories.Normalize(reason.ReasonCategory);

            // VAL-BD-083, kedua arahnya. Dokter peminta order ini wajib memakai alasan
            // berkategori klinis; pelaku lain yang memegang butir BloodOrder : Cancel wajib
            // memakai alasan berkategori operasional. Alasan berkategori lain — misalnya
            // Emergency — tidak cocok dengan keduanya dan ikut tertolak di sini. Siapa "dokter
            // peminta" diturunkan dari kepemilikan data, bukan dari nama peran, jabatan, atau
            // UserType.
            var expectedCategory =
                await IsRequestingDoctorAsync(order.RequestingDoctorId, actorUserId, cancellationToken)
                    ? BloodBankReasonCategories.OrderCancellationClinical
                    : BloodBankReasonCategories.OrderCancellationOperational;

            if (category != expectedCategory)
                return Failed(BloodOrderOutcome.NotAllowedByState, CategoryMismatchMessage);

            var now = DateTime.UtcNow;
            var fromStatus = order.OrderStatus;

            order.OrderStatus = BbkBloodOrderStatus.Cancelled;
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;

            AppendTransition(
                order.Id,
                action: CancelAction,
                fromStatus: fromStatus,
                toStatus: BbkBloodOrderStatus.Cancelled,
                reasonCode: reason.ReasonCode,
                reasonNote: reason.ReasonText,
                actorUserId: actorUserId,
                occurredAt: now);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodOrderOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(
                order,
                category == BloodBankReasonCategories.OrderCancellationClinical
                    ? "Order darah berhasil dibatalkan dengan alasan pembatalan klinis."
                    : "Order darah berhasil dibatalkan dengan alasan pembatalan operasional.");
        }

        /// <summary>
        /// Menandai order kedaluwarsa karena kunjungan asalnya berakhir (<c>DEC-BD-014</c>).
        /// </summary>
        /// <remarks>
        /// <b>Tidak punya endpoint</b>, sesuai kontrak: kedaluwarsa dipicu sistem dari sinyal
        /// kunjungan, bukan oleh seseorang yang menekan tombol. Method ini dipanggil dari jalur
        /// yang membaca sinyal itu; pemicu terjadwalnya berada di luar cakupan
        /// <c>BE-BD-003</c>.
        /// </remarks>
        public async Task<BloodOrderResult> ExpireAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var order = await TrackedAsync(id, cancellationToken);

            if (order == null)
                return Failed(BloodOrderOutcome.NotFound, NotFoundMessage);

            if (!IsCancellable(order.OrderStatus))
            {
                return Failed(
                    BloodOrderOutcome.NotAllowedByState,
                    $"Order berstatus {LabelOf(order.OrderStatus)} tidak dapat dinyatakan kedaluwarsa.");
            }

            var closure = await _encounterStatusReader.ReadAsync(order.EncounterId, cancellationToken);

            if (!closure.IsClosed)
            {
                return Failed(
                    BloodOrderOutcome.NotAllowedByState,
                    "Kunjungan asal order ini masih berjalan, sehingga ordernya belum kedaluwarsa.");
            }

            var now = DateTime.UtcNow;
            var fromStatus = order.OrderStatus;

            order.OrderStatus = BbkBloodOrderStatus.Expired;
            order.Version++;
            order.UpdateDateTime = now;
            order.UpdateBy = actorUserId;

            // OccurredAt adalah waktu kunjungan benar-benar berakhir — untuk Rawat Inap, waktu
            // pasien pulang fisik — bukan waktu pemicu ini kebetulan berjalan. Waktu pencatatan
            // tetap tersimpan terpisah di CreateDateTime (AC-BD-017).
            AppendTransition(
                order.Id,
                action: ExpireAction,
                fromStatus: fromStatus,
                toStatus: BbkBloodOrderStatus.Expired,
                reasonCode: null,
                reasonNote: $"Kunjungan berakhir — sinyal {closure.Signal}.",
                actorUserId: actorUserId,
                occurredAt: closure.ClosedAt ?? now);

            if (!await TrySaveAsync(cancellationToken))
                return Failed(BloodOrderOutcome.VersionConflict, ConcurrencyMessage);

            return Succeeded(order, "Order darah dinyatakan kedaluwarsa karena kunjungan asalnya berakhir.");
        }

        // =================================================================
        // Pembuatan order — jalur bersama ketiga endpoint
        // =================================================================

        /// <summary>
        /// Jalur tunggal pembuatan order untuk ketiga endpoint pembuatan.
        /// </summary>
        /// <remarks>
        /// <b>Satu jalur, bukan tiga.</b> Elektronik, manual, dan lanjutan-ganda berbeda hanya
        /// pada penanda sumber dan ada tidaknya alasan pembenaran. Menuliskannya tiga kali akan
        /// membuat ketiganya berpeluang menyimpang satu sama lain — dan yang paling mudah
        /// tertinggal justru pemeriksaan kewenangan unit dan deteksi ganda.
        ///
        /// <b>Urutan pemeriksaannya disengaja:</b> seluruh penolakan diselesaikan
        /// <b>sebelum</b> nomor order diminta, supaya deret tidak berlubang oleh permintaan
        /// yang memang tidak sah (<c>INV-PLT-002</c>).
        /// </remarks>
        private async Task<BloodOrderResult> CreateInternalAsync(
            CreateBloodOrderRequest request,
            BbkOrderSource orderSource,
            Guid actorUserId,
            string? duplicateOverrideReason,
            CancellationToken cancellationToken)
        {
            // VAL-BD-011: setiap order wajib menyimpan siapa yang membuatnya. Ditegakkan
            // dengan tidak pernah menerima pelaku dari isian permintaan.
            if (actorUserId == Guid.Empty)
                return Failed(BloodOrderOutcome.Invalid, ActorUnknownMessage);

            // VAL-BD-010: kelengkapan rujukan. Berlaku bagi kedua sumber. [Required] pada Guid
            // tidak pernah menolak Guid.Empty, sehingga penjaga sesungguhnya ada di sini. Pesan
            // VAL-BD-010 dipakai persis untuk order manual; order elektronik mendapat pesan yang
            // tidak menyebut "order manual" supaya tidak menyesatkan petugas unit.
            if (request.PatientId == Guid.Empty ||
                request.EncounterId == Guid.Empty ||
                request.ServiceUnitId == Guid.Empty ||
                request.RequestingDoctorId == Guid.Empty)
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    orderSource == BbkOrderSource.Manual
                        ? ManualIncompleteMessage
                        : "Order darah wajib mengisi pasien, kunjungan, dokter peminta, dan unit pelayanan pemesan.");
            }

            if (request.Lines == null || request.Lines.Count == 0)
                return Failed(BloodOrderOutcome.Invalid, "Order darah wajib memuat minimal satu baris kebutuhan.");

            // VAL-BD-002: jumlah diminta wajib lebih dari nol.
            if (request.Lines.Any(x => x.RequestedQuantity <= 0))
                return Failed(BloodOrderOutcome.Invalid, "Jumlah kantong yang diminta harus lebih dari nol.");

            var componentIds = request.Lines.Select(x => x.BloodComponentId).ToList();

            if (componentIds.Any(x => x == Guid.Empty))
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete, cancellationToken);

            if (!patientExists)
                return Failed(BloodOrderOutcome.Invalid, "Pasien tidak ditemukan atau sudah dihapus.");

            var encounter = await _dbContext.Set<Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == request.EncounterId && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
                return Failed(BloodOrderOutcome.Invalid, "Kunjungan tidak ditemukan atau sudah dihapus.");

            // Kunjungan wajib milik pasien yang sama. Tanpa pemeriksaan ini, order dapat lahir
            // menggantung pada kunjungan pasien lain dan seluruh penilaian kedaluwarsa ikut
            // salah sasaran.
            if (encounter.PatientId != request.PatientId)
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Kunjungan yang dipilih bukan milik pasien ini.");
            }

            // Kunjungan yang sudah berakhir tidak dapat menerima order baru — order yang lahir
            // di situ akan langsung kedaluwarsa (DEC-BD-014).
            if (await _encounterStatusReader.IsEncounterClosedAsync(request.EncounterId, cancellationToken))
            {
                return Failed(
                    BloodOrderOutcome.NotAllowedByState,
                    "Kunjungan ini sudah berakhir, sehingga tidak dapat menerima order darah baru. " +
                    "Buat order pada kunjungan yang berjalan.");
            }

            var serviceUnit = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .Where(x => x.Id == request.ServiceUnitId && !x.IsDelete)
                .Select(x => new { x.Id, x.IsAvailableForBloodOrder })
                .FirstOrDefaultAsync(cancellationToken);

            if (serviceUnit == null)
                return Failed(BloodOrderOutcome.Invalid, "Unit pelayanan tidak ditemukan atau sudah dihapus.");

            // VAL-BD-013 — 403. Kewenangan unit memesan darah adalah data induk yang disetel
            // pemilik Master Data (BD-DOM-18), bukan nama peran yang dikunci di kode.
            if (!serviceUnit.IsAvailableForBloodOrder)
            {
                return Failed(
                    BloodOrderOutcome.UnitNotAuthorized,
                    "Unit pelayanan ini belum diberi kewenangan memesan darah.");
            }

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.RequestingDoctorId && !x.IsDelete, cancellationToken);

            if (!doctorExists)
                return Failed(BloodOrderOutcome.Invalid, "Dokter peminta tidak ditemukan atau sudah dihapus.");

            // VAL-BD-003: seluruh komponen wajib ada di katalog dan masih aktif.
            var distinctComponentIds = componentIds.Distinct().ToList();

            var components = await _dbContext.Set<MstBloodComponent>()
                .AsNoTracking()
                .Where(x => distinctComponentIds.Contains(x.Id) && !x.IsDelete && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (components.Count != distinctComponentIds.Count)
            {
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas.");
            }

            // Satu transaksi dari deteksi ganda sampai order tersimpan, dijaga kunci penasihat
            // per pasien + kunjungan. Tanpa kunci ini dua petugas yang menyimpan order PRC yang
            // sama pada saat hampir bersamaan sama-sama lolos deteksi ganda, dan penahanan
            // BD-XINV-01 bocor tanpa alasan tertulis siapa pun.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquireOrderCreationLockAsync(request.PatientId, request.EncounterId, cancellationToken);

            // BD-XINV-01 — deteksi order ganda. Dilewati hanya ketika pemanggil menyertakan
            // alasan tertulis lewat endpoint confirm-duplicate (ASM-BD-001).
            if (duplicateOverrideReason == null)
            {
                var duplicateComponentIds = await FindActiveDuplicateComponentIdsAsync(
                    request.PatientId,
                    request.EncounterId,
                    componentIds,
                    cancellationToken);

                if (duplicateComponentIds.Count > 0)
                {
                    return new BloodOrderResult(
                        BloodOrderOutcome.DuplicateOrder,
                        null,
                        "Sudah ada order darah aktif untuk pasien dan komponen ini pada kunjungan yang sama. " +
                        "Lanjutkan hanya dengan alasan tertulis.",
                        duplicateComponentIds);
                }
            }

            var now = DateTime.UtcNow;

            // Nomor diminta paling akhir: seluruh penolakan sudah selesai di atas, sehingga
            // deret tidak berlubang oleh permintaan yang memang tidak sah (INV-PLT-002).
            string orderNumber;

            try
            {
                orderNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: OrderSequenceKey,
                        Prefix: OrderNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: OrderSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                // Pesan asli alokator tidak diteruskan apa adanya: ia menyebut deret, digit,
                // dan kebijakan yang tidak berarti bagi petugas.
                return Failed(
                    BloodOrderOutcome.Invalid,
                    "Nomor order darah gagal diterbitkan. Hubungi administrator sistem.");
            }

            var order = new BbkBloodOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                PatientId = request.PatientId,
                EncounterId = request.EncounterId,
                ServiceUnitId = request.ServiceUnitId,
                RequestingDoctorId = request.RequestingDoctorId,
                OrderSource = orderSource,
                InputByUserId = actorUserId,
                OrderStatus = BbkBloodOrderStatus.Active,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            var sequence = 1;

            foreach (var line in request.Lines)
            {
                order.Lines.Add(new BbkBloodOrderLine
                {
                    Id = Guid.NewGuid(),
                    BloodOrderId = order.Id,
                    BloodComponentId = line.BloodComponentId,
                    RequestedQuantity = line.RequestedQuantity,
                    Sequence = sequence++,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            _dbContext.Set<BbkBloodOrder>().Add(order);

            AppendTransition(
                order.Id,
                action: duplicateOverrideReason != null ? ConfirmedDuplicateAction : CreateAction,
                fromStatus: null,
                toStatus: BbkBloodOrderStatus.Active,
                reasonCode: null,
                reasonNote: duplicateOverrideReason,
                actorUserId: actorUserId,
                occurredAt: now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Succeeded(
                order,
                duplicateOverrideReason != null
                    ? "Order darah berhasil dibuat dengan alasan tertulis atas order ganda."
                    : orderSource == BbkOrderSource.Manual
                        ? "Order darah manual berhasil dibuat."
                        : "Order darah berhasil dibuat.");
        }

        /// <summary>
        /// <c>BD-XINV-01</c> — komponen mana pada permintaan ini yang sudah punya order
        /// <b>aktif</b> bagi pasien dan kunjungan yang sama (<c>DEC-BD-005</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kuncinya empat sekaligus — pasien, kunjungan, komponen, dan status aktif — dan
        /// masing-masing dibuktikan acceptance criteria tersendiri:
        /// </para>
        /// <list type="number">
        /// <item>
        /// Per <b>komponen</b>. Order PRC yang aktif tidak menahan order trombosit
        /// (<c>AC-BD-002</c>).
        /// </item>
        /// <item>
        /// Per <b>kunjungan</b>. Kunjungan berbeda tidak saling menahan (<c>AC-BD-003</c>).
        /// </item>
        /// <item>
        /// Hanya status <b>aktif</b>. Order yang dibatalkan, terpenuhi penuh, atau kedaluwarsa
        /// tidak menahan apa pun (<c>DEC-BD-006</c>, <c>AC-BD-004</c>).
        /// </item>
        /// </list>
        /// <para>
        /// <b>Kenapa kunjungan yang sudah berakhir tidak diperiksa lagi di sini.</b> Pembuatan
        /// order pada kunjungan yang sudah berakhir sudah ditolak lebih dulu di
        /// <see cref="CreateInternalAsync"/>, dan pembanding di sini selalu kunjungan yang sama
        /// dengan permintaannya. Memeriksanya lagi hanya menghasilkan kode yang tidak pernah
        /// dapat dilalui.
        /// </para>
        /// </remarks>
        private Task<List<Guid>> FindActiveDuplicateComponentIdsAsync(
            Guid patientId,
            Guid encounterId,
            List<Guid> componentIds,
            CancellationToken cancellationToken)
            => _dbContext.Set<BbkBloodOrderLine>()
                .AsNoTracking()
                .Where(l =>
                    !l.IsDelete &&
                    componentIds.Contains(l.BloodComponentId) &&
                    l.BloodOrder != null &&
                    !l.BloodOrder.IsDelete &&
                    !l.BloodOrder.IsCancel &&
                    l.BloodOrder.PatientId == patientId &&
                    l.BloodOrder.EncounterId == encounterId &&
                    (l.BloodOrder.OrderStatus == BbkBloodOrderStatus.Active ||
                     l.BloodOrder.OrderStatus == BbkBloodOrderStatus.PartiallyFulfilled))
                .Select(l => l.BloodComponentId)
                .Distinct()
                .ToListAsync(cancellationToken);

        // =================================================================
        // Riwayat perpindahan status
        // =================================================================

        /// <summary>
        /// Menambahkan satu baris riwayat. Append-only: tidak pernah menyunting baris lama.
        /// </summary>
        private void AppendTransition(
            Guid orderId,
            string action,
            BbkBloodOrderStatus? fromStatus,
            BbkBloodOrderStatus toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime occurredAt)
        {
            _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
            {
                Id = Guid.NewGuid(),
                Scope = BbkTransitionScopes.BloodOrder,
                EntityId = orderId,
                Action = action,
                FromStatus = fromStatus?.ToString(),
                ToStatus = toStatus.ToString(),
                ReasonCode = reasonCode,
                ReasonNote = Truncate(reasonNote, 500),
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        /// <summary>
        /// Riwayat perpindahan status satu order, terlama lebih dulu. Memulangkan <c>null</c>
        /// bila ordernya tidak ada, supaya controller dapat membedakan "belum ada riwayat" dari
        /// "tidak ada order".
        /// </summary>
        public async Task<List<BloodOrderTransitionDto>?> GetStatusHistoryAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var exists = await BaseQuery().AnyAsync(x => x.Id == id, cancellationToken);

            return exists ? await ReadTransitionsAsync(id, cancellationToken) : null;
        }

        private async Task<List<BloodOrderTransitionDto>> ReadTransitionsAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
            => await _dbContext.Set<BbkTransitionHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.Scope == BbkTransitionScopes.BloodOrder &&
                    x.EntityId == orderId &&
                    !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.OccurredAt)
                .Select(x => new BloodOrderTransitionDto
                {
                    Id = x.Id,
                    Action = x.Action,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ReasonCode = x.ReasonCode,
                    ReasonNote = x.ReasonNote,
                    ActorUserId = x.ActorUserId,
                    OccurredAt = x.OccurredAt
                })
                .ToListAsync(cancellationToken);

        // =================================================================
        // Pemetaan dan metadata
        // =================================================================

        public static BloodOrderDetailDto ToDetail(BbkBloodOrder entity) => new()
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            PatientId = entity.PatientId,
            PatientName = entity.Patient?.FullName,
            MedicalRecordNumber = entity.Patient?.MedicalRecordNumber,
            EncounterId = entity.EncounterId,
            EncounterNumber = entity.Encounter?.EncounterNumber,
            ServiceUnitId = entity.ServiceUnitId,
            ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
            RequestingDoctorId = entity.RequestingDoctorId,
            RequestingDoctorName = entity.RequestingDoctor?.FullName,
            OrderSource = entity.OrderSource,
            OrderSourceLabel = LabelOf(entity.OrderSource),
            InputByUserId = entity.InputByUserId,
            OrderStatus = entity.OrderStatus,
            OrderStatusLabel = LabelOf(entity.OrderStatus),
            Version = entity.Version,
            Lines = entity.Lines
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .Select(x => new BloodOrderLineDto
                {
                    Id = x.Id,
                    BloodComponentId = x.BloodComponentId,
                    BloodComponentCode = x.BloodComponent?.ComponentCode,
                    BloodComponentName = x.BloodComponent?.ComponentName,
                    RequestedQuantity = x.RequestedQuantity,
                    Sequence = x.Sequence
                })
                .ToList(),
            AvailableActions = AvailableActionsOf(entity),
            CreateDateTime = entity.CreateDateTime,
            UpdateDateTime = entity.UpdateDateTime
        };

        /// <summary>
        /// Mengisi keterangan kejadian pada detail dari riwayat, bukan dari kolom order.
        /// </summary>
        /// <remarks>
        /// Alasan tertulis order ganda (<c>ASM-BD-001</c>) dan waktu kedaluwarsa hidup di
        /// <c>BbkTransitionHistory</c>. Detail menampilkannya supaya layar tidak perlu menelusuri
        /// riwayat sendiri, tetapi sumbernya tetap satu.
        /// </remarks>
        private static void ApplyTransitionFacts(BloodOrderDetailDto detail)
        {
            var confirmed = detail.Transitions.FirstOrDefault(x => x.Action == ConfirmedDuplicateAction);

            detail.DuplicateOverrideReason = confirmed?.ReasonNote;
            detail.DuplicateOverrideByUserId = confirmed?.ActorUserId;
            detail.DuplicateOverrideAt = confirmed?.OccurredAt;
            detail.ExpiredAt = detail.Transitions.LastOrDefault(x => x.Action == ExpireAction)?.OccurredAt;
        }

        /// <summary>
        /// Menyusun ringkasan pemenuhan dari baris order. Lihat catatan batas pada
        /// <see cref="GetFulfillmentAsync"/>.
        /// </summary>
        private static FulfillmentSummaryDto BuildFulfillment(BbkBloodOrder entity)
        {
            var lines = entity.Lines
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .Select(x => new FulfillmentSummaryLineDto
                {
                    BloodOrderLineId = x.Id,
                    BloodComponentId = x.BloodComponentId,
                    BloodComponentCode = x.BloodComponent?.ComponentCode,
                    BloodComponentName = x.BloodComponent?.ComponentName,
                    RequestedQuantity = x.RequestedQuantity,

                    // Pemberian kantong lahir pada BE-BD-006/BE-BD-007; koreksinya pada
                    // BE-BD-010. Sampai keduanya ada, jumlah yang diberikan memang nol.
                    IssuedQuantity = 0,
                    OutstandingQuantity = x.RequestedQuantity
                })
                .ToList();

            return new FulfillmentSummaryDto
            {
                BloodOrderId = entity.Id,
                OrderNumber = entity.OrderNumber,
                OrderStatus = entity.OrderStatus,
                OrderStatusLabel = LabelOf(entity.OrderStatus),
                TotalRequestedQuantity = lines.Sum(x => x.RequestedQuantity),
                TotalIssuedQuantity = lines.Sum(x => x.IssuedQuantity),
                TotalOutstandingQuantity = lines.Sum(x => x.OutstandingQuantity),
                Lines = lines,
                Message =
                    "Jumlah diberikan dihitung dari pemberian kantong yang nyata. " +
                    "Pencatatan pemberian kantong belum tersedia pada tahap ini, sehingga " +
                    "seluruh kebutuhan masih tercatat belum terpenuhi."
            };
        }

        /// <summary>
        /// Aksi yang layak dicoba pada keadaan sekarang. <b>Kelayakan status saja</b> — bukan
        /// kewenangan, yang tetap dijaga butir hak akses pada endpoint masing-masing.
        /// </summary>
        private static List<string> AvailableActionsOf(BbkBloodOrder entity)
        {
            var actions = new List<string>();

            if (IsCancellable(entity.OrderStatus))
                actions.Add("Cancel");

            return actions;
        }

        public static BloodOrderFilterMetadataResponse BuildFilterMetadata() => new()
        {
            DefaultFilter = new BloodOrderDefaultFilterResponse(),
            OrderStatusOptions = Enum.GetValues<BbkBloodOrderStatus>()
                .Select(x => new BloodOrderOptionItemResponse
                {
                    Value = (int)x,
                    Label = LabelOf(x)
                })
                .ToList(),
            OrderSourceOptions = Enum.GetValues<BbkOrderSource>()
                .Select(x => new BloodOrderOptionItemResponse
                {
                    Value = (int)x,
                    Label = LabelOf(x)
                })
                .ToList(),
            SortOptions = new List<BloodOrderSortOptionResponse>
            {
                new() { Value = "createDateTime", Label = "Waktu dibuat" },
                new() { Value = "orderNumber", Label = "Nomor order" },
                new() { Value = "orderStatus", Label = "Status order" }
            },
            SortDirections = new List<string> { "asc", "desc" },
            PageSizeOptions = new List<int> { 10, 25, 50, 100 }
        };

        private static string LabelOf(BbkBloodOrderStatus status) => status switch
        {
            BbkBloodOrderStatus.Active => "Aktif",
            BbkBloodOrderStatus.PartiallyFulfilled => "Terpenuhi sebagian",
            BbkBloodOrderStatus.FullyFulfilled => "Terpenuhi penuh",
            BbkBloodOrderStatus.Cancelled => "Dibatalkan",
            BbkBloodOrderStatus.Expired => "Kedaluwarsa",
            _ => status.ToString()
        };

        private static string LabelOf(BbkOrderSource source) => source switch
        {
            BbkOrderSource.Electronic => "Elektronik",
            BbkOrderSource.Manual => "Manual",
            _ => source.ToString()
        };

        // =================================================================
        // Penolong
        // =================================================================

        /// <summary>Ketiga status terminal tidak dapat dibatalkan maupun dikedaluwarsakan.</summary>
        private static bool IsCancellable(BbkBloodOrderStatus status)
            => status is BbkBloodOrderStatus.Active or BbkBloodOrderStatus.PartiallyFulfilled;

        /// <summary>
        /// Apakah pengguna terautentikasi adalah dokter peminta order ini.
        /// </summary>
        /// <remarks>
        /// <b>Pemeriksaan kepemilikan data, bukan pemeriksaan peran.</b> Yang dibandingkan
        /// adalah tautan akun pengguna ke satu baris <c>MstDoctor</c> lewat
        /// <c>ApplicationUser.DoctorId</c> — yaitu dokter yang tercatat pada ordernya sendiri.
        /// Tidak ada nama peran, nama jabatan, atau <c>UserType</c> yang dibaca di sini.
        /// </remarks>
        private async Task<bool> IsRequestingDoctorAsync(
            Guid requestingDoctorId,
            Guid actorUserId,
            CancellationToken cancellationToken)
            => await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == actorUserId && x.DoctorId == requestingDoctorId,
                    cancellationToken);

        /// <summary>
        /// Mengantrekan pembuatan order pada pasien dan kunjungan yang sama.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Polanya sama dengan <c>NumberSeriesAllocator</c>: <c>pg_advisory_xact_lock</c> hidup
        /// selama transaksi pembuatan dan lepas sendiri ketika transaksi selesai. Kunci ini
        /// <b>mengantre</b>, bukan menolak — permintaan kedua menunggu sampai yang pertama
        /// tersimpan, lalu deteksi gandanya melihat order pertama itu.
        /// </para>
        /// <para>
        /// <b>Kenapa bukan index unik.</b> <c>DEC-BD-005</c> sengaja mengizinkan order ganda
        /// yang dilanjutkan dengan alasan tertulis, sehingga pasangan pasien + kunjungan +
        /// komponen memang boleh muncul dua kali. Index unik akan melarang yang justru diizinkan
        /// keputusan itu.
        /// </para>
        /// <para>
        /// <b>Batas yang jujur.</b> Di luar PostgreSQL — yang di repository ini berarti
        /// pengujian InMemory — langkah ini dilewati, sehingga perilaku berebut hanya dapat
        /// dibuktikan terhadap PostgreSQL sungguhan.
        /// </para>
        /// </remarks>
        private async Task AcquireOrderCreationLockAsync(
            Guid patientId,
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            var lockKey = $"BBK_BLOOD_ORDER_{patientId:N}_{encounterId:N}";

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        /// <summary>
        /// Menyimpan perubahan, lalu menerjemahkan bentrok token <c>Version</c> menjadi hasil
        /// <c>409</c> alih-alih galat server.
        /// </summary>
        /// <remarks>
        /// <c>Version</c> dipetakan sebagai concurrency token, sehingga <c>UPDATE</c> hanya
        /// berhasil bila baris di database masih bernilai versi yang dibaca. Dua petugas yang
        /// membatalkan order yang sama bersamaan karena itu tidak menghasilkan dua baris
        /// riwayat pembatalan: yang kedua ditolak.
        /// </remarks>
        private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Buang perubahan yang gagal — termasuk baris riwayat yang sudah ditambahkan —
                // supaya tidak ikut tersimpan oleh SaveChanges lain pada scope yang sama.
                _dbContext.ChangeTracker.Clear();

                return false;
            }
        }

        private IQueryable<BbkBloodOrder> BaseQuery()
            => _dbContext.Set<BbkBloodOrder>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private IQueryable<BbkBloodOrder> DetailQuery()
            => BaseQuery()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.ServiceUnit)
                .Include(x => x.RequestingDoctor)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.BloodComponent);

        private Task<BbkBloodOrder?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<BbkBloodOrder>()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static IQueryable<BbkBloodOrder> ApplySort(
            IQueryable<BbkBloodOrder> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "ordernumber" => descending
                    ? query.OrderByDescending(x => x.OrderNumber)
                    : query.OrderBy(x => x.OrderNumber),
                "orderstatus" => descending
                    ? query.OrderByDescending(x => x.OrderStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.OrderStatus).ThenBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static string NormalizeCode(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        private static string? Truncate(string? value, int maxLength)
            => value != null && value.Length > maxLength ? value[..maxLength] : value;

        private static BloodOrderResult Succeeded(BbkBloodOrder entity, string message)
            => new(BloodOrderOutcome.Success, entity, message, new List<Guid>());

        private static BloodOrderResult Failed(BloodOrderOutcome outcome, string message)
            => new(outcome, null, message, new List<Guid>());
    }

    /// <summary>
    /// Hasil satu tindakan pada order darah. Dipetakan ke HTTP status oleh controller, bukan
    /// oleh service.
    /// </summary>
    /// <param name="Outcome">Jenis hasilnya.</param>
    /// <param name="Entity">Order yang terpengaruh, bila tindakannya berhasil.</param>
    /// <param name="Message">Pesan bagi pengguna, dalam Bahasa Indonesia.</param>
    /// <param name="DuplicateComponentIds">
    /// Komponen yang memicu penahanan order ganda. Terisi hanya pada
    /// <see cref="BloodOrderOutcome.DuplicateOrder"/>, supaya layar dapat menyebut komponen
    /// mana yang bentrok tanpa menebak.
    /// </param>
    public sealed record BloodOrderResult(
        BloodOrderOutcome Outcome,
        BbkBloodOrder? Entity,
        string Message,
        IReadOnlyList<Guid> DuplicateComponentIds);

    /// <summary>Jenis hasil satu tindakan pada order darah.</summary>
    public enum BloodOrderOutcome
    {
        Success = 0,

        /// <summary>Order tidak ada atau sudah dihapus.</summary>
        NotFound = 1,

        /// <summary>Isian permintaan tidak sah.</summary>
        Invalid = 2,

        /// <summary>
        /// Sudah ada order aktif untuk pasien, kunjungan, dan komponen yang sama
        /// (<c>VAL-BD-001</c>).
        /// </summary>
        DuplicateOrder = 3,

        /// <summary>
        /// Unit pelayanan belum diberi kewenangan memesan darah (<c>VAL-BD-013</c>).
        /// </summary>
        UnitNotAuthorized = 4,

        /// <summary>Keadaan datanya tidak memenuhi syarat tindakan ini.</summary>
        NotAllowedByState = 5,

        /// <summary>Order sudah berubah di tangan orang lain.</summary>
        VersionConflict = 6
    }
}
