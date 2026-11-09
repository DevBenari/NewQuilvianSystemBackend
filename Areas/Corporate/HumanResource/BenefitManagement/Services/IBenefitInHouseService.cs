using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    public interface IBenefitInHouseService
    {
        Task<InHouseCoverageInquiryDto> InquireCoverageAsync(InHouseCoverageInquiryQuery query, CancellationToken cancellationToken);
        Task<BenefitBalanceDeductionResultDto> DeductBalanceAsync(DeductInHouseBalanceRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
        Task<List<BenefitBalanceMovementDto>> GetLedgerMovementsAsync(Guid employeeId, int? year, CancellationToken cancellationToken);
        Task<BenefitBalanceMovementDto> AdjustLedgerBalanceAsync(AdjustBalanceManualRequestDto request, Guid actorUserId, CancellationToken cancellationToken);
    }
}
