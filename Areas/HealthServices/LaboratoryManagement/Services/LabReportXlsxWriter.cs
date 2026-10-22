using System.Globalization;
using ClosedXML.Excel;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Penulis berkas <c>.xlsx</c> laporan Laboratorium <c>S16b</c> lewat <c>ClosedXML</c> yang sudah terpasang
    /// (<c>LAB-DEC-233</c>, <c>LAB-API-v1</c> <c>r49</c> 44.4, <c>BE-LAB-98</c>) — nol pustaka baru.
    ///
    /// <para>
    /// Berkas <b>dibentuk dari respons yang sama dengan layar</b>, bukan dari kueri kedua (<c>RPT-002</c>),
    /// sehingga isi unduhan tidak pernah berbeda dari yang tampil. Satu lembar: baris 1 judul, baris 2 periode,
    /// baris 3 keterangan, lalu tabel berjudul kolom Bahasa Indonesia. Angka disimpan sebagai angka.
    /// </para>
    /// </summary>
    public class LabReportXlsxWriter
    {
        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

        /// <summary>Statistik Pesanan — satu baris per unit, baris <i>Sub Total</i> per jenis kunjungan, baris <i>Total</i>.</summary>
        public LabXlsxFile Write(LabOrderStatisticsReportResponse report)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Statistik Pesanan");

            var kolom = report.Total.Cells;
            var jumlahKolom = 2 + kolom.Count * 2 + 2;

            ws.Cell(1, 1).Value = "Statistik Pesanan Laboratorium";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = "Periode: " + Periode(report.Period);
            ws.Cell(3, 1).Value = "Dihitung menurut waktu pesanan diminta (WIB). Pesanan dan pemeriksaan yang dibatalkan atau gugur tidak dihitung — sama dengan Beranda.";
            ws.Cell(3, 1).Style.Font.Italic = true;

            // Judul dua tingkat: disiplin, lalu Pesanan/Pemeriksaan.
            const int h1 = 5, h2 = 6;
            ws.Cell(h1, 1).Value = "Jenis Kunjungan";
            ws.Cell(h1, 2).Value = "Unit Layanan";
            ws.Range(h1, 1, h2, 1).Merge();
            ws.Range(h1, 2, h2, 2).Merge();
            var c = 3;
            foreach (var d in kolom)
            {
                ws.Cell(h1, c).Value = d.DisciplineName;
                ws.Range(h1, c, h1, c + 1).Merge();
                ws.Cell(h2, c).Value = "Pesanan";
                ws.Cell(h2, c + 1).Value = "Pemeriksaan";
                c += 2;
            }
            ws.Cell(h1, c).Value = "Jumlah";
            ws.Range(h1, c, h1, c + 1).Merge();
            ws.Cell(h2, c).Value = "Pesanan";
            ws.Cell(h2, c + 1).Value = "Pemeriksaan";
            var judul = ws.Range(h1, 1, h2, jumlahKolom);
            judul.Style.Font.Bold = true;
            judul.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F4F5");
            judul.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var r = h2 + 1;
            var barisData = 0;

            void TulisAngka(int row, List<LabOrderStatisticsCell> cells, int pesanan, int pemeriksaan)
            {
                var col = 3;
                foreach (var d in kolom)
                {
                    var cell = cells.FirstOrDefault(x => x.Discipline == d.Discipline);
                    ws.Cell(row, col).Value = cell?.OrderCount ?? 0;
                    ws.Cell(row, col + 1).Value = cell?.ExaminationCount ?? 0;
                    col += 2;
                }
                ws.Cell(row, col).Value = pesanan;
                ws.Cell(row, col + 1).Value = pemeriksaan;
            }

            foreach (var g in report.Groups)
            {
                foreach (var u in g.Units)
                {
                    ws.Cell(r, 1).Value = g.EncounterTypeName;
                    ws.Cell(r, 2).Value = u.ServiceUnitName;
                    TulisAngka(r, u.Cells, u.OrderCount, u.ExaminationCount);
                    r++;
                    barisData++;
                }

                ws.Cell(r, 1).Value = "Sub Total " + g.EncounterTypeName;
                ws.Range(r, 1, r, 2).Merge();
                TulisAngka(r, g.Subtotal.Cells, g.Subtotal.OrderCount, g.Subtotal.ExaminationCount);
                ws.Range(r, 1, r, jumlahKolom).Style.Font.Bold = true;
                r++;
            }

            ws.Cell(r, 1).Value = "Total";
            ws.Range(r, 1, r, 2).Merge();
            TulisAngka(r, report.Total.Cells, report.Total.OrderCount, report.Total.ExaminationCount);
            var total = ws.Range(r, 1, r, jumlahKolom);
            total.Style.Font.Bold = true;
            total.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

            ws.Range(h1, 1, r, jumlahKolom).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(h1, 1, r, jumlahKolom).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(h2 + 1, 3, r, jumlahKolom).Style.NumberFormat.Format = "#,##0";
            ws.Columns(1, jumlahKolom).AdjustToContents();

            return Simpan(workbook, barisData);
        }

        /// <summary>
        /// Laporan Pembatalan (<c>BE-LAB-100</c>) — dua belas kolom BR-151, satu baris per peristiwa, terbaru di atas, dan
        /// baris total per jenis. Waktu ditulis WIB; alasan kosong ditulis <i>Alasan tidak dicatat</i>. Berkas ini
        /// memuat identitas pasien — keluarnya dijaga izin unduh dan dicatat (tanpa pasien) oleh pemanggil.
        /// </summary>
        public LabXlsxFile Write(LabCancellationReportExport report)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Laporan Pembatalan");

            string[] judul =
            {
                "Waktu Batal (WIB)", "Jenis", "No. Order", "No. RM", "Nama Pasien", "Disiplin", "Pemeriksaan",
                "Status Sebelum", "Sudah Layak Tagih", "Alasan", "Dibatalkan Oleh", "Unit Layanan"
            };

            ws.Cell(1, 1).Value = "Laporan Pembatalan Order & Pemeriksaan";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = "Periode: " + Periode(report.Period);
            ws.Cell(3, 1).Value = "Satu baris per peristiwa pembatalan, menurut waktu pembatalan (WIB). Pembatalan wadah dan pemeriksaan gugur tidak termasuk.";
            ws.Cell(3, 1).Style.Font.Italic = true;

            const int h = 5;
            for (var i = 0; i < judul.Length; i++)
                ws.Cell(h, i + 1).Value = judul[i];
            var kepala = ws.Range(h, 1, h, judul.Length);
            kepala.Style.Font.Bold = true;
            kepala.Style.Fill.BackgroundColor = XLColor.FromHtml("#E6F4F5");

            var r = h + 1;
            foreach (var x in report.Items)
            {
                var isOrder = x.Kind == "Order";
                ws.Cell(r, 1).Value = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(x.CancelledAt, DateTimeKind.Utc), Wib);
                ws.Cell(r, 1).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                ws.Cell(r, 2).Value = isOrder ? "Pesanan" : "Pemeriksaan";
                ws.Cell(r, 3).Value = x.OrderNumber;
                ws.Cell(r, 4).Value = x.MedicalRecordNumber ?? string.Empty;
                ws.Cell(r, 5).Value = x.PatientName ?? string.Empty;
                ws.Cell(r, 6).Value = x.DisciplineName;
                ws.Cell(r, 7).Value = isOrder ? $"{x.ExaminationCount ?? 0} pemeriksaan" : x.ExaminationName ?? string.Empty;
                ws.Cell(r, 8).Value = LabelStatusSebelum(isOrder, x.FromStatus);
                ws.Cell(r, 9).Value = x.WasChargeEligible switch { true => "Ya", false => "Tidak", null => string.Empty };
                ws.Cell(r, 10).Value = x.IsReasonRecorded ? x.Reason ?? string.Empty : "Alasan tidak dicatat";
                if (!x.IsReasonRecorded)
                    ws.Cell(r, 10).Style.Font.Italic = true;
                ws.Cell(r, 11).Value = x.CancelledByName ?? string.Empty;
                ws.Cell(r, 12).Value = x.ServiceUnitName ?? string.Empty;
                r++;
            }

            r++;
            ws.Cell(r, 1).Value = $"Total: Pesanan {report.Totals.OrderCount} · Pemeriksaan {report.Totals.ExaminationCount} · Seluruhnya {report.Totals.TotalCount}";
            ws.Cell(r, 1).Style.Font.Bold = true;

            if (report.Items.Count > 0)
            {
                var tabel = ws.Range(h, 1, h + report.Items.Count, judul.Length);
                tabel.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tabel.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
            ws.Columns(1, judul.Length).AdjustToContents();

            return Simpan(workbook, report.Items.Count);
        }

        private static readonly TimeZoneInfo Wib = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");

        private static string LabelStatusSebelum(bool isOrder, string? fromStatus) =>
            fromStatus is null ? string.Empty
            : isOrder && Enum.TryParse<LabOrderStatus>(fromStatus, out var o) ? LabFilterMetadataFactory.LabelOrderStatus(o)
            : !isOrder && Enum.TryParse<LabExaminationStatus>(fromStatus, out var e) ? LabFilterMetadataFactory.LabelStatusPemeriksaan(e)
            : fromStatus;

        /// <summary><i>1 Oktober 2026 – 31 Oktober 2026</i>.</summary>
        internal static string Periode(LabReportPeriodResponse period) =>
            $"{period.StartDate.ToString("d MMMM yyyy", Indonesia)} – {period.EndDate.ToString("d MMMM yyyy", Indonesia)}";

        private static LabXlsxFile Simpan(XLWorkbook workbook, int rowCount)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return new LabXlsxFile(stream.ToArray(), rowCount);
        }
    }

    /// <summary>Isi berkas <c>.xlsx</c> beserta jumlah baris datanya — pola <see cref="LabCsvFile"/>.</summary>
    public sealed record LabXlsxFile(byte[] Content, int RowCount);
}
