using System.Globalization;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    public static class AccLedgerCsv
    {
        public const string ContentType = "text/csv; charset=utf-8";

        private const string PemisahKolom = ";";
        private const string PemisahDesimal = ",";
        private const string AkhirBaris = "\r\n";

        private static readonly TimeSpan ZonaWaktuWib = TimeSpan.FromHours(7);
        private static readonly char[] AwalanRumus = { '=', '+', '-', '@', '\t', '\r' };

        public static byte[] Tulis(
            IEnumerable<IReadOnlyList<string>> kepala,
            IReadOnlyList<string> kolom,
            IEnumerable<IReadOnlyList<string>> baris)
        {
            var isi = new StringBuilder();

            foreach (var b in kepala)
                TulisBaris(isi, b);

            isi.Append(AkhirBaris);
            TulisBaris(isi, kolom);

            foreach (var b in baris)
                TulisBaris(isi, b);

            var pembuka = Encoding.UTF8.GetPreamble();
            var badan = Encoding.UTF8.GetBytes(isi.ToString());
            var hasil = new byte[pembuka.Length + badan.Length];

            Buffer.BlockCopy(pembuka, 0, hasil, 0, pembuka.Length);
            Buffer.BlockCopy(badan, 0, hasil, pembuka.Length, badan.Length);

            return hasil;
        }

        public static string Angka(decimal nilai)
            => nilai.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", PemisahDesimal);

        public static string Tanggal(DateTime nilai)
            => nilai.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public static string Teks(string? nilai)
        {
            var bersih = nilai ?? string.Empty;

            if (bersih.Length > 0 && Array.IndexOf(AwalanRumus, bersih[0]) >= 0)
                bersih = "'" + bersih;

            return "\"" + bersih.Replace("\"", "\"\"") + "\"";
        }

        public static string WaktuUnduh()
            => DateTimeOffset.UtcNow.ToOffset(ZonaWaktuWib)
                .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " WIB";

        public static IReadOnlyList<string> BarisKepala(string label, string? nilai)
            => new[] { Teks(label), Teks(nilai) };

        private static void TulisBaris(StringBuilder isi, IReadOnlyList<string> sel)
        {
            isi.Append(string.Join(PemisahKolom, sel));
            isi.Append(AkhirBaris);
        }
    }
}
