using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Hasil operasi rekonsiliasi obat beserta kode status yang sudah ditetapkan kontrak.
    /// </summary>
    public sealed class MedicationReconciliationResult<T>
    {
        public bool IsSuccess { get; private init; }
        public int StatusCode { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public T? Data { get; private init; }

        public static MedicationReconciliationResult<T> Ok(T data, string message, int statusCode = StatusCodes.Status200OK)
            => new() { IsSuccess = true, StatusCode = statusCode, Data = data, Message = message };

        public static MedicationReconciliationResult<T> Fail(int statusCode, string message)
            => new() { IsSuccess = false, StatusCode = statusCode, Message = message };
    }

    /// <summary>
    /// Rekonsiliasi obat bawaan pasien rawat inap — <c>BE-RWI-101</c>, <c>FR-DOK-092</c>,
    /// <c>FR-DOK-093</c>, <c>RWI-DEC-132</c> s.d. <c>RWI-DEC-134</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Proses bisnisnya.</b> Perawat mencatat obat yang dibawa pasien dari rumah. Dokter yang
    /// merawat memutuskan setiap obat: Lanjut Sama, Lanjut Ubah, atau Hentikan. "Lanjut" mengisi
    /// butir <b>draft</b> resep — tidak pernah resep aktif — sehingga resep itu tetap melewati alur
    /// resep biasa dan MAR baru membuat dosis setelah resepnya aktif.
    /// </para>
    /// <para>
    /// <b>Kewenangan dari data, bukan dari nama peran.</b> Hak akses <c>MedicationReconciliation :
    /// Create</c> dan <c>: Decide</c> diatur admin lewat layar Akses Role. Di atasnya service ini
    /// memeriksa: pencatat adalah pegawai yang ditempatkan pada unit episode, dan pemutus adalah
    /// dokter berpenugasan aktif pada episode (aturan menulis resep rawat inap, <c>RWI-DEC-099</c>).
    /// Perawat yang mencoba memutuskan tidak tertaut dokter, sehingga ditolak <c>403</c>.
    /// </para>
    /// <para>
    /// <b>Riwayat tidak pernah ditimpa.</b> Mengganti keputusan menambah baris keputusan baru yang
    /// menunjuk keputusan lama. Penggantian hanya sah selama resep hasil keputusan sebelumnya masih
    /// draft; setelah resep itu aktif, jawabannya <c>409</c>.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Budi membawa Amlodipine, Metformin, dan Vitamin. dr. Ahmad memilih Amlodipine
    /// "Lanjut Sama" → butir draft resep Amlodipine terbentuk; Metformin "Lanjut Ubah" → butir draft
    /// terbentuk untuk diubah dosisnya di layar resep; Vitamin "Hentikan" → tidak ada butir. Sebelum
    /// resep diselesaikan, dr. Ahmad mengganti Metformin menjadi "Hentikan" → baris keputusan nomor 2
    /// lahir dan butir draft Metformin dikeluarkan dari draft dalam transaksi yang sama. Setelah
    /// resepnya diselesaikan, penggantian keputusan Amlodipine ditolak <c>409</c>.
    /// </para>
    /// </remarks>
    public class MedicationReconciliationService
    {
        private const string LogCategory = "HealthServices.Pharmacy.MedicationReconciliation";

        private const string PenolakanBukanDokterPerawat =
            "Keputusan per obat hanya dapat diambil dokter yang merawat pasien ini.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly NursingActorService _nursingActorService;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly EncounterInsuranceService _encounterInsuranceService;
        private readonly PrescriptionNumberService _prescriptionNumberService;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly PrescriptionSummaryService _prescriptionSummaryService;
        private readonly LoggerService _loggerService;

        public MedicationReconciliationService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService,
            NursingActorService nursingActorService,
            InsuranceCoverageService insuranceCoverageService,
            EncounterInsuranceService encounterInsuranceService,
            PrescriptionNumberService prescriptionNumberService,
            PrescriptionAggregateService prescriptionAggregateService,
            PrescriptionSummaryService prescriptionSummaryService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
            _nursingActorService = nursingActorService;
            _insuranceCoverageService = insuranceCoverageService;
            _encounterInsuranceService = encounterInsuranceService;
            _prescriptionNumberService = prescriptionNumberService;
            _prescriptionAggregateService = prescriptionAggregateService;
            _prescriptionSummaryService = prescriptionSummaryService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        /// <summary>
        /// Daftar obat bawaan satu episode beserta keputusan terakhirnya.
        /// </summary>
        public async Task<List<ReconciliationItemResponse>> GetByEpisodeAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .AsNoTracking()
                .Include(x => x.DoseUnitMeasurement)
                .Include(x => x.RecordedByEmployee)
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.RecordedAt)
                .ToListAsync(cancellationToken);

            var itemIds = items.Select(x => x.Id).ToList();

            var decisions = await _dbContext.Set<PhmMedicationReconciliationDecision>()
                .AsNoTracking()
                .Include(x => x.DecidedByDoctor)
                .Where(x => itemIds.Contains(x.ReconciliationItemId) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            return items
                .Select(item => ToItemResponse(
                    item,
                    decisions
                        .Where(d => d.ReconciliationItemId == item.Id)
                        .OrderByDescending(d => d.SequenceNumber)
                        .FirstOrDefault()))
                .ToList();
        }

        /// <summary>
        /// Riwayat keputusan satu obat bawaan, dari yang pertama.
        /// </summary>
        public async Task<MedicationReconciliationResult<List<ReconciliationDecisionResponse>>> GetDecisionsAsync(
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (!ada)
            {
                return MedicationReconciliationResult<List<ReconciliationDecisionResponse>>.Fail(
                    StatusCodes.Status404NotFound, "Obat bawaan tidak ditemukan.");
            }

            var decisions = await _dbContext.Set<PhmMedicationReconciliationDecision>()
                .AsNoTracking()
                .Include(x => x.DecidedByDoctor)
                .Where(x => x.ReconciliationItemId == itemId && !x.IsDelete)
                .OrderBy(x => x.SequenceNumber)
                .ToListAsync(cancellationToken);

            return MedicationReconciliationResult<List<ReconciliationDecisionResponse>>.Ok(
                decisions.Select(ToDecisionResponse).ToList(),
                "Riwayat keputusan obat bawaan berhasil diambil.");
        }

        // =====================================================================
        // Perawat mencatat obat bawaan
        // =====================================================================

        /// <summary>
        /// Perawat mencatat satu obat bawaan — status awal <c>Pending</c>.
        /// </summary>
        public async Task<MedicationReconciliationResult<ReconciliationItemResponse>> RecordHomeMedicationAsync(
            CreateReconciliationItemRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey.Trim();

            if (kunci != null)
            {
                var sudahAda = await _dbContext.Set<PhmMedicationReconciliationItem>()
                    .AsNoTracking()
                    .Include(x => x.DoseUnitMeasurement)
                    .Include(x => x.RecordedByEmployee)
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

                if (sudahAda != null)
                {
                    return MedicationReconciliationResult<ReconciliationItemResponse>.Ok(
                        ToItemResponse(sudahAda, null),
                        "Obat bawaan sudah tercatat sebelumnya dengan kunci permintaan yang sama.");
                }
            }

            // VAL-DOK-53.
            if (request.DrugId == Guid.Empty)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pilih obat dari master obat. Bila belum ada, daftarkan dulu sebagai obat non-formularium.");
            }

            if (!request.Route.HasValue || !Enum.IsDefined(typeof(HomeMedicationRoute), request.Route.Value))
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Cara pemberian obat wajib dipilih.");
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.InpEpisodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Perawatan pasien ini sudah ditutup.");
            }

            var now = DateTime.UtcNow;

            var employeeId = await _nursingActorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status403Forbidden, InpatientClinicalContextService.PenolakanPerawatTanpaPegawai);
            }

            var bertugas = await _clinicalContextService.IsNurseOnDutyAtEpisodeAsync(
                episode.Id, employeeId.Value, now, cancellationToken);

            if (!bertugas)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status403Forbidden, InpatientClinicalContextService.PenolakanPerawatUnitLain);
            }

            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.DrugId && !x.IsDelete, cancellationToken);

            if (drug == null)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pilih obat dari master obat. Bila belum ada, daftarkan dulu sebagai obat non-formularium.");
            }

            if (request.DoseUnitMeasurementId.HasValue && request.DoseUnitMeasurementId.Value != Guid.Empty)
            {
                var satuanAda = await _dbContext.Set<MstMeasurement>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.DoseUnitMeasurementId.Value && !x.IsDelete, cancellationToken);

                if (!satuanAda)
                {
                    return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                        StatusCodes.Status400BadRequest, "Satuan dosis tidak ditemukan.");
                }
            }

            var entity = new PhmMedicationReconciliationItem
            {
                Id = Guid.NewGuid(),
                EncounterId = episode.EncounterId,
                InpEpisodeId = episode.Id,
                PatientId = episode.PatientId,
                DrugId = drug.Id,
                DrugNameSnapshot = drug.DrugName,
                IsFormularySnapshot = drug.IsFormulary,
                Dose = request.Dose,
                DoseUnitMeasurementId = request.DoseUnitMeasurementId.HasValue && request.DoseUnitMeasurementId.Value != Guid.Empty
                    ? request.DoseUnitMeasurementId
                    : null,
                DrugFormSnapshot = drug.DrugForm,
                FrequencyText = NormalizeText(request.FrequencyText),
                Route = request.Route.Value,
                Note = NormalizeText(request.Note),
                RecordedAt = now,
                RecordedByEmployeeId = employeeId.Value,
                RecordedByUserId = actorUserId,
                CurrentDecision = ReconciliationDecisionType.Pending,
                IdempotencyKey = kunci,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmMedicationReconciliationItem>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Note SENSITIF — tidak masuk payload logger.
            await _loggerService.InfoAsync(
                LogCategory,
                "MedicationReconciliation.RecordHomeMedication",
                "Perawat mencatat obat bawaan pasien.",
                new { entity.Id, entity.InpEpisodeId, entity.DrugId, entity.RecordedByEmployeeId });

            var tersimpan = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .AsNoTracking()
                .Include(x => x.DoseUnitMeasurement)
                .Include(x => x.RecordedByEmployee)
                .FirstAsync(x => x.Id == entity.Id, cancellationToken);

            return MedicationReconciliationResult<ReconciliationItemResponse>.Ok(
                ToItemResponse(tersimpan, null),
                "Obat bawaan berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Membatalkan baris salah catat <b>sebelum</b> ada keputusan dokter — <c>VAL-DOK-53a</c>.
        /// </summary>
        /// <remarks>
        /// Kamus data 0.5 bagian 13.2 tidak menyediakan kolom alasan pembatalan. Alasan tetap wajib
        /// dan disimpan pada jejak audit logger bersama pelakunya, tanpa menambah kolom di luar kamus
        /// data yang disetujui.
        /// </remarks>
        public async Task<MedicationReconciliationResult<ReconciliationItemResponse>> CancelAsync(
            Guid itemId,
            string? reason,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");
            }

            var entity = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status404NotFound, "Obat bawaan tidak ditemukan.");
            }

            if (!entity.IsActive)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status409Conflict, "Obat bawaan ini sudah dibatalkan.");
            }

            var sudahDiputuskan = entity.CurrentDecision != ReconciliationDecisionType.Pending ||
                await _dbContext.Set<PhmMedicationReconciliationDecision>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ReconciliationItemId == entity.Id && !x.IsDelete, cancellationToken);

            if (sudahDiputuskan)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Obat ini sudah diputuskan dokter sehingga tidak dapat dibatalkan.");
            }

            var episodeStatus = await ReadEpisodeStatusAsync(entity.InpEpisodeId, cancellationToken);

            if (episodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Perawatan pasien ini sudah ditutup.");
            }

            var now = DateTime.UtcNow;

            var employeeId = await _nursingActorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status403Forbidden, InpatientClinicalContextService.PenolakanPerawatTanpaPegawai);
            }

            var bertugas = await _clinicalContextService.IsNurseOnDutyAtEpisodeAsync(
                entity.InpEpisodeId, employeeId.Value, now, cancellationToken);

            if (!bertugas)
            {
                return MedicationReconciliationResult<ReconciliationItemResponse>.Fail(
                    StatusCodes.Status403Forbidden, InpatientClinicalContextService.PenolakanPerawatUnitLain);
            }

            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "MedicationReconciliation.Cancel",
                "Baris obat bawaan salah catat dibatalkan sebelum ada keputusan dokter.",
                new { entity.Id, entity.InpEpisodeId, CancelledBy = actorUserId, Reason = reason.Trim() });

            var tersimpan = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .AsNoTracking()
                .Include(x => x.DoseUnitMeasurement)
                .Include(x => x.RecordedByEmployee)
                .FirstAsync(x => x.Id == entity.Id, cancellationToken);

            return MedicationReconciliationResult<ReconciliationItemResponse>.Ok(
                ToItemResponse(tersimpan, null),
                "Obat bawaan berhasil dibatalkan.");
        }

        // =====================================================================
        // Dokter memutuskan
        // =====================================================================

        /// <summary>
        /// Dokter memutuskan atau mengganti keputusan atas satu obat bawaan —
        /// <c>FR-DOK-092</c>, <c>FR-DOK-093</c>, state matrix 0.6.0 bagian 8.5.
        /// </summary>
        public async Task<MedicationReconciliationResult<ReconciliationDecisionResponse>> DecideAsync(
            Guid itemId,
            CreateReconciliationDecisionRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.DecisionType is not (ReconciliationDecisionType.ContinueSame
                or ReconciliationDecisionType.ContinueModified
                or ReconciliationDecisionType.Stopped))
            {
                return FailDecision(StatusCodes.Status400BadRequest,
                    "Keputusan wajib salah satu dari Lanjut Sama, Lanjut Ubah, atau Hentikan.");
            }

            var item = await _dbContext.Set<PhmMedicationReconciliationItem>()
                .FirstOrDefaultAsync(x => x.Id == itemId && !x.IsDelete, cancellationToken);

            if (item == null)
                return FailDecision(StatusCodes.Status404NotFound, "Obat bawaan tidak ditemukan.");

            if (!item.IsActive)
                return FailDecision(StatusCodes.Status409Conflict, "Obat bawaan ini sudah dibatalkan.");

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == item.InpEpisodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
                return FailDecision(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            if (episode.EpisodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
                return FailDecision(StatusCodes.Status422UnprocessableEntity, "Perawatan pasien ini sudah ditutup.");

            var now = DateTime.UtcNow;

            // VAL-DOK-52. Akun perawat tidak tertaut dokter, sehingga berhenti di sini.
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            if (!doctorId.HasValue)
                return FailDecision(StatusCodes.Status403Forbidden, PenolakanBukanDokterPerawat);

            var bertugas = await _clinicalContextService.IsDoctorAssignedAsync(
                episode.Id, doctorId.Value, now, cancellationToken);

            if (!bertugas)
                return FailDecision(StatusCodes.Status403Forbidden, PenolakanBukanDokterPerawat);

            var sebelumnya = await _dbContext.Set<PhmMedicationReconciliationDecision>()
                .Where(x => x.ReconciliationItemId == item.Id && !x.IsDelete)
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken);

            PhmPrescriptionItem? butirSebelumnya = null;

            if (sebelumnya?.ResultPrescriptionItemId is Guid idButirLama)
            {
                butirSebelumnya = await _dbContext.Set<PhmPrescriptionItem>()
                    .FirstOrDefaultAsync(x => x.Id == idButirLama, cancellationToken);

                // VAL-DOK-52a. Keputusan hanya boleh diganti selama resep hasilnya masih draft.
                if (butirSebelumnya != null && !butirSebelumnya.IsDelete)
                {
                    var resepMasihDraft = await IsPrescriptionDraftAsync(butirSebelumnya.PrescriptionId, cancellationToken);

                    if (!resepMasihDraft)
                    {
                        return FailDecision(StatusCodes.Status409Conflict,
                            "Resep dari keputusan sebelumnya sudah aktif. Ubah terapi lewat resep atau hentikan obatnya.");
                    }
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            Guid? resultPrescriptionId = null;
            Guid? resultPrescriptionItemId = null;
            var prescriptionsToRebuild = new HashSet<Guid>();

            var lanjut = request.DecisionType != ReconciliationDecisionType.Stopped;

            if (lanjut)
            {
                if (butirSebelumnya != null && !butirSebelumnya.IsDelete)
                {
                    // Lanjut → Lanjut: butir draft yang sama tetap dipakai, tidak digandakan.
                    resultPrescriptionId = butirSebelumnya.PrescriptionId;
                    resultPrescriptionItemId = butirSebelumnya.Id;
                }
                else
                {
                    var draft = await ResolveTargetDraftAsync(
                        episode, doctorId.Value, request.TargetDraftPrescriptionId, actorUserId, now, cancellationToken);

                    if (!draft.IsSuccess || draft.Data == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return FailDecision(draft.StatusCode, draft.Message);
                    }

                    var butir = await AddDraftItemAsync(draft.Data, item, actorUserId, now, cancellationToken);

                    if (!butir.IsSuccess || butir.Data == null)
                    {
                        // INT-DOK-17: butir draft gagal dibuat → keputusan ikut batal.
                        await transaction.RollbackAsync(cancellationToken);
                        return FailDecision(butir.StatusCode, butir.Message);
                    }

                    resultPrescriptionId = draft.Data.Id;
                    resultPrescriptionItemId = butir.Data.Id;
                    prescriptionsToRebuild.Add(draft.Data.Id);
                }
            }
            else if (butirSebelumnya != null && !butirSebelumnya.IsDelete)
            {
                // Lanjut → Hentikan: butir draft dikeluarkan dari draft dalam transaksi yang sama.
                butirSebelumnya.IsDelete = true;
                butirSebelumnya.IsActive = false;
                butirSebelumnya.DeleteDateTime = now;
                butirSebelumnya.DeleteBy = actorUserId;
                butirSebelumnya.UpdateDateTime = now;
                butirSebelumnya.UpdateBy = actorUserId;
                prescriptionsToRebuild.Add(butirSebelumnya.PrescriptionId);
            }

            var keputusan = new PhmMedicationReconciliationDecision
            {
                Id = Guid.NewGuid(),
                ReconciliationItemId = item.Id,
                SequenceNumber = (sebelumnya?.SequenceNumber ?? 0) + 1,
                DecisionType = request.DecisionType,
                DecisionNote = NormalizeText(request.DecisionNote),
                DecidedByDoctorId = doctorId.Value,
                DecidedByUserId = actorUserId,
                DecidedAt = now,
                ResultPrescriptionId = resultPrescriptionId,
                ResultPrescriptionItemId = resultPrescriptionItemId,
                SupersedesDecisionId = sebelumnya?.Id,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmMedicationReconciliationDecision>().Add(keputusan);

            item.CurrentDecision = request.DecisionType;
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);

                foreach (var prescriptionId in prescriptionsToRebuild)
                {
                    await _prescriptionAggregateService.RebuildAsync(prescriptionId, actorUserId, now, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua dokter memutuskan obat yang sama bersamaan: index unik nomor urut keputusan
                // menolak yang kedua. Tidak ada keputusan setengah tersimpan.
                await transaction.RollbackAsync(cancellationToken);
                return FailDecision(StatusCodes.Status409Conflict,
                    "Obat ini baru saja diputuskan dokter lain. Muat ulang lalu periksa kembali.");
            }

            // DecisionNote SENSITIF — tidak masuk payload logger.
            await _loggerService.InfoAsync(
                LogCategory,
                "MedicationReconciliation.Decide",
                "Dokter memutuskan obat bawaan pasien.",
                new
                {
                    keputusan.Id,
                    keputusan.ReconciliationItemId,
                    keputusan.SequenceNumber,
                    DecisionType = keputusan.DecisionType.ToString(),
                    keputusan.DecidedByDoctorId,
                    keputusan.ResultPrescriptionItemId,
                    keputusan.SupersedesDecisionId
                });

            var tersimpan = await _dbContext.Set<PhmMedicationReconciliationDecision>()
                .AsNoTracking()
                .Include(x => x.DecidedByDoctor)
                .FirstAsync(x => x.Id == keputusan.Id, cancellationToken);

            return MedicationReconciliationResult<ReconciliationDecisionResponse>.Ok(
                ToDecisionResponse(tersimpan),
                "Keputusan obat bawaan berhasil disimpan.",
                StatusCodes.Status201Created);
        }

        // =====================================================================
        // Draft resep
        // =====================================================================

        /// <summary>
        /// Menentukan draft resep tujuan butir hasil keputusan "Lanjut".
        /// </summary>
        /// <remarks>
        /// Urutannya: draft yang disebut dokter; draft resep rawat inap milik dokter login pada episode
        /// ini; bila belum ada, draft baru pada catatan dokter login yang masih terbuka pada episode
        /// ini. <c>PhmPrescription.ConsultationId</c> tetap wajib (kamus data bagian 7.1) dan
        /// konsultasi bayangan dilarang (arsitektur 11.10), sehingga tanpa catatan dokter yang terbuka
        /// draft baru tidak dapat dibuat dan keputusan ditolak <c>422</c> beserta arahannya.
        /// </remarks>
        private async Task<MedicationReconciliationResult<PhmPrescription>> ResolveTargetDraftAsync(
            InpEpisode episode,
            Guid doctorId,
            Guid? targetDraftPrescriptionId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (targetDraftPrescriptionId.HasValue && targetDraftPrescriptionId.Value != Guid.Empty)
            {
                var target = await _dbContext.Set<PhmPrescription>()
                    .FirstOrDefaultAsync(x => x.Id == targetDraftPrescriptionId.Value && !x.IsDelete, cancellationToken);

                if (target == null || target.InpEpisodeId != episode.Id)
                {
                    return MedicationReconciliationResult<PhmPrescription>.Fail(
                        StatusCodes.Status404NotFound, "Draft resep tujuan tidak ditemukan pada perawatan ini.");
                }

                if (!IsDraft(target))
                {
                    return MedicationReconciliationResult<PhmPrescription>.Fail(
                        StatusCodes.Status409Conflict, "Resep tujuan sudah tidak berstatus draft.");
                }

                return MedicationReconciliationResult<PhmPrescription>.Ok(target, string.Empty);
            }

            var draftAda = await _dbContext.Set<PhmPrescription>()
                .Where(x =>
                    x.InpEpisodeId == episode.Id &&
                    x.DoctorId == doctorId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.PrescriptionStatus == PrescriptionStatus.Draft &&
                    x.PaymentStatus == PrescriptionPaymentStatus.NotBilled &&
                    x.PrescriptionOrderType != PrescriptionOrderType.Discharge)
                .OrderByDescending(x => x.PrescriptionDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (draftAda != null)
                return MedicationReconciliationResult<PhmPrescription>.Ok(draftAda, string.Empty);

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .Where(x =>
                    x.InpEpisodeId == episode.Id &&
                    x.DoctorId == doctorId &&
                    !x.IsDelete &&
                    x.ConsultationStatus != DoctorConsultationStatus.Completed &&
                    x.ConsultationStatus != DoctorConsultationStatus.Cancelled)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (consultation == null)
            {
                return MedicationReconciliationResult<PhmPrescription>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Belum ada catatan dokter Anda yang masih terbuka pada perawatan ini untuk menampung draft " +
                    "resep. Buat catatan dokter lebih dulu, lalu ulangi keputusan.");
            }

            var insuranceContext = await _encounterInsuranceService.GetContextAsync(
                consultation.EncounterId, now, cancellationToken);

            if (!insuranceContext.IsValid)
            {
                return MedicationReconciliationResult<PhmPrescription>.Fail(
                    StatusCodes.Status400BadRequest,
                    insuranceContext.ErrorMessage ?? "Konteks pembayaran kunjungan tidak valid.");
            }

            var prescription = new PhmPrescription
            {
                Id = Guid.NewGuid(),
                PrescriptionNumber = await _prescriptionNumberService.GenerateAsync(now, cancellationToken),
                EncounterId = consultation.EncounterId,
                ConsultationId = consultation.Id,
                InpEpisodeId = episode.Id,
                PrescriptionOrderType = PrescriptionOrderType.Routine,
                PatientId = consultation.PatientId,
                DoctorId = consultation.DoctorId,
                ServiceUnitId = consultation.ServiceUnitId,
                ClinicId = consultation.ClinicId,
                PaymentSourceId = insuranceContext.PaymentSourceId,
                PatientInsuranceId = insuranceContext.PatientInsuranceId,
                InsuranceProviderId = insuranceContext.InsuranceProviderId,
                PaymentTypeSnapshot = insuranceContext.PaymentType,
                PatientClassNameSnapshot = insuranceContext.PatientClassName,
                PaymentSourceNameSnapshot = insuranceContext.PaymentSourceName,
                InsuranceProviderNameSnapshot = insuranceContext.InsuranceProviderName,
                PolicyNumberSnapshot = insuranceContext.PolicyNumber,
                BenefitPlanCodeSnapshot = insuranceContext.BenefitPlanCode,
                BenefitPlanNameSnapshot = insuranceContext.BenefitPlanName,
                PrescriptionStatus = PrescriptionStatus.Draft,
                PaymentStatus = PrescriptionPaymentStatus.NotBilled,
                // Keadaan yang dituntut PrescriptionWorkflowService.FinalizeFromConsultationAsync dan
                // PrescriptionValidationService untuk draft yang akan difinalkan bersama catatan dokter.
                FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForClinicalFinalization,
                PrescriptionDateTime = now,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmPrescription>().Add(prescription);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _prescriptionSummaryService.RebuildConsultationSummaryAsync(
                consultation.Id, actorUserId, now, cancellationToken);

            return MedicationReconciliationResult<PhmPrescription>.Ok(prescription, string.Empty);
        }

        /// <summary>
        /// Menyalin obat bawaan menjadi butir draft resep — <c>INT-DOK-17</c>. Obat, dosis, dan
        /// frekuensi disalin; penanda formularium disalin dari master obat (<c>RWI-FACT-033</c>).
        /// Tarif dan coverage dihitung sumber yang sama dengan jalur tambah butir resep biasa.
        /// </summary>
        private async Task<MedicationReconciliationResult<PhmPrescriptionItem>> AddDraftItemAsync(
            PhmPrescription prescription,
            PhmMedicationReconciliationItem source,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .FirstOrDefaultAsync(x => x.Id == source.DrugId && !x.IsDelete, cancellationToken);

            if (drug == null)
            {
                return MedicationReconciliationResult<PhmPrescriptionItem>.Fail(
                    StatusCodes.Status409Conflict, "Obat bawaan ini sudah tidak ada pada master obat.");
            }

            var doseUnitId = source.DoseUnitMeasurementId ?? drug.DefaultDoseUnitMeasurementId;
            var doseUnit = doseUnitId.HasValue
                ? await _dbContext.Set<MstMeasurement>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == doseUnitId.Value && !x.IsDelete, cancellationToken)
                : null;

            var dispenseUnit = drug.DispenseUnitMeasurementId.HasValue
                ? await _dbContext.Set<MstMeasurement>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == drug.DispenseUnitMeasurementId.Value && !x.IsDelete, cancellationToken)
                : null;

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                prescription.EncounterId, drug.Id, 1, prescription.PrescriptionDateTime, cancellationToken);

            if (!coverage.IsValid)
            {
                return MedicationReconciliationResult<PhmPrescriptionItem>.Fail(
                    StatusCodes.Status400BadRequest,
                    coverage.ErrorMessage ?? "Tarif atau coverage obat tidak dapat dihitung.");
            }

            var entity = new PhmPrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescription.Id,
                DrugId = drug.Id,
                TariffId = coverage.TariffId,
                InsuranceTariffId = coverage.InsuranceTariffId,
                InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId,
                DrugCodeSnapshot = drug.DrugCode,
                DrugNameSnapshot = drug.DrugName,
                GenericNameSnapshot = drug.GenericName,
                DrugCategoryNameSnapshot = drug.DrugCategory?.DrugCategoryName,
                DrugFormSnapshot = drug.DrugForm,
                StrengthSnapshot = drug.Strength,
                RouteSnapshot = drug.Route,
                IsFormularySnapshot = drug.IsFormulary,
                IsGenericSnapshot = drug.IsGeneric,
                IsAntibioticSnapshot = drug.IsAntibiotic,
                IsNarcoticSnapshot = drug.IsNarcotic,
                IsPsychotropicSnapshot = drug.IsPsychotropic,
                IsHighAlertSnapshot = drug.IsHighAlert,
                Dose = source.Dose ?? 1,
                DoseUnitMeasurementId = doseUnit?.Id,
                DoseUnitNameSnapshot = doseUnit?.MeasurementName,
                DoseUnitSymbolSnapshot = doseUnit?.MeasurementSymbol,
                FrequencyText = source.FrequencyText,
                DoctorNote = "Dilanjutkan dari rekonsiliasi obat bawaan.",
                Quantity = coverage.Quantity,
                DispenseUnitMeasurementId = dispenseUnit?.Id,
                DispenseUnitNameSnapshot = dispenseUnit?.MeasurementName,
                DispenseUnitSymbolSnapshot = dispenseUnit?.MeasurementSymbol,
                HospitalUnitPrice = coverage.HospitalUnitPrice,
                ContractUnitPrice = coverage.ContractUnitPrice,
                UnitPrice = coverage.UnitPrice,
                TotalPrice = coverage.TotalPrice,
                PricingSource = coverage.PricingSource,
                IsCoverageApplicable = coverage.IsCoverageApplicable,
                IsCoveredByInsurance = coverage.IsCovered,
                CoverageStatus = coverage.CoverageStatus,
                CoveragePercent = coverage.CoveragePercent,
                CoveredAmount = coverage.CoveredAmount,
                PatientPayAmount = coverage.PatientPayAmount,
                CoPaymentAmount = coverage.CoPaymentAmount,
                IsNeedApproval = coverage.IsNeedApproval || drug.IsNeedApproval,
                IsApproved = false,
                IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient,
                CoverageNote = coverage.CoverageNote,
                DoseKind = PrescriptionDoseKind.Fixed,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<PhmPrescriptionItem>().Add(entity);

            return MedicationReconciliationResult<PhmPrescriptionItem>.Ok(entity, string.Empty);
        }

        private async Task<bool> IsPrescriptionDraftAsync(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var prescription = await _dbContext.Set<PhmPrescription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken);

            return prescription != null && IsDraft(prescription);
        }

        private static bool IsDraft(PhmPrescription prescription) =>
            !prescription.IsCancel &&
            prescription.PrescriptionStatus == PrescriptionStatus.Draft &&
            prescription.PaymentStatus == PrescriptionPaymentStatus.NotBilled;

        private async Task<InpEpisodeStatus?> ReadEpisodeStatusAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (InpEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static MedicationReconciliationResult<ReconciliationDecisionResponse> FailDecision(int statusCode, string message)
            => MedicationReconciliationResult<ReconciliationDecisionResponse>.Fail(statusCode, message);

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ReconciliationItemResponse ToItemResponse(
            PhmMedicationReconciliationItem x,
            PhmMedicationReconciliationDecision? latest)
        {
            var response = new ReconciliationItemResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                InpEpisodeId = x.InpEpisodeId,
                PatientId = x.PatientId,
                DrugId = x.DrugId,
                DrugNameSnapshot = x.DrugNameSnapshot,
                IsFormularySnapshot = x.IsFormularySnapshot,
                Dose = x.Dose,
                DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                DoseUnitName = x.DoseUnitMeasurement?.MeasurementName,
                DrugFormSnapshot = x.DrugFormSnapshot,
                FrequencyText = x.FrequencyText,
                Route = x.Route,
                Note = x.Note,
                RecordedAt = x.RecordedAt,
                RecordedByEmployeeId = x.RecordedByEmployeeId,
                RecordedByEmployeeName = x.RecordedByEmployee?.FullName,
                RecordedByUserId = x.RecordedByUserId,
                CurrentDecision = x.CurrentDecision,
                IsActive = x.IsActive,
                LatestDecision = latest == null ? null : ToDecisionResponse(latest)
            };

            if (x.IsActive)
            {
                if (x.CurrentDecision == ReconciliationDecisionType.Pending)
                    response.AvailableActions.Add("Cancel");

                response.AvailableActions.Add("Decide");
            }

            return response;
        }

        private static ReconciliationDecisionResponse ToDecisionResponse(PhmMedicationReconciliationDecision x) => new()
        {
            Id = x.Id,
            ReconciliationItemId = x.ReconciliationItemId,
            SequenceNumber = x.SequenceNumber,
            DecisionType = x.DecisionType,
            DecisionNote = x.DecisionNote,
            DecidedByDoctorId = x.DecidedByDoctorId,
            DecidedByDoctorName = x.DecidedByDoctor?.FullName,
            DecidedByUserId = x.DecidedByUserId,
            DecidedAt = x.DecidedAt,
            ResultPrescriptionId = x.ResultPrescriptionId,
            ResultPrescriptionItemId = x.ResultPrescriptionItemId,
            SupersedesDecisionId = x.SupersedesDecisionId
        };
    }
}
