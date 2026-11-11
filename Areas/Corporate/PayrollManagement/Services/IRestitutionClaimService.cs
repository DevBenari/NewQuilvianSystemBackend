using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan siklus hidup klaim restitusi ESS, kuota plafon tahunan, alur persetujuan 3-tier anti-self-approval, kompensasi salah potong masa lalu, dan injeksi payroll non-pajak (BE-HRD-166 s.d. BE-HRD-170, HRD-DEC-130, HRD-DEC-131).
    /// </summary>
    public interface IRestitutionClaimService
    {
        Task<EmployeeRemainingQuotaDto> GetEmployeeRemainingQuotaAsync(Guid employeeId, string benefitCategory, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto> SubmitClaimAsync(SubmitRestitutionClaimRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<List<RestitutionClaimResponseDto>> GetClaimsAsync(RestitutionClaimFilterDto filter, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto?> GetClaimByIdAsync(Guid claimId, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto> ApproveBySupervisorAsync(Guid claimId, SupervisorApprovalRequestDto request, Guid supervisorUserId, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto> ReviewByHrAsync(Guid claimId, HrReviewRequestDto request, Guid hrReviewerUserId, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto> FinalApproveAsync(Guid claimId, FinalApprovalRequestDto request, Guid finalApproverUserId, CancellationToken cancellationToken = default);
        Task<RestitutionClaimResponseDto> RejectClaimAsync(Guid claimId, RejectClaimRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<PayrollRestitutionResponseDto> CreateSalaryCorrectionAsync(CreateSalaryCorrectionRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<SalaryGrievanceInvestigationResultDto> InvestigateAndResolveSalaryGrievanceAsync(SalaryGrievanceInvestigationRequestDto request, Guid investigatorUserId, CancellationToken cancellationToken = default);
        Task<List<PayrollRestitutionResponseDto>> GetPayrollRestitutionsAsync(Guid? employeeId, Guid? targetPayrollRunId, string? status, CancellationToken cancellationToken = default);
        Task<InjectRestitutionToPayrollResultDto> InjectApprovedRestitutionsToPayrollAsync(Guid targetPayrollRunId, Guid userId, CancellationToken cancellationToken = default);
    }
}
