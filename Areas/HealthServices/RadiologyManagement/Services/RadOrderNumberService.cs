using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Alokasi nomor pesanan radiologi, misalnya <c>RAD-RSMMC-000042</c>.
    /// Tidak memakai interface, mengikuti pola project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Mengapa pesanan perlu nomornya sendiri.</b> Guid tidak dapat dibacakan lewat telepon,
    /// tidak muat pada label, dan tidak dapat diketik petugas yang memegang lembar permintaan.
    /// </para>
    /// <para>
    /// <b>Bentuknya sama dengan nomor pesanan Lab</b> (<c>LAB-RSMMC-000042</c>,
    /// <c>LabOrderNumberService</c>, <c>LAB-DEC-072</c>) — keputusan pemilik modul 2026-10-16.
    /// Bentuk lama <c>RAD-ORD-{yyMMddHHmmss}-{6 acak}</c> (27 karakter) tidak dapat disebut.
    /// </para>
    /// <para>
    /// <b>Proteksi alokasinya</b> (<c>QBE-CODE-003</c>) mengikuti Lab: <c>MAX + 1</c> dibaca di
    /// bawah <c>pg_advisory_xact_lock</c>, index unik <c>IX_RadOrder_OrderNumber</c> menjadi
    /// penjaga terakhir, dan celah tidak pernah diisi ulang — nomor yang sudah tercetak pada label
    /// tidak boleh berpindah pemilik. Kuncinya hanya berarti di dalam transaksi; pemanggil wajib
    /// membungkusnya.
    /// </para>
    /// </remarks>
    public class RadOrderNumberService
    {
        public const string OrderCodePrefix = "RAD-RSMMC-";

        /// <summary>Lebar tampilan minimum, sama dengan Lab. Nomor di atas enam digit tetap benar.</summary>
        public const int CodeNumberLength = 6;

        /// <summary>
        /// Hanya baris yang bentuknya memang nomor pesanan yang dibaca; tanpanya satu baris
        /// berformat lain membuat <c>CAST</c> gagal dan seluruh pembuatan pesanan berhenti.
        /// </summary>
        private const string NumberPattern = "^" + OrderCodePrefix + "[0-9]+$";

        private readonly ApplicationDbContext _dbContext;

        public RadOrderNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Menyusun satu nomor dari nilai urutnya.</summary>
        public static string Format(long number) =>
            OrderCodePrefix + number.ToString().PadLeft(CodeNumberLength, '0');

        /// <summary>Mengalokasikan satu nomor pesanan baru.</summary>
        public async Task<string> AllocateOneAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext('RAD_ORDER_NUMBER')::bigint)",
                    cancellationToken);
            }

            var last = await ReadLastNumberAsync(cancellationToken);

            return Format(last + 1);
        }

        private async Task<long> ReadLastNumberAsync(CancellationToken cancellationToken)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                // Dibandingkan sebagai ANGKA: perbandingan teks pecah diam-diam pada digit ketujuh.
                var prefixLength = OrderCodePrefix.Length;

                var tertinggi = await _dbContext.Database
                    .SqlQuery<long?>(
                        $@"SELECT MAX(CAST(SUBSTRING(o.""OrderNumber"" FROM {prefixLength + 1}) AS BIGINT)) AS ""Value""
                           FROM public.""RadOrder"" o
                           WHERE o.""OrderNumber"" ~ {NumberPattern}")
                    .SingleAsync(cancellationToken);

                return tertinggi ?? 0;
            }

            // Jalur cadangan selain PostgreSQL (harness uji). Bukan jalur produksi.
            var nomor = await _dbContext.Set<RadOrder>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.OrderNumber.StartsWith(OrderCodePrefix))
                .Select(x => x.OrderNumber)
                .ToListAsync(cancellationToken);

            return nomor
                .Select(x => long.TryParse(x[OrderCodePrefix.Length..], out var nilai) ? nilai : 0)
                .DefaultIfEmpty(0)
                .Max();
        }
    }
}
