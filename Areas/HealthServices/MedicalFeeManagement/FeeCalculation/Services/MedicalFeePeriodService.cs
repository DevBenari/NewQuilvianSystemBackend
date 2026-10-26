using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Service pengelola siklus hidup periode perhitungan jasa medis (MdfFeePeriod)
    /// sesuai spesifikasi MDF-DES-005, 010, 012, 015, 016, 017 dan matriks status MDF-STATE-1.0 §1.
    /// Mengatur transisi Open -> Calculated -> Verified -> Approved -> Closed dan Reopen / Return.
    /// </summary>
    public class MedicalFeePeriodService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MedicalFeePeriodService> _logger;

        public MedicalFeePeriodService(
            ApplicationDbContext dbContext,
            ILogger<MedicalFeePeriodService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // =====================================================================
        // Query Operations
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter untuk tabel periode kalkulasi.
        /// </summary>
        public FeePeriodFilterMetadataResponse GetFilterMetadata()
        {
            return new FeePeriodFilterMetadataResponse
            {
                DefaultFilter = new FeePeriodDefaultFilterResponse(),
                SortOptions = new List<FeePeriodSortOptionResponse>
                {
                    new() { Value = "PeriodStart", Label = "Tanggal Awal" },
                    new() { Value = "PeriodEnd", Label = "Tanggal Akhir" },
                    new() { Value = "PeriodCode", Label = "Kode Periode" },
                    new() { Value = "Status", Label = "Status" },
                    new() { Value = "CreatedAt", Label = "Waktu Pembuatan" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                StatusOptions = new List<string>
                {
                    MdfFeePeriodStatuses.Open,
                    MdfFeePeriodStatuses.Calculated,
                    MdfFeePeriodStatuses.Verified,
                    MdfFeePeriodStatuses.Approved,
                    MdfFeePeriodStatuses.Closed
                },
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Mengambil ringkasan statistik periode perhitungan (Total, Open, Calculated, Verified, Approved, Closed).
        /// </summary>
        public async Task<FeePeriodSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var activePeriods = _dbContext.MdfFeePeriods.AsNoTracking().Where(p => !p.IsDelete);

            var total = await activePeriods.CountAsync(ct);
            var open = await activePeriods.CountAsync(p => p.Status == MdfFeePeriodStatuses.Open, ct);
            var calculated = await activePeriods.CountAsync(p => p.Status == MdfFeePeriodStatuses.Calculated, ct);
            var verified = await activePeriods.CountAsync(p => p.Status == MdfFeePeriodStatuses.Verified, ct);
            var approved = await activePeriods.CountAsync(p => p.Status == MdfFeePeriodStatuses.Approved, ct);
            var closed = await activePeriods.CountAsync(p => p.Status == MdfFeePeriodStatuses.Closed, ct);

            return new FeePeriodSummaryResponse
            {
                TotalPeriods = total,
                OpenPeriods = open,
                CalculatedPeriods = calculated,
                VerifiedPeriods = verified,
                ApprovedPeriods = approved,
                ClosedPeriods = closed
            };
        }

        /// <summary>
        /// Mengambil daftar periode perhitungan secara terpaginasi dengan agregasi angka ringkas.
        /// </summary>
        public async Task<PagedResult<FeePeriodHeaderResponse>> GetPagedAsync(
            FeePeriodPagedQuery query,
            CancellationToken ct = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : (query.PageSize > 200 ? 200 : query.PageSize);

            var q = _dbContext.MdfFeePeriods
                .AsNoTracking()
                .Where(p => !p.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                q = q.Where(p => p.PeriodCode.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(p => p.Status == query.Status);
            }

            if (query.Year.HasValue)
            {
                q = q.Where(p => p.PeriodStart.Year == query.Year.Value);
            }

            var totalData = await q.CountAsync(ct);

            var sortDesc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "periodcode" => sortDesc ? q.OrderByDescending(p => p.PeriodCode) : q.OrderBy(p => p.PeriodCode),
                "periodend" => sortDesc ? q.OrderByDescending(p => p.PeriodEnd) : q.OrderBy(p => p.PeriodEnd),
                "status" => sortDesc ? q.OrderByDescending(p => p.Status) : q.OrderBy(p => p.Status),
                "createdat" => sortDesc ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
                _ => sortDesc ? q.OrderByDescending(p => p.PeriodStart) : q.OrderBy(p => p.PeriodStart),
            };

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new FeePeriodHeaderResponse
                {
                    Id = p.Id,
                    PeriodCode = p.PeriodCode,
                    PeriodStart = p.PeriodStart,
                    PeriodEnd = p.PeriodEnd,
                    Status = p.Status,
                    CalculatedAt = p.CalculatedAt,
                    VerifiedAt = p.VerifiedAt,
                    ApprovedAt = p.ApprovedAt,
                    ClosedAt = p.ClosedAt,
                    PayeeCount = p.ServiceFees.Where(sf => !sf.IsDelete).Select(sf => sf.PayeeReferenceId).Distinct().Count(),
                    GrossTotal = p.ServiceFees.Where(sf => !sf.IsDelete).Sum(sf => (decimal?)sf.GrossAmount) ?? 0m,
                    UnresolvedCount = p.UnresolvedServices.Where(u => !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open).Count(),
                    RowVersion = p.RowVersion
                })
                .ToListAsync(ct);

            return new PagedResult<FeePeriodHeaderResponse>(items, totalData, pageNumber, pageSize);
        }

        /// <summary>
        /// Mengambil detail lengkap satu periode beserta rincian statistik dan layanan unresolved.
        /// </summary>
        public async Task<FeePeriodDetailResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            var serviceFees = await _dbContext.MdfServiceFees
                .AsNoTracking()
                .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                .ToListAsync(ct);

            var payeeCount = serviceFees.Select(sf => sf.PayeeReferenceId).Distinct().Count();
            var grossTotal = serviceFees.Sum(sf => sf.GrossAmount);
            var adjustmentTotal = serviceFees.Sum(sf => sf.AdjustmentAmount);
            var finalTotal = serviceFees.Sum(sf => sf.FinalAmount);

            var detailCount = await _dbContext.MdfServiceFeeDetails
                .AsNoTracking()
                .Where(d => d.PeriodId == id && !d.IsDelete)
                .CountAsync(ct);

            var unresolvedList = await _dbContext.MdfUnresolvedServices
                .AsNoTracking()
                .Where(u => u.PeriodId == id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open)
                .ToListAsync(ct);

            var unresolvedByReason = unresolvedList
                .GroupBy(u => u.Reason)
                .ToDictionary(g => g.Key, g => g.Count());

            return new FeePeriodDetailResponse
            {
                Id = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodStart = period.PeriodStart,
                PeriodEnd = period.PeriodEnd,
                Status = period.Status,
                CalculatedAt = period.CalculatedAt,
                CalculatedBy = period.CalculatedBy,
                VerifiedAt = period.VerifiedAt,
                VerifiedBy = period.VerifiedBy,
                ApprovedAt = period.ApprovedAt,
                ApprovedBy = period.ApprovedBy,
                ClosedAt = period.ClosedAt,
                ClosedBy = period.ClosedBy,
                PayeeCount = payeeCount,
                DetailCount = detailCount,
                GrossTotal = grossTotal,
                AdjustmentTotal = adjustmentTotal,
                FinalTotal = finalTotal,
                UnresolvedCount = unresolvedList.Count,
                UnresolvedByReason = unresolvedByReason,
                RowVersion = period.RowVersion
            };
        }

        // =====================================================================
        // Lifecycle Operations (State Machine)
        // =====================================================================

        /// <summary>
        /// Membuka periode kalkulasi baru (Status = Open).
        /// </summary>
        public async Task<FeePeriodHeaderResponse> CreatePeriodAsync(
            CreateFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            if (request.PeriodEnd < request.PeriodStart)
            {
                throw new InvalidOperationException("Tanggal akhir periode (PeriodEnd) tidak boleh lebih awal dari tanggal awal (PeriodStart).");
            }

            var normalizedCode = request.PeriodCode.Trim();
            var exists = await _dbContext.MdfFeePeriods
                .AnyAsync(p => p.PeriodCode.ToLower() == normalizedCode.ToLower() && !p.IsDelete, ct);

            if (exists)
            {
                throw new InvalidOperationException($"Kode periode '{normalizedCode}' sudah terdaftar.");
            }

            var period = new MdfFeePeriod
            {
                Id = Guid.NewGuid(),
                PeriodCode = normalizedCode,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                Status = MdfFeePeriodStatuses.Open,
                RowVersion = Guid.NewGuid(),
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MdfFeePeriods.Add(period);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil dibuat oleh user '{UserId}'.",
                period.PeriodCode, period.Id, userId);

            return new FeePeriodHeaderResponse
            {
                Id = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodStart = period.PeriodStart,
                PeriodEnd = period.PeriodEnd,
                Status = period.Status,
                PayeeCount = 0,
                GrossTotal = 0m,
                UnresolvedCount = 0,
                RowVersion = period.RowVersion
            };
        }

        /// <summary>
        /// Membuka kembali periode dari status Calculated ke Open (MDF-DES-015, MDF-STATE-1.0).
        /// Membersihkan rincian layanan dan daftar unresolved berstatus Open.
        /// </summary>
        public async Task<FeePeriodDetailResponse> ReopenPeriodAsync(
            Guid id,
            ReopenFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            if (period.Status != MdfFeePeriodStatuses.Calculated)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat dibuka kembali. Hanya periode berstatus 'Calculated' yang dapat dibuka kembali.");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            try
            {
                // Soft-delete rincian perhitungan lama (MdfServiceFeeDetail)
                var oldDetails = await _dbContext.MdfServiceFeeDetails
                    .Where(d => d.PeriodId == id && !d.IsDelete)
                    .ToListAsync(ct);
                foreach (var d in oldDetails)
                {
                    d.IsDelete = true;
                    d.DeletedBy = userId.ToString();
                    d.DeletedAt = DateTime.UtcNow;
                }

                // Soft-delete ringkasan hasil jasa (MdfServiceFee)
                var oldFees = await _dbContext.MdfServiceFees
                    .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                    .ToListAsync(ct);
                foreach (var sf in oldFees)
                {
                    sf.IsDelete = true;
                    sf.DeletedBy = userId.ToString();
                    sf.DeletedAt = DateTime.UtcNow;
                }

                // Soft-delete unresolved services yang masih Open
                var openUnresolved = await _dbContext.MdfUnresolvedServices
                    .Where(u => u.PeriodId == id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open)
                    .ToListAsync(ct);
                foreach (var u in openUnresolved)
                {
                    u.IsDelete = true;
                    u.DeletedBy = userId.ToString();
                    u.DeletedAt = DateTime.UtcNow;
                }

                // Kembalikan status periode ke Open
                period.Status = MdfFeePeriodStatuses.Open;
                period.CalculatedAt = null;
                period.CalculatedBy = null;
                period.RowVersion = Guid.NewGuid();
                period.UpdatedBy = userId.ToString();
                period.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil dibuka kembali ke status Open oleh user '{UserId}'.",
                    period.PeriodCode, period.Id, userId);

                return await GetByIdAsync(id, ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Memverifikasi periode kalkulasi: Calculated -> Verified (MDF-STATE-1.0 §1).
        /// </summary>
        public async Task<FeePeriodDetailResponse> VerifyPeriodAsync(
            Guid id,
            VerifyFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            if (period.Status != MdfFeePeriodStatuses.Calculated)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat diverifikasi. Hanya periode berstatus 'Calculated' yang dapat diverifikasi.");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                // Update status seluruh MdfServiceFee ke Verified
                var fees = await _dbContext.MdfServiceFees
                    .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                    .ToListAsync(ct);

                foreach (var sf in fees)
                {
                    sf.Status = MdfServiceFeeStatuses.Verified;
                    sf.VerifiedBy = userId;
                    sf.VerifiedAt = now;
                    sf.RowVersion = Guid.NewGuid();
                    sf.UpdatedBy = userId.ToString();
                    sf.UpdatedAt = now;
                }

                // Update periode
                period.Status = MdfFeePeriodStatuses.Verified;
                period.VerifiedBy = userId;
                period.VerifiedAt = now;
                period.RowVersion = Guid.NewGuid();
                period.UpdatedBy = userId.ToString();
                period.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil diverifikasi oleh user '{UserId}'.",
                    period.PeriodCode, period.Id, userId);

                return await GetByIdAsync(id, ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Mengembalikan periode yang telah diverifikasi kembali ke status Calculated: Verified -> Calculated (MDF-STATE-1.0 §1).
        /// </summary>
        public async Task<FeePeriodDetailResponse> ReturnPeriodAsync(
            Guid id,
            ReturnFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            if (period.Status != MdfFeePeriodStatuses.Verified)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat dikembalikan. Hanya periode berstatus 'Verified' yang dapat dikembalikan.");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                // Turunkan status seluruh MdfServiceFee kembali ke Calculated
                var fees = await _dbContext.MdfServiceFees
                    .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                    .ToListAsync(ct);

                foreach (var sf in fees)
                {
                    sf.Status = MdfServiceFeeStatuses.Calculated;
                    sf.VerifiedBy = null;
                    sf.VerifiedAt = null;
                    sf.RowVersion = Guid.NewGuid();
                    sf.UpdatedBy = userId.ToString();
                    sf.UpdatedAt = now;
                }

                // Update status periode kembali ke Calculated
                period.Status = MdfFeePeriodStatuses.Calculated;
                period.VerifiedBy = null;
                period.VerifiedAt = null;
                period.RowVersion = Guid.NewGuid();
                period.UpdatedBy = userId.ToString();
                period.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) dikembalikan ke status Calculated oleh user '{UserId}'. Alasan: {Reason}",
                    period.PeriodCode, period.Id, userId, request.Reason);

                return await GetByIdAsync(id, ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Menyetujui periode kalkulasi: Verified -> Approved (MDF-DES-017, MDF-STATE-1.0 §1).
        /// Menerapkan kontrol Maker-Checker: ApprovedBy wajib berbeda dari VerifiedBy.
        /// </summary>
        public async Task<FeePeriodDetailResponse> ApprovePeriodAsync(
            Guid id,
            ApproveFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            if (period.Status != MdfFeePeriodStatuses.Verified)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat disetujui. Hanya periode berstatus 'Verified' yang dapat disetujui.");
            }

            // Maker-Checker Check (MDF-DES-017)
            if (period.VerifiedBy.HasValue && period.VerifiedBy.Value == userId)
            {
                throw new InvalidOperationException("Penyetuju (ApprovedBy) tidak boleh sama dengan pemverifikasi (VerifiedBy) — pelanggaran aturan Maker-Checker (MDF-DES-017).");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var now = DateTime.UtcNow;

                // Update status seluruh MdfServiceFee ke Approved
                var fees = await _dbContext.MdfServiceFees
                    .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                    .ToListAsync(ct);

                foreach (var sf in fees)
                {
                    sf.Status = MdfServiceFeeStatuses.Approved;
                    sf.ApprovedBy = userId;
                    sf.ApprovedAt = now;
                    sf.RowVersion = Guid.NewGuid();
                    sf.UpdatedBy = userId.ToString();
                    sf.UpdatedAt = now;
                }

                // Update periode ke Approved
                period.Status = MdfFeePeriodStatuses.Approved;
                period.ApprovedBy = userId;
                period.ApprovedAt = now;
                period.RowVersion = Guid.NewGuid();
                period.UpdatedBy = userId.ToString();
                period.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) disetujui oleh user '{UserId}'.",
                    period.PeriodCode, period.Id, userId);

                return await GetByIdAsync(id, ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        /// <summary>
        /// Menutup periode kalkulasi: Approved -> Closed (MDF-DES-016, MDF-STATE-1.0 §1).
        /// Memvalidasi bahwa tidak ada layanan unresolved berstatus Open (MDF_PERIOD_HAS_UNRESOLVED).
        /// Setelah Closed, angka terkunci permanen dan tidak dapat dibuka kembali.
        /// </summary>
        public async Task<FeePeriodDetailResponse> ClosePeriodAsync(
            Guid id,
            CloseFeePeriodRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{id}' tidak ditemukan.");
            }

            if (period.Status != MdfFeePeriodStatuses.Approved)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat ditutup. Hanya periode berstatus 'Approved' yang dapat ditutup.");
            }

            // Validasi Penutupan Periode: Tidak boleh ada MdfUnresolvedService berstatus Open (MDF-DES-016)
            var openUnresolvedCount = await _dbContext.MdfUnresolvedServices
                .CountAsync(u => u.PeriodId == id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open, ct);

            if (openUnresolvedCount > 0)
            {
                throw new InvalidOperationException($"Periode tidak dapat ditutup: {openUnresolvedCount} layanan belum dapat dihitung jasanya (MDF_PERIOD_HAS_UNRESOLVED). Selesaikan atau diskualifikasi layanan terlebih dahulu.");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            var now = DateTime.UtcNow;
            period.Status = MdfFeePeriodStatuses.Closed;
            period.ClosedBy = userId;
            period.ClosedAt = now;
            period.RowVersion = Guid.NewGuid();
            period.UpdatedBy = userId.ToString();
            period.UpdatedAt = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil ditutup secara permanen oleh user '{UserId}'.",
                period.PeriodCode, period.Id, userId);

            return await GetByIdAsync(id, ct);
        }
    }
}

