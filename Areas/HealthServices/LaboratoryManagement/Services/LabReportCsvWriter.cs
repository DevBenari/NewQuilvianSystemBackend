using System.Globalization;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Menulis ketiga laporan operasional menjadi CSV yang <b>terbuka benar di Excel berbahasa
    /// Indonesia</b> (<c>LAB-API-v1</c> <c>r37</c> 32.3, <c>02-backend-architecture.md</c> 23.10 butir 1) —
    /// <b>tanpa pustaka baru</b>.
    ///
    /// <para>
    /// Aturannya: UTF-8 dengan BOM (tanpa BOM, Excel membaca huruf non-ASCII sebagai sandi lain);
    /// pemisah <b>titik koma</b> dan desimal <b>koma</b> (pengaturan regional Indonesia memakai koma
    /// sebagai desimal, sehingga pemisah koma akan memecah angka); baris pertama periode laporan;
    /// judul kolom Bahasa Indonesia; nilai kosong ditulis kosong — bukan 0; nilai berisi titik koma,
    /// tanda kutip, atau pindah baris diapit tanda kutip.
    /// </para>
    ///
    /// <para>
    /// <b>Berkas dibentuk dari respons laporan yang sama dengan layar</b>, bukan dari kueri kedua,
    /// sehingga isi unduhan tidak dapat berbeda dari yang dibaca di layar.
    /// </para>
    /// </summary>
    public class LabReportCsvWriter
    {
        private const char Pemisah = ';';

        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        private static readonly NumberFormatInfo DesimalKoma = new()
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = string.Empty
        };

        /// <summary>
        /// Jumlah pemeriksaan: ringkasan per disiplin, lalu rincian per jenis pemeriksaan.
        /// <see cref="LabCsvFile.RowCount"/> = baris ringkasan (satu per disiplin).
        /// </summary>
        public LabCsvFile Write(LabExaminationCountReportResponse report)
        {
            var csv = Mulai(report.Period);

            Baris(csv, "Disiplin", "Jumlah dirilis", "Keterangan");
            foreach (var row in report.Rows)
            {
                Baris(csv, row.DisciplineName, Angka(row.Total), row.NotCountableReason);
            }
            Baris(csv, "Total terhitung", Angka(report.TotalCountable), null);

            csv.Append("\r\n");
            Baris(csv, "Disiplin", "Jenis pemeriksaan", "Jumlah dirilis");
            foreach (var row in report.Rows)
            {
                foreach (var procedure in row.Procedures)
                {
                    Baris(csv, row.DisciplineName, procedure.ProcedureName, Angka(procedure.Total));
                }
            }

            return Selesai(csv, report.Rows.Count);
        }

        /// <summary>
        /// Penolakan wadah: angka per disiplin, lalu rincian per alasan.
        /// <see cref="LabCsvFile.RowCount"/> = baris per disiplin.
        /// </summary>
        public LabCsvFile Write(LabSpecimenRejectionReportResponse report)
        {
            var csv = Mulai(report.Period);

            Baris(csv, "Disiplin", "Wadah diputuskan", "Wadah ditolak", "Angka penolakan (%)");
            foreach (var row in report.Rows)
            {
                Baris(csv, row.DisciplineName, Angka(row.DecidedCount), Angka(row.RejectedCount), Desimal(row.RejectionRatePercent));
            }

            csv.Append("\r\n");
            Baris(csv, "Disiplin", "Kode alasan", "Alasan penolakan", "Jumlah");
            foreach (var reason in report.Reasons)
            {
                var disiplin = report.Rows.FirstOrDefault(x => x.Discipline == reason.Discipline)?.DisciplineName
                               ?? reason.Discipline
                               ?? string.Empty;

                Baris(csv, disiplin, reason.ReasonCode, reason.ReasonName, Angka(reason.Count));
            }

            return Selesai(csv, report.Rows.Count);
        }

        /// <summary>
        /// Waktu penyelesaian per disiplin × kesegeraan. <see cref="LabCsvFile.RowCount"/> = baris laporan.
        /// </summary>
        public LabCsvFile Write(LabTurnaroundTimeReportResponse report)
        {
            var csv = Mulai(report.Period);

            Baris(csv, "Disiplin", "Kesegeraan", "Jumlah dirilis", "Rata-rata (menit)", "Terlambat", "Cito tanpa batas", "Keterangan");
            foreach (var row in report.Rows)
            {
                Baris(csv,
                    row.DisciplineName,
                    row.Urgency == "Routine" ? "Rutin" : row.Urgency,
                    Angka(row.ReleasedCount),
                    Desimal(row.AverageMinutes),
                    Angka(row.OverdueCount),
                    Angka(row.WithoutLimitCount),
                    row.NotCountableReason);
            }

            return Selesai(csv, report.Rows.Count);
        }

        private static StringBuilder Mulai(LabReportPeriodResponse period)
        {
            var csv = new StringBuilder();
            Baris(csv, "Periode", $"{Tanggal(period.StartDate)} - {Tanggal(period.EndDate)}");
            return csv;
        }

        private static LabCsvFile Selesai(StringBuilder csv, int rowCount)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var isi = encoding.GetBytes(csv.ToString());
            var bom = encoding.GetPreamble();

            var berkas = new byte[bom.Length + isi.Length];
            Buffer.BlockCopy(bom, 0, berkas, 0, bom.Length);
            Buffer.BlockCopy(isi, 0, berkas, bom.Length, isi.Length);

            return new LabCsvFile(berkas, rowCount);
        }

        private static void Baris(StringBuilder csv, params string?[] nilai)
        {
            csv.Append(string.Join(Pemisah, nilai.Select(Escape)));
            csv.Append("\r\n");
        }

        private static string Escape(string? nilai)
        {
            if (string.IsNullOrEmpty(nilai))
            {
                return string.Empty;
            }

            return nilai.IndexOfAny(new[] { Pemisah, '"', '\r', '\n' }) >= 0
                ? $"\"{nilai.Replace("\"", "\"\"")}\""
                : nilai;
        }

        private static string? Angka(int? nilai) => nilai?.ToString(CultureInfo.InvariantCulture);

        private static string? Desimal(decimal? nilai) => nilai?.ToString("0.0", DesimalKoma);

        private static string Tanggal(DateOnly tanggal) => $"{tanggal.Day} {NamaBulan[tanggal.Month - 1]} {tanggal.Year}";
    }

    /// <summary>
    /// Berkas CSV yang siap dikirim. <see cref="RowCount"/> — baris tabel utama laporan — dicatat pada log
    /// unduhan (<c>LAB-PERM-v1</c> rev 12 14.5).
    /// </summary>
    public sealed record LabCsvFile(byte[] Content, int RowCount);
}
