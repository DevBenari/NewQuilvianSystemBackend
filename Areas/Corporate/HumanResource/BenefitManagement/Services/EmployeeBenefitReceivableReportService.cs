using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Laporan HR "Piutang Benefit Pegawai": piutang EMPLOYEE_BENEFIT dari AR Karyawan Billing, hanya dibaca.
    ///
    /// Sumber nilai finansial tetap Finance: <c>FinReceivable</c> (pokok, alokasi, outstanding, status) dan jadwal
    /// angsuran <c>FinReceivableInstallment</c> (potongan gaji yang BERHASIL = PaidAmount). Service ini tidak menulis
    /// apa pun, tidak menyimpan saldo sendiri, dan tidak memicu potongan gaji. Potongan gaji hanya berjalan dari
    /// perjanjian angsuran yang disetujui Finance dan hasil payroll yang diterima Finance; memilih AR di Kasir tidak
    /// pernah memotong gaji.
    ///
    /// Tanpa N+1: setiap halaman memakai kueri tetap (piutang, perjanjian, angsuran, invoice, pasien, pemilik).
    /// </summary>
    public sealed class EmployeeBenefitReceivableReportService
    {
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public EmployeeBenefitReceivableReportService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Perjanjian yang sah menghasilkan potongan: sudah disetujui, atau sudah selesai dicicil.
        private static bool IsEffectivePlanStatus(string status) =>
            status == FinReceivableInstallmentPlanStatuses.Disetujui
            || status == FinReceivableInstallmentPlanStatuses.Selesai;

        public async Task<PagedResult<EmployeeBenefitReceivableReportRow>> GetPagedAsync(
            EmployeeBenefitReceivableReportQuery request,
            CancellationToken cancellationToken)
        {
            request ??= new EmployeeBenefitReceivableReportQuery();
            var pageNumber = Math.Max(request.PageNumber, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

            var query = BuildQuery(request);

            var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "duedate" => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
                "outstandingamount" => descending ? query.OrderByDescending(x => x.OutstandingAmount) : query.OrderBy(x => x.OutstandingAmount),
                "receivablenumber" => descending ? query.OrderByDescending(x => x.ReceivableNumber) : query.OrderBy(x => x.ReceivableNumber),
                _ => descending ? query.OrderByDescending(x => x.RecognizedAt) : query.OrderBy(x => x.RecognizedAt)
            };

            var total = await query.CountAsync(cancellationToken);
            var entities = await query
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);

            var rows = await BuildRowsAsync(entities, cancellationToken);

            return new PagedResult<EmployeeBenefitReceivableReportRow>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows
            };
        }

        public async Task<EmployeeBenefitReceivableReportSummaryResponse> GetSummaryAsync(
            EmployeeBenefitReceivableReportQuery request,
            CancellationToken cancellationToken)
        {
            request ??= new EmployeeBenefitReceivableReportQuery();
            var query = BuildQuery(request);

            var totals = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    Original = g.Sum(x => x.OriginalAmount),
                    Outstanding = g.Sum(x => x.OutstandingAmount),
                    Allocated = g.Sum(x => x.AllocatedAmount),
                    SettledCount = g.Count(x => x.Status == FinReceivableStatuses.Settled)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var receivableIds = query.Select(x => x.Id);

            // Sedang dicicil: ada perjanjian DISETUJUI (belum SELESAI) dan piutang belum lunas.
            var underInstallment = await query
                .Where(x => x.Status != FinReceivableStatuses.Settled && x.Status != FinReceivableStatuses.WrittenOff
                    && _dbContext.FinReceivableInstallmentPlans.Any(p =>
                        !p.IsDelete && p.ReceivableId == x.Id && p.Status == FinReceivableInstallmentPlanStatuses.Disetujui))
                .GroupBy(_ => 1)
                .Select(g => new { Count = g.Count(), Outstanding = g.Sum(x => x.OutstandingAmount) })
                .FirstOrDefaultAsync(cancellationToken);

            var deducted = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(i => !i.IsDelete
                    && i.Status != FinReceivableInstallmentStatuses.Dibatalkan
                    && _dbContext.FinReceivableInstallmentPlans.Any(p =>
                        !p.IsDelete && p.Id == i.PlanId && receivableIds.Contains(p.ReceivableId)
                        && (p.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                            || p.Status == FinReceivableInstallmentPlanStatuses.Selesai)))
                .SumAsync(i => (decimal?)i.PaidAmount, cancellationToken) ?? 0m;

            return new EmployeeBenefitReceivableReportSummaryResponse
            {
                TotalReceivable = totals?.Count ?? 0,
                TotalOriginalAmount = totals?.Original ?? 0m,
                TotalOutstandingAmount = totals?.Outstanding ?? 0m,
                TotalAllocatedAmount = totals?.Allocated ?? 0m,
                SettledCount = totals?.SettledCount ?? 0,
                UnderInstallmentCount = underInstallment?.Count ?? 0,
                UnderInstallmentOutstandingAmount = underInstallment?.Outstanding ?? 0m,
                TotalPayrollDeductedAmount = deducted
            };
        }

        public async Task<EmployeeBenefitReceivableReportDetailResponse> GetDetailAsync(
            Guid receivableId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.FinReceivables.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == receivableId && !x.IsDelete
                    && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit, cancellationToken)
                ?? throw new KeyNotFoundException("Piutang benefit pegawai tidak ditemukan.");

            var row = (await BuildRowsAsync(new List<FinReceivable> { entity }, cancellationToken)).Single();

            var plans = (await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
                    .Where(p => !p.IsDelete && p.ReceivableId == receivableId)
                    .ToListAsync(cancellationToken))
                .OrderByDescending(p => p.RequestedAt)
                .ToList();
            var planIds = plans.Select(p => p.Id).ToList();
            var installments = planIds.Count == 0
                ? new List<FinReceivableInstallment>()
                : await _dbContext.FinReceivableInstallments.AsNoTracking()
                    .Where(i => !i.IsDelete && planIds.Contains(i.PlanId))
                    .OrderBy(i => i.InstallmentNumber)
                    .ToListAsync(cancellationToken);
            var installmentsByPlan = installments.ToLookup(i => i.PlanId);

            return new EmployeeBenefitReceivableReportDetailResponse
            {
                Receivable = row,
                Plans = plans.Select(p => new EmployeeBenefitReceivablePlanRow
                {
                    PlanId = p.Id,
                    PlanNumber = p.PlanNumber,
                    Status = p.Status,
                    InstallmentCount = p.InstallmentCount,
                    InstallmentAmount = p.InstallmentAmount,
                    TotalAgreedAmount = p.TotalAgreedAmount,
                    FirstDeductionPeriod = p.FirstDeductionPeriod,
                    ApprovedAt = p.ApprovedAt,
                    Installments = installmentsByPlan[p.Id].Select(i => new EmployeeBenefitReceivableInstallmentRow
                    {
                        InstallmentNumber = i.InstallmentNumber,
                        DeductionPeriod = i.DeductionPeriod,
                        ScheduledAmount = i.ScheduledAmount,
                        CarriedOverAmount = i.CarriedOverAmount,
                        PaidAmount = i.PaidAmount,
                        OutstandingAmount = i.OutstandingAmount,
                        Status = i.Status,
                        LastResultAt = i.LastResultAt
                    }).ToList()
                }).ToList()
            };
        }

        // ------------------------------------------------------------------------------------
        // Saringan
        // ------------------------------------------------------------------------------------

        private IQueryable<FinReceivable> BuildQuery(EmployeeBenefitReceivableReportQuery request)
        {
            var query = _dbContext.FinReceivables.AsNoTracking()
                .Where(x => !x.IsDelete && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit);

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim().ToUpperInvariant();
                query = query.Where(x => x.Status == status);
            }
            else
            {
                // Piutang yang dibatalkan (mis. koreksi pemilik salah orang) bukan piutang aktif pegawai.
                query = query.Where(x => x.Status != FinReceivableStatuses.Cancelled);
            }

            if (!string.IsNullOrWhiteSpace(request.EmployeeArOrigin))
            {
                var origin = request.EmployeeArOrigin.Trim().ToUpperInvariant();
                query = query.Where(x => x.EmployeeArOrigin == origin);
            }

            if (request.BenefitOwnerWorkforceProfileId.HasValue)
            {
                // Piutang lama hanya membawa MstEmployee.Id; diterjemahkan ke Workforce tanpa mengubah datanya.
                var ownerProfileId = request.BenefitOwnerWorkforceProfileId.Value;
                var ownerEmployeeIds = _dbContext.MstEmployees.AsNoTracking()
                    .Where(e => e.WorkforceProfileId == ownerProfileId)
                    .Select(e => e.Id);
                query = query.Where(x =>
                    x.BenefitOwnerWorkforceProfileId == ownerProfileId
                    || (!x.BenefitOwnerWorkforceProfileId.HasValue && x.BenefitOwnerId.HasValue
                        && ownerEmployeeIds.Contains(x.BenefitOwnerId.Value)));
            }

            if (request.RecognizedFrom.HasValue)
            {
                var from = FinanceBusinessDate.GetStartOfDayUtc(request.RecognizedFrom.Value);
                query = query.Where(x => x.RecognizedAt >= from);
            }
            if (request.RecognizedTo.HasValue)
            {
                var toExclusive = FinanceBusinessDate.GetEndOfDayUtc(request.RecognizedTo.Value);
                query = query.Where(x => x.RecognizedAt < toExclusive);
            }
            if (request.DueDateFrom.HasValue) query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
            if (request.DueDateTo.HasValue) query = query.Where(x => x.DueDate <= request.DueDateTo.Value);

            if (request.HasApprovedInstallmentPlan.HasValue)
            {
                var wantsPlan = request.HasApprovedInstallmentPlan.Value;
                query = query.Where(x =>
                    _dbContext.FinReceivableInstallmentPlans.Any(p =>
                        !p.IsDelete && p.ReceivableId == x.Id
                        && (p.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                            || p.Status == FinReceivableInstallmentPlanStatuses.Selesai)) == wantsPlan);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToUpper();

                var matchingInvoiceIds = _dbContext.BilInvoices.AsNoTracking()
                    .Where(i => i.InvoiceNumber.ToUpper().Contains(search))
                    .Select(i => i.Id);

                var matchingReceivableIdsFromPatient = _dbContext.FinReceivableItems.AsNoTracking()
                    .Where(item => item.PatientId.HasValue &&
                        _dbContext.MstPatients.Any(p => !p.IsDelete && p.Id == item.PatientId.Value &&
                            (p.FullName.ToUpper().Contains(search) || p.MedicalRecordNumber.ToUpper().Contains(search))))
                    .Select(item => item.ReceivableId);

                var matchingOwnerEmployees = _dbContext.MstEmployees.AsNoTracking()
                    .Where(e => !e.IsDelete &&
                        (e.FullName.ToUpper().Contains(search) || e.EmployeeNumber.ToUpper().Contains(search)));
                var matchingOwnerEmployeeIds = matchingOwnerEmployees.Select(e => e.Id);
                var matchingOwnerEmployeeProfileIds = matchingOwnerEmployees.Select(e => e.WorkforceProfileId);
                var matchingOwnerDoctorProfileIds = _dbContext.MstDoctors.AsNoTracking()
                    .Where(d => !d.IsDelete &&
                        (d.FullName.ToUpper().Contains(search) || d.DoctorNumber.ToUpper().Contains(search)))
                    .Select(d => d.WorkforceProfileId);

                query = query.Where(x =>
                    x.ReceivableNumber.ToUpper().Contains(search)
                    || (x.InvoiceId.HasValue && matchingInvoiceIds.Contains(x.InvoiceId.Value))
                    || matchingReceivableIdsFromPatient.Contains(x.Id)
                    || (x.BenefitOwnerWorkforceProfileId.HasValue
                        && (matchingOwnerEmployeeProfileIds.Contains(x.BenefitOwnerWorkforceProfileId.Value)
                            || matchingOwnerDoctorProfileIds.Contains(x.BenefitOwnerWorkforceProfileId.Value)))
                    || (x.BenefitOwnerId.HasValue && matchingOwnerEmployeeIds.Contains(x.BenefitOwnerId.Value)));
            }

            return query;
        }

        // ------------------------------------------------------------------------------------
        // Baris laporan
        // ------------------------------------------------------------------------------------

        private async Task<List<EmployeeBenefitReceivableReportRow>> BuildRowsAsync(
            IReadOnlyList<FinReceivable> entities,
            CancellationToken cancellationToken)
        {
            if (entities.Count == 0) return new List<EmployeeBenefitReceivableReportRow>();

            var receivableIds = entities.Select(x => x.Id).ToList();

            var ownerLookup = await FinanceBenefitOwnerDirectory.LoadAsync(
                _dbContext,
                entities
                    .Where(x => x.BenefitOwnerId.HasValue || x.BenefitOwnerWorkforceProfileId.HasValue)
                    .Select(x => ((Guid?)x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId)),
                cancellationToken);

            var invoiceIds = entities.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).Distinct().ToList();
            var invoiceNumbers = invoiceIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.BilInvoices.AsNoTracking()
                    .Where(i => invoiceIds.Contains(i.Id))
                    .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, cancellationToken);

            var patientByReceivable = (await _dbContext.FinReceivableItems.AsNoTracking()
                    .Where(item => receivableIds.Contains(item.ReceivableId) && item.PatientId.HasValue)
                    .Select(item => new { item.ReceivableId, PatientId = item.PatientId!.Value })
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.ReceivableId)
                .ToDictionary(g => g.Key, g => g.First().PatientId);
            var patientIds = patientByReceivable.Values.Distinct().ToList();
            var patients = patientIds.Count == 0
                ? new Dictionary<Guid, (string Name, string Mrn)>()
                : (await _dbContext.MstPatients.AsNoTracking()
                        .Where(p => !p.IsDelete && patientIds.Contains(p.Id))
                        .Select(p => new { p.Id, p.FullName, p.MedicalRecordNumber })
                        .ToListAsync(cancellationToken))
                    .ToDictionary(p => p.Id, p => (Name: p.FullName, Mrn: p.MedicalRecordNumber));

            var plans = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
                .Where(p => !p.IsDelete && receivableIds.Contains(p.ReceivableId)
                    && (p.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                        || p.Status == FinReceivableInstallmentPlanStatuses.Selesai))
                .ToListAsync(cancellationToken);
            var planIds = plans.Select(p => p.Id).ToList();
            var installments = planIds.Count == 0
                ? new List<FinReceivableInstallment>()
                : await _dbContext.FinReceivableInstallments.AsNoTracking()
                    .Where(i => !i.IsDelete && planIds.Contains(i.PlanId)
                        && i.Status != FinReceivableInstallmentStatuses.Dibatalkan)
                    .ToListAsync(cancellationToken);
            var installmentsByPlan = installments.ToLookup(i => i.PlanId);

            var rows = new List<EmployeeBenefitReceivableReportRow>(entities.Count);
            foreach (var x in entities)
            {
                var row = new EmployeeBenefitReceivableReportRow
                {
                    ReceivableId = x.Id,
                    ReceivableNumber = x.ReceivableNumber,
                    RecognizedAt = x.RecognizedAt,
                    InvoiceId = x.InvoiceId,
                    BenefitRelationship = x.BenefitRelationship,
                    EmployeeArOrigin = x.EmployeeArOrigin,
                    OriginalAmount = x.OriginalAmount,
                    AllocatedAmount = x.AllocatedAmount,
                    AdjustedAmount = x.AdjustedAmount,
                    WrittenOffAmount = x.WrittenOffAmount,
                    OutstandingAmount = x.OutstandingAmount,
                    Status = x.Status,
                    DueDate = x.DueDate
                };

                if (x.InvoiceId.HasValue && invoiceNumbers.TryGetValue(x.InvoiceId.Value, out var invoiceNumber))
                    row.InvoiceNumber = invoiceNumber;

                var owner = ownerLookup.Find(x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId);
                row.BenefitOwnerWorkforceProfileId = owner?.WorkforceProfileId ?? ownerLookup.ResolveProfileId(x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId);
                if (owner is not null)
                {
                    row.BenefitOwnerType = owner.OwnerType;
                    row.BenefitOwnerStaffNumber = owner.StaffNumber;
                    row.BenefitOwnerName = owner.StaffName;
                }

                if (patientByReceivable.TryGetValue(x.Id, out var patientId))
                {
                    row.PatientId = patientId;
                    if (patients.TryGetValue(patientId, out var patient))
                    {
                        row.PatientName = patient.Name;
                        row.PatientMedicalRecordNumber = patient.Mrn;
                    }
                }

                // Perjanjian terbaru yang sah (DISETUJUI / SELESAI). Potongan berhasil = jumlah PaidAmount seluruh
                // angsuran perjanjian yang sah (tidak dihitung dobel antar perjanjian karena satu piutang hanya
                // punya satu perjanjian aktif; perjanjian lama yang SELESAI/dibatalkan tetap membawa PaidAmount-nya).
                var receivablePlans = plans.Where(p => p.ReceivableId == x.Id)
                    .OrderByDescending(p => p.ApprovedAt ?? p.RequestedAt)
                    .ToList();
                if (receivablePlans.Count > 0)
                {
                    var current = receivablePlans[0];
                    row.InstallmentPlanId = current.Id;
                    row.InstallmentPlanNumber = current.PlanNumber;
                    row.InstallmentPlanStatus = current.Status;
                    row.InstallmentCount = current.InstallmentCount;
                    row.InstallmentAmount = current.InstallmentAmount;
                    row.FirstDeductionPeriod = current.FirstDeductionPeriod;

                    var all = receivablePlans.SelectMany(p => installmentsByPlan[p.Id]).ToList();
                    row.PayrollDeductedAmount = all.Sum(i => i.PaidAmount);

                    var latest = all
                        .Where(i => i.LastResultAt.HasValue)
                        .OrderByDescending(i => i.LastResultAt)
                        .FirstOrDefault();
                    if (latest is not null)
                    {
                        row.LatestDeductionPeriod = latest.DeductionPeriod;
                        row.LatestDeductionStatus = latest.Status;
                        row.LatestDeductionResultAt = latest.LastResultAt;
                    }
                }

                rows.Add(row);
            }

            return rows;
        }
    }
}
