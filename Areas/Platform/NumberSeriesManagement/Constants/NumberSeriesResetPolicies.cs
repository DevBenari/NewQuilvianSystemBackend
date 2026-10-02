namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants
{
    /// <summary>
    /// Empat kebijakan pengulangan deret nomor yang sah.
    /// </summary>
    /// <remarks>
    /// <b>Deret baru memakai <see cref="Never"/>.</b> <c>DEC-PLT-004</c> menetapkan deret berjalan
    /// terus dan tidak pernah diulang — tidak per tahun, tidak per fasilitas. Ketiga nilai lain ada
    /// semata untuk menampung deret lama ketika <c>PLT-SLICE-02</c> memindahkannya, dan bukan
    /// pilihan yang ditawarkan kepada modul baru.
    ///
    /// <b>Nilainya sengaja sama persis dengan <c>BillingNumberResetPolicies</c>.</b> Menjaganya
    /// identik membuat pemindahan empat deret Billing kelak menjadi penyalinan baris, bukan
    /// penerjemahan nilai — dan penerjemahan nilai pada kolom yang menentukan periode pencacah
    /// adalah cara termudah menerbitkan nomor kembar.
    /// </remarks>
    public static class NumberSeriesResetPolicies
    {
        /// <summary>Pencacah tidak pernah diulang. Satu-satunya nilai untuk deret baru.</summary>
        public const string Never = "NEVER";

        /// <summary>Pencacah diulang tiap tahun. Hanya untuk menampung deret lama.</summary>
        public const string Yearly = "YEARLY";

        /// <summary>Pencacah diulang tiap bulan. Hanya untuk menampung deret lama.</summary>
        public const string Monthly = "MONTHLY";

        /// <summary>Pencacah diulang tiap hari. Hanya untuk menampung deret lama.</summary>
        public const string Daily = "DAILY";

        /// <summary>Keempat nilai yang sah, dipakai validasi dan check constraint database.</summary>
        public static readonly IReadOnlySet<string> All =
            new HashSet<string>([Never, Yearly, Monthly, Daily], StringComparer.OrdinalIgnoreCase);
    }
}
