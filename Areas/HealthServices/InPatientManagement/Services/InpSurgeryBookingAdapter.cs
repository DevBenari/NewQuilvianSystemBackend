using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pemesanan ruang bedah dari bangsal (<c>BE-RWI-175</c>, <c>INV-RWF-25</c>, <c>RWI-DEC-175</c>,
    /// <c>RWI-DEC-176</c>). Adapter memvalidasi episode dan order tindakan, lalu memanggil
    /// <see cref="OperatingRoomCaseService.CreateFromWardBookingAsync"/> dalam proses yang sama;
    /// transaksi pembuatan kasus tetap milik Kamar Operasi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Aturan "tepat satu order" dijaga di sini, bukan dengan mempersempit <c>POST cases</c> OK.</b>
    /// Petugas OK tetap dapat membuat kasus dengan banyak tindakan; pesanan bangsal selalu merujuk
    /// satu order tindakan operasi aktif milik kunjungan episode.
    /// </para>
    /// <para>
    /// <b>Tab Obgyn memaksa jenis layanan <c>Obstetric</c>.</b> Contoh: Ns. Siti membuka tab Bedah
    /// Obgyn untuk Ny. Ani dan memesan "Sectio Caesarea" → kasus OK berjenis Obstetri, berstatus
    /// Diminta, dan isiannya tidak dapat diganti menjadi bedah umum dari tab itu.
    /// </para>
    /// </remarks>
    public sealed class InpSurgeryBookingAdapter
    {
        /// <summary>422 — order tidak ada, tidak aktif, atau bukan milik kunjungan episode ini.</summary>
        public const string NoActiveOrderCode = "INP-SRG-001";

        /// <summary>422 — episode bukan <c>Admitted</c>.</summary>
        public const string EpisodeNotAdmittedCode = "INP-SRG-002";

        /// <summary>Awalan kunci idempotensi supaya tidak bertabrakan dengan kunci layar OK.</summary>
        private const string IdempotencyPrefix = "INP-SRG:";

        private const int MaxIdempotencyKeyLength = 80;

        private static readonly TimeSpan PreferredAtPastTolerance = TimeSpan.FromMinutes(15);

        private static readonly string[] AllowedLaterality = ["Left", "Right", "Bilateral", "NotApplicable"];

        private static readonly PatientProcedureStatus[] BookableProcedureStatuses =
            [PatientProcedureStatus.Planned, PatientProcedureStatus.Ordered];

        private readonly ApplicationDbContext _dbContext;
        private readonly OperatingRoomCaseService _caseService;

        public InpSurgeryBookingAdapter(ApplicationDbContext dbContext, OperatingRoomCaseService caseService)
        {
            _dbContext = dbContext;
            _caseService = caseService;
        }

        public async Task<InpSurgeryBookingResult> BookAsync(
            Guid episodeId,
            SurgeryBookingRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var validation = Validate(request, idempotencyKey, out var parsed);
            if (validation != null)
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.Invalid, validation);

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.PatientId, x.EncounterId, x.EpisodeStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (episode == null)
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.NotFound, "Episode rawat inap tidak ditemukan.");

            if (episode.EpisodeStatus != InpEpisodeStatus.Admitted)
            {
                // VAL-RWF-71 (disahkan RWI-DEC-220 butir 2).
                return InpSurgeryBookingResult.Fail(
                    InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Pemesanan ruang bedah hanya untuk pasien yang sedang dirawat",
                    EpisodeNotAdmittedCode);
            }

            var procedure = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x => x.Id == request.PatientProcedureId)
                .Select(x => new
                {
                    x.Id, x.EncounterId, x.PatientId, x.DoctorId, x.InstructingDoctorId,
                    x.IsSurgeryRelated, x.IsActive, x.IsDelete, x.IsCancel, x.ProcedureStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (procedure == null)
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.NotFound, "Order tindakan tidak ditemukan.");

            var isActiveSurgeryOrderOfEpisode =
                !procedure.IsDelete && !procedure.IsCancel && procedure.IsActive && procedure.IsSurgeryRelated &&
                BookableProcedureStatuses.Contains(procedure.ProcedureStatus) &&
                procedure.EncounterId == episode.EncounterId && procedure.PatientId == episode.PatientId;

            if (!isActiveSurgeryOrderOfEpisode)
            {
                return InpSurgeryBookingResult.Fail(
                    InpEpisodeOperationStatus.BusinessRuleRejected,
                    "Tindakan operasi belum dipesan dokter.",
                    NoActiveOrderCode);
            }

            var caseRequest = new CreateOprCaseRequest
            {
                PatientId = episode.PatientId,
                EncounterId = episode.EncounterId,
                // Dokter pemesan = dokter pemberi instruksi order (perawat menginput atas instruksi),
                // atau dokter order bila dokter memesan sendiri. Operator = dokter order.
                RequesterDoctorId = procedure.InstructingDoctorId ?? procedure.DoctorId,
                PrimarySurgeonId = procedure.DoctorId,
                CaseType = parsed.CaseType,
                Priority = parsed.Priority,
                Indication = request.Indication.Trim(),
                Laterality = parsed.Laterality,
                EstimatedMinutes = request.EstimatedMinutes,
                PreferredAt = parsed.PreferredAtUtc,
                Procedures = [new OprCaseProcedureRequest { PatientProcedureId = procedure.Id, IsPrimary = true }],
                IdempotencyKey = IdempotencyPrefix + parsed.IdempotencyKey,
                ExpectedVersion = 0,
                SurgicalServiceType = parsed.SurgicalServiceType,
                PlannedAnesthesiaType = parsed.PlannedAnesthesiaType
            };

            try
            {
                var created = await _caseService.CreateFromWardBookingAsync(caseRequest, request.Note, cancellationToken);

                return InpSurgeryBookingResult.Ok(created, "Pesanan ruang bedah berhasil dikirim ke Kamar Operasi.");
            }
            catch (OperatingRoomConflictException ex)
            {
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.Conflict, ex.Message, ex.Code);
            }
            catch (OperatingRoomUnprocessableException ex)
            {
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.BusinessRuleRejected, ex.Message, ex.Code);
            }
            catch (OperatingRoomForbiddenException ex)
            {
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.NotFound, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return InpSurgeryBookingResult.Fail(InpEpisodeOperationStatus.Invalid, ex.Message);
            }
        }

        private static string? Validate(SurgeryBookingRequest request, string? idempotencyKey, out ParsedBooking parsed)
        {
            parsed = default;

            if (request == null)
                return "Isian pemesanan ruang bedah wajib diisi.";

            var key = idempotencyKey?.Trim();
            if (string.IsNullOrWhiteSpace(key))
                return "Header Idempotency-Key wajib diisi.";
            if (key.Length > MaxIdempotencyKeyLength)
                return $"Idempotency-Key maksimal {MaxIdempotencyKeyLength} karakter.";

            OprSurgicalServiceType serviceType;
            if (string.Equals(request.BookingTab?.Trim(), SurgeryBookingTabs.Surgery, StringComparison.OrdinalIgnoreCase))
                serviceType = OprSurgicalServiceType.General;
            else if (string.Equals(request.BookingTab?.Trim(), SurgeryBookingTabs.Obstetric, StringComparison.OrdinalIgnoreCase))
                serviceType = OprSurgicalServiceType.Obstetric;
            else
                return "Tab pemesanan harus Surgery atau Obstetric.";

            if (request.PatientProcedureId == Guid.Empty)
                return "Order tindakan operasi wajib dipilih.";

            if (!request.PreferredAt.HasValue)
                return "Waktu yang diinginkan wajib diisi.";
            var preferredAtUtc = request.PreferredAt.Value.ToUniversalTime();
            // VAL-RWF-73.
            if (preferredAtUtc < DateTime.UtcNow - PreferredAtPastTolerance)
                return "Tanggal dan jam operasi yang diinginkan sudah lewat";

            if (!TryParseEnum<OprPlannedAnesthesiaType>(request.PlannedAnesthesiaType, out var anesthesia))
                return "Rencana jenis anestesi harus General, Regional, Local, atau Sedation.";
            if (!TryParseEnum<OprPriority>(request.Priority, out var priority))
                return "Prioritas harus Routine, Urgent, atau Emergency.";
            if (!TryParseEnum<OprCaseType>(request.CaseType, out var caseType))
                return "Jenis kasus harus Elective atau Emergency.";

            if (string.IsNullOrWhiteSpace(request.Indication))
                return "Indikasi operasi wajib diisi.";
            if (request.Indication.Trim().Length > 4000)
                return "Indikasi operasi maksimal 4000 karakter.";

            string? laterality = null;
            if (!string.IsNullOrWhiteSpace(request.Laterality))
            {
                laterality = AllowedLaterality.FirstOrDefault(
                    x => string.Equals(x, request.Laterality.Trim(), StringComparison.OrdinalIgnoreCase));
                if (laterality == null)
                    return "Sisi operasi harus Left, Right, Bilateral, atau NotApplicable.";
            }

            if (request.EstimatedMinutes is < 1 or > 1440)
                return "Perkiraan durasi harus antara 1 dan 1440 menit.";

            if (request.Note != null && request.Note.Trim().Length > 1000)
                return "Catatan maksimal 1000 karakter.";

            parsed = new ParsedBooking(key, serviceType, anesthesia, priority, caseType, laterality, preferredAtUtc);
            return null;
        }

        /// <summary>Hanya nama nilai yang diterima, bukan angka, supaya "3" tidak lolos sebagai pilihan.</summary>
        private static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value) || value.Trim().All(char.IsDigit))
                return false;

            return Enum.TryParse(value.Trim(), ignoreCase: true, out result) && Enum.IsDefined(result);
        }

        private readonly record struct ParsedBooking(
            string IdempotencyKey,
            OprSurgicalServiceType SurgicalServiceType,
            OprPlannedAnesthesiaType PlannedAnesthesiaType,
            OprPriority Priority,
            OprCaseType CaseType,
            string? Laterality,
            DateTime PreferredAtUtc);
    }

    /// <summary>Hasil pemesanan ruang bedah dari bangsal.</summary>
    public sealed class InpSurgeryBookingResult
    {
        private InpSurgeryBookingResult(
            InpEpisodeOperationStatus status,
            OprCaseDetailResponse? surgeryCase,
            string message,
            string? code)
        {
            Status = status;
            Case = surgeryCase;
            Message = message;
            Code = code;
        }

        public InpEpisodeOperationStatus Status { get; }

        /// <summary>Kasus OK yang terbentuk (atau yang sudah ada pada permintaan ulang berkunci sama).</summary>
        public OprCaseDetailResponse? Case { get; }

        public string Message { get; }

        /// <summary>Kode kontrak, misalnya <c>INP-SRG-001</c> atau kode OK <c>OPR013</c>.</summary>
        public string? Code { get; }

        public static InpSurgeryBookingResult Ok(OprCaseDetailResponse surgeryCase, string message)
            => new(InpEpisodeOperationStatus.Success, surgeryCase, message, null);

        public static InpSurgeryBookingResult Fail(InpEpisodeOperationStatus status, string message, string? code = null)
            => new(status, null, message, code);
    }
}
