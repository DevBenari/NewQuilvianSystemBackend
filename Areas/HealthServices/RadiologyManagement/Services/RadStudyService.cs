using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Siklus hidup study radiologi sesuai <c>RJ-BIL-GATE-DEC-004</c>.
    ///
    /// Tiga hal yang ditegakkan di sini, dan ketiganya adalah acceptance criteria
    /// <c>RJ-BIL-BE-004</c>:
    ///
    /// <list type="number">
    /// <item>Acquisition ditolak selama identitas belum diverifikasi atau gerbang keselamatan
    /// wajib belum tuntas — termasuk ketika aturannya belum ditetapkan sama sekali.</item>
    /// <item>Kelayakan tagih normal hanya terbit untuk acquisition yang benar-benar dikerjakan
    /// dan menghasilkan citra yang dapat dipakai. Status apa pun sebelum itu tidak menagih.</item>
    /// <item>Pengulangan membuat study baru dan mempertahankan study aslinya utuh, beserta
    /// sebab dan tanggung jawabnya.</item>
    /// </list>
    /// </summary>
    public class RadStudyService
    {
        private const string LogCategory = "HealthServices.RadiologyManagement";
        private const string ExaminationUnit = "Examination";

        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public RadStudyService(
            ApplicationDbContext dbContext,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        /* ================================================================ *
         * Pembuatan study
         * ================================================================ */

        /// <summary>
        /// Membuat study baru pada sebuah pesanan, lengkap dengan baris pemeriksaan keselamatan
        /// yang berlaku saat itu.
        ///
        /// Baris keselamatan dibuat di muka, bukan saat acquisition hendak dimulai. Petugas
        /// perlu tahu apa saja yang harus diselesaikan sebelum pasien masuk ruangan, bukan
        /// setelah ia sudah berbaring di meja pemeriksaan.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> CreateStudyAsync(
            Guid radOrderId,
            CreateRadStudyRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var order = await _dbContext.RadOrders
                .Include(x => x.Studies.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == radOrderId && !x.IsDelete, cancellationToken);

            if (order == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.OrderNotFound,
                    "Pesanan radiologi tidak ditemukan.");
            }

            if (order.OrderStatus is RadOrderStatus.Cancelled or RadOrderStatus.Rejected)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Pesanan yang sudah dibatalkan atau ditolak tidak dapat menambah study baru.");
            }

            var procedureId = request?.ProcedureId ?? order.ProcedureId;
            var sequence = order.Studies.Count == 0
                ? 1
                : order.Studies.Max(x => x.StudySequence) + 1;

            var study = new RadStudy
            {
                RadOrderId = order.Id,
                EncounterId = order.EncounterId,
                ProcedureId = procedureId,
                ModalityId = order.ModalityId,
                StudySequence = sequence,
                StudyNumber = BuildStudyNumber(order.Id, sequence),
                StudyStatus = RadStudyStatus.Planned,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            await AttachSafetyChecksAsync(study, order.ModalityId, procedureId, actorUserId, now, cancellationToken);

            _dbContext.RadStudies.Add(study);

            AddHistory(order, study, "Study.Create", null, RadStudyStatus.Planned.ToString(),
                null, null, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /// <summary>
        /// Menyalin butir keselamatan yang berlaku menjadi baris pemeriksaan milik study.
        ///
        /// Kode, nama, kewajiban, dan versi aturan dibekukan pada baris. Master data boleh
        /// berubah kemudian; apa yang ditanyakan hari itu tidak boleh ikut berubah.
        /// </summary>
        private async Task AttachSafetyChecksAsync(
            RadStudy study,
            Guid modalityId,
            Guid procedureId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var rules = await LoadApplicableRulesAsync(modalityId, procedureId, now, cancellationToken);

            foreach (var rule in rules)
            {
                study.SafetyChecks.Add(new RadStudySafetyCheck
                {
                    RadStudyId = study.Id,
                    SafetyRequirementId = rule.SafetyRequirementId,
                    RequirementCodeSnapshot = rule.SafetyRequirement?.RequirementCode ?? string.Empty,
                    RequirementNameSnapshot = rule.SafetyRequirement?.RequirementName ?? string.Empty,
                    IsMandatorySnapshot = rule.IsMandatory,
                    RuleVersionSnapshot = rule.RuleVersion,
                    CheckState = RadSafetyCheckState.Pending,
                    CreateBy = actorUserId,
                    CreateDateTime = now,
                });
            }
        }

        /// <summary>
        /// Aturan keselamatan yang berlaku untuk sebuah modalitas dan pemeriksaan pada saat ini.
        ///
        /// Aturan khusus pemeriksaan dan aturan seluruh modalitas sama-sama ikut. Keduanya
        /// menumpuk, tidak saling menggantikan: aturan yang lebih spesifik menambah pertanyaan,
        /// bukan menghapus pertanyaan yang lebih umum.
        /// </summary>
        private async Task<List<MstRadModalitySafetyRule>> LoadApplicableRulesAsync(
            Guid modalityId,
            Guid procedureId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            return await _dbContext.MstRadModalitySafetyRules
                .Include(x => x.SafetyRequirement)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ModalityId == modalityId &&
                    (x.ProcedureId == null || x.ProcedureId == procedureId) &&
                    x.EffectiveFrom <= now &&
                    (x.EffectiveTo == null || x.EffectiveTo > now))
                .ToListAsync(cancellationToken);
        }

        /* ================================================================ *
         * Gerbang identitas dan keselamatan
         * ================================================================ */

        /// <summary>
        /// Menyatakan identitas pasien, kunjungan, pemeriksaan, dan modalitas sudah diverifikasi.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> VerifyPatientAsync(
            Guid studyId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.StudyStatus != RadStudyStatus.Planned)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Verifikasi identitas hanya dapat dilakukan pada study berstatus Planned; " +
                    $"study ini berstatus {study.StudyStatus}.");
            }

            var from = study.StudyStatus;
            study.StudyStatus = RadStudyStatus.PatientVerified;
            study.PatientVerifiedAt = now;
            study.PatientVerifiedByUserId = actorUserId;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.VerifyPatient", from.ToString(),
                study.StudyStatus.ToString(), null, null, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /// <summary>
        /// Menjawab satu butir gerbang keselamatan.
        ///
        /// Jawaban boleh diubah selama acquisition belum dimulai. Setelah dimulai, jawaban
        /// dibekukan: mengubah catatan keselamatan setelah pasien disinari berarti menulis ulang
        /// sejarah, dan itu justru yang paling perlu dicegah pada modul ini.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> DecideSafetyCheckAsync(
            Guid studyId,
            RadSafetyCheckDecisionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.AcquisitionStartedAt != null)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Jawaban gerbang keselamatan tidak dapat diubah setelah acquisition dimulai.");
            }

            var check = study.SafetyChecks.FirstOrDefault(
                x => x.SafetyRequirementId == request.SafetyRequirementId && !x.IsDelete);

            if (check == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.SafetyGateNotCleared,
                    "Butir keselamatan yang dimaksud tidak berlaku untuk study ini.");
            }

            check.CheckState = request.CheckState;
            check.DecidedAt = now;
            check.DecidedByUserId = actorUserId;
            check.Note = request.Note;
            check.UpdateBy = actorUserId;
            check.UpdateDateTime = now;
            check.Version += 1;

            AddHistory(study.RadOrder!, study, "Study.DecideSafetyCheck",
                null, request.CheckState.ToString(),
                check.RequirementCodeSnapshot, request.Note, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /// <summary>
        /// Menyatakan gerbang keselamatan tuntas.
        ///
        /// Penilaiannya diserahkan sepenuhnya kepada <see cref="RadSafetyGateEvaluator"/>, yang
        /// murni dan diuji terpisah. Service ini hanya menerjemahkan hasilnya menjadi status dan
        /// pesan.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> ClearSafetyAsync(
            Guid studyId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.StudyStatus != RadStudyStatus.PatientVerified)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.IdentityNotVerified,
                    "Gerbang keselamatan hanya dapat dinyatakan tuntas setelah identitas " +
                    "pasien, kunjungan, pemeriksaan, dan modalitas diverifikasi.");
            }

            var rules = await LoadApplicableRulesAsync(
                study.ModalityId, study.ProcedureId, now, cancellationToken);

            var outcome = RadSafetyGateEvaluator.Evaluate(rules, study.SafetyChecks.ToList());

            if (!outcome.PolicyConfigured)
            {
                return RadOperationResult<RadStudyResponse>.PolicyNotConfigured(
                    RadErrorCodes.SafetyPolicyNotConfigured,
                    RadSafetyGateEvaluator.DescribeBlockage(outcome));
            }

            if (!outcome.Cleared)
            {
                return RadOperationResult<RadStudyResponse>.SafetyBlocked(
                    RadErrorCodes.SafetyGateNotCleared,
                    RadSafetyGateEvaluator.DescribeBlockage(outcome));
            }

            var from = study.StudyStatus;
            study.StudyStatus = RadStudyStatus.SafetyCleared;
            study.SafetyClearedAt = now;
            study.SafetyClearedByUserId = actorUserId;
            study.SafetyRuleVersionAtClearance = outcome.RuleVersion;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.ClearSafety", from.ToString(),
                study.StudyStatus.ToString(), null, null, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /* ================================================================ *
         * Acquisition
         * ================================================================ */

        /// <summary>
        /// Memulai acquisition.
        ///
        /// <b>Inilah acceptance criteria 1.</b> Gerbang diperiksa ulang di sini, bukan hanya
        /// dipercaya dari status <c>SafetyCleared</c>. Alasannya: aturan dapat berubah, jawaban
        /// dapat berubah, dan jarak waktu antara "dinyatakan lolos" dan "mesin dinyalakan" bisa
        /// panjang. Memeriksa ulang murah; melewatkannya tidak.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> StartAcquisitionAsync(
            Guid studyId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.PatientVerifiedAt == null)
            {
                return RadOperationResult<RadStudyResponse>.SafetyBlocked(
                    RadErrorCodes.IdentityNotVerified,
                    "Acquisition ditolak: identitas pasien, kunjungan, pemeriksaan, dan " +
                    "modalitas belum diverifikasi.");
            }

            if (study.StudyStatus != RadStudyStatus.SafetyCleared)
            {
                return RadOperationResult<RadStudyResponse>.SafetyBlocked(
                    RadErrorCodes.SafetyGateNotCleared,
                    $"Acquisition ditolak: study berstatus {study.StudyStatus}, bukan SafetyCleared.");
            }

            // Pemeriksaan ulang. Status SafetyCleared adalah catatan masa lalu; yang menentukan
            // sekarang adalah keadaan gerbangnya saat ini.
            var rules = await LoadApplicableRulesAsync(
                study.ModalityId, study.ProcedureId, now, cancellationToken);

            var outcome = RadSafetyGateEvaluator.Evaluate(rules, study.SafetyChecks.ToList());

            if (!outcome.PolicyConfigured)
            {
                return RadOperationResult<RadStudyResponse>.PolicyNotConfigured(
                    RadErrorCodes.SafetyPolicyNotConfigured,
                    RadSafetyGateEvaluator.DescribeBlockage(outcome));
            }

            if (!outcome.Cleared)
            {
                return RadOperationResult<RadStudyResponse>.SafetyBlocked(
                    RadErrorCodes.SafetyGateNotCleared,
                    RadSafetyGateEvaluator.DescribeBlockage(outcome));
            }

            var from = study.StudyStatus;
            study.StudyStatus = RadStudyStatus.AcquisitionStarted;
            study.AcquisitionStartedAt = now;
            study.AcquisitionStartedByUserId = actorUserId;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.StartAcquisition", from.ToString(),
                study.StudyStatus.ToString(), null, null, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /// <summary>Menandai acquisition selesai dikerjakan; kualitasnya belum dinilai.</summary>
        public async Task<RadOperationResult<RadStudyResponse>> CompleteAcquisitionAsync(
            Guid studyId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.StudyStatus != RadStudyStatus.AcquisitionStarted)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Acquisition hanya dapat diselesaikan dari status AcquisitionStarted; " +
                    $"study ini berstatus {study.StudyStatus}.");
            }

            var from = study.StudyStatus;
            study.StudyStatus = RadStudyStatus.Acquired;
            study.AcquiredAt = now;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.CompleteAcquisition", from.ToString(),
                study.StudyStatus.ToString(), null, null, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /* ================================================================ *
         * Penilaian kualitas dan penerbitan fakta ke Billing
         * ================================================================ */

        /// <summary>
        /// Menilai apakah citra dapat dipakai, lalu menerbitkan fakta kelayakan tagih bila ya.
        ///
        /// <b>Inilah acceptance criteria 2.</b> Fakta hanya terbit untuk study yang benar-benar
        /// dikerjakan **dan** menghasilkan citra yang dapat dipakai. Citra yang ditolak
        /// kualitasnya tidak menerbitkan apa pun — <c>GATE-DEC-004</c> menyatakan kegagalan
        /// kualitas masuk alur pengecualian, bukan otomatis menjadi tagihan penuh.
        ///
        /// Penerbitan fakta dilakukan **setelah** perubahan klinis tersimpan, bukan di dalam
        /// transaksinya. <c>ClinicalMilestoneFactProducer</c> menolak dipanggil di dalam
        /// transaksi, dan alasannya masuk akal: fakta klinis yang sudah dikirim ke Billing tidak
        /// dapat ditarik kembali oleh rollback.
        /// </summary>
        public async Task<RadOperationResult<RadStudyActionResult>> DecideQualityAsync(
            Guid studyId,
            RadAcquisitionQualityRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyActionResult>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.StudyStatus != RadStudyStatus.Acquired)
            {
                return RadOperationResult<RadStudyActionResult>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Kualitas hanya dapat dinilai untuk study berstatus Acquired; " +
                    $"study ini berstatus {study.StudyStatus}.");
            }

            var from = study.StudyStatus;
            study.IsUsable = request.IsUsable;
            study.QualityDecidedAt = now;
            study.QualityDecidedByUserId = actorUserId;
            study.QualityNote = request.QualityNote;
            study.StudyStatus = request.IsUsable
                ? RadStudyStatus.QualityAccepted
                : RadStudyStatus.QualityRejected;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.DecideQuality", from.ToString(),
                study.StudyStatus.ToString(), null, request.QualityNote, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            // Citra yang tidak dapat dipakai tidak menerbitkan kelayakan tagih. Konsumsi bahan
            // yang terlanjur terpakai tetap tercatat pada barisnya sendiri, dan Billing yang
            // menilai akibatnya.
            if (!request.IsUsable)
            {
                return RadOperationResult<RadStudyActionResult>.Success(
                    new RadStudyActionResult(MapStudy(study), null));
            }

            var handoff = await EmitChargeEligibilityAsync(study, actorUserId, cancellationToken);

            if (handoff.Kind is ClinicalFactEmissionKind.Emitted or ClinicalFactEmissionKind.Replayed)
            {
                study.BillingFactSubmitted = true;
                study.BillingFactSubmittedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return RadOperationResult<RadStudyActionResult>.Success(
                new RadStudyActionResult(MapStudy(study), handoff));
        }

        /* ================================================================ *
         * Penghentian dan pengulangan
         * ================================================================ */

        /// <summary>
        /// Menghentikan acquisition di tengah jalan.
        ///
        /// Tidak menerbitkan pembatalan finansial apa pun secara otomatis. <c>GATE-DEC-004</c>
        /// menyatakan acquisition yang dihentikan tidak otomatis menjadi tagihan penuh maupun
        /// pembatalan penuh; yang menentukan adalah bagian yang sempat dikerjakan dan bahan yang
        /// terlanjur terpakai, dan penilaiannya milik Billing.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> AbortAcquisitionAsync(
            Guid studyId,
            RadAbortAcquisitionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(request?.AbortReason))
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.ReasonRequired,
                    "Penghentian acquisition wajib disertai alasan.");
            }

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.StudyStatus != RadStudyStatus.AcquisitionStarted)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    $"Hanya acquisition yang sedang berjalan yang dapat dihentikan; " +
                    $"study ini berstatus {study.StudyStatus}.");
            }

            var from = study.StudyStatus;
            study.StudyStatus = RadStudyStatus.Aborted;
            study.AbortCause = request.AbortCause;
            study.AbortReason = request.AbortReason;
            study.AbortedAt = now;
            study.PerformedPortionNote = request.PerformedPortionNote;
            study.IsUsable = false;
            Touch(study, actorUserId, now);

            AddHistory(study.RadOrder!, study, "Study.AbortAcquisition", from.ToString(),
                study.StudyStatus.ToString(), request.AbortCause.ToString(),
                request.AbortReason, actorUserId, now);

            await SaveWithConcurrencyGuardAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(study));
        }

        /// <summary>
        /// Mengulang sebuah study.
        ///
        /// <b>Inilah acceptance criteria 3.</b> Study asli **tidak disentuh sama sekali** —
        /// statusnya, waktunya, jawaban keselamatannya, dan konsumsi bahannya tetap utuh. Yang
        /// dibuat adalah study baru yang menunjuk ke study asli beserta sebab pengulangannya.
        ///
        /// Pertanyaan yang harus tetap terjawab setelah pengulangan adalah "berapa kali pasien
        /// ini sebenarnya disinari", dan satu-satunya cara menjawabnya adalah dengan tidak
        /// pernah menimpa study yang sudah terjadi.
        ///
        /// Pengulangan karena kebutuhan klinis baru wajib menyertakan pesanan tambahan yang sah.
        /// Pengulangan karena kesalahan internal rumah sakit tidak menuntutnya, dan sebabnya
        /// ikut terkirim ke Billing supaya tidak otomatis menambah tagihan pasien.
        /// </summary>
        public async Task<RadOperationResult<RadStudyResponse>> RepeatStudyAsync(
            Guid studyId,
            RadRepeatStudyRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(request?.RepeatReason))
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.ReasonRequired,
                    "Pengulangan wajib disertai alasan.");
            }

            var source = await LoadStudyAsync(studyId, cancellationToken);
            if (source == null)
            {
                return RadOperationResult<RadStudyResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            // Hanya study yang sudah benar-benar berjalan yang dapat diulang. Study yang belum
            // pernah dimulai tidak perlu diulang — ia cukup dilanjutkan.
            var dapatDiulang = source.StudyStatus is
                RadStudyStatus.Aborted or
                RadStudyStatus.QualityRejected or
                RadStudyStatus.RepeatRequired or
                RadStudyStatus.QualityAccepted;

            if (!dapatDiulang)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.RepeatSourceInvalid,
                    $"Study berstatus {source.StudyStatus} tidak dapat diulang. Pengulangan " +
                    "hanya berlaku untuk study yang acquisition-nya sudah pernah berjalan.");
            }

            if (request.RepeatCause == RadRepeatCause.NewClinicalRequirement &&
                request.AdditionalOrderId == null)
            {
                return RadOperationResult<RadStudyResponse>.Validation(
                    RadErrorCodes.RepeatAuthorizationRequired,
                    "Pengulangan karena kebutuhan klinis baru wajib menyertakan pesanan " +
                    "tambahan yang sah, bukan hanya alasan.");
            }

            var order = source.RadOrder!;

            var existingSequences = await _dbContext.RadStudies
                .Where(x => x.RadOrderId == order.Id && !x.IsDelete)
                .Select(x => x.StudySequence)
                .ToListAsync(cancellationToken);

            var sequence = existingSequences.Count == 0 ? 1 : existingSequences.Max() + 1;

            var repeat = new RadStudy
            {
                RadOrderId = order.Id,
                EncounterId = source.EncounterId,
                ProcedureId = source.ProcedureId,
                ModalityId = source.ModalityId,
                StudySequence = sequence,
                StudyNumber = BuildStudyNumber(order.Id, sequence),
                StudyStatus = RadStudyStatus.Planned,
                RepeatOfStudyId = source.Id,
                RepeatCause = request.RepeatCause,
                RepeatReason = request.RepeatReason,
                AdditionalOrderId = request.AdditionalOrderId,
                RepeatAuthorizedByUserId = actorUserId,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            await AttachSafetyChecksAsync(
                repeat, source.ModalityId, source.ProcedureId, actorUserId, now, cancellationToken);

            _dbContext.RadStudies.Add(repeat);

            // Study asli hanya ditandai bahwa ia menjadi asal pengulangan. Statusnya tidak
            // dipindahkan dan datanya tidak diubah.
            AddHistory(order, source, "Study.RepeatRequested", source.StudyStatus.ToString(),
                source.StudyStatus.ToString(), request.RepeatCause.ToString(),
                request.RepeatReason, actorUserId, now);

            AddHistory(order, repeat, "Study.CreateAsRepeat", null,
                RadStudyStatus.Planned.ToString(), request.RepeatCause.ToString(),
                request.RepeatReason, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RadOperationResult<RadStudyResponse>.Success(MapStudy(repeat));
        }

        /* ================================================================ *
         * Konsumsi bahan
         * ================================================================ */

        /// <summary>
        /// Mencatat bahan yang benar-benar terpakai pada sebuah acquisition.
        ///
        /// Boleh dicatat pada study yang gagal maupun yang berhasil. Kontras yang sudah
        /// disuntikkan tetap terpakai walau citranya gagal, dan menagih nol untuk itu sama
        /// salahnya dengan menagih penuh.
        /// </summary>
        public async Task<RadOperationResult<RadConsumptionResponse>> RecordConsumptionAsync(
            Guid studyId,
            RadConsumptionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var study = await LoadStudyAsync(studyId, cancellationToken);
            if (study == null)
            {
                return RadOperationResult<RadConsumptionResponse>.NotFound(
                    RadErrorCodes.StudyNotFound, "Study radiologi tidak ditemukan.");
            }

            if (study.AcquisitionStartedAt == null)
            {
                return RadOperationResult<RadConsumptionResponse>.Validation(
                    RadErrorCodes.InvalidTransition,
                    "Konsumsi bahan hanya dapat dicatat setelah acquisition dimulai.");
            }

            var consumption = new RadAcquisitionConsumption
            {
                RadStudyId = study.Id,
                ItemType = request.ItemType,
                ItemCode = request.ItemCode,
                ItemName = request.ItemName,
                Quantity = request.Quantity,
                Unit = request.Unit,
                ConsumedDespiteFailure = request.ConsumedDespiteFailure,
                RecordedByUserId = actorUserId,
                RecordedAt = now,
                Note = request.Note,
                CreateBy = actorUserId,
                CreateDateTime = now,
            };

            _dbContext.RadAcquisitionConsumptions.Add(consumption);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RadOperationResult<RadConsumptionResponse>.Success(MapConsumption(consumption));
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        /// <summary>
        /// Daftar modalitas beserta penanda apakah aturan keselamatannya sudah ditetapkan.
        ///
        /// Penanda itu perlu terlihat sebelum pasien dipanggil. Modalitas tanpa aturan aktif
        /// akan menolak setiap acquisition, dan mengetahuinya di depan jauh lebih baik daripada
        /// mengetahuinya ketika pasien sudah berbaring di meja pemeriksaan.
        /// </summary>
        public async Task<List<RadModalityResponse>> GetModalitiesAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.MstRadModalities
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .Select(x => new RadModalityResponse
                {
                    Id = x.Id,
                    ModalityCode = x.ModalityCode,
                    ModalityName = x.ModalityName,
                    UsesIonisingRadiation = x.UsesIonisingRadiation,
                    SupportsContrast = x.SupportsContrast,
                    HasActiveSafetyRule = x.SafetyRules.Any(r =>
                        !r.IsDelete && r.IsActive &&
                        r.EffectiveFrom <= now &&
                        (r.EffectiveTo == null || r.EffectiveTo > now)),
                    IsActive = x.IsActive,
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Katalog butir keselamatan yang dikenal sistem.
        ///
        /// Ini kosakata, bukan kebijakan. <c>SourceNote</c> ikut dikirim supaya pembacanya tahu
        /// bahwa daftar bawaan adalah baseline implementasi, bukan SOP yang sudah disahkan.
        /// </summary>
        public async Task<List<RadSafetyRequirementResponse>> GetSafetyRequirementsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.MstRadSafetyRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .Select(x => new RadSafetyRequirementResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    Category = x.Category,
                    Description = x.Description,
                    RequiresNote = x.RequiresNote,
                    SourceNote = x.SourceNote,
                    IsActive = x.IsActive,
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RadStudyResponse>> GetByOrderAsync(
            Guid radOrderId,
            CancellationToken cancellationToken = default)
        {
            var studies = await _dbContext.RadStudies
                .AsNoTracking()
                .Include(x => x.SafetyChecks.Where(c => !c.IsDelete))
                .Include(x => x.Consumptions.Where(c => !c.IsDelete))
                .Where(x => x.RadOrderId == radOrderId && !x.IsDelete)
                .OrderBy(x => x.StudySequence)
                .ToListAsync(cancellationToken);

            return studies.Select(MapStudy).ToList();
        }

        public async Task<List<RadTransitionHistoryResponse>> GetHistoryAsync(
            Guid radOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.RadTransitionHistories
                .AsNoTracking()
                .Where(x => x.RadOrderId == radOrderId && !x.IsDelete)
                .OrderBy(x => x.OccurredAt)
                .Select(x => new RadTransitionHistoryResponse
                {
                    Id = x.Id,
                    RadOrderId = x.RadOrderId,
                    RadStudyId = x.RadStudyId,
                    Scope = x.Scope.ToString(),
                    Action = x.Action,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ReasonCode = x.ReasonCode,
                    ReasonNote = x.ReasonNote,
                    ActorUserId = x.ActorUserId,
                    OccurredAt = x.OccurredAt,
                })
                .ToListAsync(cancellationToken);
        }

        /* ================================================================ *
         * Pembantu
         * ================================================================ */

        private Task<RadStudy?> LoadStudyAsync(Guid studyId, CancellationToken cancellationToken)
        {
            return _dbContext.RadStudies
                .Include(x => x.RadOrder)
                .Include(x => x.SafetyChecks.Where(c => !c.IsDelete))
                .Include(x => x.Consumptions.Where(c => !c.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == studyId && !x.IsDelete, cancellationToken);
        }

        private static void Touch(RadStudy study, Guid actorUserId, DateTime now)
        {
            study.UpdateBy = actorUserId;
            study.UpdateDateTime = now;
            study.Version += 1;
        }

        private void AddHistory(
            RadOrder order,
            RadStudy? study,
            string action,
            string? fromStatus,
            string toStatus,
            string? reasonCode,
            string? reasonNote,
            Guid actorUserId,
            DateTime now)
        {
            _dbContext.RadTransitionHistories.Add(new RadTransitionHistory
            {
                RadOrderId = order.Id,
                RadStudyId = study?.Id,
                EncounterId = order.EncounterId,
                Scope = study == null ? RadTransitionScope.RadOrder : RadTransitionScope.RadStudy,
                Action = action,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ReasonCode = reasonCode,
                ReasonNote = reasonNote,
                ActorUserId = actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now,
            });
        }

        private async Task SaveWithConcurrencyGuardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new RadConcurrencyException(
                    "Study ini sudah diubah petugas lain. Muat ulang lalu ulangi tindakan Anda.");
            }
        }

        private static string BuildStudyNumber(Guid orderId, int sequence) =>
            $"RAD-{orderId:N}-{sequence:D3}".ToUpperInvariant();

        /// <summary>
        /// Menyusun muatan fakta kelayakan tagih.
        ///
        /// Tidak memuat satu pun nominal yang dianggap final. <c>RuleSnapshot</c> membawa sebab
        /// pengulangan bila study ini adalah pengulangan, sehingga Billing dapat menerapkan
        /// aturan tanggung jawabnya tanpa menebak — <c>GATE-DEC-004</c> melarang pengulangan
        /// karena kesalahan internal rumah sakit otomatis menambah tagihan pasien.
        /// </summary>
        private Task<ClinicalFactEmissionResult> EmitChargeEligibilityAsync(
            RadStudy study,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var request = new ClinicalMilestoneFactRequest
            {
                SourceContext = BillingSourceContract.RadiologySourceContext,
                SourceAggregateId = study.RadOrderId,
                SourceItemId = study.Id,
                EffectType = BillingSourceContract.RadiologyChargeEffectType,
                EncounterId = study.EncounterId,
                OccurredAt = study.QualityDecidedAt ?? DateTime.UtcNow,
                Quantity = 1m,
                Unit = ExaminationUnit,
                RuleSnapshot = JsonSerializer.Serialize(new
                {
                    milestone = "StudyQualityAccepted",
                    studyNumber = study.StudyNumber,
                    studySequence = study.StudySequence,
                    isRepeat = study.RepeatOfStudyId != null,
                    repeatOfStudyId = study.RepeatOfStudyId,
                    repeatCause = study.RepeatCause?.ToString(),
                    additionalOrderId = study.AdditionalOrderId,
                    safetyRuleVersion = study.SafetyRuleVersionAtClearance,
                }),
            };

            return _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                request, actorUserId, cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            if (!Guid.TryParse(value, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan. " +
                    "Tindakan radiologi tidak dijalankan.");
            }

            return userId;
        }

        internal static RadStudyResponse MapStudy(RadStudy study)
        {
            return new RadStudyResponse
            {
                Id = study.Id,
                RadOrderId = study.RadOrderId,
                EncounterId = study.EncounterId,
                StudyNumber = study.StudyNumber,
                StudySequence = study.StudySequence,
                ProcedureId = study.ProcedureId,
                ModalityId = study.ModalityId,
                StudyStatus = study.StudyStatus.ToString(),
                PatientVerifiedAt = study.PatientVerifiedAt,
                SafetyClearedAt = study.SafetyClearedAt,
                SafetyRuleVersionAtClearance = study.SafetyRuleVersionAtClearance,
                AcquisitionStartedAt = study.AcquisitionStartedAt,
                AcquiredAt = study.AcquiredAt,
                IsUsable = study.IsUsable,
                QualityNote = study.QualityNote,
                AbortCause = study.AbortCause?.ToString(),
                AbortReason = study.AbortReason,
                PerformedPortionNote = study.PerformedPortionNote,
                RepeatOfStudyId = study.RepeatOfStudyId,
                RepeatCause = study.RepeatCause?.ToString(),
                RepeatReason = study.RepeatReason,
                AdditionalOrderId = study.AdditionalOrderId,
                BillingFactSubmitted = study.BillingFactSubmitted,
                BillingFactSubmittedAt = study.BillingFactSubmittedAt,
                Version = study.Version,
                SafetyChecks = study.SafetyChecks
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.RequirementCodeSnapshot)
                    .Select(x => new RadStudySafetyCheckResponse
                    {
                        Id = x.Id,
                        SafetyRequirementId = x.SafetyRequirementId,
                        RequirementCode = x.RequirementCodeSnapshot,
                        RequirementName = x.RequirementNameSnapshot,
                        IsMandatory = x.IsMandatorySnapshot,
                        CheckState = x.CheckState.ToString(),
                        DecidedAt = x.DecidedAt,
                        Note = x.Note,
                    })
                    .ToList(),
                Consumptions = study.Consumptions
                    .Where(x => !x.IsDelete)
                    .Select(MapConsumption)
                    .ToList(),
            };
        }

        private static RadConsumptionResponse MapConsumption(RadAcquisitionConsumption entity)
        {
            return new RadConsumptionResponse
            {
                Id = entity.Id,
                ItemType = entity.ItemType.ToString(),
                ItemCode = entity.ItemCode,
                ItemName = entity.ItemName,
                Quantity = entity.Quantity,
                Unit = entity.Unit,
                ConsumedDespiteFailure = entity.ConsumedDespiteFailure,
                RecordedAt = entity.RecordedAt,
                Note = entity.Note,
            };
        }
    }

    /// <summary>
    /// Hasil satu tindakan study beserta ringkasan penyerahan fakta ke Billing bila tindakan
    /// tersebut memang menerbitkan fakta.
    /// </summary>
    public sealed record RadStudyActionResult(
        RadStudyResponse Study,
        ClinicalFactEmissionResult? Handoff);

    /// <summary>
    /// Ditandai terpisah agar controller dapat membalas <c>409 Conflict</c> dan bukan
    /// <c>400 Bad Request</c> ketika dua petugas mengubah data yang sama bersamaan.
    /// </summary>
    public sealed class RadConcurrencyException : Exception
    {
        public RadConcurrencyException(string message) : base(message)
        {
        }
    }
}
