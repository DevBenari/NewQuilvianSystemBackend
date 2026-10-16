using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services
{
    /// <summary>
    /// Alokasi No. Foto radiologi — nomor study yang pendek dan dapat disebut lewat telepon,
    /// misalnya <c>RAD-000042</c>.
    ///
    /// <para>
    /// <b>Mengapa tidak lagi dibentuk dari Guid pesanan.</b> Bentuk lama
    /// <c>RAD-{Guid pesanan}-{urut}</c> panjangnya 40 karakter dan tidak dapat dibacakan,
    /// padahal di v1 No. Foto adalah nomor pendek yang tercetak pada label dan amplop film.
    /// Pemilik modul memilih bentuk yang sama keluarganya dengan nomor Lab
    /// (<c>LAB-RSMMC-000042</c>, <c>LAB-DEC-072</c>): awalan tetap dan nomor urut berpadding.
    /// </para>
    ///
    /// <para>
    /// <b>Proteksi alokasinya</b> (<c>QBE-CODE-003</c>) sama dengan
    /// <c>LabOrderNumberService</c>: <c>MAX + 1</c> dibaca di bawah
    /// <c>pg_advisory_xact_lock</c>, sehingga dua study yang dibuat bersamaan tidak membaca angka
    /// yang sama, dan index unik <c>StudyNumber</c> tetap menjadi penjaga terakhirnya. Celah
    /// tidak pernah diisi ulang — nomor yang sudah tercetak pada film tidak boleh berpindah
    /// pemilik. Kuncinya hanya berarti di dalam transaksi; pemanggil wajib membungkusnya.
    /// </para>
    /// </summary>
    public class RadStudyNumberService
    {
        public const string StudyNumberPrefix = "RAD-";

        /// <summary>Lebar tampilan minimum. Nomor di atas enam digit tetap benar.</summary>
        public const int StudyNumberLength = 6;

        /// <summary>
        /// Hanya baris yang bentuknya memang No. Foto yang dibaca; tanpanya satu baris berformat
        /// lain membuat <c>CAST</c> gagal dan seluruh pembuatan study berhenti.
        /// </summary>
        private const string NumberPattern = "^" + StudyNumberPrefix + "[0-9]+$";

        private readonly ApplicationDbContext _dbContext;

        public RadStudyNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Menyusun satu No. Foto dari nilai urutnya.</summary>
        public static string Format(long number) =>
            StudyNumberPrefix + number.ToString().PadLeft(StudyNumberLength, '0');

        /// <summary>Mengalokasikan satu No. Foto baru.</summary>
        public async Task<string> AllocateOneAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext('RAD_STUDY_NUMBER')::bigint)",
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
                var prefixLength = StudyNumberPrefix.Length;

                var tertinggi = await _dbContext.Database
                    .SqlQuery<long?>(
                        $@"SELECT MAX(CAST(SUBSTRING(s.""StudyNumber"" FROM {prefixLength + 1}) AS BIGINT)) AS ""Value""
                           FROM public.""RadStudy"" s
                           WHERE s.""StudyNumber"" ~ {NumberPattern}")
                    .SingleAsync(cancellationToken);

                return tertinggi ?? 0;
            }

            // Jalur cadangan selain PostgreSQL (harness uji InMemory). Diurutkan sebagai angka di
            // memori; jalur ini bukan jalur produksi.
            var nomor = await _dbContext.Set<RadStudy>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.StudyNumber.StartsWith(StudyNumberPrefix))
                .Select(x => x.StudyNumber)
                .ToListAsync(cancellationToken);

            return nomor
                .Select(x => long.TryParse(x[StudyNumberPrefix.Length..], out var nilai) ? nilai : 0)
                .DefaultIfEmpty(0)
                .Max();
        }
    }
}
