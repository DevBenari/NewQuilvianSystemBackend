using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdProbationService : IHrdProbationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmployeeEmploymentRecordService _employmentRecordService;

        public HrdProbationService(
            ApplicationDbContext dbContext,
            IEmployeeEmploymentRecordService employmentRecordService)
        {
            _dbContext = dbContext;
            _employmentRecordService = employmentRecordService;
        }

        public async Task<HrdProbation?> CreateForContractAsync(
            WfpContractHistory contract,
            Guid hospitalSiteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) return null;

            // 1. Kriteria: hanya untuk Initial contract (kontrak Renewal atau Conversion tidak menghasilkan masa percobaan)
            if (contract.HistoryType != "Initial")
            {
                return null;
            }

            // 2. Kriteria: hanya bila jenis kontrak memberlakukan probation (PKWTT, bukan PKWT)
            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(c => c.Id == contract.ContractTypeId && !c.IsDelete, cancellationToken);

            if (contractType == null || !contractType.IsProbationApplicable)
            {
                return null;
            }

            // 3. Kriteria: satu kontrak paling banyak satu masa percobaan
            var existing = await _dbContext.HrdProbations
                .FirstOrDefaultAsync(p => p.ContractHistoryId == contract.Id && !p.IsDelete, cancellationToken);

            if (existing != null)
            {
                return existing;
            }

            // Ambil pengaturan hari pengingat
            var setting = await _dbContext.HrdEmploymentLifecycleSettings
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);
            var reminderDays = setting?.ProbationReminderDays ?? 14;

            var startDate = DateOnly.FromDateTime(contract.StartDate);
            var endDate = startDate.AddMonths(3).AddDays(-1); // Tepat 3 bulan (UU 13/2003 Pasal 60)
            var reminderFromDate = endDate.AddDays(-reminderDays);

            var probation = new HrdProbation
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = contract.WorkforceProfileId,
                ContractHistoryId = contract.Id,
                StartDate = startDate,
                EndDate = endDate,
                ReminderFromDate = reminderFromDate,
                ProbationStatus = ProbationStatus.Running,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdProbations.AddAsync(probation, cancellationToken);

            // Perbarui ProbationEndDate pada kontrak
            contract.ProbationEndDate = endDate.ToDateTime(TimeOnly.MinValue);
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Sinkronkan ke MstEmployee
            await _employmentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);

            return probation;
        }

        public async Task<RecruitmentResult<PagedResult<HrdProbationResponse>>> GetPagedAsync(
            HrdProbationQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdProbations
                .AsNoTracking()
                .Include(p => p.WorkforceProfile)
                .Include(p => p.ContractHistory)
                .Where(p => !p.IsDelete);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.ProbationStatus == query.Status.Value);
            }

            if (query.EndDateFrom.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.EndDate >= query.EndDateFrom.Value);
            }

            if (query.EndDateTo.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.EndDate <= query.EndDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    (p.WorkforceProfile != null && (p.WorkforceProfile.ProfileCode.ToLower().Contains(term) || p.WorkforceProfile.DisplayName.ToLower().Contains(term))) ||
                    (p.ContractHistory != null && p.ContractHistory.ContractNumber.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderBy(p => p.EndDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new HrdProbationResponse
                {
                    Id = p.Id,
                    WorkforceProfileId = p.WorkforceProfileId,
                    WorkforceCode = p.WorkforceProfile != null ? p.WorkforceProfile.ProfileCode : string.Empty,
                    FullName = p.WorkforceProfile != null ? p.WorkforceProfile.DisplayName : string.Empty,
                    ContractHistoryId = p.ContractHistoryId,
                    ContractNumber = p.ContractHistory != null ? p.ContractHistory.ContractNumber : string.Empty,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ReminderFromDate = p.ReminderFromDate,
                    ProbationStatus = p.ProbationStatus,
                    ClosedAt = p.ClosedAt,
                    RowVersion = p.RowVersion
                })
                .ToListAsync(cancellationToken);

            var paged = new PagedResult<HrdProbationResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<HrdProbationResponse>>.Ok(paged, "Daftar masa percobaan berhasil diambil.");
        }

        public async Task<RecruitmentResult<HrdProbationResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdProbations
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ContractHistory)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<HrdProbationResponse>.NotFound("Data masa percobaan tidak ditemukan.");
            }

            var response = new HrdProbationResponse
            {
                Id = p.Id,
                WorkforceProfileId = p.WorkforceProfileId,
                WorkforceCode = p.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = p.WorkforceProfile?.DisplayName ?? string.Empty,
                ContractHistoryId = p.ContractHistoryId,
                ContractNumber = p.ContractHistory?.ContractNumber ?? string.Empty,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ReminderFromDate = p.ReminderFromDate,
                ProbationStatus = p.ProbationStatus,
                ClosedAt = p.ClosedAt,
                RowVersion = p.RowVersion
            };

            return RecruitmentResult<HrdProbationResponse>.Ok(response, "Detail masa percobaan berhasil diambil.");
        }
    }
}
