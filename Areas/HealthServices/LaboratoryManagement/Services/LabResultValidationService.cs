using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Validasi, rilis, dan <i>Kembalikan ke analis</i> atas hasil Patologi Klinik (<c>S4</c>,
    /// <c>02-backend-architecture.md</c> 20.4). Dipisah dari <see cref="LabExaminationService"/>
    /// karena ketiga tindakan ini punya pelaku, izin, dan integrasi yang berbeda dari penulisan
    /// hasil. <see cref="LabExaminationService"/> tetap memegang penulisan hasil, Final, dan Reopen.
    ///
    /// <para>
    /// <b>Kewenangan berlapis dua</b> (<c>LAB-DEC-142</c>). Lapis jabatan — aksi <c>Validate</c>
    /// atau <c>Release</c> pada resource <c>LabExaminationResult</c> — ditegakkan filter hak akses
    /// sebelum service dipanggil. Lapis orang — penunjukan pada kredensial Human Resource —
    /// ditegakkan di sini lewat <see cref="LabClinicalPrivilegeResolver"/>.
    /// </para>
    ///
    /// <para>
    /// <b>Satu <c>SaveChangesAsync</c> per tindakan</b> sudah atomik (20.1): kolom tindakan, baris
    /// riwayat, dan — saat rilis — dokumen rekam medis tersimpan bersama atau tidak sama sekali.
    /// <c>Version</c> naik pada setiap tindakan, sehingga dua tindakan pada detik yang sama tidak
    /// dapat sama-sama berhasil.
    /// </para>
    /// </summary>
    public class LabResultValidationService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LabClinicalPrivilegeResolver _privilegeResolver;
        private readonly ClinicalDocumentIntegrityService _integrityService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabResultValidationService(
            ApplicationDbContext dbContext,
            LabClinicalPrivilegeResolver privilegeResolver,
            ClinicalDocumentIntegrityService integrityService,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _privilegeResolver = privilegeResolver;
            _integrityService = integrityService;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Menyatakan angka hasil Patologi Klinik yang sudah Final <b>benar</b>
        /// (<c>LAB-API-v1</c> <c>r34</c> 29.2; <c>LAB-STATE-v1</c> <c>r5</c> 7.2).
        ///
        /// <para>
        /// <b>Urutan pemeriksaan disengaja</b> (20.4, <c>LAB-VAL-v1</c> <c>r12</c> 14.1):
        /// (1) Patologi Klinik — <c>VAL-126</c>; (2) tidak batal atau gugur — <c>VAL-127</c>;
        /// (3) keadaan hasil — <c>VAL-124</c>, <c>VAL-125</c>; (4) lapis orang — <c>VAL-128</c>;
        /// (5) empat mata — <c>VAL-130</c>, <c>VAL-129</c>, <c>VAL-132</c>; (6) tulis. Lapis orang
        /// sengaja <b>sesudah</b> keadaan hasil: dokter yang menekan Validasi pada hasil Draft
        /// membaca sebab yang sebenarnya — <i>belum Final</i> — bukan penolakan kewenangan.
        /// </para>
        /// </summary>
        /// <exception cref="KeyNotFoundException">Pemeriksaan tidak ditemukan — <c>404</c>.</exception>
        /// <exception cref="LabExaminationValidationException"><c>422</c>.</exception>
        /// <exception cref="LabExaminationConflictException">Sudah divalidasi, atau bentrok <c>Version</c> — <c>409</c>.</exception>
        /// <exception cref="LabExaminationForbiddenException">Tidak ditunjuk — <c>403</c> dengan sebabnya.</exception>
        /// <exception cref="LabPrivilegeReadException">Data kewenangan tidak terbaca — <c>503</c>; nol yang tersimpan.</exception>
        public async Task<LabExaminationCompletionResponse> ValidateAsync(
            Guid id,
            LabResultSignOffRequest? request,
            CancellationToken cancellationToken = default)
        {
            var examination = await LoadAsync(id, cancellationToken);

            // (1) VAL-126 dan (2) VAL-127.
            EnsureClinicalPathologyAndRunning(examination);

            // (3) VAL-124 lalu VAL-125.
            if (examination.FinalizedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil ini belum dinyatakan selesai oleh analis, jadi belum dapat divalidasi.");
            }

            if (examination.ValidatedAt is not null)
            {
                throw new LabExaminationConflictException("Hasil ini sudah divalidasi.");
            }

            // (4) VAL-128 — lapis orang. Galat pembacaan dibiarkan naik sebagai
            // LabPrivilegeReadException: controller menjawab 503, dan nol yang tersimpan.
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var privilege = await EnsureAppointedAsync(actorUserId, LabPrivilegeKind.Validation, now, cancellationToken);

            // (5) Empat mata.
            // VAL-130. Tanpa pengisi yang tercatat, "bukan orang yang sama" tidak dapat dibuktikan.
            if (examination.ResultEnteredByUserId is null)
            {
                throw new LabExaminationValidationException(
                    "Pengisi hasil ini tidak tercatat, sehingga prinsip empat mata tidak dapat diperiksa. Minta analis membuka kembali lalu menyimpan ulang hasilnya.");
            }

            var (reason, note) = await ResolveFourEyesExceptionAsync(
                merangkap: examination.ResultEnteredByUserId == actorUserId,
                request,
                // VAL-129.
                "Anda yang mengisi hasil ini. Validasi oleh orang yang sama memerlukan alasan pengecualian.",
                cancellationToken);

            // Snapshot jabatan dari penempatan yang benar-benar memegang aksi Validate (AC-239).
            var acting = await _privilegeResolver.ResolveActingPositionAsync(
                actorUserId,
                LabPrivilegeKind.Validation,
                cancellationToken);

            // (6) Tulis.
            examination.ValidatedAt = now;
            examination.ValidatedByUserId = actorUserId;
            examination.ValidatedByPositionId = acting?.PositionId;
            examination.ValidatedByPositionNameSnapshot = acting?.PositionName;
            examination.ValidatedByPrivilegeId = privilege.PrivilegeId;
            examination.ValidationExceptionReasonId = reason?.Id;
            examination.ValidationExceptionReasonNameSnapshot = reason?.ReasonName;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;
            examination.Version += 1;

            // Hanya bila merangkap: kode alasan pengecualian dan catatannya (LAB-PERM-v1 rev 11
            // 13.6). Catatan disimpan di riwayat, tidak tercetak.
            AddHistory(examination, "LabExamination.ValidateResult", "Finalized", "Validated",
                reason?.ReasonCode, reason is null ? null : note, actorUserId, now);

            await SaveAsync(cancellationToken);

            // Payload log sengaja tanpa nilai hasil, catatan pengecualian, dan nama pasien
            // (LAB-PERM-v1 rev 11 13.6).
            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.ValidateResult",
                "Hasil Patologi Klinik divalidasi.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    examination.ValidatedAt,
                    examination.ValidatedByPrivilegeId,
                    IsFourEyesException = reason is not null,
                    ExceptionReasonCode = reason?.ReasonCode
                });

            var names = await ReadNamesAsync(new[] { actorUserId }, cancellationToken);

            return LabExaminationService.BuildCompletionResponse(examination, names.GetValueOrDefault(actorUserId));
        }

        /// <summary>
        /// Merilis hasil Patologi Klinik yang sudah divalidasi, sehingga menjadi dokumen klinis
        /// pasien — <b>sekaligus</b> mendaftarkannya ke rekam medis sebagai dokumen tertanda
        /// tangan dan terkunci (<c>r34</c> 29.2, 29.7; <c>INT-08</c>; <c>LAB-DEC-017</c>).
        ///
        /// <para>
        /// <b>Atomik.</b> Kolom rilis, baris riwayat, dan baris <c>MrcClinicalDocumentIntegrity</c>
        /// tersimpan dalam <b>satu</b> <c>SaveChangesAsync</c>. Pendaftaran dipanggil <b>sebelum</b>
        /// satu pun kolom rilis diubah dan tidak menyimpan sendiri, sehingga pendaftaran yang gagal
        /// meninggalkan <c>DbContext</c> bersih: nol <c>ReleasedAt</c>, nol riwayat, nol dokumen, dan
        /// hasil tetap tervalidasi (<c>VAL-137</c>). Pola ini menyalin
        /// <c>ConsultationFinalizationService</c> milik Farmasi.
        /// </para>
        ///
        /// <para>
        /// <b>Pasien diambil dari kunjungan order</b> (<c>LabOrder.EncounterId</c> →
        /// <c>RegPatientEncounter.PatientId</c>), bukan dari sumber lain. Perilis tercatat sebagai
        /// penulis sekaligus penanda tangan dokumen (20.10 butir 6, pilihan A).
        /// </para>
        /// </summary>
        /// <param name="id">Pemeriksaan yang dirilis.</param>
        /// <param name="request">Alasan pengecualian bila perilis juga pemvalidasi.</param>
        /// <param name="deviceInfo">Perangkat pemanggil, dari permintaan HTTP — dicatat pada tanda tangan.</param>
        /// <param name="ipAddress">Alamat jaringan pemanggil, dari permintaan HTTP.</param>
        /// <param name="cancellationToken">Pembatalan permintaan.</param>
        /// <exception cref="KeyNotFoundException">Pemeriksaan tidak ditemukan — <c>404</c>.</exception>
        /// <exception cref="LabExaminationValidationException"><c>422</c>, termasuk <c>VAL-137</c>.</exception>
        /// <exception cref="LabExaminationConflictException">Sudah dirilis, atau bentrok — <c>409</c>.</exception>
        /// <exception cref="LabExaminationForbiddenException">Tidak ditunjuk — <c>403</c> dengan sebabnya.</exception>
        /// <exception cref="LabPrivilegeReadException">Data kewenangan tidak terbaca — <c>503</c>; nol yang tersimpan.</exception>
        public async Task<LabExaminationCompletionResponse> ReleaseAsync(
            Guid id,
            LabResultSignOffRequest? request,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            var examination = await LoadAsync(id, cancellationToken);

            // (1) VAL-126 dan (2) VAL-127.
            EnsureClinicalPathologyAndRunning(examination);

            // (3) VAL-133 — belum divalidasi, lalu sudah dirilis.
            if (examination.ValidatedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil ini belum divalidasi, jadi belum dapat dirilis.");
            }

            if (examination.ReleasedAt is not null)
            {
                throw new LabExaminationConflictException("Hasil ini sudah dirilis.");
            }

            // (4) VAL-128 — lapis orang, dengan kode RILIS. Pemegang kode validasi saja ditolak
            // (AC-217, AC-231).
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var privilege = await EnsureAppointedAsync(actorUserId, LabPrivilegeKind.Release, now, cancellationToken);

            // (5) Empat mata — VAL-131, VAL-132.
            var (reason, note) = await ResolveFourEyesExceptionAsync(
                merangkap: examination.ValidatedByUserId == actorUserId,
                request,
                "Anda yang memvalidasi hasil ini. Rilis oleh orang yang sama memerlukan alasan pengecualian.",
                cancellationToken);

            var acting = await _privilegeResolver.ResolveActingPositionAsync(
                actorUserId,
                LabPrivilegeKind.Release,
                cancellationToken);

            // (6) Pendaftaran rekam medis — SEBELUM kolom rilis diubah. Pasien dari kunjungan order.
            var patientId = await _dbContext.RegPatientEncounters
                .AsNoTracking()
                .Where(x => x.Id == examination.LabOrder!.EncounterId)
                .Select(x => (Guid?)x.PatientId)
                .FirstOrDefaultAsync(cancellationToken);

            try
            {
                await _integrityService.RegisterSignedAsync(
                    ClinicalDocumentKind.LaboratoryResult,
                    examination.Id,
                    patientId ?? Guid.Empty,
                    examination.LabOrder!.EncounterId,
                    actorUserId,
                    deviceInfo,
                    ipAddress,
                    now,
                    cancellationToken);
            }
            catch (InvalidOperationException pendaftaranGagal)
            {
                // VAL-137. Dijawab sebagai penolakan yang terbaca, bukan galat sistem. Belum ada
                // satu pun perubahan yang dibuat, jadi hasil tetap tervalidasi.
                throw new LabExaminationValidationException(
                    $"Hasil tidak dapat dirilis karena pendaftaran ke rekam medis gagal: {pendaftaranGagal.Message} Hasil tetap tervalidasi.");
            }

            // (7) Tulis.
            examination.ReleasedAt = now;
            examination.ReleasedByUserId = actorUserId;
            examination.ReleasedByPositionId = acting?.PositionId;
            examination.ReleasedByPositionNameSnapshot = acting?.PositionName;
            examination.ReleasedByPrivilegeId = privilege.PrivilegeId;
            examination.ReleaseExceptionReasonId = reason?.Id;
            examination.ReleaseExceptionReasonNameSnapshot = reason?.ReasonName;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;
            examination.Version += 1;

            AddHistory(examination, "LabExamination.ReleaseResult", "Validated", "Released",
                reason?.ReasonCode, reason is null ? null : note, actorUserId, now);

            // Satu SaveChangesAsync: rilis, riwayat, dan dokumen rekam medis berhasil bersama atau
            // gagal bersama. Rilis ganda bersamaan ditolak token Version atau index unik
            // (DocumentKind, DocumentId) milik Rekam Medis — keduanya dijawab 409.
            await SaveAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.ReleaseResult",
                "Hasil Patologi Klinik dirilis dan didaftarkan ke rekam medis.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    examination.ReleasedAt,
                    examination.ReleasedByPrivilegeId,
                    IsFourEyesException = reason is not null,
                    ExceptionReasonCode = reason?.ReasonCode
                });

            var names = await ReadNamesAsync(
                new[] { actorUserId, examination.ValidatedByUserId ?? Guid.Empty },
                cancellationToken);

            return LabExaminationService.BuildCompletionResponse(
                examination,
                examination.ValidatedByUserId is Guid validator ? names.GetValueOrDefault(validator) : null,
                names.GetValueOrDefault(actorUserId));
        }

        /// <summary>
        /// <i>Kembalikan ke analis</i> — hasil tervalidasi yang ternyata keliru <b>sebelum</b>
        /// dirilis kembali menjadi Draft, beralasan (<c>LAB-DEC-138</c>; <c>r34</c> 29.2;
        /// <c>LAB-STATE-v1</c> <c>r5</c> 7.2).
        ///
        /// <para>
        /// <b>Jejak validasi tidak dihapus.</b> Kolom validasi dan <c>FinalizedAt</c> pada baris
        /// pemeriksaan dikosongkan — hasil kembali di tangan analis — tetapi baris riwayat
        /// <c>ValidateResult</c> sebelumnya <b>tidak disentuh</b>. Satu baris baru
        /// <c>ReturnResultToAnalyst</c> mencatat siapa, kapan, dan kode alasannya (<c>INV-47</c>,
        /// <c>AC-206</c>).
        /// </para>
        ///
        /// <para>
        /// <c>ReopenCount</c> <b>tidak</b> naik: yang menghitung Reopen adalah analis yang membuka
        /// sendiri; pengembalian dihitung dari riwayatnya. Tidak ada pemberitahuan dan tidak ada
        /// versi bernomor.
        /// </para>
        ///
        /// <para>
        /// <b>Urutan</b> (14.1): <c>VAL-126</c>, <c>VAL-127</c>, <c>VAL-134</c>, lalu lapis orang
        /// <c>VAL-138</c> — pemegang kode validasi <b>atau</b> rilis — lalu alasan <c>VAL-135</c>.
        /// </para>
        /// </summary>
        /// <exception cref="KeyNotFoundException">Pemeriksaan tidak ditemukan — <c>404</c>.</exception>
        /// <exception cref="LabExaminationValidationException"><c>422</c>.</exception>
        /// <exception cref="LabExaminationConflictException">Sudah dirilis, atau bentrok — <c>409</c>.</exception>
        /// <exception cref="LabExaminationForbiddenException">Bukan pemegang kode validasi maupun rilis — <c>403</c>.</exception>
        /// <exception cref="LabPrivilegeReadException">Data kewenangan tidak terbaca — <c>503</c>; nol yang tersimpan.</exception>
        public async Task<LabExaminationCompletionResponse> ReturnToAnalystAsync(
            Guid id,
            LabResultReturnRequest? request,
            CancellationToken cancellationToken = default)
        {
            var examination = await LoadAsync(id, cancellationToken);

            // VAL-126 dan VAL-127.
            EnsureClinicalPathologyAndRunning(examination);

            // VAL-134 — belum divalidasi, lalu sudah dirilis.
            if (examination.ValidatedAt is null)
            {
                throw new LabExaminationValidationException(
                    "Hasil ini belum divalidasi; analis dapat membukanya kembali sendiri.");
            }

            if (examination.ReleasedAt is not null)
            {
                throw new LabExaminationConflictException(
                    "Hasil yang sudah dirilis hanya dapat diubah lewat koreksi.");
            }

            // VAL-138 — lapis orang. Satu atribut hak akses tidak dapat menyatakan "atau" atas dua
            // penunjukan, jadi service yang memeriksanya: kode validasi lebih dulu, lalu kode rilis.
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var bolehValidasi = await _privilegeResolver.ResolveAsync(
                actorUserId, LabDiscipline.ClinicalPathology, LabPrivilegeKind.Validation, now, cancellationToken);

            if (!bolehValidasi.IsGranted)
            {
                var bolehRilis = await _privilegeResolver.ResolveAsync(
                    actorUserId, LabDiscipline.ClinicalPathology, LabPrivilegeKind.Release, now, cancellationToken);

                if (!bolehRilis.IsGranted)
                {
                    throw new LabExaminationForbiddenException(
                        "Anda bukan pemegang kewenangan validasi atau rilis Patologi Klinik.");
                }
            }

            // VAL-135 — alasan wajib, aktif; catatan bila diwajibkan, maksimal 500.
            if (request?.CorrectionReasonId is not Guid reasonId)
            {
                throw new LabExaminationValidationException("Alasan pengembalian wajib dipilih dari daftar.");
            }

            var reason = await _dbContext.LabResultCorrectionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == reasonId && !x.IsDelete && x.IsActive, cancellationToken);

            if (reason is null)
            {
                throw new LabExaminationValidationException("Alasan pengembalian wajib dipilih dari daftar.");
            }

            var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            if (reason.RequiresNote && note is null)
            {
                throw new LabExaminationValidationException("Alasan ini mewajibkan catatan.");
            }

            if (note is not null && note.Length > 500)
            {
                throw new LabExaminationValidationException("Catatan paling panjang 500 karakter.");
            }

            // Tulis. Hasil kembali Draft: Final dan seluruh kolom validasi dikosongkan, ReopenCount tetap.
            examination.FinalizedAt = null;
            examination.FinalizedByUserId = null;
            examination.ValidatedAt = null;
            examination.ValidatedByUserId = null;
            examination.ValidatedByPositionId = null;
            examination.ValidatedByPositionNameSnapshot = null;
            examination.ValidatedByPrivilegeId = null;
            examination.ValidationExceptionReasonId = null;
            examination.ValidationExceptionReasonNameSnapshot = null;

            examination.UpdateDateTime = now;
            examination.UpdateBy = actorUserId;
            examination.Version += 1;

            AddHistory(examination, "LabExamination.ReturnResultToAnalyst", "Validated", "Draft",
                reason.ReasonCode, note, actorUserId, now);

            await SaveAsync(cancellationToken);

            // Payload log tanpa catatan pengembalian, nilai hasil, dan nama pasien (13.6). Kode
            // alasan ikut — ia kunci laporan mutu, bukan data sensitif.
            await _loggerService.AuditAsync(
                LogCategory,
                "LabExamination.ReturnResultToAnalyst",
                "Hasil tervalidasi dikembalikan kepada analis sebelum dirilis.",
                new
                {
                    examination.Id,
                    examination.LabOrderId,
                    ReturnedAt = now,
                    CorrectionReasonCode = reason.ReasonCode
                });

            return LabExaminationService.BuildCompletionResponse(examination);
        }

        // LabOrder WAJIB ikut dimuat: disiplin dan status order dibaca darinya, dan baris riwayat
        // serta dokumen rekam medis menyimpan EncounterId yang ber-foreign key (pelajaran Reopen
        // BE-LAB-54).
        private async Task<LabExamination> LoadAsync(Guid id, CancellationToken cancellationToken) =>
            await _dbContext.LabExaminations
                .Include(x => x.Procedure)
                .Include(x => x.LabOrder)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pemeriksaan tidak ditemukan.");

        /// <summary>
        /// <c>VAL-126</c> lalu <c>VAL-127</c> — sama bagi validasi dan rilis. Disiplin yang diterima
        /// dibaca dari <see cref="LabReleasableDisciplines"/> (<c>BE-LAB-82</c>), satu jawaban dengan
        /// laporan operasional. Disiplin kosong — order dan katalog sama-sama tanpa disiplin — tetap
        /// ditolak.
        /// </summary>
        private static void EnsureClinicalPathologyAndRunning(LabExamination examination)
        {
            if (LabExaminationService.ResolveDiscipline(examination) is not LabDiscipline discipline ||
                !LabReleasableDisciplines.Contains(discipline))
            {
                throw new LabExaminationValidationException(
                    "Validasi dan rilis hasil Mikrobiologi serta Patologi Anatomi belum tersedia.");
            }

            // INV-51.
            if (examination.ExaminationStatus is LabExaminationStatus.Voided or LabExaminationStatus.Cancelled ||
                examination.LabOrder?.OrderStatus == LabOrderStatus.Cancelled)
            {
                throw new LabExaminationValidationException(
                    "Pemeriksaan ini sudah dibatalkan atau gugur, sehingga tidak dapat divalidasi maupun dirilis.");
            }
        }

        /// <summary><c>VAL-128</c> — lapis orang. Penolakan menjadi <c>403</c> dengan sebabnya.</summary>
        private async Task<LabPrivilegeCheck> EnsureAppointedAsync(
            Guid actorUserId,
            LabPrivilegeKind kind,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var privilege = await _privilegeResolver.ResolveAsync(
                actorUserId,
                LabDiscipline.ClinicalPathology,
                kind,
                now,
                cancellationToken);

            if (!privilege.IsGranted)
            {
                throw new LabExaminationForbiddenException(privilege.Message!);
            }

            return privilege;
        }

        /// <summary>
        /// Empat mata — satu aturan bagi validasi (<c>VAL-129</c>) dan rilis (<c>VAL-131</c>),
        /// ditambah <c>VAL-132</c>. Pelaku yang merangkap wajib membawa alasan <b>aktif</b>, dan
        /// catatan bila alasannya mewajibkan. Pelaku yang <b>tidak</b> merangkap tidak boleh
        /// membawa alasan maupun catatan — penanda pengecualian tercetak dan dibaca dokter pemesan.
        /// </summary>
        private async Task<(LabFourEyesExceptionReason? Reason, string? Note)> ResolveFourEyesExceptionAsync(
            bool merangkap,
            LabResultSignOffRequest? request,
            string pesanTanpaAlasan,
            CancellationToken cancellationToken)
        {
            var reasonId = request?.ExceptionReasonId;
            var note = string.IsNullOrWhiteSpace(request?.ExceptionNote) ? null : request.ExceptionNote.Trim();

            if (!merangkap)
            {
                if (reasonId is not null || note is not null)
                {
                    throw new LabExaminationValidationException(
                        "Alasan pengecualian hanya diisi bila Anda merangkap peran pada hasil ini.");
                }

                return (null, null);
            }

            if (reasonId is null)
            {
                throw new LabExaminationValidationException(pesanTanpaAlasan);
            }

            var reason = await _dbContext.LabFourEyesExceptionReasons
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == reasonId.Value && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (reason is null)
            {
                throw new LabExaminationValidationException(
                    "Alasan pengecualian tidak ditemukan atau sudah nonaktif.");
            }

            if (reason.RequiresNote && note is null)
            {
                throw new LabExaminationValidationException("Alasan ini mewajibkan catatan.");
            }

            if (note is not null && note.Length > 500)
            {
                throw new LabExaminationValidationException("Catatan paling panjang 500 karakter.");
            }

            return (reason, note);
        }

        /// <summary>
        /// Satu baris riwayat per tindakan. Baris lama <b>tidak pernah diubah</b> — itulah yang
        /// membuat validasi yang dibatalkan tetap terbaca (<c>INV-47</c>, <c>AC-206</c>).
        /// </summary>
        private void AddHistory(
            LabExamination examination,
            string action,
            string fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime now)
        {
            _dbContext.LabTransitionHistories.Add(new LabTransitionHistory
            {
                LabOrderId = examination.LabOrderId,
                LabExaminationId = examination.Id,
                EncounterId = examination.LabOrder!.EncounterId,
                Scope = LabTransitionScope.LabExamination,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = now
            });
        }

        /// <summary>
        /// Bentrok token <c>Version</c>, atau pelanggaran index unik — misalnya dua rilis bersamaan
        /// yang sama-sama mendaftarkan dokumen rekam medis — dijawab <c>409</c>, bukan <c>500</c>.
        /// </summary>
        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new LabExaminationConflictException(LabExaminationService.ResultConcurrencyMessage);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            {
                throw new LabExaminationConflictException(LabExaminationService.ResultConcurrencyMessage);
            }
        }

        /// <summary>Nama pelaku, satu kueri untuk seluruh pengguna yang diminta.</summary>
        private async Task<Dictionary<Guid, string?>> ReadNamesAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken)
        {
            var ids = userIds.Where(x => x != Guid.Empty).Distinct().ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
