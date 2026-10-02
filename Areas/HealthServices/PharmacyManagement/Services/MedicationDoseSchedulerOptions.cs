namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Pengaturan pembentukan dosis MAR terjadwal — <c>BE-RWI-114</c>, <c>INT-KEP-08</c>. Bagian konfigurasi
    /// <c>HealthServices:MedicationDoseScheduler</c>; seluruh nilai punya bawaan sehingga bagian itu boleh tidak ada.
    /// </summary>
    public class MedicationDoseSchedulerOptions
    {
        /// <summary>
        /// Bawaan menyala. Mematikannya tidak menghilangkan dosis: MAR yang dibuka tetap membentuk dosisnya sendiri.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Jeda antar putaran. Bawaan 900 detik — "tiap 15 menit" pada <c>INT-KEP-08</c>.</summary>
        public int PollIntervalSeconds { get; set; } = 900;

        /// <summary>
        /// Akun pelaku pembentukan terjadwal, tercatat sebagai pembuat baris dosis. Kosong → akun SuperAdmin.
        /// </summary>
        public Guid? SystemActorUserId { get; set; }
    }
}
