using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Permintaan merencanakan satu sampel sekaligus satu komponen pemeriksaan.
    ///
    /// Barcode sengaja tidak ada di sini. Barcode dibuat server dan tidak pernah diterima dari
    /// client, sesuai keputusan author <c>RJ-BIL-OQ-010</c>.
    /// </summary>
    public class PlanLabSpecimenRequest
    {
        /// <summary>
        /// Jenis pemeriksaan yang akan dikerjakan dari wadah ini.
        ///
        /// <b>Inilah perubahan inti <c>LAB-DEC-024</c>.</b> Satu wadah menopang satu atau lebih
        /// pemeriksaan — satu tabung darah ungu untuk hemoglobin, leukosit, dan trombosit
        /// sekaligus. Sebelumnya satu wadah sama dengan satu pemeriksaan, sehingga pasien
        /// menerima tiga barcode untuk satu kali tusukan jarum.
        ///
        /// Wajib memuat sekurang-kurangnya satu (<c>VAL-05</c>), dan tidak boleh memuat jenis
        /// yang sama dua kali (<c>VAL-07</c>).
        /// </summary>
        public List<Guid> Examinations { get; set; } = new();

        /// <summary>
        /// Jalur ringkas satu pemeriksaan, dipertahankan untuk pemanggil lama.
        ///
        /// Dipakai hanya ketika <see cref="Examinations"/> kosong. Bila keduanya kosong,
        /// procedure pesanan yang dipakai — dan bila pesanan pun tidak punya, permintaan ditolak
        /// <c>VAL-05</c>.
        /// </summary>
        public Guid? ProcedureId { get; set; }

        [MaxLength(200)]
        public string? SpecimenDescription { get; set; }

        /// <summary>
        /// Jenis bahan yang dibawa wadah ini, dipilih dari <c>GET /lab-specimen-types/options</c>
        /// (<c>LAB-DEC-040</c>, <c>BR-35</c>).
        ///
        /// Wajib diisi untuk wadah baru (<c>VAL-51</c>). Jenis yang sudah dinonaktifkan ditolak
        /// (<c>VAL-55</c>), dan jenis yang tidak ada pada daftar ditolak (<c>VAL-52</c>) —
        /// termasuk upaya menamai jenisnya sebagai teks lewat
        /// <see cref="SpecimenTypeOtherNote"/> tanpa memilih apa pun.
        /// </summary>
        public Guid? SpecimenTypeId { get; set; }

        /// <summary>
        /// Keterangan jenis, hanya untuk jenis <c>Lainnya</c>.
        ///
        /// Wajib bila jenis terpilih adalah baris <c>Lainnya</c> (<c>VAL-53</c>), dan ditolak
        /// bila jenis terpilih bukan <c>Lainnya</c> (<c>VAL-54</c>).
        /// </summary>
        [MaxLength(128)]
        public string? SpecimenTypeOtherNote { get; set; }

        /// <summary>
        /// Banyaknya bahan di dalam wadah (<c>LAB-DEC-041</c>, <c>BR-36</c>).
        ///
        /// <b>Berapa pun nilainya diterima.</b> <c>RULE-021</c> menyatakan tidak ada batas
        /// minimum maupun maksimum, sehingga tidak ada satu pun pemeriksaan di sini yang
        /// membandingkannya terhadap ambang apa pun.
        /// </summary>
        public decimal? VolumeAmount { get; set; }

        /// <summary>
        /// Satuan volume, dipilih dari <c>MstMeasurement</c> ber-<c>IsForLaboratory</c>.
        ///
        /// Wajib bila <see cref="VolumeAmount"/> diisi (<c>VAL-56</c>); satuan di luar daftar
        /// satuan laboratorium ditolak (<c>VAL-57</c>).
        /// </summary>
        public Guid? VolumeUnitId { get; set; }

        /// <summary>
        /// Kapan wadahnya benar-benar sampai di meja penerimaan (<c>LAB-DEC-042</c>,
        /// <c>BR-37</c>).
        ///
        /// Tidak boleh berada di masa depan (<c>VAL-58</c>). Boleh dikosongkan — wadah yang
        /// direncanakan sebelum bahannya datang memang belum punya waktu kedatangan.
        ///
        /// <b>Ini bukan <c>ReceivedAt</c>.</b> <c>ReceivedAt</c> diisi server dan tidak dapat
        /// diubah dari luar (<c>AC-65</c>); yang diisi petugas adalah ruas ini.
        /// </summary>
        public DateTime? PhysicallyReceivedAt { get; set; }

        /// <summary>
        /// Waktu pengambilan sampel <b>yang dinyatakan petugas</b> (<c>LAB-CONFLICT-006</c>
        /// pilihan A, <c>LAB-API-v1</c> <c>r16</c>). Boleh kosong.
        ///
        /// <b>Alasannya bukan aturan, melainkan data.</b> Waktu pengambilan di klinik perujuk
        /// memang diketahui petugas, dan sebelum ruas ini ada ia <b>hilang</b> — tidak
        /// tersimpan di mana pun. <c>CollectedAt</c> yang tersimpan diisi server pada tindakan
        /// pengambilan, dan pada jalur rujukan luar cap waktu itu justru <b>lebih akhir</b>
        /// daripada waktu kedatangan sampel.
        ///
        /// Ruas inilah pembanding <c>VAL-59</c>. Ketika ia kosong, aturan itu <b>tidak
        /// menyala</b> — dan itu disengaja: membandingkan terhadap cap waktu server akan
        /// menolak skenario contoh <c>BR-37</c> sendiri.
        ///
        /// <b>Nilainya dipakai sebagai pembanding, dan belum disimpan.</b> Menyimpannya ke
        /// <c>LabSpecimen.CollectedAt</c> akan <b>ditimpa</b> tindakan pengambilan, yang
        /// menyetel kolom itu menjadi waktu server tanpa syarat — sehingga justru menghapus
        /// data yang hendak diselamatkan. Menyimpannya dengan benar menuntut kolom tersendiri
        /// beserta migration-nya, dan itu keputusan terpisah yang belum diambil siapa pun.
        /// Dilaporkan pada <c>BE-LAB-37</c>, bukan diputuskan diam-diam di sini.
        /// </summary>
        public DateTime? CollectedAt { get; set; }
    }

    public class CollectLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class ReceiveLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class AcceptLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Permintaan menolak sampel. <see cref="ReasonCode"/> wajib dan harus berasal dari katalog
    /// alasan; free-text saja tidak diterima.
    /// </summary>
    public class RejectLabSpecimenRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Permintaan pengambilan ulang. Menghasilkan sampel baru dengan identitas dan barcode
    /// baru yang tetap menunjuk sampel yang ditolak sebagai asal-usulnya.
    /// </summary>
    public class RequestLabRecollectionRequest
    {
        [Required]
        public LabRecollectionCause? Cause { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class HoldLabRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ResumeLabRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class CancelLabSpecimenRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Penyaring daftar penerimaan lintas pesanan (<c>LAB-API-v1</c> <c>r17</c>,
    /// <c>FR-11.5</c>). Bentuknya mengikuti <c>LabOrderPagedQuery</c> yang sudah berjalan
    /// sejak <c>r5</c>, supaya hanya ada satu bentuk penyaring yang perlu dirawat.
    /// </summary>
    public class LabSpecimenPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Awal rentang <b>waktu kedatangan sebenarnya</b> — bukan waktu pencatatan.
        /// Lihat catatan pada <c>GetListAsync</c>.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Akhir rentang waktu kedatangan sebenarnya.</summary>
        public DateTime? EndDate { get; set; }

        public LabSpecimenStatus? SpecimenStatus { get; set; }

        /// <summary>Pencarian bebas pada barcode wadah, nomor order, nama pasien, dan No. RM.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu baris pada daftar penerimaan lintas pesanan (<c>LAB-API-v1</c> <c>r17</c>).
    ///
    /// Ia mewarisi seluruh ruas wadah yang sudah ada, lalu menambahkan yang dibutuhkan daftar
    /// lintas pesanan: penautan ke pesanannya, identitas pasien, dan <b>waktu pencatatan</b>.
    /// </summary>
    public class LabSpecimenListResponse : LabSpecimenResponse
    {
        /// <summary>Nomor pesanan yang dapat disebut petugas (<c>r16</c>).</summary>
        public string OrderNumber { get; set; } = string.Empty;

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        /// <summary>
        /// Waktu baris ini dicatat sistem.
        ///
        /// Dikembalikan <b>justru supaya selisihnya</b> terhadap
        /// <see cref="LabSpecimenResponse.PhysicallyReceivedAt"/> dapat ditampilkan
        /// (<c>LAB-DEC-042</c> butir 5). Selisih sebelas jam pada sampel yang datang pukul
        /// 21.00 adalah jam operasional; selisih yang sama pada sampel pukul 10.00 adalah
        /// pertanyaan yang pantas diajukan.
        /// </summary>
        public DateTime CreateDateTime { get; set; }
    }

    public class LabSpecimenResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrderId { get; set; }

        public string SpecimenBarcode { get; set; } = string.Empty;

        public int SpecimenSequence { get; set; }

        public string? SpecimenDescription { get; set; }

        /// <summary>Jenis bahan wadah ini, kosong pada wadah yang dibuat sebelum `LAB-DEC-040`.</summary>
        public Guid? SpecimenTypeId { get; set; }

        /// <summary>
        /// Nama jenis seperti yang dilihat petugas, misalnya "Blood" atau "Jaringan". Disertakan
        /// supaya layar tidak perlu memanggil data induk hanya untuk menampilkan satu nama.
        /// </summary>
        public string? SpecimenTypeName { get; set; }

        /// <summary>Keterangan jenis, terisi hanya pada wadah berjenis <c>Lainnya</c>.</summary>
        public string? SpecimenTypeOtherNote { get; set; }

        public decimal? VolumeAmount { get; set; }

        public Guid? VolumeUnitId { get; set; }

        /// <summary>
        /// Simbol satuan volume, misalnya <c>mL</c> atau <c>slide</c>. Ditampilkan berdampingan
        /// dengan <see cref="VolumeAmount"/> supaya angkanya tidak pernah berdiri tanpa satuan.
        /// </summary>
        public string? VolumeUnitSymbol { get; set; }

        public string SpecimenStatus { get; set; } = string.Empty;

        public DateTime? CollectedAt { get; set; }

        /// <summary>Kapan datanya masuk ke sistem. Diisi server, tidak pernah dari permintaan.</summary>
        public DateTime? ReceivedAt { get; set; }

        /// <summary>
        /// Kapan wadahnya benar-benar sampai di meja penerimaan, diisi petugas.
        ///
        /// Selisihnya terhadap <see cref="ReceivedAt"/> adalah lama keterlambatan pencatatan,
        /// dan kepala instalasi membacanya dari kedua ruas ini berdampingan (<c>AC-67</c>).
        /// </summary>
        public DateTime? PhysicallyReceivedAt { get; set; }

        public DateTime? DecidedAt { get; set; }

        public string? RejectionReasonCode { get; set; }

        public string? RejectionNote { get; set; }

        public Guid? SupersededSpecimenId { get; set; }

        public string? RecollectionCause { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// Ringkasan hasil penyerahan fakta ke Billing. Diisi hanya pada tindakan yang memang
        /// menerbitkan fakta, dan tidak pernah memuat keputusan finansial.
        /// </summary>
        public LabBillingHandoffResponse? BillingHandoff { get; set; }
    }

    /// <summary>
    /// Hasil penyerahan fakta klinis ke Billing.
    ///
    /// Berisi keterangan proses, bukan status pembayaran. Laboratorium tidak pernah menyatakan
    /// sesuatu sudah dibayar, dibatalkan secara finansial, atau disetujui penjamin.
    /// </summary>
    public class LabBillingHandoffResponse
    {
        public string Kind { get; set; } = string.Empty;

        public bool IsClinicallySafe { get; set; }

        public Guid? MilestoneFactId { get; set; }

        public int? MilestoneFactVersion { get; set; }

        public string? Code { get; set; }

        public string? Message { get; set; }

        /// <summary>
        /// Seluruh identitas fakta yang terbit dari satu keputusan.
        ///
        /// Sejak <c>FR-05.1</c>, satu wadah yang dinyatakan layak menerbitkan fakta sebanyak
        /// pemeriksaan yang ditopangnya. <see cref="MilestoneFactId"/> tetap diisi identitas
        /// fakta pertama supaya pemanggil lama tidak putus; ruas inilah yang membawa
        /// seluruhnya.
        /// </summary>
        public List<Guid> MilestoneFactIds { get; set; } = new();

        /// <summary>Jumlah fakta yang terbit — sama dengan jumlah pemeriksaan pada wadah itu.</summary>
        public int MilestoneFactCount { get; set; }
    }

    public class LabTransitionHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid LabOrderId { get; set; }

        public Guid? LabSpecimenId { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? FromStatus { get; set; }

        public string ToStatus { get; set; } = string.Empty;

        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
