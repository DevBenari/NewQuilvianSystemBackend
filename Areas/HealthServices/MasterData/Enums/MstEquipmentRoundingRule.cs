namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Aturan pembulatan unit tagih berbasis waktu — <c>keperawatan</c> kontrak <c>0.6.0</c> bagian 12.8.
    /// Dipakai jenis alat (<c>MstMedicalEquipment.RoundingRule</c>) dan tarif komponen operasi per jam
    /// (<c>MstTariff.ChargeRounding</c>, <c>episode-rawat-inap</c> kontrak <c>0.10.0</c>).
    /// </summary>
    /// <remarks>
    /// Contoh: sewa kamar operasi per jam dipakai 2 jam 10 menit. <see cref="CeilingWholeUnit"/>
    /// menagih 3 jam; <see cref="Proportional"/> menagih 2,17 jam.
    /// </remarks>
    public enum MstEquipmentRoundingRule
    {
        /// <summary>Dibulatkan ke atas ke unit utuh. Bawaan.</summary>
        CeilingWholeUnit = 1,

        /// <summary>Proporsional sesuai lama pemakaian.</summary>
        Proportional = 2
    }
}
