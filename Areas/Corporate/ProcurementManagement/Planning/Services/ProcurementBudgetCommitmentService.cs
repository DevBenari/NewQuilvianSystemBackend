using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Services;

/// <summary>
/// Pencatat dan pelepas komitmen pagu untuk sub-modul lain (BE-PBJ-010; <c>integration-contract.md</c>
/// bagian 2, PBJ-DEC-009). Belum ada pemanggil pada sub-modul ini; <c>pemilihan-penyedia</c> dan
/// <c>kontrak-pelaksanaan</c> memakainya kelak.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ikut transaksi pemanggil.</b> Metode hanya mengubah change tracker; pemanggil yang menyimpan,
/// sehingga komitmen tidak pernah tercatat untuk dokumen yang gagal disimpan.
/// </para>
/// <para>
/// <b>Idempoten.</b> Satu dokumen sumber punya paling banyak satu komitmen aktif per garis keturunan
/// pagu. Memanggil ulang <see cref="CommitAsync"/> memperbarui nilainya, bukan menambah baris. Contoh:
/// penetapan pemenang <c>PBJ_AWARD</c> Rp 120 juta dicatat, lalu dikoreksi Rp 118 juta → tetap satu
/// komitmen aktif bernilai Rp 118 juta.
/// </para>
/// </remarks>
public sealed class ProcurementBudgetCommitmentService
{
    private readonly ApplicationDbContext _dbContext;

    public ProcurementBudgetCommitmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Garis keturunan pagu berlaku yang disetujui untuk satu kunci, atau <c>null</c> bila belum ada
    /// pagu. Pemanggil memakai hasilnya untuk <see cref="CommitAsync"/>.
    /// </summary>
    public Task<Guid?> FindCurrentLineageAsync(
        int fiscalYear, Guid departmentId, Guid itemCategoryId, Guid fundSourceId, CancellationToken cancellationToken) =>
        _dbContext.PrcBudgets.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsCurrentVersion && x.Status == ProcurementStatuses.Plan.Approved &&
                        x.FiscalYear == fiscalYear && x.DepartmentId == departmentId &&
                        x.ItemCategoryId == itemCategoryId && x.FundSourceId == fundSourceId)
            .Select(x => (Guid?)x.LineageKey)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>Mencatat atau memperbarui komitmen aktif satu dokumen sumber pada satu garis keturunan pagu.</summary>
    public async Task<PrcBudgetCommitment> CommitAsync(
        Guid budgetLineageKey,
        string sourceDocumentType,
        Guid sourceDocumentId,
        decimal amount,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (budgetLineageKey == Guid.Empty || sourceDocumentId == Guid.Empty)
            throw new ArgumentException("Garis keturunan pagu dan dokumen sumber wajib diisi.");
        if (string.IsNullOrWhiteSpace(sourceDocumentType) || sourceDocumentType.Trim().Length > 40)
            throw new ArgumentException("Jenis dokumen sumber wajib diisi (maks. 40 karakter).", nameof(sourceDocumentType));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Nilai komitmen tidak boleh negatif.");
        if (!await _dbContext.PrcBudgets.AnyAsync(x => x.LineageKey == budgetLineageKey && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException("Pagu tidak ditemukan.");

        var type = sourceDocumentType.Trim();
        var now = DateTime.UtcNow;
        var existing = await _dbContext.PrcBudgetCommitments.FirstOrDefaultAsync(x =>
            x.BudgetLineageKey == budgetLineageKey && x.SourceDocumentType == type &&
            x.SourceDocumentId == sourceDocumentId && x.IsActive && !x.IsDelete, cancellationToken);
        if (existing != null)
        {
            existing.Amount = amount;
            existing.UpdateDateTime = now;
            existing.UpdateBy = actorUserId;
            return existing;
        }

        var commitment = new PrcBudgetCommitment
        {
            BudgetLineageKey = budgetLineageKey,
            SourceDocumentType = type,
            SourceDocumentId = sourceDocumentId,
            Amount = amount,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PrcBudgetCommitments.Add(commitment);
        return commitment;
    }

    /// <summary>
    /// Melepas seluruh komitmen aktif satu dokumen sumber (mis. kontrak dibatalkan). Baris tidak dihapus;
    /// menjadi tidak aktif dengan waktu lepas tercatat. Mengembalikan jumlah komitmen yang dilepas.
    /// </summary>
    public async Task<int> ReleaseAsync(
        string sourceDocumentType,
        Guid sourceDocumentId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var type = sourceDocumentType?.Trim() ?? string.Empty;
        var active = await _dbContext.PrcBudgetCommitments
            .Where(x => x.SourceDocumentType == type && x.SourceDocumentId == sourceDocumentId && x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var commitment in active)
        {
            commitment.IsActive = false;
            commitment.ReleasedAt = now;
            commitment.UpdateDateTime = now;
            commitment.UpdateBy = actorUserId;
        }
        return active.Count;
    }
}
