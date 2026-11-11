using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IWfpContractHistoryWriteService
    {
        Task<WfpContractHistory> CreateInitialContractAsync(WfpContractHistory contract, Guid userId, CancellationToken cancellationToken = default);
        Task<WfpContractHistory> CreateRenewalContractAsync(WfpContractHistory contract, Guid userId, CancellationToken cancellationToken = default);
        Task<WfpContractHistory> CreateAmendmentContractAsync(WfpContractHistory contract, Guid userId, CancellationToken cancellationToken = default);
        Task<WfpContractHistory> ApplyCorrectionAsync(
            Guid contractHistoryId,
            string newContractNumber,
            Guid newContractTypeId,
            DateTime newStartDate,
            DateTime? newEndDate,
            string? newDocumentPath,
            string? newDocumentFileName,
            string? newDocumentContentType,
            Guid userId,
            CancellationToken cancellationToken = default);
        Task<WfpContractHistory> ActivateContractAsync(
            Guid contractHistoryId,
            DateTime signedDate,
            string documentPath,
            string documentFileName,
            string documentContentType,
            Guid userId,
            CancellationToken cancellationToken = default);
        Task<WfpContractHistory> CloseContractAsync(
            Guid contractHistoryId,
            string terminalStatus,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
