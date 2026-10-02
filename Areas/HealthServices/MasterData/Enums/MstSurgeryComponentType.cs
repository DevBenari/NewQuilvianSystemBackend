namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Komponen biaya operasi yang ditagihkan Kamar Operasi lewat master tarif —
    /// <c>episode-rawat-inap</c> kontrak <c>0.10.0</c> bagian 12.8 (<c>BE-RWI-172</c>, migration <c>E4</c>).
    /// </summary>
    /// <remarks>
    /// Tindakan operasinya sendiri <b>tidak</b> memakai nilai ini: ia ditagih lewat order tindakan
    /// yang dirujuk kasus (<c>RWI-DEC-196</c>). Bahan dan implan ditagih per obat (<c>DrugId</c>).
    /// </remarks>
    public enum MstSurgeryComponentType
    {
        /// <summary>Bukan tarif komponen operasi. Bawaan seluruh tarif lama.</summary>
        None = 0,

        /// <summary>Jasa anestesi.</summary>
        AnesthesiaService = 1,

        /// <summary>Sewa kamar operasi.</summary>
        OperatingRoomRent = 2
    }
}
