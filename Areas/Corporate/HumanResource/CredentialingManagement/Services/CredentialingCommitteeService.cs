using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public class CredentialingCommitteeService : ICredentialingCommitteeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CredentialingCommitteeService> _logger;

        public CredentialingCommitteeService(
            ApplicationDbContext context,
            ILogger<CredentialingCommitteeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CredentialingCommitteeDto>> GetAllCommitteesAsync(
            CancellationToken cancellationToken = default)
        {
            var committees = await _context.MstCredentialingCommittees
                .Include(c => c.ChairpersonWorkforceProfile)
                .Include(c => c.SecretaryWorkforceProfile)
                .Where(c => c.IsActive && !c.IsDelete)
                .OrderBy(c => c.CommitteeCode)
                .ToListAsync(cancellationToken);

            return committees.Select(c => new CredentialingCommitteeDto
            {
                Id = c.Id,
                CommitteeCode = c.CommitteeCode,
                CommitteeName = c.CommitteeName,
                SubCommitteeCredentialName = c.SubCommitteeCredentialName,
                ChairpersonWorkforceProfileId = c.ChairpersonWorkforceProfileId,
                ChairpersonName = c.ChairpersonWorkforceProfile?.DisplayName,
                SecretaryWorkforceProfileId = c.SecretaryWorkforceProfileId,
                SecretaryName = c.SecretaryWorkforceProfile?.DisplayName,
                MinimumPeerReviewersCount = c.MinimumPeerReviewersCount,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<CredentialingCommitteeDto> GetCommitteeByCodeAsync(
            string committeeCode,
            CancellationToken cancellationToken = default)
        {
            var c = await _context.MstCredentialingCommittees
                .Include(x => x.ChairpersonWorkforceProfile)
                .Include(x => x.SecretaryWorkforceProfile)
                .FirstOrDefaultAsync(x => x.CommitteeCode.Equals(committeeCode, StringComparison.OrdinalIgnoreCase) && x.IsActive && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                throw new KeyNotFoundException($"Komite Kredensial dengan kode {committeeCode} tidak ditemukan.");
            }

            return new CredentialingCommitteeDto
            {
                Id = c.Id,
                CommitteeCode = c.CommitteeCode,
                CommitteeName = c.CommitteeName,
                SubCommitteeCredentialName = c.SubCommitteeCredentialName,
                ChairpersonWorkforceProfileId = c.ChairpersonWorkforceProfileId,
                ChairpersonName = c.ChairpersonWorkforceProfile?.DisplayName,
                SecretaryWorkforceProfileId = c.SecretaryWorkforceProfileId,
                SecretaryName = c.SecretaryWorkforceProfile?.DisplayName,
                MinimumPeerReviewersCount = c.MinimumPeerReviewersCount,
                Description = c.Description,
                IsActive = c.IsActive
            };
        }

        public async Task<List<ClinicalPrivilegeCatalogItemDto>> GetWhitebookCatalogAsync(
            string? committeeCode = null,
            Guid? professionId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.MstClinicalPrivilegeCatalogs
                .Where(c => c.IsActive && !c.IsDelete);

            if (professionId.HasValue)
            {
                query = query.Where(c => c.ProfessionId == professionId.Value);
            }

            var catalogs = await query
                .OrderBy(c => c.PrivilegeCode)
                .ToListAsync(cancellationToken);

            return catalogs.Select(c => new ClinicalPrivilegeCatalogItemDto
            {
                Id = c.Id,
                PrivilegeCode = c.PrivilegeCode,
                PrivilegeName = c.PrivilegeName,
                PrivilegeCategory = c.PrivilegeCategory,
                ReferenceProcedureCode = c.ReferenceProcedureCode,
                RequiresSupervision = c.RequiresSupervision,
                AllowsIndependentPractice = c.AllowsIndependentPractice,
                IsHighRisk = c.IsHighRisk,
                Description = c.Description
            }).ToList();
        }

        public async Task EnsureDefaultCommitteesExistAsync(
            Guid systemUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCodes = await _context.MstCredentialingCommittees
                .Select(c => c.CommitteeCode)
                .ToListAsync(cancellationToken);

            var defaultCommittees = new[]
            {
                new MstCredentialingCommittee
                {
                    Id = Guid.NewGuid(),
                    CommitteeCode = "MEDIK",
                    CommitteeName = "Komite Medik",
                    SubCommitteeCredentialName = "Sub-Komite Kredensial Medik",
                    MinimumPeerReviewersCount = 2,
                    Description = "Mengelola kredensial Dokter Spesialis, Dokter Subspesialis, Dokter Umum, dan Dokter Gigi.",
                    IsActive = true,
                    CreateBy = systemUserId,
                    CreateDateTime = DateTime.UtcNow
                },
                new MstCredentialingCommittee
                {
                    Id = Guid.NewGuid(),
                    CommitteeCode = "KEPERAWATAN",
                    CommitteeName = "Komite Keperawatan",
                    SubCommitteeCredentialName = "Sub-Komite Kredensial Keperawatan",
                    MinimumPeerReviewersCount = 2,
                    Description = "Mengelola kredensial Perawat Vokasi, Ners, dan Bidan sesuai jenjang klinis PK I s.d. PK V.",
                    IsActive = true,
                    CreateBy = systemUserId,
                    CreateDateTime = DateTime.UtcNow
                },
                new MstCredentialingCommittee
                {
                    Id = Guid.NewGuid(),
                    CommitteeCode = "NAKES_LAIN",
                    CommitteeName = "Komite Tenaga Kesehatan Lainnya",
                    SubCommitteeCredentialName = "Sub-Komite Kredensial Tenaga Kesehatan Lainnya",
                    MinimumPeerReviewersCount = 2,
                    Description = "Mengelola kredensial Apoteker, TTK, Radiografer, ATLM, Fisioterapis, Dietisien, dan Perekam Medis.",
                    IsActive = true,
                    CreateBy = systemUserId,
                    CreateDateTime = DateTime.UtcNow
                }
            };

            foreach (var dc in defaultCommittees)
            {
                if (!existingCodes.Contains(dc.CommitteeCode, StringComparer.OrdinalIgnoreCase))
                {
                    _context.MstCredentialingCommittees.Add(dc);
                    _logger.LogInformation("Menambahkan default Komite Kredensial: {Code} - {Name}", dc.CommitteeCode, dc.CommitteeName);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
