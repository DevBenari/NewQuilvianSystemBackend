using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class RestitutionClaimService : IRestitutionClaimService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRestitutionSettingService _settingService;
        private readonly ILogger<RestitutionClaimService> _logger;

        public RestitutionClaimService(
            ApplicationDbContext dbContext,
            IRestitutionSettingService settingService,
            ILogger<RestitutionClaimService> logger)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<EmployeeRemainingQuotaDto> GetEmployeeRemainingQuotaAsync(
            Guid employeeId,
            string benefitCategory,
            CancellationToken cancellationToken = default)
        {
            var emp = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDelete, cancellationToken);

            if (emp == null)
            {
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            int serviceMonths = ((now.Year - emp.JoinDate.Year) * 12) + now.Month - emp.JoinDate.Month;
            if (serviceMonths < 0) serviceMonths = 0;

            string grade = !string.IsNullOrWhiteSpace(emp.GradeLevel) ? emp.GradeLevel : "GRADE-IV";
            var policy = await _settingService.GetPolicyForGradeAndCategoryAsync(grade, benefitCategory, cancellationToken);

            decimal annualLimit = policy?.AnnualLimitAmount ?? 0m;
            int minServiceMonths = policy?.MinimumServiceMonthsRequired ?? 0;
            bool isEligible = serviceMonths >= minServiceMonths;
            bool allowCito = policy?.AllowEmergencyCitoCashier ?? false;

            if (!isEligible)
            {
                annualLimit = 0m;
            }

            // Hitung akumulasi klaim approved dalam tahun berjalan
            var approvedStatuses = new[] { "Approved", "QueuedForPayroll", "ProcessedInPayroll", "VoucherIssued", "DisbursedViaCashier", "Closed" };
            var usedAmount = await _dbContext.TrxEmployeeRestitutionClaims
                .AsNoTracking()
                .Where(c => c.EmployeeId == employeeId &&
                            c.ClaimCategory == benefitCategory &&
                            approvedStatuses.Contains(c.ClaimStatus) &&
                            c.ExpenseDate.Year == now.Year &&
                            !c.IsDelete)
                .SumAsync(c => c.ApprovedAmount, cancellationToken);

            decimal remainingQuota = Math.Max(0m, annualLimit - usedAmount);
            decimal pctRemaining = annualLimit > 0 ? Math.Round((remainingQuota / annualLimit) * 100m, 2) : 0m;
            bool isWarningLow = annualLimit > 0 && remainingQuota < (annualLimit * 0.20m); // Peringatan < 20% (DED-W2)

            return new EmployeeRemainingQuotaDto
            {
                EmployeeId = employeeId,
                EmployeeName = emp.FullName,
                EmployeeNik = emp.EmployeeNumber,
                GradeLevelCode = grade,
                BenefitCategory = benefitCategory,
                AnnualLimitAmount = annualLimit,
                MinimumServiceMonths = minServiceMonths,
                CurrentServiceMonths = serviceMonths,
                IsEligibleByServiceMonths = isEligible,
                AccumulatedUsedAmount = usedAmount,
                RemainingQuotaAmount = remainingQuota,
                PercentageRemaining = pctRemaining,
                IsWarningLowQuota = isWarningLow,
                AllowEmergencyCitoCashier = allowCito
            };
        }

        public async Task<RestitutionClaimResponseDto> SubmitClaimAsync(
            SubmitRestitutionClaimRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Validasi tanggal kuitansi: tidak boleh di masa depan & tidak boleh > 60 hari lampau (VR-DED-09)
            if (request.ExpenseDate > now)
            {
                throw new ArgumentException("Tanggal kuitansi/nota tidak boleh berada di masa depan.");
            }

            var daysDifference = (now.Date - request.ExpenseDate.Date).TotalDays;
            if (daysDifference > 60)
            {
                throw new InvalidOperationException($"Pengajuan ditolak otomatis: Kuitansi bertanggal {request.ExpenseDate:yyyy-MM-dd} telah melebihi batas kedaluwarsa maksimal 60 hari kalender (VR-DED-09).");
            }

            // Evaluasi kuota tahunan pegawai (VR-DED-10)
            var quota = await GetEmployeeRemainingQuotaAsync(request.EmployeeId, request.ClaimCategory, cancellationToken);
            if (!quota.IsEligibleByServiceMonths)
            {
                throw new InvalidOperationException($"Pegawai belum memenuhi syarat minimal masa kerja {quota.MinimumServiceMonths} bulan untuk kategori {request.ClaimCategory} (masa kerja saat ini: {quota.CurrentServiceMonths} bulan).");
            }

            if (quota.RemainingQuotaAmount <= 0)
            {
                throw new InvalidOperationException($"Plafon tahunan pegawai untuk kategori {request.ClaimCategory} telah habis terpakai.");
            }

            // Pembangkitan nomor klaim unik
            string claimNumber = $"CLM-{now.Year}{now.Month:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var claim = new TrxEmployeeRestitutionClaim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = claimNumber,
                EmployeeId = request.EmployeeId,
                ClaimCategory = request.ClaimCategory,
                ExpenseDate = DateTime.SpecifyKind(request.ExpenseDate, DateTimeKind.Utc),
                ClaimAmount = request.ClaimAmount,
                ApprovedAmount = 0m,
                ReceiptAttachmentUrl = request.ReceiptAttachmentUrl,
                MedicalResumeAttachmentUrl = request.MedicalResumeAttachmentUrl,
                ClaimStatus = "Submitted",
                ReasonNotes = request.ReasonNotes,
                IsCitoEmergency = request.IsCitoEmergency,
                CreateDateTime = now,
                CreateBy = userId
            };

            await _dbContext.TrxEmployeeRestitutionClaims.AddAsync(claim, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Klaim restitusi {ClaimNo} berhasil diajukan oleh Pegawai {EmpId} sebesar Rp {Amount:N2}.",
                claimNumber, request.EmployeeId, request.ClaimAmount);

            return await GetClaimByIdAsync(claim.Id, cancellationToken) ?? throw new InvalidOperationException("Gagal membaca klaim yang baru disimpan.");
        }

        public async Task<List<RestitutionClaimResponseDto>> GetClaimsAsync(
            RestitutionClaimFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxEmployeeRestitutionClaims
                .AsNoTracking()
                .Include(c => c.Employee)
                .Include(c => c.Supervisor)
                .Include(c => c.HrReviewer)
                .Include(c => c.FinalApprover)
                .Where(c => !c.IsDelete);

            if (filter.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == filter.EmployeeId.Value);

            if (!string.IsNullOrWhiteSpace(filter.ClaimCategory))
                query = query.Where(c => c.ClaimCategory == filter.ClaimCategory);

            if (!string.IsNullOrWhiteSpace(filter.ClaimStatus))
                query = query.Where(c => c.ClaimStatus == filter.ClaimStatus);

            if (filter.IsCitoEmergency.HasValue)
                query = query.Where(c => c.IsCitoEmergency == filter.IsCitoEmergency.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.ExpenseDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.ExpenseDate <= filter.EndDate.Value);

            var list = await query
                .OrderByDescending(c => c.CreateDateTime)
                .ToListAsync(cancellationToken);

            return list.Select(MapToResponseDto).ToList();
        }

        public async Task<RestitutionClaimResponseDto?> GetClaimByIdAsync(
            Guid claimId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .AsNoTracking()
                .Include(c => c.Employee)
                .Include(c => c.Supervisor)
                .Include(c => c.HrReviewer)
                .Include(c => c.FinalApprover)
                .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDelete, cancellationToken);

            return claim != null ? MapToResponseDto(claim) : null;
        }

        public async Task<RestitutionClaimResponseDto> ApproveBySupervisorAsync(
            Guid claimId,
            SupervisorApprovalRequestDto request,
            Guid supervisorUserId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDelete, cancellationToken);

            if (claim == null) throw new KeyNotFoundException($"Klaim dengan ID {claimId} tidak ditemukan.");

            // Penegakan mutlak Anti-Self-Approval (DED-B8, VR-DED-08)
            if (claim.EmployeeId == supervisorUserId || claim.CreateBy == supervisorUserId)
            {
                throw new InvalidOperationException("Pelanggaran aturan Anti-Self-Approval: Atasan dilarang keras menyetujui klaim restitusi miliknya sendiri (DED-B8).");
            }

            if (claim.ClaimStatus != "Submitted")
            {
                throw new InvalidOperationException($"Klaim tidak dapat disetujui atasan karena berstatus {claim.ClaimStatus}.");
            }

            claim.SupervisorId = supervisorUserId;
            claim.SupervisorApprovedAt = DateTime.UtcNow;
            claim.ClaimStatus = "SupervisorApproved";
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = supervisorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Klaim {ClaimNo} disetujui Supervisor {User}.", claim.ClaimNumber, supervisorUserId);

            return await GetClaimByIdAsync(claim.Id, cancellationToken) ?? throw new InvalidOperationException();
        }

        public async Task<RestitutionClaimResponseDto> ReviewByHrAsync(
            Guid claimId,
            HrReviewRequestDto request,
            Guid hrReviewerUserId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDelete, cancellationToken);

            if (claim == null) throw new KeyNotFoundException($"Klaim dengan ID {claimId} tidak ditemukan.");

            // Penegakan Anti-Self-Approval (DED-B8)
            if (claim.EmployeeId == hrReviewerUserId || claim.CreateBy == hrReviewerUserId)
            {
                throw new InvalidOperationException("Pelanggaran Anti-Self-Approval: Tim HR dilarang mereview klaim restitusi miliknya sendiri (DED-B8).");
            }

            if (claim.ClaimStatus != "SupervisorApproved")
            {
                throw new InvalidOperationException($"Klaim tidak dapat direview HR karena berstatus {claim.ClaimStatus} (harus 'SupervisorApproved').");
            }

            claim.HrReviewerId = hrReviewerUserId;
            claim.HrReviewedAt = DateTime.UtcNow;
            claim.ClaimStatus = "HrReviewed";
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = hrReviewerUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Klaim {ClaimNo} direview oleh HR Reviewer {User}.", claim.ClaimNumber, hrReviewerUserId);

            return await GetClaimByIdAsync(claim.Id, cancellationToken) ?? throw new InvalidOperationException();
        }

        public async Task<RestitutionClaimResponseDto> FinalApproveAsync(
            Guid claimId,
            FinalApprovalRequestDto request,
            Guid finalApproverUserId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDelete, cancellationToken);

            if (claim == null) throw new KeyNotFoundException($"Klaim dengan ID {claimId} tidak ditemukan.");

            // Penegakan Anti-Self-Approval (DED-B8)
            if (claim.EmployeeId == finalApproverUserId || claim.CreateBy == finalApproverUserId)
            {
                throw new InvalidOperationException("Pelanggaran Anti-Self-Approval: Final Approver dilarang menyetujui klaim restitusi miliknya sendiri (DED-B8).");
            }

            if (claim.ClaimStatus != "HrReviewed")
            {
                throw new InvalidOperationException($"Klaim tidak dapat disahkan final karena berstatus {claim.ClaimStatus} (harus 'HrReviewed').");
            }

            var quota = await GetEmployeeRemainingQuotaAsync(claim.EmployeeId, claim.ClaimCategory, cancellationToken);
            decimal allowableAmount = Math.Min(request.ApprovedAmount, Math.Min(claim.ClaimAmount, quota.RemainingQuotaAmount));

            if (allowableAmount <= 0)
            {
                throw new InvalidOperationException("Nominal persetujuan final tidak valid atau melebihi sisa plafon tahunan pegawai.");
            }

            claim.FinalApproverId = finalApproverUserId;
            claim.FinalApprovedAt = DateTime.UtcNow;
            claim.ApprovedAmount = allowableAmount;
            claim.ClaimStatus = "Approved";
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = finalApproverUserId;

            // Jika klaim regular (bukan cito kasir), otomatis buat entitas TrxPayrollRestitution siap injeksi payroll
            if (!claim.IsCitoEmergency)
            {
                var restitution = new TrxPayrollRestitution
                {
                    Id = Guid.NewGuid(),
                    RestitutionNumber = $"RST-{DateTime.UtcNow.Year}{DateTime.UtcNow.Month:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                    EmployeeId = claim.EmployeeId,
                    ClaimId = claim.Id,
                    RestitutionType = "BenefitReimbursement",
                    Amount = allowableAmount,
                    Status = "Approved",
                    Justification = $"Reimbursement klaim {claim.ClaimCategory} no. {claim.ClaimNumber}",
                    IsTaxable = false, // Mutlak non-pajak PPh 21 (DED-B5, HRD-ASM-33)
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = finalApproverUserId
                };
                await _dbContext.TrxPayrollRestitutions.AddAsync(restitution, cancellationToken);
                claim.ClaimStatus = "QueuedForPayroll";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Klaim {ClaimNo} disetujui final sebesar Rp {Amount:N2} oleh {User}.",
                claim.ClaimNumber, allowableAmount, finalApproverUserId);

            return await GetClaimByIdAsync(claim.Id, cancellationToken) ?? throw new InvalidOperationException();
        }

        public async Task<RestitutionClaimResponseDto> RejectClaimAsync(
            Guid claimId,
            RejectClaimRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .FirstOrDefaultAsync(c => c.Id == claimId && !c.IsDelete, cancellationToken);

            if (claim == null) throw new KeyNotFoundException($"Klaim dengan ID {claimId} tidak ditemukan.");

            claim.ClaimStatus = "Rejected";
            claim.RejectionReason = request.RejectionReason;
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Klaim {ClaimNo} ditolak oleh {User} dengan alasan: {Reason}.",
                claim.ClaimNumber, userId, request.RejectionReason);

            return await GetClaimByIdAsync(claim.Id, cancellationToken) ?? throw new InvalidOperationException();
        }

        public async Task<PayrollRestitutionResponseDto> CreateSalaryCorrectionAsync(
            CreateSalaryCorrectionRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // Verifikasi bahwa slip lampau yang terimbas valid dan final (PAY-B6, DED-B4)
            var affectedRun = await _dbContext.PrlPayrollRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.AffectedPayrollRunId && !r.IsDelete, cancellationToken);

            if (affectedRun == null)
            {
                throw new KeyNotFoundException($"Payroll run lampau dengan ID {request.AffectedPayrollRunId} tidak ditemukan.");
            }

            string rstNumber = $"RST-CORR-{DateTime.UtcNow.Year}{DateTime.UtcNow.Month:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var restitution = new TrxPayrollRestitution
            {
                Id = Guid.NewGuid(),
                RestitutionNumber = rstNumber,
                EmployeeId = request.EmployeeId,
                ClaimId = null,
                RestitutionType = "SalaryAdjustmentCorrection",
                Amount = request.Amount,
                AffectedPayrollRunId = request.AffectedPayrollRunId,
                AffectedPayslipId = request.AffectedPayslipId,
                TargetPayrollRunId = request.TargetPayrollRunId,
                Status = "Approved",
                Justification = request.Justification,
                IsTaxable = false, // Mutlak non-pajak PPh 21 (DED-B5, HRD-ASM-33)
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.TrxPayrollRestitutions.AddAsync(restitution, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Restitusi koreksi salah potong {RstNo} berhasil dicatat untuk Pegawai {EmpId} sebesar Rp {Amount:N2} menautkan Run {RunId}.",
                rstNumber, request.EmployeeId, request.Amount, request.AffectedPayrollRunId);

            var emp = await _dbContext.MstEmployees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

            return new PayrollRestitutionResponseDto
            {
                Id = restitution.Id,
                RestitutionNumber = restitution.RestitutionNumber,
                EmployeeId = restitution.EmployeeId,
                EmployeeName = emp?.FullName,
                EmployeeNik = emp?.EmployeeNumber,
                RestitutionType = restitution.RestitutionType,
                Amount = restitution.Amount,
                AffectedPayrollRunId = restitution.AffectedPayrollRunId,
                AffectedPayslipId = restitution.AffectedPayslipId,
                TargetPayrollRunId = restitution.TargetPayrollRunId,
                Status = restitution.Status,
                Justification = restitution.Justification,
                IsTaxable = restitution.IsTaxable,
                CreatedAt = restitution.CreateDateTime
            };
        }

        public async Task<SalaryGrievanceInvestigationResultDto> InvestigateAndResolveSalaryGrievanceAsync(
            SalaryGrievanceInvestigationRequestDto request,
            Guid investigatorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDelete, cancellationToken);
            if (emp == null)
            {
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");
            }

            var affectedRun = await _dbContext.PrlPayrollRuns
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.AffectedPayrollRunId && !r.IsDelete, cancellationToken);
            if (affectedRun == null)
            {
                throw new KeyNotFoundException($"Payroll run lampau dengan ID {request.AffectedPayrollRunId} tidak ditemukan.");
            }

            if (request.ApprovedRefundAmount <= 0)
            {
                throw new InvalidOperationException("Nominal pengembalian dana yang disetujui harus lebih besar dari 0.");
            }

            if (request.ApprovedRefundAmount > request.ClaimedExcessDeductionAmount)
            {
                throw new InvalidOperationException($"Nominal pengembalian (Rp {request.ApprovedRefundAmount:N2}) tidak boleh melebihi nilai keluhan salah potong yang diajukan (Rp {request.ClaimedExcessDeductionAmount:N2}).");
            }

            string rstNumber = $"RST-GRV-{DateTime.UtcNow.Year}{DateTime.UtcNow.Month:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var restitution = new TrxPayrollRestitution
            {
                Id = Guid.NewGuid(),
                RestitutionNumber = rstNumber,
                EmployeeId = request.EmployeeId,
                ClaimId = null,
                RestitutionType = "SalaryAdjustmentCorrection",
                Amount = request.ApprovedRefundAmount,
                AffectedPayrollRunId = request.AffectedPayrollRunId,
                AffectedPayslipId = request.AffectedPayslipId,
                TargetPayrollRunId = request.TargetPayrollRunId,
                Status = "Approved",
                Justification = $"[Investigasi Keluhan HR Payroll] Telaah Officer {investigatorUserId}: {request.InvestigationFindings}. Klaim salah potong: Rp {request.ClaimedExcessDeductionAmount:N2}, Disetujui: Rp {request.ApprovedRefundAmount:N2}",
                IsTaxable = false, // Mutlak non-pajak PPh 21 (DED-B5, HRD-ASM-33, VR-DED-05)
                CreateDateTime = DateTime.UtcNow,
                CreateBy = investigatorUserId
            };

            await _dbContext.TrxPayrollRestitutions.AddAsync(restitution, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Investigasi keluhan salah potong diselesaikan. Restitusi {RestitutionNumber} diterbitkan untuk pegawai {EmployeeId} senilai Rp {Amount} (Non-pajak PPh 21)",
                rstNumber, request.EmployeeId, request.ApprovedRefundAmount);

            return new SalaryGrievanceInvestigationResultDto
            {
                RestitutionId = restitution.Id,
                RestitutionNumber = restitution.RestitutionNumber,
                EmployeeId = restitution.EmployeeId,
                EmployeeName = emp.FullName,
                EmployeeNik = emp.EmployeeNumber,
                AffectedPayrollRunId = request.AffectedPayrollRunId,
                AffectedPayslipId = request.AffectedPayslipId,
                ClaimedExcessDeductionAmount = request.ClaimedExcessDeductionAmount,
                ApprovedRefundAmount = restitution.Amount,
                IsTaxable = false,
                Status = restitution.Status,
                InvestigationFindings = request.InvestigationFindings,
                CreatedAt = restitution.CreateDateTime,
                Message = "Investigasi keluhan salah potong gaji berhasil diproses. Restitusi koreksi non-pajak PPh 21 telah diterbitkan dan disetujui untuk siklus payroll berikutnya."
            };
        }

        public async Task<List<PayrollRestitutionResponseDto>> GetPayrollRestitutionsAsync(
            Guid? employeeId,
            Guid? targetPayrollRunId,
            string? status,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxPayrollRestitutions
                .AsNoTracking()
                .Include(r => r.Employee)
                .Where(r => !r.IsDelete);

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            if (targetPayrollRunId.HasValue)
                query = query.Where(r => r.TargetPayrollRunId == targetPayrollRunId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            var list = await query
                .OrderByDescending(r => r.CreateDateTime)
                .ToListAsync(cancellationToken);

            return list.Select(r => new PayrollRestitutionResponseDto
            {
                Id = r.Id,
                RestitutionNumber = r.RestitutionNumber,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee?.FullName,
                EmployeeNik = r.Employee?.EmployeeNumber,
                ClaimId = r.ClaimId,
                RestitutionType = r.RestitutionType,
                Amount = r.Amount,
                AffectedPayrollRunId = r.AffectedPayrollRunId,
                AffectedPayslipId = r.AffectedPayslipId,
                TargetPayrollRunId = r.TargetPayrollRunId,
                Status = r.Status,
                Justification = r.Justification,
                IsTaxable = r.IsTaxable,
                CreatedAt = r.CreateDateTime
            }).ToList();
        }

        public async Task<InjectRestitutionToPayrollResultDto> InjectApprovedRestitutionsToPayrollAsync(
            Guid targetPayrollRunId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var targetRun = await _dbContext.PrlPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == targetPayrollRunId && !r.IsDelete, cancellationToken);

            if (targetRun == null)
            {
                throw new KeyNotFoundException($"Payroll run tujuan dengan ID {targetPayrollRunId} tidak ditemukan.");
            }

            // Ambil seluruh restitusi yang belum diinjeksi (Status 'Approved')
            var pendingRestitutions = await _dbContext.TrxPayrollRestitutions
                .Include(r => r.Employee)
                .Where(r => (r.TargetPayrollRunId == targetPayrollRunId || r.TargetPayrollRunId == null) &&
                            r.Status == "Approved" &&
                            !r.IsDelete)
                .ToListAsync(cancellationToken);

            int injectedCount = 0;
            decimal totalReimb = 0m;
            decimal totalCorr = 0m;
            var injectedDtos = new List<PayrollRestitutionResponseDto>();

            foreach (var rst in pendingRestitutions)
            {
                // Temukan PrlPayrollRunEmployee untuk pegawai ini pada run target
                var runEmp = await _dbContext.PrlPayrollRunEmployees
                    .FirstOrDefaultAsync(re => re.PayrollRunId == targetPayrollRunId &&
                                               (re.WorkforceProfileId == rst.EmployeeId || re.Id == rst.EmployeeId) &&
                                               !re.IsDelete, cancellationToken);

                if (runEmp == null)
                {
                    // Fallback: cari lewat mapping profil
                    var emp = await _dbContext.MstEmployees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == rst.EmployeeId, cancellationToken);
                    if (emp != null && emp.WorkforceProfileId != Guid.Empty)
                    {
                        runEmp = await _dbContext.PrlPayrollRunEmployees
                            .FirstOrDefaultAsync(re => re.PayrollRunId == targetPayrollRunId &&
                                                       re.WorkforceProfileId == emp.WorkforceProfileId &&
                                                       !re.IsDelete, cancellationToken);
                    }
                }

                if (runEmp != null)
                {
                    string compCode = rst.RestitutionType == "SalaryAdjustmentCorrection" ? "RST-CORR" : "RST-REIMB";
                    string compName = rst.RestitutionType == "SalaryAdjustmentCorrection"
                        ? "Restitusi Koreksi Salah Potong Periode Lampau"
                        : "Restitusi Reimbursement Klaim Kesehatan ESS";

                    var component = new PrlPayrollEmployeeComponent
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunEmployeeId = runEmp.Id,
                        SourceId = rst.Id,
                        SourceType = "Restitution",
                        ComponentCodeSnapshot = compCode,
                        ComponentNameSnapshot = compName,
                        ComponentTypeSnapshot = "Earning",
                        CalculationMethodSnapshot = "Fixed",
                        CurrencyCode = "IDR",
                        Quantity = 1m,
                        Rate = rst.Amount,
                        BaseAmount = rst.Amount,
                        Amount = rst.Amount,
                        TaxableAmount = 0m,
                        IsTaxable = false, // Mutlak non-pajak PPh 21 (DED-B5, HRD-ASM-33)
                        IsDisplayedOnPayslip = true,
                        Notes = rst.Justification ?? compName,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = userId
                    };

                    await _dbContext.PrlPayrollEmployeeComponents.AddAsync(component, cancellationToken);

                    // Tambahkan ke penghasilan kotor / net pay pegawai
                    runEmp.GrossPay += rst.Amount;
                    runEmp.NetPay += rst.Amount;
                    runEmp.UpdateDateTime = DateTime.UtcNow;
                    runEmp.UpdateBy = userId;
                }

                rst.Status = "InjectedToPayroll";
                rst.TargetPayrollRunId = targetPayrollRunId;
                rst.UpdateDateTime = DateTime.UtcNow;
                rst.UpdateBy = userId;

                if (rst.ClaimId.HasValue)
                {
                    var claim = await _dbContext.TrxEmployeeRestitutionClaims
                        .FirstOrDefaultAsync(c => c.Id == rst.ClaimId.Value, cancellationToken);
                    if (claim != null)
                    {
                        claim.ClaimStatus = "ProcessedInPayroll";
                        claim.UpdateDateTime = DateTime.UtcNow;
                        claim.UpdateBy = userId;
                    }
                }

                if (rst.RestitutionType == "SalaryAdjustmentCorrection")
                    totalCorr += rst.Amount;
                else
                    totalReimb += rst.Amount;

                injectedCount++;

                injectedDtos.Add(new PayrollRestitutionResponseDto
                {
                    Id = rst.Id,
                    RestitutionNumber = rst.RestitutionNumber,
                    EmployeeId = rst.EmployeeId,
                    EmployeeName = rst.Employee?.FullName,
                    EmployeeNik = rst.Employee?.EmployeeNumber,
                    ClaimId = rst.ClaimId,
                    RestitutionType = rst.RestitutionType,
                    Amount = rst.Amount,
                    AffectedPayrollRunId = rst.AffectedPayrollRunId,
                    AffectedPayslipId = rst.AffectedPayslipId,
                    TargetPayrollRunId = targetPayrollRunId,
                    Status = rst.Status,
                    Justification = rst.Justification,
                    IsTaxable = rst.IsTaxable,
                    CreatedAt = rst.CreateDateTime
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Berhasil menginjeksi {Count} komponen restitusi ke Payroll Run {RunId}: Total Reimbursement: Rp {Reimb:N2}, Total Koreksi: Rp {Corr:N2}.",
                injectedCount, targetPayrollRunId, totalReimb, totalCorr);

            return new InjectRestitutionToPayrollResultDto
            {
                PayrollRunId = targetPayrollRunId,
                TotalRestitutionsInjected = injectedCount,
                TotalReimbursementAmount = totalReimb,
                TotalCorrectionAmount = totalCorr,
                TotalNonTaxableInjected = totalReimb + totalCorr,
                InjectedItems = injectedDtos,
                Message = $"Berhasil menginjeksi {injectedCount} komponen restitusi non-pajak ke payroll run."
            };
        }

        private static RestitutionClaimResponseDto MapToResponseDto(TrxEmployeeRestitutionClaim c)
        {
            return new RestitutionClaimResponseDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.Employee?.FullName,
                EmployeeNik = c.Employee?.EmployeeNumber,
                ClaimCategory = c.ClaimCategory,
                ExpenseDate = c.ExpenseDate,
                ClaimAmount = c.ClaimAmount,
                ApprovedAmount = c.ApprovedAmount,
                ReceiptAttachmentUrl = c.ReceiptAttachmentUrl,
                MedicalResumeAttachmentUrl = c.MedicalResumeAttachmentUrl,
                ClaimStatus = c.ClaimStatus,
                ReasonNotes = c.ReasonNotes,
                RejectionReason = c.RejectionReason,
                SupervisorId = c.SupervisorId,
                SupervisorName = c.Supervisor?.FullName,
                SupervisorApprovedAt = c.SupervisorApprovedAt,
                HrReviewerId = c.HrReviewerId,
                HrReviewerName = c.HrReviewer?.UserName,
                HrReviewedAt = c.HrReviewedAt,
                FinalApproverId = c.FinalApproverId,
                FinalApproverName = c.FinalApprover?.UserName,
                FinalApprovedAt = c.FinalApprovedAt,
                IsCitoEmergency = c.IsCitoEmergency,
                CreatedAt = c.CreateDateTime
            };
        }
    }
}
