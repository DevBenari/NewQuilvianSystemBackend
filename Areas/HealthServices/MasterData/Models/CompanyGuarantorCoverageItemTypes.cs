namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Kosakata ItemType aturan tanggungan penjamin perusahaan (MstCompanyGuarantorCoverageRule).
    /// Satu-satunya tempat alias didefinisikan, dipakai bersama oleh master data (saat menyimpan
    /// aturan) dan mesin tanggungan (saat mencocokkan komponen tagihan), supaya keduanya tidak bisa
    /// berselisih lagi.
    ///
    /// "ServiceCategory" adalah nama LAMA untuk kategori tarif. Master data menyimpan aturan baru
    /// sebagai "TariffCategory", tetapi baris lama dan impor data masih dapat memuat
    /// "ServiceCategory". Keduanya merujuk TariffCategoryId yang sama, jadi mesin tanggungan harus
    /// memperlakukannya setara.
    /// </summary>
    public static class CompanyGuarantorCoverageItemTypes
    {
        public const string Tariff = "Tariff";
        public const string Drug = "Drug";
        public const string DrugCategory = "DrugCategory";
        public const string Procedure = "Procedure";
        public const string TariffCategory = "TariffCategory";
        public const string LegacyServiceCategory = "ServiceCategory";

        /// <summary>
        /// Memangkas spasi dan memetakan alias lama "ServiceCategory" ke "TariffCategory".
        /// Tipe lain dikembalikan apa adanya (sudah dipangkas); validasi nilai yang diizinkan
        /// tetap milik pemanggil.
        /// </summary>
        public static string Normalize(string? itemType)
        {
            var trimmed = itemType?.Trim() ?? string.Empty;
            return trimmed.Equals(LegacyServiceCategory, StringComparison.OrdinalIgnoreCase)
                ? TariffCategory
                : trimmed;
        }

        /// <summary>
        /// True untuk "TariffCategory" maupun alias lama "ServiceCategory" (tanpa membedakan huruf besar/kecil).
        /// </summary>
        public static bool IsTariffCategory(string? itemType) =>
            string.Equals(Normalize(itemType), TariffCategory, StringComparison.OrdinalIgnoreCase);
    }
}
