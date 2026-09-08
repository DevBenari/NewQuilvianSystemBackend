using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Hasil satu perintah pada catatan tindakan keperawatan.
    /// </summary>
    public sealed class NursingInterventionResult
    {
        public bool IsSuccess { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public CliNursingIntervention? Intervention { get; init; }

        /// <summary>
        /// Benar ketika catatan yang dikembalikan sudah ada sebelumnya dan permintaan ini adalah
        /// kiriman ulang dengan kunci yang sama - <c>VAL-KEP-15</c>.
        /// </summary>
        public bool IsReplay { get; init; }

        internal static NursingInterventionResult Ok(
            CliNursingIntervention intervention,
            bool isReplay = false,
            int? statusCode = null) => new()
            {
                IsSuccess = true,
                StatusCode = statusCode
                    ?? (isReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created),
                Intervention = intervention,
                IsReplay = isReplay
            };

        internal static NursingInterventionResult Fail(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Pemilik seluruh perintah dan pembacaan catatan tindakan keperawatan - <c>CAP-014</c>,
    /// <c>BE-RWI-061</c>, <c>BE-RWI-062</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua mesin status yang tidak boleh saling mengunci.</b> Keadaan klinis catatan
    /// (<c>Recorded</c>, <c>Finalized</c>) dan keadaan pengiriman tagihan
    /// (<c>NotApplicable</c>, <c>Pending</c>, <c>Dispatched</c>, <c>Failed</c>) hidup
    /// berdampingan. Kegagalan sistem tagihan <b>tidak pernah</b> membatalkan catatan klinis -
    /// <c>AC-CAP014-02</c>.
    /// </para>
    /// <para>
    /// <b>Idempotency berlapis dua.</b> Pemeriksaan kunci di sini menangani kiriman ulang biasa;
    /// unique parsial pada database menangani dua permintaan yang tiba benar-benar bersamaan.
    /// Ketika penjaga database yang menolak, service ini membaca ulang barisnya lalu menjawab
    /// seolah kiriman ulang biasa - <c>200</c> beserta baris yang sudah ada, bukan <c>409</c>.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class NursingInterventionService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        /// <summary>Kalimat penolakan <c>VAL-KEP-13</c>, apa adanya seperti pada validation matrix.</summary>
        public const string PenolakanWaktuMasaDepan =
            "Waktu tindakan tidak boleh melewati waktu sekarang.";

        /// <summary>Kalimat penolakan <c>VAL-KEP-14</c>, apa adanya seperti pada validation matrix.</summary>
        public const string PenolakanSebelumMasukKamar =
            "Waktu tindakan sebelum pasien masuk kamar. Periksa kembali waktunya.";

        /// <summary>
        /// Toleransi selisih jam antar-mesin, supaya perbedaan beberapa detik tidak terbaca
        /// sebagai waktu di masa depan. Sama besarnya dengan yang dipakai jalur visite dokter.
        /// </summary>
        private static readonly TimeSpan ToleransiJamMaju = TimeSpan.FromMinutes(5);

        /// <summary>Kalimat penolakan <c>VAL-KEP-06</c>, apa adanya seperti pada validation matrix.</summary>
        public const string PenolakanBukanPenulis =
            "Catatan ini ditulis petugas lain. Anda tidak dapat mengubahnya.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _contextService;
        private readonly NursingActorService _actorService;
        private readonly ClinicalDocumentIntegrityService _integrityService;
        private readonly ClinicalNoteAddendumService _addendumService;

        public NursingInterventionService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService contextService,
            NursingActorService actorService,
            ClinicalDocumentIntegrityService integrityService,
            ClinicalNoteAddendumService addendumService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
            _actorService = actorService;
            _integrityService = integrityService;
            _addendumService = addendumService;
        }

        // =====================================================================
        // BE-RWI-061 - pencatatan
        // =====================================================================

        /// <summary>
        /// Mencatat satu tindakan keperawatan yang sudah dilakukan.
        /// </summary>
        /// <remarks>
        /// Kiriman ulang dengan kunci yang sama mengembalikan catatan yang sama beserta
        /// <c>200</c> - bukan <c>201</c>, dan bukan <c>409</c>. Bagi pengguna, tombol yang
        /// tertekan dua kali tidak melahirkan dua tindakan pada rekam medis.
        /// </remarks>
        public async Task<NursingInterventionResult> RecordAsync(
            CreateNursingInterventionRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var nama = request.InterventionName?.Trim();

            if (string.IsNullOrWhiteSpace(nama))
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama tindakan wajib diisi.");
            }

            if (request.EncounterId == Guid.Empty)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kunjungan wajib diisi.");
            }

            var kunci = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();

            // Kiriman ulang biasa dijawab lebih dulu, sebelum satu pun penjaga lain dijalankan.
            // Permintaan kedua yang identik tidak boleh ditolak hanya karena waktunya kini sudah
            // lewat batas toleransi, misalnya.
            if (kunci != null)
            {
                var sudahAda = await FindByIdempotencyKeyAsync(kunci, cancellationToken);

                if (sudahAda != null)
                    return NursingInterventionResult.Ok(sudahAda, isReplay: true);
            }

            var konteks = await _contextService.ResolveAsync(
                request.EncounterId,
                expectedEpisodeId: request.InpEpisodeId,
                forNewDocument: true,
                cancellationToken: cancellationToken);

            if (!konteks.IsResolved || konteks.Context == null)
            {
                return NursingInterventionResult.Fail(
                    konteks.StatusCode,
                    konteks.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat ditentukan.");
            }

            var now = DateTime.UtcNow;
            var waktuTindakan = request.PerformedAt ?? now;

            // VAL-KEP-13. Tindakan yang belum terjadi bukan fakta; mencatatnya membuat lini masa
            // pasien memuat pekerjaan besok.
            if (waktuTindakan > now.Add(ToleransiJamMaju))
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    PenolakanWaktuMasaDepan);
            }

            // VAL-KEP-14. Batas bawah hanya berlaku ketika saat masuk kamar memang diketahui.
            if (konteks.Context.AdmittedAt.HasValue &&
                waktuTindakan < konteks.Context.AdmittedAt.Value)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    PenolakanSebelumMasukKamar);
            }

            var penjagaButir = await EnsureCarePlanItemAsync(
                request.CarePlanItemId, konteks.Context.EpisodeId, cancellationToken);

            if (penjagaButir != null)
                return penjagaButir;

            var employeeId = await ResolvePerformerAsync(
                request.PerformedByEmployeeId, user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    request.PerformedByEmployeeId.HasValue && request.PerformedByEmployeeId.Value != Guid.Empty
                        ? "Perawat pelaksana tidak ditemukan atau tidak aktif."
                        : NursingActorService.PenolakanTanpaPegawai);
            }

            var tindakan = new CliNursingIntervention
            {
                Id = Guid.NewGuid(),
                EncounterId = konteks.Context.EncounterId,
                InpEpisodeId = konteks.Context.EpisodeId,
                PatientId = konteks.Context.PatientId,
                CarePlanItemId = NormalizeNullableGuid(request.CarePlanItemId),
                InterventionName = nama,
                PerformedAt = waktuTindakan,
                PerformedByEmployeeId = employeeId.Value,
                ResultNote = NormalizeNullableText(request.ResultNote),
                RecordStatus = NursingInterventionStatus.Recorded,
                IdempotencyKey = kunci,
                IsBillable = request.IsBillable,

                // INT-KEP-05. Tindakan yang dapat ditagih menunggu di Pending sampai Billing
                // punya kemampuan menerimanya; tidak ada satu pun yang hilang sementara itu.
                BillingDispatchStatus = request.IsBillable
                    ? NursingBillingDispatchStatus.Pending
                    : NursingBillingDispatchStatus.NotApplicable,
                BillingDispatchAttemptCount = 0,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<CliNursingIntervention>().Add(tindakan);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                // Penjaga database yang menolak: dua permintaan berkunci sama tiba benar-benar
                // bersamaan. Yang kalah membaca ulang baris pemenangnya lalu menjawab seperti
                // kiriman ulang biasa - VAL-KEP-15 menuntut 200, bukan 409.
                _dbContext.Entry(tindakan).State = EntityState.Detached;

                var pemenang = await FindByIdempotencyKeyAsync(kunci, cancellationToken);

                if (pemenang != null)
                    return NursingInterventionResult.Ok(pemenang, isReplay: true);

                throw;
            }

            return NursingInterventionResult.Ok(tindakan);
        }

        /// <summary>Daftar tindakan satu perawatan, terurut waktu tindakan.</summary>
        public async Task<PagedResult<NursingInterventionListItem>> GetByEpisodeAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            Guid? performedByEmployeeId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var query = _dbContext.Set<CliNursingIntervention>()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete);

            if (from.HasValue)
                query = query.Where(x => x.PerformedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.PerformedAt <= to.Value);

            if (performedByEmployeeId.HasValue && performedByEmployeeId.Value != Guid.Empty)
                query = query.Where(x => x.PerformedByEmployeeId == performedByEmployeeId.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var baris = await query
                .OrderBy(x => x.PerformedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var namaPegawai = await _actorService.GetEmployeeNamesAsync(
                baris.Select(x => x.PerformedByEmployeeId).ToList(), cancellationToken);

            return new PagedResult<NursingInterventionListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = baris.Select(x => new NursingInterventionListItem
                {
                    Id = x.Id,
                    InterventionName = x.InterventionName,
                    PerformedAt = x.PerformedAt,
                    PerformedByEmployeeId = x.PerformedByEmployeeId,
                    PerformedByEmployeeName = namaPegawai.GetValueOrDefault(x.PerformedByEmployeeId),
                    CarePlanItemId = x.CarePlanItemId,
                    RecordStatus = x.RecordStatus,
                    RecordStatusLabel = NamaKeadaanCatatan(x.RecordStatus),
                    BillingDispatchStatus = x.BillingDispatchStatus,
                    BillingDispatchStatusLabel = NamaKeadaanTagihan(x.BillingDispatchStatus),
                    IsBillable = x.IsBillable
                }).ToList()
            };
        }

        /// <summary>Satu catatan tindakan, atau kosong bila tidak ditemukan.</summary>
        public async Task<CliNursingIntervention?> FindAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<CliNursingIntervention>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        }

        /// <summary>Membentuk balasan lengkap satu catatan tindakan.</summary>
        public async Task<NursingInterventionResponse> ToResponseAsync(
            CliNursingIntervention tindakan,
            bool isReplay = false,
            CancellationToken cancellationToken = default)
        {
            var namaPegawai = await _actorService.GetEmployeeNamesAsync(
                new[] { tindakan.PerformedByEmployeeId }, cancellationToken);

            string? masalah = null;

            if (tindakan.CarePlanItemId.HasValue)
            {
                masalah = await _dbContext.Set<CliNursingCarePlanItem>()
                    .AsNoTracking()
                    .Where(x => x.Id == tindakan.CarePlanItemId.Value)
                    .Select(x => x.ProblemStatement)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new NursingInterventionResponse
            {
                Id = tindakan.Id,
                EncounterId = tindakan.EncounterId,
                InpEpisodeId = tindakan.InpEpisodeId,
                PatientId = tindakan.PatientId,
                CarePlanItemId = tindakan.CarePlanItemId,
                CarePlanProblemStatement = masalah,
                InterventionName = tindakan.InterventionName,
                PerformedAt = tindakan.PerformedAt,
                PerformedByEmployeeId = tindakan.PerformedByEmployeeId,
                PerformedByEmployeeName = namaPegawai.GetValueOrDefault(tindakan.PerformedByEmployeeId),
                ResultNote = tindakan.ResultNote,
                RecordStatus = tindakan.RecordStatus,
                RecordStatusLabel = NamaKeadaanCatatan(tindakan.RecordStatus),
                FinalizedAt = tindakan.FinalizedAt,
                IsBillable = tindakan.IsBillable,
                BillingDispatchStatus = tindakan.BillingDispatchStatus,
                BillingDispatchStatusLabel = NamaKeadaanTagihan(tindakan.BillingDispatchStatus),
                CreateDateTime = tindakan.CreateDateTime,
                UpdateDateTime = tindakan.UpdateDateTime,
                IsReplay = isReplay
            };
        }

        // =====================================================================
        // BE-RWI-062 - penyuntingan, finalisasi, koreksi, dan keadaan tagihan
        // =====================================================================

        /// <summary>
        /// Menyunting isi catatan tindakan selama catatannya belum final.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>state-transition-matrix.md</c> bagian 3 dan <c>VAL-KEP-06</c>: hanya penulisnya
        /// yang boleh menyunting, dan hanya selama catatannya belum final. Sesudah final,
        /// penjaganya bukan lagi aturan di sini melainkan mesin keutuhan dokumen -
        /// <c>EnsureMutableAsync</c> menolaknya dengan arahan memakai koreksi.
        /// </para>
        /// <para>
        /// <b>Dua penjaga sengaja berlapis.</b> Pemeriksaan status di sini menjawab pertanyaan
        /// domain, sedangkan mesin keutuhan menjawab pertanyaan rekam medis. Membiarkan hanya
        /// salah satunya berarti catatan yang sudah ditandatangani masih dapat disunting lewat
        /// jalur yang lupa memeriksanya.
        /// </para>
        /// </remarks>
        public async Task<NursingInterventionResult> UpdateAsync(
            Guid id,
            UpdateNursingInterventionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var nama = request.InterventionName?.Trim();

            if (string.IsNullOrWhiteSpace(nama))
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama tindakan wajib diisi.");
            }

            var tindakan = await FindAsync(id, cancellationToken);

            if (tindakan == null)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan.");
            }

            if (tindakan.RecordStatus == NursingInterventionStatus.Finalized)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan ini sudah final dan tidak dapat diubah. Gunakan koreksi untuk membetulkan.");
            }

            // VAL-KEP-06. Kepemilikan data diturunkan dari baris, bukan dari nama peran.
            if (tindakan.CreateBy != actorUserId)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status403Forbidden,
                    PenolakanBukanPenulis);
            }

            var penjagaKeutuhan = await _integrityService.EnsureMutableAsync(
                ClinicalDocumentKind.Procedure, tindakan.Id, cancellationToken);

            if (!penjagaKeutuhan.IsAllowed)
            {
                return NursingInterventionResult.Fail(
                    penjagaKeutuhan.StatusCode,
                    penjagaKeutuhan.ErrorMessage ?? "Catatan ini tidak dapat diubah.");
            }

            var now = DateTime.UtcNow;

            tindakan.InterventionName = nama;
            tindakan.ResultNote = NormalizeNullableText(request.ResultNote);
            tindakan.UpdateDateTime = now;
            tindakan.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingInterventionResult.Ok(tindakan, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>
        /// Menyatakan catatan tindakan final, sekaligus mendaftarkannya pada mesin keutuhan
        /// rekam medis sebagai dokumen <c>Procedure</c> tertanda tangan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>AC-CAP014-03</c>, <c>state-transition-matrix.md</c> bagian 3. Pendaftaran dan
        /// finalisasi berada pada <b>satu</b> <c>SaveChanges</c>: bila pendaftaran gagal,
        /// finalisasi ikut batal. Catatan final tanpa baris keutuhan adalah catatan yang tidak
        /// dapat dikoreksi selamanya - persis keadaan yang sedang ditutup.
        /// </para>
        /// <para>
        /// <b>Keadaan tagihan tidak disentuh sama sekali di sini.</b> Itu mesin status yang lain,
        /// dan menggabungkan keduanya adalah kesalahan yang paling mahal pada slice ini.
        /// </para>
        /// <para>
        /// Aman dipanggil berulang: catatan yang sudah final dikembalikan apa adanya.
        /// </para>
        /// </remarks>
        public async Task<NursingInterventionResult> FinalizeAsync(
            Guid id,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            var tindakan = await FindAsync(id, cancellationToken);

            if (tindakan == null)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan.");
            }

            if (tindakan.RecordStatus == NursingInterventionStatus.Finalized)
                return NursingInterventionResult.Ok(tindakan, isReplay: true);

            var now = DateTime.UtcNow;

            // Penulis catatan yang menjadi penanda tangan, bukan pengguna yang menekan tombol -
            // RWI-AC-157. Keduanya berbeda ketika kepala ruangan yang memfinalkan.
            var authorUserId = tindakan.CreateBy != Guid.Empty ? tindakan.CreateBy : actorUserId;

            tindakan.RecordStatus = NursingInterventionStatus.Finalized;
            tindakan.FinalizedAt = now;
            tindakan.FinalizedByUserId = actorUserId;
            tindakan.UpdateDateTime = now;
            tindakan.UpdateBy = actorUserId;

            try
            {
                await _integrityService.RegisterSignedAsync(
                    ClinicalDocumentKind.Procedure,
                    tindakan.Id,
                    tindakan.PatientId,
                    tindakan.EncounterId,
                    authorUserId,
                    deviceInfo,
                    ipAddress,
                    now,
                    cancellationToken);
            }
            catch (InvalidOperationException pendaftaranGagal)
            {
                // Keluar sebelum SaveChanges. Tidak satu pun perubahan di atas ikut tersimpan,
                // sehingga catatannya tetap Recorded - AC-CAP014-03 kriteria 3.
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan tidak dapat difinalkan karena pendaftaran pada rekam medis gagal: " +
                    pendaftaranGagal.Message);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingInterventionResult.Ok(tindakan, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>
        /// Mencatat hasil percobaan pengiriman tagihan satu tindakan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>INT-KEP-05</c>, <c>AC-CAP014-02</c>. <b>Catatan klinisnya tidak pernah disentuh
        /// metode ini</b> - hanya kolom keadaan pengiriman yang berubah. Itulah yang membuat
        /// kegagalan sistem tagihan tidak dapat menghapus bukti bahwa tindakannya pernah
        /// dilakukan.
        /// </para>
        /// <para>
        /// Belum ada endpoint yang memanggilnya, dan itu disengaja:
        /// <c>BillingManagement</c> belum memiliki kemampuan transaksi yang menerima pemicu ini,
        /// sehingga pemanggilnya kelak adalah pengirim milik modul itu. Yang sudah ada sekarang
        /// adalah mesin keadaannya beserta permukaan bacanya, supaya tindakan yang menunggu tidak
        /// hilang.
        /// </para>
        /// </remarks>
        /// <param name="id">Catatan tindakan yang dikirim.</param>
        /// <param name="berhasil">Benar bila Billing menerima pemicunya.</param>
        /// <param name="failureReason">Sebab kegagalan; diabaikan ketika berhasil.</param>
        /// <param name="actorUserId">Pengguna atau proses yang menjalankan pengiriman.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<NursingInterventionResult> RecordBillingDispatchOutcomeAsync(
            Guid id,
            bool berhasil,
            string? failureReason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var tindakan = await FindAsync(id, cancellationToken);

            if (tindakan == null)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan.");
            }

            if (!tindakan.IsBillable)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan ini tidak ditagihkan, sehingga tidak ada pengiriman tagihan untuknya.");
            }

            var now = DateTime.UtcNow;

            tindakan.BillingDispatchStatus = berhasil
                ? NursingBillingDispatchStatus.Dispatched
                : NursingBillingDispatchStatus.Failed;
            tindakan.BillingDispatchedAt = now;
            tindakan.BillingDispatchAttemptCount += 1;
            tindakan.BillingDispatchFailureReason = berhasil
                ? null
                : Potong(NormalizeNullableText(failureReason), 500);
            tindakan.UpdateDateTime = now;
            tindakan.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingInterventionResult.Ok(tindakan, statusCode: StatusCodes.Status200OK);
        }

        /// <summary>Keadaan pengiriman tagihan satu tindakan - <c>AC-CAP014-02</c>.</summary>
        /// <returns>Kosong bila catatannya tidak ditemukan.</returns>
        public async Task<BillingDispatchResponse?> GetBillingDispatchAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var tindakan = await _dbContext.Set<CliNursingIntervention>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (tindakan == null)
                return null;

            return new BillingDispatchResponse
            {
                InterventionId = tindakan.Id,
                IsBillable = tindakan.IsBillable,
                BillingDispatchStatus = tindakan.BillingDispatchStatus,
                BillingDispatchStatusLabel = NamaKeadaanTagihan(tindakan.BillingDispatchStatus),
                BillingDispatchedAt = tindakan.BillingDispatchedAt,
                BillingDispatchAttemptCount = tindakan.BillingDispatchAttemptCount,
                BillingDispatchFailureReason = tindakan.BillingDispatchFailureReason,
                RecordStatus = tindakan.RecordStatus,
                RecordStatusLabel = NamaKeadaanCatatan(tindakan.RecordStatus),
                Message = PesanKeadaanTagihan(tindakan)
            };
        }

        /// <summary>Menambah satu koreksi pada catatan tindakan yang sudah final.</summary>
        /// <remarks>
        /// <b>Tidak menyimpan barisnya sendiri.</b> Ia meneruskan ke
        /// <c>ClinicalNoteAddendumService</c> milik <c>MedicalRecordManagement</c> dengan jenis
        /// dokumen <c>Procedure</c>. Membangun penyimpanan koreksi di dalam
        /// <c>ClinicalManagement</c> berarti membuat mesin koreksi tandingan, dan itu dilarang
        /// <c>RWI-DEC-087</c> serta justru dihindari <c>RWI-DEC-091</c>.
        /// </remarks>
        public async Task<(IntegrityGuardResult Result, MrcClinicalNoteAddendum? Addendum)> CreateAddendumAsync(
            Guid id,
            CreateInterventionAddendumRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            return await _addendumService.CreateAsync(
                ClinicalDocumentKind.Procedure,
                id,
                actorUserId,
                actorHasSubstituteAuthority: false,
                request.Content,
                request.Reason,
                deviceInfo,
                ipAddress,
                DateTime.UtcNow,
                cancellationToken);
        }

        /// <summary>Daftar koreksi satu catatan tindakan, terurut nomor.</summary>
        public async Task<List<MrcClinicalNoteAddendum>> ListAddendumsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _addendumService.ListByDocumentAsync(
                ClinicalDocumentKind.Procedure, id, cancellationToken);
        }

        /// <summary>Nama penulis beberapa koreksi sekaligus, untuk tampilan daftar.</summary>
        public async Task<Dictionary<Guid, string?>> GetAuthorNamesAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            var ids = userIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (ids.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.DisplayName })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.DisplayName, cancellationToken);
        }

        /// <summary>Nama keadaan catatan dalam Bahasa Indonesia.</summary>
        public static string NamaKeadaanCatatan(NursingInterventionStatus status) => status switch
        {
            NursingInterventionStatus.Recorded => "Tercatat",
            NursingInterventionStatus.Finalized => "Final",
            _ => status.ToString()
        };

        /// <summary>Nama keadaan pengiriman tagihan dalam Bahasa Indonesia.</summary>
        public static string NamaKeadaanTagihan(NursingBillingDispatchStatus status) => status switch
        {
            NursingBillingDispatchStatus.NotApplicable => "Tidak ditagihkan",
            NursingBillingDispatchStatus.Pending => "Menunggu dikirim ke tagihan",
            NursingBillingDispatchStatus.Dispatched => "Sudah dikirim ke tagihan",
            NursingBillingDispatchStatus.Failed => "Pengiriman tagihan gagal",
            _ => status.ToString()
        };

        // =====================================================================
        // Penolong internal
        // =====================================================================

        private async Task<CliNursingIntervention?> FindByIdempotencyKeyAsync(
            string kunci,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<CliNursingIntervention>()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);
        }

        /// <summary>
        /// Memastikan butir rencana asuhan yang dirujuk memang milik perawatan yang sama.
        /// </summary>
        private async Task<NursingInterventionResult?> EnsureCarePlanItemAsync(
            Guid? carePlanItemId,
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            if (!carePlanItemId.HasValue || carePlanItemId.Value == Guid.Empty)
                return null;

            var episodeButir = await _dbContext.Set<CliNursingCarePlanItem>()
                .AsNoTracking()
                .Where(x => x.Id == carePlanItemId.Value && !x.IsDelete)
                .Join(_dbContext.Set<CliNursingCarePlan>().AsNoTracking(),
                      butir => butir.CarePlanId,
                      rencana => rencana.Id,
                      (butir, rencana) => (Guid?)rencana.InpEpisodeId)
                .FirstOrDefaultAsync(cancellationToken);

            if (episodeButir == null)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Butir rencana asuhan tidak ditemukan.");
            }

            if (episodeButir.Value != episodeId)
            {
                return NursingInterventionResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Butir rencana asuhan bukan milik perawatan yang sama.");
            }

            return null;
        }

        /// <summary>
        /// Menemukan perawat pelaksana: yang disebut permintaan bila terisi dan memang ada, atau
        /// perawat yang sedang masuk.
        /// </summary>
        private async Task<Guid?> ResolvePerformerAsync(
            Guid? diminta,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (diminta.HasValue && diminta.Value != Guid.Empty)
            {
                var ada = await _actorService.IsActiveEmployeeAsync(diminta.Value, cancellationToken);
                return ada ? diminta : null;
            }

            return await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);
        }

        /// <summary>
        /// Kalimat siap tampil yang menjelaskan keadaan pengiriman tagihan bagi pengguna.
        /// </summary>
        /// <remarks>
        /// Selalu menegaskan bahwa catatan klinisnya aman, karena itulah pertanyaan pertama
        /// perawat ketika melihat penanda gagal.
        /// </remarks>
        private static string PesanKeadaanTagihan(CliNursingIntervention tindakan) =>
            tindakan.BillingDispatchStatus switch
            {
                NursingBillingDispatchStatus.NotApplicable =>
                    "Tindakan ini tidak ditagihkan.",
                NursingBillingDispatchStatus.Pending =>
                    "Tagihan tindakan ini menunggu dikirim. Catatan klinisnya sudah tersimpan.",
                NursingBillingDispatchStatus.Dispatched =>
                    "Tagihan tindakan ini sudah terkirim.",
                NursingBillingDispatchStatus.Failed =>
                    "Pengiriman tagihan tindakan ini gagal dan dapat dicoba ulang. " +
                    "Catatan klinisnya tetap tersimpan dan tidak terpengaruh.",
                _ => "Keadaan pengiriman tagihan tidak dikenali."
            };

        private static string? Potong(string? value, int maxLength) =>
            value != null && value.Length > maxLength ? value[..maxLength] : value;

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
