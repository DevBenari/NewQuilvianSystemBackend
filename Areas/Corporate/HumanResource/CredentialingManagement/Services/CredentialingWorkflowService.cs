using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public class CredentialingWorkflowService : ICredentialingWorkflowService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CredentialingWorkflowService> _logger;

        public CredentialingWorkflowService(
            ApplicationDbContext context,
            ILogger<CredentialingWorkflowService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CredentialingApplicationDto> CreateApplicationAsync(
            CreateCredentialingApplicationDto request,
            Guid applicantUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId, cancellationToken);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja dengan ID {request.WorkforceProfileId} tidak ditemukan.");
            }

            var committee = await _context.MstCredentialingCommittees
                .FirstOrDefaultAsync(c => c.Id == request.CommitteeId, cancellationToken);

            if (committee == null)
            {
                throw new KeyNotFoundException($"Komite Kredensial dengan ID {request.CommitteeId} tidak ditemukan.");
            }

            // Validasi VR-CRD-02 & M09-P0-05: Nakes wajib memiliki SIP aktif di faskes ini yang telah lolos verifikasi sumber primer (PSV)
            var hasPsvVerifiedSip = await _context.WfpCredentialLicenses
                .AnyAsync(l => l.WorkforceProfileId == request.WorkforceProfileId
                            && l.LicenseType == "SIP"
                            && l.IsActive && !l.IsRevoked
                            && l.IsPsvVerified
                            && l.ExpiredDate >= DateTime.UtcNow, cancellationToken);

            if (!hasPsvVerifiedSip)
            {
                throw new InvalidOperationException(
                    "Permohonan kredensial tidak dapat diajukan (VR-CRD-02, M09-P0-05). Tenaga medis/kesehatan wajib memiliki Surat Izin Praktik (SIP) aktif di faskes ini yang telah terverifikasi keabsahannya via Primary Source Verification (PSV) ke portal SatuSehat SDMK / KKI.");
            }

            // Validasi cegah permohonan ganda yang masih berjalan
            var hasPendingApp = await _context.TrxCredentialingApplications
                .AnyAsync(a => a.WorkforceProfileId == request.WorkforceProfileId
                            && a.IsActive && !a.IsDelete
                            && (a.ApplicationStatus == "Submitted" || a.ApplicationStatus == "InReview"), cancellationToken);
            if (hasPendingApp)
            {
                throw new InvalidOperationException("Tenaga medis/kesehatan masih memiliki permohonan kredensial yang sedang diproses (Submitted / InReview).");
            }

            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
            var countThisMonth = await _context.TrxCredentialingApplications
                .CountAsync(a => a.ApplicationNumber.StartsWith($"APP-CRED-{yearMonth}"), cancellationToken);
            var appNumber = $"APP-CRED-{yearMonth}-{(countThisMonth + 1):D4}";

            var application = new TrxCredentialingApplication
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                ProfessionId = request.ProfessionId,
                SpecializationId = request.SpecializationId,
                ApplicationNumber = appNumber,
                ApplicationType = request.ApplicationType,
                ApplicationStatus = "Submitted",
                ApplicationDate = DateTime.UtcNow,
                IsLicenseCompliant = true,
                IsPrivilegeCompliant = false,
                BlocksScheduling = true,
                BlocksClinicalService = true,
                WorkforceSnapshotJson = JsonSerializer.Serialize(new
                {
                    ProfileId = profile.Id,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    CommitteeId = committee.Id,
                    CommitteeCode = committee.CommitteeCode,
                    CommitteeName = committee.CommitteeName
                }),
                RequirementSnapshotJson = request.RequestedPrivilegeCatalogIds != null && request.RequestedPrivilegeCatalogIds.Any()
                    ? JsonSerializer.Serialize(request.RequestedPrivilegeCatalogIds)
                    : null,
                Notes = request.Notes,
                SubmittedAt = DateTime.UtcNow,
                SubmittedByUserId = applicantUserId,
                IsActive = true,
                CreateBy = applicantUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxCredentialingApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Permohonan kredensial #{AppNumber} untuk nakes {Applicant} berhasil diajukan ke {Committee}.",
                application.ApplicationNumber, profile.DisplayName, committee.CommitteeName);

            return MapToDto(application, profile, committee, new List<string>());
        }

        public async Task<CredentialingApplicationDto> AssignPeerReviewersAsync(
            AssignPeerReviewersDto request,
            Guid assignedByUserId,
            CancellationToken cancellationToken = default)
        {
            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            // ATURAN ANTI-SELF-REVIEW (VR-CRD-15): Pemohon dilarang menilai dirinya sendiri
            if (request.ReviewerWorkforceProfileIds.Contains(application.WorkforceProfileId))
            {
                throw new InvalidOperationException(
                    "Pelanggaran integritas audit klinis (VR-CRD-15): Pemohon kredensial dilarang keras ditunjuk menjadi anggota panel Mitra Bestari (Peer Reviewer) untuk menilai portofolio klinis dirinya sendiri.");
            }

            // Validasi jumlah reviewer minimal 2 orang unik (HRD-DEC-147)
            var distinctReviewerIds = request.ReviewerWorkforceProfileIds.Distinct().ToList();
            if (distinctReviewerIds.Count < 2)
            {
                throw new InvalidOperationException(
                    "Panel Mitra Bestari wajib beranggotakan minimal 2 (dua) orang dokter/nakes sejawat seprofesi yang berbeda (HRD-DEC-147).");
            }

            var reviewers = await _context.MstWorkforceProfiles
                .Where(p => request.ReviewerWorkforceProfileIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var reviewerNames = reviewers.Select(r => r.DisplayName).ToList();

            var assignmentRecord = new
            {
                AssignedAt = DateTime.UtcNow,
                AssignedByUserId = assignedByUserId,
                ReviewerProfiles = reviewers.Select(r => new { r.Id, r.DisplayName, r.ProfileCode }),
                Notes = request.AssignmentNotes
            };

            application.ComplianceResultJson = JsonSerializer.Serialize(assignmentRecord);
            application.ApplicationStatus = "InReview";
            application.UpdateBy = assignedByUserId;
            application.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Panel Mitra Bestari sebanyak {Count} orang berhasil ditugaskan untuk permohonan #{AppNumber}.",
                reviewers.Count, application.ApplicationNumber);

            return MapToDto(application, application.WorkforceProfile, null, reviewerNames);
        }

        public async Task<bool> SubmitPeerReviewAssessmentAsync(
            SubmitPeerReviewAssessmentDto request,
            Guid assessorUserId,
            CancellationToken cancellationToken = default,
            Guid? assessorWorkforceProfileId = null)
        {
            var application = await _context.TrxCredentialingApplications
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            // ATURAN ANTI-SELF-REVIEW (VR-CRD-15)
            if (request.ReviewerWorkforceProfileId == application.WorkforceProfileId
                || (assessorWorkforceProfileId.HasValue && assessorWorkforceProfileId.Value == application.WorkforceProfileId))
            {
                throw new InvalidOperationException(
                    "Pelanggaran integritas audit klinis (VR-CRD-15): Pemohon dilarang melakukan asesmen terhadap dirinya sendiri.");
            }

            // MSL-16 (19 Nov 2026): sebelumnya ID reviewer dari body dipercaya begitu saja, sehingga pemohon bisa mengirim
            // penilaian atas nama reviewer lain. Kini penilaian hanya diterima dari anggota panel yang ditugaskan, atas nama
            // akunnya sendiri, satu kali per akun, dan hanya selama permohonan berstatus InReview.
            if (!string.Equals(application.ApplicationStatus, CredentialingWorklistQueryService.StatusInReview, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Penilaian Mitra Bestari hanya dapat dikirim saat permohonan sedang ditelaah panel (status InReview).");
            }

            if (assessorWorkforceProfileId.HasValue && assessorWorkforceProfileId.Value != request.ReviewerWorkforceProfileId)
            {
                throw new InvalidOperationException("Penilaian Mitra Bestari hanya boleh dikirim atas nama akun Anda sendiri.");
            }

            var panel = CredentialingWorklistQueryService.ReadComplianceSnapshot(application.ComplianceResultJson).Reviewers;
            if (!panel.Any(r => r.Id == request.ReviewerWorkforceProfileId))
            {
                throw new InvalidOperationException("Anda bukan anggota panel Mitra Bestari yang ditugaskan untuk permohonan ini.");
            }

            var alreadyAssessed = await _context.TrxClinicalPrivilegeAssessments
                .AnyAsync(a => a.ClinicalPrivilegeRequestId == application.Id
                    && a.AssessmentType == CredentialingWorklistQueryService.PeerAssessmentType
                    && a.AssessorUserId == assessorUserId
                    && a.IsActive && !a.IsDelete, cancellationToken);
            if (alreadyAssessed)
            {
                throw new InvalidOperationException("Anda sudah mengirim penilaian untuk permohonan ini.");
            }

            // Simpan rincian asesmen ke TrxClinicalPrivilegeAssessment
            var assessment = new TrxClinicalPrivilegeAssessment
            {
                Id = Guid.NewGuid(),
                ClinicalPrivilegeRequestId = application.Id, // Menggunakan Application ID sebagai relasi permohonan
                AssessorUserId = assessorUserId,
                AssessmentType = "PeerReviewPortofolio",
                AssessmentDate = DateTime.UtcNow,
                AssessmentResult = "Completed",
                CompetencyResultJson = JsonSerializer.Serialize(new
                {
                    ReviewerProfileId = request.ReviewerWorkforceProfileId,
                    AssessorUserId = assessorUserId,
                    Items = request.AssessmentItems,
                    OverallNotes = request.OverallRecommendation
                }),
                Recommendation = request.OverallRecommendation ?? "Rekomendasi kewenangan klinis sesuai hasil telaah portofolio.",
                RequiresSupervision = request.AssessmentItems.Any(i => i.RecommendedSupervisionLevel == SupervisionLevel.Level2_UnderSupervision || i.RecommendedSupervisionLevel == SupervisionLevel.Level3_Proctored),
                IsActive = true,
                CreateBy = assessorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxClinicalPrivilegeAssessments.Add(assessment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Asesmen Mitra Bestari berhasil direkam untuk permohonan #{AppNumber} oleh reviewer {ReviewerId}",
                application.ApplicationNumber, request.ReviewerWorkforceProfileId);

            return true;
        }

        public async Task<PlenarySessionResultDto> ConcludePlenarySessionAsync(
            ConcludePlenarySessionDto request,
            Guid chairpersonUserId,
            CancellationToken cancellationToken = default)
        {
            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            // MSL-16 (19 Nov 2026): pleno hanya untuk permohonan yang sedang ditelaah panel, agar BAP tidak tertimpa dua kali.
            if (!string.Equals(application.ApplicationStatus, CredentialingWorklistQueryService.StatusInReview, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Sidang pleno hanya dapat disahkan untuk permohonan yang sedang ditelaah panel Mitra Bestari. Status saat ini: {application.ApplicationStatus}.");
            }

            // Validasi penilaian Mitra Bestari minimal 2 orang telah masuk
            var assessmentCount = await _context.TrxClinicalPrivilegeAssessments
                .CountAsync(a => a.ClinicalPrivilegeRequestId == application.Id
                              && a.AssessmentType == CredentialingWorklistQueryService.PeerAssessmentType
                              && a.IsActive && !a.IsDelete, cancellationToken);
            if (assessmentCount < 2)
            {
                throw new InvalidOperationException(
                    $"Sidang pleno belum dapat disahkan: minimal 2 penilaian dari panel Mitra Bestari wajib selesai (saat ini baru {assessmentCount} penilaian masuk).");
            }

            // FIN-HRD-09-BE-006 & M09-P0-06: Pertahankan riwayat penugasan reviewer yang ada di ComplianceResultJson
            object? existingReviewers = null;
            if (!string.IsNullOrWhiteSpace(application.ComplianceResultJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(application.ComplianceResultJson);
                    if (doc.RootElement.TryGetProperty("ReviewerProfiles", out var revs))
                    {
                        existingReviewers = JsonSerializer.Deserialize<object>(revs.GetRawText());
                    }
                }
                catch
                {
                    // Abaikan galat pembacaan JSON lama
                }
            }

            var independentCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level1_Independent);
            var supervisedCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level2_UnderSupervision || p.RecommendedSupervisionLevel == SupervisionLevel.Level3_Proctored);
            var deniedCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level4_NotPermitted);

            var plenaryRecord = new
            {
                ReviewerProfiles = existingReviewers, // Pertahankan daftar reviewer penugasan
                PlenaryBapNumber = request.PlenaryBapNumber,
                PlenaryDate = request.PlenaryDate,
                Decision = request.PlenaryDecision,
                ChairpersonNotes = request.CommitteeChairpersonNotes,
                EvaluatedCount = request.FinalApprovedPrivileges.Count,
                IndependentCount = independentCount,
                SupervisedCount = supervisedCount,
                DeniedCount = deniedCount,
                FinalApprovedPrivileges = request.FinalApprovedPrivileges,
                ConcludedAt = DateTime.UtcNow,
                ConcludedByUserId = chairpersonUserId
            };

            application.ComplianceResultJson = JsonSerializer.Serialize(plenaryRecord);
            application.ApplicationStatus = "PlenarySessionConcluded";
            application.ClosedAt = DateTime.UtcNow;
            application.ClosedByUserId = chairpersonUserId;
            application.CompletedDate = DateTime.UtcNow;
            application.IsPrivilegeCompliant = request.PlenaryDecision.Equals("Recommended", StringComparison.OrdinalIgnoreCase);
            application.UpdateBy = chairpersonUserId;
            application.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sidang Pleno Komite Kredensial untuk permohonan #{AppNumber} telah selesai ber-BAP No. {BapNumber}. Keputusan: {Decision}",
                application.ApplicationNumber, request.PlenaryBapNumber, request.PlenaryDecision);

            return new PlenarySessionResultDto
            {
                ApplicationId = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                PlenaryBapNumber = request.PlenaryBapNumber,
                PlenaryDate = request.PlenaryDate,
                PlenaryDecision = request.PlenaryDecision,
                TotalPrivilegesEvaluated = request.FinalApprovedPrivileges.Count,
                TotalIndependentPrivileges = independentCount,
                TotalSupervisedPrivileges = supervisedCount,
                TotalDeniedPrivileges = deniedCount,
                Status = application.ApplicationStatus
            };
        }

        public async Task<CredentialingApplicationDto> GetApplicationDetailAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default)
        {
            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {applicationId} tidak ditemukan.");
            }

            var reviewerNames = new List<string>();
            if (!string.IsNullOrWhiteSpace(application.ComplianceResultJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(application.ComplianceResultJson);
                    if (doc.RootElement.TryGetProperty("ReviewerProfiles", out var revs))
                    {
                        foreach (var r in revs.EnumerateArray())
                        {
                            if (r.TryGetProperty("DisplayName", out var nameProp))
                            {
                                reviewerNames.Add(nameProp.GetString() ?? "");
                            }
                        }
                    }
                }
                catch
                {
                    // abaikan parsing error jika JSON berformat plenary session
                }
            }

            return MapToDto(application, application.WorkforceProfile, null, reviewerNames);
        }

        public async Task<List<CredentialingApplicationDto>> GetApplicationsByCommitteeAsync(
            Guid committeeId,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .Where(a => a.IsActive && !a.IsDelete);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.ApplicationStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var applications = await query
                .OrderByDescending(a => a.CreateDateTime)
                .ToListAsync(cancellationToken);

            return applications.Select(a => MapToDto(a, a.WorkforceProfile, null, new List<string>())).ToList();
        }

        private static CredentialingApplicationDto MapToDto(
            TrxCredentialingApplication app,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstWorkforceProfile? profile,
            MstCredentialingCommittee? committee,
            List<string> reviewerNames)
        {
            return new CredentialingApplicationDto
            {
                Id = app.Id,
                ApplicationNumber = app.ApplicationNumber,
                WorkforceProfileId = app.WorkforceProfileId,
                ApplicantName = profile?.DisplayName ?? "-",
                ApplicantEmployeeCode = profile?.ProfileCode ?? "-",
                CommitteeCode = committee?.CommitteeCode ?? "-",
                CommitteeName = committee?.CommitteeName ?? "-",
                ApplicationType = app.ApplicationType,
                ApplicationStatus = app.ApplicationStatus,
                ApplicationDate = app.ApplicationDate,
                IsLicensePsvVerified = app.IsLicenseCompliant,
                AssignedReviewerNames = reviewerNames,
                Notes = app.Notes
            };
        }
    }
}
