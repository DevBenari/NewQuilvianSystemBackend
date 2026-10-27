using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
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
                "createdat" => sortDesc ? q.OrderByDescending(p => p.CreateDateTime) : q.OrderBy(p => p.CreateDateTime),
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

            return new PagedResult<FeePeriodHeaderResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
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
                .Where(d => d.ServiceFee!.PeriodId == id && !d.IsDelete)
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
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
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
                    .Where(d => d.ServiceFee!.PeriodId == id && !d.IsDelete)
                    .ToListAsync(ct);
                foreach (var d in oldDetails)
                {
                    d.IsDelete = true;
                    d.DeleteBy = userId;
                    d.DeleteDateTime = DateTime.UtcNow;
                }

                // Soft-delete ringkasan hasil jasa (MdfServiceFee)
                var oldFees = await _dbContext.MdfServiceFees
                    .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                    .ToListAsync(ct);
                foreach (var sf in oldFees)
                {
                    sf.IsDelete = true;
                    sf.DeleteBy = userId;
                    sf.DeleteDateTime = DateTime.UtcNow;
                }

                // Soft-delete unresolved services yang masih Open
                var openUnresolved = await _dbContext.MdfUnresolvedServices
                    .Where(u => u.PeriodId == id && !u.IsDelete && u.Status == MdfUnresolvedStatuses.Open)
                    .ToListAsync(ct);
                foreach (var u in openUnresolved)
                {
                    u.IsDelete = true;
                    u.DeleteBy = userId;
                    u.DeleteDateTime = DateTime.UtcNow;
                }

                // Kembalikan status periode ke Open
                period.Status = MdfFeePeriodStatuses.Open;
                period.CalculatedAt = null;
                period.CalculatedBy = null;
                period.RowVersion = Guid.NewGuid();
                period.UpdateBy = userId;
                period.UpdateDateTime = DateTime.UtcNow;

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
        /// Memverifikasi periode kalkulasi: Calculated -> Verified (MDF-STATE-1.0 §1, MDF-VAL-1.0 §4.7, T-55).
        /// Prasyarat mutlak: Seluruh MdfServiceFee pada periode tersebut wajib sudah berstatus 'Verified'.
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
                throw new InvalidOperationException($"Periode berstatus '{period.Status}' tidak dapat diverifikasi. Hanya periode berstatus 'Calculated' yang dapat diverifikasi (MDF-STATE-1.0 §1).");
            }

            if (request.ExpectedRowVersion.HasValue && period.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data periode telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            // Validasi Rule 4.7 (T-55): Seluruh MdfServiceFee periode itu sudah Verified
            var fees = await _dbContext.MdfServiceFees
                .Where(sf => sf.PeriodId == id && !sf.IsDelete)
                .ToListAsync(ct);

            if (fees.Count == 0)
            {
                throw new InvalidOperationException("Periode tidak memiliki hasil jasa kalkulasi untuk diverifikasi.");
            }

            var unverifiedCount = fees.Count(sf => sf.Status == MdfServiceFeeStatuses.Calculated);
            if (unverifiedCount > 0)
            {
                throw new InvalidOperationException(
                    $"Periode tidak dapat diverifikasi: terdapat {unverifiedCount} hasil jasa penerima yang masih berstatus 'Calculated' (MDF-VAL-1.0 §4.7). Seluruh hasil jasa wajib diverifikasi terlebih dahulu sebelum periode dapat diverifikasi.");
            }

            var now = DateTime.UtcNow;
            period.Status = MdfFeePeriodStatuses.Verified;
            period.VerifiedBy = userId;
            period.VerifiedAt = now;
            period.RowVersion = Guid.NewGuid();
            period.UpdateBy = userId;
            period.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil diverifikasi oleh user '{UserId}'.",
                period.PeriodCode, period.Id, userId);

            return await GetByIdAsync(id, ct);
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
                    sf.UpdateBy = userId;
                    sf.UpdateDateTime = now;
                }

                // Update status periode kembali ke Calculated
                period.Status = MdfFeePeriodStatuses.Calculated;
                period.VerifiedBy = null;
                period.VerifiedAt = null;
                period.RowVersion = Guid.NewGuid();
                period.UpdateBy = userId;
                period.UpdateDateTime = now;

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
                    if (sf.VerifiedBy.HasValue && sf.VerifiedBy.Value == userId)
                    {
                        throw new InvalidOperationException($"Penyetuju (ApprovedBy) tidak boleh sama dengan pemverifikasi (VerifiedBy) pada hasil jasa penerima '{sf.FeeNumber}' — pelanggaran aturan Maker-Checker (MDF-DES-017, T-59).");
                    }

                    sf.Status = MdfServiceFeeStatuses.Approved;
                    sf.ApprovedBy = userId;
                    sf.ApprovedAt = now;
                    sf.RowVersion = Guid.NewGuid();
                    sf.UpdateBy = userId;
                    sf.UpdateDateTime = now;

                    // MDF-DES-018, T-71, T-78, MDF-STATE-1.0 §1 & §2:
                    // Penyerahan ke Finance dibuat di transaksi yang sama persis dengan persetujuan periode.
                    // Nilai penyerahan adalah nilai kotor (GrossAmount = FinalAmount, MF-DEC-005, T-72).
                    // ServiceFee status otomatis bertransisi Approved -> HandedOff.
                    var handoff = new MdfFinanceHandoff
                    {
                        Id = Guid.NewGuid(),
                        ServiceFeeId = sf.Id,
                        HandoffKey = Guid.NewGuid(),
                        PayeeType = sf.PayeeType,
                        PayeeReferenceId = sf.PayeeReferenceId,
                        PeriodCode = period.PeriodCode,
                        GrossAmount = sf.FinalAmount,
                        Status = MdfFinanceHandoffStatuses.Created,
                        CorrelationId = Guid.NewGuid(),
                        CausationId = period.Id,
                        RowVersion = Guid.NewGuid(),
                        CreateBy = userId,
                        CreateDateTime = now
                    };
                    _dbContext.MdfFinanceHandoffs.Add(handoff);
                    sf.Status = MdfServiceFeeStatuses.HandedOff;
                }

                // Update periode ke Approved
                period.Status = MdfFeePeriodStatuses.Approved;
                period.ApprovedBy = userId;
                period.ApprovedAt = now;
                period.RowVersion = Guid.NewGuid();
                period.UpdateBy = userId;
                period.UpdateDateTime = now;

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
            period.UpdateBy = userId;
            period.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Periode kalkulasi '{PeriodCode}' (ID: {PeriodId}) berhasil ditutup secara permanen oleh user '{UserId}'.",
                period.PeriodCode, period.Id, userId);

            return await GetByIdAsync(id, ct);
        }

        // =========================================================================
        // Hasil Jasa Medis (MdfServiceFee) — MDF-API-1.0 §5, MDF-STATE-1.0 §2, MDF-VAL-1.0 §4.7
        // =========================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, opsi pengurutan, dan opsi status untuk daftar hasil jasa medis.
        /// </summary>
        public ServiceFeeFilterMetadataResponse GetServiceFeeFilterMetadata()
        {
            return new ServiceFeeFilterMetadataResponse
            {
                DefaultFilter = new ServiceFeeDefaultFilterResponse
                {
                    SortBy = "GrossAmount",
                    SortDirection = "desc",
                    PageNumber = 1,
                    PageSize = 25
                },
                SortOptions = new List<ServiceFeeSortOptionResponse>
                {
                    new() { Value = "GrossAmount", Label = "Nilai Kotor Jasa" },
                    new() { Value = "FinalAmount", Label = "Nilai Akhir Jasa" },
                    new() { Value = "FeeNumber", Label = "Nomor Hasil Jasa" },
                    new() { Value = "Status", Label = "Status" },
                    new() { Value = "CalculatedAt", Label = "Tanggal Perhitungan" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                StatusOptions = new List<string>
                {
                    MdfServiceFeeStatuses.Calculated,
                    MdfServiceFeeStatuses.Verified,
                    MdfServiceFeeStatuses.Approved,
                    MdfServiceFeeStatuses.HandedOff
                },
                PayeeTypeOptions = new List<string>
                {
                    MdfPayeeTypes.Doctor,
                    MdfPayeeTypes.Workforce
                },
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Mengambil daftar hasil perhitungan jasa medis (MdfServiceFee) terpaginasi dengan filter periodId, payeeType, status, dan pencarian.
        /// Nama penerima jasa diperkaya secara polimorfik dari master data (MstDoctor atau MstWorkforceProfile).
        /// </summary>
        public async Task<PagedResult<ServiceFeeHeaderResponse>> GetServiceFeesPagedAsync(
            ServiceFeePagedQuery query,
            CancellationToken ct = default)
        {
            var q = _dbContext.MdfServiceFees
                .AsNoTracking()
                .Include(sf => sf.Period)
                .Where(sf => !sf.IsDelete);

            if (query.PeriodId.HasValue)
            {
                q = q.Where(sf => sf.PeriodId == query.PeriodId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.PayeeType))
            {
                q = q.Where(sf => sf.PayeeType == query.PayeeType.Trim());
            }

            if (query.PayeeReferenceId.HasValue)
            {
                q = q.Where(sf => sf.PayeeReferenceId == query.PayeeReferenceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(sf => sf.Status == query.Status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                q = q.Where(sf => EF.Functions.ILike(sf.FeeNumber, $"%{search}%"));
            }

            var totalItems = await q.CountAsync(ct);

            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 25 : query.PageSize;

            bool isAsc = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            q = (query.SortBy?.ToLowerInvariant(), isAsc) switch
            {
                ("feenumber", true) => q.OrderBy(sf => sf.FeeNumber),
                ("feenumber", false) => q.OrderByDescending(sf => sf.FeeNumber),
                ("finalamount", true) => q.OrderBy(sf => sf.FinalAmount),
                ("finalamount", false) => q.OrderByDescending(sf => sf.FinalAmount),
                ("status", true) => q.OrderBy(sf => sf.Status),
                ("status", false) => q.OrderByDescending(sf => sf.Status),
                ("calculatedat", true) => q.OrderBy(sf => sf.CalculatedAt),
                ("calculatedat", false) => q.OrderByDescending(sf => sf.CalculatedAt),
                ("grossamount", true) => q.OrderBy(sf => sf.GrossAmount),
                _ => q.OrderByDescending(sf => sf.GrossAmount)
            };

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sf => new ServiceFeeHeaderResponse
                {
                    Id = sf.Id,
                    FeeNumber = sf.FeeNumber,
                    PeriodId = sf.PeriodId,
                    PeriodCode = sf.Period != null ? sf.Period.PeriodCode : string.Empty,
                    PayeeType = sf.PayeeType,
                    PayeeReferenceId = sf.PayeeReferenceId,
                    PayeeName = string.Empty,
                    GrossAmount = sf.GrossAmount,
                    AdjustmentAmount = sf.AdjustmentAmount,
                    FinalAmount = sf.FinalAmount,
                    Status = sf.Status,
                    CalculatedAt = sf.CalculatedAt,
                    VerifiedAt = sf.VerifiedAt,
                    VerifiedBy = sf.VerifiedBy,
                    ApprovedAt = sf.ApprovedAt,
                    ApprovedBy = sf.ApprovedBy,
                    DetailCount = _dbContext.MdfServiceFeeDetails.Count(d => d.ServiceFeeId == sf.Id && !d.IsDelete),
                    AdjustmentCount = _dbContext.MdfFeeAdjustments.Count(a => a.ServiceFeeId == sf.Id && !a.IsDelete),
                    RowVersion = sf.RowVersion
                })
                .ToListAsync(ct);

            await EnrichPayeeNamesAsync(items, ct);

            return new PagedResult<ServiceFeeHeaderResponse>(items, totalItems, pageNumber, pageSize);
        }

        /// <summary>
        /// Mengambil rincian header satu hasil jasa medis berdasarkan ID.
        /// </summary>
        public async Task<ServiceFeeHeaderResponse> GetServiceFeeByIdAsync(Guid id, CancellationToken ct = default)
        {
            var sf = await _dbContext.MdfServiceFees
                .AsNoTracking()
                .Include(f => f.Period)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDelete, ct);

            if (sf == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{id}' tidak ditemukan.");
            }

            var response = new ServiceFeeHeaderResponse
            {
                Id = sf.Id,
                FeeNumber = sf.FeeNumber,
                PeriodId = sf.PeriodId,
                PeriodCode = sf.Period != null ? sf.Period.PeriodCode : string.Empty,
                PayeeType = sf.PayeeType,
                PayeeReferenceId = sf.PayeeReferenceId,
                PayeeName = string.Empty,
                GrossAmount = sf.GrossAmount,
                AdjustmentAmount = sf.AdjustmentAmount,
                FinalAmount = sf.FinalAmount,
                Status = sf.Status,
                CalculatedAt = sf.CalculatedAt,
                VerifiedAt = sf.VerifiedAt,
                VerifiedBy = sf.VerifiedBy,
                ApprovedAt = sf.ApprovedAt,
                ApprovedBy = sf.ApprovedBy,
                DetailCount = await _dbContext.MdfServiceFeeDetails.CountAsync(d => d.ServiceFeeId == sf.Id && !d.IsDelete, ct),
                AdjustmentCount = await _dbContext.MdfFeeAdjustments.CountAsync(a => a.ServiceFeeId == sf.Id && !a.IsDelete, ct),
                RowVersion = sf.RowVersion
            };

            var list = new List<ServiceFeeHeaderResponse> { response };
            await EnrichPayeeNamesAsync(list, ct);
            return response;
        }

        /// <summary>
        /// Mengambil rincian per layanan/tindakan (MdfServiceFeeDetail) untuk satu hasil jasa medis.
        /// Menjawab pertanyaan audit "Dari mana angkanya" (MDF-API-1.0 §5, MDF-DES-010).
        /// </summary>
        public async Task<List<ServiceFeeDetailBreakdownResponse>> GetServiceFeeDetailsAsync(Guid serviceFeeId, CancellationToken ct = default)
        {
            var exists = await _dbContext.MdfServiceFees
                .AsNoTracking()
                .AnyAsync(sf => sf.Id == serviceFeeId && !sf.IsDelete, ct);

            if (!exists)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{serviceFeeId}' tidak ditemukan.");
            }

            var details = await _dbContext.MdfServiceFeeDetails
                .AsNoTracking()
                .Include(d => d.Tariff)
                .Include(d => d.Role)
                .Include(d => d.SharingRule)
                    .ThenInclude(r => r!.Agreement)
                .Where(d => d.ServiceFeeId == serviceFeeId && !d.IsDelete)
                .OrderBy(d => d.ServiceDate)
                .ThenBy(d => d.SourceDetailId)
                .Select(d => new ServiceFeeDetailBreakdownResponse
                {
                    Id = d.Id,
                    ServiceFeeId = d.ServiceFeeId,
                    SourceDomain = d.SourceDomain,
                    SourceDetailId = d.SourceDetailId,
                    InvoiceItemId = d.InvoiceItemId,
                    ServiceDate = d.ServiceDate,
                    TariffId = d.TariffId,
                    TariffName = d.Tariff != null ? d.Tariff.TariffName : null,
                    RoleId = d.RoleId,
                    RoleCode = d.Role != null ? d.Role.RoleCode : string.Empty,
                    RoleName = d.Role != null ? d.Role.RoleName : null,
                    BaseAmount = d.BaseAmount,
                    SharingPercentage = d.SharingPercentage,
                    CalculatedAmount = d.CalculatedAmount,
                    SharingRuleId = d.SharingRuleId,
                    AgreementNumber = d.SharingRule != null && d.SharingRule.Agreement != null ? d.SharingRule.Agreement.AgreementNumber : string.Empty
                })
                .ToListAsync(ct);

            return details;
        }

        /// <summary>
        /// Memverifikasi satu hasil jasa medis: Calculated -> Verified (MDF-STATE-1.0 §2, MDF-VAL-1.0 §4.7).
        /// </summary>
        public async Task<ServiceFeeHeaderResponse> VerifyServiceFeeAsync(
            Guid id,
            VerifyServiceFeeRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var sf = await _dbContext.MdfServiceFees
                .Include(f => f.Period)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDelete, ct);

            if (sf == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{id}' tidak ditemukan.");
            }

            if (sf.Period != null && sf.Period.Status == MdfFeePeriodStatuses.Closed)
            {
                throw new InvalidOperationException("Periode kalkulasi sudah ditutup secara permanen (Closed). Hasil jasa tidak dapat diverifikasi (MDF-VAL-1.0 §4.11, T-52).");
            }

            if (sf.Status != MdfServiceFeeStatuses.Calculated)
            {
                throw new InvalidOperationException($"Hasil jasa berstatus '{sf.Status}' tidak dapat diverifikasi. Hanya hasil jasa berstatus 'Calculated' yang dapat diverifikasi (MDF-STATE-1.0 §2).");
            }

            if (request.ExpectedRowVersion.HasValue && sf.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data hasil jasa telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            var now = DateTime.UtcNow;
            sf.Status = MdfServiceFeeStatuses.Verified;
            sf.VerifiedBy = userId;
            sf.VerifiedAt = now;
            sf.RowVersion = Guid.NewGuid();
            sf.UpdateBy = userId;
            sf.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Hasil jasa '{FeeNumber}' (ID: {FeeId}) berhasil diverifikasi oleh user '{UserId}'.",
                sf.FeeNumber, sf.Id, userId);

            return await GetServiceFeeByIdAsync(id, ct);
        }

        /// <summary>
        /// Mengembalikan status satu hasil jasa medis: Verified -> Calculated (MDF-STATE-1.0 §2).
        /// Jika periode induk berstatus Verified, status periode otomatis diturunkan kembali ke Calculated (MDF-VAL-1.0 §4.7).
        /// </summary>
        public async Task<ServiceFeeHeaderResponse> ReturnServiceFeeAsync(
            Guid id,
            ReturnServiceFeeRequest request,
            Guid userId,
            CancellationToken ct = default)
        {
            var sf = await _dbContext.MdfServiceFees
                .Include(f => f.Period)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDelete, ct);

            if (sf == null)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{id}' tidak ditemukan.");
            }

            if (sf.Period != null && (sf.Period.Status == MdfFeePeriodStatuses.Closed || sf.Period.Status == MdfFeePeriodStatuses.Approved))
            {
                throw new InvalidOperationException($"Periode kalkulasi berstatus '{sf.Period.Status}'. Hasil jasa tidak dapat dikembalikan ke status Calculated (MDF-VAL-1.0 §4.11, §6.4).");
            }

            if (sf.Status != MdfServiceFeeStatuses.Verified)
            {
                throw new InvalidOperationException($"Hasil jasa berstatus '{sf.Status}' tidak dapat dikembalikan. Hanya hasil jasa berstatus 'Verified' yang dapat dikembalikan ke 'Calculated' (MDF-STATE-1.0 §2).");
            }

            if (request.ExpectedRowVersion.HasValue && sf.RowVersion != request.ExpectedRowVersion.Value)
            {
                throw new DbUpdateConcurrencyException("Data hasil jasa telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.");
            }

            var now = DateTime.UtcNow;
            sf.Status = MdfServiceFeeStatuses.Calculated;
            sf.VerifiedBy = null;
            sf.VerifiedAt = null;
            sf.RowVersion = Guid.NewGuid();
            sf.UpdateBy = userId;
            sf.UpdateDateTime = now;

            // Jika periode induk sudah berstatus Verified, kembalikan status periode ke Calculated karena ada item yang kembali ke Calculated (MDF-VAL-1.0 §4.7)
            if (sf.Period != null && sf.Period.Status == MdfFeePeriodStatuses.Verified)
            {
                sf.Period.Status = MdfFeePeriodStatuses.Calculated;
                sf.Period.VerifiedBy = null;
                sf.Period.VerifiedAt = null;
                sf.Period.RowVersion = Guid.NewGuid();
                sf.Period.UpdateBy = userId;
                sf.Period.UpdateDateTime = now;
            }

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Hasil jasa '{FeeNumber}' (ID: {FeeId}) dikembalikan ke status Calculated oleh user '{UserId}'. Alasan: {Reason}",
                sf.FeeNumber, sf.Id, userId, request.Reason);

            return await GetServiceFeeByIdAsync(id, ct);
        }

        /// <summary>
        /// Melakukan verifikasi massal terhadap seluruh hasil jasa medis berstatus 'Calculated' pada satu periode.
        /// </summary>
        public async Task<BatchVerifyResultResponse> BatchVerifyServiceFeesAsync(
            Guid periodId,
            Guid userId,
            CancellationToken ct = default)
        {
            var period = await _dbContext.MdfFeePeriods
                .FirstOrDefaultAsync(p => p.Id == periodId && !p.IsDelete, ct);

            if (period == null)
            {
                throw new KeyNotFoundException($"Periode perhitungan dengan ID '{periodId}' tidak ditemukan.");
            }

            if (period.Status == MdfFeePeriodStatuses.Closed || period.Status == MdfFeePeriodStatuses.Approved)
            {
                throw new InvalidOperationException($"Periode berstatus '{period.Status}'. Hasil jasa tidak dapat diverifikasi (MDF-VAL-1.0 §4.11).");
            }

            var fees = await _dbContext.MdfServiceFees
                .Where(sf => sf.PeriodId == periodId && !sf.IsDelete)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;
            int verifiedCount = 0;
            int alreadyVerifiedCount = 0;

            foreach (var sf in fees)
            {
                if (sf.Status == MdfServiceFeeStatuses.Calculated)
                {
                    sf.Status = MdfServiceFeeStatuses.Verified;
                    sf.VerifiedBy = userId;
                    sf.VerifiedAt = now;
                    sf.RowVersion = Guid.NewGuid();
                    sf.UpdateBy = userId;
                    sf.UpdateDateTime = now;
                    verifiedCount++;
                }
                else if (sf.Status == MdfServiceFeeStatuses.Verified || sf.Status == MdfServiceFeeStatuses.Approved)
                {
                    alreadyVerifiedCount++;
                }
            }

            if (verifiedCount > 0)
            {
                await _dbContext.SaveChangesAsync(ct);
            }

            _logger.LogInformation("Batch verifikasi periode '{PeriodCode}': {VerifiedCount} hasil jasa diverifikasi, {AlreadyVerified} sudah terverifikasi sebelumnya.",
                period.PeriodCode, verifiedCount, alreadyVerifiedCount);

            return new BatchVerifyResultResponse
            {
                PeriodId = periodId,
                TotalFees = fees.Count,
                VerifiedCount = verifiedCount,
                AlreadyVerifiedCount = alreadyVerifiedCount
            };
        }

        /// <summary>
        /// Memperkaya nama penerima jasa secara polimorfik dari tabel MstDoctors atau MstWorkforceProfiles.
        /// </summary>
        private async Task EnrichPayeeNamesAsync(List<ServiceFeeHeaderResponse> items, CancellationToken ct)
        {
            if (items == null || items.Count == 0)
                return;

            var doctorIds = items
                .Where(i => i.PayeeType == MdfPayeeTypes.Doctor)
                .Select(i => i.PayeeReferenceId)
                .Distinct()
                .ToList();

            var workforceIds = items
                .Where(i => i.PayeeType == MdfPayeeTypes.Workforce)
                .Select(i => i.PayeeReferenceId)
                .Distinct()
                .ToList();

            Dictionary<Guid, string> doctorNames = new();
            if (doctorIds.Count > 0)
            {
                doctorNames = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .Where(d => doctorIds.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id, d => d.FullName, ct);
            }

            Dictionary<Guid, string> workforceNames = new();
            if (workforceIds.Count > 0)
            {
                workforceNames = await _dbContext.MstWorkforceProfiles
                    .AsNoTracking()
                    .Where(w => workforceIds.Contains(w.Id))
                    .ToDictionaryAsync(w => w.Id, w => w.DisplayName, ct);
            }

            foreach (var item in items)
            {
                if (item.PayeeType == MdfPayeeTypes.Doctor && doctorNames.TryGetValue(item.PayeeReferenceId, out var docName))
                {
                    item.PayeeName = docName;
                }
                else if (item.PayeeType == MdfPayeeTypes.Workforce && workforceNames.TryGetValue(item.PayeeReferenceId, out var wfName))
                {
                    item.PayeeName = wfName;
                }
                else
                {
                    item.PayeeName = "-";
                }
            }
        }
    }
}

