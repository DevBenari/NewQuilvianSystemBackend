using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Kategori pemeriksaan Radiologi untuk No. Foto — keputusan pemilik modul 2026-10-16.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kategori dibaca dari <b>kode alat pencitraan</b> pesanan (kode modalitas DICOM), bukan dari
    /// kategori master pemeriksaan: kategori hasil impor v1 terlalu kasar (mamografi, densitometri,
    /// dan barium seluruhnya tercatat "X-Ray"), sedangkan alat dipilih saat pesanan dibuat dan
    /// memisahkan kesembilan kategori dengan tepat.
    /// </para>
    /// <para>
    /// Kode alat yang tidak dikenal jatuh ke <c>RAD-01</c> (radiografi konvensional) supaya
    /// Konfirmasi tidak pernah gagal karena master alat bertambah. Alat baru dengan kode DICOM
    /// standar sudah terpetakan di sini — misalnya <c>BMD</c>/<c>DXA</c> untuk RAD-09.
    /// </para>
    /// </remarks>
    public static class RadExamCategory
    {
        public const string Fallback = "RAD-01";

        private static readonly IReadOnlyDictionary<string, string> PerKodeAlat =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // RAD-01 Radiografi Konvensional (X-Ray)
                ["CR"] = "RAD-01", ["DX"] = "RAD-01", ["XR"] = "RAD-01",
                // RAD-02 Ultrasonografi (USG)
                ["US"] = "RAD-02",
                // RAD-03 CT Scan
                ["CT"] = "RAD-03",
                // RAD-04 MRI
                ["MR"] = "RAD-04",
                // RAD-05 Mammografi
                ["MG"] = "RAD-05",
                // RAD-06 Fluoroskopi
                ["RF"] = "RAD-06",
                // RAD-07 Angiografi dan Radiologi Intervensi
                ["XA"] = "RAD-07",
                // RAD-08 Kedokteran Nuklir
                ["NM"] = "RAD-08", ["PT"] = "RAD-08",
                // RAD-09 Pemeriksaan Kepadatan Tulang
                ["BMD"] = "RAD-09", ["DXA"] = "RAD-09",
            };

        /// <summary>Kode kategori (<c>RAD-01</c>..<c>RAD-09</c>) dari kode alat pencitraan.</summary>
        public static string FromModalityCode(string? modalityCode) =>
            !string.IsNullOrWhiteSpace(modalityCode) && PerKodeAlat.TryGetValue(modalityCode.Trim(), out var kode)
                ? kode
                : Fallback;
    }

    /// <summary>
    /// Alokasi No. Foto radiologi, misalnya <c>RAD-03-42</c>: kode kategori pemeriksaan lalu
    /// nomor urut <b>per kategori</b>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Konsep v1</b> (dibaca dari API v1 tayang, 2026-10-16): satu No. Foto per pemeriksaan,
    /// diberikan saat pemeriksaan <b>dikonfirmasi</b> — pemeriksaan yang belum dikonfirmasi belum
    /// bernomor. Pengulangan study tidak mengganti nomornya. Nomor study (<c>RAD-000042</c>) tetap
    /// menjadi pengenal tiap percobaan di konsol pemeriksaan.
    /// </para>
    /// <para>
    /// <b>Proteksi alokasinya</b> (<c>QBE-CODE-003</c>) mengikuti <c>LabOrderNumberService</c>:
    /// <c>MAX + 1</c> di bawah <c>pg_advisory_xact_lock</c> — satu kunci per kategori, sehingga
    /// kategori lain tidak ikut menunggu — dan index unik <c>PhotoNumber</c> sebagai penjaga
    /// terakhir. Celah tidak pernah diisi ulang. Kuncinya hanya berarti di dalam transaksi.
    /// </para>
    /// </remarks>
    public class RadPhotoNumberService
    {
        /// <summary>Lebar minimum nomor urut: RAD-01-01 … RAD-01-11 … RAD-01-100 (keputusan pemilik modul).</summary>
        public const int CodeNumberLength = 2;

        private readonly ApplicationDbContext _dbContext;

        public RadPhotoNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Menyusun satu No. Foto dari kategori dan nilai urutnya.</summary>
        public static string Format(string categoryCode, long number) =>
            $"{categoryCode}-{number.ToString().PadLeft(CodeNumberLength, '0')}";

        /// <summary>Benar bila No. Foto ini milik kategori tersebut.</summary>
        public static bool BelongsTo(string? photoNumber, string categoryCode) =>
            !string.IsNullOrEmpty(photoNumber) && photoNumber.StartsWith(categoryCode + "-", StringComparison.Ordinal);

        /// <summary>Mengalokasikan satu No. Foto baru untuk alat pencitraan tersebut.</summary>
        public async Task<string> AllocateAsync(string? modalityCode, CancellationToken cancellationToken = default)
        {
            var kategori = RadExamCategory.FromModalityCode(modalityCode);

            if (_dbContext.Database.IsNpgsql())
            {
                var kunci = $"RAD_PHOTO_NUMBER_{kategori}";
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext({kunci})::bigint)",
                    cancellationToken);
            }

            return Format(kategori, await ReadLastNumberAsync(kategori, cancellationToken) + 1);
        }

        private async Task<long> ReadLastNumberAsync(string kategori, CancellationToken cancellationToken)
        {
            var awalan = kategori + "-";

            if (_dbContext.Database.IsNpgsql())
            {
                // Dibandingkan sebagai ANGKA: perbandingan teks pecah diam-diam pada digit ketujuh.
                var pola = "^" + awalan + "[0-9]+$";
                var mulai = awalan.Length + 1;

                var tertinggi = await _dbContext.Database
                    .SqlQuery<long?>(
                        $@"SELECT MAX(CAST(SUBSTRING(o.""PhotoNumber"" FROM {mulai}) AS BIGINT)) AS ""Value""
                           FROM public.""RadOrder"" o
                           WHERE o.""PhotoNumber"" ~ {pola}")
                    .SingleAsync(cancellationToken);

                return tertinggi ?? 0;
            }

            // Jalur cadangan selain PostgreSQL (harness uji). Bukan jalur produksi.
            var nomor = await _dbContext.Set<RadOrder>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.PhotoNumber != null && x.PhotoNumber.StartsWith(awalan))
                .Select(x => x.PhotoNumber!)
                .ToListAsync(cancellationToken);

            return nomor
                .Select(x => long.TryParse(x[awalan.Length..], out var nilai) ? nilai : 0)
                .DefaultIfEmpty(0)
                .Max();
        }
    }
}
