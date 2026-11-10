using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IApprovalDelegationV2Service
    {
        Task<ApprovalDelegationV2ResponseDto> CreateDelegationAsync(Guid principalEmployeeId, CreateApprovalDelegationRequestDto request);
        Task<ApprovalDelegationV2ResponseDto> RevokeDelegationAsync(Guid delegationId, RevokeDelegationRequestDto request, Guid currentUserId);
        Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalDelegationV2ResponseDto>> GetDelegationsAsync(bool? isActive, int pageNumber, int pageSize);
        Task<List<ApprovalDelegationV2ResponseDto>> GetMyActingRolesAsync(Guid delegateEmployeeId);
    }

    public class ApprovalDelegationV2Service : IApprovalDelegationV2Service
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberSeriesAllocator _numberAllocator;

        public ApprovalDelegationV2Service(ApplicationDbContext context, NumberSeriesAllocator numberAllocator)
        {
            _context = context;
            _numberAllocator = numberAllocator;
        }

        public async Task<ApprovalDelegationV2ResponseDto> CreateDelegationAsync(Guid principalEmployeeId, CreateApprovalDelegationRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi VR-WF-03: Rentang tanggal aktif harus sah
            if (request.EndDate < request.StartDate)
            {
                throw new InvalidOperationException("Tanggal selesai wewenang Plh tidak boleh lebih awal dari tanggal mulai (VR-WF-03)");
            }

            if (request.DelegateEmployeeId == principalEmployeeId)
            {
                throw new InvalidOperationException("Pendelegasian wewenang Plh ke diri sendiri dilarang mutlak (HRD-DEC-184)");
            }

            var principal = await _context.MstEmployees.FindAsync(principalEmployeeId);
            var delegateEmp = await _context.MstEmployees.FindAsync(request.DelegateEmployeeId);

            if (principal == null || delegateEmp == null)
            {
                throw new InvalidOperationException("Pejabat pemberi atau penerima wewenang Plh tidak ditemukan di sistem");
            }

            // Nomor SK Plh: urut harian dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: SK-PLH-20261110-0001.
            // Kontrak pembuatan delegasi hanya membawa ID pegawai pemberi wewenang, sehingga stempel aktor deret memakai aktor sistem.
            var delegationNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: WorkflowNumberSeries.DelegationKey,
                Prefix: WorkflowNumberSeries.DelegationPrefix,
                ResetPolicy: WorkflowNumberSeries.DelegationResetPolicy,
                SequenceDigits: WorkflowNumberSeries.DelegationDigits,
                ActorUserId: WorkflowNumberSeries.SystemFallbackUserId,
                Instant: DateTimeOffset.UtcNow));

            var delegation = new TrxApprovalDelegation
            {
                Id = Guid.NewGuid(),
                DelegationNumber = delegationNumber,
                DelegatorUserId = principal.WorkforceProfileId, // User proxy
                DelegateUserId = delegateEmp.WorkforceProfileId,
                PrincipalEmployeeId = principalEmployeeId,
                DelegateEmployeeId = request.DelegateEmployeeId,
                DelegatedRequestTypes = string.IsNullOrWhiteSpace(request.DelegatedRequestTypes) ? "ALL" : request.DelegatedRequestTypes,
                DelegationReason = request.DelegationReason,
                EffectiveStartAt = request.StartDate,
                EffectiveEndAt = request.EndDate,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DelegationStatus = "Active",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxApprovalDelegations.Add(delegation);
            await _context.SaveChangesAsync();

            return new ApprovalDelegationV2ResponseDto
            {
                Id = delegation.Id,
                DelegationNumber = delegation.DelegationNumber,
                PrincipalEmployeeId = principal.Id,
                PrincipalName = principal.FullName,
                PrincipalPosition = "Pejabat Definitif",
                DelegateEmployeeId = delegateEmp.Id,
                DelegateName = delegateEmp.FullName,
                DelegatePosition = "Pejabat Pelaksana Harian (Plh)",
                StartDate = delegation.EffectiveStartAt,
                EndDate = delegation.EffectiveEndAt,
                DelegatedRequestTypes = delegation.DelegatedRequestTypes,
                DelegationReason = delegation.DelegationReason ?? string.Empty,
                DelegationStatus = delegation.DelegationStatus,
                IsActive = delegation.IsActive,
                CreatedAt = delegation.CreateDateTime
            };
        }

        public async Task<ApprovalDelegationV2ResponseDto> RevokeDelegationAsync(Guid delegationId, RevokeDelegationRequestDto request, Guid currentUserId)
        {
            var delegation = await _context.TrxApprovalDelegations.FindAsync(delegationId);
            if (delegation == null) throw new KeyNotFoundException("Surat kuasa delegasi Plh tidak ditemukan");

            delegation.IsActive = false;
            delegation.DelegationStatus = "Revoked";
            delegation.RevokedAt = DateTime.UtcNow;
            delegation.RevocationReason = request.RevokeReason;

            await _context.SaveChangesAsync();

            var principal = await _context.MstEmployees.FindAsync(delegation.PrincipalEmployeeId ?? Guid.Empty);
            var delegateEmp = await _context.MstEmployees.FindAsync(delegation.DelegateEmployeeId ?? Guid.Empty);

            return new ApprovalDelegationV2ResponseDto
            {
                Id = delegation.Id,
                DelegationNumber = delegation.DelegationNumber,
                PrincipalEmployeeId = principal?.Id ?? Guid.Empty,
                PrincipalName = principal?.FullName ?? "Pejabat Definitif",
                PrincipalPosition = "Pejabat Definitif",
                DelegateEmployeeId = delegateEmp?.Id ?? Guid.Empty,
                DelegateName = delegateEmp?.FullName ?? "Pejabat Plh",
                DelegatePosition = "Pejabat Plh",
                StartDate = delegation.EffectiveStartAt,
                EndDate = delegation.EffectiveEndAt,
                DelegatedRequestTypes = delegation.DelegatedRequestTypes ?? "ALL",
                DelegationReason = delegation.DelegationReason ?? string.Empty,
                DelegationStatus = delegation.DelegationStatus,
                IsActive = delegation.IsActive,
                CreatedAt = delegation.CreateDateTime,
                RevokedAt = delegation.RevokedAt,
                RevocationReason = delegation.RevocationReason
            };
        }

        public async Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalDelegationV2ResponseDto>> GetDelegationsAsync(
            bool? isActive, int pageNumber, int pageSize)
        {
            var query = _context.TrxApprovalDelegations.AsNoTracking();

            if (isActive.HasValue)
            {
                query = query.Where(d => d.IsActive == isActive.Value);
            }

            var totalData = await query.CountAsync();
            var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

            var items = await query
                .OrderByDescending(d => d.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultItems = new List<ApprovalDelegationV2ResponseDto>();
            foreach (var d in items)
            {
                var principal = await _context.MstEmployees.FindAsync(d.PrincipalEmployeeId ?? Guid.Empty);
                var delegateEmp = await _context.MstEmployees.FindAsync(d.DelegateEmployeeId ?? Guid.Empty);

                resultItems.Add(new ApprovalDelegationV2ResponseDto
                {
                    Id = d.Id,
                    DelegationNumber = d.DelegationNumber,
                    PrincipalEmployeeId = principal?.Id ?? Guid.Empty,
                    PrincipalName = principal?.FullName ?? "Pejabat Definitif",
                    PrincipalPosition = "Pejabat Definitif",
                    DelegateEmployeeId = delegateEmp?.Id ?? Guid.Empty,
                    DelegateName = delegateEmp?.FullName ?? "Pejabat Plh",
                    DelegatePosition = "Pejabat Pelaksana Harian (Plh)",
                    StartDate = d.EffectiveStartAt,
                    EndDate = d.EffectiveEndAt,
                    DelegatedRequestTypes = d.DelegatedRequestTypes ?? "ALL",
                    DelegationReason = d.DelegationReason ?? string.Empty,
                    DelegationStatus = d.DelegationStatus,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreateDateTime,
                    RevokedAt = d.RevokedAt,
                    RevocationReason = d.RevocationReason
                });
            }

            return new QuilvianSystemBackend.Responses.PagedResult<ApprovalDelegationV2ResponseDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = totalPage,
                Items = resultItems
            };
        }

        public async Task<List<ApprovalDelegationV2ResponseDto>> GetMyActingRolesAsync(Guid delegateEmployeeId)
        {
            var now = DateTime.UtcNow;
            var delegations = await _context.TrxApprovalDelegations
                .Where(d => d.DelegateEmployeeId == delegateEmployeeId &&
                            d.IsActive &&
                            d.StartDate <= now &&
                            d.EndDate >= now)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<ApprovalDelegationV2ResponseDto>();
            foreach (var d in delegations)
            {
                var principal = await _context.MstEmployees.FindAsync(d.PrincipalEmployeeId ?? Guid.Empty);
                result.Add(new ApprovalDelegationV2ResponseDto
                {
                    Id = d.Id,
                    DelegationNumber = d.DelegationNumber,
                    PrincipalEmployeeId = principal?.Id ?? Guid.Empty,
                    PrincipalName = principal?.FullName ?? "Pejabat Definitif",
                    PrincipalPosition = "Pejabat Definitif",
                    DelegateEmployeeId = delegateEmployeeId,
                    DelegateName = "Saya (Plh)",
                    DelegatePosition = "Pejabat Plh",
                    StartDate = d.EffectiveStartAt,
                    EndDate = d.EffectiveEndAt,
                    DelegatedRequestTypes = d.DelegatedRequestTypes ?? "ALL",
                    DelegationReason = d.DelegationReason ?? string.Empty,
                    DelegationStatus = d.DelegationStatus,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreateDateTime
                });
            }

            return result;
        }
    }
}
