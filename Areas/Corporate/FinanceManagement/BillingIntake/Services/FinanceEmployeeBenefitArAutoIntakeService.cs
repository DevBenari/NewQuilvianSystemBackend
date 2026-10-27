using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;

/// <summary>Pengolah satu fakta masuk. Satu-satunya implementasi produksi memanggil jalur manual yang sudah ada.</summary>
public interface IBillingIntakeProcessor
{
    Task<BillingIntakeResponse> ProcessAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken);
}

/// <summary>
/// Memanggil <see cref="FinanceBillingIntakeService.ProcessAsync"/> apa adanya — jalur yang sama dengan tombol Proses
/// di Pemantauan Finance, termasuk transaksi, kunci, pencatatan galat, dan jejak audit. Tidak ada logika pengolahan
/// baru di sini.
/// </summary>
public sealed class BillingIntakeProcessor(FinanceBillingIntakeService intakeService) : IBillingIntakeProcessor
{
    public Task<BillingIntakeResponse> ProcessAsync(Guid intakeId, Guid actorUserId, CancellationToken cancellationToken) =>
        intakeService.ProcessAsync(intakeId, actorUserId, cancellationToken);
}

public sealed record EmployeeBenefitArAutoIntakeResult(int Discovered, int Processed, int Failed, int Skipped)
{
    public int Total => Discovered + Processed + Failed + Skipped;
}

/// <summary>
/// Intake otomatis HANYA untuk AR Karyawan (handoff AR berjenis EMPLOYEE_BENEFIT): menemukan fakta yang sudah melewati masa
/// tunggu lalu mengolahnya menjadi FinReceivable lewat jalur manual yang sudah ada. Jenis fakta lain (koleksi, deposit,
/// refund, selisih kas, koreksi) TIDAK disentuh dan tetap manual — pengolahan sebagian dari mereka butuh peninjauan manusia.
///
/// Batas yang sengaja dipegang:
/// <list type="bullet">
/// <item>Menulis baris intake memakai bentuk yang sama dengan <c>SyncNewFactsAsync</c>; indeks unik identitas intake
/// menjaga dari duplikat bila sinkron manual atau instansi lain berjalan bersamaan.</item>
/// <item>Handoff pengganti koreksi salah orang tidak diambil (mengikuti <c>SyncNewFactsAsync</c>): ia diolah lewat intake
/// ADJUSTMENT bersama pembatalan piutang lama.</item>
/// <item>Hanya baris berstatus NEW yang diolah. Baris ERROR tidak dicoba ulang otomatis: kegagalan tetap terlihat di
/// Pemantauan Finance sampai petugas menanganinya (tanpa putaran coba ulang yang menutupi masalah data).</item>
/// <item>Pengolahan sebuah fakta yang sudah diolah proses lain (kunci per intake + status) dihitung "dilewati", bukan galat.</item>
/// <item>Piutang yang lahir tetap berasal dari <c>BilArHandoff.Amount</c> apa adanya; Finance tidak menghitung ulang
/// dan tidak mengubah apa pun di tabel Bil*.</item>
/// </list>
/// </summary>
public sealed class FinanceEmployeeBenefitArAutoIntakeService(ApplicationDbContext dbContext, IBillingIntakeProcessor processor)
{
    public async Task<EmployeeBenefitArAutoIntakeResult> RunAsync(
        Guid actorUserId, TimeSpan grace, int maxPerCycle, CancellationToken cancellationToken, DateTimeOffset? now = null)
    {
        var limit = Math.Clamp(maxPerCycle, 1, 500);
        var cutoff = (now ?? DateTimeOffset.UtcNow) - (grace < TimeSpan.Zero ? TimeSpan.Zero : grace);

        // Handoff AR Karyawan yang masih CREATED (belum diakui Finance). Penyaringan waktu dilakukan di memori: jumlahnya
        // dibatasi oleh yang belum diolah, dan perbandingan DateTimeOffset tidak seragam di semua penyedia basis data.
        var handoffs = await dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == BillingHandoffStatuses.Created
                        && x.DebtorType == BillingArDebtorTypes.EmployeeBenefit)
            .ToListAsync(cancellationToken);
        if (handoffs.Count == 0) return new EmployeeBenefitArAutoIntakeResult(0, 0, 0, 0);

        var replacementCorrelationIds = (await dbContext.BilHandoffAdjustments.AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => x.CorrelationId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var eligible = handoffs
            .Where(x => x.CreatedAt <= cutoff && !replacementCorrelationIds.Contains(x.CorrelationId))
            .OrderBy(x => x.CreatedAt)
            .ToList();
        if (eligible.Count == 0) return new EmployeeBenefitArAutoIntakeResult(0, 0, 0, 0);

        var discovered = await DiscoverAsync(eligible, limit, actorUserId, cancellationToken);

        // Diolah: baris AR berstatus NEW yang handoff sumbernya memenuhi syarat di atas — termasuk baris yang sudah
        // ditemukan lewat sinkron manual tetapi belum diolah siapa pun.
        var eligibleHandoffIds = eligible.Select(x => x.Id).ToHashSet();
        var pending = await dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .Where(x => !x.IsDelete && x.HandoffType == FinBillingHandoffTypes.Ar
                        && x.Status == FinBillingHandoffIntakeStatuses.New)
            .Select(x => new { x.Id, x.SourceHandoffId })
            .ToListAsync(cancellationToken);
        var intakeIds = pending
            .Where(x => eligibleHandoffIds.Contains(x.SourceHandoffId))
            .Select(x => x.Id)
            .Take(limit)
            .ToList();

        int processed = 0, failed = 0, skipped = 0;
        foreach (var intakeId in intakeIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var result = await processor.ProcessAsync(intakeId, actorUserId, cancellationToken);
                if (result.Status is FinBillingHandoffIntakeStatuses.Consumed or FinBillingHandoffIntakeStatuses.Acknowledged)
                    processed++;
                else
                    failed++; // ProcessAsync sudah menyimpan galatnya pada baris (status ERROR) agar terlihat petugas.
            }
            catch (Exception exception) when (exception is KeyNotFoundException or BillingIntakeValidationException)
            {
                skipped++; // sudah diolah proses lain, atau fakta sumber sudah tidak ada.
            }
            finally
            {
                dbContext.ChangeTracker.Clear();
            }
        }

        return new EmployeeBenefitArAutoIntakeResult(discovered, processed, failed, skipped);
    }

    private async Task<int> DiscoverAsync(
        List<BilArHandoff> eligible, int limit, Guid actorUserId, CancellationToken cancellationToken)
    {
        var existingKeys = (await dbContext.FinBillingHandoffIntakes.AsNoTracking()
            .Where(x => !x.IsDelete && x.HandoffType == FinBillingHandoffTypes.Ar)
            .Select(x => x.SourceHandoffKey)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var toCreate = eligible.Where(x => !existingKeys.Contains(x.HandoffKey)).Take(limit).ToList();
        if (toCreate.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var handoff in toCreate)
        {
            dbContext.FinBillingHandoffIntakes.Add(new FinBillingHandoffIntake
            {
                HandoffType = FinBillingHandoffTypes.Ar,
                SourceHandoffId = handoff.Id,
                SourceHandoffKey = handoff.HandoffKey,
                Status = FinBillingHandoffIntakeStatuses.New,
                CorrelationId = handoff.CorrelationId,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Berpacu dengan sinkron lain; indeks unik IX_FinBillingHandoffIntake_Identity mencegah duplikat.
            foreach (var entry in dbContext.ChangeTracker.Entries<FinBillingHandoffIntake>()
                         .Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;
            return 0;
        }

        return toCreate.Count;
    }
}
