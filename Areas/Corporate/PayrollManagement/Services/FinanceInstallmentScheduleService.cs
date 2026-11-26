using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Jadwal angsuran potongan pegawai yang dibaca LANGSUNG dari Finance AR (PAY-BE-FIX-029).
    /// </summary>
    public interface IFinanceInstallmentScheduleService
    {
        Task<FinanceInstallmentScheduleResponseDto> GetAsync(FinanceInstallmentScheduleFilterDto filter, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Finance AR adalah sumber kebenaran piutang; halaman Jadwal Angsuran Potongan membaca angsurannya (FinReceivableInstallment),
    /// perjanjiannya (FinReceivableInstallmentPlan), dan piutangnya (FinReceivable) tanpa salinan ke tabel HRD dan tanpa sinkronisasi.
    /// Operasi ini HANYA MEMBACA: tidak mengubah saldo, jadwal, atau status Finance, tidak membuat Payroll Run, dan tidak membuat input
    /// potongan. Pemilik piutang dibaca lewat identitas Workforce (karyawan maupun dokter tetap).
    ///
    /// Granularitas: satu baris = satu angsuran (satu periode). Jadwal HRD lama (satu baris per perjanjian) disajikan terpisah dan
    /// hanya bila TIDAK menunjuk perjanjian Finance (ExternalReferenceId = PlanNumber pada sinkronisasi lama); yang menunjuk
    /// perjanjian yang sama disembunyikan agar tidak ada penggandaan, dan jumlahnya dilaporkan.
    /// </summary>
    public sealed class FinanceInstallmentScheduleService : IFinanceInstallmentScheduleService
    {
        // Sama dengan sumber input potongan Payroll modern (PayrollInstallmentDeductionService.InstallmentSourceType).
        private const string PayrollInstallmentSourceType = "FinanceReceivableInstallment";
        private const int MaximumPageSize = 100;
        private const int MaximumLegacyRows = 200;

        private static readonly Regex PeriodPattern = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

        private static readonly string[] OutstandingInstallmentStatuses =
        {
            FinReceivableInstallmentStatuses.Dijadwalkan,
            FinReceivableInstallmentStatuses.TerbayarSebagian,
            FinReceivableInstallmentStatuses.Tertunggak
        };

        private static readonly string[] KnownInstallmentStatuses =
        {
            FinReceivableInstallmentStatuses.Dijadwalkan,
            FinReceivableInstallmentStatuses.TerbayarSebagian,
            FinReceivableInstallmentStatuses.Terbayar,
            FinReceivableInstallmentStatuses.Tertunggak,
            FinReceivableInstallmentStatuses.Dibatalkan
        };

        private static readonly string[] LegacyOutstandingStatuses = { "Active", "Paused" };

        private readonly ApplicationDbContext _dbContext;

        public FinanceInstallmentScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private sealed record ScheduleRow(
            Guid InstallmentId,
            Guid PlanId,
            Guid ReceivableId,
            string PlanNumber,
            string ReceivableNumber,
            int InstallmentNumber,
            int InstallmentCount,
            string DeductionPeriod,
            Guid? BenefitOwnerId,
            Guid? BenefitOwnerWorkforceProfileId,
            decimal ScheduledAmount,
            decimal CarriedOverAmount,
            decimal PaidAmount,
            decimal OutstandingAmount,
            decimal ReceivableOutstandingAmount,
            decimal TotalAgreedAmount,
            string AgreementStatus,
            string InstallmentStatus);

        public async Task<FinanceInstallmentScheduleResponseDto> GetAsync(
            FinanceInstallmentScheduleFilterDto filter, CancellationToken cancellationToken = default)
        {
            filter ??= new FinanceInstallmentScheduleFilterDto();
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 25 : Math.Min(filter.PageSize, MaximumPageSize);
            var scope = NormalizeScope(filter.InstallmentStatus);
            var period = NormalizePeriod(filter.Period);
            var owners = await ResolveOwnerFilterAsync(filter, cancellationToken);

            var response = new FinanceInstallmentScheduleResponseDto { PageNumber = pageNumber, PageSize = pageSize };

            // Pemilik tidak dikenal (filter menunjuk pegawai tanpa piutang atau identitas tidak valid): hasil kosong, bukan semua data.
            if (owners is { IsEmpty: true })
            {
                response.TotalPage = 1;
                return response;
            }

            var query = BuildQuery(scope, period, owners);

            // Ringkasan dari himpunan filter YANG SAMA dengan tabel. Baris skalar ringan; saldo piutang dijumlahkan sekali per piutang.
            var summarySource = await query
                .Select(x => new
                {
                    x.PlanId,
                    x.ReceivableId,
                    x.BenefitOwnerId,
                    x.BenefitOwnerWorkforceProfileId,
                    x.InstallmentStatus,
                    x.AgreementStatus,
                    x.ScheduledAmount,
                    x.OutstandingAmount,
                    x.ReceivableOutstandingAmount
                })
                .ToListAsync(cancellationToken);

            response.TotalData = summarySource.Count;
            response.TotalPage = Math.Max(1, (int)Math.Ceiling(summarySource.Count / (double)pageSize));

            var ownerPairs = summarySource
                .Select(x => (EmployeeId: x.BenefitOwnerId, WorkforceProfileId: x.BenefitOwnerWorkforceProfileId))
                .Distinct()
                .ToList();
            var ownerLookup = await FinanceBenefitOwnerDirectory.LoadAsync(_dbContext, ownerPairs, cancellationToken);

            var active = summarySource.Where(x => x.InstallmentStatus != FinReceivableInstallmentStatuses.Dibatalkan).ToList();
            response.Summary = new FinanceInstallmentScheduleSummaryDto
            {
                InstallmentCount = summarySource.Count,
                AgreementCount = summarySource.Select(x => x.PlanId).Distinct().Count(),
                OwnerCount = ownerPairs
                    .Select(pair =>
                    {
                        var owner = ownerLookup.Find(pair.EmployeeId, pair.WorkforceProfileId);
                        return owner?.WorkforceProfileId ?? pair.WorkforceProfileId ?? pair.EmployeeId;
                    })
                    .Distinct()
                    .Count(),
                TotalScheduledAmount = active.Sum(x => x.ScheduledAmount),
                TotalOutstandingInstallment = active.Sum(x => x.OutstandingAmount),
                OverdueCount = summarySource.Count(x => x.InstallmentStatus == FinReceivableInstallmentStatuses.Tertunggak),
                // Satu piutang dengan banyak angsuran dijumlahkan SEKALI; hanya perjanjian DISETUJUI yang masih menjadi kewajiban aktif.
                TotalReceivableOutstanding = summarySource
                    .Where(x => x.AgreementStatus == FinReceivableInstallmentPlanStatuses.Disetujui)
                    .GroupBy(x => x.ReceivableId)
                    .Sum(g => g.First().ReceivableOutstandingAmount)
            };

            var pageRows = await query
                .OrderBy(x => x.DeductionPeriod)
                .ThenBy(x => x.PlanNumber)
                .ThenBy(x => x.InstallmentNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var links = await LoadPayrollLinksAsync(pageRows.Select(r => r.InstallmentId).ToList(), cancellationToken);

            response.Items = pageRows.Select(r =>
            {
                var owner = ownerLookup.Find(r.BenefitOwnerId, r.BenefitOwnerWorkforceProfileId);
                links.TryGetValue(r.InstallmentId, out var link);
                return new FinanceInstallmentScheduleRowDto
                {
                    InstallmentId = r.InstallmentId,
                    PlanId = r.PlanId,
                    ReceivableId = r.ReceivableId,
                    PlanNumber = r.PlanNumber,
                    ReceivableNumber = r.ReceivableNumber,
                    InstallmentCode = $"{r.PlanNumber}-{r.InstallmentNumber}",
                    InstallmentNumber = r.InstallmentNumber,
                    InstallmentCount = r.InstallmentCount,
                    DeductionPeriod = r.DeductionPeriod,
                    WorkforceProfileId = owner?.WorkforceProfileId ?? r.BenefitOwnerWorkforceProfileId,
                    EmployeeId = owner?.EmployeeId ?? r.BenefitOwnerId,
                    OwnerType = owner?.OwnerType,
                    StaffNumber = owner?.StaffNumber,
                    StaffName = owner?.StaffName,
                    OrganizationUnitName = owner?.OrganizationUnitName,
                    ScheduledAmount = r.ScheduledAmount,
                    CarriedOverAmount = r.CarriedOverAmount,
                    PaidAmount = r.PaidAmount,
                    OutstandingAmount = r.OutstandingAmount,
                    ReceivableOutstandingAmount = r.ReceivableOutstandingAmount,
                    TotalAgreedAmount = r.TotalAgreedAmount,
                    AgreementStatus = r.AgreementStatus,
                    InstallmentStatus = r.InstallmentStatus,
                    PayrollRunNumber = link?.RunNumber,
                    PayrollRunStatus = link?.RunStatus,
                    PayrollInputStatus = link?.InputStatus
                };
            }).ToList();

            await FillLegacyAsync(response, scope, period, owners, cancellationToken);
            return response;
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // Filter
        // ------------------------------------------------------------------------------------------------------------------------

        private static string? NormalizeScope(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = value.Trim().ToUpperInvariant();
            if (normalized == DeductionScheduleScopes.Outstanding) return normalized;
            if (KnownInstallmentStatuses.Contains(normalized)) return normalized;
            throw new ArgumentException("Status angsuran tidak dikenali.");
        }

        private static string? NormalizePeriod(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var normalized = value.Trim();
            if (!PeriodPattern.IsMatch(normalized)) throw new ArgumentException("Periode harus berformat yyyy-MM.");
            return normalized;
        }

        private sealed record OwnerFilter(IReadOnlyCollection<Guid> EmployeeIds, IReadOnlyCollection<Guid> WorkforceProfileIds)
        {
            public bool IsEmpty => EmployeeIds.Count == 0 && WorkforceProfileIds.Count == 0;
        }

        /// <summary>
        /// Filter pegawai memakai dua identitas sekaligus: MstEmployee.Id (BenefitOwnerId) dan MstWorkforceProfile.Id
        /// (BenefitOwnerWorkforceProfileId, satu-satunya milik dokter tetap). Null = tidak ada filter pegawai.
        /// </summary>
        private async Task<OwnerFilter?> ResolveOwnerFilterAsync(FinanceInstallmentScheduleFilterDto filter, CancellationToken cancellationToken)
        {
            if (!filter.EmployeeId.HasValue && !filter.WorkforceProfileId.HasValue) return null;

            HashSet<Guid>? employeeIds = null;
            HashSet<Guid>? profileIds = null;

            if (filter.EmployeeId.HasValue)
            {
                var employee = await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.Id == filter.EmployeeId.Value)
                    .Select(x => new { x.Id, x.WorkforceProfileId })
                    .FirstOrDefaultAsync(cancellationToken);

                employeeIds = new HashSet<Guid> { filter.EmployeeId.Value };
                profileIds = new HashSet<Guid>();
                if (employee is not null && employee.WorkforceProfileId != Guid.Empty) profileIds.Add(employee.WorkforceProfileId);
            }

            if (filter.WorkforceProfileId.HasValue)
            {
                var profileId = filter.WorkforceProfileId.Value;
                var linkedEmployeeIds = await _dbContext.MstEmployees.AsNoTracking()
                    .Where(x => x.WorkforceProfileId == profileId)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                if (employeeIds is null)
                {
                    employeeIds = linkedEmployeeIds.ToHashSet();
                    profileIds = new HashSet<Guid> { profileId };
                }
                else
                {
                    // Kedua identitas diisi: harus menunjuk orang yang sama.
                    profileIds!.IntersectWith(new[] { profileId });
                    employeeIds.IntersectWith(linkedEmployeeIds);
                }
            }

            return new OwnerFilter(employeeIds ?? new HashSet<Guid>(), profileIds ?? new HashSet<Guid>());
        }

        private IQueryable<ScheduleRow> BuildQuery(string? scope, string? period, OwnerFilter? owners)
        {
            var query =
                from inst in _dbContext.FinReceivableInstallments.AsNoTracking()
                join plan in _dbContext.FinReceivableInstallmentPlans.AsNoTracking() on inst.PlanId equals plan.Id
                join rec in _dbContext.FinReceivables.AsNoTracking() on plan.ReceivableId equals rec.Id
                where !inst.IsDelete && !plan.IsDelete && !rec.IsDelete
                      // Hanya piutang pegawai/dokter (benefit karyawan) yang pemiliknya teridentifikasi.
                      && rec.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit
                      && (rec.BenefitOwnerId != null || rec.BenefitOwnerWorkforceProfileId != null)
                      // Perjanjian MENUNGGU/DITOLAK belum menjadi jadwal potongan.
                      && (plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                          || plan.Status == FinReceivableInstallmentPlanStatuses.Selesai
                          || plan.Status == FinReceivableInstallmentPlanStatuses.Dibatalkan)
                select new ScheduleRow(
                    inst.Id, plan.Id, rec.Id, plan.PlanNumber, rec.ReceivableNumber, inst.InstallmentNumber, plan.InstallmentCount,
                    inst.DeductionPeriod, rec.BenefitOwnerId, rec.BenefitOwnerWorkforceProfileId, inst.ScheduledAmount,
                    inst.CarriedOverAmount, inst.PaidAmount, inst.OutstandingAmount, rec.OutstandingAmount, plan.TotalAgreedAmount,
                    plan.Status, inst.Status);

            if (scope == DeductionScheduleScopes.Outstanding)
            {
                // Perjanjian DIBATALKAN atau SELESAI tidak pernah menjadi angsuran aktif.
                query = query.Where(x => x.AgreementStatus == FinReceivableInstallmentPlanStatuses.Disetujui
                                         && OutstandingInstallmentStatuses.Contains(x.InstallmentStatus));
            }
            else if (scope is not null)
            {
                query = query.Where(x => x.InstallmentStatus == scope);
            }

            if (period is not null) query = query.Where(x => x.DeductionPeriod == period);

            if (owners is not null)
            {
                var employeeIds = owners.EmployeeIds.ToList();
                var profileIds = owners.WorkforceProfileIds.ToList();
                query = query.Where(x =>
                    (x.BenefitOwnerId != null && employeeIds.Contains(x.BenefitOwnerId.Value))
                    || (x.BenefitOwnerWorkforceProfileId != null && profileIds.Contains(x.BenefitOwnerWorkforceProfileId.Value)));
            }

            return query;
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // Keterkaitan Payroll (informasi saja; status angsuran tetap milik Finance)
        // ------------------------------------------------------------------------------------------------------------------------

        private sealed record PayrollLink(string RunNumber, string RunStatus, string InputStatus);

        private async Task<Dictionary<Guid, PayrollLink>> LoadPayrollLinksAsync(List<Guid> installmentIds, CancellationToken cancellationToken)
        {
            if (installmentIds.Count == 0) return new Dictionary<Guid, PayrollLink>();

            var rows = await (
                from input in _dbContext.PrlPayrollVariableInputs.AsNoTracking()
                join employee in _dbContext.PrlPayrollRunEmployees.AsNoTracking() on input.PayrollRunEmployeeId equals employee.Id
                join run in _dbContext.PrlPayrollRuns.AsNoTracking() on employee.PayrollRunId equals run.Id
                where !input.IsDelete
                      && input.SourceType == PayrollInstallmentSourceType
                      && input.SourceId != null
                      && installmentIds.Contains(input.SourceId.Value)
                select new { SourceId = input.SourceId.Value, input.InputStatus, run.RunNumber, run.RunStatus, input.CreateDateTime })
                .ToListAsync(cancellationToken);

            // Satu angsuran dapat muncul pada beberapa run (hitung ulang atau run koreksi): yang terbaru yang ditampilkan.
            return rows
                .GroupBy(x => x.SourceId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var latest = g.OrderByDescending(x => x.CreateDateTime).First();
                        return new PayrollLink(latest.RunNumber, latest.RunStatus, latest.InputStatus);
                    });
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // Jadwal HRD lama (tanpa duplikasi dengan Finance)
        // ------------------------------------------------------------------------------------------------------------------------

        private async Task FillLegacyAsync(
            FinanceInstallmentScheduleResponseDto response, string? scope, string? period, OwnerFilter? owners, CancellationToken cancellationToken)
        {
            // Jadwal lama tidak punya periode per angsuran dan tidak memakai status Finance: filter periode atau status Finance
            // spesifik tidak berlaku bagi mereka (bukan "kosong karena tidak ada").
            if (period is not null) return;
            if (scope is not null && scope != DeductionScheduleScopes.Outstanding) return;

            var legacy = _dbContext.TrxPayrollDeductionSchedules.AsNoTracking().Where(s => !s.IsDelete);

            if (owners is not null)
            {
                var employeeIds = owners.EmployeeIds.ToList();
                legacy = legacy.Where(s => employeeIds.Contains(s.EmployeeId));
            }

            if (scope == DeductionScheduleScopes.Outstanding)
                legacy = legacy.Where(s => LegacyOutstandingStatuses.Contains(s.ScheduleStatus) && s.RemainingPrincipal > 0);

            // Salinan perjanjian Finance dikenali dari identitas terverifikasi (ExternalReferenceId = nomor perjanjian), bukan dari kemiripan nominal.
            var duplicates = legacy.Where(s => s.ExternalReferenceId != null
                && _dbContext.FinReceivableInstallmentPlans.Any(p => !p.IsDelete && p.PlanNumber == s.ExternalReferenceId));

            response.HiddenLegacyDuplicateCount = await duplicates.CountAsync(cancellationToken);

            var rows = await legacy
                .Where(s => s.ExternalReferenceId == null
                    || !_dbContext.FinReceivableInstallmentPlans.Any(p => !p.IsDelete && p.PlanNumber == s.ExternalReferenceId))
                .Include(s => s.Employee)
                .OrderBy(s => s.PriorityLevel)
                .ThenBy(s => s.CreateDateTime)
                .Take(MaximumLegacyRows)
                .ToListAsync(cancellationToken);

            response.LegacyItems = rows.Select(s => new LegacyDeductionScheduleRowDto
            {
                Id = s.Id,
                ScheduleNumber = s.ScheduleNumber,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee?.FullName,
                EmployeeNik = s.Employee?.EmployeeNumber,
                DeductionCategory = s.DeductionCategory,
                PriorityLevel = s.PriorityLevel,
                ExternalReferenceId = s.ExternalReferenceId,
                MonthlyInstallmentAmount = s.MonthlyInstallmentAmount,
                TotalInstallmentMonths = s.TotalInstallmentMonths,
                PaidInstallmentCount = s.PaidInstallmentCount,
                RemainingPrincipal = s.RemainingPrincipal,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                ScheduleStatus = s.ScheduleStatus
            }).ToList();
        }
    }
}
