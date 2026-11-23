namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Status siklus Opening Tax Balance (saldo awal PPh historis). Hanya APPROVED yang boleh dipakai rekonsiliasi.</summary>
public static class OpeningTaxBalanceStatuses
{
    public const string Draft = "DRAFT";
    public const string PendingApproval = "PENDING_APPROVAL";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Superseded = "SUPERSEDED";
}

/// <summary>Asal data saldo awal. Data pemberi kerja yang sama (sebelum cutover Quilvian) tidak boleh dicampur dengan pemberi kerja sebelumnya.</summary>
public static class OpeningTaxBalanceEmployerSources
{
    public const string SameEmployerPreCutover = "SAME_EMPLOYER_PRE_CUTOVER";
    public const string PreviousEmployer = "PREVIOUS_EMPLOYER";
}

/// <summary>
/// Satu saldo awal PPh historis yang sudah DISETUJUI untuk satu pegawai dan satu tahun pajak (pembacaan murni). Satu baris mewakili
/// BANYAK masa pajak: <see cref="CoveredMonths"/> menyimpan bulan-bulan yang dicakup (1..12) sehingga jumlah bulan tidak pernah
/// disamakan dengan jumlah baris. Nilai adalah kumulatif atas bulan-bulan itu saja.
///
/// Rancangan persistensinya (tabel baru, belum ada migration) ada pada laporan PAY-BE-FIX-008. Sampai tabelnya tersedia, sumbernya
/// adalah <see cref="UnavailablePayrollOpeningTaxBalanceSource"/> dan tidak ada saldo awal yang dipakai.
/// </summary>
public sealed record PayrollOpeningTaxBalanceFacts(
    Guid Id,
    int TaxYear,
    IReadOnlyList<int> CoveredMonths,
    decimal TaxableGross,
    decimal EmployerPremium,
    decimal PensionDeduction,
    decimal TaxWithheld,
    string EmployerSource,
    string Status,
    string? SourceReference,
    int Version);

/// <summary>
/// Pembaca saldo awal PPh yang disetujui. Murni baca: tidak pernah membuat, mengubah, atau menyetujui saldo awal. Implementasi harus
/// hanya mengembalikan status APPROVED dan versi terkini (bukan SUPERSEDED).
/// </summary>
public interface IPayrollOpeningTaxBalanceSource
{
    /// <summary>False bila penyimpanan saldo awal belum tersedia (skema belum diterapkan). Pemanggil wajib menyatakannya, bukan menganggap saldo nol.</summary>
    bool IsAvailable { get; }

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>> LoadApprovedAsync(
        IReadOnlyCollection<Guid> workforceProfileIds, int taxYear, CancellationToken cancellationToken);
}

/// <summary>
/// Sumber saldo awal sementara selama tabelnya belum ada: tidak mengembalikan apa pun dan menyatakan dirinya tidak tersedia. Ini BUKAN
/// penyimpanan sementara dan tidak menyimpan data apa pun; ia hanya membuat konsolidasi YTD jujur ("saldo awal belum dapat dipakai")
/// alih-alih diam-diam menganggap saldo nol. Diganti implementasi berbasis tabel setelah migrasi database disetujui.
/// </summary>
public sealed class UnavailablePayrollOpeningTaxBalanceSource : IPayrollOpeningTaxBalanceSource
{
    public bool IsAvailable => false;

    public Task<IReadOnlyDictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>> LoadApprovedAsync(
        IReadOnlyCollection<Guid> workforceProfileIds, int taxYear, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>>(
            new Dictionary<Guid, IReadOnlyList<PayrollOpeningTaxBalanceFacts>>());
}
