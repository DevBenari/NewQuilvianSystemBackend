namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums
{
    /// <summary>
    /// Gerbang pemberian yang dilewati satu otorisasi darurat (INV-BD-030, DEC-BD-038).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enum tiga nilai dipilih supaya keadaan "darurat yang tidak melewati gerbang apa pun"
    /// <b>tidak dapat ditulis sama sekali</b>. Dua kolom bool akan memungkinkan
    /// <c>(false, false)</c> yang tak bermakna, dan penanda darurat tanpa keterangan berhenti
    /// bermakna bagi pembaca rekam berikutnya.
    /// </para>
    /// </remarks>
    public enum BbkEmergencyBypassScope
    {
        /// <summary>Hanya gerbang bukti kecocokan yang dilewati; lokasi penyimpanan sedang aktif.</summary>
        CompatibilityEvidence = 0,

        /// <summary>Hanya gerbang lokasi penyimpanan aktif yang dilewati; bukti kecocokan berlaku.</summary>
        InactiveStorageLocation = 1,

        /// <summary>Kedua gerbang dilewati sekaligus.</summary>
        Both = 2
    }
}
