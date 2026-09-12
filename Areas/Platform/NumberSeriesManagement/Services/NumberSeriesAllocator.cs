using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services
{
    /// <summary>
    /// Alokator nomor bisnis bersama. Satu-satunya cara sah menerbitkan nomor bisnis pada kode
    /// baru (<c>QBE-CODE-006</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Perbedaan tunggal dari mesin yang diekstrak, dan seluruh nilai slice ini ada di
    /// situ.</b> Algoritmanya diambil utuh dari <c>BillingNumberSeriesService.AllocateNumberAsync</c>
    /// yang sudah terbukti — kunci penasihat, kunci deret, kunci periode, perakitan
    /// <c>awalan-periode-urut</c>. Yang berubah hanya <b>tempat pencacahnya di-<c>commit</c></b>:
    /// mesin lama menaikkannya di dalam transaksi pemanggil, alokator ini menaikkannya pada
    /// transaksi dan koneksinya sendiri (<c>DEC-PLT-007</c>, <c>DEC-PLT-008</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Akibat perubahan itu — dan ini yang paling perlu dipahami.</b> Ketika pekerjaan bisnis
    /// pemanggil dibatalkan setelah nomor terbit, nomornya <b>hangus selamanya</b>. Deret
    /// berlubang, dan lubang itu <b>tidak boleh</b> diisi (<c>INV-PLT-002</c>). Perilaku ini
    /// dipilih sadar: lebih baik ada nomor yang tidak terpakai daripada ada satu nomor yang
    /// menempel pada dua catatan — petugas mungkin sudah sempat melihat, mencatat, atau
    /// menyebutkan nomor itu sebelum pekerjaannya batal (<c>INV-PLT-001</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Contoh berangka.</b> Petugas membuat order darah, alokator menerbitkan nomor urut
    /// ke-123, lalu validasi menolak order itu. Order berikutnya mendapat ke-124 — bukan ke-123.
    /// Posisi 123 kosong selamanya.
    /// </para>
    ///
    /// <para>
    /// <b>Nol nomor dihitung dari data.</b> Nilai diambil dari pencacah tersimpan, tidak pernah
    /// dari <c>Count+1</c>, <c>Max+1</c>, maupun pemindaian nomor yang sudah terbit
    /// (<c>QBE-CODE-003</c>). Menghitung dari data akan mengabaikan lubang yang sah dan
    /// menerbitkan ulang nomor yang sudah menempel.
    /// </para>
    ///
    /// <para>
    /// <b>Kunci dipegang sesingkat mungkin.</b> <c>pg_advisory_xact_lock</c> hidup di dalam
    /// transaksi alokasi yang umurnya hanya beberapa milidetik, bukan sepanjang transaksi bisnis
    /// pemanggil. Konsekuensi yang menguntungkan: satu pekerjaan bisnis yang lambat tidak lagi
    /// menahan alokasi nomor di belakangnya — berbeda dari mesin lama.
    /// </para>
    ///
    /// <para>
    /// <b>Platform tidak memiliki awalan maupun format.</b> Keempatnya datang dari modul
    /// pemanggil lewat <see cref="NumberAllocationRequest"/> (<c>DEC-PLT-005</c>).
    /// </para>
    /// </remarks>
    public class NumberSeriesAllocator
    {
        private const int MaxPrefixLength = 15;
        private const int MinSequenceDigits = 4;
        private const int MaxSequenceDigits = 12;

        /// <summary>Periode untuk deret yang tidak pernah diulang (<c>DEC-PLT-004</c>).</summary>
        private const string GlobalScopeKey = "GLOBAL";

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        /// <remarks>
        /// <b>Alokator sengaja TIDAK menerima <c>ApplicationDbContext</c> scoped.</b> Menerimanya
        /// berarti menulis pencacah di dalam transaksi pemanggil, dan itu persis perilaku yang
        /// ditolak <c>DEC-PLT-008</c>. Factory-lah yang memberi koneksi tersendiri.
        /// </remarks>
        public NumberSeriesAllocator(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Menerbitkan satu nomor bisnis, atomik dan durabel.
        /// </summary>
        /// <exception cref="NumberSeriesAllocationException">
        /// Permintaan ditolak. Setiap penolakan terjadi <b>sebelum</b> pencacah di-<c>commit</c>,
        /// sehingga nol nomor terbit dan deret tidak berlubang karenanya.
        /// </exception>
        public async Task<string> AllocateAsync(
            NumberAllocationRequest request,
            CancellationToken cancellationToken = default)
        {
            var prefix = ValidateAndNormalize(request);
            var resetPolicy = request.ResetPolicy.Trim().ToUpperInvariant();
            var scopeKey = ResolveScopeKey(resetPolicy, request.Instant);

            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            // Transaksi milik alokator sendiri. Ia di-commit terlepas dari nasib pekerjaan
            // pemanggil — inilah yang membuat nomor hangus, bukan dipakai ulang (DEC-PLT-008).
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            await AcquireSeriesLockAsync(context, request.SequenceKey, scopeKey, cancellationToken);

            var series = await context.NumNumberSeries.SingleOrDefaultAsync(
                x => x.SequenceKey == request.SequenceKey && x.ScopeKey == scopeKey,
                cancellationToken);

            var now = DateTime.UtcNow;
            long nextValue;

            if (series is null)
            {
                nextValue = 1;

                context.NumNumberSeries.Add(new NumNumberSeries
                {
                    Id = Guid.NewGuid(),
                    SequenceKey = request.SequenceKey,
                    ScopeKey = scopeKey,
                    ResetPolicy = resetPolicy,
                    CurrentValue = nextValue,
                    LastAllocatedAt = request.Instant,
                    CreateDateTime = now,
                    CreateBy = request.ActorUserId
                });
            }
            else
            {
                // checked: melampaui batas long dilempar, bukan berputar diam-diam ke negatif.
                try
                {
                    checked { nextValue = series.CurrentValue + 1; }
                }
                catch (OverflowException ex)
                {
                    throw new NumberSeriesAllocationException(
                        "Deret nomor ini sudah mencapai batas dan tidak dapat dinaikkan lagi.",
                        ex)
                    { ValidationCode = "VAL-PLT-006" };
                }

                series.CurrentValue = nextValue;
                series.LastAllocatedAt = request.Instant;
                series.UpdateDateTime = now;
                series.UpdateBy = request.ActorUserId;
            }

            var sequence = nextValue.ToString($"D{request.SequenceDigits}");

            // VAL-PLT-007 diperiksa SEBELUM commit. Menerbitkan nomor yang bentuknya menyimpang
            // lebih buruk daripada menolaknya: bentuk yang menyimpang baru ketahuan jauh di
            // belakang, ketika nomornya sudah menempel pada catatan. Pelajaran dari FACT-PLT-009,
            // yaitu LegalEntityController yang deretnya habis diam-diam setelah 999.
            if (sequence.Length > request.SequenceDigits)
            {
                throw new NumberSeriesAllocationException(
                    "Nilai deret melampaui jumlah digit yang dikonfigurasi.")
                { ValidationCode = "VAL-PLT-007" };
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return scopeKey == GlobalScopeKey
                ? $"{prefix}-{sequence}"
                : $"{prefix}-{scopeKey}-{sequence}";
        }

        /// <summary>
        /// Mengantrekan permintaan pada deret dan periode yang sama.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kunci ini <b>mengantre</b>, bukan menolak. Dua permintaan bersamaan pada deret yang
        /// sama diselesaikan berurutan tanpa satu pun menerima galat; permintaan pada deret
        /// berbeda tidak ikut menunggu karena kuncinya diturunkan dari penanda deret dan
        /// periodenya.
        /// </para>
        /// <para>
        /// <b>Batas yang jujur.</b> <c>pg_advisory_xact_lock</c> hanya ada di PostgreSQL. Pada
        /// provider lain — yang di repository ini berarti pengujian — langkah ini dilewati, dan
        /// yang tersisa sebagai penjaga adalah index unik <c>(SequenceKey, ScopeKey)</c> di
        /// database. Karena itu <b>perilaku berebut tidak dapat dibuktikan di luar PostgreSQL</b>,
        /// dan pembuktiannya menjadi lingkup <c>PLT-BE-004</c>.
        /// </para>
        /// </remarks>
        private static async Task AcquireSeriesLockAsync(
            ApplicationDbContext context,
            string sequenceKey,
            string scopeKey,
            CancellationToken cancellationToken)
        {
            if (!context.Database.IsNpgsql())
                return;

            var lockKey = $"NUM_SERIES_{sequenceKey}_{scopeKey}";

            await context.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [lockKey],
                cancellationToken);
        }

        /// <summary>
        /// Menghitung penanda periode dari kebijakan pengulangan.
        /// </summary>
        /// <remarks>
        /// Deret baru selalu berakhir di <c>GLOBAL</c> karena <c>DEC-PLT-004</c> menetapkan
        /// <c>NEVER</c>. Ketiga cabang lain hanya dilalui deret lama yang kelak dipindahkan
        /// <c>PLT-SLICE-02</c>, dan perhitungannya memakai zona waktu bisnis — bukan UTC — supaya
        /// pergantian hari terjadi pada tengah malam setempat.
        /// </remarks>
        private static string ResolveScopeKey(string resetPolicy, DateTimeOffset instant)
        {
            if (resetPolicy == NumberSeriesResetPolicies.Never)
                return GlobalScopeKey;

            var local = TimeZoneInfo.ConvertTime(instant, ResolveBusinessTimeZone());

            return resetPolicy switch
            {
                NumberSeriesResetPolicies.Yearly => local.ToString("yyyy"),
                NumberSeriesResetPolicies.Monthly => local.ToString("yyyyMM"),
                NumberSeriesResetPolicies.Daily => local.ToString("yyyyMMdd"),
                _ => throw new NumberSeriesAllocationException(
                    "Kebijakan pengulangan nomor harus NEVER, YEARLY, MONTHLY, atau DAILY.")
                { ValidationCode = "VAL-PLT-003" }
            };
        }

        /// <remarks>Mengikuti mesin yang diekstrak: <c>Asia/Jakarta</c>, cadangan Windows.</remarks>
        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        /// <summary>
        /// Memeriksa seluruh parameter sebelum satu baris pun disentuh.
        /// </summary>
        /// <remarks>
        /// <b>Parameter yang salah ditolak, bukan ditambal nilai bawaan platform.</b> Awalan dan
        /// panjang nomor milik modul (<c>DEC-PLT-005</c>); menambalnya berarti menerbitkan nomor
        /// berbentuk lain dari yang dirancang modulnya — dan bentuk itu terlanjur menempel
        /// permanen pada catatan.
        /// </remarks>
        private static string ValidateAndNormalize(NumberAllocationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SequenceKey))
            {
                throw new NumberSeriesAllocationException("Penanda deret nomor wajib diisi.")
                { ValidationCode = "VAL-PLT-001" };
            }

            if (string.IsNullOrWhiteSpace(request.Prefix))
            {
                throw new NumberSeriesAllocationException(
                    "Awalan nomor wajib diisi dan maksimal 15 karakter.")
                { ValidationCode = "VAL-PLT-002" };
            }

            var prefix = request.Prefix.Trim().ToUpperInvariant();

            if (prefix.Length > MaxPrefixLength)
            {
                throw new NumberSeriesAllocationException(
                    "Awalan nomor wajib diisi dan maksimal 15 karakter.")
                { ValidationCode = "VAL-PLT-002" };
            }

            if (string.IsNullOrWhiteSpace(request.ResetPolicy)
                || !NumberSeriesResetPolicies.All.Contains(request.ResetPolicy.Trim()))
            {
                throw new NumberSeriesAllocationException(
                    "Kebijakan pengulangan nomor harus NEVER, YEARLY, MONTHLY, atau DAILY.")
                { ValidationCode = "VAL-PLT-003" };
            }

            if (request.SequenceDigits is < MinSequenceDigits or > MaxSequenceDigits)
            {
                throw new NumberSeriesAllocationException(
                    "Jumlah digit nomor harus antara 4 dan 12.")
                { ValidationCode = "VAL-PLT-004" };
            }

            if (request.ActorUserId == Guid.Empty)
            {
                throw new NumberSeriesAllocationException("Petugas pelaku tidak dikenali.")
                { ValidationCode = "VAL-PLT-005" };
            }

            return prefix;
        }
    }
}
