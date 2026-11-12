using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Alur Kerja FPPE, Logbook Tindakan Klinis, dan Sidang Pleno Komite Medik
    /// </summary>
    public class ProctoringWorkflowService : IProctoringWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberSeriesAllocator _numberAllocator;

        public ProctoringWorkflowService(ApplicationDbContext context, NumberSeriesAllocator numberAllocator)
        {
            _context = context;
            _numberAllocator = numberAllocator;
        }

        public async Task<FppeEngagementResponseDto> CreateEngagementAsync(CreateFppeEngagementRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validasi Dokter Target
            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .Include(p => p.PrimaryDepartment)
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (staffProfile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja dengan ID {request.WorkforceProfileId} tidak ditemukan.");
            }

            // 2. Validasi Senioritas Dokter Proctor (VR-OPP-08)
            var proctorProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == request.AssignedProctorWorkforceId, cancellationToken);

            if (proctorProfile == null)
            {
                throw new KeyNotFoundException($"Profil Dokter Proctor pendamping dengan ID {request.AssignedProctorWorkforceId} tidak ditemukan.");
            }

            var proctorDoctor = await _context.Set<MstDoctor>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == request.AssignedProctorWorkforceId, cancellationToken);

            var proctorStartDate = proctorDoctor?.JoinDate ?? proctorProfile.CreateDateTime;
            var experienceYears = (DateTime.UtcNow - proctorStartDate).TotalDays / 365.25;

            // Jika ada riwayat kerja dan masa kerja kurang dari 3 tahun
            if (proctorDoctor != null && experienceYears < 3.0 && proctorDoctor.JoinDate.HasValue)
            {
                throw new InvalidOperationException("Dokter proctor FPPE wajib merupakan dokter spesialis senior berpengalaman minimal 3 tahun (VR-OPP-08).");
            }

            // 3. Validasi Target Kasus & Durasi Sesuai Kategori
            var startDate = request.StartDate;
            DateTime targetEndDate;

            if (request.TriggerSource == "NEW_STAFF")
            {
                // VR-OPP-09: Staf Baru minimal 10 kasus & minimal 3 bulan (90 hari)
                targetEndDate = request.TargetEndDate ?? startDate.AddDays(90);
                var durationDays = (targetEndDate - startDate).TotalDays;

                if (request.TargetCaseCount < 10 || durationDays < 89)
                {
                    throw new InvalidOperationException("Proctoring staf baru wajib memantau minimal 10 kasus klinis pertama dalam kurun waktu 3 bulan (VR-OPP-09).");
                }
            }
            else if (request.TriggerSource == "NEW_PRIVILEGE_PROCTORING")
            {
                // VR-OPP-10: Prosedur Baru minimal 5 kasus & minimal 6 bulan (180 hari)
                targetEndDate = request.TargetEndDate ?? startDate.AddDays(180);
                var durationDays = (targetEndDate - startDate).TotalDays;

                if (request.TargetCaseCount < 5 || durationDays < 179)
                {
                    throw new InvalidOperationException("Proctoring penambahan prosedur baru wajib memantau minimal 5 kasus dalam kurun waktu 6 bulan (VR-OPP-10).");
                }
            }
            else
            {
                targetEndDate = request.TargetEndDate ?? startDate.AddDays(90);
                if (request.TargetCaseCount <= 0)
                {
                    request.TargetCaseCount = 10;
                }
            }

            // 4. Pembentukan Penugasan FPPE
            // Nomor urut bulanan dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: FPPE-202611-0001.
            // Kontrak pembuatan penugasan belum membawa identitas pengguna, sehingga stempel aktor deret memakai aktor sistem.
            var engagementNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: PerformanceNumberSeries.FppeEngagementKey,
                Prefix: PerformanceNumberSeries.FppeEngagementPrefix,
                ResetPolicy: PerformanceNumberSeries.FppeEngagementResetPolicy,
                SequenceDigits: PerformanceNumberSeries.FppeEngagementDigits,
                ActorUserId: PerformanceNumberSeries.SystemFallbackUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var engagement = new HrdFppeEngagement
            {
                Id = Guid.NewGuid(),
                EngagementNumber = engagementNumber,
                WorkforceProfileId = request.WorkforceProfileId,
                PrivilegeCatalogId = request.PrivilegeCatalogId,
                TriggerSource = request.TriggerSource,
                InitialReasonJustification = request.InitialReasonJustification,
                AssignedProctorWorkforceId = request.AssignedProctorWorkforceId,
                StartDate = startDate,
                TargetEndDate = targetEndDate,
                TargetCaseCount = request.TargetCaseCount,
                CompletedCaseCount = 0,
                Status = "ACTIVE",
                CreateDateTime = DateTime.UtcNow
            };

            _context.HrdFppeEngagements.Add(engagement);
            await _context.SaveChangesAsync(cancellationToken);

            return new FppeEngagementResponseDto
            {
                Id = engagement.Id,
                EngagementNumber = engagement.EngagementNumber,
                WorkforceProfileId = engagement.WorkforceProfileId,
                StaffName = staffProfile.DisplayName,
                StaffCode = staffProfile.ProfileCode,
                StaffDepartment = staffProfile.PrimaryDepartment?.DepartmentName ?? "Klinis",
                PrivilegeCatalogId = engagement.PrivilegeCatalogId,
                TriggerSource = engagement.TriggerSource,
                InitialReasonJustification = engagement.InitialReasonJustification,
                AssignedProctorWorkforceId = engagement.AssignedProctorWorkforceId,
                ProctorName = proctorProfile.DisplayName,
                StartDate = engagement.StartDate,
                TargetEndDate = engagement.TargetEndDate,
                TargetCaseCount = engagement.TargetCaseCount,
                CompletedCaseCount = engagement.CompletedCaseCount,
                Status = engagement.Status,
                CaseLogs = new List<FppeCaseLogResponseDto>()
            };
        }

        public async Task<FppeEngagementResponseDto?> GetEngagementByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var engagement = await _context.HrdFppeEngagements
                .Include(e => e.CaseLogs)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (engagement == null) return null;

            var staffProfile = await _context.Set<MstWorkforceProfile>()
                .Include(p => p.PrimaryDepartment)
                .FirstOrDefaultAsync(p => p.Id == engagement.WorkforceProfileId, cancellationToken);

            var proctorProfile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(p => p.Id == engagement.AssignedProctorWorkforceId, cancellationToken);

            var conclusion = await _context.HrdFppeEvaluationConclusions
                .FirstOrDefaultAsync(c => c.FppeEngagementId == id, cancellationToken);

            var caseLogsDto = engagement.CaseLogs
                .OrderBy(c => c.ProcedureDate)
                .Select(c => new FppeCaseLogResponseDto
                {
                    Id = c.Id,
                    FppeEngagementId = c.FppeEngagementId,
                    SimrsPatientEpisodeId = c.SimrsPatientEpisodeId,
                    ProcedurePerformed = c.ProcedurePerformed,
                    ProcedureDate = c.ProcedureDate,
                    TechnicalSkillScore = c.TechnicalSkillScore,
                    CriticalDecisionScore = c.CriticalDecisionScore,
                    HasComplication = c.HasComplication,
                    ComplicationDescription = c.ComplicationDescription,
                    ProctorFeedbackNotes = c.ProctorFeedbackNotes,
                    ObservedIndependenceLevel = c.ObservedIndependenceLevel,
                    IsVerifiedByProctor = c.IsVerifiedByProctor,
                    ProctorVerifiedAt = c.ProctorVerifiedAt
                }).ToList();

            FppeConclusionResponseDto? conclusionDto = null;
            if (conclusion != null)
            {
                conclusionDto = new FppeConclusionResponseDto
                {
                    Id = conclusion.Id,
                    FppeEngagementId = conclusion.FppeEngagementId,
                    RecommendedAction = conclusion.RecommendedAction,
                    PlenaryReviewSummary = conclusion.PlenaryReviewSummary,
                    RemediationPlan = conclusion.RemediationPlan,
                    ApprovedByCommitteeChairmanId = conclusion.ApprovedByCommitteeChairmanId,
                    FinalizedAt = conclusion.FinalizedAt,
                    SignatureHash = conclusion.SignatureHash
                };
            }

            return new FppeEngagementResponseDto
            {
                Id = engagement.Id,
                EngagementNumber = engagement.EngagementNumber,
                WorkforceProfileId = engagement.WorkforceProfileId,
                StaffName = staffProfile?.DisplayName ?? "Dokter Target",
                StaffCode = staffProfile?.ProfileCode ?? "-",
                StaffDepartment = staffProfile?.PrimaryDepartment?.DepartmentName ?? "Klinis",
                PrivilegeCatalogId = engagement.PrivilegeCatalogId,
                TriggerSource = engagement.TriggerSource,
                InitialReasonJustification = engagement.InitialReasonJustification,
                AssignedProctorWorkforceId = engagement.AssignedProctorWorkforceId,
                ProctorName = proctorProfile?.DisplayName ?? "Dokter Proctor",
                StartDate = engagement.StartDate,
                TargetEndDate = engagement.TargetEndDate,
                TargetCaseCount = engagement.TargetCaseCount,
                CompletedCaseCount = engagement.CompletedCaseCount,
                Status = engagement.Status,
                CaseLogs = caseLogsDto,
                Conclusion = conclusionDto
            };
        }

        public async Task<FppeCaseLogResponseDto> AddCaseLogAsync(Guid engagementId, CreateFppeCaseLogRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var engagement = await _context.HrdFppeEngagements
                .FirstOrDefaultAsync(e => e.Id == engagementId, cancellationToken);

            if (engagement == null)
            {
                throw new KeyNotFoundException($"Penugasan FPPE dengan ID {engagementId} tidak ditemukan.");
            }

            if (engagement.Status != "ACTIVE" && engagement.Status != "EXTENDED")
            {
                throw new InvalidOperationException($"Penugasan FPPE dengan status {engagement.Status} tidak menerima penambahan log kasus.");
            }

            // Pseudonimisasi Rekam Medis Pasien (VR-OPP-11)
            var pseudonymizedEpisodeId = AnonymizePatientEpisodeId(request.SimrsPatientEpisodeId);

            var caseLog = new HrdFppeCaseLog
            {
                Id = Guid.NewGuid(),
                FppeEngagementId = engagementId,
                SimrsPatientEpisodeId = pseudonymizedEpisodeId,
                ProcedurePerformed = request.ProcedurePerformed,
                ProcedureDate = request.ProcedureDate,
                TechnicalSkillScore = request.TechnicalSkillScore,
                CriticalDecisionScore = request.CriticalDecisionScore,
                HasComplication = request.HasComplication,
                ComplicationDescription = request.ComplicationDescription,
                ProctorFeedbackNotes = request.ProctorFeedbackNotes,
                ObservedIndependenceLevel = request.ObservedIndependenceLevel,
                IsVerifiedByProctor = false, // Menunggu verifikasi eksplisit proctor
                ProctorVerifiedAt = null,
                CreateDateTime = DateTime.UtcNow
            };

            _context.HrdFppeCaseLogs.Add(caseLog);
            await _context.SaveChangesAsync(cancellationToken);

            return new FppeCaseLogResponseDto
            {
                Id = caseLog.Id,
                FppeEngagementId = caseLog.FppeEngagementId,
                SimrsPatientEpisodeId = caseLog.SimrsPatientEpisodeId,
                ProcedurePerformed = caseLog.ProcedurePerformed,
                ProcedureDate = caseLog.ProcedureDate,
                TechnicalSkillScore = caseLog.TechnicalSkillScore,
                CriticalDecisionScore = caseLog.CriticalDecisionScore,
                HasComplication = caseLog.HasComplication,
                ComplicationDescription = caseLog.ComplicationDescription,
                ProctorFeedbackNotes = caseLog.ProctorFeedbackNotes,
                ObservedIndependenceLevel = caseLog.ObservedIndependenceLevel,
                IsVerifiedByProctor = caseLog.IsVerifiedByProctor,
                ProctorVerifiedAt = caseLog.ProctorVerifiedAt
            };
        }

        public async Task<FppeCaseLogResponseDto> VerifyCaseLogAsync(Guid logId, VerifyFppeCaseLogRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var caseLog = await _context.HrdFppeCaseLogs
                .Include(c => c.FppeEngagement)
                .FirstOrDefaultAsync(c => c.Id == logId, cancellationToken);

            if (caseLog == null)
            {
                throw new KeyNotFoundException($"Log kasus FPPE dengan ID {logId} tidak ditemukan.");
            }

            var engagement = caseLog.FppeEngagement;
            if (engagement == null)
            {
                throw new InvalidOperationException("Relasi penugasan FPPE untuk log kasus ini tidak valid.");
            }

            // Update evaluasi hasil pengamatan proctor
            caseLog.TechnicalSkillScore = request.TechnicalSkillScore;
            caseLog.CriticalDecisionScore = request.CriticalDecisionScore;
            caseLog.ObservedIndependenceLevel = request.ObservedIndependenceLevel;
            caseLog.HasComplication = request.HasComplication;
            caseLog.ComplicationDescription = request.ComplicationDescription;
            caseLog.ProctorFeedbackNotes = request.ProctorFeedbackNotes;
            caseLog.IsVerifiedByProctor = true;
            caseLog.ProctorVerifiedAt = DateTime.UtcNow;
            caseLog.UpdateDateTime = DateTime.UtcNow;

            // Update counter CompletedCaseCount di entitas penugasan
            var completedCount = await _context.HrdFppeCaseLogs
                .CountAsync(c => c.FppeEngagementId == engagement.Id && (c.Id == logId || c.IsVerifiedByProctor), cancellationToken);

            engagement.CompletedCaseCount = completedCount;
            engagement.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new FppeCaseLogResponseDto
            {
                Id = caseLog.Id,
                FppeEngagementId = caseLog.FppeEngagementId,
                SimrsPatientEpisodeId = caseLog.SimrsPatientEpisodeId,
                ProcedurePerformed = caseLog.ProcedurePerformed,
                ProcedureDate = caseLog.ProcedureDate,
                TechnicalSkillScore = caseLog.TechnicalSkillScore,
                CriticalDecisionScore = caseLog.CriticalDecisionScore,
                HasComplication = caseLog.HasComplication,
                ComplicationDescription = caseLog.ComplicationDescription,
                ProctorFeedbackNotes = caseLog.ProctorFeedbackNotes,
                ObservedIndependenceLevel = caseLog.ObservedIndependenceLevel,
                IsVerifiedByProctor = caseLog.IsVerifiedByProctor,
                ProctorVerifiedAt = caseLog.ProctorVerifiedAt
            };
        }

        public async Task<FppeConclusionResponseDto> ConcludeEngagementAsync(Guid engagementId, ConcludeFppeEngagementRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var engagement = await _context.HrdFppeEngagements
                .Include(e => e.CaseLogs)
                .FirstOrDefaultAsync(e => e.Id == engagementId, cancellationToken);

            if (engagement == null)
            {
                throw new KeyNotFoundException($"Penugasan FPPE dengan ID {engagementId} tidak ditemukan.");
            }

            // 1. Validasi Seluruh Kasus Wajib Diverifikasi Proctor Sebelum Sidang (VR-OPP-12)
            var unverifiedCases = engagement.CaseLogs.Where(c => !c.IsVerifiedByProctor).ToList();
            if (unverifiedCases.Any())
            {
                throw new InvalidOperationException("Seluruh catatan kasus dalam logbook proctoring wajib diverifikasi oleh dokter pendamping (VR-OPP-12).");
            }

            // 2. Validasi Otorisasi Pengesahan Sidang Pleno (VR-OPP-14)
            if (request.ApprovedByCommitteeChairmanId == Guid.Empty)
            {
                throw new InvalidOperationException("Putusan evaluasi terarah FPPE wajib disahkan secara resmi oleh sidang pleno Komite Medik (VR-OPP-14).");
            }

            // 3. Validasi Batasan 4 Vonis Baku & Batas Perpanjangan Proctoring (VR-OPP-13)
            var validActions = new[] { "GRANT_INDEPENDENT", "EXTEND_SUPERVISION", "RESTRICT_PRIVILEGE", "REVOKE_PRIVILEGE" };
            if (!validActions.Contains(request.RecommendedAction))
            {
                throw new ArgumentException($"Rekomendasi vonis '{request.RecommendedAction}' tidak sah. Pilihan baku: GRANT_INDEPENDENT, EXTEND_SUPERVISION, RESTRICT_PRIVILEGE, REVOKE_PRIVILEGE.");
            }

            if (request.RecommendedAction == "EXTEND_SUPERVISION")
            {
                // VR-OPP-13: Perpanjangan maksimal 3 bulan dan hanya boleh 1 kali
                if (engagement.Status == "EXTENDED")
                {
                    throw new InvalidOperationException("Perpanjangan observasi proctoring FPPE dibatasi maksimal 3 bulan untuk 1 kali kesempatan (VR-OPP-13).");
                }

                if (request.ExtensionMonths > 3)
                {
                    throw new InvalidOperationException("Perpanjangan observasi proctoring FPPE dibatasi maksimal 3 bulan untuk 1 kali kesempatan (VR-OPP-13).");
                }

                engagement.Status = "EXTENDED";
                engagement.TargetEndDate = engagement.TargetEndDate.AddMonths(request.ExtensionMonths);
            }
            else if (request.RecommendedAction == "GRANT_INDEPENDENT")
            {
                engagement.Status = "COMPLETED_SUCCESS";
            }
            else if (request.RecommendedAction == "RESTRICT_PRIVILEGE" || request.RecommendedAction == "REVOKE_PRIVILEGE")
            {
                engagement.Status = "FAILED_RESTRICTED";
            }

            // 4. Pembentukan Digital Hash Tanda Tangan Pleno
            var finalizedAt = DateTime.UtcNow;
            var signaturePayload = $"{engagementId}|{request.RecommendedAction}|{request.ApprovedByCommitteeChairmanId}|{finalizedAt:O}|QUILVIAN-FPPE-PLENARY";
            var signatureHash = ComputeSha256Hex(signaturePayload);

            var existingConclusion = await _context.HrdFppeEvaluationConclusions
                .FirstOrDefaultAsync(c => c.FppeEngagementId == engagementId, cancellationToken);

            if (existingConclusion == null)
            {
                existingConclusion = new HrdFppeEvaluationConclusion
                {
                    Id = Guid.NewGuid(),
                    FppeEngagementId = engagementId,
                    RecommendedAction = request.RecommendedAction,
                    PlenaryReviewSummary = request.PlenaryReviewSummary,
                    RemediationPlan = request.RemediationPlan,
                    ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId,
                    FinalizedAt = finalizedAt,
                    SignatureHash = signatureHash,
                    CreateDateTime = finalizedAt
                };
                _context.HrdFppeEvaluationConclusions.Add(existingConclusion);
            }
            else
            {
                existingConclusion.RecommendedAction = request.RecommendedAction;
                existingConclusion.PlenaryReviewSummary = request.PlenaryReviewSummary;
                existingConclusion.RemediationPlan = request.RemediationPlan;
                existingConclusion.ApprovedByCommitteeChairmanId = request.ApprovedByCommitteeChairmanId;
                existingConclusion.FinalizedAt = finalizedAt;
                existingConclusion.SignatureHash = signatureHash;
                existingConclusion.UpdateDateTime = finalizedAt;
            }

            engagement.UpdateDateTime = finalizedAt;
            await _context.SaveChangesAsync(cancellationToken);

            return new FppeConclusionResponseDto
            {
                Id = existingConclusion.Id,
                FppeEngagementId = existingConclusion.FppeEngagementId,
                RecommendedAction = existingConclusion.RecommendedAction,
                PlenaryReviewSummary = existingConclusion.PlenaryReviewSummary,
                RemediationPlan = existingConclusion.RemediationPlan,
                ApprovedByCommitteeChairmanId = existingConclusion.ApprovedByCommitteeChairmanId,
                FinalizedAt = existingConclusion.FinalizedAt,
                SignatureHash = existingConclusion.SignatureHash
            };
        }

        public async Task<SyncFppePrivilegeResponseDto> SyncPrivilegeAsync(Guid engagementId, SyncFppePrivilegeRequestDto request, CancellationToken cancellationToken = default)
        {
            var engagement = await _context.HrdFppeEngagements
                .FirstOrDefaultAsync(e => e.Id == engagementId, cancellationToken);

            if (engagement == null)
            {
                throw new KeyNotFoundException($"Penugasan FPPE dengan ID {engagementId} tidak ditemukan.");
            }

            var conclusion = await _context.HrdFppeEvaluationConclusions
                .FirstOrDefaultAsync(c => c.FppeEngagementId == engagementId, cancellationToken);

            if (conclusion == null)
            {
                throw new InvalidOperationException("Penugasan FPPE belum memiliki putusan sidang pleno yang disahkan.");
            }

            var doctor = await _context.Set<MstDoctor>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == engagement.WorkforceProfileId, cancellationToken);

            string privilegeStatusStr;
            string syncMsg;

            switch (conclusion.RecommendedAction)
            {
                case "GRANT_INDEPENDENT":
                    privilegeStatusStr = "ACTIVE_INDEPENDENT";
                    syncMsg = "Kewenangan klinis mandiri penuh disahkan aktif di SIMRS EMR.";
                    if (doctor != null) doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Active;
                    break;

                case "EXTEND_SUPERVISION":
                    privilegeStatusStr = "SUPERVISED_EXTENDED";
                    syncMsg = "Masa pendampingan proctoring diperpanjang hingga batas observasi baru.";
                    if (doctor != null) doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Active;
                    break;

                case "RESTRICT_PRIVILEGE":
                    privilegeStatusStr = "RESTRICTED_LEVEL_3";
                    syncMsg = "Kewenangan klinis diturunkan ke Level 3 supervisi (Wajib Co-Signature 24 Jam di EMR).";
                    if (doctor != null) doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
                    break;

                case "REVOKE_PRIVILEGE":
                    privilegeStatusStr = "REVOKED_BLOCKED";
                    syncMsg = "Kewenangan klinis dicabut total; hak order resep dan pemesanan OK diblokir permanen (Hard-Stop).";
                    if (doctor != null) doctor.ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Revoked;
                    break;

                default:
                    privilegeStatusStr = "UNKNOWN";
                    syncMsg = "Status wewenang diproses sesuai arahan Komite Medik.";
                    break;
            }

            if (doctor != null)
            {
                doctor.UpdateDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new SyncFppePrivilegeResponseDto
            {
                FppeEngagementId = engagementId,
                RecommendedAction = conclusion.RecommendedAction,
                PrivilegeStatus = privilegeStatusStr,
                SyncStatus = "SUCCESS",
                SyncedAt = DateTime.UtcNow,
                Message = syncMsg
            };
        }

        private static string AnonymizePatientEpisodeId(string rawPatientEpisodeId)
        {
            if (string.IsNullOrWhiteSpace(rawPatientEpisodeId))
            {
                return $"PSEUDO-{Guid.NewGuid().ToString("N")[..8].ToUpper()}-{DateTime.UtcNow:yyyyMMdd}";
            }

            if (rawPatientEpisodeId.StartsWith("PSEUDO-", StringComparison.OrdinalIgnoreCase))
            {
                return rawPatientEpisodeId;
            }

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPatientEpisodeId + "_SALT_QUILVIAN_PATIENT_PRIVACY"));
            var shortHash = Convert.ToHexString(hashBytes)[..8].ToUpper();
            return $"PSEUDO-{shortHash}-{DateTime.UtcNow:yyyyMMdd}";
        }

        private static string ComputeSha256Hex(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
        }
    }
}
