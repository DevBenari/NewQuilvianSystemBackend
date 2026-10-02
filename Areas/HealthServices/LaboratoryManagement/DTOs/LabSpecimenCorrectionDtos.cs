namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Mengoreksi dan melengkapi Informasi Specimen dari halaman hasil
    /// (<c>LAB-API-v1</c> <c>r27</c> bagian 21.4, <c>LAB-DEC-107</c>).
    ///
    /// <b>Seluruh ruas OPSIONAL; yang tidak dikirim tidak diubah.</b> Itu sebabnya ia
    /// <c>PATCH</c>, bukan <c>PUT</c>: halaman hasil mengoreksi <b>sebagian</b> — biasanya
    /// satu ruas yang keliru. <c>PUT</c> menuntut pemanggil mengirim seluruh isi specimen, dan
    /// ruas yang lupa disertakan akan terhapus diam-diam.
    /// </summary>
    public class LabSpecimenCorrectionRequest
    {
        /// <summary>Jenis specimen — salah satu dari 31 kelompok.</summary>
        public Guid? SpecimenTypeId { get; set; }

        /// <summary><b>Wajib</b> bila jenis yang dipilih bertanda <c>Lainnya</c> (<c>VAL-104</c>).</summary>
        public string? SpecimenTypeOtherNote { get; set; }

        /// <summary>
        /// Spesifik Specimen. <b>Menggantikan SELURUH</b> pilihan yang ada — mengirim daftar
        /// kosong berarti mengosongkannya.
        /// </summary>
        public List<Guid>? DetailTypeIds { get; set; }

        public decimal? VolumeAmount { get; set; }

        /// <summary>Menunjuk <c>MstMeasurement</c> bertanda <c>IsForLaboratory</c> (<c>VAL-110</c>).</summary>
        public Guid? VolumeUnitId { get; set; }

        /// <summary>Keterangan operasional bebas, maksimal 500.</summary>
        public string? SpecimenDescription { get; set; }

        /// <summary>Waktu penerimaan fisik. Aturan BR-37 tetap berlaku.</summary>
        public DateTime? PhysicallyReceivedAt { get; set; }
    }

    /// <summary>Satu baris jejak perubahan ruas.</summary>
    public class LabFieldChangeResponse
    {
        public string FieldName { get; set; } = string.Empty;

        /// <summary>Nama ruas dalam Bahasa Indonesia, untuk layar.</summary>
        public string FieldLabel { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public Guid? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
