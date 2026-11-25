using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    /// <summary>
    /// Alur permohonan kredensial: pengajuan, panel Mitra Bestari, asesmen, dan sidang pleno.
    /// </summary>
    /// <remarks>
    /// BE-HRD-430/431 (20 November 2026): panel reviewer dan hasil asesmen kini disimpan sebagai baris
    /// <see cref="TrxCredentialingCommitteeReview"/> (satu baris per reviewer, peran <c>PeerReviewer</c>), bukan di
    /// <c>ComplianceResultJson</c> yang ditimpa saat pleno dan bukan di <c>TrxClinicalPrivilegeAssessment</c> yang
    /// foreign key-nya menunjuk tabel permintaan kewenangan lain (M09-P0-06, M09-P0-07).
    /// </remarks>
    public class CredentialingWorkflowService : ICredentialingWorkflowService
    {
        private const string ApplicationSequenceKey = "TRX_CREDENTIALING_APPLICATION";
        private const string ApplicationSeriesPrefix = "APP-CRED";
        private const int ApplicationSequenceDigits = 4;
        private const string ReviewSequenceKey = "TRX_CREDENTIALING_COMMITTEE_REVIEW";
        private const string ReviewSeriesPrefix = "CRV-RSMMC";
        private const int ReviewSequenceDigits = 6;
        private const string StatusSubmitted = "Submitted";

        private static readonly string[] AllowedPlenaryDecisions = { "Recommended", "ConditionallyRecommended", "Rejected" };

        private readonly ApplicationDbContext _context;
        private readonly ILogger<CredentialingWorkflowService> _logger;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public CredentialingWorkflowService(
            ApplicationDbContext context,
            ILogger<CredentialingWorkflowService> logger,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _context = context;
            _logger = logger;
            _numberSeriesAllocator = numberSeriesAllocator ?? throw new ArgumentNullException(nameof(numberSeriesAllocator));
        }

        public async Task<CredentialingApplicationDto> CreateApplicationAsync(
            CreateCredentialingApplicationDto request,
            Guid applicantUserId,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            // BE-HRD-430: dua pengajuan paralel untuk pegawai yang sama diserialkan, sehingga cek "masih ada
            // permohonan berjalan" di bawah ini tidak bisa dilewati oleh race.
            await AcquireTransactionLockAsync($"HRD_CRED_APPLICATION_{request.WorkforceProfileId:N}", cancellationToken);

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

            var now = DateTime.UtcNow;
            var professionId = request.ProfessionId ?? await ResolveProfessionIdAsync(request.WorkforceProfileId, cancellationToken);

            // VR-CRD-02 & M09-P0-05 / BE-HRD-430: SIP yang meloloskan permohonan harus relevan — berjenis SIP, aktif,
            // belum dicabut, lolos PSV, belum kedaluwarsa menurut waktu server, mencatat faskes praktik, dan (bila
            // master jenis lisensinya terikat profesi) seprofesi dengan permohonan.
            var sipQuery = _context.WfpCredentialLicenses
                .AsNoTracking()
                .Where(l => l.WorkforceProfileId == request.WorkforceProfileId
                            && l.LicenseType == "SIP"
                            && l.IsActive && !l.IsRevoked && !l.IsDelete
                            && l.IsPsvVerified
                            && l.ExpiredDate >= now
                            && l.SipFaskesName != null && l.SipFaskesName != "");
            if (professionId.HasValue)
            {
                var pid = professionId.Value;
                sipQuery = sipQuery.Where(l => l.LicenseTypeMaster == null
                                               || l.LicenseTypeMaster.ProfessionId == null
                                               || l.LicenseTypeMaster.ProfessionId == pid);
            }

            if (!await sipQuery.AnyAsync(cancellationToken))
            {
                throw new InvalidOperationException(
                    "Permohonan kredensial tidak dapat diajukan (VR-CRD-02, M09-P0-05). Tenaga medis/kesehatan wajib memiliki Surat Izin Praktik (SIP) aktif yang mencatat faskes praktik, seprofesi dengan permohonan, belum kedaluwarsa, dan telah terverifikasi keabsahannya via Primary Source Verification (PSV).");
            }

            // BE-HRD-430: syarat wajib profesi selain SIP (lisensi/sertifikat bertipe master) harus terpenuhi bukti terverifikasi.
            var missingRequirements = await FindMissingMandatoryRequirementsAsync(
                request.WorkforceProfileId, professionId, request.SpecializationId, now, cancellationToken);
            if (missingRequirements.Count > 0)
            {
                throw new InvalidOperationException(
                    "Permohonan kredensial belum memenuhi syarat wajib profesi: " + string.Join("; ", missingRequirements) +
                    ". Lengkapi dan minta verifikasi dokumennya terlebih dahulu.");
            }

            var hasPendingApp = await _context.TrxCredentialingApplications
                .AnyAsync(a => a.WorkforceProfileId == request.WorkforceProfileId
                            && a.IsActive && !a.IsDelete
                            && (a.ApplicationStatus == StatusSubmitted || a.ApplicationStatus == CredentialingWorklistQueryService.StatusInReview), cancellationToken);
            if (hasPendingApp)
            {
                throw new CredentialingConflictException("Tenaga medis/kesehatan masih memiliki permohonan kredensial yang sedang diproses (Submitted / InReview).");
            }

            var appNumber = await AllocateApplicationNumberAsync(applicantUserId, cancellationToken);

            var application = new TrxCredentialingApplication
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                ProfessionId = professionId,
                SpecializationId = request.SpecializationId,
                ApplicationNumber = appNumber,
                ApplicationType = request.ApplicationType,
                ApplicationStatus = StatusSubmitted,
                ApplicationDate = now,
                IsLicenseCompliant = true,
                IsCertificationCompliant = true,
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
                    ? JsonSerializer.Serialize(request.RequestedPrivilegeCatalogIds.Distinct().ToList())
                    : null,
                Notes = request.Notes,
                SubmittedAt = now,
                SubmittedByUserId = applicantUserId,
                IsActive = true,
                CreateBy = applicantUserId,
                CreateDateTime = now
            };

            _context.TrxCredentialingApplications.Add(application);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await AcquireTransactionLockAsync(PanelLockKey(request.ApplicationId), cancellationToken);

            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && a.IsActive && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            if (!string.Equals(application.ApplicationStatus, StatusSubmitted, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(application.ApplicationStatus, CredentialingWorklistQueryService.StatusInReview, StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialingConflictException(
                    $"Panel Mitra Bestari hanya dapat ditetapkan untuk permohonan berstatus Submitted atau InReview. Status saat ini: {application.ApplicationStatus}.");
            }

            // ATURAN ANTI-SELF-REVIEW (VR-CRD-15): Pemohon dilarang menilai dirinya sendiri
            if (request.ReviewerWorkforceProfileIds.Contains(application.WorkforceProfileId))
            {
                throw new InvalidOperationException(
                    "Pelanggaran integritas audit klinis (VR-CRD-15): Pemohon kredensial dilarang keras ditunjuk menjadi anggota panel Mitra Bestari (Peer Reviewer) untuk menilai portofolio klinis dirinya sendiri.");
            }

            // Validasi jumlah reviewer minimal 2 orang unik (HRD-DEC-147)
            var distinctReviewerIds = request.ReviewerWorkforceProfileIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (distinctReviewerIds.Count < 2)
            {
                throw new InvalidOperationException(
                    "Panel Mitra Bestari wajib beranggotakan minimal 2 (dua) orang dokter/nakes sejawat seprofesi yang berbeda (HRD-DEC-147).");
            }

            var reviewers = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(p => distinctReviewerIds.Contains(p.Id) && p.IsActive && !p.IsDelete)
                .Select(p => new { p.Id, p.DisplayName, p.ProfileCode })
                .ToListAsync(cancellationToken);
            if (reviewers.Count != distinctReviewerIds.Count)
            {
                throw new InvalidOperationException("Sebagian reviewer tidak ditemukan atau profilnya tidak aktif.");
            }

            // BE-HRD-430: reviewer harus seprofesi dengan permohonan (bila profesi permohonan diketahui).
            var applicationProfessionId = application.ProfessionId ?? await ResolveProfessionIdAsync(application.WorkforceProfileId, cancellationToken);
            if (applicationProfessionId.HasValue)
            {
                var mismatched = new List<string>();
                foreach (var reviewer in reviewers)
                {
                    var reviewerProfessionId = await ResolveProfessionIdAsync(reviewer.Id, cancellationToken);
                    if (reviewerProfessionId != applicationProfessionId)
                    {
                        mismatched.Add(reviewer.DisplayName);
                    }
                }

                if (mismatched.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Reviewer berikut tidak seprofesi dengan pemohon sehingga tidak dapat menjadi Mitra Bestari: " + string.Join(", ", mismatched) + ".");
                }
            }

            // Penilaian hanya bisa dikirim dari akun reviewer sendiri (MSL-16), jadi setiap reviewer wajib punya akun aktif.
            var accounts = await _context.Users
                .AsNoTracking()
                .Where(u => u.WorkforceProfileId.HasValue && distinctReviewerIds.Contains(u.WorkforceProfileId.Value) && u.IsActive)
                .Select(u => new { u.Id, ProfileId = u.WorkforceProfileId!.Value })
                .ToListAsync(cancellationToken);
            var accountByProfile = accounts.GroupBy(a => a.ProfileId).ToDictionary(g => g.Key, g => g.First().Id);
            var withoutAccount = reviewers.Where(r => !accountByProfile.ContainsKey(r.Id)).Select(r => r.DisplayName).ToList();
            if (withoutAccount.Count > 0)
            {
                throw new InvalidOperationException(
                    "Reviewer berikut belum memiliki akun aktif sehingga tidak dapat mengirim penilaian: " + string.Join(", ", withoutAccount) + ".");
            }

            var now = DateTime.UtcNow;
            var existingRows = await ActivePanelRows(application.Id).ToListAsync(cancellationToken);
            var existingByProfile = existingRows
                .Select(row => (Row: row, Evidence: CredentialingWorklistQueryService.ReadPeerReviewEvidence(row.ReviewEvidenceJson)))
                .Where(x => x.Evidence.ReviewerProfileId.HasValue)
                .GroupBy(x => x.Evidence.ReviewerProfileId!.Value)
                .ToDictionary(g => g.Key, g => g.First().Row);

            // Keluarkan anggota lama yang tidak lagi ditunjuk. Yang sudah menilai tidak boleh dikeluarkan: hasilnya sumber pleno.
            foreach (var row in existingByProfile.Where(x => !distinctReviewerIds.Contains(x.Key)).Select(x => x.Value))
            {
                if (string.Equals(row.Recommendation, CredentialingWorklistQueryService.ReviewCompleted, StringComparison.OrdinalIgnoreCase))
                {
                    throw new CredentialingConflictException("Reviewer yang sudah mengirim penilaian tidak dapat dikeluarkan dari panel.");
                }

                row.Recommendation = CredentialingWorklistQueryService.ReviewWithdrawn;
                row.UpdateBy = assignedByUserId;
                row.UpdateDateTime = now;
            }

            foreach (var reviewer in reviewers.Where(r => !existingByProfile.ContainsKey(r.Id)))
            {
                var evidence = new CredentialingWorklistQueryService.PeerReviewEvidence
                {
                    ReviewerProfileId = reviewer.Id,
                    ReviewerName = reviewer.DisplayName,
                    ReviewerCode = reviewer.ProfileCode,
                    AssignedByUserId = assignedByUserId,
                    AssignedAt = now,
                    AssignmentNotes = request.AssignmentNotes
                };

                _context.TrxCredentialingCommitteeReviews.Add(new TrxCredentialingCommitteeReview
                {
                    Id = Guid.NewGuid(),
                    CredentialingApplicationId = application.Id,
                    ReviewNumber = await AllocateReviewNumberAsync(assignedByUserId, cancellationToken),
                    MeetingDate = now,
                    ReviewerUserId = accountByProfile[reviewer.Id],
                    CommitteeMemberRole = CredentialingWorklistQueryService.PeerReviewerRole,
                    Recommendation = CredentialingWorklistQueryService.ReviewPending,
                    ReviewEvidenceJson = JsonSerializer.Serialize(evidence),
                    IsActive = true,
                    CreateBy = assignedByUserId,
                    CreateDateTime = now
                });
            }

            application.ApplicationStatus = CredentialingWorklistQueryService.StatusInReview;
            application.UpdateBy = assignedByUserId;
            application.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Panel Mitra Bestari sebanyak {Count} orang berhasil ditugaskan untuk permohonan #{AppNumber}.",
                reviewers.Count, application.ApplicationNumber);

            return MapToDto(application, application.WorkforceProfile, null, reviewers.Select(r => r.DisplayName).ToList());
        }

        public async Task<bool> SubmitPeerReviewAssessmentAsync(
            SubmitPeerReviewAssessmentDto request,
            Guid assessorUserId,
            CancellationToken cancellationToken = default,
            Guid? assessorWorkforceProfileId = null)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await AcquireTransactionLockAsync(PanelLockKey(request.ApplicationId), cancellationToken);

            var application = await _context.TrxCredentialingApplications
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && a.IsActive && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            // ATURAN ANTI-SELF-REVIEW (VR-CRD-15)
            if (request.ReviewerWorkforceProfileId == application.WorkforceProfileId
                || (assessorWorkforceProfileId.HasValue && assessorWorkforceProfileId.Value == application.WorkforceProfileId))
            {
                throw new UnauthorizedAccessException(
                    "Pelanggaran integritas audit klinis (VR-CRD-15): Pemohon dilarang melakukan asesmen terhadap dirinya sendiri.");
            }

            if (!string.Equals(application.ApplicationStatus, CredentialingWorklistQueryService.StatusInReview, StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialingConflictException("Penilaian Mitra Bestari hanya dapat dikirim saat permohonan sedang ditelaah panel (status InReview).");
            }

            if (assessorWorkforceProfileId.HasValue && assessorWorkforceProfileId.Value != request.ReviewerWorkforceProfileId)
            {
                throw new UnauthorizedAccessException("Penilaian Mitra Bestari hanya boleh dikirim atas nama akun Anda sendiri.");
            }

            // BE-HRD-431: keanggotaan panel dibaca dari baris penugasan yang persisten, dicocokkan dengan akun pengirim.
            var row = await ActivePanelRows(application.Id)
                .FirstOrDefaultAsync(r => r.ReviewerUserId == assessorUserId, cancellationToken);
            var evidence = row == null ? null : CredentialingWorklistQueryService.ReadPeerReviewEvidence(row.ReviewEvidenceJson);
            if (row == null || evidence == null || evidence.ReviewerProfileId != request.ReviewerWorkforceProfileId)
            {
                throw new UnauthorizedAccessException("Anda bukan anggota panel Mitra Bestari yang ditugaskan untuk permohonan ini.");
            }

            if (string.Equals(row.Recommendation, CredentialingWorklistQueryService.ReviewCompleted, StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialingConflictException("Anda sudah mengirim penilaian untuk permohonan ini.");
            }

            EnsureItemsHaveSource(request.AssessmentItems, application.RequirementSnapshotJson, "Butir asesmen");

            var now = DateTime.UtcNow;
            evidence.SubmittedAt = now;
            evidence.OverallNotes = request.OverallRecommendation;
            evidence.RequiresSupervision = request.AssessmentItems.Any(i =>
                i.RecommendedSupervisionLevel == SupervisionLevel.Level2_UnderSupervision ||
                i.RecommendedSupervisionLevel == SupervisionLevel.Level3_Proctored);
            evidence.Items = request.AssessmentItems;

            row.Recommendation = CredentialingWorklistQueryService.ReviewCompleted;
            row.Comments = Truncate(request.OverallRecommendation ?? "Rekomendasi kewenangan klinis sesuai hasil telaah portofolio.", 2000);
            row.ReviewEvidenceJson = JsonSerializer.Serialize(evidence);
            row.UpdateBy = assessorUserId;
            row.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await AcquireTransactionLockAsync(PanelLockKey(request.ApplicationId), cancellationToken);

            var application = await _context.TrxCredentialingApplications
                .Include(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && a.IsActive && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                throw new KeyNotFoundException($"Permohonan kredensial dengan ID {request.ApplicationId} tidak ditemukan.");
            }

            // MSL-16 (19 Nov 2026): pleno hanya untuk permohonan yang sedang ditelaah panel, agar BAP tidak tertimpa dua kali.
            if (!string.Equals(application.ApplicationStatus, CredentialingWorklistQueryService.StatusInReview, StringComparison.OrdinalIgnoreCase))
            {
                throw new CredentialingConflictException(
                    $"Sidang pleno hanya dapat disahkan untuk permohonan yang sedang ditelaah panel Mitra Bestari. Status saat ini: {application.ApplicationStatus}.");
            }

            var chairpersonIsApplicant = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == chairpersonUserId && u.WorkforceProfileId == application.WorkforceProfileId, cancellationToken);
            if (chairpersonIsApplicant)
            {
                throw new UnauthorizedAccessException("Pemohon kredensial tidak dapat mengesahkan sidang pleno atas permohonannya sendiri (VR-CRD-15).");
            }

            if (!AllowedPlenaryDecisions.Contains(request.PlenaryDecision, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Keputusan pleno harus Recommended, ConditionallyRecommended, atau Rejected.");
            }

            // Validasi penilaian Mitra Bestari minimal 2 orang berbeda telah masuk (dibaca dari baris panel persisten).
            var panelRows = await ActivePanelRows(application.Id).AsNoTracking().ToListAsync(cancellationToken);
            var completedReviewers = panelRows
                .Where(r => string.Equals(r.Recommendation, CredentialingWorklistQueryService.ReviewCompleted, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.ReviewerUserId)
                .Distinct()
                .Count();
            if (completedReviewers < 2)
            {
                throw new InvalidOperationException(
                    $"Sidang pleno belum dapat disahkan: minimal 2 penilaian dari panel Mitra Bestari wajib selesai (saat ini baru {completedReviewers} penilaian masuk).");
            }

            EnsureItemsHaveSource(request.FinalApprovedPrivileges, application.RequirementSnapshotJson, "Kewenangan hasil pleno");

            var independentCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level1_Independent);
            var supervisedCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level2_UnderSupervision || p.RecommendedSupervisionLevel == SupervisionLevel.Level3_Proctored);
            var deniedCount = request.FinalApprovedPrivileges.Count(p => p.RecommendedSupervisionLevel == SupervisionLevel.Level4_NotPermitted);

            var panelSnapshot = panelRows
                .Select(r => CredentialingWorklistQueryService.ReadPeerReviewEvidence(r.ReviewEvidenceJson))
                .Where(e => e.ReviewerProfileId.HasValue)
                .Select(e => new { Id = e.ReviewerProfileId!.Value, DisplayName = e.ReviewerName, ProfileCode = e.ReviewerCode })
                .ToList();

            var now = DateTime.UtcNow;
            var plenaryRecord = new
            {
                // Salinan informatif; sumber resmi panel tetap baris TrxCredentialingCommitteeReview.
                ReviewerProfiles = panelSnapshot,
                PlenaryBapNumber = request.PlenaryBapNumber,
                PlenaryDate = request.PlenaryDate,
                Decision = request.PlenaryDecision,
                ChairpersonNotes = request.CommitteeChairpersonNotes,
                EvaluatedCount = request.FinalApprovedPrivileges.Count,
                IndependentCount = independentCount,
                SupervisedCount = supervisedCount,
                DeniedCount = deniedCount,
                FinalApprovedPrivileges = request.FinalApprovedPrivileges,
                ConcludedAt = now,
                ConcludedByUserId = chairpersonUserId
            };

            application.ComplianceResultJson = JsonSerializer.Serialize(plenaryRecord);
            application.ApplicationStatus = CredentialingWorklistQueryService.StatusPlenaryConcluded;
            application.ClosedAt = now;
            application.ClosedByUserId = chairpersonUserId;
            application.CompletedDate = now;
            application.IsPrivilegeCompliant = request.PlenaryDecision.Equals("Recommended", StringComparison.OrdinalIgnoreCase);
            application.UpdateBy = chairpersonUserId;
            application.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

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

            var reviewerNames = (await ActivePanelRows(applicationId).AsNoTracking().Select(r => r.ReviewEvidenceJson).ToListAsync(cancellationToken))
                .Select(CredentialingWorklistQueryService.ReadPeerReviewEvidence)
                .Select(e => e.ReviewerName ?? "-")
                .ToList();

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

        private static string PanelLockKey(Guid applicationId) => $"HRD_CRED_APPLICATION_PANEL_{applicationId:N}";

        /// <summary>Baris panel Mitra Bestari yang masih berlaku (belum dikeluarkan) untuk satu permohonan.</summary>
        private IQueryable<TrxCredentialingCommitteeReview> ActivePanelRows(Guid applicationId) =>
            _context.TrxCredentialingCommitteeReviews
                .Where(r => r.CredentialingApplicationId == applicationId
                            && r.CommitteeMemberRole == CredentialingWorklistQueryService.PeerReviewerRole
                            && r.Recommendation != CredentialingWorklistQueryService.ReviewWithdrawn
                            && r.IsActive && !r.IsDelete);

        /// <summary>
        /// Setiap butir keputusan/asesmen harus menunjuk katalog kewenangan yang diajukan pemohon (bila daftar
        /// pengajuan tersimpan) dan tidak boleh ganda.
        /// </summary>
        private static void EnsureItemsHaveSource(IReadOnlyCollection<PrivilegeAssessmentItemDto> items, string? requirementSnapshotJson, string label)
        {
            if (items.GroupBy(i => i.CatalogId).Any(g => g.Count() > 1))
            {
                throw new InvalidOperationException($"{label} tidak boleh memuat katalog kewenangan yang sama lebih dari sekali.");
            }

            var requested = CredentialingWorklistQueryService.ReadRequestedCatalogIds(requirementSnapshotJson);
            if (requested.Count == 0)
            {
                return;
            }

            var outside = items.Where(i => !requested.Contains(i.CatalogId)).ToList();
            if (outside.Count > 0)
            {
                throw new InvalidOperationException($"{label} memuat kewenangan yang tidak diajukan pemohon.");
            }
        }

        private async Task<Guid?> ResolveProfessionIdAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var doctorProfession = await _context.MstDoctors
                .AsNoTracking()
                .Where(d => d.WorkforceProfileId == workforceProfileId && !d.IsDelete)
                .Select(d => (Guid?)d.ProfessionId)
                .FirstOrDefaultAsync(cancellationToken);
            if (doctorProfession.HasValue && doctorProfession.Value != Guid.Empty)
            {
                return doctorProfession;
            }

            var employeeProfession = await _context.MstEmployees
                .AsNoTracking()
                .Where(e => e.WorkforceProfileId == workforceProfileId && !e.IsDelete && e.ProfessionId != null)
                .Select(e => e.ProfessionId)
                .FirstOrDefaultAsync(cancellationToken);
            if (employeeProfession.HasValue)
            {
                return employeeProfession;
            }

            return await _context.MstExternalUsers
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.ProfessionId != null)
                .Select(x => x.ProfessionId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Syarat wajib profesi bertipe lisensi/sertifikat yang belum didukung bukti terverifikasi dan masih berlaku.
        /// Syarat lain (kompetensi, pelatihan, jabatan, katalog kewenangan) dinilai pada matriks persyaratan jabatan dan pleno.
        /// </summary>
        private async Task<List<string>> FindMissingMandatoryRequirementsAsync(
            Guid workforceProfileId,
            Guid? professionId,
            Guid? specializationId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var missing = new List<string>();
            if (!professionId.HasValue)
            {
                return missing;
            }

            var pid = professionId.Value;
            var requirements = await _context.Set<MstCredentialingRequirement>()
                .AsNoTracking()
                .Where(r => r.IsActive && !r.IsDelete && r.IsMandatory
                            && r.ProfessionId == pid
                            && r.PositionId == null
                            && (r.SpecializationId == null || r.SpecializationId == specializationId)
                            && (r.LicenseTypeId != null || r.CertificationTypeId != null)
                            && (r.EffectiveStartDate == null || r.EffectiveStartDate <= now)
                            && (r.EffectiveEndDate == null || r.EffectiveEndDate >= now))
                .Select(r => new { r.RequirementName, r.LicenseTypeId, r.CertificationTypeId, r.RequiresVerification })
                .ToListAsync(cancellationToken);

            foreach (var requirement in requirements)
            {
                bool satisfied;
                if (requirement.LicenseTypeId.HasValue)
                {
                    var licenseTypeId = requirement.LicenseTypeId.Value;
                    satisfied = await _context.WfpCredentialLicenses
                        .AsNoTracking()
                        .AnyAsync(l => l.WorkforceProfileId == workforceProfileId
                                       && l.LicenseTypeId == licenseTypeId
                                       && l.IsActive && !l.IsRevoked && !l.IsDelete
                                       && l.ExpiredDate >= now
                                       && (!requirement.RequiresVerification || l.IsPsvVerified || l.IsVerified), cancellationToken);
                }
                else
                {
                    var certificationTypeId = requirement.CertificationTypeId!.Value;
                    satisfied = await _context.Set<WfpCertification>()
                        .AsNoTracking()
                        .AnyAsync(c => c.WorkforceProfileId == workforceProfileId
                                       && c.CertificationTypeId == certificationTypeId
                                       && c.IsActive && !c.IsDelete
                                       && (c.IsLifetime || c.ExpiredDate == null || c.ExpiredDate >= now)
                                       && (!requirement.RequiresVerification || c.IsVerified), cancellationToken);
                }

                if (!satisfied)
                {
                    missing.Add(requirement.RequirementName);
                }
            }

            return missing;
        }

        private async Task<string> AllocateApplicationNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            // Format lama APP-CRED-yyyyMM-NNNN dipertahankan; batas bawah = nomor tertinggi bulan berjalan yang sudah
            // terbit lewat Count+1 sebelum perubahan ini, agar tidak bentrok.
            var monthCode = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ResolveBusinessTimeZone()).ToString("yyyyMM", CultureInfo.InvariantCulture);
            var legacyPrefix = $"{ApplicationSeriesPrefix}-{monthCode}-";
            var existing = await _context.TrxCredentialingApplications
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(a => a.ApplicationNumber.StartsWith(legacyPrefix))
                .Select(a => a.ApplicationNumber)
                .ToListAsync(cancellationToken);
            var watermark = existing
                .Select(n => long.TryParse(n[legacyPrefix.Length..], out var parsed) ? parsed : 0L)
                .DefaultIfEmpty(0L)
                .Max();

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ApplicationSequenceKey,
                    Prefix: ApplicationSeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Monthly,
                    SequenceDigits: ApplicationSequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: watermark),
                cancellationToken);
        }

        private Task<string> AllocateReviewNumberAsync(Guid actorUserId, CancellationToken cancellationToken) =>
            _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ReviewSequenceKey,
                    Prefix: ReviewSeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: ReviewSequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);

        private async Task AcquireTransactionLockAsync(string lockKey, CancellationToken cancellationToken)
        {
            if (!_context.Database.IsNpgsql())
            {
                return;
            }

            await _context.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            foreach (var id in new[] { "Asia/Jakarta", "SE Asia Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];

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
