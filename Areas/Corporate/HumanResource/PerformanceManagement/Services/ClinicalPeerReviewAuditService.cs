using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    public class ClinicalPeerReviewAuditService : IClinicalPeerReviewAuditService
    {
        private readonly ApplicationDbContext _context;

        public ClinicalPeerReviewAuditService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ClinicalPeerReviewAuditLogDto> LogPeerReviewAccessAsync(LogPeerReviewAccessRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var auditLog = new HrdClinicalPeerReviewAuditLog
            {
                Id = Guid.NewGuid(),
                AccessedWorkforceProfileId = request.AccessedWorkforceProfileId,
                RecordType = request.RecordType,
                RecordId = request.RecordId,
                AccessedByUserId = request.AccessedByUserId,
                AccessReason = request.AccessReason,
                AccessedAt = DateTime.UtcNow,
                IpAddress = request.IpAddress ?? "127.0.0.1",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = request.AccessedByUserId,
                IsDelete = false
            };

            await _context.HrdClinicalPeerReviewAuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Ambil nama dokter yang diakses jika ada
            var doctor = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.AccessedWorkforceProfileId, cancellationToken);

            var accessor = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.AccessedByUserId, cancellationToken);

            return new ClinicalPeerReviewAuditLogDto
            {
                Id = auditLog.Id,
                AccessedWorkforceProfileId = auditLog.AccessedWorkforceProfileId,
                AccessedDoctorName = doctor?.DisplayName ?? "Unknown Doctor",
                RecordType = auditLog.RecordType,
                RecordId = auditLog.RecordId,
                AccessedByUserId = auditLog.AccessedByUserId,
                AccessedByUserName = accessor?.DisplayName ?? "Clinical Auditor",
                AccessReason = auditLog.AccessReason,
                AccessedAt = auditLog.AccessedAt,
                IpAddress = auditLog.IpAddress
            };
        }

        public async Task<List<ClinicalPeerReviewAuditLogDto>> GetAuditLogsAsync(Guid? workforceProfileId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
        {
            var query = _context.HrdClinicalPeerReviewAuditLogs.AsNoTracking();

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(l => l.AccessedWorkforceProfileId == workforceProfileId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.AccessedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.AccessedAt <= toDate.Value);
            }

            var logs = await query
                .OrderByDescending(l => l.AccessedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            var doctorIds = logs.Select(l => l.AccessedWorkforceProfileId).Distinct().ToList();
            var accessorIds = logs.Select(l => l.AccessedByUserId).Distinct().ToList();
            var allProfileIds = doctorIds.Union(accessorIds).ToList();

            var profiles = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(p => allProfileIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.DisplayName, cancellationToken);

            return logs.Select(l => new ClinicalPeerReviewAuditLogDto
            {
                Id = l.Id,
                AccessedWorkforceProfileId = l.AccessedWorkforceProfileId,
                AccessedDoctorName = profiles.TryGetValue(l.AccessedWorkforceProfileId, out var docName) ? docName : "Unknown Doctor",
                RecordType = l.RecordType,
                RecordId = l.RecordId,
                AccessedByUserId = l.AccessedByUserId,
                AccessedByUserName = profiles.TryGetValue(l.AccessedByUserId, out var accName) ? accName : "Auditor",
                AccessReason = l.AccessReason,
                AccessedAt = l.AccessedAt,
                IpAddress = l.IpAddress
            }).ToList();
        }
    }
}
