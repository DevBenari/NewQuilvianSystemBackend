using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class WfpContractHistoryWriteService : IWfpContractHistoryWriteService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmployeeEmploymentRecordService _employeeEmploymentRecordService;

        public WfpContractHistoryWriteService(
            ApplicationDbContext dbContext,
            IEmployeeEmploymentRecordService employeeEmploymentRecordService)
        {
            _dbContext = dbContext;
            _employeeEmploymentRecordService = employeeEmploymentRecordService;
        }

        public async Task<WfpContractHistory> CreateInitialContractAsync(
            WfpContractHistory contract,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            contract.Id = contract.Id == Guid.Empty ? Guid.NewGuid() : contract.Id;
            contract.HistoryType = "Initial";
            contract.RowVersion = Guid.NewGuid();
            contract.CreateDateTime = DateTime.UtcNow;
            contract.CreateBy = userId;

            await _dbContext.WfpContractHistories.AddAsync(contract, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (contract.ContractStatus == "Active")
            {
                await _employeeEmploymentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);
            }

            return contract;
        }

        public async Task<WfpContractHistory> CreateRenewalContractAsync(
            WfpContractHistory contract,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            contract.Id = contract.Id == Guid.Empty ? Guid.NewGuid() : contract.Id;
            contract.HistoryType = "Renewal";
            contract.RowVersion = Guid.NewGuid();
            contract.CreateDateTime = DateTime.UtcNow;
            contract.CreateBy = userId;

            await _dbContext.WfpContractHistories.AddAsync(contract, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (contract.ContractStatus == "Active")
            {
                await _employeeEmploymentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);
            }

            return contract;
        }

        public async Task<WfpContractHistory> CreateAmendmentContractAsync(
            WfpContractHistory contract,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));

            contract.Id = contract.Id == Guid.Empty ? Guid.NewGuid() : contract.Id;
            contract.HistoryType = "Amendment";
            contract.RowVersion = Guid.NewGuid();
            contract.CreateDateTime = DateTime.UtcNow;
            contract.CreateBy = userId;

            await _dbContext.WfpContractHistories.AddAsync(contract, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (contract.ContractStatus == "Active")
            {
                await _employeeEmploymentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);
            }

            return contract;
        }

        public async Task<WfpContractHistory> ApplyCorrectionAsync(
            Guid contractHistoryId,
            string newContractNumber,
            Guid newContractTypeId,
            DateTime newStartDate,
            DateTime? newEndDate,
            string? newDocumentPath,
            string? newDocumentFileName,
            string? newDocumentContentType,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.Id == contractHistoryId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                throw new InvalidOperationException($"Kontrak dengan ID {contractHistoryId} tidak ditemukan.");
            }

            contract.ContractNumber = newContractNumber;
            contract.ContractTypeId = newContractTypeId;
            contract.StartDate = newStartDate;
            contract.EndDate = newEndDate;

            if (!string.IsNullOrWhiteSpace(newDocumentPath))
            {
                contract.DocumentPath = newDocumentPath;
                contract.DocumentFileName = newDocumentFileName;
                contract.DocumentContentType = newDocumentContentType;
            }

            contract.RowVersion = Guid.NewGuid();
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (contract.ContractStatus == "Active" && contract.IsCurrent)
            {
                await _employeeEmploymentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);
            }

            return contract;
        }

        public async Task<WfpContractHistory> ActivateContractAsync(
            Guid contractHistoryId,
            DateTime signedDate,
            string documentPath,
            string documentFileName,
            string documentContentType,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.Id == contractHistoryId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                throw new InvalidOperationException($"Kontrak dengan ID {contractHistoryId} tidak ditemukan.");
            }

            // Nonaktifkan IsCurrent pada kontrak sebelumnya bila ada
            var previousCurrentContracts = await _dbContext.WfpContractHistories
                .Where(c => c.WorkforceProfileId == contract.WorkforceProfileId && c.Id != contract.Id && c.IsCurrent && !c.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var prev in previousCurrentContracts)
            {
                prev.IsCurrent = false;
                if (prev.ContractStatus == "Active")
                {
                    prev.ContractStatus = "Ended";
                }
                prev.UpdateDateTime = DateTime.UtcNow;
                prev.UpdateBy = userId;
                prev.RowVersion = Guid.NewGuid();
            }

            contract.ContractStatus = "Active";
            contract.IsCurrent = true;
            contract.SignedDate = signedDate;
            contract.SignedRecordedAt = DateTime.UtcNow;
            contract.SignedRecordedByUserId = userId;
            contract.DocumentPath = documentPath;
            contract.DocumentFileName = documentFileName;
            contract.DocumentContentType = documentContentType;
            contract.RowVersion = Guid.NewGuid();
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _employeeEmploymentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);

            return contract;
        }

        public async Task<WfpContractHistory> CloseContractAsync(
            Guid contractHistoryId,
            string terminalStatus,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.Id == contractHistoryId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                throw new InvalidOperationException($"Kontrak dengan ID {contractHistoryId} tidak ditemukan.");
            }

            contract.ContractStatus = terminalStatus;
            contract.IsCurrent = false;
            contract.RowVersion = Guid.NewGuid();
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return contract;
        }
    }
}
