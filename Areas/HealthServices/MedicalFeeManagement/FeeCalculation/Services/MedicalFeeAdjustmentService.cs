using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Collections.Concurrent;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Service pengelola penyesuaian koreksi hasil jasa medis berjenjang (MdfServiceFeeAdjustment).
    /// Mengimplementasikan keputusan bisnis MF-DEC-007, arsitektur MDF-DES-017,
    /// siklus hidup status MDF-STATE-1.0 §3, dan aturan validasi MDF-VAL-1.0 §6.
    /// 
    /// ATURAN UTAMA:
    /// 1. Pembatalan/koreksi nilai hanya lewat penyesuaian selisih; angka historis lama tidak pernah diubah (MF-DEC-007).
    /// 2. Maker-Checker ditegakkan dua lapis (service dan database check constraint CK_MdfServiceFeeAdjustment_Maker):
    ///    RequestedBy wajib berbeda dari ApprovedBy (MDF-VAL-1.0 §6.8, T-60, T-61).
    /// 3. Status Approved dan Rejected bersifat final dan tidak dapat diubah (MDF-VAL-1.0 §6.9, T-68).
    /// 4. Koreksi Deduction tidak boleh menghasilkan FinalAmount negatif (MDF-VAL-1.0 §6.11, T-67).
    /// 5. Pengajuan koreksi wajib membawa header Idempotency-Key (MDF-VAL-1.0 §6.12, T-70).
    /// </summary>
    public class MedicalFeeAdjustmentService
    {
        private static readonly ConcurrentDictionary<string, ServiceFeeAdjustmentResponse> _idempotencyStore = new();

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MedicalFeeAdjustmentService> _logger;

        public MedicalFeeAdjustmentService(
            ApplicationDbContext dbContext,
            ILogger<MedicalFeeAdjustmentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =========================================================================
        // Metadata & Query
        // =========================================================================

        /// <summary>
        /// Mengambil metadata filter dan opsi konfigurasi penyesuaian hasil jasa medis.
        /// </summary>
        public AdjustmentFilterMetadataResponse GetFilterMetadata()
        {
            return new AdjustmentFilterMetadataResponse();
        }

        /// <summary>
        /// Mengambil daftar riwayat koreksi penyesuaian untuk satu hasil jasa medis tertentu.
        /// </summary>
        public async Task<List<ServiceFeeAdjustmentResponse>> GetAdjustmentsByServiceFeeIdAsync(
            Guid serviceFeeId,
            MedicalFeeActorContext? actorContext = null,
            CancellationToken ct = default)
        {
            var fee = await _dbContext.MdfServiceFees
                .AsNoTracking()
                .FirstOrDefaultAsync(sf => sf.Id == serviceFeeId && !sf.IsDelete, ct);

            if (fee == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{serviceFeeId}' tidak ditemukan.");
            }

            // BE-MDF-017 / T-82: Pembatasan data tenaga medis di lapis service (404 jika bukan miliknya)
            MedicalFeeDataRestrictionPolicy.AssertCanAccessServiceFee(actorContext, fee.PayeeReferenceId, serviceFeeId);

            var adjustments = await _dbContext.MdfServiceFeeAdjustments
                .AsNoTracking()
                .Where(a => a.ServiceFeeId == serviceFeeId && !a.IsDelete)
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync(ct);

            var responses = adjustments.Select(a => MapToResponse(a, fee)).ToList();
            await EnrichUserNamesAsync(responses, ct);

            return responses;
        }

        /// <summary>
        /// Mengambil detail satu koreksi penyesuaian berdasarkan ID.
        /// </summary>
        public async Task<ServiceFeeAdjustmentResponse> GetAdjustmentByIdAsync(
            Guid serviceFeeId,
            Guid adjustmentId,
            MedicalFeeActorContext? actorContext = null,
            CancellationToken ct = default)
        {
            var fee = await _dbContext.MdfServiceFees
                .AsNoTracking()
                .FirstOrDefaultAsync(sf => sf.Id == serviceFeeId && !sf.IsDelete, ct);

            if (fee == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{serviceFeeId}' tidak ditemukan.");
            }

            // BE-MDF-017 / T-82: Pembatasan data tenaga medis di lapis service (404 jika bukan miliknya)
            MedicalFeeDataRestrictionPolicy.AssertCanAccessServiceFee(actorContext, fee.PayeeReferenceId, serviceFeeId);

            var adj = await _dbContext.MdfServiceFeeAdjustments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == adjustmentId && a.ServiceFeeId == serviceFeeId && !a.IsDelete, ct);

            if (adj == null)
            {
                throw new KeyNotFoundException($"Koreksi penyesuaian dengan ID '{adjustmentId}' pada hasil jasa '{serviceFeeId}' tidak ditemukan.");
            }

            var response = MapToResponse(adj, fee);
            var list = new List<ServiceFeeAdjustmentResponse> { response };
            await EnrichUserNamesAsync(list, ct);

            return response;
        }

        /// <summary>
        /// Mengambil detail satu koreksi penyesuaian langsung berdasarkan ID penyesuaian.
        /// </summary>
        public async Task<ServiceFeeAdjustmentResponse> GetAdjustmentByIdDirectAsync(
            Guid adjustmentId,
            CancellationToken ct = default)
        {
            var adj = await _dbContext.MdfServiceFeeAdjustments
                .AsNoTracking()
                .Include(a => a.ServiceFee)
                .FirstOrDefaultAsync(a => a.Id == adjustmentId && !a.IsDelete, ct);

            if (adj == null)
            {
                throw new KeyNotFoundException($"Koreksi penyesuaian dengan ID '{adjustmentId}' tidak ditemukan.");
            }

            var response = MapToResponse(adj, adj.ServiceFee!);
            var list = new List<ServiceFeeAdjustmentResponse> { response };
            await EnrichUserNamesAsync(list, ct);

            return response;
        }

        /// <summary>
        /// Mengambil daftar koreksi terpaginasi dengan filter status, direction, dan pencarian teks.
        /// </summary>
        public async Task<PagedResult<ServiceFeeAdjustmentResponse>> GetAdjustmentsPagedAsync(
            AdjustmentPagedQuery query,
            CancellationToken ct = default)
        {
            var q = _dbContext.MdfServiceFeeAdjustments
                .AsNoTracking()
                .Include(a => a.ServiceFee)
                .Where(a => !a.IsDelete);

            if (query.ServiceFeeId.HasValue)
            {
                q = q.Where(a => a.ServiceFeeId == query.ServiceFeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(a => a.Status == query.Status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.Direction))
            {
                q = q.Where(a => a.Direction == query.Direction.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                q = q.Where(a => EF.Functions.ILike(a.AdjustmentNumber, $"%{search}%") ||
                                 EF.Functions.ILike(a.Reason, $"%{search}%"));
            }

            var totalItems = await q.CountAsync(ct);

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 25 : query.PageSize;

            bool isAsc = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            q = (query.SortBy?.ToLowerInvariant(), isAsc) switch
            {
                ("amount", true) => q.OrderBy(a => a.Amount),
                ("amount", false) => q.OrderByDescending(a => a.Amount),
                ("adjustmentnumber", true) => q.OrderBy(a => a.AdjustmentNumber),
                ("adjustmentnumber", false) => q.OrderByDescending(a => a.AdjustmentNumber),
                ("status", true) => q.OrderBy(a => a.Status),
                ("status", false) => q.OrderByDescending(a => a.Status),
                ("requestedat", true) => q.OrderBy(a => a.RequestedAt),
                _ => q.OrderByDescending(a => a.RequestedAt)
            };

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var responses = items.Select(a => MapToResponse(a, a.ServiceFee!)).ToList();
            await EnrichUserNamesAsync(responses, ct);

            return new PagedResult<ServiceFeeAdjustmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = responses
            };
        }

        // =========================================================================
        // Mutation Operations (Request, Approve, Reject)
        // =========================================================================

        /// <summary>
        /// Mengajukan penyesuaian koreksi jasa medis baru (status: Pending).
        /// Wajib menyertakan header Idempotency-Key untuk perlindungan eksekusi ganda (MDF-VAL-1.0 §6.12, T-70).
        /// </summary>
        public async Task<ServiceFeeAdjustmentResponse> RequestAdjustmentAsync(
            Guid serviceFeeId,
            CreateAdjustmentRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            // 1. Validasi Idempotency-Key (MDF-VAL-1.0 §6.12, T-70)
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new ArgumentException("Header 'Idempotency-Key' wajib disertakan saat mengajukan koreksi (MDF-VAL-1.0 §6.12).");
            }

            var normalizedKey = idempotencyKey.Trim();
            if (_idempotencyStore.TryGetValue(normalizedKey, out var existingResponse))
            {
                _logger.LogInformation("Permintaan koreksi dengan Idempotency-Key '{Key}' telah diproses sebelumnya. Mengembalikan hasil yang tersimpan.", normalizedKey);
                return existingResponse;
            }

            // 2. Validasi Nominal & Arah (MDF-VAL-1.0 §6.5, §6.6, T-66)
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Nominal koreksi penyesuaian harus lebih besar dari 0 (MDF-VAL-1.0 §6.6, T-66).");
            }

            if (request.Direction != MdfAdjustmentDirections.Addition && request.Direction != MdfAdjustmentDirections.Deduction)
            {
                throw new ArgumentException($"Arah koreksi '{request.Direction}' tidak valid. Hanya diizinkan 'Addition' atau 'Deduction' (MDF-VAL-1.0 §6.5).");
            }

            // 3. Validasi Alasan (MDF-VAL-1.0 §6.7, T-65)
            var trimmedReason = request.Reason?.Trim() ?? string.Empty;
            if (trimmedReason.Length < 10 || trimmedReason.Length > 500)
            {
                throw new ArgumentException("Alasan koreksi wajib diisi antara 10 sampai 500 karakter (MDF-VAL-1.0 §6.7, T-65).");
            }

            // 4. Validasi Keberadaan Hasil Jasa Medis
            var sf = await _dbContext.MdfServiceFees
                .FirstOrDefaultAsync(f => f.Id == serviceFeeId && !f.IsDelete, ct);

            if (sf == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{serviceFeeId}' tidak ditemukan.");
            }

            if (request.ExpectedRowVersion.HasValue && sf.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data hasil jasa telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            // 5. Validasi Koreksi Deduction tidak melebihi nilai kotor jasa (MDF-VAL-1.0 §6.11, T-67)
            if (request.Direction == MdfAdjustmentDirections.Deduction)
            {
                var projectedFinal = sf.GrossAmount + sf.AdjustmentAmount - request.Amount;
                if (projectedFinal < 0)
                {
                    throw new InvalidOperationException(
                        $"Koreksi pengurangan sebesar Rp{request.Amount:N2} melebihi nilai kotor jasa sehingga menghasilkan nilai akhir negatif Rp{projectedFinal:N2} (MDF-VAL-1.0 §6.11, T-67). Nilai akhir jasa tidak boleh negatif.");
                }
            }

            // 6. Generate AdjustmentNumber unik: ADJ-YYYYMM-XXXX
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
            var monthCount = await _dbContext.MdfServiceFeeAdjustments
                .CountAsync(a => a.AdjustmentNumber.StartsWith($"ADJ-{yearMonth}-"), ct);
            var adjustmentNumber = $"ADJ-{yearMonth}-{(monthCount + 1):D4}";

            var now = DateTime.UtcNow;
            var adjustment = new MdfServiceFeeAdjustment
            {
                Id = Guid.NewGuid(),
                AdjustmentNumber = adjustmentNumber,
                ServiceFeeId = serviceFeeId,
                Direction = request.Direction,
                Amount = request.Amount,
                Reason = trimmedReason,
                Status = MdfAdjustmentStatuses.Pending,
                RequestedBy = actorUserId,
                RequestedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = now,
                UpdateBy = actorUserId,
                UpdateDateTime = now
            };

            _dbContext.MdfServiceFeeAdjustments.Add(adjustment);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Koreksi jasa '{AdjustmentNumber}' berhasil diajukan untuk hasil jasa '{FeeNumber}' oleh user '{UserId}'.",
                adjustment.AdjustmentNumber, sf.FeeNumber, actorUserId);

            var response = MapToResponse(adjustment, sf);
            var list = new List<ServiceFeeAdjustmentResponse> { response };
            await EnrichUserNamesAsync(list, ct);

            // Simpan ke memory idempotency store
            _idempotencyStore.TryAdd(normalizedKey, response);

            return response;
        }

        /// <summary>
        /// Menyetujui pengajuan penyesuaian koreksi jasa medis (status: Pending -> Approved).
        /// Menerapkan kontrol Maker-Checker dua lapis: ApprovedBy wajib berbeda dari RequestedBy (MDF-DES-017, MDF-VAL-1.0 §6.8, T-60, T-61).
        /// Memperbarui AdjustmentAmount dan FinalAmount pada MdfServiceFee.
        /// </summary>
        public async Task<ServiceFeeAdjustmentResponse> ApproveAdjustmentAsync(
            Guid serviceFeeId,
            Guid adjustmentId,
            ApproveAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                var adj = await _dbContext.MdfServiceFeeAdjustments
                    .Include(a => a.ServiceFee)
                    .FirstOrDefaultAsync(a => a.Id == adjustmentId && a.ServiceFeeId == serviceFeeId && !a.IsDelete, ct);

                if (adj == null)
                {
                    throw new KeyNotFoundException($"Koreksi penyesuaian dengan ID '{adjustmentId}' pada hasil jasa '{serviceFeeId}' tidak ditemukan.");
                }

                if (request.ExpectedRowVersion.HasValue && adj.RowVersion != request.ExpectedRowVersion.Value)
                {
                    throw new DbUpdateConcurrencyException("Data koreksi telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
                }

                // 1. Validasi Status Final (MDF-VAL-1.0 §6.9, T-68)
                if (adj.Status != MdfAdjustmentStatuses.Pending)
                {
                    throw new InvalidOperationException(
                        $"Koreksi penyesuaian berstatus '{adj.Status}' tidak dapat diubah (MDF-VAL-1.0 §6.9, MDF_ADJUSTMENT_STATE_INVALID, T-68). Status Approved atau Rejected bersifat permanen.");
                }

                // 2. Validasi Maker-Checker (MDF-DES-017, MDF-VAL-1.0 §6.8, T-60, T-61)
                if (adj.RequestedBy == actorUserId)
                {
                    throw new InvalidOperationException(
                        "Penyetuju koreksi tidak boleh sama dengan pembuat pengajuan (RequestedBy == ApprovedBy) — pelanggaran aturan Maker-Checker (MDF-VAL-1.0 §6.8, MDF_MAKER_CHECKER_VIOLATION, T-60).");
                }

                var sf = adj.ServiceFee;
                if (sf == null)
                {
                    throw new InvalidOperationException("Data induk hasil jasa medis tidak ditemukan.");
                }

                // 3. Perhitungan dampak koreksi terhadap nilai akhir (MDF-VAL-1.0 §6.1, §6.11, T-64, T-67)
                decimal delta = adj.Direction == MdfAdjustmentDirections.Addition ? adj.Amount : -adj.Amount;
                decimal newAdjustmentTotal = sf.AdjustmentAmount + delta;
                decimal newFinalAmount = sf.GrossAmount + newAdjustmentTotal;

                if (newFinalAmount < 0)
                {
                    throw new InvalidOperationException(
                        $"Persetujuan koreksi dibatalkan karena menghasilkan nilai akhir jasa medis negatif Rp{newFinalAmount:N2} (MDF-VAL-1.0 §6.11, T-67).");
                }

                var now = DateTime.UtcNow;

                // Update koreksi ke Approved
                adj.Status = MdfAdjustmentStatuses.Approved;
                adj.ApprovedBy = actorUserId;
                adj.ApprovedAt = now;
                adj.RowVersion = Guid.NewGuid();
                adj.UpdateBy = actorUserId;
                adj.UpdateDateTime = now;

                // Update nilai pada aggregate root MdfServiceFee
                sf.AdjustmentAmount = newAdjustmentTotal;
                sf.FinalAmount = newFinalAmount;
                sf.RowVersion = Guid.NewGuid();
                sf.UpdateBy = actorUserId;
                sf.UpdateDateTime = now;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Koreksi jasa '{AdjustmentNumber}' disetujui oleh user '{ApproverId}'. Nilai akhir jasa '{FeeNumber}' kini Rp{FinalAmount:N2}.",
                    adj.AdjustmentNumber, actorUserId, sf.FeeNumber, sf.FinalAmount);

                var response = MapToResponse(adj, sf);
                var list = new List<ServiceFeeAdjustmentResponse> { response };
                await EnrichUserNamesAsync(list, ct);

                return response;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Menolak pengajuan penyesuaian koreksi jasa medis (status: Pending -> Rejected).
        /// Wajib menyertakan alasan penolakan (MDF-VAL-1.0 §6.10, T-69).
        /// Menerapkan kontrol Maker-Checker: penolak wajib berbeda dari pengaju.
        /// </summary>
        public async Task<ServiceFeeAdjustmentResponse> RejectAdjustmentAsync(
            Guid serviceFeeId,
            Guid adjustmentId,
            RejectAdjustmentRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var trimmedRejection = request.RejectionReason?.Trim() ?? string.Empty;
            if (trimmedRejection.Length < 10 || trimmedRejection.Length > 500)
            {
                throw new ArgumentException("Alasan penolakan koreksi wajib diisi antara 10 sampai 500 karakter (MDF-VAL-1.0 §6.10, T-69).");
            }

            var adj = await _dbContext.MdfServiceFeeAdjustments
                .Include(a => a.ServiceFee)
                .FirstOrDefaultAsync(a => a.Id == adjustmentId && a.ServiceFeeId == serviceFeeId && !a.IsDelete, ct);

            if (adj == null)
            {
                throw new KeyNotFoundException($"Koreksi penyesuaian dengan ID '{adjustmentId}' pada hasil jasa '{serviceFeeId}' tidak ditemukan.");
            }

            if (request.ExpectedRowVersion.HasValue && adj.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data koreksi telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            // 1. Validasi Status Final (MDF-VAL-1.0 §6.9, T-68)
            if (adj.Status != MdfAdjustmentStatuses.Pending)
            {
                throw new InvalidOperationException(
                    $"Koreksi penyesuaian berstatus '{adj.Status}' tidak dapat ditolak (MDF-VAL-1.0 §6.9, MDF_ADJUSTMENT_STATE_INVALID, T-68). Status Approved atau Rejected bersifat permanen.");
            }

            // 2. Validasi Maker-Checker
            if (adj.RequestedBy == actorUserId)
            {
                throw new InvalidOperationException(
                    "Penolak koreksi tidak boleh sama dengan pembuat pengajuan (RequestedBy == ApprovedBy) — pelanggaran aturan Maker-Checker (MDF-VAL-1.0 §6.8, MDF_MAKER_CHECKER_VIOLATION).");
            }

            var now = DateTime.UtcNow;
            adj.Status = MdfAdjustmentStatuses.Rejected;
            adj.ApprovedBy = actorUserId;
            adj.ApprovedAt = now;
            adj.RejectionReason = trimmedRejection;
            adj.RowVersion = Guid.NewGuid();
            adj.UpdateBy = actorUserId;
            adj.UpdateDateTime = now;

            // Nilai pada MdfServiceFee tidak berubah saat ditolak
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Koreksi jasa '{AdjustmentNumber}' ditolak oleh user '{ApproverId}'. Alasan: {Reason}",
                adj.AdjustmentNumber, actorUserId, trimmedRejection);

            var sf = adj.ServiceFee ?? await _dbContext.MdfServiceFees.FindAsync(new object[] { serviceFeeId }, ct);
            var response = MapToResponse(adj, sf!);
            var list = new List<ServiceFeeAdjustmentResponse> { response };
            await EnrichUserNamesAsync(list, ct);

            return response;
        }

        // =========================================================================
        // Internal Helpers
        // =========================================================================

        private static ServiceFeeAdjustmentResponse MapToResponse(MdfServiceFeeAdjustment a, MdfServiceFee sf)
        {
            return new ServiceFeeAdjustmentResponse
            {
                Id = a.Id,
                AdjustmentNumber = a.AdjustmentNumber,
                ServiceFeeId = a.ServiceFeeId,
                FeeNumber = sf != null ? sf.FeeNumber : string.Empty,
                Direction = a.Direction,
                Amount = a.Amount,
                Reason = a.Reason,
                Status = a.Status,
                RequestedBy = a.RequestedBy,
                RequestedByName = string.Empty,
                RequestedAt = a.RequestedAt,
                ApprovedBy = a.ApprovedBy,
                ApprovedByName = null,
                ApprovedAt = a.ApprovedAt,
                RejectionReason = a.RejectionReason,
                RowVersion = a.RowVersion,
                CurrentGrossAmount = sf != null ? sf.GrossAmount : 0m,
                CurrentAdjustmentAmount = sf != null ? sf.AdjustmentAmount : 0m,
                CurrentFinalAmount = sf != null ? sf.FinalAmount : 0m
            };
        }

        private async Task EnrichUserNamesAsync(List<ServiceFeeAdjustmentResponse> responses, CancellationToken ct)
        {
            if (responses == null || responses.Count == 0)
                return;

            var userIds = responses
                .Select(r => r.RequestedBy)
                .Concat(responses.Where(r => r.ApprovedBy.HasValue).Select(r => r.ApprovedBy!.Value))
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
                return;

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? "-", ct);

            foreach (var r in responses)
            {
                if (users.TryGetValue(r.RequestedBy, out var reqName))
                {
                    r.RequestedByName = reqName;
                }
                else
                {
                    r.RequestedByName = r.RequestedBy.ToString();
                }

                if (r.ApprovedBy.HasValue && users.TryGetValue(r.ApprovedBy.Value, out var appName))
                {
                    r.ApprovedByName = appName;
                }
            }
        }
    }
}
