using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Service pengelola siklus hidup penyerahan hasil jasa medis ke Finance AP (MdfFinanceHandoff).
    /// Mengimplementasikan kontrak MDF-INTEGRATION-1.0 §7, MDF-STATE-1.0 §5, MDF-VAL-1.0 §7.
    /// Pola arsitektur: Handoff / Integration Gateway (pola BilArHandoff).
    /// 
    /// ATURAN KESELAMATAN & INTEGRASI:
    /// 1. Nilai penyerahan adalah nilai kotor (GrossAmount = FinalAmount), sebelum potongan pajak PPh 21 atau kasbon (MF-DEC-005, T-72).
    /// 2. Handoff hanya dibuat untuk hasil jasa yang berstatus Approved (MDF-VAL-1.0 §7.4, T-74).
    /// 3. Satu hasil jasa hanya boleh memiliki tepat satu baris penyerahan (MDF-VAL-1.0 §7.5, T-73).
    /// 4. HandoffKey bersifat unik dan TIDAK BOLEH berubah saat pengiriman ulang (Rule 7.7, T-76).
    /// 5. Konfirmasi (ACK) dari Finance wajib menyertakan HandoffKey yang cocok (Rule 7.8, T-75).
    /// 6. Penolakan dari Finance wajib membawa failureReason minimal 10 karakter (Rule 7.9, T-77).
    /// 7. Medical Fee TIDAK PERNAH menulis langsung ke tabel milik modul Finance (Rule 8).
    /// </summary>
    public class MedicalFeeHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MedicalFeeHandoffService> _logger;

        public MedicalFeeHandoffService(
            ApplicationDbContext dbContext,
            ILogger<MedicalFeeHandoffService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Mengambil daftar penyerahan ke Finance terpaginasi dengan filter status, periodCode, payeeType, dan tanggal.
        /// </summary>
        public async Task<PagedResult<MedicalFeeHandoffItemDto>> GetPagedAsync(
            MedicalFeeHandoffListQuery query,
            CancellationToken ct = default)
        {
            var baseQuery = _dbContext.MdfFinanceHandoffs
                .Include(h => h.ServiceFee)
                .Where(h => !h.IsDelete);

            // Filter kode periode
            if (!string.IsNullOrWhiteSpace(query.PeriodCode))
            {
                var periodCode = query.PeriodCode.Trim();
                baseQuery = baseQuery.Where(h => h.PeriodCode == periodCode);
            }

            // Filter status
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim();
                baseQuery = baseQuery.Where(h => h.Status == status);
            }

            // Filter jenis penerima
            if (!string.IsNullOrWhiteSpace(query.PayeeType))
            {
                var payeeType = query.PayeeType.Trim();
                baseQuery = baseQuery.Where(h => h.PayeeType == payeeType);
            }

            // Filter rentang tanggal pembuatan
            if (query.DateFrom.HasValue)
            {
                baseQuery = baseQuery.Where(h => h.CreateDateTime >= query.DateFrom.Value);
            }
            if (query.DateTo.HasValue)
            {
                baseQuery = baseQuery.Where(h => h.CreateDateTime <= query.DateTo.Value);
            }

            // Pencarian kata kunci bebas
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                if (Guid.TryParse(term, out var searchGuid))
                {
                    baseQuery = baseQuery.Where(h =>
                        h.HandoffKey == searchGuid ||
                        h.ServiceFeeId == searchGuid ||
                        h.PayeeReferenceId == searchGuid);
                }
                else
                {
                    baseQuery = baseQuery.Where(h =>
                        EF.Functions.ILike(h.PeriodCode, $"%{term}%") ||
                        (h.ServiceFee != null && EF.Functions.ILike(h.ServiceFee.FeeNumber, $"%{term}%")));
                }
            }

            var totalCount = await baseQuery.CountAsync(ct);

            // Pengurutan
            IOrderedQueryable<MdfFinanceHandoff> orderedQuery = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
            {
                ("periodcode", "asc") => baseQuery.OrderBy(h => h.PeriodCode),
                ("periodcode", "desc") => baseQuery.OrderByDescending(h => h.PeriodCode),
                ("grossamount", "asc") => baseQuery.OrderBy(h => h.GrossAmount),
                ("grossamount", "desc") => baseQuery.OrderByDescending(h => h.GrossAmount),
                ("status", "asc") => baseQuery.OrderBy(h => h.Status),
                ("status", "desc") => baseQuery.OrderByDescending(h => h.Status),
                ("createdatetime", "asc") => baseQuery.OrderBy(h => h.CreateDateTime),
                _ => baseQuery.OrderByDescending(h => h.CreateDateTime)
            };

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new MedicalFeeHandoffItemDto
                {
                    Id = h.Id,
                    ServiceFeeId = h.ServiceFeeId,
                    FeeNumber = h.ServiceFee != null ? h.ServiceFee.FeeNumber : string.Empty,
                    HandoffKey = h.HandoffKey,
                    PayeeType = h.PayeeType,
                    PayeeReferenceId = h.PayeeReferenceId,
                    PeriodCode = h.PeriodCode,
                    GrossAmount = h.GrossAmount,
                    Status = h.Status,
                    CorrelationId = h.CorrelationId,
                    CreatedAt = h.CreateDateTime,
                    AcknowledgedAt = h.AcknowledgedAt,
                    FailureReason = h.FailureReason,
                    RowVersion = h.RowVersion
                })
                .ToListAsync(ct);

            // Nama penerima diturunkan di memori: operator range [..8] tidak boleh ada di dalam expression tree EF.
            foreach (var item in items)
            {
                item.PayeeName = BuildPayeeLabel(item.PayeeType, item.PayeeReferenceId);
            }

            return new PagedResult<MedicalFeeHandoffItemDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Mengambil detail satu penyerahan ke Finance berdasarkan ID.
        /// </summary>
        public async Task<MedicalFeeHandoffDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var handoff = await _dbContext.MdfFinanceHandoffs
                .Include(h => h.ServiceFee)
                    .ThenInclude(sf => sf!.Period)
                .Include(h => h.ServiceFee)
                    .ThenInclude(sf => sf!.Details)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, ct);

            if (handoff == null)
            {
                throw new KeyNotFoundException($"Data penyerahan ke Finance dengan ID '{id}' tidak ditemukan.");
            }

            var sf = handoff.ServiceFee;
            return new MedicalFeeHandoffDetailDto
            {
                Id = handoff.Id,
                ServiceFeeId = handoff.ServiceFeeId,
                FeeNumber = sf != null ? sf.FeeNumber : string.Empty,
                HandoffKey = handoff.HandoffKey,
                PayeeType = handoff.PayeeType,
                PayeeReferenceId = handoff.PayeeReferenceId,
                PayeeName = BuildPayeeLabel(handoff.PayeeType, handoff.PayeeReferenceId),
                PeriodId = sf != null ? sf.PeriodId : Guid.Empty,
                PeriodCode = handoff.PeriodCode,
                GrossAmount = handoff.GrossAmount,
                Status = handoff.Status,
                CorrelationId = handoff.CorrelationId,
                CausationId = handoff.CausationId,
                AcknowledgedAt = handoff.AcknowledgedAt,
                FailureReason = handoff.FailureReason,
                RowVersion = handoff.RowVersion,
                CreateDateTime = handoff.CreateDateTime,
                CreateBy = handoff.CreateBy,
                LastUpdateDateTime = handoff.UpdateDateTime,
                LastUpdateBy = handoff.UpdateDateTime.HasValue ? handoff.UpdateBy : (Guid?)null,
                ServiceFeeFinalAmount = sf != null ? sf.FinalAmount : handoff.GrossAmount,
                ServiceFeeStatus = sf != null ? sf.Status : string.Empty,
                DetailCount = sf?.Details?.Count ?? 0
            };
        }

        /// <summary>
        /// Mengambil opsi metadata untuk filter antarmuka penyerahan Finance.
        /// </summary>
        public async Task<MedicalFeeHandoffMetadataResponse> GetMetadataAsync(CancellationToken ct = default)
        {
            var periodCodes = await _dbContext.MdfFinanceHandoffs
                .Where(h => !h.IsDelete)
                .Select(h => h.PeriodCode)
                .Distinct()
                .OrderByDescending(p => p)
                .ToListAsync(ct);

            return new MedicalFeeHandoffMetadataResponse
            {
                Statuses = new List<HandoffFilterOptionItem>
                {
                    new() { Key = MdfFinanceHandoffStatuses.Created, Label = "Created (Diserahkan)" },
                    new() { Key = MdfFinanceHandoffStatuses.Acknowledged, Label = "Acknowledged (Diterima Finance)" },
                    new() { Key = MdfFinanceHandoffStatuses.Failed, Label = "Failed (Ditolak Finance)" }
                },
                PayeeTypes = new List<HandoffFilterOptionItem>
                {
                    new() { Key = MdfPayeeTypes.Doctor, Label = "Dokter" },
                    new() { Key = MdfPayeeTypes.Workforce, Label = "Tenaga Kesehatan" }
                },
                PeriodCodes = periodCodes
            };
        }

        /// <summary>
        /// Finance mengonfirmasi penerimaan penyerahan (ACK) — MDF-VAL-1.0 §7.8, T-75.
        /// HandoffKey wajib cocok dengan data tersimpan.
        /// </summary>
        public async Task<MedicalFeeHandoffDetailDto> AcknowledgeAsync(
            Guid id,
            AcknowledgeHandoffRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var handoff = await _dbContext.MdfFinanceHandoffs
                .Include(h => h.ServiceFee)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, ct);

            if (handoff == null)
            {
                throw new KeyNotFoundException($"Data penyerahan dengan ID '{id}' tidak ditemukan.");
            }

            if (request.ExpectedRowVersion.HasValue && handoff.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data penyerahan telah diperbarui oleh proses lain. Silakan muat ulang halaman.");
            }

            // Jika sudah Acknowledged dengan HandoffKey yang sama, operasi bersifat idempoten
            if (handoff.Status == MdfFinanceHandoffStatuses.Acknowledged)
            {
                if (handoff.HandoffKey != request.HandoffKey)
                {
                    throw new InvalidOperationException("Kunci penyerahan (HandoffKey) tidak cocok dengan data tercatat (MDF-VAL-1.0 §7.8, T-75).");
                }
                return await GetByIdAsync(id, ct);
            }

            // Transisi hanya sah dari Created -> Acknowledged (MDF-STATE-1.0 §5)
            if (handoff.Status != MdfFinanceHandoffStatuses.Created)
            {
                throw new InvalidOperationException($"Penyerahan berstatus '{handoff.Status}' tidak dapat dikonfirmasi. Hanya penyerahan berstatus 'Created' yang dapat di-ACK.");
            }

            // Validasi kecocokan HandoffKey (MDF-VAL-1.0 §7.8, T-75)
            if (request.HandoffKey == Guid.Empty || request.HandoffKey != handoff.HandoffKey)
            {
                throw new InvalidOperationException("Kunci penyerahan (HandoffKey) tidak cocok dengan data tercatat (MDF-VAL-1.0 §7.8, T-75).");
            }

            var now = DateTime.UtcNow;
            handoff.Status = MdfFinanceHandoffStatuses.Acknowledged;
            handoff.AcknowledgedAt = now;
            handoff.RowVersion = Guid.NewGuid();
            handoff.UpdateBy = actorUserId;
            handoff.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Penyerahan hasil jasa ID '{HandoffId}' berhasil dikonfirmasi (ACK) oleh Finance. HandoffKey: {HandoffKey}.",
                handoff.Id, handoff.HandoffKey);

            return await GetByIdAsync(id, ct);
        }

        /// <summary>
        /// Finance menolak penyerahan karena ketidakkonsistenan data (Failure) — MDF-VAL-1.0 §7.9, T-77.
        /// Wajib menyertakan failureReason minimal 10 karakter.
        /// </summary>
        public async Task<MedicalFeeHandoffDetailDto> FailAsync(
            Guid id,
            FailHandoffRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.FailureReason) || request.FailureReason.Trim().Length < 10)
            {
                throw new ArgumentException("Alasan penolakan (FailureReason) wajib diisi minimal 10 karakter (MDF-VAL-1.0 §7.9, T-77).", nameof(request.FailureReason));
            }

            var handoff = await _dbContext.MdfFinanceHandoffs
                .Include(h => h.ServiceFee)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, ct);

            if (handoff == null)
            {
                throw new KeyNotFoundException($"Data penyerahan dengan ID '{id}' tidak ditemukan.");
            }

            if (request.ExpectedRowVersion.HasValue && handoff.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data penyerahan telah diperbarui oleh proses lain. Silakan muat ulang halaman.");
            }

            // Transisi hanya sah dari Created -> Failed (MDF-STATE-1.0 §5)
            if (handoff.Status != MdfFinanceHandoffStatuses.Created)
            {
                throw new InvalidOperationException($"Penyerahan berstatus '{handoff.Status}' tidak dapat ditolak. Hanya penyerahan berstatus 'Created' yang dapat dinyatakan Failed.");
            }

            var now = DateTime.UtcNow;
            handoff.Status = MdfFinanceHandoffStatuses.Failed;
            handoff.FailureReason = request.FailureReason.Trim();
            handoff.RowVersion = Guid.NewGuid();
            handoff.UpdateBy = actorUserId;
            handoff.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogWarning("Penyerahan hasil jasa ID '{HandoffId}' ditolak oleh Finance. Alasan: {Reason}.",
                handoff.Id, handoff.FailureReason);

            return await GetByIdAsync(id, ct);
        }

        /// <summary>
        /// Mengirim ulang penyerahan yang berstatus Failed setelah penyebabnya diperbaiki — MDF-STATE-1.0 §5, T-76.
        /// INVARIAN KRUSIAL: HandoffKey TIDAK BOLEH BERUBAH saat pengiriman ulang (Rule 7.7, T-76).
        /// </summary>
        public async Task<MedicalFeeHandoffDetailDto> ResendAsync(
            Guid id,
            ResendHandoffRequest? request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var handoff = await _dbContext.MdfFinanceHandoffs
                .Include(h => h.ServiceFee)
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDelete, ct);

            if (handoff == null)
            {
                throw new KeyNotFoundException($"Data penyerahan dengan ID '{id}' tidak ditemukan.");
            }

            if (request?.ExpectedRowVersion.HasValue == true && handoff.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data penyerahan telah diperbarui oleh proses lain. Silakan muat ulang halaman.");
            }

            // Transisi hanya sah dari Failed -> Created (MDF-STATE-1.0 §5)
            if (handoff.Status != MdfFinanceHandoffStatuses.Failed)
            {
                throw new InvalidOperationException($"Penyerahan berstatus '{handoff.Status}' tidak dapat dikirim ulang. Hanya penyerahan berstatus 'Failed' yang dapat dikirim ulang.");
            }

            var now = DateTime.UtcNow;
            // CATATAN KRUSIAL (Rule 7.7, T-76): HandoffKey sengaja DIPERTAHANKAN dan TIDAK dibuat ulang!
            handoff.Status = MdfFinanceHandoffStatuses.Created;
            handoff.FailureReason = null;
            handoff.AcknowledgedAt = null;
            handoff.RowVersion = Guid.NewGuid();
            handoff.UpdateBy = actorUserId;
            handoff.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Penyerahan hasil jasa ID '{HandoffId}' berhasil dikirim ulang ke status Created dengan HandoffKey tetap: {HandoffKey}.",
                handoff.Id, handoff.HandoffKey);

            return await GetByIdAsync(id, ct);
        }

        /// <summary>
        /// Helper pembentukan handoff untuk satu hasil jasa yang sudah Approved (MDF-VAL-1.0 §7.4, §7.5).
        /// Dipanggil dalam transaksi persetujuan periode atau dispatch.
        /// </summary>
        public async Task<MdfFinanceHandoff> CreateHandoffForServiceFeeAsync(
            Guid serviceFeeId,
            Guid correlationId,
            Guid? causationId,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var sf = await _dbContext.MdfServiceFees
                .Include(s => s.Period)
                .FirstOrDefaultAsync(s => s.Id == serviceFeeId && !s.IsDelete, ct);

            if (sf == null)
            {
                throw new KeyNotFoundException($"Hasil jasa ID '{serviceFeeId}' tidak ditemukan.");
            }

            // Validasi status: hanya hasil jasa Approved yang dapat diserahkan (MDF-VAL-1.0 §7.4, T-74)
            if (sf.Status != MdfServiceFeeStatuses.Approved)
            {
                throw new InvalidOperationException($"Hasil jasa '{sf.FeeNumber}' berstatus '{sf.Status}'. Penyerahan ke Finance hanya dapat dibuat untuk hasil jasa yang berstatus 'Approved' (MDF-VAL-1.0 §7.4, T-74).");
            }

            // Validasi keunikan: satu hasil jasa paling banyak satu penyerahan (MDF-VAL-1.0 §7.5, T-73)
            var exists = await _dbContext.MdfFinanceHandoffs
                .AnyAsync(h => h.ServiceFeeId == serviceFeeId && !h.IsDelete, ct);

            if (exists)
            {
                throw new InvalidOperationException($"Hasil jasa '{sf.FeeNumber}' sudah memiliki data penyerahan ke Finance (MDF_HANDOFF_ALREADY_EXISTS, MDF-VAL-1.0 §7.5, T-73).");
            }

            var now = DateTime.UtcNow;
            var handoff = new MdfFinanceHandoff
            {
                Id = Guid.NewGuid(),
                ServiceFeeId = sf.Id,
                HandoffKey = Guid.NewGuid(),
                PayeeType = sf.PayeeType,
                PayeeReferenceId = sf.PayeeReferenceId,
                PeriodCode = sf.Period?.PeriodCode ?? "UNKNOWN",
                GrossAmount = sf.FinalAmount, // Nilai kotor sebelum potongan (MF-DEC-005, T-72)
                Status = MdfFinanceHandoffStatuses.Created,
                CorrelationId = correlationId,
                CausationId = causationId,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.MdfFinanceHandoffs.Add(handoff);

            // Transisi status hasil jasa: Approved -> HandedOff (MDF-STATE-1.0 §2)
            sf.Status = MdfServiceFeeStatuses.HandedOff;
            sf.RowVersion = Guid.NewGuid();
            sf.UpdateBy = actorUserId;
            sf.UpdateDateTime = now;

            return handoff;
        }

        /// <summary>Label penerima sementara (jenis + 8 karakter pertama ID rujukan) sampai nama sungguhan diselesaikan.</summary>
        private static string BuildPayeeLabel(string payeeType, Guid payeeReferenceId) =>
            $"{(payeeType == MdfPayeeTypes.Doctor ? "Dokter" : "Tenaga")} #{payeeReferenceId.ToString()[..8]}";
    }
}
