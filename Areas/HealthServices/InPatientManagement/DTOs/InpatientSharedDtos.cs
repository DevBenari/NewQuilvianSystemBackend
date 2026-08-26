namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Satu pilihan pada penyaring layar: nilai yang dikirim balik ke backend beserta label
    /// yang dibaca petugas.
    /// </summary>
    /// <remarks>
    /// Bentuk ini mengikuti pasangan <c>Value</c> dan <c>Label</c> yang sudah dipakai seluruh
    /// endpoint <c>filters/metadata</c> di repository ini, misalnya pada
    /// <c>BedController.GetFilterMetadata</c>. Dipakai bersama oleh seluruh layar Rawat Inap
    /// supaya frontend tidak menghadapi tiga bentuk pilihan yang berbeda.
    /// </remarks>
    public class InpatientOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu pilihan pengurutan pada layar daftar.
    /// </summary>
    public class InpatientSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu aturan Kelayakan Penempatan yang gagal.
    /// </summary>
    /// <remarks>
    /// <b>Kenapa berbentuk daftar, bukan satu kalimat.</b> Petugas perlu tahu apakah yang
    /// menghalangi adalah keadaan tempat tidurnya, jenis kelamin pasien, atau kebutuhan
    /// isolasinya — karena tindakan lanjutannya berbeda untuk masing-masing. Bentuk daftar
    /// dikunci api contract bagian Bed Occupancy dan `02-backend-architecture.md` bagian 1.7.
    /// </remarks>
    public class PlacementEligibilityFailureResponse
    {
        /// <summary>Nomor aturan pada daftar Kelayakan Penempatan, 1 sampai 9.</summary>
        public int RuleNumber { get; set; }

        /// <summary>Penanda aturan yang tetap sama walau kalimatnya diperbaiki.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Kalimat sebagaimana dibaca petugas di layar.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Kode status HTTP yang seharusnya dipakai bila aturan ini yang menolak.</summary>
        public int StatusCode { get; set; }
    }
}
