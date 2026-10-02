using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Rentang breakpoint yang berlaku bagi satu pasangan organisme dan antibiotik, beserta
    /// kandungan cakram antibiotiknya — seluruhnya disalin menjadi snapshot pada baris hasil.
    /// </summary>
    /// <param name="LowerMm">Batas bawah; zona di bawahnya menghasilkan <c>Resistant</c>.</param>
    /// <param name="UpperMm">Batas atas; zona di atasnya menghasilkan <c>Sensitive</c>.</param>
    /// <param name="DiscContentUg">Kandungan cakram, boleh kosong.</param>
    public readonly record struct LabBreakpointSnapshot(int LowerMm, int UpperMm, int? DiscContentUg);

    /// <summary>
    /// Hasil penilaian satu baris kepekaan: nilai yang dihitung, nilai yang berlaku, dan
    /// apakah analis menimpanya.
    /// </summary>
    public readonly record struct LabSusceptibilityVerdict(
        LabSusceptibilityResult? ComputedResult,
        LabSusceptibilityResult Result,
        bool IsResultOverridden);

    /// <summary>
    /// Menghitung interpretasi <c>S</c>/<c>I</c>/<c>R</c> dari lebar zona hambat terhadap
    /// rentang breakpoint (<c>LAB-DEC-123</c>, <c>BE-LAB-61</c>).
    ///
    /// <b>Dasarnya bukti, bukan dugaan.</b> Seluruh 22 baris pada <c>LAB-EVD-006</c> konsisten
    /// terhadap satu aturan, termasuk ketiga batasnya — Netilmicin zona <c>13</c> pada rentang
    /// <c>12-15</c> menghasilkan <c>I</c>; Fosfomycin zona <c>11</c> pada <c>12-16</c>
    /// menghasilkan <c>R</c>; Imipenem zona <c>32</c> pada <c>13-16</c> menghasilkan <c>S</c>.
    ///
    /// <b>Angka cutoff nol dihardcode di sini</b> (<c>LAB-DEC-039</c>). Rentangnya datang dari
    /// <c>LabSusceptibilityBreakpoint</c>; kelas ini hanya mengetahui <i>bentuk</i> aturannya.
    /// </summary>
    public class LabSusceptibilityInterpreter
    {
        private readonly ApplicationDbContext _dbContext;

        public LabSusceptibilityInterpreter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Inti perhitungan. <b>Fungsi murni</b> — nol menyentuh database, sehingga perilakunya
        /// dapat diperiksa langsung.
        ///
        /// <list type="table">
        /// <item><term>zona &lt; batas bawah</term><description><c>Resistant</c></description></item>
        /// <item><term>batas bawah ≤ zona ≤ batas atas</term><description><c>Intermediate</c></description></item>
        /// <item><term>zona &gt; batas atas</term><description><c>Sensitive</c></description></item>
        /// <item><term>zona kosong, atau rentang tidak tersedia</term><description><c>null</c></description></item>
        /// </list>
        ///
        /// <b>Zona bernilai <c>0</c> DIHITUNG, bukan diabaikan</b> (<c>LAB-DEC-128</c>). Ia
        /// pengukuran sah yang berarti nol zona hambat terbentuk — sebelas dari 22 baris pada
        /// <c>LAB-EVD-006</c> bernilai <c>0</c> dan seluruhnya <c>R</c>. Hanya ruas
        /// <b>kosong</b> yang berarti belum diukur.
        /// </summary>
        public static LabSusceptibilityResult? Compute(int? zoneDiameterMm, LabBreakpointSnapshot? breakpoint)
        {
            if (zoneDiameterMm is null || breakpoint is null)
                return null;

            var zone = zoneDiameterMm.Value;
            var range = breakpoint.Value;

            // Perhatikan tandanya. Batas bawah dan batas atas keduanya TERMASUK ke dalam
            // Intermediate: zona yang persis sama dengan batas bawah bukan Resistant, dan
            // zona yang persis sama dengan batas atas bukan Sensitive. Netilmicin zona 13
            // pada rentang 12-15 membuktikannya — ia I, bukan R.
            if (zone < range.LowerMm)
                return LabSusceptibilityResult.Resistant;

            if (zone > range.UpperMm)
                return LabSusceptibilityResult.Sensitive;

            return LabSusceptibilityResult.Intermediate;
        }

        /// <summary>
        /// Menggabungkan hitungan sistem dengan nilai yang dikirim analis.
        ///
        /// <b>Tiga keadaan, dan ketiganya sah:</b>
        /// <list type="number">
        /// <item>Rentang tersedia dan analis nol mengirim nilai — hitungan dipakai.</item>
        /// <item>Rentang tersedia dan analis mengirim nilai <b>berbeda</b> — itu penimpaan,
        /// dan alasannya <b>wajib</b> (<c>VAL-113</c>). Penimpaan dibuka sebab
        /// <b>resistensi intrinsik</b> menuntut penilaian di luar rumus.</item>
        /// <item>Rentang <b>tidak tersedia</b> — nilai dari analis menjadi satu-satunya
        /// sumber, dan ia <b>wajib</b> (<c>VAL-114</c>). Ini keadaan yang pasti terjadi lebih
        /// dulu, sebab data induk breakpoint dimulai kosong.</item>
        /// </list>
        /// </summary>
        /// <exception cref="LabSusceptibilityInterpretationException">
        /// Nilai wajib tidak dikirim, atau penimpaan tidak beralasan.
        /// </exception>
        public static LabSusceptibilityVerdict Resolve(
            int? zoneDiameterMm,
            LabBreakpointSnapshot? breakpoint,
            LabSusceptibilityResult? providedResult,
            string? overrideReason,
            string antibioticNameForMessage)
        {
            var computed = Compute(zoneDiameterMm, breakpoint);

            if (computed is null)
            {
                // VAL-114. Tanpa rentang, sistem nol punya dasar menghitung apa pun.
                if (providedResult is null)
                {
                    throw new LabSusceptibilityInterpretationException(
                        $"Breakpoint untuk {antibioticNameForMessage} belum disetel, " +
                        "jadi interpretasinya harus diisi sendiri.");
                }

                return new LabSusceptibilityVerdict(null, providedResult.Value, false);
            }

            if (providedResult is null || providedResult.Value == computed.Value)
                return new LabSusceptibilityVerdict(computed, computed.Value, false);

            // VAL-113. Yang diwajibkan alasannya, bukan kepatuhannya — menutup penimpaan
            // berarti melaporkan Sensitive pada kuman yang secara alami kebal.
            if (string.IsNullOrWhiteSpace(overrideReason))
            {
                throw new LabSusceptibilityInterpretationException(
                    $"Interpretasi {antibioticNameForMessage} berbeda dari hitungan sistem; " +
                    "tuliskan alasannya.");
            }

            return new LabSusceptibilityVerdict(computed, providedResult.Value, true);
        }

        /// <summary>
        /// Memuat seluruh rentang yang berlaku bagi satu organisme, dipetakan menurut
        /// antibiotiknya.
        ///
        /// <b>Sekali kueri untuk seluruh baris kepekaan satu isolat</b>, bukan satu kueri per
        /// baris — satu antibiogram lazim memuat dua puluh baris lebih.
        /// </summary>
        public async Task<IReadOnlyDictionary<Guid, LabBreakpointSnapshot>> LoadBreakpointsAsync(
            Guid labOrganismId,
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .Include(x => x.LabAntibiotic)
                .Where(x => !x.IsDelete && x.IsActive && x.LabOrganismId == labOrganismId)
                .Select(x => new
                {
                    x.LabAntibioticId,
                    x.LowerMm,
                    x.UpperMm,
                    DiscContentUg = x.LabAntibiotic!.DiscContentUg
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                x => x.LabAntibioticId,
                x => new LabBreakpointSnapshot(x.LowerMm, x.UpperMm, x.DiscContentUg));
        }

        /// <summary>
        /// Apakah organisme ini punya sedikitnya satu rentang yang berlaku.
        ///
        /// Menjadi ruas turunan <c>breakpointAvailable</c> pada pembacaan hasil
        /// (<c>LAB-API-v1</c> <c>r27</c> bagian 22.3). <b>Tanpa ruas itu</b>, layar tidak dapat
        /// membedakan <i>"interpretasinya memang perlu diketik"</i> dari <i>"sistem gagal
        /// menghitung"</i> — keduanya terlihat persis sama: ruas kosong.
        /// </summary>
        public async Task<bool> HasAnyBreakpointAsync(
            Guid labOrganismId,
            CancellationToken cancellationToken = default)
            => await _dbContext.LabSusceptibilityBreakpoints
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.IsActive && x.LabOrganismId == labOrganismId, cancellationToken);
    }

    /// <summary>
    /// Interpretasi tidak dapat ditetapkan: nilai wajib tidak dikirim, atau penimpaan tidak
    /// beralasan. Dipetakan menjadi <c>422</c>.
    /// </summary>
    public sealed class LabSusceptibilityInterpretationException(string message) : Exception(message);
}
