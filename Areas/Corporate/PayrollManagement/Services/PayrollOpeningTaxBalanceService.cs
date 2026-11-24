using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Kemampuan pemanggil menurut Akses Role (bantuan tampilan dan penjaga nominal; pengaman tetap diperiksa di Backend).</summary>
public sealed record PayrollOpeningTaxBalanceCaller(
    bool CanViewAmount, bool CanUpdate, bool CanSubmit, bool CanApprove, bool CanReject, bool CanCreate, bool CanDelete);

/// <summary>
/// Saldo awal PPh historis (PrlOpeningTaxBalance, PAY-BE-FIX-016): satu-satunya jalur sah untuk melengkapi riwayat tahun pajak yang tidak
/// punya payroll run di Quilvian, tanpa membuat payroll run historis palsu.
///
/// Siklus: DRAFT → PENDING_APPROVAL (diajukan dengan dokumen sumber) → APPROVED atau REJECTED. Penyetuju selalu pengguna yang berbeda
/// dari pengaju dan wajib punya izin melihat nominal. Yang sudah disahkan tidak disunting: perubahan dibuat sebagai revisi (versi
/// baru); saat versi baru disahkan, versi lama menjadi SUPERSEDED. Hanya APPROVED dari SAME_EMPLOYER_PRE_CUTOVER yang dipakai mesin hitung.
///
/// Dijaga di sini (tidak dapat diekspresikan sebagai constraint): bulan yang dicakup tidak boleh beririsan dengan payroll run yang sudah
/// final maupun dengan saldo APPROVED lain pegawai itu pada tahun yang sama, sehingga penghasilan dan PPh tidak terhitung dua kali.
/// Tidak pernah menulis ke payroll run, snapshot, maupun Finance. Nominal tidak ditulis ke log.
/// </summary>
public sealed class PayrollOpeningTaxBalanceService
{
    private const string LogCategory = "Corporate.PayrollManagement.OpeningTaxBalance";
    private const string LockKey = "PAYROLL_OPENING_TAX_BALANCE";
    private const int MaxPageSize = 100;
    private const decimal MaxAmount = 99_999_999_999_999.99m;

    private static readonly string[] EmployerSources =
        { OpeningTaxBalanceEmployerSources.SameEmployerPreCutover, OpeningTaxBalanceEmployerSources.PreviousEmployer };

    private static readonly string[] Statuses =
    {
        OpeningTaxBalanceStatuses.Draft, OpeningTaxBalanceStatuses.PendingApproval, OpeningTaxBalanceStatuses.Approved,
        OpeningTaxBalanceStatuses.Rejected, OpeningTaxBalanceStatuses.Superseded
    };

    /// <summary>Status yang masih "hidup" pada satu rantai versi: tidak boleh ada dua rantai terbuka untuk kunci yang sama.</summary>
    private static readonly string[] OpenStatuses =
    {
        OpeningTaxBalanceStatuses.Draft, OpeningTaxBalanceStatuses.PendingApproval, OpeningTaxBalanceStatuses.Approved
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollOpeningTaxBalanceService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<PayrollOpeningTaxBalanceMetadataResponse> GetMetadataAsync(CancellationToken cancellationToken) => new()
    {
        StorageAvailable = await PayrollOpeningTaxBalanceStorage.ExistsAsync(_dbContext, cancellationToken),
        StatusOptions = new()
        {
            new() { Value = OpeningTaxBalanceStatuses.Draft, Label = "Draf" },
            new() { Value = OpeningTaxBalanceStatuses.PendingApproval, Label = "Menunggu Persetujuan" },
            new() { Value = OpeningTaxBalanceStatuses.Approved, Label = "Disetujui" },
            new() { Value = OpeningTaxBalanceStatuses.Rejected, Label = "Ditolak" },
            new() { Value = OpeningTaxBalanceStatuses.Superseded, Label = "Digantikan Versi Baru" }
        },
        EmployerSourceOptions = new()
        {
            new()
            {
                Value = OpeningTaxBalanceEmployerSources.SameEmployerPreCutover,
                Label = "Pemberi kerja yang sama (sebelum cutover Quilvian)",
                Note = "Dipakai rekonsiliasi masa pajak terakhir setelah disetujui."
            },
            new()
            {
                Value = OpeningTaxBalanceEmployerSources.PreviousEmployer,
                Label = "Pemberi kerja sebelumnya",
                Note = "Dapat disimpan sebagai arsip, tetapi belum dipakai mesin hitung (perlu aturan dan bukti 1721-A1)."
            }
        }
    };

    public async Task<PagedResult<PayrollOpeningTaxBalanceResponse>> ListAsync(
        PayrollOpeningTaxBalanceQuery query, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureStorageAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var source = _dbContext.PrlOpeningTaxBalances.AsNoTracking().Include(x => x.WorkforceProfile).Where(x => !x.IsDelete);
        if (query.WorkforceProfileId.HasValue && query.WorkforceProfileId.Value != Guid.Empty)
            source = source.Where(x => x.WorkforceProfileId == query.WorkforceProfileId.Value);
        if (query.TaxYear.HasValue) source = source.Where(x => x.TaxYear == query.TaxYear.Value);
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            source = source.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.EmployerSource))
        {
            var employerSource = query.EmployerSource.Trim().ToUpperInvariant();
            source = source.Where(x => x.EmployerSource == employerSource);
        }

        var total = await source.CountAsync(cancellationToken);
        var rows = await source
            .OrderByDescending(x => x.TaxYear).ThenBy(x => x.WorkforceProfileId).ThenByDescending(x => x.Version)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PayrollOpeningTaxBalanceResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)),
            Items = rows.Select(x => ToResponse(x, caller)).ToList()
        };
    }

    public async Task<PayrollOpeningTaxBalanceResponse> GetAsync(Guid id, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureStorageAsync(cancellationToken);
        var entity = await _dbContext.PrlOpeningTaxBalances.AsNoTracking().Include(x => x.WorkforceProfile)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Saldo awal PPh tidak ditemukan.");
        return ToResponse(entity, caller);
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Tulis
    // ------------------------------------------------------------------------------------------------------------------------

    public Task<PayrollOpeningTaxBalanceResponse> CreateAsync(
        SavePayrollOpeningTaxBalanceRequest request, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireAmountPermission(caller);
            var input = await ValidateAsync(request, cancellationToken);

            // Satu rantai terbuka per kunci: perubahan sesudah disahkan memakai revisi, bukan baris kedua.
            var existing = await _dbContext.PrlOpeningTaxBalances.AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceProfileId == input.WorkforceProfileId && x.TaxYear == input.TaxYear
                    && x.EmployerSource == input.EmployerSource)
                .Select(x => new { x.Status, x.Version })
                .ToListAsync(cancellationToken);
            if (existing.Any(x => OpenStatuses.Contains(x.Status)))
                throw new PayrollRunConflictException(
                    "Sudah ada saldo awal yang masih berjalan (draf, menunggu persetujuan, atau disetujui) untuk pegawai, tahun pajak, dan sumber yang sama. Ubah drafnya atau buat revisi dari yang disetujui.");

            var entity = new PrlOpeningTaxBalance
            {
                Id = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                Version = existing.Count == 0 ? 1 : existing.Max(x => x.Version) + 1,
                Status = OpeningTaxBalanceStatuses.Draft
            };
            Apply(entity, input);
            _dbContext.PrlOpeningTaxBalances.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Create", "Menyusun draf saldo awal PPh.", entity, actorUserId);
            return await ReloadAsync(entity.Id, caller, cancellationToken);
        }, cancellationToken);

    public Task<PayrollOpeningTaxBalanceResponse> UpdateAsync(
        Guid id, SavePayrollOpeningTaxBalanceRequest request, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireAmountPermission(caller);
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.Status != OpeningTaxBalanceStatuses.Draft)
                throw new PayrollInvalidTransitionException(
                    $"Hanya draf yang dapat diubah (saat ini {entity.Status}). Untuk mengubah saldo yang sudah diajukan atau disetujui, buat revisi.");

            var input = await ValidateAsync(request, cancellationToken);
            if (input.WorkforceProfileId != entity.WorkforceProfileId || input.TaxYear != entity.TaxYear || input.EmployerSource != entity.EmployerSource)
                throw new PayrollRunValidationException("Pegawai, tahun pajak, dan sumber tidak dapat diubah. Hapus draf ini dan buat yang baru bila salah.");

            Apply(entity, input);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Update", "Memperbarui draf saldo awal PPh.", entity, actorUserId);
            return await ReloadAsync(entity.Id, caller, cancellationToken);
        }, cancellationToken);

    /// <summary>Membuat draf versi baru dari saldo yang sudah disetujui atau ditolak. Versi lama tetap utuh sampai versi baru disetujui.</summary>
    public Task<PayrollOpeningTaxBalanceResponse> ReviseAsync(
        Guid id, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireAmountPermission(caller);
            var source = await LoadAsync(id, cancellationToken);
            if (source.Status != OpeningTaxBalanceStatuses.Approved && source.Status != OpeningTaxBalanceStatuses.Rejected)
                throw new PayrollInvalidTransitionException(
                    $"Revisi hanya dari saldo yang disetujui atau ditolak (saat ini {source.Status}).");

            var chain = await _dbContext.PrlOpeningTaxBalances.AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceProfileId == source.WorkforceProfileId && x.TaxYear == source.TaxYear
                    && x.EmployerSource == source.EmployerSource)
                .Select(x => new { x.Status, x.Version })
                .ToListAsync(cancellationToken);
            if (chain.Any(x => x.Status == OpeningTaxBalanceStatuses.Draft || x.Status == OpeningTaxBalanceStatuses.PendingApproval))
                throw new PayrollRunConflictException("Sudah ada draf atau pengajuan yang berjalan untuk saldo ini. Selesaikan atau hapus lebih dulu.");

            var copy = new PrlOpeningTaxBalance
            {
                Id = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId,
                WorkforceProfileId = source.WorkforceProfileId,
                TaxYear = source.TaxYear,
                CoveredMonths = source.CoveredMonths.ToList(),
                EmployerSource = source.EmployerSource,
                TaxableGross = source.TaxableGross,
                EmployerPremium = source.EmployerPremium,
                PensionDeduction = source.PensionDeduction,
                TaxWithheld = source.TaxWithheld,
                SourceReference = source.SourceReference,
                Status = OpeningTaxBalanceStatuses.Draft,
                Version = chain.Max(x => x.Version) + 1
            };
            _dbContext.PrlOpeningTaxBalances.Add(copy);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Revise", $"Membuat revisi saldo awal PPh dari versi {source.Version}.", copy, actorUserId);
            return await ReloadAsync(copy.Id, caller, cancellationToken);
        }, cancellationToken);

    public Task<PayrollOpeningTaxBalanceResponse> SubmitAsync(
        Guid id, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.Status != OpeningTaxBalanceStatuses.Draft)
                throw new PayrollInvalidTransitionException($"Hanya draf yang dapat diajukan (saat ini {entity.Status}).");
            if (string.IsNullOrWhiteSpace(entity.SourceReference))
                throw new PayrollRunValidationException("Dokumen sumber wajib diisi sebelum saldo awal diajukan (mis. nomor 1721-A1 atau slip gaji).");

            entity.Status = OpeningTaxBalanceStatuses.PendingApproval;
            entity.SubmittedAt = DateTime.UtcNow;
            entity.SubmittedByUserId = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Submit", "Mengajukan saldo awal PPh untuk disetujui.", entity, actorUserId);
            return await ReloadAsync(entity.Id, caller, cancellationToken);
        }, cancellationToken);

    public Task<PayrollOpeningTaxBalanceResponse> ApproveAsync(
        Guid id, string? note, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireDecisionPermission(caller);
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.Status != OpeningTaxBalanceStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException($"Hanya saldo yang menunggu persetujuan yang dapat disetujui (saat ini {entity.Status}).");
            if (entity.SubmittedByUserId.HasValue && entity.SubmittedByUserId.Value == actorUserId)
                throw new PayrollForbiddenActionException("Pengaju tidak boleh menyetujui saldo awal yang diajukannya sendiri.");
            if (string.IsNullOrWhiteSpace(entity.SourceReference))
                throw new PayrollRunValidationException("Dokumen sumber kosong; saldo awal tidak dapat disetujui tanpa dasar.");

            // Bulan yang dicakup tidak boleh sudah tercakup payroll run final (akan terhitung dua kali).
            var yearStart = new DateTime(entity.TaxYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = yearStart.AddYears(1);
            var runPeriodEnds = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceProfileId == entity.WorkforceProfileId && x.PayrollRun != null && !x.PayrollRun.IsDelete
                    && x.PayrollRun.PeriodEndDateSnapshot >= yearStart && x.PayrollRun.PeriodEndDateSnapshot < yearEnd
                    && PayrollCalculationService.AccumulatedRunStatuses.Contains(x.PayrollRun.RunStatus))
                .Select(x => x.PayrollRun!.PeriodEndDateSnapshot)
                .ToListAsync(cancellationToken);
            var runMonths = runPeriodEnds.Select(x => x.Month).Distinct().ToHashSet();
            var runOverlap = entity.CoveredMonths.Where(runMonths.Contains).OrderBy(m => m).ToList();
            if (runOverlap.Count > 0)
                throw new PayrollRunConflictException(
                    $"Masa pajak {string.Join(", ", runOverlap)} tahun {entity.TaxYear} sudah tercakup payroll run yang disahkan; penghasilan dan PPh akan terhitung dua kali. Koreksi cakupan bulan lewat revisi.");

            // Dan tidak boleh beririsan dengan saldo APPROVED lain pegawai itu pada tahun yang sama (sumber lain). Versi sebelumnya dari
            // sumber yang sama justru akan digantikan, jadi dikecualikan.
            var otherApproved = await _dbContext.PrlOpeningTaxBalances.AsNoTracking()
                .Where(x => !x.IsDelete && x.Id != entity.Id && x.WorkforceProfileId == entity.WorkforceProfileId && x.TaxYear == entity.TaxYear
                    && x.Status == OpeningTaxBalanceStatuses.Approved && x.EmployerSource != entity.EmployerSource)
                .ToListAsync(cancellationToken);
            var balanceOverlap = otherApproved.SelectMany(x => x.CoveredMonths).Where(entity.CoveredMonths.Contains).Distinct().OrderBy(m => m).ToList();
            if (balanceOverlap.Count > 0)
                throw new PayrollRunConflictException(
                    $"Masa pajak {string.Join(", ", balanceOverlap)} tahun {entity.TaxYear} sudah tercakup saldo awal lain yang disetujui untuk pegawai ini.");

            // Versi lama dari rantai yang sama digantikan lebih dulu (dua langkah simpan) agar indeks unik "satu APPROVED per sumber" tidak terlanggar.
            var previous = await _dbContext.PrlOpeningTaxBalances
                .Where(x => !x.IsDelete && x.Id != entity.Id && x.WorkforceProfileId == entity.WorkforceProfileId && x.TaxYear == entity.TaxYear
                    && x.EmployerSource == entity.EmployerSource && x.Status == OpeningTaxBalanceStatuses.Approved)
                .ToListAsync(cancellationToken);
            foreach (var old in previous)
            {
                old.Status = OpeningTaxBalanceStatuses.Superseded;
                old.UpdateDateTime = DateTime.UtcNow;
                old.UpdateBy = actorUserId;
            }

            if (previous.Count > 0) await _dbContext.SaveChangesAsync(cancellationToken);

            entity.Status = OpeningTaxBalanceStatuses.Approved;
            entity.ApprovedAt = DateTime.UtcNow;
            entity.ApprovedByUserId = actorUserId;
            entity.DecisionNote = Clean(note, 1000);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Approve", "Menyetujui saldo awal PPh.", entity, actorUserId);
            return await ReloadAsync(entity.Id, caller, cancellationToken);
        }, cancellationToken);

    public Task<PayrollOpeningTaxBalanceResponse> RejectAsync(
        Guid id, string? note, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireDecisionPermission(caller);
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.Status != OpeningTaxBalanceStatuses.PendingApproval)
                throw new PayrollInvalidTransitionException($"Hanya saldo yang menunggu persetujuan yang dapat ditolak (saat ini {entity.Status}).");
            var reason = Clean(note, 1000);
            if (reason is null) throw new PayrollRunValidationException("Alasan penolakan wajib diisi.");

            entity.Status = OpeningTaxBalanceStatuses.Rejected;
            entity.RejectedAt = DateTime.UtcNow;
            entity.RejectedByUserId = actorUserId;
            entity.DecisionNote = reason;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Reject", "Menolak saldo awal PPh.", entity, actorUserId);
            return await ReloadAsync(entity.Id, caller, cancellationToken);
        }, cancellationToken);

    /// <summary>Menghapus (soft delete) draf atau saldo yang ditolak. Yang pernah menunggu persetujuan, disetujui, atau digantikan adalah riwayat dan tidak dihapus.</summary>
    public Task<bool> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            var entity = await LoadAsync(id, cancellationToken);
            if (entity.Status != OpeningTaxBalanceStatuses.Draft && entity.Status != OpeningTaxBalanceStatuses.Rejected)
                throw new PayrollInvalidTransitionException(
                    $"Hanya draf atau saldo yang ditolak yang dapat dihapus (saat ini {entity.Status}); saldo lain adalah riwayat.");

            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await AuditAsync("OpeningTaxBalance.Delete", "Menghapus draf saldo awal PPh.", entity, actorUserId);
            return true;
        }, cancellationToken);

    // ------------------------------------------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------------------------------------------

    private sealed record ValidInput(
        Guid WorkforceProfileId, int TaxYear, List<int> CoveredMonths, string EmployerSource,
        decimal TaxableGross, decimal EmployerPremium, decimal PensionDeduction, decimal TaxWithheld, string? SourceReference);

    private async Task<ValidInput> ValidateAsync(SavePayrollOpeningTaxBalanceRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (request.WorkforceProfileId == Guid.Empty) errors.Add("Pegawai wajib dipilih.");
        else if (!await _dbContext.MstWorkforceProfiles.AsNoTracking().AnyAsync(x => x.Id == request.WorkforceProfileId && !x.IsDelete, cancellationToken))
            errors.Add("Pegawai tidak ditemukan.");

        var currentYear = DateTime.UtcNow.Year;
        if (request.TaxYear < 2000 || request.TaxYear > currentYear)
            errors.Add($"Tahun pajak harus antara 2000 dan {currentYear}.");

        var months = (request.CoveredMonths ?? new List<int>()).ToList();
        if (months.Count == 0) errors.Add("Bulan yang dicakup wajib diisi (minimal satu bulan).");
        if (months.Any(m => m < 1 || m > 12)) errors.Add("Bulan yang dicakup harus antara 1 dan 12.");
        if (months.Distinct().Count() != months.Count) errors.Add("Bulan yang dicakup tidak boleh berulang.");

        var source = (request.EmployerSource ?? string.Empty).Trim().ToUpperInvariant();
        if (!EmployerSources.Contains(source)) errors.Add("Sumber saldo awal tidak valid. Gunakan SAME_EMPLOYER_PRE_CUTOVER atau PREVIOUS_EMPLOYER.");

        foreach (var (label, value) in new[]
                 {
                     ("Penghasilan kena pajak", request.TaxableGross), ("Premi pemberi kerja", request.EmployerPremium),
                     ("Iuran pensiun", request.PensionDeduction), ("PPh yang sudah dipotong", request.TaxWithheld)
                 })
        {
            if (value < 0m) errors.Add($"{label} tidak boleh negatif.");
            else if (value > MaxAmount) errors.Add($"{label} melebihi batas yang dapat disimpan.");
        }

        if (errors.Count > 0) throw new PayrollRunValidationException(string.Join(" ", errors));

        return new ValidInput(
            request.WorkforceProfileId, request.TaxYear, months.Distinct().OrderBy(m => m).ToList(), source,
            Math.Round(request.TaxableGross, 2), Math.Round(request.EmployerPremium, 2),
            Math.Round(request.PensionDeduction, 2), Math.Round(request.TaxWithheld, 2), Clean(request.SourceReference, 500));
    }

    private static void Apply(PrlOpeningTaxBalance entity, ValidInput input)
    {
        entity.WorkforceProfileId = input.WorkforceProfileId;
        entity.TaxYear = input.TaxYear;
        entity.CoveredMonths = input.CoveredMonths;
        entity.EmployerSource = input.EmployerSource;
        entity.TaxableGross = input.TaxableGross;
        entity.EmployerPremium = input.EmployerPremium;
        entity.PensionDeduction = input.PensionDeduction;
        entity.TaxWithheld = input.TaxWithheld;
        entity.SourceReference = input.SourceReference;
    }

    /// <summary>Nominal saldo awal sensitif: menulis angka membutuhkan izin melihat nominal (pola ViewAmount Profil Pajak).</summary>
    private static void RequireAmountPermission(PayrollOpeningTaxBalanceCaller caller)
    {
        if (!caller.CanViewAmount)
            throw new PayrollForbiddenActionException("Mengisi saldo awal PPh membutuhkan izin melihat nominal (ViewAmount).");
    }

    private static void RequireDecisionPermission(PayrollOpeningTaxBalanceCaller caller)
    {
        if (!caller.CanViewAmount)
            throw new PayrollForbiddenActionException("Menyetujui atau menolak saldo awal PPh membutuhkan izin melihat nominal (ViewAmount).");
    }

    private async Task EnsureStorageAsync(CancellationToken cancellationToken)
    {
        if (!await PayrollOpeningTaxBalanceStorage.ExistsAsync(_dbContext, cancellationToken))
            throw new PayrollPolicyNotConfirmedException(
                "Penyimpanan saldo awal PPh belum tersedia di database (DDL belum diterapkan). Fitur ini tidak dapat dipakai, dan ketiadaannya bukan saldo nol.");
    }

    private async Task<PrlOpeningTaxBalance> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlOpeningTaxBalances.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Saldo awal PPh tidak ditemukan.");

    private async Task<PayrollOpeningTaxBalanceResponse> ReloadAsync(Guid id, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.PrlOpeningTaxBalances.AsNoTracking().Include(x => x.WorkforceProfile)
            .FirstAsync(x => x.Id == id, cancellationToken);
        return ToResponse(entity, caller);
    }

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    private static PayrollOpeningTaxBalanceResponse ToResponse(PrlOpeningTaxBalance x, PayrollOpeningTaxBalanceCaller caller)
    {
        var usedByEngine = x.Status == OpeningTaxBalanceStatuses.Approved && x.EmployerSource == OpeningTaxBalanceEmployerSources.SameEmployerPreCutover;
        var isDraft = x.Status == OpeningTaxBalanceStatuses.Draft;
        var isPending = x.Status == OpeningTaxBalanceStatuses.PendingApproval;
        return new PayrollOpeningTaxBalanceResponse
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            TaxYear = x.TaxYear,
            CoveredMonths = x.CoveredMonths.OrderBy(m => m).ToList(),
            EmployerSource = x.EmployerSource,
            AmountsVisible = caller.CanViewAmount,
            TaxableGross = caller.CanViewAmount ? x.TaxableGross : null,
            EmployerPremium = caller.CanViewAmount ? x.EmployerPremium : null,
            PensionDeduction = caller.CanViewAmount ? x.PensionDeduction : null,
            TaxWithheld = caller.CanViewAmount ? x.TaxWithheld : null,
            SourceReference = x.SourceReference,
            Status = x.Status,
            Version = x.Version,
            SubmittedAt = x.SubmittedAt,
            SubmittedByUserId = x.SubmittedByUserId,
            ApprovedAt = x.ApprovedAt,
            ApprovedByUserId = x.ApprovedByUserId,
            RejectedAt = x.RejectedAt,
            RejectedByUserId = x.RejectedByUserId,
            DecisionNote = x.DecisionNote,
            IsUsedByEngine = usedByEngine,
            UsageNote = x.Status == OpeningTaxBalanceStatuses.Approved && !usedByEngine
                ? "Disimpan sebagai arsip: saldo dari pemberi kerja sebelumnya belum dipakai mesin hitung."
                : null,
            CreateDateTime = x.CreateDateTime,
            UpdateDateTime = x.UpdateDateTime,
            Actions = new PayrollOpeningTaxBalanceActionsResponse
            {
                CanUpdate = caller.CanUpdate && caller.CanViewAmount && isDraft,
                CanSubmit = caller.CanSubmit && isDraft,
                CanApprove = caller.CanApprove && caller.CanViewAmount && isPending,
                CanReject = caller.CanReject && caller.CanViewAmount && isPending,
                CanRevise = caller.CanCreate && caller.CanViewAmount
                    && (x.Status == OpeningTaxBalanceStatuses.Approved || x.Status == OpeningTaxBalanceStatuses.Rejected),
                CanDelete = caller.CanDelete && (isDraft || x.Status == OpeningTaxBalanceStatuses.Rejected)
            }
        };
    }

    // Nominal tidak ditulis ke log (hanya identitas dan status).
    private Task AuditAsync(string action, string message, PrlOpeningTaxBalance entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, message, new
        {
            entity.Id,
            entity.WorkforceProfileId,
            entity.TaxYear,
            entity.EmployerSource,
            entity.Version,
            entity.Status,
            CoveredMonths = string.Join(",", entity.CoveredMonths),
            ActorUserId = actorUserId
        });
}
