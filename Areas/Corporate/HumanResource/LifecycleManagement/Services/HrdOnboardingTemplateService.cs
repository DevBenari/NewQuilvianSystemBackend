using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdOnboardingTemplateService : IHrdOnboardingTemplateService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdOnboardingTemplateService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<OnboardingTemplateListItemResponse>>> GetPagedAsync(
            OnboardingTemplateQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.MstOnboardingTemplates
                .AsNoTracking()
                .Include(t => t.OrganizationUnit)
                .Include(t => t.Tasks)
                .Where(t => !t.IsDelete);

            if (query.TemplateLayer.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.TemplateLayer == query.TemplateLayer.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.IsActive.HasValue)
            {
                dbQuery = dbQuery.Where(t => t.IsActive == query.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(t =>
                    t.TemplateCode.ToLower().Contains(term) ||
                    t.TemplateName.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderBy(t => t.TemplateLayer)
                .ThenBy(t => t.TemplateCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new OnboardingTemplateListItemResponse
                {
                    Id = t.Id,
                    TemplateCode = t.TemplateCode,
                    TemplateName = t.TemplateName,
                    TemplateLayer = t.TemplateLayer,
                    OrganizationUnitId = t.OrganizationUnitId,
                    OrganizationUnitName = t.OrganizationUnit != null ? t.OrganizationUnit.UnitName : null,
                    TargetCompletionDays = t.TargetCompletionDays,
                    Version = t.Version,
                    TaskCount = t.Tasks.Count(x => !x.IsDelete),
                    IsActive = t.IsActive
                })
                .ToListAsync(cancellationToken);

            var paged = new PagedResult<OnboardingTemplateListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<OnboardingTemplateListItemResponse>>.Ok(paged, "Daftar template onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingTemplateResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var template = await _dbContext.MstOnboardingTemplates
                .AsNoTracking()
                .Include(t => t.OrganizationUnit)
                .Include(t => t.Tasks.Where(x => !x.IsDelete))
                    .ThenInclude(t => t.CertificationType)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDelete, cancellationToken);

            if (template == null)
            {
                return RecruitmentResult<OnboardingTemplateResponse>.NotFound("Template onboarding tidak ditemukan.");
            }

            var response = MapToResponse(template);
            return RecruitmentResult<OnboardingTemplateResponse>.Ok(response, "Detail template onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingTemplateResponse>> CreateAsync(
            CreateOnboardingTemplateRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.TemplateCode) || string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return RecruitmentResult<OnboardingTemplateResponse>.BadRequest("TemplateCode dan TemplateName wajib diisi.");
            }

            // Validasi VR-ONB-21: Lapis Unit wajib menentukan OrganizationUnitId
            if (request.TemplateLayer == OnboardingTemplateLayer.Unit)
            {
                if (!request.OrganizationUnitId.HasValue || request.OrganizationUnitId.Value == Guid.Empty)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable("Template onboarding lapis Unit wajib menentukan unit organisasi (VR-ONB-21).");
                }
            }
            else
            {
                request.OrganizationUnitId = null;
            }

            // Validasi Unik Kode
            var codeExists = await _dbContext.MstOnboardingTemplates
                .AnyAsync(t => t.TemplateCode == request.TemplateCode && !t.IsDelete, cancellationToken);
            if (codeExists)
            {
                return RecruitmentResult<OnboardingTemplateResponse>.Conflict($"Template dengan kode '{request.TemplateCode}' sudah ada.");
            }

            // Validasi VR-ONB-20 & VR-ONB-21: Satu template aktif per lapis General/Clinical, atau per unit untuk Unit
            if (request.TemplateLayer == OnboardingTemplateLayer.General || request.TemplateLayer == OnboardingTemplateLayer.Clinical)
            {
                var activeExists = await _dbContext.MstOnboardingTemplates
                    .AnyAsync(t => t.TemplateLayer == request.TemplateLayer && t.IsActive && !t.IsDelete, cancellationToken);
                if (activeExists)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Sudah ada template aktif untuk lapis {request.TemplateLayer}. Hanya 1 template aktif yang diizinkan per lapis (VR-ONB-20).");
                }
            }
            else if (request.TemplateLayer == OnboardingTemplateLayer.Unit)
            {
                var activeUnitExists = await _dbContext.MstOnboardingTemplates
                    .AnyAsync(t => t.TemplateLayer == OnboardingTemplateLayer.Unit && t.OrganizationUnitId == request.OrganizationUnitId && t.IsActive && !t.IsDelete, cancellationToken);
                if (activeUnitExists)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        "Sudah ada template aktif untuk unit organisasi ini (VR-ONB-21).");
                }
            }

            var template = new MstOnboardingTemplate
            {
                Id = Guid.NewGuid(),
                TemplateCode = request.TemplateCode.Trim(),
                TemplateName = request.TemplateName.Trim(),
                TemplateLayer = request.TemplateLayer,
                OrganizationUnitId = request.OrganizationUnitId,
                TargetCompletionDays = request.TargetCompletionDays,
                Version = 1,
                Description = request.Description,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            // Rakit tasks & sisipkan system items jika belum ada
            var tasks = BuildTasksForCreate(request, template.Id, userId);

            // Validasi VR-ONB-23: Sertifikat wajib memilih CertificationTypeId
            foreach (var task in tasks)
            {
                if (task.AutomaticSource == OnboardingAutomaticSource.MandatoryCertification && (!task.CertificationTypeId.HasValue || task.CertificationTypeId.Value == Guid.Empty))
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Butir '{task.TaskName}' dengan sumber sertifikat wajib menentukan jenis sertifikat (VR-ONB-23).");
                }
            }

            template.Tasks = tasks;

            await _dbContext.MstOnboardingTemplates.AddAsync(template, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(template.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingTemplateResponse>> UpdateAsync(
            Guid id,
            UpdateOnboardingTemplateRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var template = await _dbContext.MstOnboardingTemplates
                .Include(t => t.Tasks.Where(x => !x.IsDelete))
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDelete, cancellationToken);

            if (template == null)
            {
                return RecruitmentResult<OnboardingTemplateResponse>.NotFound("Template onboarding tidak ditemukan.");
            }

            // Validasi VR-ONB-20 / VR-ONB-21 bila mengaktifkan kembali
            if (request.IsActive && !template.IsActive)
            {
                if (template.TemplateLayer == OnboardingTemplateLayer.General || template.TemplateLayer == OnboardingTemplateLayer.Clinical)
                {
                    var activeExists = await _dbContext.MstOnboardingTemplates
                        .AnyAsync(t => t.Id != id && t.TemplateLayer == template.TemplateLayer && t.IsActive && !t.IsDelete, cancellationToken);
                    if (activeExists)
                    {
                        return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                            $"Sudah ada template aktif lain untuk lapis {template.TemplateLayer} (VR-ONB-20).");
                    }
                }
                else if (template.TemplateLayer == OnboardingTemplateLayer.Unit)
                {
                    var activeUnitExists = await _dbContext.MstOnboardingTemplates
                        .AnyAsync(t => t.Id != id && t.TemplateLayer == OnboardingTemplateLayer.Unit && t.OrganizationUnitId == template.OrganizationUnitId && t.IsActive && !t.IsDelete, cancellationToken);
                    if (activeUnitExists)
                    {
                        return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                            "Sudah ada template aktif lain untuk unit organisasi ini (VR-ONB-21).");
                    }
                }
            }

            // Validasi VR-ONB-22: Butir sistem tidak boleh dihapus atau diubah tanda wajib / perannya
            var existingSystemTasks = template.Tasks.Where(x => !string.IsNullOrWhiteSpace(x.SystemItemCode)).ToList();
            foreach (var sysTask in existingSystemTasks)
            {
                var reqTask = request.Tasks.FirstOrDefault(t => t.SystemItemCode == sysTask.SystemItemCode || (t.Id.HasValue && t.Id.Value == sysTask.Id));
                if (reqTask == null || !reqTask.IsActive)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Butir sistem '{sysTask.TaskName}' ({sysTask.SystemItemCode}) terkunci dan tidak boleh dihapus atau dinonaktifkan (VR-ONB-22).");
                }

                if (!reqTask.IsRequired)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Butir sistem '{sysTask.TaskName}' ({sysTask.SystemItemCode}) wajib bernilai IsRequired = true (VR-ONB-22).");
                }

                if (reqTask.ResponsibleRole != sysTask.ResponsibleRole || reqTask.ItemKind != sysTask.ItemKind)
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Peran penanggung jawab atau jenis butir sistem '{sysTask.TaskName}' tidak boleh diubah (VR-ONB-22).");
                }
            }

            // Validasi VR-ONB-23: Sertifikat wajib memilih jenis
            foreach (var reqTask in request.Tasks)
            {
                if (reqTask.AutomaticSource == OnboardingAutomaticSource.MandatoryCertification && (!reqTask.CertificationTypeId.HasValue || reqTask.CertificationTypeId.Value == Guid.Empty))
                {
                    return RecruitmentResult<OnboardingTemplateResponse>.Unprocessable(
                        $"Butir '{reqTask.TaskName}' dengan sumber sertifikat wajib menentukan jenis sertifikat (VR-ONB-23).");
                }
            }

            template.TemplateName = request.TemplateName.Trim();
            template.TargetCompletionDays = request.TargetCompletionDays;
            template.Description = request.Description;
            template.IsActive = request.IsActive;
            template.Version += 1;
            template.UpdateDateTime = DateTime.UtcNow;
            template.UpdateBy = userId;

            // Sinkronisasi tasks
            var requestedIds = request.Tasks.Where(t => t.Id.HasValue).Select(t => t.Id!.Value).ToHashSet();
            foreach (var oldTask in template.Tasks)
            {
                if (!requestedIds.Contains(oldTask.Id))
                {
                    oldTask.IsDelete = true;
                    oldTask.DeleteDateTime = DateTime.UtcNow;
                    oldTask.DeleteBy = userId;
                }
            }

            foreach (var reqTask in request.Tasks)
            {
                if (reqTask.Id.HasValue)
                {
                    var existing = template.Tasks.FirstOrDefault(t => t.Id == reqTask.Id.Value);
                    if (existing != null)
                    {
                        existing.TaskName = reqTask.TaskName.Trim();
                        existing.TaskCategory = reqTask.TaskCategory;
                        existing.Description = reqTask.Description;
                        existing.DueDayOffset = reqTask.DueDayOffset;
                        existing.IsRequired = string.IsNullOrWhiteSpace(existing.SystemItemCode) ? reqTask.IsRequired : true;
                        existing.IsActive = reqTask.IsActive;

                        if (string.IsNullOrWhiteSpace(existing.SystemItemCode))
                        {
                            existing.ResponsibleRole = reqTask.ResponsibleRole;
                            existing.ItemKind = reqTask.ItemKind;
                            existing.AutomaticSource = reqTask.AutomaticSource;
                            existing.CertificationTypeId = reqTask.CertificationTypeId;
                        }

                        existing.UpdateDateTime = DateTime.UtcNow;
                        existing.UpdateBy = userId;
                    }
                }
                else
                {
                    template.Tasks.Add(new MstOnboardingTemplateTask
                    {
                        Id = Guid.NewGuid(),
                        OnboardingTemplateId = template.Id,
                        TaskCode = reqTask.TaskCode.Trim(),
                        TaskName = reqTask.TaskName.Trim(),
                        TaskCategory = reqTask.TaskCategory,
                        Description = reqTask.Description,
                        ResponsibleRole = reqTask.ResponsibleRole,
                        ItemKind = reqTask.ItemKind,
                        AutomaticSource = reqTask.AutomaticSource,
                        CertificationTypeId = reqTask.CertificationTypeId,
                        SystemItemCode = reqTask.SystemItemCode,
                        IsRequired = reqTask.IsRequired,
                        DueDayOffset = reqTask.DueDayOffset,
                        IsActive = true,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = userId
                    });
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(template.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<bool>> DeleteAsync(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var template = await _dbContext.MstOnboardingTemplates
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDelete, cancellationToken);

            if (template == null)
            {
                return RecruitmentResult<bool>.NotFound("Template onboarding tidak ditemukan.");
            }

            // Cek apakah sudah dipakai oleh kasus onboarding aktif
            var isUsed = await _dbContext.HrdOnboardingCaseItems
                .AnyAsync(i => template.Tasks.Select(t => t.Id).Contains(i.SourceTemplateTaskId ?? Guid.Empty) && !i.IsDelete, cancellationToken);

            if (isUsed)
            {
                return RecruitmentResult<bool>.Unprocessable("Template tidak dapat dihapus karena sudah digunakan oleh kasus onboarding aktif.");
            }

            template.IsDelete = true;
            template.DeleteDateTime = DateTime.UtcNow;
            template.DeleteBy = userId;

            foreach (var task in template.Tasks)
            {
                task.IsDelete = true;
                task.DeleteDateTime = DateTime.UtcNow;
                task.DeleteBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Template onboarding berhasil dihapus.");
        }

        private static List<MstOnboardingTemplateTask> BuildTasksForCreate(
            CreateOnboardingTemplateRequest request,
            Guid templateId,
            Guid userId)
        {
            var tasks = new List<MstOnboardingTemplateTask>();
            var userTasks = request.Tasks ?? new List<CreateOnboardingTemplateTaskRequest>();

            foreach (var req in userTasks)
            {
                tasks.Add(new MstOnboardingTemplateTask
                {
                    Id = Guid.NewGuid(),
                    OnboardingTemplateId = templateId,
                    TaskCode = req.TaskCode.Trim(),
                    TaskName = req.TaskName.Trim(),
                    TaskCategory = req.TaskCategory,
                    Description = req.Description,
                    ResponsibleRole = req.ResponsibleRole,
                    ItemKind = req.ItemKind,
                    AutomaticSource = req.AutomaticSource,
                    CertificationTypeId = req.CertificationTypeId,
                    SystemItemCode = req.SystemItemCode,
                    IsRequired = req.IsRequired,
                    DueDayOffset = req.DueDayOffset,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                });
            }

            // Sisipkan butir sistem wajib bila belum dimasukkan user
            if (request.TemplateLayer == OnboardingTemplateLayer.General)
            {
                EnsureSystemTask(tasks, templateId, userId, EmploymentLifecycleValueConstants.ItemFirstDayAttendance,
                    "FIRST_DAY_ATTENDANCE", "Kehadiran Hari Pertama", "Orientasi", OnboardingResponsibleRole.Hrd, OnboardingItemKind.Manual, null, 0);

                EnsureSystemTask(tasks, templateId, userId, EmploymentLifecycleValueConstants.ItemContractSigned,
                    "CONTRACT_SIGNED", "Tanda Tangan Kontrak Kerja", "Legalitas", OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.ContractSigned, 0);

                EnsureSystemTask(tasks, templateId, userId, EmploymentLifecycleValueConstants.ItemFitnessClearance,
                    "FITNESS_CLEARANCE", "Kelaikan Hasil MCU Pra-Kerja", "Kesehatan", OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.FitnessClearance, 0);

                EnsureSystemTask(tasks, templateId, userId, EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned,
                    "DIRECT_SUPERVISOR_ASSIGNED", "Penetapan Atasan Langsung", "Organisasi", OnboardingResponsibleRole.Hrd, OnboardingItemKind.Manual, null, 0);
            }
            else if (request.TemplateLayer == OnboardingTemplateLayer.Clinical)
            {
                EnsureSystemTask(tasks, templateId, userId, EmploymentLifecycleValueConstants.ItemPreceptorAssigned,
                    "PRECEPTOR_ASSIGNED", "Penetapan Preceptor / Pendamping Klinis", "Klinis", OnboardingResponsibleRole.UnitHead, OnboardingItemKind.Manual, null, 0);
            }

            return tasks;
        }

        private static void EnsureSystemTask(
            List<MstOnboardingTemplateTask> tasks,
            Guid templateId,
            Guid userId,
            string systemItemCode,
            string defaultTaskCode,
            string defaultTaskName,
            string category,
            OnboardingResponsibleRole role,
            OnboardingItemKind kind,
            OnboardingAutomaticSource? source,
            int dueDayOffset)
        {
            var existing = tasks.FirstOrDefault(t => t.SystemItemCode == systemItemCode);
            if (existing == null)
            {
                tasks.Insert(0, new MstOnboardingTemplateTask
                {
                    Id = Guid.NewGuid(),
                    OnboardingTemplateId = templateId,
                    TaskCode = defaultTaskCode,
                    TaskName = defaultTaskName,
                    TaskCategory = category,
                    SystemItemCode = systemItemCode,
                    ResponsibleRole = role,
                    ItemKind = kind,
                    AutomaticSource = source,
                    IsRequired = true,
                    DueDayOffset = dueDayOffset,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = userId
                });
            }
            else
            {
                existing.IsRequired = true;
                existing.ResponsibleRole = role;
                existing.ItemKind = kind;
                existing.AutomaticSource = source;
            }
        }

        private static OnboardingTemplateResponse MapToResponse(MstOnboardingTemplate t)
        {
            return new OnboardingTemplateResponse
            {
                Id = t.Id,
                TemplateCode = t.TemplateCode,
                TemplateName = t.TemplateName,
                TemplateLayer = t.TemplateLayer,
                OrganizationUnitId = t.OrganizationUnitId,
                OrganizationUnitName = t.OrganizationUnit?.UnitName,
                TargetCompletionDays = t.TargetCompletionDays,
                Version = t.Version,
                Description = t.Description,
                IsActive = t.IsActive,
                Tasks = t.Tasks.Where(x => !x.IsDelete).OrderBy(x => x.DueDayOffset).Select(x => new OnboardingTemplateTaskResponse
                {
                    Id = x.Id,
                    OnboardingTemplateId = x.OnboardingTemplateId,
                    TaskCode = x.TaskCode,
                    TaskName = x.TaskName,
                    TaskCategory = x.TaskCategory,
                    Description = x.Description,
                    ResponsibleRole = x.ResponsibleRole,
                    ItemKind = x.ItemKind,
                    AutomaticSource = x.AutomaticSource,
                    CertificationTypeId = x.CertificationTypeId,
                    CertificationTypeName = x.CertificationType?.CertificationTypeName,
                    SystemItemCode = x.SystemItemCode,
                    IsRequired = x.IsRequired,
                    DueDayOffset = x.DueDayOffset,
                    IsActive = x.IsActive
                }).ToList()
            };
        }
    }
}
