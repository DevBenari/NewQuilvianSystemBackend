using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu <b>wadah fisik</b> laboratorium — tabung, pot, atau slide.
    ///
    /// Sejak <c>LAB-DEC-024</c> wadah bukan lagi satuan yang ditagih. Satu tabung darah ungu
    /// menopang hemoglobin, leukosit, dan trombosit sekaligus, sehingga jenis pemeriksaan dan
    /// salinan tarifnya melekat pada <see cref="LabExamination"/>, bukan di sini. Wadah hanya
    /// membawa barcode, status bahan, dan jejak waktu penanganannya.
    ///
    /// Keenam kolom salinan tarif yang dahulu ada di sini — <c>ProcedureId</c> beserta salinan
    /// kode, nama, tarif, dan harga — dihapus <c>BE-LAB-11</c>. Yang membacanya kini membaca
    /// baris pemeriksaan yang ditopang wadah ini lewat <see cref="Examinations"/>.
    ///
    /// Wadah yang ditolak tidak pernah dihapus. Pengambilan ulang membuat baris baru yang
    /// menunjuk wadah sebelumnya melalui <see cref="SupersededSpecimenId"/>.
    /// </summary>
    public class LabSpecimen : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Barcode operasional yang tidak bermakna, berbentuk <c>LSP-</c> diikuti 32 karakter
        /// heksadesimal. Dibuat server, unik, dan tidak pernah berubah setelah sampel dibuat.
        ///
        /// Nilai ini sengaja tidak memuat nama pasien, nomor rekam medis, NIK, tanggal lahir,
        /// nomor telepon, nomor encounter, penjamin, maupun diagnosis. Barcode juga bukan
        /// kredensial otorisasi dan bukan pengganti relasi database — penelusuran ke pesanan,
        /// encounter, dan pasien tetap melalui foreign key.
        /// </summary>
        [Required]
        public string SpecimenBarcode { get; set; } = string.Empty;

        /// <summary>Nomor urut sampel dalam satu pesanan, untuk keterbacaan operasional.</summary>
        public int SpecimenSequence { get; set; }

        /// <summary>
        /// Keterangan operasional bebas, misalnya "lengan kiri, tabung kedua".
        ///
        /// <b>Sejak <c>LAB-DEC-040</c> ruas ini berhenti menjadi tempat menyimpan jenis bahan.</b>
        /// Jenisnya kini tersimpan sebagai penunjuk pada <see cref="SpecimenTypeId"/>. Isi lama
        /// yang terlanjur memuat jenis sebagai teks bebas <b>tidak dihapus dan tidak
        /// dipindahkan</b> — memetakannya berarti menebak (<c>AC-61</c>).
        /// </summary>
        public string? SpecimenDescription { get; set; }

        /// <summary>
        /// Jenis bahan yang dibawa wadah ini, dipilih dari data induk
        /// <see cref="LabSpecimenType"/> (<c>LAB-DEC-040</c>, <c>BR-35</c>).
        ///
        /// <b>Kenapa boleh kosong di basis data padahal wajib di API.</b> Tabel ini sudah berisi
        /// data sejak sebelum jenis specimen menjadi data induk. Kolom wajib tanpa nilai bawaan
        /// yang masuk akal akan menggagalkan migration atau memaksa pengisian tebakan, sehingga
        /// kewajibannya ditegakkan di lapisan API untuk wadah baru (<c>VAL-51</c>) — pola yang
        /// sama dengan <c>LabOrder.Discipline</c>.
        /// </summary>
        public Guid? SpecimenTypeId { get; set; }

        /// <summary>
        /// Keterangan jenis ketika petugas memilih baris <c>Lainnya</c>.
        ///
        /// Wajib diisi bila jenis terpilih ber-<c>IsOtherBucket</c> (<c>VAL-53</c>), dan tidak
        /// boleh diisi bila jenisnya bukan <c>Lainnya</c> (<c>VAL-54</c>). Inilah bahan daftar
        /// pantau pemakaian <c>Lainnya</c> yang dibangun <c>BE-LAB-25</c>.
        /// </summary>
        public string? SpecimenTypeOtherNote { get; set; }

        /// <summary>
        /// Banyaknya bahan di dalam wadah, dinyatakan dalam satuan <see cref="VolumeUnitId"/>.
        ///
        /// <b>Tidak ada batas minimum maupun maksimum.</b> <c>RULE-021</c> menyatakan tidak ada
        /// ketentuan bisnisnya dan <c>LAB-DEC-041</c> menerimanya apa adanya: sistem tidak
        /// menolak volume yang kecil dan tidak menyimpulkan sendiri apakah bahannya cukup. Yang
        /// menyatakan bahan tidak cukup adalah petugas, lewat penetapan kelayakan
        /// (<c>AC-63</c>).
        /// </summary>
        public decimal? VolumeAmount { get; set; }

        /// <summary>
        /// Satuan volume, menunjuk <c>MstMeasurement</c> milik Master Data.
        ///
        /// <b>Laboratorium sengaja tidak membuat daftar satuannya sendiri.</b>
        /// <c>MstMeasurement</c> sudah memiliki penanda <c>IsForLaboratory</c>; daftar kedua akan
        /// melahirkan dua sumber satuan dan melanggar <c>AC-49</c>. Wajib terisi bila
        /// <see cref="VolumeAmount"/> terisi (<c>VAL-56</c>), dan satuan yang dipilih harus
        /// satuan laboratorium (<c>VAL-57</c>).
        /// </summary>
        public Guid? VolumeUnitId { get; set; }

        public LabSpecimenStatus SpecimenStatus { get; set; } = LabSpecimenStatus.Planned;

        public LabSpecimenStatus? StatusBeforeHold { get; set; }

        public DateTime? CollectedAt { get; set; }

        public Guid? CollectedByUserId { get; set; }

        /// <summary>
        /// <b>Kapan datanya masuk ke sistem</b> — diisi server saat petugas menekan tindakan
        /// penerimaan, dan <b>tidak dapat diubah endpoint mana pun</b> (<c>AC-65</c>,
        /// <c>LAB-DEC-042</c> butir 1).
        ///
        /// Tidak ada satu pun DTO permintaan Laboratorium yang memiliki ruas bernama
        /// <c>ReceivedAt</c>; nilai yang dikirim dari luar diabaikan tanpa pesan kesalahan.
        /// Bandingkan dengan <see cref="PhysicallyReceivedAt"/>, yang justru diisi petugas.
        /// </summary>
        public DateTime? ReceivedAt { get; set; }

        public Guid? ReceivedByUserId { get; set; }

        /// <summary>
        /// <b>Kapan wadahnya benar-benar sampai di meja penerimaan</b> — diisi petugas
        /// (<c>LAB-DEC-042</c>, <c>BR-37</c>).
        ///
        /// <b>Kenapa dua waktu, bukan satu yang ditimpa.</b> Waktu yang diisi manusia adalah
        /// keterangan; waktu yang dicatat sistem adalah fakta. Keduanya menjawab pertanyaan yang
        /// berbeda, dan bila yang satu menimpa yang lain, yang hilang justru kemampuan menjawab
        /// pertanyaan paling wajar di kemudian hari: apakah sampel ini terlambat dicatat, dan
        /// berapa lama.
        ///
        /// Sampel yang tiba pukul 21.10 hari Senin dan baru diregistrasi Selasa pukul 08.05
        /// muncul pada laporan penerimaan <b>hari Senin</b>, sementara selisih sebelas jamnya
        /// tetap terbaca (<c>AC-67</c>).
        ///
        /// <b>Tidak dipakai menghitung keterlambatan cito.</b> <c>AC-17</c> tetap menghitungnya
        /// dari wadah yang dinyatakan layak sampai hasil terbit, lewat
        /// <c>LabExamination.ChargeEligibleAt</c>.
        /// </summary>
        public DateTime? PhysicallyReceivedAt { get; set; }

        /// <summary>Waktu keputusan layak atau tolak. Berbeda dari waktu penerimaan fisik.</summary>
        public DateTime? DecidedAt { get; set; }

        public Guid? DecidedByUserId { get; set; }

        /// <summary>
        /// Rujukan ke katalog alasan penolakan. Disimpan bersama
        /// <see cref="RejectionReasonCode"/> agar alasan yang kelak dinonaktifkan tidak merusak
        /// riwayat yang sudah tersimpan.
        /// </summary>
        public Guid? RejectionReasonId { get; set; }

        public string? RejectionReasonCode { get; set; }

        public string? RejectionNote { get; set; }

        /// <summary>Sampel yang digantikan oleh sampel ini pada pengambilan ulang.</summary>
        public Guid? SupersededSpecimenId { get; set; }

        public LabRecollectionCause? RecollectionCause { get; set; }

        public string? RecollectionReason { get; set; }

        public Guid? RecollectionAuthorizedByUserId { get; set; }

        public DateTime? RecollectionAuthorizedAt { get; set; }

        public int Version { get; set; }

        public LabOrder? LabOrder { get; set; }

        public MstLabRejectionReason? RejectionReason { get; set; }

        /// <summary>
        /// Jenis bahan wadah ini. Relasinya <c>Restrict</c>: jenis yang sudah menempel pada
        /// sebuah wadah tidak dapat dihapus, karena riwayat penerimaan lama harus tetap terbaca.
        /// </summary>
        public LabSpecimenType? SpecimenType { get; set; }

        /// <summary>
        /// Satuan volume wadah ini. Relasinya <c>Restrict</c> dengan alasan yang sama seperti
        /// <see cref="SpecimenType"/>, dan tabelnya milik Master Data — Laboratorium hanya
        /// menunjuk, tidak pernah mengubahnya.
        /// </summary>
        public MstMeasurement? VolumeUnit { get; set; }

        public LabSpecimen? SupersededSpecimen { get; set; }

        /// <summary>
        /// Pemeriksaan yang ditopang wadah ini. Satu wadah menopang satu atau lebih pemeriksaan
        /// (<c>LAB-DEC-024</c>, <c>AC-35</c>) — satu tabung darah ungu dapat menopang
        /// hemoglobin, leukosit, dan trombosit sekaligus dengan satu barcode.
        /// </summary>
        public ICollection<LabExamination> Examinations { get; set; } = new List<LabExamination>();
    }
}
