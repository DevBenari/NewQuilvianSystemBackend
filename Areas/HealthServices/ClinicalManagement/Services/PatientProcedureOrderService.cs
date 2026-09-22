using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Service pengelola pesanan tindakan rawat inap, aturan penginput/DPJP (INV-DOK-17),
    /// verifikasi instruksi dokter (BE-RWI-098), serta pembatalan otomatis saat penutupan
    /// episode (RWI-DEC-143, BE-RWI-097).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Perbaikan kompilasi BE-RWI-098.</b> Versi yang mendarat bersama <c>BE-RWI-097</c>
    /// memanggil <c>ILoggerService</c> yang tidak ada di repository, memanggil
    /// <c>ResolveActorDoctorIdAsync</c> tanpa argumen identitas pengguna, dan membaca
    /// <c>MstProcedure.Price</c> yang juga tidak ada. Ketiganya diperbaiki di sini: logger memakai
    /// <see cref="LoggerService"/> seperti seluruh controller klinis, identitas dokter memakai
    /// tanda tangan method yang sebenarnya, dan harga memakai
    /// <see cref="InsuranceCoverageService.ResolveProcedureAsync"/> — sumber tarif yang sama dengan
    /// jalur tindakan dari catatan dokter, sehingga pesanan perawat tidak menghasilkan harga yang
    /// berbeda untuk tindakan yang sama.
    /// </para>
    /// </remarks>
    public class PatientProcedureOrderService
    {
        private const string LogCategory = "ClinicalManagement.PatientProcedureOrderService";

        /// <summary>
        /// <c>VAL-DOK-50</c> — pengguna bukan dokter pemberi instruksi.
        /// </summary>
        public const string PenolakanBukanPemberiInstruksi =
            "Hanya dokter pemberi instruksi yang dapat memverifikasi pesanan ini.";

        /// <summary>
        /// <c>VAL-DOK-50a</c> — status verifikasi bukan <c>Pending</c>.
        /// </summary>
        public const string PenolakanBukanMenungguVerifikasi =
            "Pesanan ini sudah diverifikasi atau tidak memerlukan verifikasi.";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly NursingActorService _nursingActorService;
        private readonly LoggerService _loggerService;

        public PatientProcedureOrderService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService,
            InsuranceCoverageService insuranceCoverageService,
            NursingActorService nursingActorService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
            _insuranceCoverageService = insuranceCoverageService;
            _nursingActorService = nursingActorService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Membuat pesanan tindakan rawat inap oleh dokter atau perawat (FR-DOK-100 / BE-RWI-097).
        /// Perawat wajib menyebut dokter pemberi instruksi yang aktif bertugas atas pasien.
        /// </summary>
        public async Task<PatientProcedureOrderResult> CreateInpatientOrderAsync(
            CreateInpatientProcedureOrderRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                return PatientProcedureOrderResult.BadRequest("Quantity tindakan harus lebih dari 0.");
            }

            var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
                ? null
                : request.IdempotencyKey.Trim();

            if (idempotencyKey != null)
            {
                var existing = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

                if (existing != null)
                {
                    return PatientProcedureOrderResult.Success(
                        MapToResponse(existing),
                        "Pesanan tindakan sudah tercatat sebelumnya dengan kunci permintaan yang sama.");
                }
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.InpEpisodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return PatientProcedureOrderResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Closed || episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return PatientProcedureOrderResult.UnprocessableEntity(
                    "Perawatan rawat inap pasien ini sudah ditutup, sehingga tindakan baru tidak dapat dicatat lagi.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Draft)
            {
                return PatientProcedureOrderResult.BadRequest(
                    "Perawatan rawat inap masih berstatus Draft; pasien belum teradmisi.");
            }

            var now = DateTime.UtcNow;
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            Guid orderDoctorId;
            Guid? instructingDoctorId;
            PatientProcedureInstructionVerificationStatus verificationStatus;

            if (doctorId.HasValue)
            {
                var isDoctorAssigned = await _clinicalContextService.IsDoctorAssignedAsync(
                    episode.Id, doctorId.Value, now, cancellationToken);

                if (!isDoctorAssigned)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Anda tidak sedang bertugas atas pasien ini. Minta kepala ruangan membuatkan " +
                        "penugasan singkat untuk menulis catatan terlambat.");
                }

                orderDoctorId = doctorId.Value;
                instructingDoctorId = null;
                verificationStatus = PatientProcedureInstructionVerificationStatus.NotRequired;
            }
            else
            {
                // State matrix 0.6.0 bagian 8.3: perawat memesan dari unit tempat episode dirawat
                // (RWI-DEC-100). Penjaga ini tidak membaca nama peran; ia membaca penempatan
                // pegawai pada unit episode.
                var employeeId = await _nursingActorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

                if (employeeId == null)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        InpatientClinicalContextService.PenolakanPerawatTanpaPegawai);
                }

                var nurseOnDuty = await _clinicalContextService.IsNurseOnDutyAtEpisodeAsync(
                    episode.Id, employeeId.Value, now, cancellationToken);

                if (!nurseOnDuty)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        InpatientClinicalContextService.PenolakanPerawatUnitLain);
                }

                // VAL-DOK-46.
                if (!request.InstructingDoctorId.HasValue || request.InstructingDoctorId.Value == Guid.Empty)
                {
                    return PatientProcedureOrderResult.BadRequest("Pilih dokter yang memberi instruksi.");
                }

                // VAL-DOK-47.
                var isInstructingDoctorAssigned = await _clinicalContextService.IsDoctorAssignedAsync(
                    episode.Id, request.InstructingDoctorId.Value, now, cancellationToken);

                if (!isInstructingDoctorAssigned)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Dokter yang dipilih tidak sedang bertugas atas pasien ini.");
                }

                orderDoctorId = request.InstructingDoctorId.Value;
                instructingDoctorId = request.InstructingDoctorId.Value;
                verificationStatus = PatientProcedureInstructionVerificationStatus.Pending;
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProcedureId && !x.IsDelete, cancellationToken);

            if (procedure == null)
            {
                return PatientProcedureOrderResult.NotFound("Master tindakan tidak ditemukan.");
            }

            var pricing = await _insuranceCoverageService.ResolveProcedureAsync(
                episode.EncounterId,
                procedure.Id,
                request.Quantity,
                now,
                cancellationToken);

            if (!pricing.IsValid)
            {
                return PatientProcedureOrderResult.BadRequest(
                    pricing.ErrorMessage ?? "Tarif atau coverage tindakan tidak dapat ditentukan.");
            }

            var entity = new TrxPatientProcedure
            {
                Id = Guid.NewGuid(),
                EncounterId = episode.EncounterId,
                ConsultationId = null, // Dilonggarkan sejak R7 (BE-RWI-097)
                PatientId = episode.PatientId,
                DoctorId = orderDoctorId,
                ServiceUnitId = episode.ServiceUnitId,
                ClinicId = null,
                InpEpisodeId = episode.Id,
                PhysicianVisitId = null,
                IdempotencyKey = idempotencyKey,
                ProcedureId = procedure.Id,
                TariffId = pricing.TariffId,
                InsuranceTariffId = pricing.InsuranceTariffId,
                InsuranceCoverageRuleId = pricing.InsuranceCoverageRuleId,
                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,
                ProcedureTypeSnapshot = procedure.ProcedureType,
                ProcedureCategoryNameSnapshot = procedure.ProcedureCategoryName,
                ProcedureMasterType = "Master",
                IsFromMasterProcedure = true,
                IsPrimaryProcedure = request.IsPrimaryProcedure,
                IsEmergencyProcedure = request.IsEmergencyProcedure,
                IsSurgeryRelated = procedure.IsSurgery,
                IsPackageProcedure = false,
                ProcedureSource = PatientProcedureSource.DoctorOrder,
                ProcedureStatus = PatientProcedureStatus.Ordered,
                ProcedureDateTime = now,
                PlannedAt = now,
                Quantity = pricing.Quantity,
                UnitNameSnapshot = "Tindakan",
                UnitPrice = pricing.UnitPrice,
                TotalPrice = pricing.TotalPrice,
                HospitalPriceSnapshot = pricing.HospitalUnitPrice,
                InsuranceContractPrice = pricing.ContractUnitPrice,
                IsFreeOfCharge = false,
                IsBillable = true,
                IsCoveredByInsurance = pricing.IsCovered,
                CoverageStatus = pricing.CoverageStatus,
                CoveragePercent = pricing.CoveragePercent,
                CoveredAmount = pricing.CoveredAmount,
                PatientPayAmount = pricing.PatientPayAmount,
                CoverageNote = pricing.CoverageNote,
                IsNeedApproval = pricing.IsNeedApproval || procedure.IsNeedApproval,
                IsApproved = false,
                IsExecuted = false,
                ClinicalNote = NormalizeText(request.ClinicalReason),
                InstructionNote = NormalizeText(request.InstructionNote),
                IsBillingGenerated = false,
                OrderedByUserId = actorUserId,
                InstructingDoctorId = instructingDoctorId,
                InstructionVerificationStatus = verificationStatus,
                CancelledByEpisodeClosure = false,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.Set<TrxPatientProcedure>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedureOrderService.CreateInpatientOrderAsync",
                "Berhasil membuat pesanan tindakan rawat inap.",
                new
                {
                    entity.Id,
                    entity.InpEpisodeId,
                    entity.DoctorId,
                    entity.InstructingDoctorId,
                    VerificationStatus = entity.InstructionVerificationStatus.ToString(),
                    entity.OrderedByUserId
                });

            return PatientProcedureOrderResult.Created(
                MapToResponse(entity),
                "Pesanan tindakan rawat inap berhasil dibuat.");
        }

        /// <summary>
        /// Mengubah pesanan tindakan yang belum dilaksanakan (INV-DOK-17).
        /// Hanya penginput asli yang berhak mengubah.
        /// </summary>
        public async Task<PatientProcedureOrderResult> UpdateOrderAsync(
            Guid id,
            UpdatePatientProcedureRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            // INV-DOK-17 / VAL-DOK-48: pesanan yang belum dilaksanakan hanya diubah penginputnya.
            if (entity.OrderedByUserId.HasValue && entity.OrderedByUserId.Value != actorUserId)
            {
                return PatientProcedureOrderResult.Forbidden(
                    "Pesanan ini dibuat petugas lain. Hanya penginputnya yang dapat mengubah isi pesanan.");
            }

            if (entity.InpEpisodeId.HasValue)
            {
                var episodeStatus = await ReadEpisodeStatusAsync(entity.InpEpisodeId.Value, cancellationToken);

                if (episodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
                {
                    return PatientProcedureOrderResult.UnprocessableEntity(
                        "Perawatan rawat inap sudah ditutup; tindakan tidak dapat diubah.");
                }
            }

            if (entity.IsBillingGenerated)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan yang sudah masuk billing tidak dapat diubah.");
            }

            if (entity.IsExecuted)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan yang sudah dieksekusi tidak dapat diubah.");
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan yang sudah dibatalkan tidak dapat diubah.");
            }

            if (request.Quantity <= 0)
            {
                return PatientProcedureOrderResult.BadRequest("Quantity tindakan harus lebih dari 0.");
            }

            var now = DateTime.UtcNow;
            entity.Quantity = request.Quantity;
            entity.TotalPrice = entity.UnitPrice * request.Quantity;
            entity.PatientPayAmount = entity.TotalPrice - entity.CoveredAmount;
            entity.IsPrimaryProcedure = request.IsPrimaryProcedure;
            entity.IsEmergencyProcedure = request.IsEmergencyProcedure;
            entity.ClinicalNote = NormalizeText(request.ClinicalNote);
            entity.InstructionNote = NormalizeText(request.InstructionNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Pesanan tindakan berhasil diperbarui.");
        }

        /// <summary>
        /// Membatalkan pesanan tindakan (FR-DOK-102 / INV-DOK-17).
        /// Hanya penginput atau DPJP aktif pada episode rawat inap yang berwenang membatalkan.
        /// </summary>
        public async Task<PatientProcedureOrderResult> CancelOrderAsync(
            Guid id,
            string reason,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return PatientProcedureOrderResult.BadRequest("Alasan pembatalan wajib diisi.");
            }

            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            if (entity.IsBillingGenerated)
            {
                return PatientProcedureOrderResult.Conflict(
                    "Tindakan yang sudah masuk billing tidak dapat dibatalkan dari modul klinis.");
            }

            if (entity.IsExecuted)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan yang sudah dieksekusi tidak dapat dibatalkan.");
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan sudah dibatalkan sebelumnya.");
            }

            // FR-DOK-102 / VAL-DOK-49: pesanan hanya dibatalkan oleh penginput atau DPJP aktif.
            if (entity.InpEpisodeId.HasValue && entity.OrderedByUserId.HasValue)
            {
                var isSubmitter = entity.OrderedByUserId.Value == actorUserId;
                var isDpjp = false;

                if (!isSubmitter)
                {
                    var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                        user, actorUserId, cancellationToken);

                    if (doctorId.HasValue)
                    {
                        isDpjp = await _clinicalContextService.IsDpjpAssignedAsync(
                            entity.InpEpisodeId.Value, doctorId.Value, DateTime.UtcNow, cancellationToken);
                    }
                }

                if (!isSubmitter && !isDpjp)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Pesanan hanya dapat dibatalkan penginputnya atau DPJP pasien.");
                }
            }

            var now = DateTime.UtcNow;
            entity.ProcedureStatus = PatientProcedureStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = reason.Trim();
            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // CancelReason SENSITIF — permission-audit-matrix 0.6.0 bagian 9.2; tidak dicatat.
            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedureOrderService.CancelOrderAsync",
                "Pesanan tindakan berhasil dibatalkan.",
                new
                {
                    entity.Id,
                    entity.InpEpisodeId,
                    CancelledBy = actorUserId
                });

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Pesanan tindakan berhasil dibatalkan.");
        }

        /// <summary>
        /// Verifikasi instruksi pesanan tindakan oleh dokter pemberi instruksi —
        /// <c>BE-RWI-098</c>, <c>FR-DOK-104</c>, <c>INV-DOK-17</c>, <c>VAL-DOK-50</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Siapa yang boleh.</b> Hanya dokter yang tertulis sebagai pemberi instruksi pada pesanan
        /// itu — bukan DPJP, bukan dokter lain. Identitasnya diturunkan dari akun login, tidak
        /// pernah dari isi permintaan. Pemeriksaan ini melekat pada data pesanan, sehingga bukan
        /// hardcode peran: hak akses <c>PatientProcedure : Verify</c> tetap diatur layar Akses Role.
        /// </para>
        /// <para>
        /// <b>Yang tidak berubah.</b> Verifikasi <b>hanya</b> menyentuh empat kolom:
        /// <c>InstructionVerificationStatus</c>, <c>InstructionVerifiedAt</c>,
        /// <c>InstructionVerifiedByUserId</c>, dan jejak audit ubah. Penginput
        /// (<c>OrderedByUserId</c>), dokter pesanan, tindakan, jumlah, catatan klinis, maupun status
        /// pesanan tidak disentuh — <c>RWI-DEC-139</c> butir (2).
        /// </para>
        /// <para>
        /// <b>Tidak bergantung status pesanan.</b> Pesanan yang sudah dilaksanakan tetap dapat
        /// diverifikasi, dan pemberi instruksi yang penugasannya sudah berakhir tetap boleh
        /// memverifikasi — verifikasi bukan dokumen baru (permission-audit-matrix 0.6.0 bagian 8.6).
        /// </para>
        /// <para>
        /// <b>Contoh.</b> 23.00 Ns. Siti memesan cek GDS untuk Budi atas instruksi dr. Ahmad → status
        /// <c>Pending</c>. 07.30 dr. Rina (DPJP) menekan Verifikasi → <c>403</c>. 08.00 dr. Ahmad
        /// menekan Verifikasi → <c>Verified</c>; penginput tetap Ns. Siti. 08.01 dr. Ahmad menekan lagi
        /// → <c>409</c>.
        /// </para>
        /// </remarks>
        public async Task<PatientProcedureOrderResult> VerifyInstructionAsync(
            Guid id,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            // Pesanan dokter tidak punya pemberi instruksi; tidak ada yang dapat diverifikasi.
            if (!entity.InstructingDoctorId.HasValue ||
                entity.InstructingDoctorId.Value == Guid.Empty)
            {
                return PatientProcedureOrderResult.Conflict(PenolakanBukanMenungguVerifikasi);
            }

            // VAL-DOK-50.
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(
                user, actorUserId, cancellationToken);

            if (!doctorId.HasValue || doctorId.Value != entity.InstructingDoctorId.Value)
            {
                return PatientProcedureOrderResult.Forbidden(PenolakanBukanPemberiInstruksi);
            }

            // VAL-DOK-50a. Transisi hanya Pending → Verified (INV-DOK-17).
            if (entity.InstructionVerificationStatus != PatientProcedureInstructionVerificationStatus.Pending)
            {
                return PatientProcedureOrderResult.Conflict(PenolakanBukanMenungguVerifikasi);
            }

            var now = DateTime.UtcNow;
            entity.InstructionVerificationStatus = PatientProcedureInstructionVerificationStatus.Verified;
            entity.InstructionVerifiedAt = now;
            entity.InstructionVerifiedByUserId = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedureOrderService.VerifyInstructionAsync",
                "Instruksi pesanan tindakan diverifikasi dokter pemberi instruksi.",
                new
                {
                    entity.Id,
                    entity.InpEpisodeId,
                    entity.InstructingDoctorId,
                    entity.OrderedByUserId,
                    VerifiedBy = actorUserId
                });

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Instruksi pesanan tindakan berhasil diverifikasi.");
        }

        /// <summary>
        /// Daftar tunggu verifikasi instruksi milik dokter login — <c>BE-RWI-098</c>,
        /// api-contract 0.6.0 bagian 12.5 <c>GET /instruction-verification-worklist</c>.
        /// </summary>
        /// <remarks>
        /// Hanya pesanan berstatus <c>Pending</c> yang pemberi instruksinya dokter login. Daftar
        /// sengaja tidak menyaring penugasan aktif: dokter yang penugasannya sudah berakhir tetap
        /// wajib dapat menemukan pesanan yang menunggu verifikasinya. Identitas pasien yang
        /// dikembalikan hanya nama, nomor rekam medis, dan nomor episode.
        /// </remarks>
        public async Task<PagedResult<InstructionVerificationItemResponse>> GetInstructionVerificationWorklistAsync(
            Guid doctorId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.InstructingDoctorId == doctorId &&
                    x.InstructionVerificationStatus == PatientProcedureInstructionVerificationStatus.Pending);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InstructionVerificationItemResponse
                {
                    PatientProcedureId = x.Id,
                    InpEpisodeId = x.InpEpisodeId,
                    EpisodeNumber = _dbContext.Set<InpEpisode>()
                        .Where(e => e.Id == x.InpEpisodeId)
                        .Select(e => e.EpisodeNumber)
                        .FirstOrDefault(),
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    ProcedureId = x.ProcedureId,
                    ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                    ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                    Quantity = x.Quantity,
                    ProcedureStatus = x.ProcedureStatus,
                    IsExecuted = x.IsExecuted,
                    OrderedAt = x.CreateDateTime,
                    OrderedByUserId = x.OrderedByUserId,
                    OrderedByUserName = x.OrderedByUser != null ? x.OrderedByUser.DisplayName : null,
                    InstructionVerificationStatus = x.InstructionVerificationStatus
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<InstructionVerificationItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Penjaga pelaksanaan pesanan tindakan rawat inap — <c>BE-RWI-098</c> kriteria 4,
        /// state matrix 0.6.0 bagian 8.3 baris "Melaksanakan".
        /// </summary>
        /// <remarks>
        /// <para>
        /// Pelaksana diambil dari akun login dan menjadi penulis sekaligus penanda tangan catatan
        /// pelaksanaan; karena itu mesin addendum hanya mengenali pelaksana tersebut sebagai
        /// penulis asli (<c>FR-DOK-103</c>). Penjaga ini memastikan pelaksananya memang berwenang:
        /// dokter lewat penugasan aktif pada episode, perawat lewat penempatan pada unit episode.
        /// </para>
        /// <para>
        /// Tindakan tanpa episode rawat inap — poliklinik, medical check-up, IGD — dijawab lolos
        /// tanpa pemeriksaan apa pun, sehingga perilakunya tidak bergeser.
        /// </para>
        /// </remarks>
        public async Task<PatientProcedureOrderResult?> EnsureInpatientExecutorAsync(
            TrxPatientProcedure entity,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!entity.InpEpisodeId.HasValue || entity.InpEpisodeId.Value == Guid.Empty)
                return null;

            var episodeStatus = await ReadEpisodeStatusAsync(entity.InpEpisodeId.Value, cancellationToken);

            if (episodeStatus is InpEpisodeStatus.Closed or InpEpisodeStatus.Cancelled)
            {
                return PatientProcedureOrderResult.UnprocessableEntity(
                    "Perawatan pasien ini sudah ditutup, sehingga tindakan tidak dapat ditandai dilaksanakan.");
            }

            var now = DateTime.UtcNow;
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);

            if (doctorId.HasValue)
            {
                var bertugas = await _clinicalContextService.IsDoctorAssignedAsync(
                    entity.InpEpisodeId.Value, doctorId.Value, now, cancellationToken);

                return bertugas
                    ? null
                    : PatientProcedureOrderResult.Forbidden("Anda tidak sedang bertugas atas pasien ini.");
            }

            var employeeId = await _nursingActorService.ResolveEmployeeIdAsync(user, actorUserId, cancellationToken);

            if (employeeId == null)
            {
                return PatientProcedureOrderResult.Forbidden(
                    InpatientClinicalContextService.PenolakanPerawatTanpaPegawai);
            }

            var perawatBertugas = await _clinicalContextService.IsNurseOnDutyAtEpisodeAsync(
                entity.InpEpisodeId.Value, employeeId.Value, now, cancellationToken);

            return perawatBertugas
                ? null
                : PatientProcedureOrderResult.Forbidden(InpatientClinicalContextService.PenolakanPerawatUnitLain);
        }

        /// <summary>
        /// Pembatalan otomatis seluruh pesanan tindakan tertunda saat penutupan episode (Langkah 5 / BE-RWI-083 / RWI-DEC-143).
        /// Dipanggil di dalam transaksi penutupan; tidak melakukan commit sendiri.
        /// </summary>
        public async Task<int> CancelPendingOrdersForClosureAsync(
            Guid episodeId,
            Guid closedByUserId,
            CancellationToken cancellationToken = default)
        {
            var pendingOrders = await _dbContext.Set<TrxPatientProcedure>()
                .Where(x => x.InpEpisodeId == episodeId &&
                            !x.IsDelete &&
                            !x.IsExecuted &&
                            !x.IsBillingGenerated &&
                            (x.ProcedureStatus == PatientProcedureStatus.Planned ||
                             x.ProcedureStatus == PatientProcedureStatus.Ordered))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var order in pendingOrders)
            {
                order.ProcedureStatus = PatientProcedureStatus.Cancelled;
                order.CancelledByEpisodeClosure = true;
                order.CancelReason = "Episode ditutup sebelum tindakan dilaksanakan.";
                order.CancelledAt = now;
                order.CancelledByUserId = closedByUserId;
                order.IsActive = false;
                order.IsCancel = true;
                order.CancelDateTime = now;
                order.CancelBy = closedByUserId;
                order.UpdateDateTime = now;
                order.UpdateBy = closedByUserId;
            }

            return pendingOrders.Count;
        }

        private async Task<InpEpisodeStatus?> ReadEpisodeStatusAsync(
            Guid episodeId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (InpEpisodeStatus?)x.EpisodeStatus)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static PatientProcedureResponse MapToResponse(TrxPatientProcedure entity)
        {
            return new PatientProcedureResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                ProcedureId = entity.ProcedureId,
                TariffId = entity.TariffId,
                InsuranceTariffId = entity.InsuranceTariffId,
                InsuranceCoverageRuleId = entity.InsuranceCoverageRuleId,
                ProcedureCodeSnapshot = entity.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = entity.ProcedureNameSnapshot,
                ProcedureTypeSnapshot = entity.ProcedureTypeSnapshot,
                ProcedureSource = entity.ProcedureSource,
                ProcedureStatus = entity.ProcedureStatus,
                ProcedureDateTime = entity.ProcedureDateTime,
                PlannedAt = entity.PlannedAt,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                TotalPrice = entity.TotalPrice,
                IsBillable = entity.IsBillable,
                IsCoveredByInsurance = entity.IsCoveredByInsurance,
                CoverageStatus = entity.CoverageStatus,
                CoveredAmount = entity.CoveredAmount,
                PatientPayAmount = entity.PatientPayAmount,
                IsNeedApproval = entity.IsNeedApproval,
                IsApproved = entity.IsApproved,
                IsExecuted = entity.IsExecuted,
                ExecutedAt = entity.ExecutedAt,
                PerformedAt = entity.PerformedAt,
                PerformedByUserId = entity.PerformedByUserId,
                IsBillingGenerated = entity.IsBillingGenerated,
                BillingGeneratedAt = entity.BillingGeneratedAt,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                OrderedByUserId = entity.OrderedByUserId,
                InstructingDoctorId = entity.InstructingDoctorId,
                InstructionVerificationStatus = entity.InstructionVerificationStatus,
                InstructionVerifiedAt = entity.InstructionVerifiedAt,
                InstructionVerifiedByUserId = entity.InstructionVerifiedByUserId,
                CancelledByEpisodeClosure = entity.CancelledByEpisodeClosure,
                CanEdit = !entity.IsExecuted && !entity.IsBillingGenerated && entity.ProcedureStatus != PatientProcedureStatus.Cancelled,
                CanRemoveFromDraft = false
            };
        }
    }

    /// <summary>
    /// Hasil operasi pesanan tindakan dengan penanda status HTTP yang presisi.
    /// </summary>
    public class PatientProcedureOrderResult
    {
        public bool IsSuccess { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public PatientProcedureResponse? Data { get; private set; }

        public static PatientProcedureOrderResult Success(PatientProcedureResponse data, string message) =>
            new() { IsSuccess = true, StatusCode = StatusCodes.Status200OK, Data = data, Message = message };

        /// <summary>
        /// Pesanan baru tersimpan — <c>201</c> menurut api-contract 0.6.0 bagian 12.5. Kiriman ulang
        /// berkunci sama memakai <see cref="Success"/> (<c>200</c>).
        /// </summary>
        public static PatientProcedureOrderResult Created(PatientProcedureResponse data, string message) =>
            new() { IsSuccess = true, StatusCode = StatusCodes.Status201Created, Data = data, Message = message };

        public static PatientProcedureOrderResult BadRequest(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status400BadRequest, Message = message };

        public static PatientProcedureOrderResult Forbidden(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status403Forbidden, Message = message };

        public static PatientProcedureOrderResult NotFound(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status404NotFound, Message = message };

        public static PatientProcedureOrderResult Conflict(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status409Conflict, Message = message };

        public static PatientProcedureOrderResult UnprocessableEntity(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status422UnprocessableEntity, Message = message };
    }
}
