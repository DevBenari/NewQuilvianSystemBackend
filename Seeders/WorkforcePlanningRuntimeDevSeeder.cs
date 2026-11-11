using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Seeder data runtime development untuk modul Perencanaan Tenaga Kerja (Workforce Planning / Slice S-D1)
    /// Memenuhi task BE-HRD-050 dan otorisasi HRD-DEC-032.
    /// </summary>
    public static class WorkforcePlanningRuntimeDevSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var now = DateTime.UtcNow;

            // 1. Otorisasi peran (HRD-DEC-032 / MAPPING_REQUIRED)
            await EnsureRoleAsync(roleManager, "HospitalDirector", "Direktur Utama / Direksi penandatangan formasi dan persetujuan T10/T12");
            await EnsureRoleAsync(roleManager, "HRManager", "Manager HRD penyetuju rencana kebutuhan tenaga dan headcount T10/T11");
            await EnsureRoleAsync(roleManager, "FinanceManager", "Manager Keuangan penelaah estimasi anggaran formasi dan payroll");
            await EnsureRoleAsync(roleManager, "Recruiter", "Staf Rekrutmen / Talent Acquisition pemroses kandidat dan kursi formasi");

            // 2. Definisi alur persetujuan workflow T9 s.d. T13
            var workflows = new List<(string Code, string Name, string ReqType)>
            {
                ("T9", "Penyusunan & Pengesahan Standar Kebutuhan Tenaga (ABK-Kes)", "StaffingStandard"),
                ("T10", "Pengesahan Rencana Kebutuhan Tenaga Tahunan & Batch Formasi Awal", "ManpowerPlan"),
                ("T11", "Permintaan Tenaga Kerja Standar (Within Formation)", "HeadcountRequestWithinFormation"),
                ("T12", "Permintaan Tenaga Kerja Tambahan (Beyond Formation)", "HeadcountRequestBeyondFormation"),
                ("T13", "Pembatalan Kursi Formasi Terencana", "HeadcountCancellation")
            };

            foreach (var wf in workflows)
            {
                var existingWf = await dbContext.MstWorkflowDefinitions
                    .FirstOrDefaultAsync(w => w.WorkflowCode == wf.Code);

                if (existingWf == null)
                {
                    dbContext.MstWorkflowDefinitions.Add(new MstWorkflowDefinition
                    {
                        Id = Guid.NewGuid(),
                        WorkflowCode = wf.Code,
                        WorkflowName = wf.Name,
                        RequestType = wf.ReqType,
                        WorkflowCategory = "WorkforcePlanning",
                        Version = 1,
                        WorkflowStatus = "Active",
                        EffectiveStartDate = now,
                        AllowRequesterCancel = true,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            // 3. Pengaturan Perencanaan Rumah Sakit (HrdWorkforcePlanningSetting)
            var hospitalSite = await dbContext.MstHospitalSites.FirstOrDefaultAsync(s => s.IsActive);
            if (hospitalSite != null)
            {
                var existingSetting = await dbContext.HrdWorkforcePlanningSettings
                    .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSite.Id);

                if (existingSetting == null)
                {
                    dbContext.HrdWorkforcePlanningSettings.Add(new HrdWorkforcePlanningSetting
                    {
                        Id = Guid.NewGuid(),
                        HospitalSiteId = hospitalSite.Id,
                        LongLeaveThresholdDays = 30,
                        OverdueRequestThresholdDays = 14,
                        ChangeReason = "Inisialisasi Data Runtime Dev Default Perencanaan Tenaga Kerja (BE-HRD-050)",
                        CreateDateTime = now
                    });
                }
            }

            // 4. Master Grade Gaji Tengah (Midpoint Salary Grade) untuk Estimasi Biaya
            if (!await dbContext.MstSalaryGrades.AnyAsync())
            {
                var grades = new List<(string Code, string Name, decimal Min, decimal Mid, decimal Max)>
                {
                    ("GRD-01", "Grade Medis Spesialis Utama", 20000000m, 25000000m, 30000000m),
                    ("GRD-02", "Grade Dokter Umum / Nakes Senior", 10000000m, 12500000m, 15000000m),
                    ("GRD-03", "Grade Perawat / Nakes Madya", 6000000m, 7500000m, 9000000m),
                    ("GRD-04", "Grade Staf Penunjang / Administrasi", 4500000m, 5200000m, 6000000m)
                };

                foreach (var g in grades)
                {
                    dbContext.MstSalaryGrades.Add(new MstSalaryGrade
                    {
                        Id = Guid.NewGuid(),
                        SalaryGradeCode = g.Code,
                        SalaryGradeName = g.Name,
                        CurrencyCode = "IDR",
                        MinimumSalary = g.Min,
                        MidpointSalary = g.Mid,
                        MaximumSalary = g.Max,
                        EffectiveStartDate = now,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName, string description)
        {
            if (await roleManager.RoleExistsAsync(roleName)) return;

            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                IsSystemRole = false,
                CreateDateTime = DateTime.UtcNow
            };

            await roleManager.CreateAsync(role);
        }
    }
}
