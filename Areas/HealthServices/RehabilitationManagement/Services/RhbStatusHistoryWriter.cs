using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Penulis dan pembaca <see cref="RhbStatusHistory"/> (<c>BE-RHB-007</c>,
    /// <c>state-transition-matrix.md</c>, <c>permission-audit-matrix.md</c> bagian 5).
    /// </summary>
    /// <remarks>
    /// <see cref="Record"/> hanya menambahkan baris ke change tracker; baris tersimpan bersama
    /// perubahan status di <c>SaveChanges</c> pemanggil, sehingga riwayat tidak pernah tercatat
    /// untuk perpindahan yang gagal. Baris riwayat tidak pernah diubah atau dihapus.
    /// </remarks>
    public static class RhbStatusHistoryWriter
    {
        public const string Request = "Request";
        public const string Episode = "Episode";
        public const string Program = "Program";
        public const string Session = "Session";

        /// <summary>Tujuan terapi (BE-RHB-011): revisi dan penghentian beserta alasannya.</summary>
        public const string Goal = "Goal";

        public static void Record(
            ApplicationDbContext dbContext,
            string aggregateType,
            Guid aggregateId,
            string? fromStatus,
            string toStatus,
            string action,
            string? reasonText,
            Guid actorUserId,
            DateTime occurredAtUtc)
        {
            dbContext.RhbStatusHistories.Add(new RhbStatusHistory
            {
                AggregateType = aggregateType,
                AggregateId = aggregateId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                Action = action,
                ReasonText = RhbServiceSupport.Normalize(reasonText),
                ActorUserId = actorUserId,
                OccurredAt = occurredAtUtc,
                CreateDateTime = occurredAtUtc,
                CreateBy = actorUserId
            });
        }

        /// <summary>Riwayat satu aggregate, urut waktu terjadinya.</summary>
        public static Task<List<RehabStatusHistoryItem>> ReadAsync(
            ApplicationDbContext dbContext,
            string aggregateType,
            Guid aggregateId,
            CancellationToken cancellationToken) =>
            dbContext.RhbStatusHistories
                .AsNoTracking()
                .Where(x => x.AggregateType == aggregateType && x.AggregateId == aggregateId && !x.IsDelete)
                .OrderBy(x => x.OccurredAt)
                .Select(x => new RehabStatusHistoryItem
                {
                    Id = x.Id,
                    AggregateType = x.AggregateType,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Action = x.Action,
                    ReasonText = x.ReasonText,
                    ActorUserId = x.ActorUserId,
                    ActorName = dbContext.Users.Where(u => u.Id == x.ActorUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    OccurredAt = x.OccurredAt
                })
                .ToListAsync(cancellationToken);
    }
}
