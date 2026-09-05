using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Satu pemeriksaan terpesan sebagaimana ditampilkan kepada pemanggil.
    ///
    /// Inilah satuan yang ditagihkan (<c>LAB-DEC-024</c>). Salinan harga di bawah adalah bukti
    /// nilai saat kejadian, <b>bukan</b> tagihan — keputusan menagih tetap milik Billing.
    /// </summary>
    public class LabExaminationResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrderId { get; set; }

        /// <summary>Wadah fisik yang menopang pemeriksaan ini.</summary>
        public Guid SpecimenId { get; set; }

        /// <summary>Barcode wadah penopang, supaya layar tidak perlu memanggil ulang.</summary>
        public string? SpecimenBarcode { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public Guid? TariffId { get; set; }

        public string? TariffCode { get; set; }

        /// <summary>Salinan harga satuan saat kejadian. Bukan tagihan.</summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Keadaan pemeriksaan: <c>Ordered</c>, <c>ChargeEligible</c>, <c>Voided</c>, atau
        /// <c>Cancelled</c>.
        /// </summary>
        public string ExaminationStatus { get; set; } = string.Empty;

        /// <summary>Waktu pemeriksaan menjadi sah ditagihkan. Kosong selama belum layak.</summary>
        public DateTime? ChargeEligibleAt { get; set; }

        /// <summary>Tingkat kesegeraan: <c>Routine</c> atau <c>Cito</c> (<c>LAB-DEC-026</c>).</summary>
        public string Urgency { get; set; } = string.Empty;

        public DateTime? UrgencyMarkedAt { get; set; }

        public Guid? UrgencyMarkedByUserId { get; set; }

        /// <summary>
        /// Nama tampilan dokter yang menandai kesegeraannya. Kosong bila belum pernah ditandai,
        /// atau bila akunnya sudah tidak ada — nama yang hilang tidak boleh membuat baris
        /// pemeriksaannya ikut hilang.
        /// </summary>
        public string? UrgencyMarkedByUserName { get; set; }

        /// <summary>Pemeriksaan dikerjakan ganda (<c>LAB-DEC-026</c>).</summary>
        public bool IsDuplo { get; set; }

        public int Version { get; set; }
    }

    /// <summary>
    /// Menambah satu pemeriksaan terpesan pada sebuah pesanan, dan menautkannya ke wadah yang
    /// akan menopangnya.
    ///
    /// Perhatikan yang <b>tidak</b> ada di sini: kesegeraan, penanda duplo, dan harga.
    /// Kesegeraan dan duplo disetel lewat endpointnya sendiri (<c>BE-LAB-10</c>), sedangkan
    /// harga disalin backend dari tarif yang berlaku — bukan dikirim pemanggil. Menerima harga
    /// dari pemanggil berarti membiarkan layar menentukan angka yang dibaca Billing.
    /// </summary>
    public class AddLabExaminationRequest
    {
        /// <summary>Wadah yang akan menopang pemeriksaan ini.</summary>
        [Required]
        public Guid SpecimenId { get; set; }

        /// <summary>
        /// Jenis pemeriksaan yang dipesan. Wajib berpenanda <c>IsLaboratory</c>, aktif, dan
        /// belum dihapus (<c>VAL-17</c>).
        /// </summary>
        [Required]
        public Guid ProcedureId { get; set; }
    }

    /// <summary>
    /// Menandai satu pemeriksaan sebagai cito, atau mengembalikannya menjadi biasa
    /// (<c>LAB-DEC-026</c>).
    ///
    /// Ruasnya sengaja <c>bool</c> dan bukan nama enum: kesegeraan hanya punya dua keadaan, dan
    /// endpoint yang sama dipakai untuk kedua arah. Waktu dan pelaku penandaan tidak diterima
    /// dari pemanggil — keduanya diisi backend, karena itulah yang membuat jejaknya dapat
    /// dipercaya.
    /// </summary>
    public class SetLabExaminationUrgencyRequest
    {
        /// <summary>Benar berarti cito, salah berarti kembali biasa.</summary>
        public bool IsCito { get; set; }
    }

    /// <summary>
    /// Menandai satu pemeriksaan dikerjakan ganda, atau membatalkan penandaannya
    /// (<c>LAB-DEC-026</c>).
    ///
    /// Penanda ini <b>tidak</b> mengubah salinan tarif pada baris pemeriksaan. Apakah duplo
    /// berdampak pada tarif masih <c>LAB-OPEN-013</c> dan belum diputuskan siapa pun.
    /// </summary>
    public class SetLabExaminationDuploRequest
    {
        public bool IsDuplo { get; set; }
    }

    /// <summary>Membatalkan satu pemeriksaan terpesan tanpa menyentuh pemeriksaan lain.</summary>
    public class CancelLabExaminationRequest
    {
        /// <summary>
        /// Alasan pembatalan. Disimpan pada catatan log, bukan pada baris pemeriksaan —
        /// kolom alasan pada tabel pemeriksaan belum ada pada rilis ini.
        /// </summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}
