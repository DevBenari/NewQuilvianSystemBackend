using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Aturan kepergian pasien dari IGD: tujuan, dua rangkaian status, kejadian tambah-saja,
    /// dan sikap atas pesanan yang belum selesai.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-031</c>..<c>BE-IGD-035</c>. Menggantikan <c>EmergencyTransferService</c>.
    /// </remarks>
    public class EmergencyDepartureService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;
        private readonly EmergencyUnitAuthorityService _unitAuthorityService;

        public EmergencyDepartureService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService,
            EmergencyUnitAuthorityService unitAuthorityService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _unitAuthorityService = unitAuthorityService;
        }

        public sealed record Hasil<T>(T? Data, int StatusCode, string? Penolakan)
        {
            public bool Berhasil => Penolakan == null;
            public static Hasil<T> Ok(T data) => new(data, StatusCodes.Status200OK, null);
            public static Hasil<T> Gagal(int statusCode, string penolakan) => new(default, statusCode, penolakan);
        }

        public IQueryable<EmgDeparture> Query()
            => _dbContext.Set<EmgDeparture>()
                .AsNoTracking()
                .Include(x => x.FromServiceUnit)
                .Include(x => x.ToServiceUnit)
                .Include(x => x.Events)
                .Include(x => x.OrderItems).ThenInclude(x => x.ToServiceUnit)
                .Where(x => !x.IsDelete);

        public Task<EmgDeparture?> FindAsync(Guid id, CancellationToken cancellationToken = default)
            => Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public async Task<Hasil<EmgDeparture>> CreateAsync(
            CreateEmergencyDepartureRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var penolakan = await ValidateRequestAsync(request, cancellationToken);
            if (penolakan != null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, penolakan);

            foreach (var item in request.OrderItems)
            {
                penolakan = ValidateOrderItem(item);
                if (penolakan != null)
                    return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, penolakan);
            }

            var now = DateTime.UtcNow;
            var occurredAt = request.RequestedAt == default ? now : request.RequestedAt;
            if (occurredAt > now)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Waktu kejadian tidak boleh berada di masa depan.");

            var entity = new EmgDeparture
            {
                Id = Guid.NewGuid(),
                EmergencyVisitId = request.EmergencyVisitId,
                DepartureNumber = string.IsNullOrWhiteSpace(request.DepartureNumber)
                    ? GenerateNumber(now)
                    : request.DepartureNumber.Trim(),
                FromServiceUnitId = KosongKeNull(request.FromServiceUnitId),
                ToServiceUnitId = request.ToServiceUnitId,
                PhysicalStatus = EmergencyPhysicalStatus.Prepared,
                HandoverStatus = EmergencyHandoverStatus.Submitted,
                RequestedAt = occurredAt,
                RequestedByUserId = request.RequestedByUserId == Guid.Empty ? actorUserId : request.RequestedByUserId,
                SendingNurseUserId = KosongKeNull(request.SendingNurseUserId),
                DepartureReason = Rapikan(request.DepartureReason),
                SituationSummary = Rapikan(request.SituationSummary),
                BackgroundSummary = Rapikan(request.BackgroundSummary),
                AssessmentSummary = Rapikan(request.AssessmentSummary),
                RecommendationSummary = Rapikan(request.RecommendationSummary),
                UnavailableSections = Rapikan(request.UnavailableSections),
                UnavailableSectionReason = Rapikan(request.UnavailableSectionReason),
                AllergySnapshot = Rapikan(request.AllergySnapshot),
                LastVitalSignId = KosongKeNull(request.LastVitalSignId),
                TriageLevelSnapshot = Rapikan(request.TriageLevelSnapshot),
                Notes = Rapikan(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            entity.Events.Add(BuatKejadian(entity.Id, EmergencyDepartureEventType.Prepared, occurredAt, actorUserId));
            foreach (var input in request.OrderItems)
                entity.OrderItems.Add(BuatOrderItem(entity.Id, input, actorUserId, now));

            _dbContext.Set<EmgDeparture>().Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Hasil<EmgDeparture>.Gagal(
                    StatusCodes.Status409Conflict,
                    "Kepergian gagal disimpan karena nomor dokumen atau data terkait sudah digunakan.");
            }

            return Hasil<EmgDeparture>.Ok((await FindAsync(entity.Id, cancellationToken))!);
        }

        public async Task<Hasil<EmgDeparture>> DepartAsync(
            Guid id, DepartEmergencyDepartureRequest request, Guid actorUserId,
            CancellationToken cancellationToken = default)
            => await UbahFisikAsync(id, EmergencyPhysicalStatus.Departed,
                EmergencyDepartureEventType.Departed, request.OccurredAt, request.Reason,
                request.DowntimeReference, actorUserId, null, cancellationToken);

        public async Task<Hasil<EmgDeparture>> ArriveAsync(
            Guid id, ArriveEmergencyDepartureRequest request, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status404NotFound, "Data kepergian pasien IGD tidak ditemukan.");

            var authority = await _unitAuthorityService.PeriksaAsync(
                actorUserId, entity.ToServiceUnitId, DateTime.UtcNow, "mencatat kedatangan pasien", cancellationToken);
            if (!authority.Berwenang)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status403Forbidden, authority.Penolakan!);

            return await UbahFisikAsync(id, EmergencyPhysicalStatus.Arrived,
                EmergencyDepartureEventType.Arrived, request.OccurredAt, null,
                request.DowntimeReference, actorUserId, request.ReceivingNurseUserId, cancellationToken);
        }

        public async Task<Hasil<EmgDeparture>> CancelAsync(
            Guid id, CancelEmergencyDepartureRequest request, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.CancellationReason))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Alasan pembatalan kepergian wajib diisi.");

            var result = await UbahFisikAsync(id, EmergencyPhysicalStatus.Cancelled,
                EmergencyDepartureEventType.Cancelled, request.OccurredAt,
                request.CancellationReason, null, actorUserId, null, cancellationToken,
                EmergencyHandoverStatus.Cancelled);
            return result;
        }

        public async Task<Hasil<EmgDeparture>> SubmitHandoverAsync(
            Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgDeparture>()
                .Include(x => x.EmergencyVisit)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status404NotFound, "Data kepergian pasien IGD tidak ditemukan.");

            await BentukPesananInternalAsync(entity, actorUserId, cancellationToken);
            var tanpaSikap = entity.OrderItems.Count(x => !x.IsDelete && x.IsEffective && !Enum.IsDefined(x.Action));
            if (tanpaSikap > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Hasil<EmgDeparture>.Gagal(
                    StatusCodes.Status400BadRequest,
                    $"Masih ada {tanpaSikap} pesanan yang belum ditentukan sikapnya.");
            }

            return await UbahHandoverAsync(entity, EmergencyHandoverStatus.Pending,
                EmergencyDepartureEventType.HandoverSubmitted, null, actorUserId, DateTime.UtcNow, cancellationToken);
        }

        public async Task<Hasil<EmgDeparture>> UpdateHandoverAsync(
            Guid id, UpdateEmergencyHandoverStatusRequest request, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status404NotFound, "Data kepergian pasien IGD tidak ditemukan.");

            if (request.HandoverStatus is not (EmergencyHandoverStatus.Accepted or EmergencyHandoverStatus.Rejected))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Aksi ini hanya menerima status Accepted atau Rejected.");
            if (request.HandoverStatus == EmergencyHandoverStatus.Rejected && string.IsNullOrWhiteSpace(request.RejectionReason))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Alasan penolakan serah terima wajib diisi.");

            var authority = await _unitAuthorityService.PeriksaAsync(
                actorUserId, entity.ToServiceUnitId, DateTime.UtcNow, "meninjau serah terima", cancellationToken);
            if (!authority.Berwenang)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status403Forbidden, authority.Penolakan!);

            var eventType = request.HandoverStatus == EmergencyHandoverStatus.Accepted
                ? EmergencyDepartureEventType.HandoverAccepted
                : EmergencyDepartureEventType.HandoverRejected;
            return await UbahHandoverAsync(entity, request.HandoverStatus, eventType,
                request.RejectionReason, actorUserId, request.OccurredAt ?? DateTime.UtcNow, cancellationToken);
        }

        public async Task<Hasil<EmgHandoverOrderItem>> AddExternalOrderAsync(
            Guid id, EmergencyHandoverOrderItemInput input, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            input.OrderSource = EmergencyOrderSource.External;
            var penolakan = ValidateOrderItem(input);
            if (penolakan != null)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status400BadRequest, penolakan);
            var entity = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status404NotFound, "Data kepergian pasien IGD tidak ditemukan.");
            var authority = await PeriksaUnitAsalAsync(entity, actorUserId, "mendaftarkan pesanan luar sistem", cancellationToken);
            if (!authority.Berwenang)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status403Forbidden, authority.Penolakan!);
            var item = BuatOrderItem(id, input, actorUserId, DateTime.UtcNow);
            _dbContext.Add(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgHandoverOrderItem>.Ok(item);
        }

        public async Task<Hasil<EmgHandoverOrderItem>> SetOrderActionAsync(
            Guid departureId, Guid itemId, EmergencyHandoverOrderItemInput input,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var penolakan = ValidateOrderItem(input);
            if (penolakan != null)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status400BadRequest, penolakan);
            var departure = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == departureId && !x.IsDelete, cancellationToken);
            var current = await _dbContext.Set<EmgHandoverOrderItem>()
                .FirstOrDefaultAsync(x => x.Id == itemId && x.EmergencyDepartureId == departureId && !x.IsDelete, cancellationToken);
            if (departure == null || current == null)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status404NotFound, "Pesanan kepergian tidak ditemukan.");
            var authority = await PeriksaUnitAsalAsync(departure, actorUserId, "menentukan sikap pesanan", cancellationToken);
            if (!authority.Berwenang)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status403Forbidden, authority.Penolakan!);

            if (Enum.IsDefined(current.Action) && current.AcceptanceStatus != EmergencyOrderAcceptanceStatus.Rejected)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status409Conflict, "Sikap atas pesanan ini sudah tercatat. Perubahannya dicatat sebagai koreksi.");

            if (!Enum.IsDefined(current.Action))
            {
                TerapkanAction(current, input, actorUserId, DateTime.UtcNow);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Hasil<EmgHandoverOrderItem>.Ok(current);
            }

            current.IsEffective = false;
            current.UpdateDateTime = DateTime.UtcNow;
            current.UpdateBy = actorUserId;
            input.OrderKind = current.OrderKind;
            input.OrderSource = current.OrderSource;
            input.OrderReferenceId = current.OrderReferenceId;
            input.ExternalReference = current.ExternalReference;
            input.OrderDescription = current.OrderDescription;
            input.SupersedesOrderItemId = current.Id;
            var replacement = BuatOrderItem(departureId, input, actorUserId, DateTime.UtcNow);
            _dbContext.Add(replacement);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgHandoverOrderItem>.Ok(replacement);
        }

        public async Task<Hasil<EmgHandoverOrderItem>> SetOrderAcceptanceAsync(
            Guid departureId, Guid itemId, EmergencyOrderAcceptanceStatus target,
            string? rejectionReason, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var departure = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == departureId && !x.IsDelete, cancellationToken);
            var item = await _dbContext.Set<EmgHandoverOrderItem>()
                .FirstOrDefaultAsync(x => x.Id == itemId && x.EmergencyDepartureId == departureId && !x.IsDelete && x.IsEffective, cancellationToken);
            if (departure == null || item == null)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status404NotFound, "Pesanan kepergian tidak ditemukan.");
            if (item.AcceptanceStatus != EmergencyOrderAcceptanceStatus.Pending || target is not (EmergencyOrderAcceptanceStatus.Accepted or EmergencyOrderAcceptanceStatus.Rejected))
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status409Conflict, "Status penerimaan pesanan tidak dapat diubah dari keadaan saat ini.");
            if (target == EmergencyOrderAcceptanceStatus.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status400BadRequest, "Alasan penolakan pesanan wajib diisi.");
            var authority = await _unitAuthorityService.PeriksaAsync(
                actorUserId, departure.ToServiceUnitId, DateTime.UtcNow, "menerima atau menolak pesanan", cancellationToken);
            if (!authority.Berwenang)
                return Hasil<EmgHandoverOrderItem>.Gagal(StatusCodes.Status403Forbidden, authority.Penolakan!);
            item.AcceptanceStatus = target;
            item.AcceptedByUserId = actorUserId;
            item.AcceptedAt = DateTime.UtcNow;
            item.RejectionReason = target == EmergencyOrderAcceptanceStatus.Rejected ? Rapikan(rejectionReason) : null;
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgHandoverOrderItem>.Ok(item);
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyDepartureRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.ToServiceUnitId == Guid.Empty)
                return "ToServiceUnitId wajib diisi.";

            // Validation bagian 4 aturan 1.
            if (request.FromServiceUnitId.HasValue &&
                request.FromServiceUnitId.Value == request.ToServiceUnitId)
                return "Unit tujuan harus berbeda dengan unit asal.";

            var visitExists = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Completed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            if (!visitExists)
                return "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";

            if (request.FromServiceUnitId.HasValue &&
                request.FromServiceUnitId.Value != Guid.Empty &&
                !await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.FromServiceUnitId.Value && !x.IsDelete, cancellationToken))
                return "FromServiceUnitId tidak ditemukan.";

            if (!await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ToServiceUnitId && !x.IsDelete, cancellationToken))
                return "ToServiceUnitId tidak ditemukan.";

            return ValidateSbar(request);
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyDepartureRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyDepartureRequest)request, cancellationToken);

        /// <summary>
        /// Validation bagian 4 aturan 2 dan 3 — <c>IGD-DEC-056</c>, <c>IGD-DEC-079</c>.
        /// </summary>
        /// <remarks>
        /// Empat bagian SBAR wajib terisi <b>atau</b> ditandai tidak dapat diisi beserta
        /// alasannya. Yang dilarang bukan bagian yang kosong, melainkan bagian yang kosong
        /// tanpa seorang pun menyatakan mengapa.
        /// </remarks>
        public static string? ValidateSbar(CreateEmergencyDepartureRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var ditandaiTidakDapatDiisi = (request.UnavailableSections ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToLowerInvariant())
                .ToHashSet();

            var bagian = new (string Nama, string? Isi)[]
            {
                ("Situation", request.SituationSummary),
                ("Background", request.BackgroundSummary),
                ("Assessment", request.AssessmentSummary),
                ("Recommendation", request.RecommendationSummary),
            };

            foreach (var (nama, isi) in bagian)
            {
                if (!string.IsNullOrWhiteSpace(isi))
                    continue;

                if (!ditandaiTidakDapatDiisi.Contains(nama.ToLowerInvariant()))
                {
                    return $"Bagian {nama} belum diisi. Isi, atau tandai tidak dapat diisi " +
                           "beserta alasannya.";
                }
            }

            if (ditandaiTidakDapatDiisi.Count > 0 &&
                string.IsNullOrWhiteSpace(request.UnavailableSectionReason))
            {
                return "Alasan wajib diisi untuk bagian yang ditandai tidak dapat diisi.";
            }

            return null;
        }

        /// <summary>
        /// Matriks transisi keadaan fisik pasien — state matrix bagian 2.
        /// </summary>
        public static bool CanTransition(EmergencyPhysicalStatus current, EmergencyPhysicalStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyPhysicalStatus.Prepared => target is EmergencyPhysicalStatus.Departed
                    or EmergencyPhysicalStatus.Cancelled,
                EmergencyPhysicalStatus.Departed => target is EmergencyPhysicalStatus.Arrived
                    or EmergencyPhysicalStatus.Cancelled,
                _ => false
            };
        }

        /// <summary>
        /// Matriks transisi dokumen serah terima — state matrix bagian 3.
        /// </summary>
        /// <remarks>
        /// <c>Rejected</c> sengaja <b>bukan</b> terminal: serah terima yang ditolak tetap wajib
        /// dituntaskan, sehingga dokumennya boleh diperbaiki lalu diajukan kembali sebagai
        /// <c>Pending</c> — <c>IGD-DEC-062</c>.
        /// </remarks>
        public static bool CanTransition(EmergencyHandoverStatus current, EmergencyHandoverStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyHandoverStatus.Submitted => target is EmergencyHandoverStatus.Pending
                    or EmergencyHandoverStatus.Accepted
                    or EmergencyHandoverStatus.Rejected
                    or EmergencyHandoverStatus.Cancelled,
                EmergencyHandoverStatus.Pending => target is EmergencyHandoverStatus.Accepted
                    or EmergencyHandoverStatus.Rejected
                    or EmergencyHandoverStatus.Cancelled,
                EmergencyHandoverStatus.Rejected => target is EmergencyHandoverStatus.Pending
                    or EmergencyHandoverStatus.Accepted
                    or EmergencyHandoverStatus.Cancelled,
                _ => false
            };
        }

        /// <summary>
        /// Kombinasi dua rangkaian yang tidak sah — state matrix bagian 4.
        /// Mengembalikan pesan penolakan, atau <c>null</c> bila kombinasinya sah.
        /// </summary>
        public static string? ValidateKombinasi(
            EmergencyPhysicalStatus physical,
            EmergencyHandoverStatus handover)
        {
            // Validation bagian 4 aturan 7.
            if (handover == EmergencyHandoverStatus.Accepted &&
                physical == EmergencyPhysicalStatus.Prepared)
            {
                return "Serah terima tidak dapat diterima sebelum pasien berangkat dari IGD.";
            }

            if (physical == EmergencyPhysicalStatus.Cancelled &&
                handover != EmergencyHandoverStatus.Cancelled)
            {
                return "Kepergian yang dibatalkan membatalkan dokumen serah terimanya sekaligus.";
            }

            return null;
        }

        /// <summary>
        /// Keadaan fisik yang berarti kepergiannya <b>sudah tuntas</b> bagi gerbang penutupan
        /// kunjungan.
        /// </summary>
        /// <remarks>
        /// <c>IGD-DEC-106</c>, menjawab <c>IGD-OQ-082</c>. Gerbang penutupan membaca
        /// <b>keadaan fisik saja</b>. Dokumen serah terima yang belum final tidak menahan
        /// penutupan, karena yang dapat menuntaskannya adalah unit penerima — bukan IGD yang
        /// sedang menutup — dan karena kunjungan yang tertahan akan memblokir pendaftaran
        /// pasien yang sama ketika ia datang kembali (<c>BE-IGD-025</c>, <c>IGD-DEC-084</c>).
        /// </remarks>
        public static bool KepergianSudahTuntas(EmergencyPhysicalStatus physical)
            => physical is EmergencyPhysicalStatus.Arrived or EmergencyPhysicalStatus.Cancelled;

        /// <summary>
        /// Dokumen serah terima yang belum mencapai keadaan final. Tidak menahan penutupan,
        /// tetapi wajib tercatat dan tampil pada daftar pantau — <c>IGD-DEC-106</c> butir (a)
        /// dan (c).
        /// </summary>
        public static bool DokumenBelumFinal(EmergencyHandoverStatus handover)
            => handover is not (EmergencyHandoverStatus.Accepted or EmergencyHandoverStatus.Cancelled);

        /// <summary>
        /// Nilai awal <see cref="EmergencyOrderAcceptanceStatus"/> yang ditentukan sikap
        /// pesanan — state matrix bagian 6a.1.
        /// </summary>
        public static EmergencyOrderAcceptanceStatus AcceptanceAwal(EmergencyOrderAction action)
            => action == EmergencyOrderAction.Handover
                ? EmergencyOrderAcceptanceStatus.Pending
                : EmergencyOrderAcceptanceStatus.NotRequired;

        /// <summary>
        /// Aturan satu baris pesanan — validation bagian 5.
        /// </summary>
        public static string? ValidateOrderItem(EmergencyHandoverOrderItemInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (string.IsNullOrWhiteSpace(input.OrderDescription))
                return "Uraian pesanan wajib diisi.";

            // Aturan 8.
            if (input.OrderSource == EmergencyOrderSource.Internal &&
                (!input.OrderReferenceId.HasValue || input.OrderReferenceId.Value == Guid.Empty))
                return "Pesanan internal wajib menunjuk pesanan yang ada.";

            // Aturan 7.
            if (input.OrderSource == EmergencyOrderSource.External &&
                string.IsNullOrWhiteSpace(input.ExternalReference))
                return "Pesanan di luar sistem wajib menyertakan nomor rujukan dan uraiannya.";

            // Aturan 2.
            if (input.Action == EmergencyOrderAction.Cancel &&
                string.IsNullOrWhiteSpace(input.ActionReason))
                return "Alasan pembatalan pesanan wajib diisi.";

            // Aturan 10.
            if (input.Action == EmergencyOrderAction.Handover &&
                (!input.ToServiceUnitId.HasValue || input.ToServiceUnitId.Value == Guid.Empty))
                return "Sikap serah terima wajib menyebutkan unit penerima.";

            return null;
        }

        /// <summary>
        /// Pesanan yang menahan penutupan kunjungan — validation bagian 5.1 dan bagian 6
        /// aturan 4.
        /// </summary>
        /// <remarks>
        /// Yang menahan hanya dua: pesanan <b>tanpa sikap sama sekali</b>, dan pesanan yang
        /// <b>ditolak unit penerima dan belum diberi sikap pengganti</b>. Sikap
        /// <c>Continue</c> tidak pernah menahan — ia berarti pesanan memang sengaja dibiarkan
        /// berjalan sampai hasil final meski pasien sudah pergi.
        /// </remarks>
        public async Task<string?> ValidatePesananSebelumPenutupanAsync(
            Guid emergencyVisitId,
            CancellationToken cancellationToken = default)
        {
            var pesananDitolak = await _dbContext.Set<EmgHandoverOrderItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                    && x.IsEffective
                    && x.AcceptanceStatus == EmergencyOrderAcceptanceStatus.Rejected
                    && x.EmergencyDeparture != null
                    && x.EmergencyDeparture.EmergencyVisitId == emergencyVisitId)
                .Select(x => x.OrderDescription)
                .ToListAsync(cancellationToken);

            if (pesananDitolak.Count == 0)
                return null;

            var daftar = string.Join(", ", pesananDitolak.Take(5));
            var sisa = pesananDitolak.Count > 5 ? $" dan {pesananDitolak.Count - 5} lainnya" : string.Empty;

            return "Ada pesanan yang ditolak unit penerima dan belum ditetapkan sikap " +
                   $"penggantinya: {daftar}{sisa}.";
        }

        public async Task<Hasil<EmgDepartureEvent>> AmendEventAsync(
            Guid departureId, Guid eventId, AmendDepartureEventRequest request,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (request.OccurredAt > DateTime.UtcNow)
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status400BadRequest, "Waktu kejadian tidak boleh berada di masa depan.");
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status400BadRequest, "Alasan koreksi wajib diisi.");
            var current = await _dbContext.Set<EmgDepartureEvent>()
                .FirstOrDefaultAsync(x => x.Id == eventId && x.EmergencyDepartureId == departureId && x.IsEffective && !x.IsDelete, cancellationToken);
            if (current == null)
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status404NotFound, "Kejadian kepergian tidak ditemukan atau sudah dikoreksi.");
            current.IsEffective = false;
            current.UpdateDateTime = DateTime.UtcNow;
            current.UpdateBy = actorUserId;
            var replacement = BuatKejadian(departureId, EmergencyDepartureEventType.Amended,
                request.OccurredAt, actorUserId, request.Reason, request.DowntimeReference, current.Id);
            _dbContext.Add(replacement);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgDepartureEvent>.Ok(replacement);
        }

        public async Task<Hasil<EmgDepartureEvent>> ReverseEventAsync(
            Guid departureId, Guid eventId, ReverseDepartureEventRequest request,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status400BadRequest, "Alasan pembalikan wajib diisi.");
            if (request.ApprovedByUserId == Guid.Empty || request.ApprovedByUserId == actorUserId)
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status400BadRequest, "Pembalikan wajib disetujui orang kedua yang berbeda dari pencatat.");
            var current = await _dbContext.Set<EmgDepartureEvent>()
                .FirstOrDefaultAsync(x => x.Id == eventId && x.EmergencyDepartureId == departureId && x.IsEffective && !x.IsDelete, cancellationToken);
            if (current == null)
                return Hasil<EmgDepartureEvent>.Gagal(StatusCodes.Status404NotFound, "Kejadian kepergian tidak ditemukan atau sudah dibalik.");
            current.IsEffective = false;
            current.UpdateDateTime = DateTime.UtcNow;
            current.UpdateBy = actorUserId;
            var reversal = BuatKejadian(departureId, EmergencyDepartureEventType.Reversed,
                DateTime.UtcNow, actorUserId, request.Reason, null, current.Id, request.ApprovedByUserId);
            _dbContext.Add(reversal);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgDepartureEvent>.Ok(reversal);
        }

        private async Task<Hasil<EmgDeparture>> UbahFisikAsync(
            Guid id, EmergencyPhysicalStatus target, EmergencyDepartureEventType eventType,
            DateTime? occurredAt, string? reason, string? downtimeReference, Guid actorUserId,
            Guid? receivingNurseUserId, CancellationToken cancellationToken,
            EmergencyHandoverStatus? handoverTarget = null)
        {
            var entity = await _dbContext.Set<EmgDeparture>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status404NotFound, "Data kepergian pasien IGD tidak ditemukan.");
            if (!CanTransition(entity.PhysicalStatus, target))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status409Conflict, $"Status fisik tidak dapat berubah dari {entity.PhysicalStatus} ke {target}.");
            if (handoverTarget.HasValue && !CanTransition(entity.HandoverStatus, handoverTarget.Value))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status409Conflict, $"Status serah terima tidak dapat berubah dari {entity.HandoverStatus} ke {handoverTarget.Value}.");
            var now = DateTime.UtcNow;
            var actual = occurredAt ?? now;
            if (actual > now)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Waktu kejadian tidak boleh berada di masa depan.");
            entity.PhysicalStatus = target;
            if (target == EmergencyPhysicalStatus.Departed) entity.DepartedAt = actual;
            if (target == EmergencyPhysicalStatus.Arrived)
            {
                entity.ArrivedAt = actual;
                entity.ReceivingNurseUserId = KosongKeNull(receivingNurseUserId) ?? actorUserId;
            }
            if (target == EmergencyPhysicalStatus.Cancelled)
            {
                entity.CancellationReason = Rapikan(reason);
                entity.HandoverStatus = EmergencyHandoverStatus.Cancelled;
            }
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            _dbContext.Add(BuatKejadian(id, eventType, actual, actorUserId, reason, downtimeReference));
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgDeparture>.Ok((await FindAsync(id, cancellationToken))!);
        }

        private async Task<Hasil<EmgDeparture>> UbahHandoverAsync(
            EmgDeparture entity, EmergencyHandoverStatus target,
            EmergencyDepartureEventType eventType, string? reason, Guid actorUserId,
            DateTime occurredAt, CancellationToken cancellationToken)
        {
            if (!CanTransition(entity.HandoverStatus, target))
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status409Conflict, $"Status serah terima tidak dapat berubah dari {entity.HandoverStatus} ke {target}.");
            if (occurredAt > DateTime.UtcNow)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status400BadRequest, "Waktu kejadian tidak boleh berada di masa depan.");
            var combination = ValidateKombinasi(entity.PhysicalStatus, target);
            if (combination != null)
                return Hasil<EmgDeparture>.Gagal(StatusCodes.Status409Conflict, combination);
            entity.HandoverStatus = target;
            entity.HandoverRejectionReason = target == EmergencyHandoverStatus.Rejected ? Rapikan(reason) : null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            _dbContext.Add(BuatKejadian(entity.Id, eventType, occurredAt, actorUserId, reason));
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Hasil<EmgDeparture>.Ok((await FindAsync(entity.Id, cancellationToken))!);
        }

        private async Task BentukPesananInternalAsync(
            EmgDeparture departure, Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var encounterId = departure.EmergencyVisit?.EncounterId;
            if (!encounterId.HasValue || encounterId == Guid.Empty)
                return;
            var existing = departure.OrderItems
                .Where(x => !x.IsDelete && x.IsEffective && x.OrderReferenceId.HasValue)
                .Select(x => x.OrderReferenceId!.Value).ToHashSet();
            var now = DateTime.UtcNow;

            var prescriptions = await _dbContext.Set<TrxPrescription>().AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete
                    && x.FulfillmentStatus != PrescriptionFulfillmentStatus.Dispensed
                    && x.FulfillmentStatus != PrescriptionFulfillmentStatus.Rejected
                    && x.FulfillmentStatus != PrescriptionFulfillmentStatus.Cancelled)
                .Select(x => new { x.Id, x.PrescriptionNumber }).ToListAsync(cancellationToken);
            foreach (var x in prescriptions.Where(x => !existing.Contains(x.Id)))
                departure.OrderItems.Add(BuatPesananBelumBersikap(departure.Id,
                    EmergencyOrderKind.Medication, x.Id, $"Resep {x.PrescriptionNumber}", actorUserId, now));

            var procedures = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete
                    && x.ProcedureStatus != PatientProcedureStatus.Completed
                    && x.ProcedureStatus != PatientProcedureStatus.Cancelled)
                .Select(x => new { x.Id, x.ProcedureCodeSnapshot, x.ProcedureNameSnapshot }).ToListAsync(cancellationToken);
            foreach (var x in procedures.Where(x => !existing.Contains(x.Id)))
                departure.OrderItems.Add(BuatPesananBelumBersikap(departure.Id,
                    EmergencyOrderKind.Procedure, x.Id,
                    $"{x.ProcedureCodeSnapshot} - {x.ProcedureNameSnapshot}".Trim(' ', '-'), actorUserId, now));

            var labOrders = await _dbContext.Set<LabOrder>().AsNoTracking()
                .Include(x => x.Procedure)
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .Select(x => new { x.Id, Name = x.Procedure != null ? x.Procedure.ProcedureName : "Pemeriksaan laboratorium" })
                .ToListAsync(cancellationToken);
            foreach (var x in labOrders.Where(x => !existing.Contains(x.Id)))
                departure.OrderItems.Add(BuatPesananBelumBersikap(departure.Id,
                    EmergencyOrderKind.LaboratoryOrder, x.Id, x.Name, actorUserId, now));
        }

        private async Task<EmergencyUnitAuthorityService.Hasil> PeriksaUnitAsalAsync(
            EmgDeparture departure, Guid actorUserId, string action,
            CancellationToken cancellationToken)
        {
            if (!departure.FromServiceUnitId.HasValue || departure.FromServiceUnitId == Guid.Empty)
                return new(false, true, "Unit asal belum tercatat sehingga kewenangan tindakan tidak dapat diperiksa.");
            return await _unitAuthorityService.PeriksaAsync(actorUserId,
                departure.FromServiceUnitId.Value, DateTime.UtcNow, action, cancellationToken);
        }

        private static EmgDepartureEvent BuatKejadian(
            Guid departureId, EmergencyDepartureEventType type, DateTime occurredAt,
            Guid actorUserId, string? reason = null, string? downtimeReference = null,
            Guid? supersedesEventId = null, Guid? approvedByUserId = null)
            => new()
            {
                Id = Guid.NewGuid(), EmergencyDepartureId = departureId, EventType = type,
                OccurredAt = occurredAt, RecordedAt = DateTime.UtcNow,
                RecordedByUserId = actorUserId, Reason = Rapikan(reason),
                DowntimeReference = Rapikan(downtimeReference), IsEffective = true,
                SupersedesEventId = supersedesEventId, ApprovedByUserId = approvedByUserId,
                IsActive = true, CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId,
                IsDelete = false, IsCancel = false
            };

        private static EmgHandoverOrderItem BuatOrderItem(
            Guid departureId, EmergencyHandoverOrderItemInput input,
            Guid actorUserId, DateTime now)
        {
            var item = new EmgHandoverOrderItem
            {
                Id = Guid.NewGuid(), EmergencyDepartureId = departureId,
                OrderKind = input.OrderKind, OrderSource = input.OrderSource,
                OrderReferenceId = KosongKeNull(input.OrderReferenceId),
                ExternalReference = Rapikan(input.ExternalReference),
                OrderDescription = input.OrderDescription.Trim(), IsEffective = true,
                SupersedesOrderItemId = KosongKeNull(input.SupersedesOrderItemId),
                IsActive = true, CreateDateTime = now, CreateBy = actorUserId,
                IsDelete = false, IsCancel = false
            };
            TerapkanAction(item, input, actorUserId, now);
            return item;
        }

        private static EmgHandoverOrderItem BuatPesananBelumBersikap(
            Guid departureId, EmergencyOrderKind kind, Guid referenceId,
            string description, Guid actorUserId, DateTime now)
            => new()
            {
                Id = Guid.NewGuid(), EmergencyDepartureId = departureId,
                OrderKind = kind, OrderSource = EmergencyOrderSource.Internal,
                OrderReferenceId = referenceId, OrderDescription = description,
                Action = 0, ActionByUserId = Guid.Empty, ActionAt = DateTime.MinValue,
                AcceptanceStatus = EmergencyOrderAcceptanceStatus.NotRequired,
                IsEffective = true, IsActive = true, CreateDateTime = now,
                CreateBy = actorUserId, IsDelete = false, IsCancel = false
            };

        private static void TerapkanAction(
            EmgHandoverOrderItem item, EmergencyHandoverOrderItemInput input,
            Guid actorUserId, DateTime now)
        {
            item.Action = input.Action;
            item.ActionReason = Rapikan(input.ActionReason);
            item.ActionByUserId = actorUserId;
            item.ActionAt = now;
            item.ToServiceUnitId = input.Action == EmergencyOrderAction.Handover
                ? KosongKeNull(input.ToServiceUnitId) : null;
            item.AcceptanceStatus = AcceptanceAwal(input.Action);
            item.AcceptedByUserId = null;
            item.AcceptedAt = null;
            item.RejectionReason = null;
        }

        public static EmergencyDepartureResponse ToResponse(EmgDeparture x)
            => new()
            {
                Id = x.Id, EmergencyVisitId = x.EmergencyVisitId,
                DepartureNumber = x.DepartureNumber, FromServiceUnitId = x.FromServiceUnitId,
                FromServiceUnitName = x.FromServiceUnit?.ServiceUnitName,
                ToServiceUnitId = x.ToServiceUnitId, ToServiceUnitName = x.ToServiceUnit?.ServiceUnitName,
                PhysicalStatus = x.PhysicalStatus, HandoverStatus = x.HandoverStatus,
                RequestedAt = x.RequestedAt, RequestedByUserId = x.RequestedByUserId,
                DepartedAt = x.DepartedAt, ArrivedAt = x.ArrivedAt,
                SendingNurseUserId = x.SendingNurseUserId, ReceivingNurseUserId = x.ReceivingNurseUserId,
                DepartureReason = x.DepartureReason, SituationSummary = x.SituationSummary,
                BackgroundSummary = x.BackgroundSummary, AssessmentSummary = x.AssessmentSummary,
                RecommendationSummary = x.RecommendationSummary, UnavailableSections = x.UnavailableSections,
                UnavailableSectionReason = x.UnavailableSectionReason, AllergySnapshot = x.AllergySnapshot,
                LastVitalSignId = x.LastVitalSignId, TriageLevelSnapshot = x.TriageLevelSnapshot,
                HandoverRejectionReason = x.HandoverRejectionReason,
                CancellationReason = x.CancellationReason, Notes = x.Notes,
                IsActive = x.IsActive, CreateDateTime = x.CreateDateTime, UpdateDateTime = x.UpdateDateTime,
                Events = x.Events.OrderBy(e => e.OccurredAt).ThenBy(e => e.RecordedAt).Select(ToResponse).ToList(),
                OrderItems = x.OrderItems.OrderBy(i => i.ActionAt).Select(ToResponse).ToList()
            };

        public static EmergencyDepartureEventResponse ToResponse(EmgDepartureEvent x)
            => new() { Id = x.Id, EmergencyDepartureId = x.EmergencyDepartureId,
                EventType = x.EventType, OccurredAt = x.OccurredAt, RecordedAt = x.RecordedAt,
                RecordedByUserId = x.RecordedByUserId, Reason = x.Reason,
                DowntimeReference = x.DowntimeReference, IsEffective = x.IsEffective,
                SupersedesEventId = x.SupersedesEventId, ApprovedByUserId = x.ApprovedByUserId };

        public static EmergencyHandoverOrderItemResponse ToResponse(EmgHandoverOrderItem x)
            => new() { Id = x.Id, EmergencyDepartureId = x.EmergencyDepartureId,
                OrderKind = x.OrderKind, OrderSource = x.OrderSource,
                OrderReferenceId = x.OrderReferenceId, ExternalReference = x.ExternalReference,
                OrderDescription = x.OrderDescription, Action = x.Action, ActionReason = x.ActionReason,
                ActionByUserId = x.ActionByUserId, ActionAt = x.ActionAt,
                ToServiceUnitId = x.ToServiceUnitId, ToServiceUnitName = x.ToServiceUnit?.ServiceUnitName,
                AcceptanceStatus = x.AcceptanceStatus, AcceptedByUserId = x.AcceptedByUserId,
                AcceptedAt = x.AcceptedAt, RejectionReason = x.RejectionReason,
                IsEffective = x.IsEffective, SupersedesOrderItemId = x.SupersedesOrderItemId,
                IsActionSetManually = x.OrderKind == EmergencyOrderKind.LaboratoryOrder };

        private static string? Rapikan(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static Guid? KosongKeNull(Guid? value)
            => value.HasValue && value.Value != Guid.Empty ? value : null;

        public string GenerateNumber(DateTime now)
            => _documentNumberService.Generate("DEP", now);
    }
}
