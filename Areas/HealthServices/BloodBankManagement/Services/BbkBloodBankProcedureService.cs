using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services
{
    /// <summary>
    /// Pemilik pencatatan dan penyelesaian tindakan Bank Darah (<c>BD-AGG-05</c>). Controller tidak
    /// menyentuh <c>ApplicationDbContext</c> sendiri (<c>QBE-SVC-001</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>1. Konteks tarif dari kunjungan, bukan dari client</b> (<c>DEC-BD-048</c>). Unit dan kelas
    /// pasien dibaca dari kunjungan yang ditunjuk order, lalu disimpan pada tindakan.
    /// </para>
    ///
    /// <para>
    /// <b>2. Tarif dipilih backend</b> (<c>DEC-BD-049</c>, <c>VAL-BD-027</c>). Lihat
    /// <see cref="ResolveTariffAsync"/>: kandidat memakai predikat kecocokan yang sama dengan
    /// <c>InsuranceCoverageService.FindProcedureTariffAsync</c>, dan tarif kelas pasien diutamakan
    /// di atas tarif umum.
    /// </para>
    ///
    /// <para>
    /// <b>3. Salinan dibekukan</b> (<c>AC-BD-100</c>). Kode, nama, dan nominal disalin saat dicatat
    /// dan tidak pernah dibaca ulang dari data induk.
    /// </para>
    ///
    /// <para>
    /// <b>4. Satu fakta biaya per tindakan selesai</b> (<c>DEC-BD-016</c>, <c>BE-BD-013</c>). Sesudah
    /// <c>Recorded</c> → <c>Completed</c> tersimpan, fakta <c>BloodBank</c>/<c>BloodBankCharge</c>
    /// diserahkan lewat <c>ClinicalMilestoneFactProducer</c> — satu-satunya jalur resmi ke Billing.
    /// Tidak ada kolom penagihan pada tindakan; status penyerahan tinggal di ledger
    /// <c>CliClinicalMilestoneFact</c>. Riwayat: sampai <c>BE-BD-012</c> berkas ini sengaja tanpa
    /// dependensi Billing (<c>AC-BD-102</c>), batas yang gugur ketika <c>DEC-BD-016</c> disetujui.
    /// </para>
    /// </remarks>
    public class BbkBloodBankProcedureService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        /// <summary>
        /// Satuan fakta biaya. Menyebut <b>tindakan</b>, bukan kantong: <c>Quantity</c> selalu 1 per
        /// tindakan, berapa pun kantong yang diberikan (<c>DEC-BD-021</c>, <c>AC-BD-026</c>).
        /// </summary>
        private const string CostFactUnit = "Tindakan";

        /// <summary>Kode hasil penyerahan milik Bank Darah, bila fakta tidak sampai ke producer.</summary>
        private const string CostFactSourceIncompleteCode = "BBK_COST_FACT_SOURCE_INCOMPLETE";
        private const string CostFactUnconfirmedCode = "BBK_COST_FACT_HANDOFF_UNCONFIRMED";

        private const string LogCategory = "HealthServices.BloodBankManagement.BloodBankProcedure";

        /// <summary>Penanda deret nomor tindakan Bank Darah pada provider bersama.</summary>
        /// <remarks>
        /// Awalan, digit, dan kebijakan pengulangan milik Bank Darah (<c>QBE-CODE-005</c>); mesin
        /// alokasinya milik Platform. Pola yang sama dengan <c>ORD</c> dan <c>PMI</c>.
        /// </remarks>
        private const string ProcedureSequenceKey = "BBK_PROCEDURE";
        private const string ProcedureNumberPrefix = "TND";
        private const int ProcedureSequenceDigits = 8;

        /// <summary>Nama tindakan pada <c>BbkTransitionHistory</c>.</summary>
        private const string CreateAction = "Create";
        private const string CompleteAction = "Complete";

        private const string NotFoundMessage = "Tindakan Bank Darah tidak ditemukan atau sudah dihapus.";

        private const string ActorUnknownMessage =
            "Petugas pelaku tidak dikenali. Masuk kembali lalu ulangi tindakan ini.";

        private const string ConcurrencyMessage =
            "Tindakan ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini.";

        /// <summary>Pesan <c>VAL-BD-026</c>, persis seperti matriks validasi.</summary>
        private const string InvalidOrderMessage = "Tindakan Bank Darah wajib menunjuk satu order yang sah.";

        /// <summary>
        /// Pesan <c>VAL-BD-084</c> — tambahan kontrak <c>BE-BD-012</c>, turunan <c>DEC-BD-049</c>.
        /// </summary>
        private const string TariffUnavailableMessage =
            "Tarif tindakan ini belum diatur untuk kelas pasien dan unit kunjungan ini. " +
            "Hubungi bagian data induk tarif.";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly LoggerService _loggerService;

        public BbkBloodBankProcedureService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _loggerService = loggerService;
        }

        // =================================================================
        // Pembacaan
        // =================================================================

        public async Task<PagedResult<BloodBankProcedureListDto>> GetPagedAsync(
            string? search,
            Guid? bloodOrderId,
            Guid? patientId,
            BbkProcedureStatus? procedureStatus,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (bloodOrderId.HasValue)
                query = query.Where(x => x.BloodOrderId == bloodOrderId.Value);

            if (patientId.HasValue)
                query = query.Where(x => x.BloodOrder != null && x.BloodOrder.PatientId == patientId.Value);

            if (procedureStatus.HasValue)
                query = query.Where(x => x.ProcedureStatus == procedureStatus.Value);

            // Pencarian menyasar nomor tindakan, salinan kode/nama tindakan, nomor order, dan pasien.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProcedureNumber.ToLower().Contains(keyword) ||
                    x.ProcedureCodeSnapshot.ToLower().Contains(keyword) ||
                    x.ProcedureNameSnapshot.ToLower().Contains(keyword) ||
                    (x.BloodOrder != null && x.BloodOrder.OrderNumber.ToLower().Contains(keyword)) ||
                    (x.BloodOrder != null && x.BloodOrder.Patient != null &&
                     x.BloodOrder.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.BloodOrder != null && x.BloodOrder.Patient != null &&
                     x.BloodOrder.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodBankProcedureListDto
                {
                    Id = x.Id,
                    ProcedureNumber = x.ProcedureNumber,
                    BloodOrderId = x.BloodOrderId,
                    OrderNumber = x.BloodOrder != null ? x.BloodOrder.OrderNumber : null,
                    PatientId = x.BloodOrder != null ? x.BloodOrder.PatientId : null,
                    PatientName = x.BloodOrder != null && x.BloodOrder.Patient != null
                        ? x.BloodOrder.Patient.FullName
                        : null,
                    MedicalRecordNumber = x.BloodOrder != null && x.BloodOrder.Patient != null
                        ? x.BloodOrder.Patient.MedicalRecordNumber
                        : null,
                    ProcedureRefId = x.ProcedureRefId,
                    ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                    ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                    TariffAmountSnapshot = x.TariffAmountSnapshot,
                    BdrsDoctorId = x.BdrsDoctorId,
                    BdrsDoctorName = x.BdrsDoctor != null ? x.BdrsDoctor.FullName : null,
                    ProcedureStatus = x.ProcedureStatus,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
                item.ProcedureStatusLabel = BbkDisplayLabels.Of(item.ProcedureStatus);

            return new PagedResult<BloodBankProcedureListDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<BloodBankProcedureDetailDto?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery()
                .Include(x => x.BloodOrder)
                    .ThenInclude(x => x!.Patient)
                .Include(x => x.BloodOrder)
                    .ThenInclude(x => x!.Encounter)
                .Include(x => x.ServiceUnit)
                .Include(x => x.PatientClass)
                .Include(x => x.BdrsDoctor)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return null;

            var transitions = await ReadTransitionsAsync(entity.Id, cancellationToken);
            var completion = transitions.LastOrDefault(x => x.Action == CompleteAction);

            return new BloodBankProcedureDetailDto
            {
                Id = entity.Id,
                ProcedureNumber = entity.ProcedureNumber,
                BloodOrderId = entity.BloodOrderId,
                OrderNumber = entity.BloodOrder?.OrderNumber,
                EncounterId = entity.BloodOrder?.EncounterId,
                EncounterNumber = entity.BloodOrder?.Encounter?.EncounterNumber,
                PatientId = entity.BloodOrder?.PatientId,
                PatientName = entity.BloodOrder?.Patient?.FullName,
                MedicalRecordNumber = entity.BloodOrder?.Patient?.MedicalRecordNumber,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
                BdrsDoctorId = entity.BdrsDoctorId,
                BdrsDoctorName = entity.BdrsDoctor?.FullName,
                PerformedByUserId = entity.PerformedByUserId,
                ProcedureRefId = entity.ProcedureRefId,
                TariffId = entity.TariffId,
                ProcedureCodeSnapshot = entity.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = entity.ProcedureNameSnapshot,
                TariffAmountSnapshot = entity.TariffAmountSnapshot,
                ProcedureStatus = entity.ProcedureStatus,
                ProcedureStatusLabel = BbkDisplayLabels.Of(entity.ProcedureStatus),
                CompletedAt = completion?.OccurredAt,
                CompletedByUserId = completion?.ActorUserId,
                Transitions = transitions,
                AvailableActions = entity.ProcedureStatus == BbkProcedureStatus.Recorded
                    ? new List<string> { "Complete" }
                    : new List<string>(),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy
            };
        }

        // =================================================================
        // Perubahan
        // =================================================================

        /// <summary>
        /// Mencatat satu tindakan atas satu order darah beserta salinan tarifnya (<c>AC-BD-098</c>,
        /// <c>AC-BD-099</c>).
        /// </summary>
        /// <remarks>
        /// <b>Urutan pemeriksaannya disengaja:</b> seluruh penolakan — termasuk tarif yang tidak
        /// tersedia — diselesaikan <b>sebelum</b> nomor diminta, supaya deret tidak berlubang oleh
        /// permintaan yang memang tidak sah (<c>INV-PLT-002</c>).
        /// </remarks>
        public async Task<BloodBankProcedureResult> CreateAsync(
            CreateBloodBankProcedureRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, ActorUnknownMessage);

            if (request.BloodOrderId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, InvalidOrderMessage);

            if (request.ProcedureRefId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, "Tindakan bertarif wajib dipilih dari data induk.");

            if (request.BdrsDoctorId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, "Dokter BDRS penanggung jawab wajib dipilih.");

            // VAL-BD-026: order wajib ada dan tidak dihapus.
            var order = await _dbContext.Set<BbkBloodOrder>()
                .AsNoTracking()
                .Where(x => x.Id == request.BloodOrderId && !x.IsDelete && !x.IsCancel)
                .Select(x => new { x.Id, x.EncounterId })
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return Failed(BloodBankProcedureOutcome.Invalid, InvalidOrderMessage);

            // DEC-BD-048: unit dan kelas pasien dibaca dari kunjungan order, tidak dari client.
            var encounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => x.Id == order.EncounterId && !x.IsDelete)
                .Select(x => new { x.ServiceUnitId, x.ClinicId, x.PatientClassId })
                .FirstOrDefaultAsync(cancellationToken);

            if (encounter == null)
            {
                return Failed(
                    BloodBankProcedureOutcome.NotAllowedByState,
                    "Kunjungan asal order ini tidak ditemukan atau sudah dihapus.");
            }

            // Kamus data mewajibkan PatientClassId. Kunjungan tanpa kelas tidak dapat menjadi
            // sumber kelas pasien, dan tidak ada nilai yang boleh ditebak untuknya.
            if (!encounter.PatientClassId.HasValue)
            {
                return Failed(
                    BloodBankProcedureOutcome.NotAllowedByState,
                    "Kelas pasien belum tercatat pada kunjungan order ini, sehingga tarif tindakan " +
                    "tidak dapat ditentukan. Lengkapi kelas pasien pada pendaftaran.");
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => x.Id == request.ProcedureRefId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.Id, x.ProcedureCode, x.ProcedureName })
                .FirstOrDefaultAsync(cancellationToken);

            if (procedure == null)
            {
                return Failed(
                    BloodBankProcedureOutcome.Invalid,
                    "Tindakan bertarif tidak ditemukan atau sudah tidak aktif.");
            }

            var doctorExists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.BdrsDoctorId && !x.IsDelete, cancellationToken);

            if (!doctorExists)
                return Failed(BloodBankProcedureOutcome.Invalid, "Dokter BDRS tidak ditemukan atau sudah dihapus.");

            var now = DateTime.UtcNow;

            var tariff = await ResolveTariffAsync(
                procedure.Id,
                encounter.ServiceUnitId,
                encounter.ClinicId,
                encounter.PatientClassId.Value,
                now.Date,
                cancellationToken);

            if (tariff == null)
                return Failed(BloodBankProcedureOutcome.TariffUnavailable, TariffUnavailableMessage);

            string procedureNumber;

            try
            {
                procedureNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: ProcedureSequenceKey,
                        Prefix: ProcedureNumberPrefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: ProcedureSequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return Failed(
                    BloodBankProcedureOutcome.Invalid,
                    "Nomor tindakan Bank Darah gagal diterbitkan. Hubungi administrator sistem.");
            }

            var entity = new BbkBloodBankProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureNumber = procedureNumber,
                BloodOrderId = order.Id,
                ServiceUnitId = encounter.ServiceUnitId,
                BdrsDoctorId = request.BdrsDoctorId,
                PerformedByUserId = actorUserId,
                PatientClassId = encounter.PatientClassId.Value,
                ProcedureRefId = procedure.Id,
                TariffId = tariff.Id,
                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,

                // Sama dengan harga rumah sakit pada InsuranceCoverageService: nominal data induk,
                // tidak pernah negatif. Tidak ada pengali, diskon, maupun coverage di sini.
                TariffAmountSnapshot = Math.Max(0m, tariff.NormalPrice),
                ProcedureStatus = BbkProcedureStatus.Recorded,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<BbkBloodBankProcedure>().Add(entity);

            AppendTransition(
                entity.Id,
                CreateAction,
                fromStatus: null,
                toStatus: BbkProcedureStatus.Recorded,
                actorUserId,
                occurredAt: now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Succeeded(entity, "Tindakan Bank Darah berhasil dicatat.");
        }

        /// <summary>Menyatakan tindakan selesai: <c>Recorded</c> → <c>Completed</c> (<c>AC-BD-101</c>).</summary>
        /// <remarks>
        /// <para>
        /// <b>Urutannya mengikat</b> (<c>BE-BD-013</c>): perpindahan status dan riwayat <c>Complete</c>
        /// disimpan lebih dulu, baru fakta biaya diserahkan ke Billing. Tidak ada transaksi yang
        /// masih terbuka ketika producer dipanggil, dan kegagalan Billing — ditolak, tak terjangkau,
        /// atau hasilnya tak pasti — <b>tidak</b> mengembalikan tindakan ke <c>Recorded</c>.
        /// </para>
        /// <para>
        /// Tindakan yang sudah <c>Completed</c> tetap ditolak bila diselesaikan lagi
        /// (<c>AC-BD-101</c> jalur gagal). Kirim ulang fakta biaya memakai
        /// <see cref="ResendCostFactAsync"/>, bukan penyelesaian kedua.
        /// </para>
        /// </remarks>
        public async Task<BloodBankProcedureResult> CompleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, ActorUnknownMessage);

            var entity = await _dbContext.Set<BbkBloodBankProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

            if (entity == null)
                return Failed(BloodBankProcedureOutcome.NotFound, NotFoundMessage);

            if (entity.ProcedureStatus != BbkProcedureStatus.Recorded)
            {
                return Failed(
                    BloodBankProcedureOutcome.NotAllowedByState,
                    $"Tindakan berstatus {BbkDisplayLabels.Of(entity.ProcedureStatus)} tidak dapat dinyatakan selesai lagi.");
            }

            var now = DateTime.UtcNow;

            entity.ProcedureStatus = BbkProcedureStatus.Completed;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            AppendTransition(
                entity.Id,
                CompleteAction,
                fromStatus: BbkProcedureStatus.Recorded,
                toStatus: BbkProcedureStatus.Completed,
                actorUserId,
                occurredAt: now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // ProcedureStatus adalah concurrency token: petugas lain sudah menyelesaikannya.
                // Baris riwayat yang sudah ditambahkan ikut dibuang.
                _dbContext.ChangeTracker.Clear();

                return Failed(BloodBankProcedureOutcome.VersionConflict, ConcurrencyMessage);
            }

            // Kebenaran klinis sudah tersimpan. Baru sekarang fakta biaya diserahkan.
            var handoff = await HandOffCostFactAsync(entity.Id, actorUserId, cancellationToken);

            return Succeeded(entity, "Tindakan Bank Darah dinyatakan selesai.", handoff);
        }

        /// <summary>
        /// Mengirim ulang fakta biaya tindakan yang sudah <c>Completed</c> (<c>AC-BD-027</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Ini pengiriman ulang, bukan penyelesaian kedua.</b> Tidak ada perpindahan status, tidak
        /// ada baris riwayat baru, tidak ada salinan tarif baru. Fakta disusun ulang seluruhnya dari
        /// kolom yang tersimpan — termasuk waktu penyelesaian dari riwayat <c>Complete</c> — sehingga
        /// sidik jarinya sama persis dengan kiriman pertama. Producer lalu memutuskan: fakta yang
        /// sudah diterima Billing dikembalikan sebagai <c>Replayed</c>; fakta yang tertahan
        /// <c>Pending</c> dikirim ulang dengan kunci idempotency yang sama; fakta yang ditolak
        /// mendapat revisi baru atas identitas yang sama; fakta <c>OutcomeUnknown</c> menuntut
        /// rekonsiliasi lebih dulu. Tidak satu pun jalur itu membuat charge kedua.
        /// </para>
        /// <para>
        /// Pola ini setara <c>LabSpecimenService.AcceptAsync</c> yang mengirim ulang fakta tanpa
        /// menyentuh keadaan. Bedanya, di sini jalurnya dipisah dari <c>complete</c> karena
        /// <c>AC-BD-101</c> menuntut penyelesaian ulang tetap ditolak.
        /// </para>
        /// </remarks>
        public async Task<BloodBankProcedureResult> ResendCostFactAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
                return Failed(BloodBankProcedureOutcome.Invalid, ActorUnknownMessage);

            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return Failed(BloodBankProcedureOutcome.NotFound, NotFoundMessage);

            if (entity.ProcedureStatus != BbkProcedureStatus.Completed)
            {
                return Failed(
                    BloodBankProcedureOutcome.NotAllowedByState,
                    $"Fakta biaya hanya terbit untuk tindakan yang sudah selesai; tindakan ini berstatus " +
                    $"{BbkDisplayLabels.Of(entity.ProcedureStatus)}.");
            }

            var handoff = await HandOffCostFactAsync(entity.Id, actorUserId, cancellationToken);

            return Succeeded(entity, "Fakta biaya tindakan Bank Darah dikirim ulang.", handoff);
        }

        // =================================================================
        // Penyerahan fakta biaya — DEC-BD-016
        // =================================================================

        /// <summary>
        /// Menyerahkan fakta biaya satu tindakan selesai ke Billing lewat producer resmi.
        /// </summary>
        /// <remarks>
        /// Dipanggil hanya sesudah perubahan klinis tersimpan. Galat tak terduga dari jalur
        /// penyerahan tidak dibiarkan menjalar menjadi <c>500</c>: tindakannya sudah sah
        /// <c>Completed</c>, dan galat itu dilaporkan sebagai hasil yang belum terkonfirmasi supaya
        /// petugas tahu fakta biayanya perlu dikirim ulang atau ditinjau.
        /// </remarks>
        private async Task<ClinicalFactEmissionResult> HandOffCostFactAsync(
            Guid procedureId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = await BuildCostFactRequestAsync(procedureId, cancellationToken);

                if (request == null)
                {
                    return ClinicalFactEmissionResult.Failure(
                        ClinicalFactEmissionKind.Invalid,
                        CostFactSourceIncompleteCode,
                        "Fakta biaya tidak dapat disusun karena data penyelesaian tindakan atau kunjungan " +
                        "ordernya tidak lengkap.");
                }

                return await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                    request,
                    actorUserId,
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Perubahan klinis sudah ter-commit; yang dibuang hanya sisa pelacakan penyerahan
                // yang gagal supaya tidak ikut tersimpan oleh operasi berikutnya.
                _dbContext.ChangeTracker.Clear();

                await _loggerService.WarningAsync(
                    LogCategory,
                    "BloodBankProcedure.CostFactHandoffUnconfirmed",
                    "Penyerahan fakta biaya tindakan Bank Darah tidak dapat dipastikan.",
                    new
                    {
                        ProcedureId = procedureId,
                        ActorUserId = actorUserId,
                        ExceptionType = exception.GetType().Name
                    });

                return ClinicalFactEmissionResult.Failure(
                    ClinicalFactEmissionKind.OutcomeUnknown,
                    CostFactUnconfirmedCode,
                    "Tindakan sudah tersimpan, tetapi penyerahan fakta biaya ke Billing belum dapat " +
                    "dipastikan. Kirim ulang fakta biaya atau minta peninjauan Billing.");
            }
        }

        /// <summary>
        /// Menyusun fakta biaya dari kolom yang <b>tersimpan</b> saja.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Identitas.</b> <c>SourceAggregateId</c> = order darah, <c>SourceItemId</c> = tindakan,
        /// <c>EncounterId</c> dari order. Kantong, alokasi, pemberian, dan pasien tidak pernah menjadi
        /// identitas fakta, sehingga jumlah kantong tidak dapat melipatgandakan charge
        /// (<c>AC-BD-026</c>).
        /// </para>
        /// <para>
        /// <b>Kenapa dibaca ulang dari database, bukan dari entity di memori.</b> Sidik jari producer
        /// dan Billing memuat waktu dan nominal. Waktu di memori berketelitian 100 nanodetik dan
        /// nominal di memori membawa skala data induk tarif; nilai yang dibaca dari kolom
        /// <c>timestamp with time zone</c> dan <c>numeric(18,2)</c> tidak. Membaca dari sumber yang
        /// sama pada kiriman pertama maupun kiriman ulang menjamin sidik jari identik
        /// (<c>AC-BD-027</c>).
        /// </para>
        /// <para>
        /// <b>Tarif.</b> Nominal adalah <c>TariffAmountSnapshot</c> yang dibekukan <c>BE-BD-012</c>;
        /// <c>MstTariff</c> tidak dibaca ulang dan tidak ada nominal dari client. Salinan tarif
        /// dibawa sebagai rujukan — keputusan jumlah tagihan tetap milik Billing.
        /// </para>
        /// <para>
        /// <b>Tidak ada isi yang dapat berubah oleh koreksi.</b> Jumlah atau identitas kantong sengaja
        /// tidak dimuat, sehingga koreksi pemberian sesudahnya tidak mengubah sidik jari dan tidak
        /// memicu revisi biaya (<c>DEC-BD-034</c>, <c>AC-BD-058</c>).
        /// </para>
        /// </remarks>
        private async Task<ClinicalMilestoneFactRequest?> BuildCostFactRequestAsync(
            Guid procedureId,
            CancellationToken cancellationToken)
        {
            var source = await _dbContext.Set<BbkBloodBankProcedure>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == procedureId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.ProcedureStatus == BbkProcedureStatus.Completed)
                .Select(x => new
                {
                    x.Id,
                    x.ProcedureNumber,
                    x.BloodOrderId,
                    EncounterId = x.BloodOrder != null ? x.BloodOrder.EncounterId : Guid.Empty,
                    x.ServiceUnitId,
                    x.PatientClassId,
                    x.ProcedureRefId,
                    x.TariffId,
                    x.ProcedureCodeSnapshot,
                    x.ProcedureNameSnapshot,
                    x.TariffAmountSnapshot
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (source == null || source.EncounterId == Guid.Empty)
                return null;

            // Waktu kejadian adalah waktu penyelesaian yang tersimpan, bukan waktu kiriman.
            var completedAt = await _dbContext.Set<BbkTransitionHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.Scope == BbkTransitionScopes.BloodBankProcedure &&
                    x.EntityId == procedureId &&
                    x.Action == CompleteAction &&
                    !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ThenBy(x => x.OccurredAt)
                .Select(x => (DateTime?)x.OccurredAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (!completedAt.HasValue)
                return null;

            return new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.BloodBankSourceContext,
                SourceAggregateId = source.BloodOrderId,
                SourceItemId = source.Id,
                EffectType = BillingSourceContract.BloodBankChargeEffectType,
                EncounterId = source.EncounterId,
                OccurredAt = completedAt.Value,
                Quantity = 1m,
                Unit = CostFactUnit,
                TariffSnapshot = JsonSerializer.Serialize(new
                {
                    source = "BloodBankProcedureSnapshot",
                    procedureRefId = source.ProcedureRefId,
                    procedureCode = source.ProcedureCodeSnapshot,
                    procedureName = source.ProcedureNameSnapshot,
                    tariffId = source.TariffId,
                    patientClassId = source.PatientClassId,
                    serviceUnitId = source.ServiceUnitId,
                    unitPrice = source.TariffAmountSnapshot
                }),
                RuleSnapshot = JsonSerializer.Serialize(new
                {
                    milestone = "BloodBankProcedureCompleted",
                    procedureNumber = source.ProcedureNumber,
                    chargeBasis = "PerProcedure"
                }),
                CorrelationId = source.BloodOrderId
            };
        }

        // =================================================================
        // Resolusi tarif — DEC-BD-049
        // =================================================================

        /// <summary>
        /// Memilih satu tarif untuk tindakan pada konteks kunjungan (<c>DEC-BD-049</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Kandidat</b> memakai predikat yang sama persis dengan
        /// <c>InsuranceCoverageService.FindProcedureTariffAsync</c> — tarif yang aktif, tidak
        /// dihapus, berlaku pada tanggal pencatatan, milik tindakan ini, dan kolom unit, klinik,
        /// serta kelasnya kosong (berlaku umum) atau sama dengan kunjungan. Artinya tarif milik
        /// <b>kelas lain</b>, unit lain, atau klinik lain tidak pernah menjadi kandidat.
        /// </para>
        /// <para>
        /// <b>Urutan</b> mengutamakan tarif yang cocok dengan kelas pasien, lalu yang paling spesifik
        /// terhadap klinik dan unit, lalu tanggal mulai berlaku terbaru. Tarif tanpa kelas karena
        /// itu menjadi <b>cadangan</b> — dipakai hanya bila tidak ada tarif kelas pasien. Urutan kelas
        /// lebih dulu adalah klarifikasi pemilik pada 11 September 2026; lihat laporan
        /// <c>BE-BD-012</c> bagian 7.
        /// </para>
        /// <para>
        /// <b>Kenapa tidak memanggil <c>InsuranceCoverageService</c> langsung.</b> Method pemilihnya
        /// <c>private</c>, dan jalur publiknya menghitung coverage asuransi — menolak ketika konteks
        /// asuransi kunjungan belum siap, dan memulangkan harga kontrak asuransi, bukan nominal
        /// <c>MstTariff</c>. Mencatat tindakan Bank Darah tidak boleh bergantung pada kesiapan
        /// asuransi.
        /// </para>
        /// <para>
        /// <b>Contoh berangka.</b> Tarif umum Rp150.000 dan tarif kelas VIP Rp250.000. Pasien VIP →
        /// Rp250.000. Pasien kelas 3 → tidak ada tarif kelas 3, tarif umum Rp150.000. Bila hanya
        /// ada tarif kelas 1 → pasien VIP ditolak, bukan diberi harga kelas 1.
        /// </para>
        /// </remarks>
        private async Task<MstTariff?> ResolveTariffAsync(
            Guid procedureId,
            Guid serviceUnitId,
            Guid? clinicId,
            Guid patientClassId,
            DateTime serviceDate,
            CancellationToken cancellationToken)
        {
            var date = serviceDate.Date;

            var candidates = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ProcedureId == procedureId &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date) &&
                    (!x.ServiceUnitId.HasValue || x.ServiceUnitId == serviceUnitId) &&
                    (!x.ClinicId.HasValue || x.ClinicId == clinicId) &&
                    (!x.PatientClassId.HasValue || x.PatientClassId == patientClassId))
                .ToListAsync(cancellationToken);

            return candidates
                .OrderByDescending(x => x.PatientClassId.HasValue)
                .ThenByDescending(x => x.ClinicId.HasValue)
                .ThenByDescending(x => x.ServiceUnitId.HasValue)
                .ThenByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        // =================================================================
        // Riwayat dan penolong
        // =================================================================

        private void AppendTransition(
            Guid procedureId,
            string action,
            BbkProcedureStatus? fromStatus,
            BbkProcedureStatus toStatus,
            Guid actorUserId,
            DateTime occurredAt)
        {
            _dbContext.Set<BbkTransitionHistory>().Add(new BbkTransitionHistory
            {
                Id = Guid.NewGuid(),
                Scope = BbkTransitionScopes.BloodBankProcedure,
                EntityId = procedureId,
                Action = action,
                FromStatus = fromStatus?.ToString(),
                ToStatus = toStatus.ToString(),
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        private async Task<List<BloodBankTransitionDto>> ReadTransitionsAsync(
            Guid procedureId,
            CancellationToken cancellationToken)
            => await _dbContext.Set<BbkTransitionHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.Scope == BbkTransitionScopes.BloodBankProcedure &&
                    x.EntityId == procedureId &&
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

        private IQueryable<BbkBloodBankProcedure> BaseQuery()
            => _dbContext.Set<BbkBloodBankProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static IQueryable<BbkBloodBankProcedure> ApplySort(
            IQueryable<BbkBloodBankProcedure> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "procedurenumber" => descending
                    ? query.OrderByDescending(x => x.ProcedureNumber)
                    : query.OrderBy(x => x.ProcedureNumber),
                "procedurestatus" => descending
                    ? query.OrderByDescending(x => x.ProcedureStatus).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.ProcedureStatus).ThenBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private static BloodBankProcedureResult Succeeded(
            BbkBloodBankProcedure entity,
            string message,
            ClinicalFactEmissionResult? billingHandoff = null)
            => new(BloodBankProcedureOutcome.Success, entity, message, billingHandoff);

        private static BloodBankProcedureResult Failed(BloodBankProcedureOutcome outcome, string message)
            => new(outcome, null, message);
    }

    /// <summary>Hasil satu tindakan pada tindakan Bank Darah. Dipetakan ke HTTP status oleh controller.</summary>
    /// <remarks>
    /// <c>BillingHandoff</c> adalah hasil penyerahan fakta biaya ke Billing (<c>BE-BD-013</c>). Diisi
    /// hanya oleh aksi yang memang menyerahkan fakta; <c>null</c> pada aksi lain.
    /// </remarks>
    public sealed record BloodBankProcedureResult(
        BloodBankProcedureOutcome Outcome,
        BbkBloodBankProcedure? Entity,
        string Message,
        ClinicalFactEmissionResult? BillingHandoff = null);

    /// <summary>Jenis hasil satu tindakan pada tindakan Bank Darah.</summary>
    public enum BloodBankProcedureOutcome
    {
        Success = 0,

        /// <summary>Tindakan tidak ada atau sudah dihapus.</summary>
        NotFound = 1,

        /// <summary>Isian tidak sah — termasuk order yang tidak sah (<c>VAL-BD-026</c>).</summary>
        Invalid = 2,

        /// <summary>Keadaan data tidak memenuhi syarat tindakan ini.</summary>
        NotAllowedByState = 3,

        /// <summary>Tidak ada tarif aktif dan berlaku yang cocok (<c>VAL-BD-084</c>).</summary>
        TariffUnavailable = 4,

        /// <summary>Tindakan sudah berubah di tangan orang lain.</summary>
        VersionConflict = 5
    }
}
