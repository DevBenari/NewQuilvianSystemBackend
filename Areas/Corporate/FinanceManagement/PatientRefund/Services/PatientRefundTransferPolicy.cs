using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Services;

/// <summary>
/// Rekening pengirim rumah sakit yang sudah lolos pemeriksaan master. Seluruh isinya diturunkan dari
/// <c>MstBankAccount</c> dan <c>MstBank</c> di server — bukan dari label atau nomor kiriman browser.
/// </summary>
public sealed record PatientRefundSourceAccount(
    Guid BankAccountId,
    Guid BankId,
    string BankName,
    string AccountNumber,
    string AccountName);

/// <summary>Hasil pemeriksaan rekening pengirim: berisi <see cref="Account"/> atau <see cref="Error"/>.</summary>
public sealed record PatientRefundSourceAccountResult(PatientRefundSourceAccount? Account, string? Error)
{
    public bool IsValid => Account is not null;
}

/// <summary>
/// Satu-satunya tempat aturan transfer refund pasien: metode selalu Transfer Bank, rekening pengirim
/// berasal dari master rekening bank rumah sakit, dan rekening tujuan pasien berdiri terpisah.
/// Dipakai bersama oleh pengajuan kasir (Billing) dan pengajuan/persetujuan/pencairan Finance supaya
/// kedua pintu tidak menegakkan aturan yang berbeda.
/// </summary>
/// <remarks>
/// Hasilnya berupa pesan, bukan pengecualian, karena pemanggilnya berasal dari dua modul dengan jenis
/// pengecualian validasi yang berbeda; masing-masing melempar pengecualiannya sendiri.
/// </remarks>
public static class PatientRefundTransferPolicy
{
    /// <summary>Refund pasien dibayar dalam rupiah.</summary>
    public const string RefundCurrencyCode = "IDR";

    public const string TransferOnlyMessage =
        "Metode pengembalian dana pasien hanya Transfer Bank.";

    /// <summary>
    /// Metode kosong dianggap Transfer Bank (nilai bawaan kontrak). Nilai lain, termasuk tunai,
    /// ditolak.
    /// </summary>
    public static bool TryNormalizeMethod(string? rawMethod, out string normalizedMethod)
    {
        normalizedMethod = FinPatientRefundPaymentMethods.Transfer;

        return string.IsNullOrWhiteSpace(rawMethod)
            || string.Equals(rawMethod.Trim(), FinPatientRefundPaymentMethods.Transfer, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Memeriksa ulang rekening pengirim terhadap master: ada, tidak dihapus, aktif, banknya aktif,
    /// berupa rekening rupiah, dan — bila bank pengirim dikirim — memang milik bank itu.
    /// </summary>
    public static async Task<PatientRefundSourceAccountResult> ResolveSourceAccountAsync(
        ApplicationDbContext dbContext,
        Guid? bankAccountId,
        Guid? senderBankId,
        CancellationToken cancellationToken)
    {
        if (!bankAccountId.HasValue || bankAccountId.Value == Guid.Empty)
        {
            return Invalid("Rekening pengirim wajib dipilih dari master rekening bank rumah sakit.");
        }

        var account = await dbContext.MstBankAccounts.AsNoTracking()
            .Include(x => x.Bank)
            .SingleOrDefaultAsync(x => x.Id == bankAccountId.Value && !x.IsDelete, cancellationToken);

        if (account is null)
        {
            return Invalid("Rekening pengirim tidak ditemukan pada master rekening bank.");
        }

        if (!account.IsActive)
        {
            return Invalid("Rekening pengirim sedang tidak aktif. Pilih rekening pengirim lain.");
        }

        if (account.Bank is null || account.Bank.IsDelete || !account.Bank.IsActive)
        {
            return Invalid("Bank dari rekening pengirim sudah tidak aktif pada master bank. Pilih rekening pengirim lain.");
        }

        if (senderBankId.HasValue && senderBankId.Value != Guid.Empty && senderBankId.Value != account.BankId)
        {
            return Invalid("Rekening pengirim bukan milik bank pengirim yang dipilih.");
        }

        if (!string.Equals(account.CurrencyCode, RefundCurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            return Invalid("Rekening pengirim bukan rekening rupiah (IDR) sehingga tidak dapat dipakai untuk refund pasien.");
        }

        return new PatientRefundSourceAccountResult(
            new PatientRefundSourceAccount(
                account.Id,
                account.BankId,
                account.Bank.BankName,
                account.AccountNumber,
                account.AccountName),
            null);
    }

    /// <summary>
    /// Memeriksa rekening tujuan pasien: lengkap, dan tidak sama dengan rekening pengirim rumah sakit.
    /// Mengembalikan pesan kesalahan, atau <c>null</c> bila sah.
    /// </summary>
    public static string? ValidatePayee(
        PatientRefundSourceAccount source,
        string? payeeBankName,
        string? payeeAccountNumber,
        string? payeeAccountName)
    {
        if (string.IsNullOrWhiteSpace(payeeBankName))
            return "Nama bank tujuan pasien wajib diisi.";
        if (string.IsNullOrWhiteSpace(payeeAccountNumber))
            return "Nomor rekening tujuan pasien wajib diisi.";
        if (string.IsNullOrWhiteSpace(payeeAccountName))
            return "Nama penerima (pemilik rekening tujuan) wajib diisi.";

        var payeeDigits = DigitsOnly(payeeAccountNumber);
        if (payeeDigits.Length > 0 && payeeDigits == DigitsOnly(source.AccountNumber))
        {
            return "Rekening tujuan pasien tidak boleh sama dengan rekening pengirim rumah sakit.";
        }

        return null;
    }

    private static string DigitsOnly(string value) => new(value.Where(char.IsDigit).ToArray());

    private static PatientRefundSourceAccountResult Invalid(string error) => new(null, error);
}
