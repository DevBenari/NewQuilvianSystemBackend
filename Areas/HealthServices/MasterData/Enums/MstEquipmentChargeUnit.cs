namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Satuan tagih jenis alat medis — <c>keperawatan</c> kontrak <c>0.6.0</c> bagian 12.8
    /// (<c>BE-RWI-172</c>, migration <c>K8</c>). Wajib dipilih; tidak ada nilai bawaan.
    /// </summary>
    public enum MstEquipmentChargeUnit
    {
        /// <summary>Per pemakaian.</summary>
        PerUse = 1,

        /// <summary>Per jam.</summary>
        PerHour = 2,

        /// <summary>Per hari.</summary>
        PerDay = 3
    }
}
