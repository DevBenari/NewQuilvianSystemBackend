using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Menyiapkan rentang tanggal dari query string supaya dapat dibandingkan terhadap kolom
    /// <c>timestamp with time zone</c>.
    ///
    /// <b>Kenapa berkas ini ada.</b> Seluruh penyaring tanggal Laboratorium dikontrakkan
    /// bertipe <c>date</c> — <c>LabFilterMetadataFactory</c> menuliskannya sebagai
    /// <c>Example = "2026-09-01"</c> — dan layar memang mengirim <c>YYYY-MM-DD</c> apa adanya.
    /// Nilai seperti itu terikat sebagai <see cref="DateTimeKind.Unspecified"/>, dan Npgsql
    /// <b>menolak</b> menulisnya ke kolom <c>timestamptz</c>. Jawabannya <c>500</c>, bukan
    /// daftar.
    ///
    /// Cacat itu ditemukan 2026-09-17 saat <c>r18</c> diverifikasi, lalu diukur: <b>lima</b>
    /// endpoint terkena — daftar dan rekap pesanan, daftar dan rekap wadah, serta pantau
    /// pemakaian jenis Lainnya — di luar daftar pantau yang diperbaiki lebih dulu.
    ///
    /// <b>Ditempatkan bersama, bukan disalin ke tiap controller.</b> Enam tempat yang menyalin
    /// aturan yang sama pasti bercabang cepat atau lambat, dan cabangnya tidak akan menimbulkan
    /// galat — hanya tanggal yang salah pada satu layar dan benar pada layar lain.
    /// </summary>
    public static class LabQueryDateRange
    {
        /// <summary>
        /// Mengembalikan rentang yang siap dibandingkan.
        ///
        /// <b>Akhir rentang dinaikkan ke penghabisan hari.</b> <c>LAB-DEC-071</c> menetapkan
        /// pembandingnya <b>inklusif</b> supaya pencarian <b>satu hari</b> tetap mungkin. Tanpa
        /// ini, <c>startDate=2026-09-16&amp;endDate=2026-09-16</c> berarti rentang 00:00 sampai
        /// 00:00 dan mengembalikan <b>nol baris</b> — persis kegagalan yang keputusan itu tulis
        /// untuk dicegah.
        ///
        /// Yang dinaikkan <b>hanya</b> nilai yang jamnya tepat tengah malam, yaitu bentuk yang
        /// dihasilkan sebuah tanggal polos. Nilai yang membawa jam sendiri tidak disentuh
        /// jamnya, sehingga pemanggil yang sudah mengirim <c>...T23:59:59Z</c> memperoleh
        /// perilaku yang persis sama seperti sebelumnya.
        /// </summary>
        public static (DateTime? Start, DateTime? End) Normalize(DateTime? start, DateTime? end)
        {
            return (NormalizeStart(start), NormalizeEnd(end));
        }

        public static DateTime? NormalizeStart(DateTime? start)
        {
            return start.HasValue ? AppDateTimeHelper.ToUtc(start.Value) : null;
        }

        public static DateTime? NormalizeEnd(DateTime? end)
        {
            if (!end.HasValue)
            {
                return null;
            }

            var nilai = end.Value;

            if (nilai.TimeOfDay == TimeSpan.Zero)
            {
                nilai = nilai.Date.AddDays(1).AddTicks(-1);
            }

            return AppDateTimeHelper.ToUtc(nilai);
        }
    }
}
