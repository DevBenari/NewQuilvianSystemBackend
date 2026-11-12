using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Indikator ketepatan tanda tangan ringkasan pulang Rawat Inap per dokter per periode, untuk
    /// OPPE/FPPE HR (`BE-49`; `RM-DEC-101`, `102`, `105`–`107`; `02-backend-architecture.md` A6.9.2).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Definisi "tepat waktu" milik rekam medis; HR memakai angkanya tanpa menghitung ulang.
    /// Jam mulai = pasien keluar ruangan (cadangan: tutup episode). Jam selesai = tanda tangan
    /// pertama yang terlacak. Tanda tangan sebelum pasien keluar dihitung 0 jam.
    /// </para>
    /// <para>
    /// <b>Contoh.</b> Ibu Sari keluar 30 Oktober 09.00 WIB dan ringkasannya belum ditandatangani.
    /// Ditarik 1 November 12.00 WIB (51 jam) → masuk "&gt; 48 jam, belum ditandatangani" atas DPJP
    /// aktifnya. Pak Andi keluar 1 November 08.00, ditarik 12.00 (4 jam) → belum jatuh tempo, tidak
    /// masuk penyebut.
    /// </para>
    /// <para>
    /// Tidak menulis apa pun dan tidak membuka transaksi. Angka tidak disimpan: periode yang masih
    /// berjalan sah berubah sampai seluruh ringkasan ditandatangani.
    /// </para>
    /// </remarks>
    public sealed class DischargeSummaryTimelinessService
    {
        /// <summary>`DS-R6-01`.</summary>
        public const int MaxPeriodDays = 366;

        public const string SourceUnavailableMessage = "Data Rawat Inap tidak dapat dibaca. Coba lagi.";

        public const string ResignTrackingNote =
            "Tanda tangan ulang tanpa sesi koreksi sebelum perbaikan Rawat Inap tidak terlacak; " +
            "waktu tanda tangan pertama dapat tampak lebih lambat.";

        public const string WithoutDpjpLabel = "Tanpa DPJP aktif";

        private static readonly TimeSpan Wib = TimeSpan.FromHours(7);
        private static readonly TimeSpan Batas24 = TimeSpan.FromHours(24);
        private static readonly TimeSpan Batas48 = TimeSpan.FromHours(48);

        private readonly ApplicationDbContext _dbContext;
        private readonly IReadOnlyList<IDischargeSummaryTimelinessSource> _sources;
        private readonly TimeSpan _batasWaktu;
        private readonly TimeProvider _jam;

        /// <param name="sources">Pembaca dari Rawat Inap (`BE-50`). Kosong = endpoint menjawab 503.</param>
        /// <param name="timeProvider">Jam; kosong = jam sistem. Disuntikkan uji.</param>
        public DischargeSummaryTimelinessService(
            ApplicationDbContext dbContext,
            IEnumerable<IDischargeSummaryTimelinessSource> sources,
            IConfiguration configuration,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _sources = sources.ToList();
            _jam = timeProvider ?? TimeProvider.System;

            // A4.14: satu tarikan membaca seluruh periode sekaligus, sehingga memakai batas waktu
            // berkas (bawaan 20 detik), bukan batas waktu satu sumber (3 detik).
            _batasWaktu = TimeSpan.FromSeconds(Math.Max(1,
                configuration.GetValue("MedicalRecord:FileTimeoutSeconds", MedicalRecordTimelineService.BatasWaktuBerkasBawaanDetik)));
        }

        /// <summary>`VAL-RM-C01`–`C04`. Kosong bila sah.</summary>
        public static string? ValidateQuery(DischargeSummaryTimelinessPagedQuery query)
        {
            if (!query.PeriodFrom.HasValue || !query.PeriodTo.HasValue)
                return "Periode awal dan akhir wajib diisi.";
            if (query.PeriodFrom.Value > query.PeriodTo.Value)
                return "Periode awal tidak boleh sesudah periode akhir.";
            if (query.PeriodTo.Value.DayNumber - query.PeriodFrom.Value.DayNumber + 1 > MaxPeriodDays)
                return $"Rentang periode paling lama {MaxPeriodDays} hari.";
            if (query.PageSize < 1 || query.PageSize > 100)
                return "Ukuran halaman harus 1 sampai 100.";
            if (query.Page < 1)
                return "Halaman harus 1 atau lebih.";
            return null;
        }

        public static DischargeSummaryTimelinessFilterMetadataResponse BuildFilterMetadata() => new()
        {
            MaxPeriodDays = MaxPeriodDays,
            BucketDefinitions =
            [
                new() { Code = "within24h", Label = "≤ 24 jam", Description = "Ditandatangani paling lama 24 jam sesudah pasien keluar ruangan (cadangan: tutup episode). Tanda tangan sebelum pasien keluar dihitung 0 jam." },
                new() { Code = "between24And48h", Label = "24–48 jam", Description = "Lebih dari 24 jam sampai 48 jam, termasuk ringkasan yang belum ditandatangani dan sudah tertunda selama itu." },
                new() { Code = "over48h", Label = "> 48 jam", Description = "Lebih dari 48 jam, termasuk ringkasan yang belum ditandatangani dan sudah tertunda selama itu." },
                new() { Code = "notYetDue", Label = "Belum jatuh tempo", Description = "Belum ditandatangani dan baru tertunda 24 jam atau kurang; tidak masuk penyebut." }
            ]
        };

        /// <summary>
        /// Menyusun indikator. Mengembalikan <c>null</c> bila pembaca Rawat Inap belum terpasang,
        /// galat, atau lewat batas waktu — pemanggil menjawab 503, tidak pernah angka sebagian.
        /// </summary>
        /// <remarks>Query wajib sudah lolos <see cref="ValidateQuery"/>.</remarks>
        public async Task<DischargeSummaryTimelinessPagedResult?> GetPagedAsync(
            DischargeSummaryTimelinessPagedQuery query,
            CancellationToken cancellationToken)
        {
            if (_sources.Count == 0)
                return null;

            var periodeAwal = query.PeriodFrom!.Value;
            var periodeAkhir = query.PeriodTo!.Value;
            var dariUtc = KeUtc(periodeAwal);
            var sampaiUtc = KeUtc(periodeAkhir.AddDays(1));

            var baris = new List<DischargeSummaryTimelinessRow>();
            using (var batas = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                batas.CancelAfter(_batasWaktu);
                try
                {
                    foreach (var sumber in _sources)
                        baris.AddRange(await sumber.ReadAsync(dariUtc, sampaiUtc, batas.Token));
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }

            var sekarang = _jam.GetUtcNow().UtcDateTime;
            var dokterDiminta = await DokterDimintaAsync(query, cancellationToken);

            var dinilai = baris
                .Select(x => Nilai(x, sekarang))
                .Where(x => dokterDiminta == null || (x.DoctorId.HasValue && dokterDiminta.Contains(x.DoctorId.Value)))
                .ToList();

            var idDokter = dinilai.Where(x => x.DoctorId.HasValue).Select(x => x.DoctorId!.Value).Distinct().ToList();
            var dokter = await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(x => idDokter.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.WorkforceProfileId })
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var item = dinilai
                .GroupBy(x => x.DoctorId)
                .Select(g =>
                {
                    var baru = new DischargeSummaryTimelinessItem { DoctorId = g.Key };
                    if (g.Key.HasValue && dokter.TryGetValue(g.Key.Value, out var d))
                    {
                        baru.DoctorName = d.FullName;
                        baru.WorkforceProfileId = d.WorkforceProfileId;
                    }
                    else
                    {
                        baru.DoctorName = g.Key.HasValue ? "Dokter tidak ditemukan" : WithoutDpjpLabel;
                    }

                    foreach (var n in g)
                        Tambah(baru, n);
                    return baru;
                })
                .OrderBy(x => x.DoctorId.HasValue ? 0 : 1)
                .ThenByDescending(x => x.Over48h)
                .ThenBy(x => x.DoctorName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var total = new DischargeSummaryTimelinessCounts();
            foreach (var n in dinilai)
                Tambah(total, n);

            return new DischargeSummaryTimelinessPagedResult
            {
                PeriodFrom = periodeAwal,
                PeriodTo = periodeAkhir,
                GeneratedAt = sekarang,
                Totals = total,
                DataNotes = new DischargeSummaryTimelinessDataNotes
                {
                    ClosedAtFallbackCount = dinilai.Count(x => x.StartedFromClosedAt),
                    SignedBeforeDepartureCount = dinilai.Count(x => x.SignedBeforeDeparture),
                    PeriodStillChanging = dinilai.Any(x => !x.Signed),
                    ResignTrackingNote = ResignTrackingNote
                },
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalData = item.Count,
                TotalPage = (int)Math.Ceiling(item.Count / (double)query.PageSize),
                Items = item.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList()
            };
        }

        // ---- aturan A6.9.2 ----

        private enum Kolom { BelumJatuhTempo, Dalam24, Antara24Dan48, Lebih48 }

        private sealed record Penilaian(Guid? DoctorId, Kolom Kolom, bool Signed, bool StartedFromClosedAt, bool SignedBeforeDeparture);

        private static Penilaian Nilai(DischargeSummaryTimelinessRow x, DateTime sekarang)
        {
            if (x.FirstSignedAtUtc.HasValue)
            {
                var selisih = x.FirstSignedAtUtc.Value - x.StartAtUtc;
                var lama = selisih < TimeSpan.Zero ? TimeSpan.Zero : selisih;
                return new Penilaian(x.FirstSignerDoctorId, KolomUntuk(lama), true, x.StartedFromClosedAt, selisih < TimeSpan.Zero);
            }

            var tertunda = sekarang - x.StartAtUtc;
            var kolom = tertunda <= Batas24 ? Kolom.BelumJatuhTempo : KolomUntuk(tertunda);
            return new Penilaian(x.ActiveDpjpDoctorId, kolom, false, x.StartedFromClosedAt, false);
        }

        /// <summary>`DS-R6-07`: tepat 24:00:00 masuk ≤ 24 jam; tepat 48:00:00 masuk 24–48 jam.</summary>
        private static Kolom KolomUntuk(TimeSpan lama)
            => lama <= Batas24 ? Kolom.Dalam24 : lama <= Batas48 ? Kolom.Antara24Dan48 : Kolom.Lebih48;

        private static void Tambah(DischargeSummaryTimelinessCounts hitungan, Penilaian n)
        {
            switch (n.Kolom)
            {
                case Kolom.BelumJatuhTempo:
                    hitungan.NotYetDueCount++;
                    return;
                case Kolom.Dalam24:
                    hitungan.Within24h++;
                    break;
                case Kolom.Antara24Dan48:
                    hitungan.Between24And48h++;
                    if (!n.Signed)
                        hitungan.UnsignedBetween24And48h++;
                    break;
                case Kolom.Lebih48:
                    hitungan.Over48h++;
                    if (!n.Signed)
                        hitungan.UnsignedOver48h++;
                    break;
            }

            hitungan.AssessedCount++;
        }

        /// <summary>Kosong = tanpa saringan dokter. Dokter atau profil yang tidak dikenal → himpunan kosong, bukan galat.</summary>
        private async Task<HashSet<Guid>?> DokterDimintaAsync(
            DischargeSummaryTimelinessPagedQuery query,
            CancellationToken cancellationToken)
        {
            if (!query.DoctorId.HasValue && !query.WorkforceProfileId.HasValue)
                return null;

            var hasil = new HashSet<Guid>();
            if (query.WorkforceProfileId.HasValue)
            {
                var dariProfil = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == query.WorkforceProfileId.Value)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                hasil.UnionWith(dariProfil);
                if (query.DoctorId.HasValue)
                    hasil.IntersectWith([query.DoctorId.Value]);
            }
            else
            {
                hasil.Add(query.DoctorId!.Value);
            }

            return hasil;
        }

        /// <summary>Tengah malam WIB tanggal itu, dalam UTC.</summary>
        private static DateTime KeUtc(DateOnly tanggal)
            => DateTime.SpecifyKind(tanggal.ToDateTime(TimeOnly.MinValue) - Wib, DateTimeKind.Utc);
    }
}
