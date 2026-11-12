using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>
    /// Penulis dan pembaca <see cref="PrcDocumentHistory"/> untuk seluruh dokumen Pengadaan
    /// (BE-PBJ-002; <c>permission-audit-matrix.md</c> bagian 5).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Record"/> hanya menambahkan baris ke change tracker. Baris tersimpan bersama
    /// perubahan status pada <c>SaveChanges</c> milik pemanggil, sehingga riwayat tidak pernah
    /// tercatat untuk perpindahan yang gagal (pola <c>RhbStatusHistoryWriter</c>).
    /// </para>
    /// <para>
    /// Service ini tidak punya metode ubah atau hapus: riwayat hanya ditambah. Isi alasan dan
    /// cuplikan bersifat sensitif dan tidak ditulis ke logger.
    /// </para>
    /// </remarks>
    public sealed class ProcurementDocumentHistoryWriter
    {
        private static readonly JsonSerializerOptions SnapshotJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _dbContext;

        public ProcurementDocumentHistoryWriter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mencatat satu aksi. Contoh: Tim Pengadaan mengembalikan PR-000031 dengan alasan
        /// "Spesifikasi kurang jelas" → <c>Record(PBJ_PR, id, "PR-000031", RETURN, "SUBMITTED",
        /// "RETURNED", alasan, null, aktor, sekarang)</c>.
        /// </summary>
        public PrcDocumentHistory Record(
            string documentType,
            Guid documentId,
            string? documentNumber,
            string actionCode,
            string? fromStatus,
            string? toStatus,
            string? reason,
            object? snapshot,
            Guid actorUserId,
            DateTimeOffset occurredAtUtc,
            Guid? workflowInstanceId = null)
        {
            if (!ProcurementDocumentTypes.IsKnown(documentType))
            {
                throw new ArgumentException(
                    $"Jenis dokumen Pengadaan tidak dikenal: '{documentType}'.",
                    nameof(documentType));
            }

            if (documentId == Guid.Empty)
            {
                throw new ArgumentException("ID dokumen wajib diisi.", nameof(documentId));
            }

            if (string.IsNullOrWhiteSpace(actionCode))
            {
                throw new ArgumentException("Kode aksi wajib diisi.", nameof(actionCode));
            }

            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Pelaku aksi wajib diisi.", nameof(actorUserId));
            }

            var occurredAt = occurredAtUtc.ToUniversalTime();

            var row = new PrcDocumentHistory
            {
                DocumentType = documentType.Trim(),
                DocumentId = documentId,
                DocumentNumber = Normalize(documentNumber),
                ActionCode = actionCode.Trim(),
                FromStatus = Normalize(fromStatus),
                ToStatus = Normalize(toStatus),
                Reason = Normalize(reason),
                SnapshotJson = snapshot == null
                    ? null
                    : JsonSerializer.Serialize(snapshot, SnapshotJsonOptions),
                ActorUserId = actorUserId,
                OccurredAt = occurredAt,
                WorkflowInstanceId = workflowInstanceId,
                CreateDateTime = occurredAt.UtcDateTime,
                CreateBy = actorUserId
            };

            _dbContext.PrcDocumentHistories.Add(row);
            return row;
        }

        /// <summary>Riwayat satu dokumen, urut waktu terjadinya.</summary>
        public Task<List<DocumentHistoryResponse>> ReadAsync(
            string documentType,
            Guid documentId,
            CancellationToken cancellationToken) =>
            _dbContext.PrcDocumentHistories
                .AsNoTracking()
                .Where(x => x.DocumentType == documentType && x.DocumentId == documentId && !x.IsDelete)
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => x.CreateDateTime)
                .Select(x => new DocumentHistoryResponse
                {
                    Id = x.Id,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    DocumentNumber = x.DocumentNumber,
                    ActionCode = x.ActionCode,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    Reason = x.Reason,
                    ActorUserId = x.ActorUserId,
                    ActorName = _dbContext.Users
                        .Where(u => u.Id == x.ActorUserId)
                        .Select(u => u.DisplayName)
                        .FirstOrDefault(),
                    OccurredAt = x.OccurredAt,
                    WorkflowInstanceId = x.WorkflowInstanceId
                })
                .ToListAsync(cancellationToken);

        private static string? Normalize(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
    }
}
