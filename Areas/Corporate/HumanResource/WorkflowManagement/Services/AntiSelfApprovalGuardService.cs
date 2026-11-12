using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IAntiSelfApprovalGuardService
    {
        void ValidateAntiSelfApproval(Guid applicantEmployeeId, Guid approverEmployeeId);
        Task<Guid> ResolveStepApproverAsync(Guid? applicantEmployeeId, Guid departmentId, string approverRole);
        Task<Guid> ResolveSkipLevelApproverAsync(Guid applicantEmployeeId, Guid departmentId);
    }

    public class AntiSelfApprovalGuardService : IAntiSelfApprovalGuardService
    {
        private readonly ApplicationDbContext _context;

        public AntiSelfApprovalGuardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void ValidateAntiSelfApproval(Guid applicantEmployeeId, Guid approverEmployeeId)
        {
            // Aturan Konstitusi Bisnis HRD-DEC-183 & VR-WF-02:
            // Larangan mutlak seorang pejabat menyetujui permohonannya sendiri.
            if (applicantEmployeeId != Guid.Empty && approverEmployeeId != Guid.Empty && applicantEmployeeId == approverEmployeeId)
            {
                throw new InvalidOperationException(
                    "Pelanggaran Tata Kelola Persetujuan (VR-WF-02 / HRD-DEC-183): Pejabat pemohon dilarang keras menyetujui permohonan mandiri miliknya sendiri. Tiket harus dialihkan ke atasan tingkat lebih tinggi (Skip-Level).");
            }
        }

        public async Task<Guid> ResolveStepApproverAsync(Guid? applicantEmployeeId, Guid departmentId, string approverRole)
        {
            // Ambil daftar pegawai di departemen yang bersangkutan
            var employees = await _context.MstEmployees
                .Where(e => e.PrimaryDepartmentId == departmentId && e.IsActive)
                .ToListAsync();

            if (!employees.Any())
            {
                // Fallback to any active employee if department empty
                var anyEmp = await _context.MstEmployees.FirstOrDefaultAsync(e => e.IsActive);
                return anyEmp?.Id ?? Guid.NewGuid();
            }

            // Jika approverRole == "DirectSupervisor"
            if (approverRole == "DirectSupervisor")
            {
                var candidate = employees.FirstOrDefault(e => !applicantEmployeeId.HasValue || e.Id != applicantEmployeeId.Value);
                if (candidate != null) return candidate.Id;
            }

            // Jika approverRole == "HeadOfDepartment" (Kabid)
            if (approverRole == "HeadOfDepartment")
            {
                var candidate = employees.FirstOrDefault(e => !applicantEmployeeId.HasValue || e.Id != applicantEmployeeId.Value);
                if (candidate != null) return candidate.Id;
            }

            // Jika seluruh pejabat di unit sama dengan pemohon, lakukan skip-level ke direksi / HRD
            return await ResolveSkipLevelApproverAsync(applicantEmployeeId ?? Guid.Empty, departmentId);
        }

        public async Task<Guid> ResolveSkipLevelApproverAsync(Guid applicantEmployeeId, Guid departmentId)
        {
            // Cari pejabat di luar departemen pemohon (misal bagian SDM atau Direksi)
            var higherManager = await _context.MstEmployees
                .Where(e => e.Id != applicantEmployeeId && e.IsActive)
                .OrderBy(e => e.PrimaryDepartmentId == departmentId)
                .FirstOrDefaultAsync();

            return higherManager?.Id ?? Guid.NewGuid();
        }
    }
}
