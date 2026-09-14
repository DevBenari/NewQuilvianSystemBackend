using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs
{
    /* ------------------------------------------------------------------ *
     * Permintaan
     * ------------------------------------------------------------------ */

    public class CreateRadOrderRequest
    {
        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ProcedureId { get; set; }

        [Required]
        public Guid ModalityId { get; set; }

        public string? ClinicalIndication { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi pesanan. Boleh kosong; bila terisi tetapi tidak
        /// cocok dengan perawatan milik kunjungannya, permintaan ditolak <c>400</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-22</c>, <c>INV-DOK-12</c>. Inilah yang membuat pesanan
        /// perawatan A tidak dapat diproses sebagai milik perawatan B. Tanpa penanda ini, satu
        /// pasien yang dirawat dua kali dalam sebulan memiliki dua rangkaian pesanan yang
        /// bercampur pada layar dokter.
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Penanda cito — <c>RAD-DEC-013</c>. <b>Boleh kosong</b>, dan kosong berarti tidak cito.
        /// </summary>
        /// <remarks>
        /// <c>FR-RAD-065</c>. Dibuat <c>bool?</c> dan bukan <c>bool</c> supaya pemanggil lama
        /// yang tidak mengenal field ini tetap berhasil membuat pesanan tanpa perubahan apa pun
        /// di sisi mereka. Modul Rawat Jalan, IGD, dan Rawat Inap sudah memanggil endpoint ini
        /// hari ini; menjadikan field ini wajib akan merusak ketiganya sekaligus.
        /// </remarks>
        public bool? IsUrgent { get; set; }
    }

    /// <summary>
    /// Mengubah penanda cito setelah pesanan dibuat — <c>RAD-DEC-013</c>.
    /// </summary>
    public class RadOrderUrgencyRequest
    {
        /// <summary>Nyalakan untuk menandai cito, matikan untuk mencabutnya.</summary>
        [Required]
        public bool IsUrgent { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pesanan radiologi — <c>RAD-CONF-001</c> bagian 8 butir 3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sebelum ini <c>GET /rad-orders</c> hanya menerima <c>encounterId</c>. Akibatnya lima
    /// kategori daftar pasien radiologi — rawat jalan, rawat inap, pasien luar, persiapan, dan
    /// perjanjian — tidak dapat dibentuk sama sekali, dan layar riwayat terpaksa menarik seluruh
    /// pesanan rumah sakit lalu menyaringnya di browser.
    /// </para>
    /// <para>
    /// <b><c>EncounterId</c> tetap bernama sama.</b> Pemanggil lama yang mengirim
    /// <c>?encounterId=...</c> terus bekerja tanpa perubahan apa pun di sisi mereka; bentuk
    /// objek ini hanya menambah parameter, tidak mengganti satu pun yang sudah ada.
    /// </para>
    /// <para>
    /// Bentuknya mengikuti <c>LabMonitoringQuery</c> yang sudah berjalan pada modul
    /// Laboratorium, supaya layar radiologi dapat memakai komponen penyaring yang sama.
    /// </para>
    /// </remarks>
    public class RadOrderListQuery
    {
        /// <summary>Menyaring pesanan milik satu kunjungan. Parameter lama, tidak berubah.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Menyaring pesanan milik satu pasien, lintas kunjungan.</summary>
        public Guid? PatientId { get; set; }

        /// <summary>Nomor rekam medis, cocok sebagian.</summary>
        public string? MedicalRecordNumber { get; set; }

        /// <summary>Nomor registrasi/kunjungan, cocok sebagian.</summary>
        public string? EncounterNumber { get; set; }

        /// <summary>Nomor pesanan radiologi, cocok sebagian.</summary>
        public string? OrderNumber { get; set; }

        /// <summary>Nomor foto/study pada salah satu study pesanan, cocok sebagian.</summary>
        public string? StudyNumber { get; set; }

        /// <summary>
        /// Awal periode. Dihitung dari waktu pemesanan, dan jatuh ke waktu baris dibuat ketika
        /// pesanan lama tidak memilikinya.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Akhir periode.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Jenis kunjungan: rawat jalan, rawat inap, gawat darurat. Inilah yang memisahkan
        /// kategori daftar pasien radiologi.
        /// </summary>
        public int? EncounterType { get; set; }

        /// <summary>Unit layanan asal pesanan.</summary>
        public Guid? ServiceUnitId { get; set; }

        /// <summary>Ruangan asal pesanan.</summary>
        public Guid? RoomId { get; set; }

        /// <summary>Alat pencitraan yang diminta.</summary>
        public Guid? ModalityId { get; set; }

        /// <summary>Jenis pemeriksaan yang diminta.</summary>
        public Guid? ProcedureId { get; set; }

        /// <summary>Status operasional pesanan.</summary>
        public RadOrderStatus? OrderStatus { get; set; }

        /// <summary>Menyaring pesanan bertanda cito saja — <c>RAD-DEC-013</c>.</summary>
        public bool? OnlyUrgent { get; set; }

        /// <summary>
        /// Menyaring pesanan yang sudah mempunyai tanggal pemeriksaan terjadwal. Inilah yang
        /// membentuk daftar Pasien Perjanjian.
        /// </summary>
        public bool? OnlyScheduled { get; set; }

        /// <summary>Awal periode jadwal pemeriksaan, dipakai daftar perjanjian.</summary>
        public DateTime? ScheduledFrom { get; set; }

        /// <summary>Akhir periode jadwal pemeriksaan.</summary>
        public DateTime? ScheduledTo { get; set; }

        /// <summary>
        /// Pencarian bebas pada nama pasien, nomor rekam medis, nomor kunjungan, dan nomor
        /// pesanan.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>Kolom pengurutan. Nilainya diambil dari metadata penyaring.</summary>
        public string? SortBy { get; set; }

        /// <summary>Arah pengurutan, <c>asc</c> atau <c>desc</c>. Bawaannya <c>desc</c>.</summary>
        public string? SortDirection { get; set; }

        /// <summary>
        /// Batas jumlah baris yang dikembalikan. Bawaannya <see cref="DefaultLimit"/>, paling
        /// banyak <see cref="MaxLimit"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Mengapa ada batas, dan mengapa bukan paging penuh.</b> Sebelum penyaring ini
        /// ditambahkan, daftar pesanan hampir selalu dipanggil dengan <c>encounterId</c>,
        /// sehingga jumlah barisnya dengan sendirinya kecil. Penyaring riwayat mengubah itu:
        /// <c>?search=budi</c> adalah pertanyaan se-rumah-sakit, dan tanpa batas ia akan menarik
        /// seluruh pesanan yang pernah ada.
        /// </para>
        /// <para>
        /// Perpindahan ke <c>PagedResult</c> sudah ditargetkan <c>RAD-API-001</c>, tetapi ia
        /// mengubah bentuk response dan merusak pemanggil yang sudah ada — karena itu tetap
        /// menjadi task tersendiri. Batas baris menutup lubangnya tanpa mengubah bentuk apa pun.
        /// </para>
        /// <para>
        /// Barisnya diambil dari ujung urutan yang diminta, jadi bawaan <c>desc</c> memberi yang
        /// terbaru. Nilai bawaan sengaja jauh di atas jumlah pesanan satu kunjungan mana pun,
        /// sehingga pemanggil lama tidak akan pernah menyentuhnya.
        /// </para>
        /// </remarks>
        public int? Limit { get; set; }

        /// <summary>Batas baris bawaan ketika pemanggil tidak menyebutkannya.</summary>
        public const int DefaultLimit = 200;

        /// <summary>Batas baris tertinggi yang dilayani.</summary>
        public const int MaxLimit = 1000;

        /// <summary>Batas baris yang benar-benar dipakai, sudah dijepit ke rentang yang sah.</summary>
        public int BatasBaris() => Limit is null or < 1
            ? DefaultLimit
            : Math.Min(Limit.Value, MaxLimit);
    }

    /// <summary>
    /// Satu baris daftar kerja petugas pada sebuah alat pencitraan — <c>RAD-DEC-012</c>.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada tabel daftar kerja.</b> Seluruh isi baris ini dihitung dari <c>RadOrder</c>
    /// dan <c>RadStudy</c> yang sudah ada. Pesanan yang dibatalkan langsung hilang dari daftar
    /// tanpa proses penyelarasan apa pun, karena memang tidak ada apa pun yang perlu
    /// diselaraskan — <c>FR-RAD-061</c>.
    /// </remarks>
    public class RadWorklistItemResponse
    {
        public Guid RadOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public Guid ModalityId { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        public string OrderStatus { get; set; } = string.Empty;

        /// <summary>Teks keadaan siap tampil dalam Bahasa Indonesia.</summary>
        public string OrderStatusLabel { get; set; } = string.Empty;

        /// <summary>
        /// Penanda cito. Baris bertanda ini berada di urutan atas — <c>FR-RAD-062</c>.
        /// </summary>
        public bool IsUrgent { get; set; }

        public DateTime? UrgentMarkedAt { get; set; }

        public DateTime? RequestedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Waktu yang dipakai menempatkan pekerjaan ini pada sebuah hari kerja: jadwal bila
        /// sudah dijadwalkan, kalau tidak waktu pemesanan, kalau tidak waktu pesanan dibuat.
        /// </summary>
        public DateTime WorkAt { get; set; }

        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Study yang sudah lahir dari pesanan ini. <b>Kosong bukan berarti tidak ada
        /// pekerjaan</b> — justru pesanan tanpa study adalah yang belum direncanakan sama
        /// sekali.
        /// </summary>
        public List<RadWorklistStudyResponse> Studies { get; set; } = new();
    }

    /// <summary>Satu study pada baris daftar kerja.</summary>
    public class RadWorklistStudyResponse
    {
        public Guid Id { get; set; }

        public string StudyNumber { get; set; } = string.Empty;

        public int StudySequence { get; set; }

        public string StudyStatus { get; set; } = string.Empty;

        public string StudyStatusLabel { get; set; } = string.Empty;

        /// <summary>
        /// Penanda cito, <b>diturunkan dari pesanannya</b> — <c>AC-41</c>.
        /// </summary>
        /// <remarks>
        /// Sengaja tidak disimpan sebagai kolom pada <c>RadStudy</c>. Kolom salinan akan
        /// berselisih dengan pesanannya begitu penandanya diubah lewat
        /// <c>PUT /rad-orders/{id}/urgency</c>, dan dua sumber kebenaran untuk pertanyaan
        /// "mana yang mendesak" lebih buruk daripada satu yang perlu digabungkan.
        /// </remarks>
        public bool IsUrgent { get; set; }

        public bool? IsUsable { get; set; }
    }

    public class RadOrderTransitionRequest
    {
        public string? Reason { get; set; }

        public DateTime? ScheduledAt { get; set; }
    }

    public class CreateRadStudyRequest
    {
        /// <summary>
        /// Pemeriksaan study ini. Kosong berarti mengikuti pemeriksaan pesanannya.
        /// </summary>
        public Guid? ProcedureId { get; set; }
    }

    public class RadSafetyCheckDecisionRequest
    {
        [Required]
        public Guid SafetyRequirementId { get; set; }

        [Required]
        public RadSafetyCheckState CheckState { get; set; }

        public string? Note { get; set; }
    }

    public class RadAcquisitionQualityRequest
    {
        /// <summary>
        /// Apakah citra dapat dipakai untuk pembacaan klinis. Inilah yang menentukan kelayakan
        /// tagih normal; nilainya tidak boleh disimpulkan dari status apa pun.
        /// </summary>
        [Required]
        public bool IsUsable { get; set; }

        public string? QualityNote { get; set; }
    }

    public class RadAbortAcquisitionRequest
    {
        [Required]
        public RadAbortCause AbortCause { get; set; }

        [Required]
        public string AbortReason { get; set; } = string.Empty;

        public string? PerformedPortionNote { get; set; }
    }

    public class RadRepeatStudyRequest
    {
        [Required]
        public RadRepeatCause RepeatCause { get; set; }

        [Required]
        public string RepeatReason { get; set; } = string.Empty;

        /// <summary>
        /// Pesanan tambahan yang mengesahkan pengulangan. Wajib ketika sebabnya adalah
        /// kebutuhan klinis baru — <c>RJ-BIL-GATE-DEC-004</c> menuntut order yang sah untuk
        /// kasus itu, bukan sekadar alasan bebas.
        /// </summary>
        public Guid? AdditionalOrderId { get; set; }
    }

    public class RadConsumptionRequest
    {
        [Required]
        public RadConsumptionItemType ItemType { get; set; }

        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;

        public bool ConsumedDespiteFailure { get; set; }

        public string? Note { get; set; }
    }

    /* ------------------------------------------------------------------ *
     * Balasan
     * ------------------------------------------------------------------ */

    /// <summary>
    /// Konteks pasien dan kunjungan pada sebuah pesanan radiologi — <c>RAD-CONF-001</c>
    /// bagian 8 butir 1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Seluruhnya dibaca, tidak satu pun disalin.</b> Pemilik data ini tetap
    /// <c>registration-management</c> dan <c>patient-management</c>; Radiologi hanya
    /// menggabungkannya saat membalas permintaan. Tidak ada kolom baru di tabel radiologi mana
    /// pun untuk menampung isi bagian ini, dan itu disengaja — nama pasien yang disalin ke tabel
    /// radiologi akan berselisih dengan aslinya begitu pendaftaran memperbaiki ejaan nama.
    /// </para>
    /// <para>
    /// <b>Mengapa ikut pada setiap baris daftar.</b> Tanpa ini, layar yang menampilkan 50
    /// pesanan harus memanggil registrasi 50 kali lagi hanya untuk mendapatkan nama dan nomor
    /// rekam medis. Selain lambat, urutan balasan yang tidak dijamin membuat nama pasien
    /// berisiko menempel pada baris pesanan milik pasien lain — dan pemeriksaan atas nama yang
    /// salah adalah kejadian keselamatan, bukan cacat tampilan.
    /// </para>
    /// <para>
    /// <b>Alergi belum ikut.</b> PRD menyebutnya bersyarat, dan di repository ini alergi hanya
    /// tersimpan sebagai isi dokumen milik <c>medical-record-management</c>. Membacanya dari
    /// Radiologi adalah titik sentuh lintas modul yang belum diputuskan, sehingga sengaja tidak
    /// dikarang di sini.
    /// </para>
    /// </remarks>
    public class RadOrderPatientContextResponse
    {
        public Guid? PatientId { get; set; }

        /// <summary>Nomor rekam medis.</summary>
        public string? MedicalRecordNumber { get; set; }

        public string? PatientName { get; set; }

        /// <summary>Jenis kelamin sebagai teks. Kosong bila data pasien tidak mencatatnya.</summary>
        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Umur pada saat kunjungan dibuat, sebagaimana dibekukan pendaftaran. Dipakai apa
        /// adanya — Radiologi tidak menghitung ulang umur, karena hasil hitungannya akan berbeda
        /// dari yang tercetak pada berkas kunjungan.
        /// </summary>
        public string? AgeText { get; set; }

        /// <summary>Nomor registrasi kunjungan.</summary>
        public string? EncounterNumber { get; set; }

        /// <summary>Rawat jalan, rawat inap, atau gawat darurat.</summary>
        public string? EncounterType { get; set; }

        /// <summary>Kunjungan baru atau kunjungan lama.</summary>
        public string? VisitType { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public Guid? RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid? PatientClassId { get; set; }

        /// <summary>Kelas perawatan.</summary>
        public string? PatientClassName { get; set; }

        /// <summary>
        /// Jenis penjamin kunjungan: tunai, asuransi, atau penjamin perusahaan.
        /// </summary>
        public string? PaymentType { get; set; }

        /// <summary>
        /// Nama penjamin sebagaimana dibekukan pendaftaran.
        /// </summary>
        /// <remarks>
        /// <b>Ini nama penjamin, bukan status pembayaran.</b> Radiologi tidak memiliki satu pun
        /// status finansial — <c>RJ-BIL-GATE-DEC-004</c> — dan tidak boleh menyimpulkan lunas
        /// atau belum lunas dari field ini. Penyajian status pembayaran menunggu
        /// <c>RAD-CONF-DEC-02</c>.
        /// </remarks>
        public string? GuarantorName { get; set; }
    }

    public class RadOrderListResponse
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Nomor pesanan yang terbaca manusia, dipakai pencarian, label, dan riwayat —
        /// <c>RAD-CONF-001</c> bagian 8 butir 2.
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        /// <summary>
        /// Konteks pasien dan kunjungan, dibaca dari pendaftaran. Kosong hanya bila kunjungannya
        /// benar-benar tidak ditemukan.
        /// </summary>
        public RadOrderPatientContextResponse? Patient { get; set; }

        /// <summary>Perawatan rawat inap yang menaungi pesanan, bila ada.</summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Benar ketika hasil pemeriksaan sudah final dan sah dipakai sebagai dasar keputusan
        /// klinis.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-052</c>, <c>VAL-DOK-30</c>. Hasil yang belum final <b>wajib</b> ditandai
        /// dan tidak boleh disajikan sebagai hasil sah. Hasil basi di layar dokter adalah risiko
        /// keselamatan, bukan masalah tampilan.
        /// </remarks>
        public bool IsResultFinal { get; set; }

        /// <summary>Keterangan singkat ketersediaan hasil, siap ditampilkan apa adanya.</summary>
        public string ResultAvailabilityNote { get; set; } = string.Empty;

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public Guid ModalityId { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        /// <summary>
        /// Status operasional pesanan. Bukan status pembayaran — Radiologi tidak memiliki status
        /// finansial apa pun.
        /// </summary>
        public string OrderStatus { get; set; } = string.Empty;

        public int StudyCount { get; set; }

        /// <summary>
        /// Jumlah study yang benar-benar dikerjakan dan menghasilkan citra yang dapat dipakai,
        /// yaitu jumlah study yang sudah memenuhi milestone kelayakan tagih.
        /// </summary>
        public int UsableStudyCount { get; set; }

        public bool IsCancel { get; set; }

        /// <summary>
        /// Penanda cito. Daftar kerja mendahulukan pesanan bertanda ini —
        /// <c>RAD-DEC-013</c>.
        /// </summary>
        public bool IsUrgent { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class RadOrderDetailResponse : RadOrderListResponse
    {
        /// <summary>Siapa yang menandai pesanan ini cito. Kosong ketika tidak cito.</summary>
        public Guid? UrgentMarkedByUserId { get; set; }

        /// <summary>Kapan pesanan ini ditandai cito. Kosong ketika tidak cito.</summary>
        public DateTime? UrgentMarkedAt { get; set; }

        public string? ClinicalIndication { get; set; }

        public DateTime? RequestedAt { get; set; }

        /// <summary>Siapa yang mengonfirmasi pesanan ini. Kosong berarti belum dikonfirmasi.</summary>
        /// <remarks>
        /// <para>
        /// <c>RAD-CONF-001</c> bagian 8 butir 4. Datanya <b>tidak</b> disimpan sebagai kolom
        /// pada tabel pesanan; ia dibaca dari <c>RadTransitionHistory</c> — baris
        /// <c>Order.Accept</c> — yang memang sudah mencatat pelaku dan waktunya sejak awal.
        /// </para>
        /// <para>
        /// Menyalinnya menjadi kolom tersendiri berarti dua sumber kebenaran untuk satu fakta,
        /// dan yang kedua pasti akan berselisih dengan riwayat cepat atau lambat. Yang
        /// diselesaikan di sini hanyalah keharusan frontend memanggil endpoint riwayat terpisah
        /// hanya untuk menampilkan dua kolom pada layar detail.
        /// </para>
        /// <para>
        /// Yang dipakai adalah konfirmasi <b>pertama</b>. Pesanan yang sempat ditahan lalu
        /// dilanjutkan tidak berpindah konfirmator, karena yang ditanya layar adalah siapa yang
        /// dahulu menerima pesanan ini ke radiologi.
        /// </para>
        /// </remarks>
        public Guid? ConfirmedByUserId { get; set; }

        /// <summary>Kapan pesanan ini dikonfirmasi. Kosong berarti belum dikonfirmasi.</summary>
        public DateTime? ConfirmedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? StatusBeforeHold { get; set; }

        public string? ClosureReason { get; set; }

        public int Version { get; set; }

        public List<RadStudyResponse> Studies { get; set; } = new();
    }

    public class RadStudyResponse
    {
        public Guid Id { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid EncounterId { get; set; }

        public string StudyNumber { get; set; } = string.Empty;

        public int StudySequence { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public Guid ModalityId { get; set; }

        public string? ModalityCode { get; set; }

        public string StudyStatus { get; set; } = string.Empty;

        public DateTime? PatientVerifiedAt { get; set; }

        public DateTime? SafetyClearedAt { get; set; }

        public int? SafetyRuleVersionAtClearance { get; set; }

        public DateTime? AcquisitionStartedAt { get; set; }

        public DateTime? AcquiredAt { get; set; }

        /// <summary>Kosong berarti belum dinilai, bukan berarti tidak dapat dipakai.</summary>
        public bool? IsUsable { get; set; }

        public string? QualityNote { get; set; }

        public string? AbortCause { get; set; }

        public string? AbortReason { get; set; }

        public string? PerformedPortionNote { get; set; }

        public Guid? RepeatOfStudyId { get; set; }

        public string? RepeatCause { get; set; }

        public string? RepeatReason { get; set; }

        public Guid? AdditionalOrderId { get; set; }

        public bool BillingFactSubmitted { get; set; }

        public DateTime? BillingFactSubmittedAt { get; set; }

        public int Version { get; set; }

        public List<RadStudySafetyCheckResponse> SafetyChecks { get; set; } = new();

        public List<RadConsumptionResponse> Consumptions { get; set; } = new();
    }

    public class RadStudySafetyCheckResponse
    {
        public Guid Id { get; set; }

        public Guid SafetyRequirementId { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; }

        public string CheckState { get; set; } = string.Empty;

        public DateTime? DecidedAt { get; set; }

        public string? Note { get; set; }
    }

    public class RadConsumptionResponse
    {
        public Guid Id { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string ItemCode { get; set; } = string.Empty;

        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public bool ConsumedDespiteFailure { get; set; }

        public DateTime RecordedAt { get; set; }

        public string? Note { get; set; }
    }

    public class RadTransitionHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid RadOrderId { get; set; }

        public Guid? RadStudyId { get; set; }

        public string Scope { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string? FromStatus { get; set; }

        public string ToStatus { get; set; } = string.Empty;

        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    public class RadModalityResponse
    {
        public Guid Id { get; set; }

        public string ModalityCode { get; set; } = string.Empty;

        public string ModalityName { get; set; } = string.Empty;

        public bool UsesIonisingRadiation { get; set; }

        public bool SupportsContrast { get; set; }

        /// <summary>
        /// Apakah modalitas ini sudah punya aturan keselamatan aktif. Bernilai <c>false</c>
        /// berarti setiap acquisition padanya akan ditolak sampai admin menetapkan aturannya.
        /// </summary>
        public bool HasActiveSafetyRule { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class RadSafetyRequirementResponse
    {
        public Guid Id { get; set; }

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? Description { get; set; }

        public bool RequiresNote { get; set; }

        public string? SourceNote { get; set; }

        public bool IsActive { get; set; }
    }

    /* ==================================================================== *
     * Aturan keselamatan — siklus pengesahan RAD-DEC-005
     * ==================================================================== */

    /// <summary>
    /// Menyusun draf aturan keselamatan.
    ///
    /// Aturan yang lahir dari sini <b>belum berlaku</b>. Ia berstatus <c>Draft</c> sampai
    /// diajukan dan disahkan penanggung jawab klinis.
    /// </summary>
    public class CreateRadSafetyRuleRequest
    {
        [Required]
        public Guid ModalityId { get; set; }

        /// <summary>
        /// Pemeriksaan tertentu yang dikenai aturan ini. Dikosongkan berarti aturan berlaku
        /// untuk seluruh pemeriksaan pada alat tersebut.
        /// </summary>
        public Guid? ProcedureId { get; set; }

        [Required]
        public Guid SafetyRequirementId { get; set; }

        public bool IsMandatory { get; set; } = true;

        /// <summary>
        /// Mulai berlaku. Dikosongkan berarti berlaku sejak aturannya disahkan.
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Mengubah draf aturan keselamatan.
    ///
    /// Hanya draf yang dapat diubah. Aturan yang sedang berlaku wajib diganti lewat draf baru
    /// dan pengesahan ulang, supaya penilaian study yang sudah terjadi tidak berubah artinya.
    /// </summary>
    public class UpdateRadSafetyRuleRequest
    {
        [Required]
        public Guid ModalityId { get; set; }

        public Guid? ProcedureId { get; set; }

        [Required]
        public Guid SafetyRequirementId { get; set; }

        public bool IsMandatory { get; set; } = true;

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Menolak pengajuan aturan keselamatan. Alasan wajib diisi — tanpa itu penyusun aturan
    /// tidak tahu apa yang harus diperbaiki.
    /// </summary>
    public class RadSafetyRuleRejectRequest
    {
        [Required]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Penyaring daftar aturan keselamatan.
    ///
    /// Setiap field di sini benar-benar diproses query. Menambah field tanpa memprosesnya
    /// berarti layar menawarkan penyaring yang tidak mengubah apa pun.
    /// </summary>
    public class RadSafetyRulePagedQuery
    {
        /// <summary>Dicari pada kode dan nama alat, serta kode dan nama butir keselamatan.</summary>
        public string? Search { get; set; }

        public Guid? ModalityId { get; set; }

        public Guid? SafetyRequirementId { get; set; }

        public RadSafetyRuleStatus? RuleStatus { get; set; }

        public bool? IsMandatory { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class RadSafetyRuleResponse
    {
        public Guid Id { get; set; }

        public Guid ModalityId { get; set; }

        public string? ModalityCode { get; set; }

        public string? ModalityName { get; set; }

        public Guid? ProcedureId { get; set; }

        public Guid SafetyRequirementId { get; set; }

        public string? RequirementCode { get; set; }

        public string? RequirementName { get; set; }

        public bool IsMandatory { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Nomor versi aturan. Naik satu setiap kali aturan disahkan, dan dibekukan pada study
        /// yang sudah dinyatakan lolos.
        /// </summary>
        public int RuleVersion { get; set; }

        /// <summary>
        /// Keadaan pada siklus pengesahan: <c>Draft</c>, <c>PendingApproval</c>, <c>Active</c>,
        /// atau <c>Inactive</c>. Hanya <c>Active</c> yang dinilai gerbang keselamatan.
        /// </summary>
        public string RuleStatus { get; set; } = string.Empty;

        public string? Note { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        /// <summary>
        /// Alasan penolakan terakhir. Sengaja tidak dihapus ketika aturannya diajukan ulang,
        /// karena jejak penolakan termasuk audit yang tidak boleh diubah.
        /// </summary>
        public string? RejectionReason { get; set; }
    }
}
