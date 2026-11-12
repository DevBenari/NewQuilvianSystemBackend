using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Seeder data runtime development untuk modul Orientasi &amp; Onboarding Staf (Slice S-C4-ONB)
    /// Memenuhi task BE-HRD-085 dan otorisasi HRD-DEC-032.
    /// </summary>
    public static class OnboardingRuntimeDevSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            var now = DateTime.UtcNow;

            // 1. Otorisasi peran (HRD-DEC-032 / MAPPING_REQUIRED)
            await EnsureRoleAsync(roleManager, "HRAdmin", "Administrator HRD pemroses berkas onboarding, kontrak dan e-dossier");
            await EnsureRoleAsync(roleManager, "UnitHead", "Kepala Ruangan / Kepala Unit penilai masa percobaan T15 dan pembimbing klinis");

            // 2. Master Jenis Kepegawaian (MstEmploymentType)
            var empTypes = new List<(string Code, string Name, bool IsPermanent, bool IsContract)>
            {
                ("TETAP", "Pegawai Tetap (Permanent)", true, false),
                ("KONTRAK", "Pegawai Kontrak (PKWT)", false, true),
                ("MITRA", "Mitra Medis / Dokter Tamu", false, false)
            };

            var createdEmpTypes = new Dictionary<string, Guid>();
            foreach (var et in empTypes)
            {
                var existing = await dbContext.MstEmploymentTypes.FirstOrDefaultAsync(t => t.EmploymentTypeCode == et.Code);
                if (existing == null)
                {
                    var newType = new MstEmploymentType
                    {
                        Id = Guid.NewGuid(),
                        EmploymentTypeCode = et.Code,
                        EmploymentTypeName = et.Name,
                        IsPermanent = et.IsPermanent,
                        IsContractBased = et.IsContract,
                        IsActive = true,
                        CreateDateTime = now
                    };
                    dbContext.MstEmploymentTypes.Add(newType);
                    createdEmpTypes[et.Code] = newType.Id;
                }
                else
                {
                    createdEmpTypes[et.Code] = existing.Id;
                }
            }

            // 3. Master Jenis Kontrak (MstContractType)
            var contractTypes = new List<(string Code, string Name, int? Months, bool Renewable, bool RequiresEnd, bool Probation, string EmpTypeCode)>
            {
                ("PKWT", "Perjanjian Kerja Waktu Tertentu (Kontrak)", 12, true, true, false, "KONTRAK"),
                ("PKWTT", "Perjanjian Kerja Waktu Tidak Tertentu (Tetap)", null, false, false, true, "TETAP")
            };

            foreach (var ct in contractTypes)
            {
                var existing = await dbContext.MstContractTypes.FirstOrDefaultAsync(t => t.ContractTypeCode == ct.Code);
                if (existing == null)
                {
                    createdEmpTypes.TryGetValue(ct.EmpTypeCode, out var empTypeId);
                    dbContext.MstContractTypes.Add(new MstContractType
                    {
                        Id = Guid.NewGuid(),
                        ContractTypeCode = ct.Code,
                        ContractTypeName = ct.Name,
                        DefaultDurationMonths = ct.Months,
                        IsRenewable = ct.Renewable,
                        RequiresEndDate = ct.RequiresEnd,
                        IsProbationApplicable = ct.Probation,
                        EmploymentTypeId = empTypeId != Guid.Empty ? empTypeId : null,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            // 4. Master Status Kepegawaian (MstEmploymentStatus)
            var statuses = new List<(string Code, string Name, bool IsActiveEmp, bool IsSchedulable, bool IsPayroll)>
            {
                ("PROBATION", "Masa Percobaan (Probation)", true, true, true),
                ("ACTIVE", "Aktif Bekerja (Active)", true, true, true),
                ("RESIGNED", "Mengundurkan Diri (Resigned)", false, false, false),
                ("TERMINATED", "Pemberhentian Hubungan Kerja (Terminated)", false, false, false)
            };

            foreach (var st in statuses)
            {
                var existing = await dbContext.MstEmploymentStatuses.FirstOrDefaultAsync(s => s.EmploymentStatusCode == st.Code);
                if (existing == null)
                {
                    dbContext.MstEmploymentStatuses.Add(new MstEmploymentStatus
                    {
                        Id = Guid.NewGuid(),
                        EmploymentStatusCode = st.Code,
                        EmploymentStatusName = st.Name,
                        IsActiveEmployment = st.IsActiveEmp,
                        IsSchedulable = st.IsSchedulable,
                        IsPayrollEligible = st.IsPayroll,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            // 5. Template Onboarding Tiga Lapis (Umum & Klinis)
            var templates = new List<(string Code, string Name, OnboardingTemplateLayer Layer, int Days)>
            {
                ("TPL-ONB-GEN", "Template Onboarding Umum Rumah Sakit", OnboardingTemplateLayer.General, 30),
                ("TPL-ONB-CLN", "Template Onboarding Klinis Nakes & Keperawatan", OnboardingTemplateLayer.Clinical, 60)
            };

            foreach (var tpl in templates)
            {
                var existing = await dbContext.MstOnboardingTemplates.FirstOrDefaultAsync(t => t.TemplateCode == tpl.Code);
                if (existing == null)
                {
                    dbContext.MstOnboardingTemplates.Add(new MstOnboardingTemplate
                    {
                        Id = Guid.NewGuid(),
                        TemplateCode = tpl.Code,
                        TemplateName = tpl.Name,
                        TemplateLayer = tpl.Layer,
                        TargetCompletionDays = tpl.Days,
                        Version = 1,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            // 6. Definisi Workflow Onboarding T15 s.d. T18
            var workflows = new List<(string Code, string Name, string ReqType)>
            {
                ("T15", "Penilaian Masa Percobaan Staf (Probation Evaluation)", "ProbationAssessment"),
                ("T16", "Keputusan Perpanjangan / Pengangkatan Kontrak Kerja", "ContractDecision"),
                ("T17", "Koreksi Kontrak Kerja Administratif", "ContractCorrection"),
                ("T18", "Batch Inisialisasi Data Awal Kontrak", "InitialContractBatch")
            };

            foreach (var wf in workflows)
            {
                var existingWf = await dbContext.MstWorkflowDefinitions.FirstOrDefaultAsync(w => w.WorkflowCode == wf.Code);
                if (existingWf == null)
                {
                    dbContext.MstWorkflowDefinitions.Add(new MstWorkflowDefinition
                    {
                        Id = Guid.NewGuid(),
                        WorkflowCode = wf.Code,
                        WorkflowName = wf.Name,
                        RequestType = wf.ReqType,
                        WorkflowCategory = "OnboardingLifecycle",
                        Version = 1,
                        WorkflowStatus = "Active",
                        EffectiveStartDate = now,
                        AllowRequesterCancel = true,
                        IsActive = true,
                        CreateDateTime = now
                    });
                }
            }

            // 7. Pengaturan Siklus Hidup Kepegawaian (HrdEmploymentLifecycleSetting)
            var hospitalSite = await dbContext.MstHospitalSites.FirstOrDefaultAsync(s => s.IsActive);
            if (hospitalSite != null)
            {
                var existingSetting = await dbContext.HrdEmploymentLifecycleSettings
                    .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSite.Id);

                if (existingSetting == null)
                {
                    dbContext.HrdEmploymentLifecycleSettings.Add(new HrdEmploymentLifecycleSetting
                    {
                        Id = Guid.NewGuid(),
                        HospitalSiteId = hospitalSite.Id,
                        ContractReminderDays = 60,
                        ContractEscalationDays = 14,
                        ProbationReminderDays = 14,
                        LateStartFlagDays = 3,
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
