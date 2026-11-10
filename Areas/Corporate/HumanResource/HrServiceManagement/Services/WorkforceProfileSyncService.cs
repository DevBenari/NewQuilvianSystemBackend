using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class WorkforceProfileSyncService : IWorkforceProfileSyncService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<WorkforceProfileSyncService> _logger;

        public WorkforceProfileSyncService(
            ApplicationDbContext dbContext,
            ILogger<WorkforceProfileSyncService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ProfileSyncResultDto> ApplyApprovedProfileChangesAsync(
            Guid hrServiceRequestId,
            Guid approvedByUserId,
            CancellationToken cancellationToken = default)
        {
            var ticket = await _dbContext.TrxHrServiceRequests
                .Include(x => x.HrServiceType)
                .Include(x => x.HrServiceCategory)
                .FirstOrDefaultAsync(x => x.Id == hrServiceRequestId && !x.IsDelete, cancellationToken);

            if (ticket == null)
            {
                return new ProfileSyncResultDto
                {
                    IsSuccess = false,
                    Message = $"Tiket HR Service ID {hrServiceRequestId} tidak ditemukan."
                };
            }

            if (string.IsNullOrWhiteSpace(ticket.RequestPayloadJson) || ticket.RequestPayloadJson == "{}")
            {
                return new ProfileSyncResultDto
                {
                    IsSuccess = true,
                    Message = "Tidak ada payload perubahan profil (RequestPayloadJson kosong), pemutakhiran dilewati."
                };
            }

            var targetProfileId = ticket.RequestedByWorkforceProfileId;
            var targetProfile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(x => x.Id == targetProfileId && !x.IsDelete, cancellationToken);

            if (targetProfile == null)
            {
                return new ProfileSyncResultDto
                {
                    IsSuccess = false,
                    Message = $"Profil tenaga kerja dengan ID {targetProfileId} tidak ditemukan."
                };
            }

            var now = DateTime.UtcNow;
            string componentModified = "GeneralProfile";
            string oldValuesJson = "{}";
            string newValuesJson = ticket.RequestPayloadJson;

            // Eksekusi atomik database
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                using var jsonDoc = JsonDocument.Parse(ticket.RequestPayloadJson);
                var root = jsonDoc.RootElement;

                // 1. Kasus Perubahan Rekening Bank Payroll
                if (root.TryGetProperty("AccountNumber", out var accNumProp) || root.TryGetProperty("accountNumber", out accNumProp))
                {
                    componentModified = "BankAccount";
                    var bankName = GetStringProperty(root, "BankName", "bankName") ?? "Bank";
                    var accNumber = accNumProp.GetString() ?? string.Empty;
                    var accHolder = GetStringProperty(root, "AccountHolderName", "accountHolderName") ?? targetProfile.DisplayName;
                    var branch = GetStringProperty(root, "BankBranch", "bankBranch");
                    var accType = GetStringProperty(root, "AccountType", "accountType") ?? "Savings";

                    var existingBank = await _dbContext.WfpBankAccounts
                        .Where(x => x.WorkforceProfileId == targetProfileId && !x.IsDelete && x.IsPayrollAccount)
                        .OrderByDescending(x => x.IsPrimary)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existingBank != null)
                    {
                        var oldData = new
                        {
                            existingBank.Id,
                            existingBank.BankName,
                            existingBank.AccountNumber,
                            existingBank.AccountHolderName,
                            existingBank.BankBranch,
                            existingBank.IsPrimary,
                            existingBank.IsVerified
                        };
                        oldValuesJson = JsonSerializer.Serialize(oldData);

                        // Perbarui data rekening bank payroll
                        existingBank.BankName = bankName;
                        existingBank.AccountNumber = accNumber;
                        existingBank.AccountHolderName = accHolder;
                        existingBank.BankBranch = branch;
                        existingBank.AccountType = accType;
                        existingBank.IsVerified = true;
                        existingBank.VerifiedAt = now;
                        existingBank.VerifiedByUserId = approvedByUserId;
                        existingBank.EffectiveStartDate = now;
                        existingBank.UpdateBy = approvedByUserId;
                        existingBank.UpdateDateTime = now;
                    }
                    else
                    {
                        oldValuesJson = "{\"status\": \"NoPreviousBankAccount\"}";

                        var newBankAccount = new WfpBankAccount
                        {
                            Id = Guid.NewGuid(),
                            WorkforceProfileId = targetProfileId,
                            BankName = bankName,
                            AccountNumber = accNumber,
                            AccountHolderName = accHolder,
                            BankBranch = branch,
                            AccountType = accType,
                            CurrencyCode = "IDR",
                            IsPayrollAccount = true,
                            IsPrimary = true,
                            IsVerified = true,
                            VerifiedAt = now,
                            VerifiedByUserId = approvedByUserId,
                            EffectiveStartDate = now,
                            CreateBy = approvedByUserId,
                            CreateDateTime = now
                        };
                        _dbContext.WfpBankAccounts.Add(newBankAccount);
                    }
                }
                // 2. Kasus Perubahan Status Pernikahan / Data Personal Karyawan
                else if (root.TryGetProperty("MaritalStatus", out var maritalProp) || root.TryGetProperty("maritalStatus", out maritalProp)
                    || root.TryGetProperty("PhoneNumber", out _) || root.TryGetProperty("phoneNumber", out _))
                {
                    componentModified = "PersonalData";
                    var employee = await _dbContext.MstEmployees
                        .FirstOrDefaultAsync(x => x.WorkforceProfileId == targetProfileId && !x.IsDelete, cancellationToken);

                    if (employee != null)
                    {
                        var oldData = new
                        {
                            MaritalStatus = employee.MaritalStatus.ToString(),
                            employee.PhoneNumber,
                            employee.WhatsAppNumber
                        };
                        oldValuesJson = JsonSerializer.Serialize(oldData);

                        if (maritalProp.ValueKind == JsonValueKind.String)
                        {
                            var maritalStr = maritalProp.GetString();
                            if (Enum.TryParse<MaritalStatus>(maritalStr, true, out var parsedStatus))
                            {
                                employee.MaritalStatus = parsedStatus;
                            }
                        }

                        var phone = GetStringProperty(root, "PhoneNumber", "phoneNumber");
                        if (!string.IsNullOrWhiteSpace(phone))
                        {
                            employee.PhoneNumber = phone;
                            targetProfile.PhoneNumber = phone;
                        }

                        var wa = GetStringProperty(root, "WhatsAppNumber", "whatsAppNumber");
                        if (!string.IsNullOrWhiteSpace(wa))
                        {
                            employee.WhatsAppNumber = wa;
                            targetProfile.WhatsAppNumber = wa;
                        }

                        employee.UpdateBy = approvedByUserId;
                        employee.UpdateDateTime = now;
                        targetProfile.UpdateBy = approvedByUserId;
                        targetProfile.UpdateDateTime = now;
                    }
                }
                else
                {
                    componentModified = "GeneralInformation";
                    oldValuesJson = "{\"status\": \"UpdatedFromServiceRequest\"}";
                }

                // Catat rekam jejak resmi ke HrdProfileChangeHistory
                var historyRecord = new HrdProfileChangeHistory
                {
                    Id = Guid.NewGuid(),
                    HrServiceRequestId = ticket.Id,
                    EmployeeId = targetProfileId,
                    ComponentModified = componentModified,
                    OldValuesJson = oldValuesJson,
                    NewValuesJson = newValuesJson,
                    ApprovedByUserId = approvedByUserId,
                    AppliedAtUtc = now,
                    IsActive = true,
                    CreateBy = approvedByUserId,
                    CreateDateTime = now
                };

                _dbContext.HrdProfileChangeHistories.Add(historyRecord);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("[PROFILE-SYNC-INT-65] Berhasil menyinkronkan profil tenaga kerja {ProfileId} untuk komponen {Component} dari tiket {TicketNumber}",
                    targetProfileId, componentModified, ticket.RequestNumber);

                return new ProfileSyncResultDto
                {
                    IsSuccess = true,
                    Message = $"Profil {componentModified} berhasil dimutakhirkan secara atomik berdasarkan persetujuan tiket {ticket.RequestNumber}.",
                    ProfileChangeHistoryId = historyRecord.Id,
                    ComponentModified = componentModified,
                    OldValuesJson = oldValuesJson,
                    NewValuesJson = newValuesJson,
                    AppliedAtUtc = now
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "[PROFILE-SYNC-ERROR] Gagal memutakhirkan profil tenaga kerja {ProfileId} dari tiket {TicketId}",
                    targetProfileId, hrServiceRequestId);
                throw;
            }
        }

        public async Task<List<ProfileChangeHistoryItemDto>> GetChangeHistoryByEmployeeAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.HrdProfileChangeHistories
                .Include(x => x.HrServiceRequest)
                .Include(x => x.Employee)
                .Include(x => x.ApprovedByUser)
                .Where(x => x.EmployeeId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.AppliedAtUtc)
                .Select(x => new ProfileChangeHistoryItemDto
                {
                    Id = x.Id,
                    HrServiceRequestId = x.HrServiceRequestId,
                    RequestNumber = x.HrServiceRequest != null ? x.HrServiceRequest.RequestNumber : string.Empty,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.DisplayName : string.Empty,
                    ComponentModified = x.ComponentModified,
                    OldValuesJson = x.OldValuesJson,
                    NewValuesJson = x.NewValuesJson,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.UserName : string.Empty,
                    AppliedAtUtc = x.AppliedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProfileChangeHistoryItemDto>> GetChangeHistoryByTicketAsync(
            Guid hrServiceRequestId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.HrdProfileChangeHistories
                .Include(x => x.HrServiceRequest)
                .Include(x => x.Employee)
                .Include(x => x.ApprovedByUser)
                .Where(x => x.HrServiceRequestId == hrServiceRequestId && !x.IsDelete)
                .OrderByDescending(x => x.AppliedAtUtc)
                .Select(x => new ProfileChangeHistoryItemDto
                {
                    Id = x.Id,
                    HrServiceRequestId = x.HrServiceRequestId,
                    RequestNumber = x.HrServiceRequest != null ? x.HrServiceRequest.RequestNumber : string.Empty,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee != null ? x.Employee.DisplayName : string.Empty,
                    ComponentModified = x.ComponentModified,
                    OldValuesJson = x.OldValuesJson,
                    NewValuesJson = x.NewValuesJson,
                    ApprovedByUserId = x.ApprovedByUserId,
                    ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.UserName : string.Empty,
                    AppliedAtUtc = x.AppliedAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        private static string? GetStringProperty(JsonElement root, string prop1, string prop2)
        {
            if (root.TryGetProperty(prop1, out var p1) && p1.ValueKind == JsonValueKind.String)
                return p1.GetString();
            if (root.TryGetProperty(prop2, out var p2) && p2.ValueKind == JsonValueKind.String)
                return p2.GetString();
            return null;
        }
    }
}
