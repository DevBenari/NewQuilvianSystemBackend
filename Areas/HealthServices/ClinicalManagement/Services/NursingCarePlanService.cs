using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Hasil satu perintah pada rencana asuhan keperawatan.
    /// </summary>
    /// <remarks>
    /// Bentuk ini dipilih, bukan melempar exception, karena setiap penolakan sudah punya pesan
    /// dan kode status yang ditetapkan <c>validation-matrix.md</c>. Controller tinggal
    /// meneruskannya tanpa menerjemahkan ulang - pola yang sama dipakai
    /// <c>InpatientClinicalContextService</c> dan <c>PhysicianVisitService</c>.
    /// </remarks>
    public sealed class NursingCarePlanResult
    {
        public bool IsSuccess { get; init; }

        public int StatusCode { get; init; }

        public string? ErrorMessage { get; init; }

        public CliNursingCarePlan? CarePlan { get; init; }

        public CliNursingCarePlanItem? Item { get; init; }

        internal static NursingCarePlanResult Ok(
            CliNursingCarePlan? carePlan = null,
            CliNursingCarePlanItem? item = null,
            int statusCode = StatusCodes.Status200OK) => new()
            {
                IsSuccess = true,
                StatusCode = statusCode,
                CarePlan = carePlan,
                Item = item
            };

        internal static NursingCarePlanResult Fail(int statusCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Pemilik seluruh perintah dan pembacaan rencana asuhan keperawatan - <c>CAP-013</c>,
    /// <c>BE-RWI-059</c>, <c>BE-RWI-060</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Controller tidak menyentuh <c>ApplicationDbContext</c> untuk permukaan ini.</b>
    /// <c>QBE-SVC-001</c> menaruh CRUD dan orkestrasi domain di service. Dua controller klinis
    /// yang sudah ada memang mengakses context langsung; itu utang teknis milik modul lain dan
    /// bukan wewenang untuk menirunya pada kode baru.
    /// </para>
    /// <para>
    /// <b>Dua penjaga yang paling menentukan di berkas ini.</b> Pertama, satu perawatan tepat
    /// satu rencana asuhan - dijaga pemeriksaan aplikasi <b>dan</b> unique parsial pada
    /// database, karena pemeriksaan aplikasi saja tidak dapat mencegah dua permintaan yang tiba
    /// bersamaan. Kedua, butir tidak dapat dinyatakan tercapai tanpa evaluasi
    /// (<c>VAL-KEP-16</c>): menyatakan masalah teratasi tanpa satu pun evaluasi membuat rekam
    /// medis tidak dapat menunjukkan dasarnya.
    /// </para>
    /// <para>
    /// <b>Perubahan rencana asuhan bukan koreksi.</b> Rencana memang berubah ketika keadaan
    /// pasien berubah, sehingga ia memakai mesin versi butir dan <b>bukan</b> mesin addendum
    /// milik <c>MedicalRecordManagement</c> - <c>RWI-DEC-091</c>, <c>RWI-AC-177</c>.
    /// </para>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class NursingCarePlanService
    {
        /// <summary>Kalimat penolakan <c>VAL-KEP-16</c>, apa adanya seperti pada validation matrix.</summary>
        public const string PenolakanTanpaEvaluasi =
            "Butir ini belum punya catatan evaluasi, sehingga belum dapat dinyatakan tercapai.";

        /// <summary>Kalimat penolakan ketika perawatan tidak sedang menerima dokumentasi baru.</summary>
        public const string PenolakanEpisodeBukanAdmitted =
            "Perawatan pasien ini sedang tidak dalam keadaan dirawat, sehingga rencana asuhan " +
            "tidak dapat diubah. Catatannya tetap dapat dibaca.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _contextService;
        private readonly NursingActorService _actorService;

        public NursingCarePlanService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService contextService,
            NursingActorService actorService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
            _actorService = actorService;
        }

        // =====================================================================
        // Perintah
        // =====================================================================

        /// <summary>
        /// Membuka rencana asuhan bagi satu perawatan. Perawatan yang sudah memiliki rencana
        /// dijawab <c>409</c>.
        /// </summary>
        public async Task<NursingCarePlanResult> OpenAsync(
            CreateNursingCarePlanRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.EncounterId == Guid.Empty)
                return NursingCarePlanResult.Fail(StatusCodes.Status400BadRequest, "Kunjungan wajib diisi.");

            var konteks = await _contextService.ResolveAsync(
                request.EncounterId,
                expectedEpisodeId: request.InpEpisodeId,
                forNewDocument: true,
                cancellationToken: cancellationToken);

            if (!konteks.IsResolved || konteks.Context == null)
            {
                return NursingCarePlanResult.Fail(
                    konteks.StatusCode,
                    konteks.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat ditentukan.");
            }

            // AC-5 BE-RWI-059 dan state-transition-matrix.md bagian 2: rencana asuhan hanya
            // lahir di atas perawatan yang benar-benar Admitted. Resolver bersama memperlakukan
            // DischargePending sebagai masih berjalan untuk dokumentasi harian; rencana asuhan
            // sengaja lebih ketat, karena membuka rencana baru bagi pasien yang sedang disiapkan
            // pulang bukan pekerjaan yang diminta kontrak ini.
            if (konteks.Context.EpisodeStatus != InpEpisodeStatus.Admitted)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    PenolakanEpisodeBukanAdmitted);
            }

            var employeeId = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    NursingActorService.PenolakanTanpaPegawai);
            }

            var sudahAda = await _dbContext.Set<CliNursingCarePlan>()
                .AsNoTracking()
                .AnyAsync(x => x.InpEpisodeId == konteks.Context.EpisodeId && !x.IsDelete, cancellationToken);

            if (sudahAda)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Perawatan ini sudah memiliki rencana asuhan. Tambahkan masalah keperawatan " +
                    "pada rencana yang sudah ada.");
            }

            var now = DateTime.UtcNow;

            var rencana = new CliNursingCarePlan
            {
                Id = Guid.NewGuid(),
                EncounterId = konteks.Context.EncounterId,
                InpEpisodeId = konteks.Context.EpisodeId,
                PatientId = konteks.Context.PatientId,
                OpenedAt = now,
                OpenedByEmployeeId = employeeId.Value,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<CliNursingCarePlan>().Add(rencana);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingCarePlanResult.Ok(rencana, statusCode: StatusCodes.Status201Created);
        }

        /// <summary>Menambah satu masalah keperawatan pada rencana asuhan.</summary>
        public async Task<NursingCarePlanResult> AddItemAsync(
            Guid carePlanId,
            CreateCarePlanItemRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var masalah = request.ProblemStatement?.Trim();

            if (string.IsNullOrWhiteSpace(masalah))
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Masalah keperawatan wajib diisi.");
            }

            var rencana = await _dbContext.Set<CliNursingCarePlan>()
                .FirstOrDefaultAsync(x => x.Id == carePlanId && !x.IsDelete, cancellationToken);

            if (rencana == null)
                return NursingCarePlanResult.Fail(StatusCodes.Status404NotFound, "Rencana asuhan tidak ditemukan.");

            var penjaga = await EnsureEpisodeAdmittedAsync(rencana.InpEpisodeId, cancellationToken);

            if (penjaga != null)
                return penjaga;

            var penjagaPengkajian = await EnsureSourceAssessmentAsync(
                request.SourceAssessmentId, rencana.InpEpisodeId, cancellationToken);

            if (penjagaPengkajian != null)
                return penjagaPengkajian;

            var employeeId = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    NursingActorService.PenolakanTanpaPegawai);
            }

            var now = DateTime.UtcNow;

            var butir = new CliNursingCarePlanItem
            {
                Id = Guid.NewGuid(),
                CarePlanId = rencana.Id,
                AuthoredByEmployeeId = employeeId.Value,
                AuthoredAt = now,
                NursingDiagnosisId = NormalizeNullableGuid(request.NursingDiagnosisId),
                SourceAssessmentId = NormalizeNullableGuid(request.SourceAssessmentId),
                ProblemStatement = masalah,
                GoalStatement = NormalizeNullableText(request.GoalStatement),
                PlannedIntervention = NormalizeNullableText(request.PlannedIntervention),
                ItemStatus = NursingCarePlanItemStatus.Active,
                VersionNumber = 1,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<CliNursingCarePlanItem>().Add(butir);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingCarePlanResult.Ok(rencana, butir, StatusCodes.Status201Created);
        }

        /// <summary>
        /// Memperbarui isi satu butir. Versi sebelumnya <b>disalin lebih dulu</b> beserta penulis
        /// dan waktu aslinya - <c>BE-RWI-060</c>, <c>AC-CAP013-02</c>.
        /// </summary>
        /// <remarks>
        /// Penyalinan dan pembaruan berada pada satu <c>SaveChanges</c>, sehingga tidak pernah ada
        /// keadaan di mana butir sudah berubah tetapi versi lamanya gagal tersimpan - dan itulah
        /// keadaan yang menghapus jejak siapa yang menilai pertama kali.
        /// </remarks>
        public async Task<NursingCarePlanResult> UpdateItemAsync(
            Guid itemId,
            UpdateCarePlanItemRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var masalah = request.ProblemStatement?.Trim();

            if (string.IsNullOrWhiteSpace(masalah))
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Masalah keperawatan wajib diisi.");
            }

            var (butir, rencana, penolakan) = await AmbilButirUntukPerubahanAsync(itemId, cancellationToken);

            if (penolakan != null)
                return penolakan;

            if (butir!.ItemStatus != NursingCarePlanItemStatus.Active)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Butir ini sudah ditutup, sehingga isinya tidak dapat diperbarui.");
            }

            var employeeId = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    NursingActorService.PenolakanTanpaPegawai);
            }

            var now = DateTime.UtcNow;

            ArsipkanVersiSaatIni(butir, now, actorUserId);

            butir.ProblemStatement = masalah;
            butir.GoalStatement = NormalizeNullableText(request.GoalStatement);
            butir.PlannedIntervention = NormalizeNullableText(request.PlannedIntervention);
            butir.NursingDiagnosisId = NormalizeNullableGuid(request.NursingDiagnosisId);
            butir.VersionNumber += 1;
            butir.AuthoredByEmployeeId = employeeId.Value;
            butir.AuthoredAt = now;
            butir.UpdateDateTime = now;
            butir.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingCarePlanResult.Ok(rencana, butir);
        }

        /// <summary>Mencatat evaluasi hasil asuhan pada satu butir.</summary>
        /// <remarks>
        /// <para>
        /// <b>Evaluasi kedua tidak menimpa evaluasi pertama.</b> Ketika butir sudah punya
        /// evaluasi, keadaan lamanya diarsipkan lebih dulu ke riwayat versi - sama seperti
        /// pembaruan isi. Kontrak hanya menuntut penyalinan pada pembaruan isi, tetapi menimpa
        /// evaluasi berarti menghapus penilaian klinis yang pernah dibuat, dan pada tabel yang
        /// memang sudah menyediakan kolom <c>EvaluationNote</c> untuk versi lama itu akan menjadi
        /// kehilangan data yang tidak perlu.
        /// </para>
        /// </remarks>
        public async Task<NursingCarePlanResult> EvaluateItemAsync(
            Guid itemId,
            EvaluateCarePlanItemRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var evaluasi = request.EvaluationNote?.Trim();

            if (string.IsNullOrWhiteSpace(evaluasi))
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan evaluasi wajib diisi.");
            }

            var (butir, rencana, penolakan) = await AmbilButirUntukPerubahanAsync(itemId, cancellationToken);

            if (penolakan != null)
                return penolakan;

            var now = DateTime.UtcNow;

            if (butir!.LastEvaluatedAt != null)
            {
                var employeeId = await _actorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

                if (employeeId == null)
                {
                    return NursingCarePlanResult.Fail(
                        StatusCodes.Status400BadRequest,
                        NursingActorService.PenolakanTanpaPegawai);
                }

                ArsipkanVersiSaatIni(butir, now, actorUserId);

                butir.VersionNumber += 1;
                butir.AuthoredByEmployeeId = employeeId.Value;
                butir.AuthoredAt = now;
            }

            butir.EvaluationNote = evaluasi;
            butir.LastEvaluatedAt = now;
            butir.UpdateDateTime = now;
            butir.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingCarePlanResult.Ok(rencana, butir);
        }

        /// <summary>
        /// Menutup satu masalah keperawatan, sebagai tercapai atau sebagai tidak lagi relevan.
        /// </summary>
        /// <remarks>
        /// <c>VAL-KEP-16</c>: penutupan sebagai <c>Resolved</c> ditolak <c>400</c> bila belum ada
        /// satu pun evaluasi, dan butirnya <b>tetap</b> <c>Active</c> - tidak ada perubahan
        /// setengah jadi yang tersimpan.
        /// </remarks>
        public async Task<NursingCarePlanResult> CloseItemAsync(
            Guid itemId,
            CloseCarePlanItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var alasan = request.Reason?.Trim();

            if (string.IsNullOrWhiteSpace(alasan))
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan penutupan wajib diisi.");
            }

            if (request.TargetStatus != NursingCarePlanItemStatus.Resolved &&
                request.TargetStatus != NursingCarePlanItemStatus.Discontinued)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Keadaan tujuan penutupan hanya boleh tercapai atau dihentikan.");
            }

            var (butir, rencana, penolakan) = await AmbilButirUntukPerubahanAsync(itemId, cancellationToken);

            if (penolakan != null)
                return penolakan;

            if (butir!.ItemStatus != NursingCarePlanItemStatus.Active)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status409Conflict,
                    "Butir ini sudah ditutup sebelumnya.");
            }

            // VAL-KEP-16. Berhenti sebelum satu nilai pun berubah, sehingga butirnya benar-benar
            // tetap Active - bukan sekadar dikembalikan begitu pada balasan.
            if (request.TargetStatus == NursingCarePlanItemStatus.Resolved &&
                butir.LastEvaluatedAt == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    PenolakanTanpaEvaluasi);
            }

            var now = DateTime.UtcNow;

            butir.ItemStatus = request.TargetStatus;
            butir.ResolvedAt = now;
            butir.CloseReason = alasan;
            butir.UpdateDateTime = now;
            butir.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingCarePlanResult.Ok(rencana, butir);
        }

        // =====================================================================
        // Pembacaan
        // =====================================================================

        /// <summary>
        /// Rencana asuhan satu perawatan beserta seluruh butirnya, terurut waktu pembuatan.
        /// </summary>
        /// <returns>Kosong bila perawatan itu belum memiliki rencana asuhan.</returns>
        public async Task<NursingCarePlanResponse?> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var rencana = await _dbContext.Set<CliNursingCarePlan>()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete)
                .FirstOrDefaultAsync(cancellationToken);

            if (rencana == null)
                return null;

            var butir = await AmbilButirAsync(rencana.Id, cancellationToken);

            return await ToResponseAsync(rencana, butir, cancellationToken);
        }

        /// <summary>Seluruh butir satu rencana asuhan, terurut waktu pembuatan.</summary>
        public async Task<List<CliNursingCarePlanItem>> AmbilButirAsync(
            Guid carePlanId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<CliNursingCarePlanItem>()
                .AsNoTracking()
                .Where(x => x.CarePlanId == carePlanId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>Membentuk balasan lengkap satu rencana asuhan beserta butirnya.</summary>
        public async Task<NursingCarePlanResponse> ToResponseAsync(
            CliNursingCarePlan rencana,
            IReadOnlyList<CliNursingCarePlanItem> butir,
            CancellationToken cancellationToken = default)
        {
            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == rencana.InpEpisodeId)
                .Select(x => new { x.EpisodeNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var pasien = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == rencana.PatientId)
                .Select(x => new { x.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            var namaPegawai = await _actorService.GetEmployeeNamesAsync(
                butir.Select(x => x.AuthoredByEmployeeId)
                    .Append(rencana.OpenedByEmployeeId)
                    .ToList(),
                cancellationToken);

            var nomorPengkajian = await AmbilNomorPengkajianAsync(butir, cancellationToken);

            return new NursingCarePlanResponse
            {
                Id = rencana.Id,
                EncounterId = rencana.EncounterId,
                InpEpisodeId = rencana.InpEpisodeId,
                EpisodeNumber = episode?.EpisodeNumber,
                PatientId = rencana.PatientId,
                PatientName = pasien?.FullName,
                OpenedAt = rencana.OpenedAt,
                OpenedByEmployeeId = rencana.OpenedByEmployeeId,
                OpenedByEmployeeName = namaPegawai.GetValueOrDefault(rencana.OpenedByEmployeeId),
                IsActive = rencana.IsActive,
                TotalItemCount = butir.Count,
                ActiveItemCount = butir.Count(x => x.ItemStatus == NursingCarePlanItemStatus.Active),
                Items = butir.Select(x => ToItemResponse(x, nomorPengkajian, namaPegawai)).ToList()
            };
        }

        /// <summary>Membentuk balasan satu butir, beserta nomor pengkajian asalnya bila ada.</summary>
        public async Task<CarePlanItemResponse> ToItemResponseAsync(
            CliNursingCarePlanItem butir,
            CancellationToken cancellationToken = default)
        {
            var nomorPengkajian = await AmbilNomorPengkajianAsync(new[] { butir }, cancellationToken);

            var namaPegawai = await _actorService.GetEmployeeNamesAsync(
                new[] { butir.AuthoredByEmployeeId }, cancellationToken);

            return ToItemResponse(butir, nomorPengkajian, namaPegawai);
        }

        /// <summary>
        /// Riwayat versi satu butir, terurut dari versi pertama - <c>BE-RWI-060</c>,
        /// <c>AC-CAP013-02</c>.
        /// </summary>
        /// <remarks>
        /// Pembacaan ini <b>tidak</b> menuntut perawatannya masih berjalan. Riwayat asuhan pasien
        /// yang sudah pulang justru itulah yang paling sering dibaca - <c>AC-CAP013-03</c>.
        /// </remarks>
        /// <returns>Kosong bila butirnya tidak ditemukan.</returns>
        public async Task<List<CarePlanItemRevisionResponse>?> GetRevisionsAsync(
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var adaButir = await _dbContext.Set<CliNursingCarePlanItem>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (!adaButir)
                return null;

            var revisi = await _dbContext.Set<CliNursingCarePlanItemRevision>()
                .AsNoTracking()
                .Where(x => x.CarePlanItemId == itemId && !x.IsDelete)
                .OrderBy(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            var namaPegawai = await _actorService.GetEmployeeNamesAsync(
                revisi.Select(x => x.OriginalAuthorEmployeeId).ToList(), cancellationToken);

            return revisi
                .Select(x => new CarePlanItemRevisionResponse
                {
                    Id = x.Id,
                    CarePlanItemId = x.CarePlanItemId,
                    VersionNumber = x.VersionNumber,
                    ProblemStatement = x.ProblemStatement,
                    GoalStatement = x.GoalStatement,
                    PlannedIntervention = x.PlannedIntervention,
                    EvaluationNote = x.EvaluationNote,
                    RevisedAt = x.RevisedAt,
                    OriginalAuthorEmployeeId = x.OriginalAuthorEmployeeId,
                    OriginalAuthorEmployeeName = namaPegawai.GetValueOrDefault(x.OriginalAuthorEmployeeId),
                    OriginalAuthoredAt = x.OriginalAuthoredAt
                })
                .ToList();
        }

        /// <summary>Nama keadaan butir dalam Bahasa Indonesia.</summary>
        public static string NamaKeadaanButir(NursingCarePlanItemStatus status) => status switch
        {
            NursingCarePlanItemStatus.Active => "Masih dikerjakan",
            NursingCarePlanItemStatus.Resolved => "Teratasi",
            NursingCarePlanItemStatus.Discontinued => "Dihentikan",
            _ => status.ToString()
        };

        // =====================================================================
        // Penolong internal
        // =====================================================================

        /// <summary>
        /// Menyalin keadaan butir yang sedang berlaku ke riwayat versi.
        /// </summary>
        /// <remarks>
        /// <b>Tidak menyimpan.</b> Pemanggil wajib menjalankannya pada <c>SaveChanges</c> yang
        /// sama dengan pembaruan butirnya, supaya tidak pernah ada butir yang sudah berubah
        /// tetapi versi lamanya gagal tersimpan. Penulis dan waktu yang disalin adalah milik
        /// versi yang diarsipkan, bukan milik perawat yang mengubah - <c>AC-CAP013-02</c>.
        /// </remarks>
        private void ArsipkanVersiSaatIni(CliNursingCarePlanItem butir, DateTime now, Guid actorUserId)
        {
            var revisi = new CliNursingCarePlanItemRevision
            {
                Id = Guid.NewGuid(),
                CarePlanItemId = butir.Id,
                VersionNumber = butir.VersionNumber,
                ProblemStatement = butir.ProblemStatement,
                GoalStatement = butir.GoalStatement,
                PlannedIntervention = butir.PlannedIntervention,
                EvaluationNote = butir.EvaluationNote,
                RevisedAt = now,
                OriginalAuthorEmployeeId = butir.AuthoredByEmployeeId,
                OriginalAuthoredAt = butir.AuthoredAt,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<CliNursingCarePlanItemRevision>().Add(revisi);
        }

        /// <summary>
        /// Mengambil butir beserta rencananya untuk diubah, sekaligus menegakkan penjaga
        /// perawatan yang sedang berjalan.
        /// </summary>
        private async Task<(CliNursingCarePlanItem? Butir, CliNursingCarePlan? Rencana, NursingCarePlanResult? Penolakan)>
            AmbilButirUntukPerubahanAsync(Guid itemId, CancellationToken cancellationToken)
        {
            var butir = await _dbContext.Set<CliNursingCarePlanItem>()
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (butir == null)
            {
                return (null, null, NursingCarePlanResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Butir rencana asuhan tidak ditemukan."));
            }

            var rencana = await _dbContext.Set<CliNursingCarePlan>()
                .FirstOrDefaultAsync(x => x.Id == butir.CarePlanId && !x.IsDelete, cancellationToken);

            if (rencana == null)
            {
                return (null, null, NursingCarePlanResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Rencana asuhan tidak ditemukan."));
            }

            var penjaga = await EnsureEpisodeAdmittedAsync(rencana.InpEpisodeId, cancellationToken);

            if (penjaga != null)
                return (null, null, penjaga);

            return (butir, rencana, null);
        }

        /// <summary>
        /// Menolak perubahan asuhan ketika perawatannya tidak lagi menerima dokumentasi baru.
        /// </summary>
        /// <remarks>
        /// <c>INV-KEP-02</c> dan <c>AC-CAP013-03</c>: setelah perawatan ditutup, seluruh riwayat
        /// asuhan <b>tetap terbaca</b>, tetapi tidak satu pun perubahan diterima. Pembacaan
        /// karena itu sengaja tidak memanggil penjaga ini.
        /// </remarks>
        private async Task<NursingCarePlanResult?> EnsureEpisodeAdmittedAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            var status = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (InpEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);

            if (status == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan rawat inap tidak ditemukan.");
            }

            if (status.Value != InpEpisodeStatus.Admitted)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    PenolakanEpisodeBukanAdmitted);
            }

            return null;
        }

        /// <summary>
        /// Memastikan pengkajian asal memang ada dan memang milik perawatan yang sama -
        /// <c>AC-CAP013-01</c> beserta penjaga salah pasiennya.
        /// </summary>
        private async Task<NursingCarePlanResult?> EnsureSourceAssessmentAsync(
            Guid? sourceAssessmentId,
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            if (!sourceAssessmentId.HasValue || sourceAssessmentId.Value == Guid.Empty)
                return null;

            var pengkajian = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => x.Id == sourceAssessmentId.Value && !x.IsDelete)
                .Select(x => new { x.InpEpisodeId })
                .FirstOrDefaultAsync(cancellationToken);

            if (pengkajian == null)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengkajian asal tidak ditemukan.");
            }

            // Penjaga salah pasien. Butir asuhan yang menunjuk pengkajian milik perawatan lain
            // membuat rekam medis menghubungkan dua pasien yang berbeda.
            if (pengkajian.InpEpisodeId.HasValue && pengkajian.InpEpisodeId.Value != episodeId)
            {
                return NursingCarePlanResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pengkajian asal bukan milik perawatan yang sama.");
            }

            return null;
        }

        private async Task<Dictionary<Guid, string>> AmbilNomorPengkajianAsync(
            IReadOnlyList<CliNursingCarePlanItem> butir,
            CancellationToken cancellationToken)
        {
            var ids = butir
                .Where(x => x.SourceAssessmentId.HasValue)
                .Select(x => x.SourceAssessmentId!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<Guid, string>();

            return await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.AssessmentNumber })
                .ToDictionaryAsync(x => x.Id, x => x.AssessmentNumber, cancellationToken);
        }

        private static CarePlanItemResponse ToItemResponse(
            CliNursingCarePlanItem butir,
            IReadOnlyDictionary<Guid, string> nomorPengkajian,
            IReadOnlyDictionary<Guid, string> namaPegawai) => new()
            {
                Id = butir.Id,
                CarePlanId = butir.CarePlanId,
                NursingDiagnosisId = butir.NursingDiagnosisId,
                SourceAssessmentId = butir.SourceAssessmentId,
                SourceAssessmentNumber = butir.SourceAssessmentId.HasValue
                    ? nomorPengkajian.GetValueOrDefault(butir.SourceAssessmentId.Value)
                    : null,
                ProblemStatement = butir.ProblemStatement,
                GoalStatement = butir.GoalStatement,
                PlannedIntervention = butir.PlannedIntervention,
                EvaluationNote = butir.EvaluationNote,
                LastEvaluatedAt = butir.LastEvaluatedAt,
                ItemStatus = butir.ItemStatus,
                ItemStatusLabel = NamaKeadaanButir(butir.ItemStatus),
                ResolvedAt = butir.ResolvedAt,
                CloseReason = butir.CloseReason,
                VersionNumber = butir.VersionNumber,
                AuthoredByEmployeeId = butir.AuthoredByEmployeeId,
                AuthoredByEmployeeName = namaPegawai.GetValueOrDefault(butir.AuthoredByEmployeeId),
                AuthoredAt = butir.AuthoredAt,
                CreateDateTime = butir.CreateDateTime,
                UpdateDateTime = butir.UpdateDateTime
            };

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
