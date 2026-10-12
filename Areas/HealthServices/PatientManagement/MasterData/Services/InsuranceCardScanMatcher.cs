using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Repositories;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Mencocokkan hasil scan kartu asuransi dengan asuransi dan No. polis yang dipilih
    /// (<c>RJ-DOC-REV-BE-020</c>, <c>RJ-DOC-DEC-068</c>..<c>070</c>, <c>RJ-VAL-PM-01</c>/<c>02</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya berjalan bila request membawa <c>CardScan</c>.</b> Selama agent Plustek belum
    /// membaca kartu asuransi (<c>RJ-DOC-OQ-PM-01</c>), layar tidak mengirim <c>CardScan</c> dan
    /// pembuatan penjamin berperilaku persis seperti sebelumnya.
    /// </para>
    /// <para>
    /// <b>Aturan cocok.</b> (1) No. polis hasil scan sama persis dengan No. polis yang diisi
    /// sesudah keduanya dibuat huruf besar dan dibuang semua selain huruf dan angka —
    /// <c>AZ-123 456</c> sama dengan <c>AZ123456</c>. (2) Nama asuransi hasil scan
    /// <b>memuat</b> nama, kode, atau grup asuransi terpilih, tanpa beda huruf besar-kecil —
    /// <c>PT ASURANSI ALLIANZ LIFE</c> cocok dengan <c>Allianz</c>. Kedua syarat wajib.
    /// </para>
    /// <para>Nilai <c>CardScan</c> tidak disimpan di mana pun dan tidak dicatat ke log.</para>
    /// </remarks>
    public class InsuranceCardScanMatcher
    {
        public const string MismatchMessage =
            "RJ-VAL-PM-01: Data tidak match. Nama asuransi atau No. polis pada kartu berbeda dengan data yang dipilih, sehingga asuransi tidak dapat dijadikan penjamin.";

        public const string IncompleteMessage =
            "RJ-VAL-PM-02: Kartu tidak terbaca lengkap. Silakan scan ulang kartu asuransi.";

        private static readonly Regex NonAlphanumeric = new("[^A-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex MultiSpace = new("\\s+", RegexOptions.Compiled);

        private readonly ApplicationDbContext _dbContext;

        public InsuranceCardScanMatcher(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mengembalikan <c>null</c> bila cocok atau tidak ada hasil scan; selain itu pesan penolakan.
        /// </summary>
        public async Task<string?> ValidateAsync(
            Guid insuranceProviderId,
            string? policyNumber,
            PatientInsuranceCardScanRequest? cardScan,
            CancellationToken cancellationToken = default)
        {
            if (cardScan == null)
                return null;

            if (string.IsNullOrWhiteSpace(cardScan.ScannedProviderName) ||
                string.IsNullOrWhiteSpace(cardScan.ScannedPolicyNumber))
            {
                return IncompleteMessage;
            }

            var provider = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => x.Id == insuranceProviderId)
                .Select(x => new
                {
                    x.InsuranceProviderName,
                    x.InsuranceProviderCode,
                    x.InsuranceGroupName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (provider == null)
                return MismatchMessage;

            return IsMatch(
                    cardScan.ScannedProviderName,
                    cardScan.ScannedPolicyNumber,
                    policyNumber,
                    provider.InsuranceProviderName,
                    provider.InsuranceProviderCode,
                    provider.InsuranceGroupName)
                ? null
                : MismatchMessage;
        }

        /// <summary>Fungsi murni pencocokan; dipakai juga sebagai acuan aturan di layar.</summary>
        public static bool IsMatch(
            string scannedProviderName,
            string scannedPolicyNumber,
            string? policyNumber,
            params string?[] providerNames)
        {
            var scannedPolicy = NormalizePolicy(scannedPolicyNumber);

            if (scannedPolicy.Length == 0 || scannedPolicy != NormalizePolicy(policyNumber))
                return false;

            var scannedName = NormalizeName(scannedProviderName);

            return providerNames
                .Select(NormalizeName)
                .Where(name => name.Length > 0)
                .Any(name => scannedName.Contains(name, StringComparison.Ordinal));
        }

        public static string NormalizePolicy(string? value)
            => NonAlphanumeric.Replace((value ?? string.Empty).ToUpperInvariant(), string.Empty);

        public static string NormalizeName(string? value)
            => MultiSpace.Replace((value ?? string.Empty).Trim().ToUpperInvariant(), " ");
    }
}
