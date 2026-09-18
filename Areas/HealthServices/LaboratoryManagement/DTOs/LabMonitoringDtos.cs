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

        /// <summary>
        /// NIK pasien, cocok sebagian (<c>LAB-API-v1</c> <c>r18</c>, <c>BR-50</c> butir 1).
        ///
        /// <b>Ruas tersendiri, bukan bagian dari <see cref="Search"/>, dan itu keputusan.</b>
        /// Memasukkan NIK ke pencarian bebas membuat ketiga menu Pemeriksaan yang sudah berjalan
        /// mengembalikan baris yang sebelumnya tidak muncul — tanpa satu pun layar meminta
        /// perubahan itu. Pelebaran diam-diam sama merusaknya dengan pengetatan diam-diam.
        ///
        /// Layar memasangkannya dengan <see cref="MedicalRecordNumber"/> lewat pilihan
        /// <i>Cari menurut</i>, bukan dengan menebak bentuk isian — keputusan pemilik modul
        /// 2026-09-17, <c>r18</c> bagian 13.7.
        /// </summary>
        public string? IdentityNumber { get; set; }

        /// <summary>
        /// Waktu mana yang dibandingkan terhadap <see cref="StartDate"/> dan
        /// <see cref="EndDate"/> (<c>r18</c>).
        ///
        /// <b>Kosong berarti <see cref="LabDateCategory.OrderDate"/></b>, sehingga pemanggil
        /// yang tidak mengirimnya memperoleh perilaku yang persis sama dengan sebelum ruas ini
        /// ada.
        /// </summary>
        public LabDateCategory? DateCategory { get; set; }

        /// <summary>
        /// Awal periode. Waktu pembandingnya ditentukan <see cref="DateCategory"/>; bawaannya
        /// waktu pesanan dibuat.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Akhir periode. Pembandingnya sama dengan <see cref="StartDate"/>.</summary>
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

        /// <summary>
        /// Nomor pesanan yang dapat disebut manusia (<c>LAB-API-v1</c> <c>r16</c>).
        ///
        /// Ditempatkan di sini, bukan hanya pada <c>LabOrderListResponse</c>, karena ketiga menu
        /// Pemeriksaan membaca grup <b>Lab Monitoring</b> — pelajaran <c>r13</c>/<c>r14</c> yang
        /// diterapkan sejak awal alih-alih diulang.
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public string? EncounterNumber { get; set; }

        public Guid? PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        /// <summary>
        /// Gender pasien sebagai <b>nama enum</b> — <c>Male</c>, <c>Female</c>,
        /// <c>Unknown</c>, <c>NotDisclosed</c>, atau <c>null</c>
        /// (<c>LAB-API-v1</c> <c>r19</c>, <c>BR-50</c> bagian 5.3).
        ///
        /// Bentuknya mengikuti ruas enum lain pada DTO ini — <see cref="OrderStatus"/>,
        /// <see cref="EncounterType"/>, dan <see cref="PaymentType"/> seluruhnya <c>string</c>
        /// berisi nama enum. Nol pola baru diperkenalkan.
        ///
        /// <b><c>null</c> tetap mungkin dan bukan kelalaian:</b> <c>MstPatient.Gender</c>
        /// nullable, dan pesanan tanpa kunjungan nol punya pasien yang dapat dibaca. Layar
        /// memperlakukan <c>null</c> sama dengan <c>Unknown</c>.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Golongan darah pasien sebagai <b>nama enum</b> — <c>APositive</c>, <c>ONegative</c>,
        /// dan seterusnya, atau <c>Unknown</c>/<c>NotDisclosed</c>/<c>null</c>
        /// (<c>LAB-API-v1</c> <c>r20</c>).
        ///
        /// Dipakai <b>Label Goldar</b>, label yang menempel pada tube berisi sampling pasien.
        ///
        /// <b>Singkatannya dibentuk layar, bukan dikirim dari sini.</b> Label tube 50×25 mm
        /// menuntut <c>A+</c>, sedangkan layar dan Nota Lab masih muat menuliskan
        /// <c>A Positif</c>; mengirim singkatannya akan mengunci keduanya pada satu bentuk.
        ///
        /// <c>MstPatient.BloodType</c> non-nullable, tetapi ruas ini tetap nullable karena
        /// pesanan tanpa kunjungan nol punya pasien yang dapat dibaca.
        /// </summary>
        public string? BloodType { get; set; }

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

        /// <summary>
        /// Waktu pesanan dikonfirmasi (<c>LAB-API-v1</c> <c>r14</c>, <c>LAB-DEC-061</c>).
        /// Kosong selama pesanan belum dikonfirmasi.
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Nama konfirmator, <b>siap ditampilkan</b> (<c>AC-94</c>).
        /// </summary>
        public string? ConfirmedByName { get; set; }

        /// <summary>
        /// Nama dokter pemeriksa, <b>siap ditampilkan</b> (<c>AC-95</c>).
        ///
        /// <para>
        /// <b>Nol penunjuk dikirim pada ketiga ruas ini, dan itu disengaja.</b> Daftar pantau
        /// adalah layar <b>baca</b>: ia menampilkan antrean dan tidak melakukan aksi apa pun
        /// terhadap dokter pemeriksa maupun konfirmator. Penunjuk hanya dibutuhkan aksi, dan
        /// aksi pada modul ini berjalan lewat detail pesanan — yang sudah membawa
        /// <c>ConfirmedByUserId</c> dan <c>ExaminerDoctorId</c> sejak <c>r13</c>. Mengirim
        /// penunjuk yang tidak dipakai berarti mengirim nilai yang tidak boleh ditampilkan
        /// (<c>no-uuid-display</c>) ke layar yang tidak membutuhkannya.
        /// </para>
        /// </summary>
        public string? ExaminerDoctorName { get; set; }
    }
}
