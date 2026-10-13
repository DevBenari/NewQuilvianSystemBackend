using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// RJ-DOC-REV-BE-004 — surat dokter rawat jalan. Pemilik validasi, nomor surat, snapshot,
    /// riwayat, dan pembatalan. Dokter pada surat selalu Dokter Penanggung Jawab kunjungan.
    /// </summary>
    public class DoctorCertificateService
    {
        private const string LogCategory = "DoctorCertificate";
        private const int SequenceDigits = 6;

        /// <summary>Jenis sumber pada Admisi Transfer IGD &amp; Poliklinik untuk surat dari Poliklinik.</summary>
        public const string OutpatientTransferSourceType = "Outpatient";

        /// <summary>
        /// Status kunjungan yang boleh menerbitkan Surat Pengantar Rawat Inap: pasien sudah ditangani
        /// dokter. Kunjungan batal, tidak hadir, atau belum sampai ke dokter ditolak.
        /// </summary>
        private static readonly EncounterStatus[] AdmissionLetterEncounterStatuses =
        {
            EncounterStatus.InConsultation,
            EncounterStatus.ConsultationCompleted,
            EncounterStatus.Billing,
            EncounterStatus.Completed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;
        private readonly ClinicalActorScopeService _actorScopeService;

        public DoctorCertificateService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService,
            ClinicalActorScopeService actorScopeService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
            _actorScopeService = actorScopeService;
        }

        public sealed record Result(int StatusCode, string Message, DoctorCertificateResponse? Data = null)
        {
            public bool Success => StatusCode is >= 200 and < 300;
        }

        public static string ToTypeKey(DoctorCertificateType type) => type switch
        {
            DoctorCertificateType.Health => "health",
            DoctorCertificateType.InpatientReferral => "inpatientReferral",
            DoctorCertificateType.InpatientAdmissionLetter => "inpatientAdmission",
            _ => "sickLeave"
        };

        public static DoctorCertificateType? ParseType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "sickleave" or "sick_leave" or "1" => DoctorCertificateType.SickLeave,
            "health" or "2" => DoctorCertificateType.Health,
            "inpatientreferral" or "referral" or "3" => DoctorCertificateType.InpatientReferral,
            "inpatientadmission" or "inpatientadmissionletter" or "4" => DoctorCertificateType.InpatientAdmissionLetter,
            _ => null
        };

        private static (string Key, string Prefix) SequenceOf(DoctorCertificateType type) => type switch
        {
            DoctorCertificateType.Health => ("CLI_DOCTOR_CERT_HEALTH", "SKH"),
            DoctorCertificateType.InpatientReferral => ("CLI_DOCTOR_CERT_REFERRAL", "SRJ"),
            DoctorCertificateType.InpatientAdmissionLetter => ("CLI_DOCTOR_CERT_INPATIENT_ADMISSION", "SPRI"),
            _ => ("CLI_DOCTOR_CERT_SICK", "SKS")
        };

        public async Task<PagedResult<DoctorCertificateResponse>> GetListAsync(
            Guid? patientId,
            Guid? encounterId,
            string? certificateType,
            bool includeCancelled,
            int pageNumber,
            int pageSize,
            CancellationToken ct)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _dbContext.CliDoctorCertificates.AsNoTracking().Where(x => !x.IsDelete);

            if (patientId.HasValue) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (ParseType(certificateType) is { } type) query = query.Where(x => x.CertificateType == type);
            if (!includeCancelled) query = query.Where(x => x.CertificateStatus != DoctorCertificateStatus.Cancelled);

            var total = await query.CountAsync(ct);
            var rows = await query
                .OrderByDescending(x => x.IssuedDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var encounterNumbers = await LoadEncounterNumbersAsync(rows.Select(x => x.EncounterId), ct);
            var items = rows.Select(x => ToResponse(x, encounterNumbers, includeSignature: false)).ToList();
            await ApplyAdmissionSubmissionStatusAsync(items, ct);

            return new PagedResult<DoctorCertificateResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<DoctorCertificateResponse?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            return entity == null ? null : await ToDetailResponseAsync(entity, ct);
        }

        /// <summary>
        /// Surat Pengantar Rawat Inap terbaru berstatus terbit pada satu kunjungan — <c>BE-RWI-189</c>,
        /// <c>INT-RWA-05</c> (<c>RWI-DEC-254</c>, <c>262</c>, <c>RWI-FACT-066</c>).
        /// </summary>
        /// <remarks>
        /// Bacaan ramping untuk Workspace PPRI: hanya nomor, dokter penerbit, tanggal terbit,
        /// diagnosis, dan alasan rujukan. Data pasien dan gambar tanda tangan dokter <b>tidak</b>
        /// ikut. Surat yang dibatalkan dokter diabaikan; tanpa surat terbit hasilnya kosong.
        ///
        /// <para>
        /// Contoh: dr. Andika menerbitkan surat "Apendisitis akut — Rencana apendektomi" untuk
        /// kunjungan poli Tn. Budi pada 7 Oktober 2026. IPD mencetak diagnosis itu dan dokter
        /// perujuknya; butir 1 serah terima mendapat saran "surat pengantar dr. Andika, 07-10-2026".
        /// </para>
        /// </remarks>
        public async Task<InpatientReferralLetterSummary?> GetLatestIssuedInpatientReferralAsync(
            Guid encounterId,
            CancellationToken ct = default)
        {
            if (encounterId == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.CliDoctorCertificates.AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&
                    x.CertificateType == DoctorCertificateType.InpatientAdmissionLetter &&
                    x.CertificateStatus == DoctorCertificateStatus.Issued)
                .OrderByDescending(x => x.IssuedDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new InpatientReferralLetterSummary
                {
                    CertificateId = x.Id,
                    CertificateNumber = x.CertificateNumber,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorNameSnapshot,
                    IssuedDate = x.IssuedDate,
                    ReferralDiagnosis = x.ReferralDiagnosis,
                    ReferralReason = x.ReferralReason
                })
                .FirstOrDefaultAsync(ct);
        }

        /// <summary>
        /// Surat terbit pada satu antrean. Dengan <paramref name="type"/> pencarian dibatasi pada jenis
        /// itu, sehingga beberapa jenis surat dalam satu kunjungan tidak saling menimpa.
        /// </summary>
        public async Task<DoctorCertificateResponse?> GetActiveByQueueAsync(
            Guid queueId,
            DoctorCertificateType? type,
            CancellationToken ct)
        {
            var query = _dbContext.CliDoctorCertificates.AsNoTracking()
                .Where(x => x.QueueId == queueId && !x.IsDelete && x.CertificateStatus == DoctorCertificateStatus.Issued);
            if (type.HasValue) query = query.Where(x => x.CertificateType == type.Value);

            var entity = await query
                .OrderByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .FirstOrDefaultAsync(ct);
            return entity == null ? null : await ToDetailResponseAsync(entity, ct);
        }

        /// <summary>
        /// Pantauan Surat Pengantar Rawat Inap satu kunjungan: surat terbit terbaru, atau surat batal
        /// terbaru bila tidak ada yang terbit. Pemeriksaan cakupan pengguna dilakukan pemanggil.
        /// </summary>
        public async Task<OutpatientInpatientAdmissionMonitorResponse> GetInpatientAdmissionMonitorAsync(
            Guid encounterId,
            CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&
                    x.CertificateType == DoctorCertificateType.InpatientAdmissionLetter)
                .OrderBy(x => x.CertificateStatus == DoctorCertificateStatus.Issued ? 0 : 1)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(ct);

            if (entity == null)
            {
                return new OutpatientInpatientAdmissionMonitorResponse
                {
                    EncounterId = encounterId,
                    Status = "NotIssued",
                    StatusName = "Surat Pengantar Rawat Inap belum diterbitkan dokter"
                };
            }

            var letter = await ToDetailResponseAsync(entity, ct);
            return new OutpatientInpatientAdmissionMonitorResponse
            {
                EncounterId = encounterId,
                Status = letter.InpatientAdmission!.SubmissionStatus,
                StatusName = letter.InpatientAdmission.SubmissionStatusName,
                Letter = letter
            };
        }

        /// <remarks>
        /// Jenis <see cref="DoctorCertificateType.InpatientAdmissionLetter"/> membatasi pilihan pada
        /// unit layanan bertipe <c>Inpatient</c>, tanpa klinik.
        /// </remarks>
        public async Task<List<DoctorCertificateReferralTargetOption>> GetReferralTargetsAsync(
            string? search,
            int take,
            DoctorCertificateType? type,
            CancellationToken ct)
        {
            take = take is < 1 or > 100 ? 30 : take;
            var keyword = search?.Trim().ToLower();
            var inpatientOnly = type == DoctorCertificateType.InpatientAdmissionLetter;

            var units = _dbContext.MstServiceUnits.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
            if (inpatientOnly) units = units.Where(x => x.ServiceUnitType == ServiceUnitType.Inpatient);
            var clinics = _dbContext.MstClinics.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                units = units.Where(x => x.ServiceUnitName.ToLower().Contains(keyword) || x.ServiceUnitCode.ToLower().Contains(keyword));
                clinics = clinics.Where(x => x.ClinicName.ToLower().Contains(keyword) || x.ClinicCode.ToLower().Contains(keyword)
                    || (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)));
            }

            var unitRows = await units.OrderBy(x => x.ServiceUnitName).Take(take)
                .Select(x => new DoctorCertificateReferralTargetOption
                {
                    Kind = "serviceUnit",
                    Id = x.Id,
                    Code = x.ServiceUnitCode,
                    Name = x.ServiceUnitName
                })
                .ToListAsync(ct);

            if (inpatientOnly) return unitRows;

            var clinicRows = await clinics.OrderBy(x => x.ClinicName).Take(take)
                .Select(x => new DoctorCertificateReferralTargetOption
                {
                    Kind = "clinic",
                    Id = x.Id,
                    Code = x.ClinicCode,
                    Name = x.ClinicName,
                    ParentName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null
                })
                .ToListAsync(ct);

            return unitRows.Concat(clinicRows).Take(take).ToList();
        }

        public async Task<Result> CreateAsync(
            SaveDoctorCertificateRequest request,
            ClaimsPrincipal user,
            Guid actorUserId,
            CancellationToken ct)
        {
            var validation = await ValidateAsync(request, ct);
            if (validation.Error != null) return validation.Error;

            var type = validation.Type!.Value;
            var encounter = validation.Encounter!;

            if (type == DoctorCertificateType.InpatientAdmissionLetter)
                return await IssueInpatientAdmissionLetterAsync(request, validation, user, actorUserId, ct);

            var entity = new CliDoctorCertificate
            {
                CertificateType = type,
                CertificateStatus = DoctorCertificateStatus.Issued,
                PatientId = encounter.PatientId,
                EncounterId = encounter.Id,
                QueueId = validation.QueueId,
                ConsultationId = request.ConsultationId,
                ServiceUnitId = encounter.ServiceUnitId,
                ClinicId = encounter.ClinicId,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            await ApplyAsync(entity, request, validation, ct);

            // Nomor diminta paling akhir supaya deret tidak berlubang oleh permintaan yang ditolak.
            var (sequenceKey, prefix) = SequenceOf(type);
            try
            {
                entity.CertificateNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: sequenceKey,
                        Prefix: prefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: SequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    ct);
            }
            catch (NumberSeriesAllocationException)
            {
                return new Result(StatusCodes.Status500InternalServerError,
                    "Nomor surat dokter gagal diterbitkan. Coba simpan ulang beberapa saat lagi.");
            }

            _dbContext.CliDoctorCertificates.Add(entity);
            await _dbContext.SaveChangesAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Create", "Menerbitkan surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                CertificateType = entity.CertificateType.ToString(),
                entity.EncounterId,
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil diterbitkan.",
                await GetByIdAsync(entity.Id, ct));
        }

        public async Task<Result> UpdateAsync(Guid id, SaveDoctorCertificateRequest request, Guid actorUserId, CancellationToken ct)
        {
            var entity = await _dbContext.CliDoctorCertificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null)
                return new Result(StatusCodes.Status404NotFound, "Surat dokter tidak ditemukan.");

            if (entity.CertificateStatus == DoctorCertificateStatus.Cancelled)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter yang sudah dibatalkan tidak dapat diubah.");

            // Surat pengantar sudah menjadi pengajuan di Admisi Transfer; isinya dibekukan.
            if (entity.CertificateType == DoctorCertificateType.InpatientAdmissionLetter)
                return new Result(StatusCodes.Status409Conflict,
                    "Surat Pengantar Rawat Inap yang sudah terbit tidak dapat diubah. Batalkan surat lalu terbitkan surat baru.");

            var validation = await ValidateAsync(request, ct);
            if (validation.Error != null) return validation.Error;

            if (validation.Encounter!.Id != entity.EncounterId)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter tidak boleh dipindah ke kunjungan lain.");

            if (validation.Type != entity.CertificateType)
                return new Result(StatusCodes.Status400BadRequest,
                    "Jenis surat tidak dapat diubah. Batalkan surat ini lalu terbitkan surat baru.");

            await ApplyAsync(entity, request, validation, ct);
            entity.ConsultationId = request.ConsultationId ?? entity.ConsultationId;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Update", "Mengubah surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil diubah.", await GetByIdAsync(entity.Id, ct));
        }

        public async Task<Result> CancelAsync(
            Guid id,
            CancelDoctorCertificateRequest request,
            ClaimsPrincipal user,
            Guid actorUserId,
            CancellationToken ct)
        {
            // Baris surat dikunci seperti pada eksekusi admisi transfer, sehingga pembatalan dan
            // admisi atas surat yang sama tidak dapat berjalan bersamaan.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM public.\"CliDoctorCertificate\" WHERE \"Id\" = {id} FOR UPDATE", ct);

            var entity = await _dbContext.CliDoctorCertificates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (entity == null)
                return new Result(StatusCodes.Status404NotFound, "Surat dokter tidak ditemukan.");

            if (entity.CertificateStatus == DoctorCertificateStatus.Cancelled)
                return new Result(StatusCodes.Status400BadRequest, "Surat dokter sudah dibatalkan.");

            var reason = request.Reason?.Trim();
            if (string.IsNullOrEmpty(reason))
                return new Result(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");

            if (entity.CertificateType == DoctorCertificateType.InpatientAdmissionLetter)
            {
                var authority = await EnsureEncounterDoctorAsync(user, entity.DoctorId,
                    "Surat Pengantar Rawat Inap hanya dapat dibatalkan oleh Dokter Penanggung Jawab kunjungan.", ct);
                if (authority != null) return authority;

                if (await IsAdmittedAsync(entity.Id, ct))
                    return new Result(StatusCodes.Status409Conflict,
                        "Pengajuan sudah diadmisi menjadi episode Rawat Inap. Surat pengantar tidak dapat dibatalkan.");
            }

            var now = DateTime.UtcNow;
            entity.CertificateStatus = DoctorCertificateStatus.Cancelled;
            entity.CancelReason = reason;
            entity.IsCancel = true;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.Cancel", "Membatalkan surat dokter.", new
            {
                entity.Id,
                entity.CertificateNumber,
                CertificateType = entity.CertificateType.ToString(),
                ActorUserId = actorUserId
            });

            return new Result(StatusCodes.Status200OK, "Surat dokter berhasil dibatalkan.", await GetByIdAsync(entity.Id, ct));
        }

        /// <summary>
        /// Menerbitkan Surat Pengantar Rawat Inap. Surat tersimpan permanen saat itu juga dan langsung
        /// menjadi pengajuan <c>Outpatient</c> di Admisi Transfer; episode Rawat Inap tidak dibuat di sini.
        /// </summary>
        private async Task<Result> IssueInpatientAdmissionLetterAsync(
            SaveDoctorCertificateRequest request,
            Validation validation,
            ClaimsPrincipal user,
            Guid actorUserId,
            CancellationToken ct)
        {
            var encounter = validation.Encounter!;

            var authority = await EnsureEncounterDoctorAsync(user, encounter.DoctorId,
                "Surat Pengantar Rawat Inap hanya dapat diterbitkan oleh Dokter Penanggung Jawab kunjungan.", ct);
            if (authority != null) return authority;

            // Kunjungan dikunci supaya dua penerbitan bersamaan diproses berurutan; yang kedua
            // melihat surat aktif dari yang pertama dan ditolak.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM public.\"RegPatientEncounter\" WHERE \"Id\" = {encounter.Id} FOR UPDATE", ct);

            var activeNumber = await _dbContext.CliDoctorCertificates.AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounter.Id &&
                    !x.IsDelete &&
                    x.CertificateType == DoctorCertificateType.InpatientAdmissionLetter &&
                    x.CertificateStatus == DoctorCertificateStatus.Issued)
                .Select(x => x.CertificateNumber)
                .FirstOrDefaultAsync(ct);
            if (activeNumber != null)
                return new Result(StatusCodes.Status409Conflict,
                    $"Surat Pengantar Rawat Inap {activeNumber} untuk kunjungan ini masih aktif. Batalkan surat tersebut bila perlu menerbitkan ulang.");

            var entity = new CliDoctorCertificate
            {
                CertificateType = DoctorCertificateType.InpatientAdmissionLetter,
                CertificateStatus = DoctorCertificateStatus.Issued,
                PatientId = encounter.PatientId,
                EncounterId = encounter.Id,
                QueueId = validation.QueueId,
                ConsultationId = request.ConsultationId,
                ServiceUnitId = encounter.ServiceUnitId,
                ClinicId = encounter.ClinicId,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            await ApplyAsync(entity, request, validation, ct);

            var (sequenceKey, prefix) = SequenceOf(entity.CertificateType);
            try
            {
                entity.CertificateNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(
                        SequenceKey: sequenceKey,
                        Prefix: prefix,
                        ResetPolicy: NumberSeriesResetPolicies.Never,
                        SequenceDigits: SequenceDigits,
                        ActorUserId: actorUserId,
                        Instant: DateTimeOffset.UtcNow),
                    ct);
            }
            catch (NumberSeriesAllocationException)
            {
                return new Result(StatusCodes.Status500InternalServerError,
                    "Nomor surat pengantar gagal diterbitkan. Coba terbitkan ulang beberapa saat lagi.");
            }

            _dbContext.CliDoctorCertificates.Add(entity);
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _loggerService.AuditAsync(LogCategory, "DoctorCertificate.IssueInpatientAdmissionLetter",
                "Menerbitkan Surat Pengantar Rawat Inap dan mengirim pengajuan ke Admisi Transfer.", new
                {
                    entity.Id,
                    entity.CertificateNumber,
                    entity.EncounterId,
                    entity.DoctorId,
                    TargetServiceUnitId = entity.ReferralTargetServiceUnitId,
                    ActorUserId = actorUserId
                });

            return new Result(StatusCodes.Status200OK,
                "Surat Pengantar Rawat Inap diterbitkan dan dikirim ke Admisi Transfer Rawat Inap.",
                await GetByIdAsync(entity.Id, ct));
        }

        /// <summary>
        /// Kewenangan yang melekat pada data: pengguna yang login harus dokter yang tercatat sebagai
        /// Dokter Penanggung Jawab. Hak akses peran tetap ditegakkan <c>[AccessPermission]</c>.
        /// </summary>
        private async Task<Result?> EnsureEncounterDoctorAsync(
            ClaimsPrincipal user,
            Guid? responsibleDoctorId,
            string mismatchMessage,
            CancellationToken ct)
        {
            var actorDoctorId = await _actorScopeService.ResolveDoctorIdAsync(user, ct);
            if (!actorDoctorId.HasValue)
                return new Result(StatusCodes.Status403Forbidden,
                    "Akun Anda tidak terhubung dengan data dokter aktif.");

            return responsibleDoctorId == actorDoctorId
                ? null
                : new Result(StatusCodes.Status403Forbidden, mismatchMessage);
        }

        private Task<bool> IsAdmittedAsync(Guid certificateId, CancellationToken ct) =>
            _dbContext.RegPatientEncounters.IgnoreQueryFilters().AnyAsync(x =>
                x.AdmissionTransferId == certificateId &&
                x.AdmissionTransferSourceType == OutpatientTransferSourceType, ct);

        private sealed class Validation
        {
            public Result? Error { get; init; }
            public DoctorCertificateType? Type { get; init; }
            public QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter? Encounter { get; init; }
            public Guid? QueueId { get; init; }
            public string? ReferralTargetName { get; init; }
        }

        private async Task<Validation> ValidateAsync(SaveDoctorCertificateRequest request, CancellationToken ct)
        {
            static Validation Fail(string message) =>
                new() { Error = new Result(StatusCodes.Status400BadRequest, message) };

            var type = ParseType(request.CertificateType);
            if (type == null) return Fail("Jenis surat dokter tidak dikenali.");

            if (!request.EncounterId.HasValue || request.EncounterId == Guid.Empty)
                return Fail("Kunjungan wajib diisi.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EncounterId.Value && !x.IsDelete, ct);
            if (encounter == null)
                return new Validation { Error = new Result(StatusCodes.Status404NotFound, "Kunjungan tidak ditemukan.") };

            Guid? queueId = null;
            if (request.QueueId.HasValue && request.QueueId != Guid.Empty)
            {
                var queueMatches = await _dbContext.RegQueues.AsNoTracking()
                    .AnyAsync(x => x.Id == request.QueueId.Value && x.EncounterId == encounter.Id && !x.IsDelete, ct);
                if (!queueMatches) return Fail("Antrean tidak sesuai dengan kunjungan.");
                queueId = request.QueueId;
            }

            string? referralTargetName = null;
            switch (type)
            {
                case DoctorCertificateType.SickLeave:
                {
                    var sick = request.SickLeave;
                    if (sick?.StartDate == null || sick.EndDate == null)
                        return Fail("Tanggal mulai dan selesai istirahat wajib diisi.");
                    if (sick.EndDate.Value.Date < sick.StartDate.Value.Date)
                        return Fail("Tanggal selesai istirahat tidak boleh sebelum tanggal mulai.");
                    break;
                }
                case DoctorCertificateType.Health:
                    if (request.Health == null)
                        return Fail("Data pemeriksaan surat sehat wajib diisi.");
                    break;
                case DoctorCertificateType.InpatientReferral:
                {
                    var referral = request.InpatientReferral;
                    if (referral == null) return Fail("Data rujukan wajib diisi.");

                    if (referral.TargetClinicId is { } clinicId && clinicId != Guid.Empty)
                    {
                        referralTargetName = await _dbContext.MstClinics.AsNoTracking()
                            .Where(x => x.Id == clinicId && !x.IsDelete && x.IsActive)
                            .Select(x => x.ClinicName)
                            .FirstOrDefaultAsync(ct);
                        if (referralTargetName == null) return Fail("Klinik tujuan rujukan tidak ditemukan atau tidak aktif.");
                    }
                    else if (referral.TargetServiceUnitId is { } unitId && unitId != Guid.Empty)
                    {
                        referralTargetName = await _dbContext.MstServiceUnits.AsNoTracking()
                            .Where(x => x.Id == unitId && !x.IsDelete && x.IsActive)
                            .Select(x => x.ServiceUnitName)
                            .FirstOrDefaultAsync(ct);
                        if (referralTargetName == null) return Fail("Unit tujuan rujukan tidak ditemukan atau tidak aktif.");
                    }
                    else
                    {
                        return Fail("Unit/Tujuan rujukan wajib dipilih dari daftar.");
                    }
                    break;
                }
                case DoctorCertificateType.InpatientAdmissionLetter:
                {
                    var (error, targetName) = await ValidateInpatientAdmissionAsync(request.InpatientAdmission, encounter, ct);
                    if (error != null) return new Validation { Error = error };
                    referralTargetName = targetName;
                    break;
                }
            }

            return new Validation
            {
                Type = type,
                Encounter = encounter,
                QueueId = queueId,
                ReferralTargetName = referralTargetName
            };
        }

        /// <summary>
        /// Kunjungan harus Rawat Jalan berklinik (bukan IGD) yang sudah ditangani dokter, dan unit
        /// tujuan harus unit layanan aktif bertipe <c>Inpatient</c>.
        /// </summary>
        private async Task<(Result? Error, string? TargetName)> ValidateInpatientAdmissionAsync(
            DoctorCertificateInpatientAdmissionRequest? admission,
            QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter encounter,
            CancellationToken ct)
        {
            static (Result?, string?) Fail(int statusCode, string message) => (new Result(statusCode, message), null);

            var isEmergencyVisit = await _dbContext.Set<EmgVisit>().AsNoTracking()
                .AnyAsync(x => x.EncounterId == encounter.Id && !x.IsDelete, ct);
            if (encounter.EncounterType != EncounterType.Outpatient || encounter.ClinicId == null || isEmergencyVisit)
                return Fail(StatusCodes.Status409Conflict,
                    "Surat Pengantar Rawat Inap hanya untuk kunjungan Rawat Jalan poliklinik. Pasien IGD diajukan lewat disposisi IGD.");

            if (encounter.IsCancel || !AdmissionLetterEncounterStatuses.Contains(encounter.EncounterStatus))
                return Fail(StatusCodes.Status409Conflict,
                    "Kunjungan belum ditangani dokter, sudah dibatalkan, atau pasien tidak hadir. Surat pengantar tidak dapat diterbitkan.");

            if (admission == null)
                return Fail(StatusCodes.Status400BadRequest, "Data surat pengantar rawat inap wajib diisi.");
            if (string.IsNullOrWhiteSpace(admission.Diagnosis))
                return Fail(StatusCodes.Status400BadRequest, "Diagnosis wajib diisi.");
            if (string.IsNullOrWhiteSpace(admission.Reason))
                return Fail(StatusCodes.Status400BadRequest, "Alasan rawat inap wajib diisi.");
            if (string.IsNullOrWhiteSpace(admission.PatientCondition))
                return Fail(StatusCodes.Status400BadRequest, "Kondisi pasien wajib diisi.");
            if (admission.TargetServiceUnitId is not { } unitId || unitId == Guid.Empty)
                return Fail(StatusCodes.Status400BadRequest, "Unit Rawat Inap tujuan wajib dipilih dari daftar.");

            var targetName = await _dbContext.MstServiceUnits.AsNoTracking()
                .Where(x => x.Id == unitId && !x.IsDelete && x.IsActive && x.ServiceUnitType == ServiceUnitType.Inpatient)
                .Select(x => x.ServiceUnitName)
                .FirstOrDefaultAsync(ct);

            return targetName == null
                ? Fail(StatusCodes.Status400BadRequest, "Unit tujuan harus unit layanan Rawat Inap yang aktif.")
                : (null, targetName);
        }

        /// <summary>SIP dokter yang masih berlaku dan terverifikasi dari data kredensial SDM.</summary>
        private async Task<string?> LoadValidDoctorSipAsync(Guid workforceProfileId, CancellationToken ct)
        {
            var today = DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate().Date, DateTimeKind.Utc);
            return await _dbContext.Set<WfpCredentialLicense>().AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    !x.IsRevoked &&
                    x.IsVerified &&
                    x.VerificationStatus == CredentialVerificationStatus.Verified &&
                    x.LicenseType == "SIP" &&
                    x.ExpiredDate >= today)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.ExpiredDate)
                .Select(x => x.LicenseNumber)
                .FirstOrDefaultAsync(ct);
        }

        private async Task ApplyAsync(
            CliDoctorCertificate entity,
            SaveDoctorCertificateRequest request,
            Validation validation,
            CancellationToken ct)
        {
            var encounter = validation.Encounter!;

            var patient = await _dbContext.MstPatients.AsNoTracking()
                .Where(x => x.Id == encounter.PatientId)
                .Select(x => new { x.FullName, x.MedicalRecordNumber, x.BirthDate, x.Address })
                .FirstOrDefaultAsync(ct);

            // Dokter pada surat = Dokter Penanggung Jawab kunjungan, bukan nilai kiriman klien.
            var doctor = encounter.DoctorId.HasValue
                ? await _dbContext.MstDoctors.AsNoTracking()
                    .Where(x => x.Id == encounter.DoctorId.Value)
                    .Select(x => new { x.Id, x.FullName, x.WorkforceProfileId })
                    .FirstOrDefaultAsync(ct)
                : null;

            var clinicName = encounter.ClinicId.HasValue
                ? await _dbContext.MstClinics.AsNoTracking()
                    .Where(x => x.Id == encounter.ClinicId.Value)
                    .Select(x => x.ClinicName)
                    .FirstOrDefaultAsync(ct)
                : null;

            entity.DoctorId = doctor?.Id;
            entity.DoctorNameSnapshot = doctor?.FullName;
            entity.ClinicNameSnapshot = clinicName;
            entity.DoctorSipSnapshot = Clean(request.DoctorSip);
            entity.DoctorSignatureDataUrl = Clean(request.DoctorSignatureDataUrl);

            entity.PatientNameSnapshot = Clean(request.PatientName) ?? patient?.FullName ?? string.Empty;
            entity.MedicalRecordNumberSnapshot = patient?.MedicalRecordNumber;
            entity.BirthDateSnapshot = AsUtcDate(request.BirthDate) ?? AsUtcDate(patient?.BirthDate);
            entity.GenderSnapshot = Clean(request.Gender);
            entity.AddressSnapshot = Clean(request.Address) ?? Clean(patient?.Address);
            entity.OccupationSnapshot = Clean(request.Occupation);

            entity.IssuedDate = AsUtcDate(request.IssuedDate) ?? AsUtcDate(DateTime.UtcNow)!.Value;
            entity.Purpose = Clean(request.Purpose);
            entity.Diagnosis = Clean(request.Diagnosis);
            entity.ClinicalSummary = Clean(request.ClinicalSummary);
            entity.AdditionalNote = Clean(request.AdditionalNote);

            var sick = entity.CertificateType == DoctorCertificateType.SickLeave ? request.SickLeave : null;
            entity.SickStartDate = AsUtcDate(sick?.StartDate);
            entity.SickEndDate = AsUtcDate(sick?.EndDate);
            entity.SickDurationDays = sick?.StartDate != null && sick.EndDate != null
                ? (int)(sick.EndDate.Value.Date - sick.StartDate.Value.Date).TotalDays + 1
                : null;
            entity.ActivityRestriction = Clean(sick?.ActivityRestriction);

            var health = entity.CertificateType == DoctorCertificateType.Health ? request.Health : null;
            entity.ExaminationDate = AsUtcDate(health?.ExaminationDate);
            entity.HealthConclusion = Clean(health?.Conclusion);
            entity.BloodPressure = Clean(health?.BloodPressure);
            entity.Pulse = Clean(health?.Pulse);
            entity.Temperature = Clean(health?.Temperature);
            entity.Weight = Clean(health?.Weight);
            entity.Height = Clean(health?.Height);
            entity.ColorBlindResult = Clean(health?.ColorBlindResult);
            entity.HealthRecommendation = Clean(health?.Recommendation);

            var referral = entity.CertificateType == DoctorCertificateType.InpatientReferral ? request.InpatientReferral : null;
            entity.ReferralAdmissionDate = AsUtcDate(referral?.AdmissionDate);
            entity.ReferralDiagnosis = Clean(referral?.Diagnosis);
            entity.ReferralReason = Clean(referral?.Reason);
            entity.ReferralTargetClinicId = referral?.TargetClinicId is { } c && c != Guid.Empty ? c : null;
            entity.ReferralTargetServiceUnitId = entity.ReferralTargetClinicId == null && referral?.TargetServiceUnitId is { } u && u != Guid.Empty ? u : null;
            entity.ReferralTargetNameSnapshot = validation.ReferralTargetName;
            entity.RequestedRoomClass = Clean(referral?.RequestedRoomClass);
            entity.SpecialInstruction = Clean(referral?.SpecialInstruction);

            if (entity.CertificateType != DoctorCertificateType.InpatientAdmissionLetter) return;

            // Surat Pengantar Rawat Inap: identitas, SIP, tanggal, dan tanda tangan ditentukan server.
            // Gambar tanda tangan kiriman klien tidak pernah disimpan.
            var admission = request.InpatientAdmission!;
            entity.DoctorSignatureDataUrl = null;
            entity.DoctorSipSnapshot = doctor != null
                ? await LoadValidDoctorSipAsync(doctor.WorkforceProfileId, ct)
                : null;
            entity.PatientNameSnapshot = patient?.FullName ?? string.Empty;
            entity.BirthDateSnapshot = AsUtcDate(patient?.BirthDate);
            entity.AddressSnapshot = Clean(patient?.Address);
            entity.IssuedDate = AsUtcDate(AppDateTimeHelper.OperationalDate())!.Value;
            entity.ReferralAdmissionDate = entity.IssuedDate;

            entity.Diagnosis = Clean(admission.Diagnosis);
            entity.ReferralDiagnosis = entity.Diagnosis;
            entity.ReferralReason = Clean(admission.Reason);
            entity.ClinicalSummary = Clean(admission.PatientCondition);
            entity.ReferralTargetClinicId = null;
            entity.ReferralTargetServiceUnitId = admission.TargetServiceUnitId;
            entity.ReferralTargetNameSnapshot = validation.ReferralTargetName;
            entity.RequestedRoomClass = Clean(admission.RequestedRoomClass);
            entity.SpecialInstruction = Clean(admission.SpecialInstruction);
        }

        private async Task<Dictionary<Guid, string>> LoadEncounterNumbersAsync(IEnumerable<Guid> encounterIds, CancellationToken ct)
        {
            var ids = encounterIds.Distinct().ToList();
            return await _dbContext.RegPatientEncounters.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.EncounterNumber, ct);
        }

        private static DoctorCertificateResponse ToResponse(
            CliDoctorCertificate x,
            IReadOnlyDictionary<Guid, string> encounterNumbers,
            bool includeSignature) => new()
        {
            Id = x.Id,
            CertificateNumber = x.CertificateNumber,
            CertificateType = ToTypeKey(x.CertificateType),
            CertificateTypeName = x.CertificateType switch
            {
                DoctorCertificateType.Health => "Surat Keterangan Sehat",
                DoctorCertificateType.InpatientReferral => "Surat Rujukan",
                DoctorCertificateType.InpatientAdmissionLetter => "Surat Pengantar Rawat Inap",
                _ => "Surat Keterangan Sakit"
            },
            CertificateStatus = (int)x.CertificateStatus,
            CertificateStatusName = x.CertificateStatus == DoctorCertificateStatus.Cancelled ? "Dibatalkan" : "Terbit",
            PatientId = x.PatientId,
            EncounterId = x.EncounterId,
            EncounterNumber = encounterNumbers.TryGetValue(x.EncounterId, out var number) ? number : null,
            QueueId = x.QueueId,
            ConsultationId = x.ConsultationId,
            DoctorId = x.DoctorId,
            ClinicId = x.ClinicId,
            IssuedDate = x.IssuedDate,
            Purpose = x.Purpose,
            Diagnosis = x.Diagnosis,
            ClinicalSummary = x.ClinicalSummary,
            AdditionalNote = x.AdditionalNote,
            PatientName = x.PatientNameSnapshot,
            MedicalRecordNumber = x.MedicalRecordNumberSnapshot,
            BirthDate = x.BirthDateSnapshot,
            Gender = x.GenderSnapshot,
            Address = x.AddressSnapshot,
            Occupation = x.OccupationSnapshot,
            DoctorName = x.DoctorNameSnapshot,
            DoctorSip = x.DoctorSipSnapshot,
            ClinicName = x.ClinicNameSnapshot,
            DoctorSignatureDataUrl = includeSignature ? x.DoctorSignatureDataUrl : null,
            SickLeave = x.CertificateType == DoctorCertificateType.SickLeave
                ? new DoctorCertificateSickLeaveRequest
                {
                    StartDate = x.SickStartDate,
                    EndDate = x.SickEndDate,
                    DurationDays = x.SickDurationDays,
                    ActivityRestriction = x.ActivityRestriction
                }
                : null,
            Health = x.CertificateType == DoctorCertificateType.Health
                ? new DoctorCertificateHealthRequest
                {
                    ExaminationDate = x.ExaminationDate,
                    Conclusion = x.HealthConclusion,
                    BloodPressure = x.BloodPressure,
                    Pulse = x.Pulse,
                    Temperature = x.Temperature,
                    Weight = x.Weight,
                    Height = x.Height,
                    ColorBlindResult = x.ColorBlindResult,
                    Recommendation = x.HealthRecommendation
                }
                : null,
            InpatientReferral = x.CertificateType == DoctorCertificateType.InpatientReferral
                ? new DoctorCertificateReferralResponse
                {
                    AdmissionDate = x.ReferralAdmissionDate,
                    Diagnosis = x.ReferralDiagnosis,
                    Reason = x.ReferralReason,
                    TargetServiceUnitId = x.ReferralTargetServiceUnitId,
                    TargetClinicId = x.ReferralTargetClinicId,
                    TargetServiceUnit = x.ReferralTargetNameSnapshot,
                    RequestedRoomClass = x.RequestedRoomClass,
                    SpecialInstruction = x.SpecialInstruction
                }
                : null,
            InpatientAdmission = x.CertificateType == DoctorCertificateType.InpatientAdmissionLetter
                ? new DoctorCertificateInpatientAdmissionResponse
                {
                    TargetServiceUnitId = x.ReferralTargetServiceUnitId,
                    TargetServiceUnit = x.ReferralTargetNameSnapshot,
                    Diagnosis = x.ReferralDiagnosis,
                    Reason = x.ReferralReason,
                    PatientCondition = x.ClinicalSummary,
                    RequestedRoomClass = x.RequestedRoomClass,
                    SpecialInstruction = x.SpecialInstruction,
                    IssuedAt = x.CreateDateTime
                }
                : null,
            DoctorSignature = includeSignature && x.CertificateType == DoctorCertificateType.InpatientAdmissionLetter
                ? new DoctorCertificateSignatureResponse
                {
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorNameSnapshot,
                    DoctorSip = x.DoctorSipSnapshot,
                    SignedAt = x.CreateDateTime,
                    VerificationCode = x.CertificateNumber
                }
                : null,
            CancelReason = x.CancelReason,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime
        };

        private async Task<DoctorCertificateResponse> ToDetailResponseAsync(CliDoctorCertificate entity, CancellationToken ct)
        {
            var numbers = await LoadEncounterNumbersAsync(new[] { entity.EncounterId }, ct);
            var response = ToResponse(entity, numbers, includeSignature: true);
            await ApplyAdmissionSubmissionStatusAsync(new[] { response }, ct);
            return response;
        }

        /// <summary>
        /// Status pengajuan Surat Pengantar Rawat Inap dengan aturan yang sama seperti antrean Admisi
        /// Transfer: diadmisi bila sudah ada kunjungan Rawat Inap yang merujuk surat ini.
        /// </summary>
        private async Task ApplyAdmissionSubmissionStatusAsync(
            IReadOnlyCollection<DoctorCertificateResponse> responses,
            CancellationToken ct)
        {
            var letters = responses.Where(x => x.InpatientAdmission != null).ToList();
            if (letters.Count == 0) return;

            var ids = letters.Select(x => x.Id).ToList();
            var admissions = await (
                    from encounter in _dbContext.RegPatientEncounters.IgnoreQueryFilters()
                    where encounter.AdmissionTransferSourceType == OutpatientTransferSourceType
                        && encounter.AdmissionTransferId != null
                        && ids.Contains(encounter.AdmissionTransferId.Value)
                    select new
                    {
                        CertificateId = encounter.AdmissionTransferId!.Value,
                        Episode = _dbContext.Set<InpEpisode>()
                            .Where(e => e.EncounterId == encounter.Id)
                            .Select(e => new { e.Id, e.EpisodeNumber, e.AdmittedAt })
                            .FirstOrDefault()
                    })
                .ToListAsync(ct);
            var admissionByCertificate = admissions.ToDictionary(x => x.CertificateId);

            foreach (var letter in letters)
            {
                var admission = letter.InpatientAdmission!;
                if (admissionByCertificate.TryGetValue(letter.Id, out var admitted))
                {
                    admission.SubmissionStatus = InpatientAdmissionSubmissionStatus.Admitted;
                    admission.SubmissionStatusName = "Sudah diadmisi Rawat Inap";
                    admission.EpisodeId = admitted.Episode?.Id;
                    admission.EpisodeNumber = admitted.Episode?.EpisodeNumber;
                    admission.AdmittedAt = admitted.Episode?.AdmittedAt;
                }
                else if (letter.CertificateStatus == (int)DoctorCertificateStatus.Cancelled)
                {
                    admission.SubmissionStatus = InpatientAdmissionSubmissionStatus.Cancelled;
                    admission.SubmissionStatusName = "Dibatalkan dokter";
                }
                else
                {
                    admission.SubmissionStatus = InpatientAdmissionSubmissionStatus.Submitted;
                    admission.SubmissionStatusName = "Menunggu admisi Rawat Inap";
                }
            }
        }

        /// <summary>Tanggal kalender disimpan sebagai tengah malam UTC (kolom timestamptz).</summary>
        private static DateTime? AsUtcDate(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc) : null;

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Ringkasan Surat Pengantar Rawat Inap untuk modul lain (<c>BE-RWI-189</c>). Bukan kontrak API;
    /// sengaja tanpa data pasien dan tanpa gambar tanda tangan dokter.
    /// </summary>
    public sealed class InpatientReferralLetterSummary
    {
        public Guid CertificateId { get; init; }

        public string CertificateNumber { get; init; } = string.Empty;

        public Guid? DoctorId { get; init; }

        /// <summary>Nama dokter penerbit saat surat dibuat.</summary>
        public string? DoctorName { get; init; }

        /// <summary>Tanggal terbit; tanggal kalender disimpan sebagai tengah malam UTC.</summary>
        public DateTime IssuedDate { get; init; }

        public string? ReferralDiagnosis { get; init; }

        public string? ReferralReason { get; init; }
    }
}
