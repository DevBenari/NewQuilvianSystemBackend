namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Dasar perhitungan tarif — <c>episode-rawat-inap</c> kontrak <c>0.10.0</c> bagian 12.8
    /// (<c>BE-RWI-172</c>, migration <c>E4</c>).
    /// </summary>
    /// <remarks>
    /// Contoh: tarif sewa kamar operasi Rp1.200.000 <see cref="PerHour"/> dengan operasi 90 menit dan
    /// pembulatan ke atas ditagih 2 x Rp1.200.000 = Rp2.400.000. Tarif yang sama
    /// <see cref="PerService"/> ditagih Rp1.200.000 sekali, berapa pun lamanya.
    /// </remarks>
    public enum MstTariffChargeBasis
    {
        /// <summary>Sekali per layanan. Bawaan seluruh tarif lama.</summary>
        PerService = 0,

        /// <summary>Per jam pemakaian.</summary>
        PerHour = 1
    }
}
