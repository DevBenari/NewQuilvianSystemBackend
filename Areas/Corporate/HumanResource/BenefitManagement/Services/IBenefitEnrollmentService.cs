using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    public interface IBenefitEnrollmentService
    {
        // Administrasi Kepesertaan Asuransi Pegawai RS
        Task<PagedResult<EmployeeEnrollmentListItemDto>> GetEnrollmentsPagedAsync(EmployeeEnrollmentListQuery query, CancellationToken cancellationToken);
        Task<BenefitEnrollmentDetailDto> GetEnrollmentDetailAsync(Guid id, CancellationToken cancellationToken);
        Task<BenefitEnrollmentDetailDto> ActivateProbationEnrollmentAsync(Guid id, ActivateProbationRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<BenefitEnrollmentDetailDto> UpdatePolicyNumbersAsync(Guid id, UpdatePolicyNumbersRequestDto request, Guid actorUserId, CancellationToken cancellationToken);

        // Meja Kerja Verifikasi Berkas Fisik Kependudukan HR
        Task<PagedResult<PendingDependentVerificationDto>> GetPendingDependentVerificationsAsync(PendingDependentVerificationQuery query, CancellationToken cancellationToken);
        Task<BenefitDependentDetailDto> VerifyDependentAsync(Guid dependentId, VerifyDependentRequestDto request, Guid actorUserId, CancellationToken cancellationToken);

        // Portal ESS Mandiri Staf (Keluarga & Kuota)
        Task<List<EmployeeDependentSummaryDto>> GetMyFamilyDependentsAsync(Guid workforceProfileId, CancellationToken cancellationToken);
        Task<EmployeeFamilyQuotaSummaryDto> GetMyFamilyQuotaSummaryAsync(Guid workforceProfileId, CancellationToken cancellationToken);
        Task<EmployeeDependentSummaryDto> CreateEssDependentAsync(Guid workforceProfileId, CreateEssDependentRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
    }
}
