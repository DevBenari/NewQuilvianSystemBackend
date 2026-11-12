using QuilvianSystemBackend.Responses;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class BipartiteForumService : IBipartiteForumService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public BipartiteForumService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<BipartiteForumResponseDto> ScheduleMeetingAsync(ScheduleBipartiteMeetingRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-17: Delegasi manajemen dan serikat buruh masing-masing minimal 1 delegasi sah
            if (string.IsNullOrWhiteSpace(request.ManagementReps) || string.IsNullOrWhiteSpace(request.UnionReps))
            {
                throw new InvalidOperationException("Forum perundingan Bipartit wajib dihadiri oleh perwakilan manajemen dan serikat secara sah sesuai aturan VR-EMP-17.");
            }

            // Nomor urut tahunan dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: BIP-2026-0001
            var registerNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.Bipartite,
                Prefix: EmployeeRelationNumberSeries.Prefixes.Bipartite,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.Bipartite,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var entity = new HrdBipartiteNegotiationForum
            {
                ForumId = Guid.NewGuid(),
                RegisterNumber = registerNumber,
                MeetingDate = request.MeetingDate,
                DisputeType = request.DisputeType.Trim(),
                ManagementRepresentatives = request.ManagementReps.Trim(),
                UnionRepresentatives = request.UnionReps.Trim(),
                AgendaDescription = request.Agenda.Trim(),
                MinutesOfMeeting = "Agenda telah dijadwalkan.",
                Status = "Scheduled",
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddBipartiteForumAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }

        public async Task<PagedResult<BipartiteForumResponseDto>> GetBipartiteForumsAsync(string? disputeType, string? status, int? year, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _uow.BipartiteForums.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(disputeType))
            {
                query = query.Where(x => x.DisputeType == disputeType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status.Trim());
            }

            if (year.HasValue)
            {
                query = query.Where(x => x.MeetingDate.Year == year.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.MeetingDate)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => MapToDto(x))
                .ToListAsync(cancellationToken);

            return new PagedResult<BipartiteForumResponseDto>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = safePage,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / Math.Max(1, safePageSize))
            };
        }

        public async Task<BipartiteForumResponseDto> RecordMinutesAsync(Guid forumId, RecordMinutesRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var forum = await _uow.BipartiteForums.FirstOrDefaultAsync(x => x.ForumId == forumId, cancellationToken);
            if (forum == null)
            {
                throw new KeyNotFoundException($"Sidang Bipartit dengan ID {forumId} tidak ditemukan.");
            }

            forum.MinutesOfMeeting = request.MinutesOfMeeting.Trim();
            if (!string.IsNullOrWhiteSpace(request.DeadlockPoints))
            {
                forum.Status = "Deadlocked";
            }
            forum.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(forum);
        }

        public async Task<BipartiteForumResponseDto> UploadJointAgreementAsync(Guid forumId, UploadJointAgreementRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var forum = await _uow.BipartiteForums.FirstOrDefaultAsync(x => x.ForumId == forumId, cancellationToken);
            if (forum == null)
            {
                throw new KeyNotFoundException($"Sidang Bipartit dengan ID {forumId} tidak ditemukan.");
            }

            // VR-EMP-18: Naskah Perjanjian Bersama (PB) wajib berekstensi .pdf
            if (string.IsNullOrWhiteSpace(request.JointAgreementDocUrl) || !request.JointAgreementDocUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Naskah Perjanjian Bersama (PB) wajib dilampirkan dalam format PDF resmi sesuai aturan VR-EMP-18.");
            }

            forum.JointAgreementDocumentUrl = request.JointAgreementDocUrl.Trim();
            forum.RegistrationDisnakerNumber = request.RegistrationDisnakerNumber?.Trim();
            forum.Status = "CompletedAgreementReached";
            forum.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(forum);
        }

        private static BipartiteForumResponseDto MapToDto(HrdBipartiteNegotiationForum entity)
        {
            return new BipartiteForumResponseDto
            {
                ForumId = entity.ForumId,
                RegisterNumber = entity.RegisterNumber,
                MeetingDate = entity.MeetingDate,
                DisputeType = entity.DisputeType,
                ManagementRepresentatives = entity.ManagementRepresentatives,
                UnionRepresentatives = entity.UnionRepresentatives,
                AgendaDescription = entity.AgendaDescription,
                MinutesOfMeeting = entity.MinutesOfMeeting,
                JointAgreementDocumentUrl = entity.JointAgreementDocumentUrl,
                RegistrationDisnakerNumber = entity.RegistrationDisnakerNumber,
                Status = entity.Status,
                CreatedAt = entity.CreateDateTime
            };
        }

        private async Task<Guid> ResolveActorUserIdAsync(string actorEmail, CancellationToken cancellationToken)
        {
            var userId = await _uow.FindUserIdByEmailAsync(actorEmail, cancellationToken);
            return userId ?? EmployeeRelationNumberSeries.SystemFallbackUserId;
        }
    }
}
