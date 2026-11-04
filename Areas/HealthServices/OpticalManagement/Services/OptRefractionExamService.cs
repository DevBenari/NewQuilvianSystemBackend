using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Pemeriksaan refraksi (<c>BE-OPT-011</c>, <c>OPT-API-v1</c> grup Opt Refraction Exam, <c>OPT-STATE-v1</c> bagian 1,
    /// <c>OPT-VAL-v1</c> bagian 1 dan 3).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Alur:</b> mulai (<c>Draft</c>, versi 1 kosong) → isi hasil ukur (versi 1 diganti selama draf) → simpan final →
    /// revisi menambah versi 2, 3, … dengan alasan wajib (<c>OPT-DEC-047</c>). Draf boleh dibatalkan; pemeriksaan final
    /// tidak (<c>OPT-VAL-033</c>).
    /// </para>
    /// <para>
    /// <b>Tanpa penerbitan tagihan.</b> Fakta tarif pemeriksaan saat final (<c>OPT-DEC-042</c>) dikerjakan
    /// <c>BE-OPT-031</c>; di sini <see cref="OptRefractionExam.BillingFactEmittedAt"/> tidak disentuh.
    /// </para>
    /// <para>
    /// <b>Kewenangan klinis</b> dicek <see cref="OptPractitionerGuard"/> pada tanggal WIB saat tindakan: profesi RO/Sp.M,
    /// SIP, lalu STR (<c>OPT-VAL-001</c>..<c>003</c>). Pemeriksaan status dan versi dilakukan lebih dulu, sehingga data
    /// tidak pernah berubah bila salah satunya gagal.
    /// </para>
    /// <para>
    /// <b>Simpan ganda.</b> Tabel pemeriksaan tidak punya kolom kunci idempoten, sehingga pengulangan dicegah lewat
    /// keadaan: "Mulai" pada kunjungan yang sudah punya draf mengembalikan draf itu, final ulang ditolak
    /// <c>OPT-VAL-032</c>, dan tindakan lain memakai <c>ExpectedVersion</c>.
    /// </para>
    /// <para>
    /// <b>Kunci Rekam Medis</b> (<c>BE-OPT-013</c>, <c>OPT-DEC-050</c>). Setiap revisi hasil ukur adalah dokumen
    /// <see cref="ClinicalDocumentKind.OpticalRefraction"/> tersendiri: revisi 1 didaftarkan <c>Draft</c> saat mulai dan
    /// ditandatangani saat final; revisi sesudah final langsung bertanda tangan; draf yang dibatalkan ditandai batal.
    /// Semuanya ikut <c>SaveChanges</c> pemeriksaan, sehingga gagal bersama. Draf yang dikunci Rekam Medis karena
    /// kunjungan ditutup tidak bisa diisi atau difinalkan lagi.
    /// </para>
    /// </remarks>
    public class OptRefractionExamService
    {
        public const int DefaultPageSize = 25;
        public const int MaxPageSize = 100;

        public const string VisusFormatMessage = "Visus ditulis seperti 6/12.";
        public const string FinalMinimumMessage = "Isi visus jauh mata kanan dan kiri sebelum menyimpan final.";
        public const string AlreadyFinalMessage = "Pemeriksaan sudah final. Gunakan revisi bila ada koreksi.";
        public const string CancelFinalMessage = "Pemeriksaan final tidak dapat dibatalkan; gunakan revisi.";
        public const string CancelledMessage = "Pemeriksaan sudah dibatalkan.";
        public const string NotFinalMessage = "Pemeriksaan belum final. Ubah hasil ukur lewat isian draf.";
        public const string RevisionReasonMessage = "Tuliskan alasan revisi.";
        public const string CancelReasonMessage = "Tuliskan alasan pembatalan.";
        public const string StaleMessage = "Pemeriksaan sudah diubah petugas lain. Muat ulang halaman.";
        public const string NotFoundMessage = "Pemeriksaan refraksi tidak ditemukan.";

        // n/n berangka (boleh berdesimal), 1/~ (persepsi cahaya), atau 0 (tanpa persepsi cahaya) — OPT-DEC-059.
        private static readonly Regex VisusPattern = new(@"^(\d{1,3}(\.\d{1,2})?/\d{1,4}(\.\d{1,2})?|1/~|0)$", RegexOptions.CultureInvariant);

        private readonly ApplicationDbContext _dbContext;
        private readonly OptPractitionerGuard _guard;
        private readonly OptNumberService _numberService;
        private readonly OptVisitService _visitService;
        private readonly ClinicalDocumentIntegrityService _integrity;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeProvider _time;

        public const ClinicalDocumentKind DocumentKind = ClinicalDocumentKind.OpticalRefraction;

        public const string MedicalRecordLockedMessage =
            "Pemeriksaan ini terkunci di rekam medis karena kunjungan sudah ditutup. Batalkan draf ini dan mulai pemeriksaan pada kunjungan baru.";

        public OptRefractionExamService(
            ApplicationDbContext dbContext,
            OptPractitionerGuard guard,
            OptNumberService numberService,
            OptVisitService visitService,
            ClinicalDocumentIntegrityService integrity,
            IHttpContextAccessor httpContextAccessor,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _guard = guard;
            _numberService = numberService;
            _visitService = visitService;
            _integrity = integrity;
            _httpContextAccessor = httpContextAccessor;
            _time = timeProvider ?? TimeProvider.System;
        }

        // ---------------------------------------------------------------- baca

        public async Task<PagedResult<OptRefractionExamResponse>> GetListAsync(OptRefractionExamListQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

            var exams = _dbContext.OptRefractionExams.AsNoTracking().Where(x => !x.IsDelete);
            if (query.EncounterId is { } encounterId)
                exams = exams.Where(x => x.EncounterId == encounterId);
            if (query.PatientId is { } patientId)
                exams = exams.Where(x => x.PatientId == patientId);
            if (query.Status is { } status)
                exams = exams.Where(x => x.Status == status);

            var total = await exams.CountAsync(cancellationToken);
            var page = await exams
                .OrderByDescending(x => x.StartedAt).ThenByDescending(x => x.ExamNumber)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);

            var ids = page.Select(x => x.Id).ToList();
            var currents = await _dbContext.OptRefractionExamRevisions.AsNoTracking()
                .Where(r => ids.Contains(r.RefractionExamId) && !r.IsDelete)
                .ToListAsync(cancellationToken);
            var names = await NamesAsync(page.Select(x => x.ExaminerWorkforceProfileId).Concat(currents.Select(r => r.RevisedByWorkforceProfileId)), cancellationToken);

            return new PagedResult<OptRefractionExamResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = page.Select(exam => Project(
                        exam,
                        currents.Where(r => r.RefractionExamId == exam.Id && r.RevisionNo == exam.CurrentRevisionNo).ToList(),
                        names,
                        includeAllRevisions: false))
                    .ToList(),
            };
        }

        public async Task<OptResult<OptRefractionExamResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var exam = await _dbContext.OptRefractionExams.AsNoTracking()
                .Include(x => x.Revisions)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return exam is null
                ? OptResult<OptRefractionExamResponse>.NotFound(NotFoundMessage)
                : OptResult<OptRefractionExamResponse>.Ok(await ProjectAsync(exam, cancellationToken));
        }

        // ---------------------------------------------------------------- tindakan

        /// <summary>Mulai pemeriksaan. Kunjungan yang sudah punya draf mengembalikan draf itu, bukan draf kedua.</summary>
        public async Task<OptResult<OptRefractionExamResponse>> StartAsync(OptRefractionExamStartRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            if (request.EncounterId is not { } encounterId || encounterId == Guid.Empty)
                return OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.InvalidRequest, "Pilih kunjungan Optik.");

            var encounter = await _visitService.FindOpticalEncounterAsync(encounterId, cancellationToken);
            if (encounter is null)
                return OptResult<OptRefractionExamResponse>.NotFound("Kunjungan Optik tidak ditemukan.");
            if (OptVisitService.IsClosed(encounter.EncounterStatus))
                return OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.Val030, "Kunjungan sudah ditutup. Daftarkan kunjungan baru.");

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var draft = await _dbContext.OptRefractionExams.AsNoTracking()
                .Include(x => x.Revisions)
                .Where(x => x.EncounterId == encounterId && x.Status == OptRefractionExamStatus.Draft && !x.IsDelete)
                .OrderBy(x => x.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
            if (draft is not null)
                return OptResult<OptRefractionExamResponse>.Ok(await ProjectAsync(draft, cancellationToken));

            var now = Now();
            var pemeriksa = practitioner.Value!;
            var exam = new OptRefractionExam
            {
                ExamNumber = await _numberService.AllocateExamNumberAsync(actorUserId, cancellationToken),
                EncounterId = encounter.Id,
                PatientId = encounter.PatientId,
                ExaminerWorkforceProfileId = pemeriksa.WorkforceProfileId,
                ExaminerLicenseNumberSnapshot = pemeriksa.SipNumber,
                Status = OptRefractionExamStatus.Draft,
                CurrentRevisionNo = 1,
                StartedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };
            var revisiPertama = new OptRefractionExamRevision
            {
                RevisionNo = 1,
                RevisedByWorkforceProfileId = pemeriksa.WorkforceProfileId,
                RevisedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
            };
            exam.Revisions.Add(revisiPertama);

            _dbContext.OptRefractionExams.Add(exam);
            await _integrity.RegisterAsync(DocumentKind, revisiPertama.Id, exam.PatientId, exam.EncounterId, actorUserId, cancellationToken: cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return OptResult<OptRefractionExamResponse>.Ok(await ProjectAsync(exam, cancellationToken));
        }

        /// <summary>Isi atau ganti hasil ukur selama <c>Draft</c>.</summary>
        public async Task<OptResult<OptRefractionExamResponse>> UpdateMeasurementsAsync(Guid id, OptRefractionExamMeasurementRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var exam = await LoadTrackedAsync(id, cancellationToken);
            if (exam is null)
                return OptResult<OptRefractionExamResponse>.NotFound(NotFoundMessage);

            var current = exam.Revisions.Single(r => r.RevisionNo == exam.CurrentRevisionNo);
            var tolak = RejectUnlessDraft(exam) ?? RejectStale(exam, request.ExpectedVersion)
                ?? await RejectIfMedicalRecordLockedAsync(current, cancellationToken) ?? ValidateMeasurements(request);
            if (tolak is not null)
                return tolak;

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var now = Now();
            CopyValues(request, current);
            current.RevisedByWorkforceProfileId = practitioner.Value!.WorkforceProfileId;
            current.RevisedAt = now;
            current.UpdateDateTime = now;
            current.UpdateBy = actorUserId;
            Touch(exam, actorUserId, now);

            return await SaveAsync(exam, cancellationToken);
        }

        /// <summary>Simpan final. Tidak menerbitkan tagihan (<c>BE-OPT-031</c>).</summary>
        public async Task<OptResult<OptRefractionExamResponse>> FinalizeAsync(Guid id, OptRefractionExamFinalizeRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var exam = await LoadTrackedAsync(id, cancellationToken);
            if (exam is null)
                return OptResult<OptRefractionExamResponse>.NotFound(NotFoundMessage);

            var current = exam.Revisions.Single(r => r.RevisionNo == exam.CurrentRevisionNo);
            var tolak = RejectUnlessDraft(exam) ?? RejectStale(exam, request.ExpectedVersion)
                ?? await RejectIfMedicalRecordLockedAsync(current, cancellationToken) ?? RequireDistanceVisus(current);
            if (tolak is not null)
                return tolak;

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var now = Now();
            exam.Status = OptRefractionExamStatus.Final;
            exam.FinalizedAt = now;
            exam.FinalizedBy = actorUserId;
            Touch(exam, actorUserId, now);

            var gagal = await SignAsync(exam, current.Id, actorUserId, now, cancellationToken);
            return gagal ?? await SaveAsync(exam, cancellationToken);
        }

        /// <summary>Revisi sesudah final: menambah versi baru; versi lama dan tagihan tidak berubah (<c>OPT-DEC-047</c>).</summary>
        public async Task<OptResult<OptRefractionExamResponse>> ReviseAsync(Guid id, OptRefractionExamReviseRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var exam = await LoadTrackedAsync(id, cancellationToken);
            if (exam is null)
                return OptResult<OptRefractionExamResponse>.NotFound(NotFoundMessage);

            var tolak = exam.Status switch
            {
                OptRefractionExamStatus.Cancelled => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.InvalidState, CancelledMessage),
                OptRefractionExamStatus.Draft => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.InvalidState, NotFinalMessage),
                _ => null,
            };
            tolak ??= RejectStale(exam, request.ExpectedVersion);
            if (tolak is null && string.IsNullOrWhiteSpace(request.RevisionReason))
                tolak = OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.InvalidRequest, RevisionReasonMessage);
            tolak ??= ValidateMeasurements(request);

            var revisi = new OptRefractionExamRevision();
            if (tolak is null)
            {
                CopyValues(request, revisi);
                tolak = RequireDistanceVisus(revisi);
            }
            if (tolak is not null)
                return tolak;

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var now = Now();
            revisi.RefractionExamId = exam.Id;
            revisi.RevisionNo = exam.CurrentRevisionNo + 1;
            revisi.RevisedByWorkforceProfileId = practitioner.Value!.WorkforceProfileId;
            revisi.RevisedAt = now;
            revisi.RevisionReason = request.RevisionReason!.Trim();
            revisi.CreateDateTime = now;
            revisi.CreateBy = actorUserId;
            // Ditambahkan lewat DbSet: Id Guid sudah terisi, sehingga lewat koleksi saja EF menganggapnya baris lama (UPDATE 0 baris).
            _dbContext.OptRefractionExamRevisions.Add(revisi);
            exam.CurrentRevisionNo = revisi.RevisionNo;
            Touch(exam, actorUserId, now);

            var gagal = await SignAsync(exam, revisi.Id, actorUserId, now, cancellationToken);
            return gagal ?? await SaveAsync(exam, cancellationToken);
        }

        /// <summary>Batalkan draf. Pemeriksaan final ditolak <c>OPT-VAL-033</c>.</summary>
        public async Task<OptResult<OptRefractionExamResponse>> CancelAsync(Guid id, OptRefractionExamCancelRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var exam = await LoadTrackedAsync(id, cancellationToken);
            if (exam is null)
                return OptResult<OptRefractionExamResponse>.NotFound(NotFoundMessage);

            var tolak = exam.Status switch
            {
                OptRefractionExamStatus.Final => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.Val033, CancelFinalMessage),
                OptRefractionExamStatus.Cancelled => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.InvalidState, CancelledMessage),
                _ => null,
            };
            tolak ??= RejectStale(exam, request.ExpectedVersion);
            if (tolak is null && string.IsNullOrWhiteSpace(request.Reason))
                tolak = OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.InvalidRequest, CancelReasonMessage);
            if (tolak is not null)
                return tolak;

            var practitioner = await _guard.CheckAsync(actorUserId, Today(), cancellationToken);
            if (!practitioner.IsSuccess)
                return Relay(practitioner);

            var now = Now();
            exam.Status = OptRefractionExamStatus.Cancelled;
            exam.CancelReason = request.Reason!.Trim();
            exam.IsCancel = true;
            exam.CancelDateTime = now;
            exam.CancelBy = actorUserId;
            Touch(exam, actorUserId, now);

            var draf = exam.Revisions.Single(r => r.RevisionNo == exam.CurrentRevisionNo);
            await _integrity.MarkCancelledAsync(DocumentKind, draf.Id, actorUserId, exam.CancelReason, now, cancellationToken);
            return await SaveAsync(exam, cancellationToken);
        }

        // ---------------------------------------------------------------- validasi

        /// <summary>Aturan isian hasil ukur: notasi visus (<c>OPT-VAL-034</c>), CYL minus, AXIS, dan batas penyimpanan.</summary>
        public static OptResult<OptRefractionExamResponse>? ValidateMeasurements(OptRefractionMeasurementValues v)
        {
            foreach (var visus in new[]
                     {
                         v.VisusDistanceUncorrectedOd, v.VisusDistanceUncorrectedOs, v.VisusDistanceCorrectedOd, v.VisusDistanceCorrectedOs,
                         v.VisusNearUncorrectedOd, v.VisusNearUncorrectedOs, v.VisusNearCorrectedOd, v.VisusNearCorrectedOs,
                     })
            {
                if (!string.IsNullOrWhiteSpace(visus) && !VisusPattern.IsMatch(visus.Trim()))
                    return OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.Val034, VisusFormatMessage);
            }

            return ValidateSphCylAxis("autorefraktometer mata kanan", v.AutorefOdSph, v.AutorefOdCyl, v.AutorefOdAxis)
                ?? ValidateSphCylAxis("autorefraktometer mata kiri", v.AutorefOsSph, v.AutorefOsCyl, v.AutorefOsAxis)
                ?? ValidateSphCylAxis("koreksi subjektif mata kanan", v.SubjectiveOdSph, v.SubjectiveOdCyl, v.SubjectiveOdAxis)
                ?? ValidateSphCylAxis("koreksi subjektif mata kiri", v.SubjectiveOsSph, v.SubjectiveOsCyl, v.SubjectiveOsAxis)
                ?? ValidateAdd("mata kanan", v.SubjectiveOdAdd)
                ?? ValidateAdd("mata kiri", v.SubjectiveOsAdd)
                ?? ValidatePd("jauh", v.PdDistance)
                ?? ValidatePd("dekat", v.PdNear);
        }

        private static OptResult<OptRefractionExamResponse>? ValidateSphCylAxis(string label, decimal? sph, decimal? cyl, int? axis)
        {
            // Batas penyimpanan kolom decimal(5,2).
            if (sph is { } s && (Math.Abs(s) >= 1000m || decimal.Round(s, 2) != s))
                return Invalid($"SPH {label} tidak sah.");
            if (cyl is { } c && (c < -999.99m || decimal.Round(c, 2) != c))
                return Invalid($"CYL {label} tidak sah.");
            if (cyl > 0m)
                return Invalid($"CYL {label} harus notasi minus. Transposisikan nilai plus lebih dulu.");
            if (cyl is { } ada && ada != 0m && axis is null)
                return Invalid($"AXIS {label} wajib diisi bila ada silinder.");
            if (axis is { } a && (a < 0 || a > 180 || cyl is null || cyl == 0m))
                return Invalid($"AXIS {label} harus 0–180 dan hanya diisi bila ada silinder.");
            return null;
        }

        private static OptResult<OptRefractionExamResponse>? ValidateAdd(string label, decimal? add)
        {
            // Batas penyimpanan kolom decimal(4,2); ADD selalu positif.
            if (add is { } x && (x <= 0m || x >= 100m || decimal.Round(x, 2) != x))
                return Invalid($"ADD {label} harus bernilai positif.");
            return null;
        }

        private static OptResult<OptRefractionExamResponse>? ValidatePd(string label, decimal? pd)
        {
            // Batas penyimpanan kolom decimal(4,1).
            if (pd is { } x && (x <= 0m || x >= 1000m || decimal.Round(x, 1) != x))
                return Invalid($"PD {label} tidak sah; tulis dalam mm dengan paling banyak satu desimal.");
            return null;
        }

        /// <summary><c>OPT-VAL-031</c>: visus jauh mata kanan dan kiri, dengan atau tanpa koreksi.</summary>
        private static OptResult<OptRefractionExamResponse>? RequireDistanceVisus(OptRefractionExamRevision r)
        {
            var od = !string.IsNullOrWhiteSpace(r.VisusDistanceUncorrectedOd) || !string.IsNullOrWhiteSpace(r.VisusDistanceCorrectedOd);
            var os = !string.IsNullOrWhiteSpace(r.VisusDistanceUncorrectedOs) || !string.IsNullOrWhiteSpace(r.VisusDistanceCorrectedOs);
            return od && os ? null : OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.Val031, FinalMinimumMessage);
        }

        private static OptResult<OptRefractionExamResponse>? RejectUnlessDraft(OptRefractionExam exam) => exam.Status switch
        {
            OptRefractionExamStatus.Final => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.Val032, AlreadyFinalMessage),
            OptRefractionExamStatus.Cancelled => OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.InvalidState, CancelledMessage),
            _ => null,
        };

        private static OptResult<OptRefractionExamResponse>? RejectStale(OptRefractionExam exam, int? expectedVersion) =>
            expectedVersion is null
                ? OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.InvalidRequest, "ExpectedVersion wajib diisi.")
                : expectedVersion != exam.Version
                    ? OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage)
                    : null;

        private static OptResult<OptRefractionExamResponse> Invalid(string message) =>
            OptResult<OptRefractionExamResponse>.Invalid(OptErrorCodes.InvalidRequest, message);

        private static OptResult<OptRefractionExamResponse> Relay(OptResult<OptPractitioner> failure) => failure.Kind switch
        {
            OptResultKind.Unavailable => OptResult<OptRefractionExamResponse>.Unavailable(failure.Code!, failure.Message!),
            _ => OptResult<OptRefractionExamResponse>.Forbidden(failure.Code!, failure.Message!),
        };

        // ---------------------------------------------------------------- pembantu

        private DateTime Now() => _time.GetUtcNow().UtcDateTime;

        /// <summary>Draf yang dikunci Rekam Medis (kunjungan ditutup) tidak boleh diisi atau difinalkan lagi.</summary>
        private async Task<OptResult<OptRefractionExamResponse>?> RejectIfMedicalRecordLockedAsync(OptRefractionExamRevision draf, CancellationToken cancellationToken)
        {
            var kunci = await _integrity.EnsureDraftStillOpenAsync(DocumentKind, draf.Id, cancellationToken);
            return kunci.IsAllowed
                ? null
                : OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.MedicalRecordLocked,
                    kunci.StatusCode == StatusCodes.Status409Conflict ? MedicalRecordLockedMessage : kunci.ErrorMessage ?? MedicalRecordLockedMessage);
        }

        /// <summary>Mendaftarkan versi sebagai dokumen RM bertanda tangan, di SaveChanges yang sama dengan pemeriksaannya.</summary>
        private async Task<OptResult<OptRefractionExamResponse>?> SignAsync(OptRefractionExam exam, Guid revisionId, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var http = _httpContextAccessor.HttpContext;
            try
            {
                await _integrity.RegisterSignedAsync(DocumentKind, revisionId, exam.PatientId, exam.EncounterId, actorUserId,
                    http?.Request.Headers.UserAgent.ToString(), http?.Connection.RemoteIpAddress?.ToString(), now, cancellationToken);
                return null;
            }
            catch (InvalidOperationException ex)
            {
                // Belum ada yang tersimpan; perubahan yang terlacak dibuang agar tidak ikut tersimpan belakangan.
                _dbContext.ChangeTracker.Clear();
                return OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.MedicalRecordRegistrationFailed,
                    $"Pemeriksaan tidak tersimpan karena pendaftaran ke rekam medis gagal: {ex.Message}");
            }
        }

        private DateTime Today() => OptPractitionerLicenseReader.ToOperationalDate(_time.GetUtcNow());

        private Task<OptRefractionExam?> LoadTrackedAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.OptRefractionExams
                .Include(x => x.Revisions)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private static void Touch(OptRefractionExam exam, Guid actorUserId, DateTime now)
        {
            exam.Version += 1;
            exam.UpdateDateTime = now;
            exam.UpdateBy = actorUserId;
        }

        private async Task<OptResult<OptRefractionExamResponse>> SaveAsync(OptRefractionExam exam, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return OptResult<OptRefractionExamResponse>.Conflict(OptErrorCodes.ConcurrencyConflict, StaleMessage);
            }

            return OptResult<OptRefractionExamResponse>.Ok(await ProjectAsync(exam, cancellationToken));
        }

        private static void CopyValues(OptRefractionMeasurementValues src, OptRefractionExamRevision dst)
        {
            dst.VisusDistanceUncorrectedOd = Clean(src.VisusDistanceUncorrectedOd);
            dst.VisusDistanceUncorrectedOs = Clean(src.VisusDistanceUncorrectedOs);
            dst.VisusDistanceCorrectedOd = Clean(src.VisusDistanceCorrectedOd);
            dst.VisusDistanceCorrectedOs = Clean(src.VisusDistanceCorrectedOs);
            dst.VisusNearUncorrectedOd = Clean(src.VisusNearUncorrectedOd);
            dst.VisusNearUncorrectedOs = Clean(src.VisusNearUncorrectedOs);
            dst.VisusNearCorrectedOd = Clean(src.VisusNearCorrectedOd);
            dst.VisusNearCorrectedOs = Clean(src.VisusNearCorrectedOs);
            dst.AutorefOdSph = src.AutorefOdSph;
            dst.AutorefOdCyl = src.AutorefOdCyl;
            dst.AutorefOdAxis = src.AutorefOdAxis;
            dst.AutorefOsSph = src.AutorefOsSph;
            dst.AutorefOsCyl = src.AutorefOsCyl;
            dst.AutorefOsAxis = src.AutorefOsAxis;
            dst.SubjectiveOdSph = src.SubjectiveOdSph;
            dst.SubjectiveOdCyl = src.SubjectiveOdCyl;
            dst.SubjectiveOdAxis = src.SubjectiveOdAxis;
            dst.SubjectiveOdAdd = src.SubjectiveOdAdd;
            dst.SubjectiveOsSph = src.SubjectiveOsSph;
            dst.SubjectiveOsCyl = src.SubjectiveOsCyl;
            dst.SubjectiveOsAxis = src.SubjectiveOsAxis;
            dst.SubjectiveOsAdd = src.SubjectiveOsAdd;
            dst.PdDistance = src.PdDistance;
            dst.PdNear = src.PdNear;
            dst.KeratometryNote = Clean(src.KeratometryNote);
            dst.ClinicalNote = Clean(src.ClinicalNote);
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private async Task<OptRefractionExamResponse> ProjectAsync(OptRefractionExam exam, CancellationToken cancellationToken)
        {
            var revisions = exam.Revisions.Where(r => !r.IsDelete).ToList();
            var names = await NamesAsync(revisions.Select(r => r.RevisedByWorkforceProfileId).Append(exam.ExaminerWorkforceProfileId), cancellationToken);
            return Project(exam, revisions, names, includeAllRevisions: true);
        }

        private async Task<Dictionary<Guid, string>> NamesAsync(IEnumerable<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var ids = workforceProfileIds.Distinct().ToList();
            return await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private static OptRefractionExamResponse Project(
            OptRefractionExam exam,
            IReadOnlyCollection<OptRefractionExamRevision> revisions,
            IReadOnlyDictionary<Guid, string> names,
            bool includeAllRevisions)
        {
            var semua = revisions.OrderBy(r => r.RevisionNo).Select(r => ProjectRevision(r, names)).ToList();
            return new OptRefractionExamResponse
            {
                Id = exam.Id,
                ExamNumber = exam.ExamNumber,
                EncounterId = exam.EncounterId,
                PatientId = exam.PatientId,
                ExaminerWorkforceProfileId = exam.ExaminerWorkforceProfileId,
                ExaminerName = names.GetValueOrDefault(exam.ExaminerWorkforceProfileId, string.Empty),
                ExaminerLicenseNumberSnapshot = exam.ExaminerLicenseNumberSnapshot,
                Status = exam.Status,
                StatusLabel = OptLabels.RefractionExamStatus(exam.Status),
                CurrentRevisionNo = exam.CurrentRevisionNo,
                StartedAt = exam.StartedAt,
                FinalizedAt = exam.FinalizedAt,
                CancelReason = exam.CancelReason,
                Version = exam.Version,
                Current = semua.FirstOrDefault(r => r.RevisionNo == exam.CurrentRevisionNo),
                Revisions = includeAllRevisions ? semua : new List<OptRefractionExamRevisionResponse>(),
            };
        }

        private static OptRefractionExamRevisionResponse ProjectRevision(OptRefractionExamRevision r, IReadOnlyDictionary<Guid, string> names) => new()
        {
            Id = r.Id,
            RevisionNo = r.RevisionNo,
            RevisedByWorkforceProfileId = r.RevisedByWorkforceProfileId,
            RevisedByName = names.GetValueOrDefault(r.RevisedByWorkforceProfileId, string.Empty),
            RevisedAt = r.RevisedAt,
            RevisionReason = r.RevisionReason,
            VisusDistanceUncorrectedOd = r.VisusDistanceUncorrectedOd,
            VisusDistanceUncorrectedOs = r.VisusDistanceUncorrectedOs,
            VisusDistanceCorrectedOd = r.VisusDistanceCorrectedOd,
            VisusDistanceCorrectedOs = r.VisusDistanceCorrectedOs,
            VisusNearUncorrectedOd = r.VisusNearUncorrectedOd,
            VisusNearUncorrectedOs = r.VisusNearUncorrectedOs,
            VisusNearCorrectedOd = r.VisusNearCorrectedOd,
            VisusNearCorrectedOs = r.VisusNearCorrectedOs,
            AutorefOdSph = r.AutorefOdSph,
            AutorefOdCyl = r.AutorefOdCyl,
            AutorefOdAxis = r.AutorefOdAxis,
            AutorefOsSph = r.AutorefOsSph,
            AutorefOsCyl = r.AutorefOsCyl,
            AutorefOsAxis = r.AutorefOsAxis,
            SubjectiveOdSph = r.SubjectiveOdSph,
            SubjectiveOdCyl = r.SubjectiveOdCyl,
            SubjectiveOdAxis = r.SubjectiveOdAxis,
            SubjectiveOdAdd = r.SubjectiveOdAdd,
            SubjectiveOsSph = r.SubjectiveOsSph,
            SubjectiveOsCyl = r.SubjectiveOsCyl,
            SubjectiveOsAxis = r.SubjectiveOsAxis,
            SubjectiveOsAdd = r.SubjectiveOsAdd,
            PdDistance = r.PdDistance,
            PdNear = r.PdNear,
            KeratometryNote = r.KeratometryNote,
            ClinicalNote = r.ClinicalNote,
        };
    }
}
