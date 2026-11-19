using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        Task<ApprovalDelegationV2ResponseDto> CreateDelegationAsync(Guid principalEmployeeId, Guid principalUserId, CreateApprovalDelegationRequestDto request);
        Task<ApprovalDelegationV2ResponseDto> RevokeDelegationAsync(Guid delegationId, RevokeDelegationRequestDto request, Guid currentUserId, Guid currentEmployeeId, bool canManageAll);
        Task<QuilvianSystemBackend.Responses.PagedResult<ApprovalDelegationV2ResponseDto>> GetDelegationsAsync(bool? isActive, int pageNumber, int pageSize, Guid currentEmployeeId, bool canViewAll);
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

        public async Task<ApprovalDelegationV2ResponseDto> CreateDelegationAsync(Guid principalEmployeeId, Guid principalUserId, CreateApprovalDelegationRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Tanggal dari klien dinormalkan ke UTC karena kolom bertipe timestamp with time zone.
            var startAt = ToUtc(request.StartDate);
            var endAt = ToUtc(request.EndDate);

            // Validasi VR-WF-03: Rentang tanggal aktif harus sah
            if (endAt < startAt)
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

            // MSL-01 (18 Nov 2026): kolom DelegatorUserId/DelegateUserId ber-FK ke AspNetUsers, jadi wajib berisi ID akun,
            // bukan ID profil tenaga kerja. Plh harus punya akun login karena dialah yang membuka kotak masuk persetujuan.
            var delegatorUserId = await ResolveUserIdAsync(principal) ?? principalUserId;
            var delegateUserId = await ResolveUserIdAsync(delegateEmp);
            if (delegatorUserId == Guid.Empty)
            {
                throw new InvalidOperationException("Akun login pejabat pemberi wewenang tidak terbaca. Silakan masuk ulang lalu coba lagi.");
            }

            if (delegateUserId == null)
            {
                throw new InvalidOperationException($"{delegateEmp.FullName} belum memiliki akun login, sehingga belum bisa ditunjuk sebagai Plh. Minta bagian SDM membuatkan akunnya lebih dulu.");
            }

            if (delegateUserId == delegatorUserId)
            {
                throw new InvalidOperationException("Pendelegasian wewenang Plh ke diri sendiri dilarang mutlak (HRD-DEC-184)");
            }

            // Constraint CK_TrxApprovalDelegation_EffectivePeriod menuntut akhir > awal (Plh satu hari tetap sah).
            if (endAt <= startAt)
            {
                endAt = startAt.Date.AddDays(1).AddTicks(-1);
            }

            // Constraint CK_TrxApprovalDelegation_WorkflowScope: cakupan "ALL" berarti semua workflow;
            // cakupan per jenis permohonan disimpan sebagai JSON daftar kode jenis permohonan.
            var requestTypes = NormalizeRequestTypes(request.DelegatedRequestTypes);
            var appliesToAll = requestTypes.Count == 0 || requestTypes.Contains(AllRequestTypes);
            var scopeJson = appliesToAll ? null : JsonSerializer.Serialize(new { requestTypes });

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
                DelegatorUserId = delegatorUserId,
                DelegatorWorkforceProfileId = principal.WorkforceProfileId == Guid.Empty ? null : principal.WorkforceProfileId,
                DelegateUserId = delegateUserId.Value,
                DelegateWorkforceProfileId = delegateEmp.WorkforceProfileId == Guid.Empty ? null : delegateEmp.WorkforceProfileId,
                PrincipalEmployeeId = principalEmployeeId,
                DelegateEmployeeId = request.DelegateEmployeeId,
                DelegatedRequestTypes = appliesToAll ? AllRequestTypes : string.Join(",", requestTypes),
                AppliesToAllWorkflows = appliesToAll,
                ScopeDefinitionJson = scopeJson,
                DelegationReason = request.DelegationReason,
                EffectiveStartAt = startAt,
                EffectiveEndAt = endAt,
                StartDate = startAt,
                EndDate = endAt,
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

        public async Task<ApprovalDelegationV2ResponseDto> RevokeDelegationAsync(Guid delegationId, RevokeDelegationRequestDto request, Guid currentUserId, Guid currentEmployeeId, bool canManageAll)
        {
            var delegation = await _context.TrxApprovalDelegations.FindAsync(delegationId);
            if (delegation == null) throw new KeyNotFoundException("Surat kuasa delegasi Plh tidak ditemukan");

            // MSL-05: hanya pemberi kuasa, atau pemegang hak ViewAllDelegations, yang boleh mencabut.
            if (!canManageAll && delegation.PrincipalEmployeeId != currentEmployeeId)
            {
                throw new UnauthorizedAccessException("Surat kuasa ini hanya dapat dicabut oleh pejabat pemberi kuasa.");
            }

            if (!delegation.IsActive)
            {
                throw new InvalidOperationException("Surat kuasa ini sudah tidak aktif, sehingga tidak perlu dicabut lagi.");
            }

            delegation.IsActive = false;
            delegation.DelegationStatus = "Revoked";
            delegation.RevokedAt = DateTime.UtcNow;
            delegation.RevokedByUserId = currentUserId == Guid.Empty ? null : currentUserId;
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
            bool? isActive, int pageNumber, int pageSize, Guid currentEmployeeId, bool canViewAll)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.TrxApprovalDelegations.AsNoTracking();

            // MSL-05 (18 Nov 2026): pegawai hanya melihat surat kuasa tempat ia menjadi pemberi atau penerima kuasa.
            // Daftar lintas unit hanya untuk pemegang hak WorkflowDelegation : ViewAllDelegations (diberikan lewat Akses Role).
            if (!canViewAll)
            {
                query = query.Where(d => d.PrincipalEmployeeId == currentEmployeeId || d.DelegateEmployeeId == currentEmployeeId);
            }

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

        private const string AllRequestTypes = "ALL";

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private static List<string> NormalizeRequestTypes(string? raw)
        {
            return (raw ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToUpperInvariant())
                .Distinct()
                .ToList();
        }

        /// <summary>Akun login (AspNetUsers.Id) milik pegawai: dicocokkan lewat EmployeeId, lalu WorkforceProfileId.</summary>
        private async Task<Guid?> ResolveUserIdAsync(MstEmployee employee)
        {
            var workforceProfileId = employee.WorkforceProfileId;
            return await _context.Users.AsNoTracking()
                .Where(u => u.EmployeeId == employee.Id || (workforceProfileId != Guid.Empty && u.WorkforceProfileId == workforceProfileId))
                .OrderByDescending(u => u.EmployeeId == employee.Id)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync();
        }
    }
}
