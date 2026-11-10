using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdEmploymentContractService
    {
        Task<WfpContractHistory> CreateDraftFromOfferAsync(
            Guid workforceProfileId,
            HrdJobOffer offer,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<PagedResult<EmploymentContractResponse>>> GetPagedAsync(
            EmploymentContractQuery query,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<EmploymentContractResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<EmploymentContractPrintDataResponse>> GetPrintDataAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<EmploymentContractResponse>> RecordSignedAsync(
            Guid id,
            RecordContractSignedRequest request,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadScanAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        // --- BE-HRD-080: Keputusan Kontrak T16 & Batas 5 Tahun PKWT ---

        Task<RecruitmentResult<FixedTermCheckResponse>> FixedTermCheckAsync(
            Guid contractId,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ContractDecisionResponse>> SubmitDecisionAsync(
            Guid contractId,
            SubmitContractDecisionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ContractDecisionResponse>> GetDecisionByIdAsync(
            Guid contractId,
            Guid decisionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ContractDecisionResponse>> UpdateDecisionAsync(
            Guid contractId,
            Guid decisionId,
            SubmitContractDecisionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> WithdrawDecisionAsync(
            Guid contractId,
            Guid decisionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ContractDecisionResponse>> RecordEmployeeNotifiedAsync(
            Guid contractId,
            Guid decisionId,
            RecordEmployeeNotifiedRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> ApplyDecisionAsync(
            Guid decisionId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default);

        Task<WfpContractHistory> CreateFollowUpDraftAsync(
            Guid parentContractId,
            ContractDecisionType decisionType,
            DateOnly startDate,
            DateOnly? endDate,
            Guid actorUserId,
            CancellationToken cancellationToken = default);

        // --- BE-HRD-081: Koreksi Kontrak T17 ---

        Task<RecruitmentResult<ContractCorrectionResponse>> SubmitCorrectionAsync(
            Guid contractId,
            SubmitContractCorrectionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<ContractCorrectionResponse>> GetCorrectionByIdAsync(
            Guid contractId,
            Guid correctionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> WithdrawCorrectionAsync(
            Guid contractId,
            Guid correctionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<RecruitmentResult<bool>> ApplyCorrectionAsync(
            Guid correctionId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default);

        // --- Scan Susulan Kontrak Data Awal (VR-ONB-45) ---

        Task<RecruitmentResult<EmploymentContractResponse>> UploadLegacyDocumentAsync(
            Guid contractId,
            UploadLegacyContractDocumentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
