using QuilvianSystemBackend.Responses;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class WhistleblowerCaseService : IWhistleblowerCaseService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;

        public WhistleblowerCaseService(IEmployeeRelationUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<WhistleblowerTicketReceiptDto> SubmitWhistleblowerCaseAsync(SubmitWhistleblowerRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-15: Payload tiket whistleblower wajib terenkripsi asimetris
            if (string.IsNullOrWhiteSpace(request.EncryptedPayload) || request.EncryptedPayload.Length < 32)
            {
                throw new InvalidOperationException("Data pengaduan whistleblower wajib dienkripsi pada level payload sebelum dikirim ke server sesuai aturan VR-EMP-15.");
            }

            var ticketNumber = $"WB-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";
            var rawKey = $"{ticketNumber}:{Guid.NewGuid():N}";
            var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

            var entity = new TrxEmployeeGrievance
            {
                Id = Guid.NewGuid(),
                GrievanceNumber = ticketNumber,
                ComplainantEmployeeId = request.IsAnonymous ? null : request.ComplainantEmployeeId,
                ComplainantWorkforceProfileId = request.IsAnonymous ? Guid.Empty : (request.ComplainantEmployeeId ?? Guid.Empty),
                GrievanceType = "Whistleblowing",
                Category = request.Category.Trim(),
                Subject = "Laporan Investigasi Whistleblower (Terkunci)",
                GrievanceSummary = "Laporan dugaan pelanggaran integritas kritis tersimpan dalam ciphertext enkripsi asimetris.",
                EncryptedReportPayload = request.EncryptedPayload.Trim(),
                TargetUnit = request.TargetUnit?.Trim(),
                SeverityLevel = string.IsNullOrWhiteSpace(request.SeverityLevel) ? "Critical" : request.SeverityLevel.Trim(),
                TargetResolutionDate = DateTime.UtcNow.AddDays(14),
                GrievanceDate = DateTime.UtcNow,
                GrievanceStatus = "Submitted",
                IsWhistleblowerCase = true,
                IsAnonymous = request.IsAnonymous,
                IsConfidential = true,
                AccessClassification = "TopSecretCompliance",
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddGrievanceAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new WhistleblowerTicketReceiptDto
            {
                TicketNumber = ticketNumber,
                AccessKeyHash = keyHash,
                Message = "Laporan whistleblower berhasil diterima dan diamankan dengan enkripsi asimetris.",
                SubmittedAt = entity.CreateDateTime
            };
        }

        public async Task<PagedResult<WhistleblowerCaseDto>> GetComplianceInboxAsync(string? category, string? severityLevel, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _uow.Grievances
                .AsNoTracking()
                .Where(x => x.IsWhistleblowerCase);

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(severityLevel))
            {
                query = query.Where(x => x.SeverityLevel == severityLevel.Trim());
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.GrievanceStatus == status.Trim());
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.GrievanceDate)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => new WhistleblowerCaseDto
                {
                    GrievanceId = x.Id,
                    TicketNumber = x.GrievanceNumber,
                    Category = x.Category ?? "Whistleblower",
                    TargetUnit = x.TargetUnit,
                    SeverityLevel = x.SeverityLevel,
                    EncryptedPayload = x.EncryptedReportPayload ?? string.Empty,
                    Status = x.GrievanceStatus,
                    TargetResolutionDate = x.TargetResolutionDate ?? x.GrievanceDate.AddDays(14),
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<WhistleblowerCaseDto>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = safePage,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / Math.Max(1, safePageSize))
            };
        }
    }
}
