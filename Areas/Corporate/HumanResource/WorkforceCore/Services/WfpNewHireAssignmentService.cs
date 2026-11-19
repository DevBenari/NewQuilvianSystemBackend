using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class WfpNewHireAssignmentService : IWfpNewHireAssignmentService
    {
        private readonly ApplicationDbContext _dbContext;

        public WfpNewHireAssignmentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AssignNewHirePlacementAndSalaryAsync(
            Guid workforceProfileId,
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            Guid salaryGradeId,
            decimal basicSalary,
            DateTime startDate,
            Guid userId,
            Guid? offerApproverUserId = null,
            CancellationToken cancellationToken = default)
        {
            // Resolve actual DepartmentId from OrganizationUnit or Position master
            var orgUnit = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == organizationUnitId && !u.IsDelete, cancellationToken);
            var actualDeptId = orgUnit?.DepartmentId ?? Guid.Empty;

            var pos = await _dbContext.MstPositions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == positionId && !p.IsDelete, cancellationToken);
            if (actualDeptId == Guid.Empty && pos?.DepartmentId != null)
            {
                actualDeptId = pos.DepartmentId;
            }

            // P0-B: Hilangkan fallback FK department yang salah (tidak boleh menggunakan organizationUnitId sebagai DepartmentId)
            if (actualDeptId == Guid.Empty || !await _dbContext.MstDepartments.AnyAsync(d => d.Id == actualDeptId && !d.IsDelete, cancellationToken))
            {
                throw new InvalidOperationException("Departemen valid (MstDepartment) tidak dapat ditentukan dari Unit Organisasi atau Posisi jabatan. Konfigurasi master data belum lengkap (P0-B).");
            }

            // 1. Organization Assignment
            var existingOrg = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(o => o.WorkforceProfileId == workforceProfileId && o.IsPrimary && !o.IsDelete, cancellationToken);

            if (existingOrg == null)
            {
                var orgAssignment = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    HospitalSiteId = hospitalSiteId,
                    OrganizationUnitId = organizationUnitId,
                    DepartmentId = actualDeptId,
                    PositionId = positionId,
                    AssignmentType = "Primary",
                    IsPrimary = true,
                    EffectiveStartDate = startDate,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.Set<WfpOrganizationAssignment>().AddAsync(orgAssignment, cancellationToken);
            }

            // 2. Position Assignment
            var existingPos = await _dbContext.Set<WfpPositionAssignment>()
                .FirstOrDefaultAsync(p => p.WorkforceProfileId == workforceProfileId && p.IsPrimary && !p.IsDelete, cancellationToken);

            if (existingPos == null)
            {
                var posAssignment = new WfpPositionAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    HospitalSiteId = hospitalSiteId,
                    OrganizationUnitId = organizationUnitId,
                    DepartmentId = actualDeptId,
                    PositionId = positionId,
                    AssignmentType = "Substantive",
                    IsPrimary = true,
                    EffectiveStartDate = startDate,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.Set<WfpPositionAssignment>().AddAsync(posAssignment, cancellationToken);
            }

            // 3. Salary Assignment
            var existingSalary = await _dbContext.Set<WfpSalaryAssignment>()
                .FirstOrDefaultAsync(s => s.WorkforceProfileId == workforceProfileId && s.IsPrimary && !s.IsDelete, cancellationToken);

            if (existingSalary == null)
            {
                var salaryGrade = await _dbContext.MstSalaryGrades
                    .FirstOrDefaultAsync(g => g.Id == salaryGradeId && !g.IsDelete, cancellationToken);

                if (salaryGrade == null)
                {
                    throw new InvalidOperationException($"Master golongan gaji (MstSalaryGrade) dengan ID '{salaryGradeId}' tidak ditemukan.");
                }

                // P0-C: Struktur gaji harus cocok grade dan aktif (tidak boleh fallback ke struktur sembarang yang IsActive)
                var salaryStructure = await _dbContext.Set<MstSalaryStructure>()
                    .FirstOrDefaultAsync(s => s.SalaryGradeId == salaryGradeId && s.IsActive && !s.IsDelete, cancellationToken);

                if (salaryStructure == null)
                {
                    throw new InvalidOperationException($"Struktur penggajian aktif (MstSalaryStructure) untuk SalaryGradeId '{salaryGradeId}' tidak ditemukan. Penggajian awal tidak dapat ditetapkan (P0-C/HRD-DEC-036).");
                }

                var salaryAssignment = new WfpSalaryAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    SalaryStructureId = salaryStructure.Id,
                    SalaryGradeId = salaryGradeId,
                    BaseSalary = basicSalary,
                    CurrencyCode = salaryGrade.CurrencyCode ?? "IDR",
                    PaymentFrequency = "Monthly",
                    EffectiveStartDate = startDate,
                    IsPrimary = true,
                    IsConfidential = true,
                    IsActive = true,
                    ApprovedByUserId = offerApproverUserId ?? userId,
                    ApprovedAt = DateTime.UtcNow,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                };
                await _dbContext.Set<WfpSalaryAssignment>().AddAsync(salaryAssignment, cancellationToken);
            }

            // 4. Update Mirror on MstWorkforceProfile HANYA JIKA tanggal mulai sudah efektif (HRD-DEC-047 / P0-D)
            if (startDate.Date <= DateTime.UtcNow.Date)
            {
                var profile = await _dbContext.MstWorkforceProfiles
                    .FirstOrDefaultAsync(p => p.Id == workforceProfileId && !p.IsDelete, cancellationToken);
                if (profile != null)
                {
                    profile.PrimaryPositionId = positionId;
                    profile.PrimaryDepartmentId = actualDeptId;
                    profile.UpdateDateTime = DateTime.UtcNow;
                    profile.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
