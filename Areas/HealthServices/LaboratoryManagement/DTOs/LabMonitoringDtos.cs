using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar pantau. <b>Satu bentuk untuk ketiga disiplin</b>
    /// (<c>LAB-DEC-025</c>).
    ///
    /// Disiplin sengaja <b>tidak</b> ada di sini: ia ditentukan jalur yang dipanggil, bukan
    /// ruas yang dikirim. Itulah inti keputusannya — tiga daftar sejajar sebagai tiga menu,
    /// bukan satu daftar berpenyaring yang memaksa petugas memilih disiplinnya setiap kali
    /// membuka layar.
    /// </summary>
    public class LabMonitoringQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        /// <summary>Menyaring per pasien.</summary>
        public Guid? PatientId { get; set; }

        /// <summary>Nomor rekam medis, cocok sebagian.</summary>
        public string? MedicalRecordNumber { get; set; }

        /// <summary>Nomor kunjungan, cocok sebagian.</summary>
        public string? EncounterNumber { get; set; }

        /// <summary>Awal periode, dihitung dari waktu pesanan dibuat.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Akhir periode.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>Jenis kunjungan: rawat jalan, rawat inap, gawat darurat.</summary>
        public EncounterType? EncounterType { get; set; }

        /// <summary>Kunjungan baru atau lama.</summary>
        public VisitType? VisitType { get; set; }

        /// <summary>Unit layanan asal pesanan.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Ruangan asal pesanan.</summary>
        public Guid? RoomId { get; set; }

        /// <summary>Jenis penjamin kunjungan: tunai, asuransi, perusahaan, dan seterusnya.</summary>
        public EncounterPaymentType? PaymentType { get; set; }

        /// <summary>Status operasional pesanan.</summary>
        public LabOrderStatus? OrderStatus { get; set; }

        /// <summary>
        /// Status wadah. Sebuah pesanan ikut tersaring bila <b>salah satu</b> wadahnya berada
        /// pada status ini — satu pesanan dapat memiliki beberapa wadah dengan status berbeda.
        /// </summary>
        public LabSpecimenStatus? SpecimenStatus { get; set; }

        /// <summary>
        /// Menyaring pesanan yang memuat sekurang-kurangnya satu pemeriksaan cito
        /// (<c>LAB-DEC-026</c>).
        /// </summary>
        public bool? OnlyCito { get; set; }

        /// <summary>Pencarian bebas pada nama pasien, nomor rekam medis, dan nomor kunjungan.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu baris daftar pantau — satu <b>pesanan</b>, bukan satu pemeriksaan.
    ///
    /// Satuannya pesanan karena yang dipantau kepala instalasi adalah pekerjaan yang masuk ke
    /// disiplinnya. Rincian per pemeriksaan ada pada daftar kerja (<c>BE-LAB-14</c>).
    /// </summary>
    public class LabMonitoringItemResponse
    {
        public Guid LabOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public string? EncounterNumber { get; set; }

        public Guid? PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        /// <summary>Selalu terisi disiplin jalur yang dipanggil.</summary>
        public string Discipline { get; set; } = string.Empty;

        public string OrderStatus { get; set; } = string.Empty;

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public DateTime? RequestedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? EncounterType { get; set; }

        public string? VisitType { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public string? PaymentType { get; set; }

        public int SpecimenCount { get; set; }

        public int AcceptedSpecimenCount { get; set; }

        public int ExaminationCount { get; set; }

        /// <summary>Benar bila sekurang-kurangnya satu pemeriksaan pada pesanan ini bertanda cito.</summary>
        public bool HasCito { get; set; }

        public DateTime CreateDateTime { get; set; }
    }
}
