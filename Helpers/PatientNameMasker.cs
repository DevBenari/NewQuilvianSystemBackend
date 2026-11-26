namespace QuilvianSystemBackend.Helpers
{
    /// <summary>
    /// Penyamar nama pasien untuk tampilan publik. Huruf pertama setiap kata dipertahankan, sisanya
    /// diganti bintang (paling banyak lima). Contoh: <c>"Siti Rahmawati"</c> menjadi
    /// <c>"S*** R*****"</c>, dan <c>"Ani Yu"</c> menjadi <c>"A** Y*"</c>.
    /// </summary>
    /// <remarks>
    /// Dipindah apa adanya dari <c>QueueDisplayRuntimeController.MaskPatientName</c> supaya display
    /// antrean dan halaman publik Pendaftaran Pasien Online memakai aturan yang sama
    /// (kiosk Amendment PO, <c>BE-KSK-PO-004</c>, <c>KSK-PO-FE-002</c>).
    /// </remarks>
    public static class PatientNameMasker
    {
        public static string Mask(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(MaskWord));
        }

        private static string MaskWord(string word)
        {
            if (word.Length <= 1) return word;
            if (word.Length == 2) return word[0] + "*";
            return word[0] + new string('*', Math.Min(word.Length - 1, 5));
        }
    }
}
