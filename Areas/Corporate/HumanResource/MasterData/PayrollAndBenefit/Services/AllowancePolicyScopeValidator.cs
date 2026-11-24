using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Services
{
    /// <summary>
    /// Validasi cakupan Grade Pegawai, Grade Gaji, dan Struktur Gaji untuk kebijakan tunjangan Shift, Siaga (On Call), dan Risiko
    /// (PAY-BE-FIX-018). Aturan Grade Gaji/Struktur Gaji/masa berlaku memakai satu-satunya definisi yang sama dengan kebijakan transport
    /// (<see cref="TransportAllowancePolicyScopeRules"/>), supaya empat jenis kebijakan tidak memiliki aturan yang berbeda diam-diam.
    /// Ketiga kolom boleh kosong: kosong berarti kebijakan tidak dibatasi pada dimensi itu (perilaku sebelum cakupan ini ada).
    /// Hanya memvalidasi; tidak menulis data.
    /// </summary>
    public static class AllowancePolicyScopeValidator
    {
        /// <returns>Pesan penolakan bahasa Indonesia, atau null bila cakupan sah.</returns>
        public static async Task<string?> ValidateAsync(
            ApplicationDbContext dbContext,
            Guid? employeeGradeId,
            Guid? salaryGradeId,
            Guid? salaryStructureId,
            Guid? policyLegalEntityId,
            Guid? policyHospitalSiteId,
            DateTime? policyStartDate,
            DateTime? policyEndDate,
            CancellationToken cancellationToken = default)
        {
            if (employeeGradeId.HasValue &&
                !await dbContext.Set<MstEmployeeGrade>().AnyAsync(
                    x => x.Id == employeeGradeId.Value && x.IsActive && !x.IsDelete, cancellationToken))
            {
                return "Grade Pegawai tidak ditemukan atau tidak aktif.";
            }

            if (!salaryGradeId.HasValue && !salaryStructureId.HasValue) return null;

            TransportPolicyGradeFacts? grade = null;
            if (salaryGradeId.HasValue)
            {
                var gradeId = salaryGradeId.Value;
                grade = await dbContext.Set<MstSalaryGrade>()
                    .AsNoTracking()
                    .Where(g => g.Id == gradeId && !g.IsDelete)
                    .Select(g => new TransportPolicyGradeFacts(g.Id, g.IsActive, g.EffectiveStartDate, g.EffectiveEndDate))
                    .FirstOrDefaultAsync(cancellationToken);
            }

            TransportPolicyStructureFacts? structure = null;
            if (salaryStructureId.HasValue)
            {
                var structureId = salaryStructureId.Value;
                structure = await dbContext.Set<MstSalaryStructure>()
                    .AsNoTracking()
                    .Where(s => s.Id == structureId && !s.IsDelete)
                    .Select(s => new TransportPolicyStructureFacts(
                        s.Id, s.SalaryGradeId, s.IsActive, s.EffectiveStartDate, s.EffectiveEndDate, s.LegalEntityId, s.HospitalSiteId))
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return TransportAllowancePolicyScopeRules.ValidateScope(
                salaryGradeId,
                salaryStructureId,
                grade,
                structure,
                policyLegalEntityId,
                policyHospitalSiteId,
                policyStartDate,
                policyEndDate);
        }
    }
}
