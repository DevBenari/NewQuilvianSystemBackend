using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums
{
    /// <summary>
    /// Disiplin laboratorium yang menaungi sebuah pesanan, sesuai <c>LAB-DEC-025</c>.
    ///
    /// Ketiganya berjalan sejajar dengan daftar pasien dan alur hasilnya masing-masing:
    /// Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. Bank Darah sengaja tidak ada di
    /// sini karena tetap berada di luar scope modul.
    ///
    /// Nilai ini melekat pada pesanan sejak dibuat dan tidak berpindah sesudahnya
    /// (<c>INV-21</c>); penegakannya ada pada <c>LabOrderConfiguration</c>.
    /// </summary>
    public enum LabDiscipline
    {
        /// <summary>Patologi Klinik — darah, urin, feses, kimia klinik, hematologi, imunologi.</summary>
        [Display(Name = "Clinical Pathology")]
        ClinicalPathology = 1,

        /// <summary>Patologi Anatomi — makroskopik, mikroskopik, dan kesimpulan.</summary>
        [Display(Name = "Anatomical Pathology")]
        AnatomicalPathology = 2,

        /// <summary>Mikrobiologi — organisme, sensitivitas antibiotik, laporan R/I/S.</summary>
        [Display(Name = "Microbiology")]
        Microbiology = 3
    }

    /// <summary>
    /// Siklus hidup pesanan laboratorium sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    ///
    /// Seluruh nilai berasal dari requirement yang sudah dikunci pemilik; tidak ada status yang
    /// ditambahkan atas inisiatif implementasi.
    /// </summary>
    public enum LabOrderStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Requested")]
        Requested = 2,

        [Display(Name = "Accepted")]
        Accepted = 3,

        [Display(Name = "In Process")]
        InProcess = 4,

        [Display(Name = "Completed")]
        Completed = 5,

        [Display(Name = "On Hold")]
        OnHold = 6,

        [Display(Name = "Cancel Requested")]
        CancelRequested = 7,

        [Display(Name = "Cancelled")]
        Cancelled = 8
    }

    /// <summary>
    /// Siklus hidup sampel laboratorium sesuai <c>RJ-BIL-GATE-DEC-003</c>.
    ///
    /// <see cref="Received"/> dan <see cref="Accepted"/> sengaja dibedakan: sampel yang sudah
    /// sampai di laboratorium belum tentu dinyatakan layak periksa, dan hanya penetapan layak
    /// yang menjadi milestone kelayakan tagih.
    /// </summary>
    public enum LabSpecimenStatus
    {
        [Display(Name = "Planned")]
        Planned = 1,

        [Display(Name = "Collected")]
        Collected = 2,

        [Display(Name = "Received")]
        Received = 3,

        [Display(Name = "Accepted")]
        Accepted = 4,

        [Display(Name = "Rejected")]
        Rejected = 5,

        [Display(Name = "Recollection Required")]
        RecollectionRequired = 6,

        [Display(Name = "Cancelled")]
        Cancelled = 7,

        [Display(Name = "On Hold")]
        OnHold = 8
    }

    /// <summary>
    /// Siklus hidup satu pemeriksaan terpesan sesuai <c>LAB-STATE-v1</c> r2 bagian 3
    /// (<c>LAB-DEC-024</c>).
    ///
    /// Status ini <b>sebagian besar mengikuti wadah penopangnya</b> dan bukan sesuatu yang
    /// dipindahkan petugas satu per satu. Wadah yang dinyatakan layak membuat seluruh
    /// pemeriksaan di atasnya menjadi <see cref="ChargeEligible"/>; wadah yang ditolak membuat
    /// seluruhnya <see cref="Voided"/>. Satu-satunya perpindahan yang dilakukan langsung oleh
    /// petugas adalah pembatalan.
    ///
    /// <see cref="Voided"/> dan <see cref="Cancelled"/> adalah status terminal.
    ///
    /// Status hasil — <c>Pending</c>, <c>InProcess</c>, <c>Completed</c>, <c>Validated</c>,
    /// <c>Released</c> — sengaja <b>tidak</b> ada di sini. Slice hasil masih tertahan
    /// <c>LAB-SIGN-001</c>, dan menambahkan statusnya lebih dulu berarti menjanjikan perilaku
    /// yang belum diputuskan pihak klinis.
    /// </summary>
    public enum LabExaminationStatus
    {
        /// <summary>Sudah dipesan dan menunggu wadah penopangnya diputuskan.</summary>
        [Display(Name = "Ordered")]
        Ordered = 1,

        /// <summary>Wadah penopangnya dinyatakan layak, sehingga pemeriksaan ini sah ditagihkan.</summary>
        [Display(Name = "Charge Eligible")]
        ChargeEligible = 2,

        /// <summary>Gugur bersama wadah penopangnya yang ditolak.</summary>
        [Display(Name = "Voided")]
        Voided = 3,

        /// <summary>Dibatalkan petugas berwenang.</summary>
        [Display(Name = "Cancelled")]
        Cancelled = 4
    }

    /// <summary>
    /// Tingkat kesegeraan satu pemeriksaan terpesan sesuai <c>LAB-DEC-026</c>.
    ///
    /// Kesegeraan melekat pada <b>pemeriksaan</b>, bukan pada pesanan. Satu pesanan boleh
    /// memuat pemeriksaan cito dan pemeriksaan biasa sekaligus — dan itu memang keadaan yang
    /// lazim: dokter meminta elektrolit segera sementara profil lipid pasien yang sama dapat
    /// menunggu. Menyimpannya di tingkat pesanan akan memaksa seluruh isi pesanan ikut
    /// diperlakukan cito, lalu menenggelamkan pemeriksaan yang benar-benar mendesak.
    ///
    /// Setiap penandaan menyimpan waktu dan pelakunya pada baris pemeriksaan
    /// (<c>LAB-STATE-v1</c> r2 bagian 2).
    /// </summary>
    public enum LabExaminationUrgency
    {
        /// <summary>Biasa — mengikuti antrean normal laboratorium.</summary>
        [Display(Name = "Routine")]
        Routine = 1,

        /// <summary>Cito — didahulukan, dan tunduk pada batas waktu penyelesaiannya.</summary>
        [Display(Name = "Cito")]
        Cito = 2
    }

    /// <summary>
    /// Sebab pengambilan ulang sampel.
    ///
    /// Nilai ini menentukan siapa yang menanggung akibatnya: kesalahan internal rumah sakit
    /// tidak boleh otomatis menambah tanggungan pasien, sedangkan sebab kondisi pasien atau
    /// sebab eksternal memerlukan alasan dan otorisasi sebelum tagihan baru dipertimbangkan.
    /// Keputusan finansialnya tetap milik Billing, bukan Laboratorium.
    /// </summary>
    public enum LabRecollectionCause
    {
        [Display(Name = "Internal Hospital Error")]
        InternalHospitalError = 1,

        [Display(Name = "Patient or Specimen Condition")]
        PatientOrSpecimenCondition = 2,

        [Display(Name = "External Cause")]
        ExternalCause = 3
    }

    /// <summary>
    /// Objek yang berpindah status pada satu baris riwayat.
    /// </summary>
    public enum LabTransitionScope
    {
        [Display(Name = "Lab Order")]
        LabOrder = 1,

        [Display(Name = "Lab Specimen")]
        LabSpecimen = 2
    }

    /// <summary>
    /// Bentuk hasil sebuah pemeriksaan laboratorium sesuai <c>LAB-DEC-021</c> (BR-17).
    ///
    /// Tepat satu dari dua, ditetapkan sejak batas nilainya dibuat:
    /// <see cref="Numeric"/> memakai batas normal dan batas kritis berupa angka beserta
    /// satuannya; <see cref="Choice"/> memakai daftar pilihan yang sah pada
    /// <c>LabValueOption</c> dan tidak menerima pengetikan bebas (AC-28).
    /// </summary>
    public enum LabResultForm
    {
        /// <summary>Hasil angka — misalnya Kalium 3,5–5,1 mmol/L.</summary>
        [Display(Name = "Numeric")]
        Numeric = 1,

        /// <summary>Hasil pilihan terbatas — misalnya Protein urin: Negatif, +1, +2, +3, +4.</summary>
        [Display(Name = "Choice")]
        Choice = 2
    }

    /// <summary>
    /// Pembatas jenis kelamin sebuah baris batas nilai sesuai <c>LAB-DEC-018</c> (BR-14).
    ///
    /// Satu jenis pemeriksaan boleh punya beberapa baris batas yang dibedakan menurut jenis
    /// kelamin dan kelompok umur — Hemoglobin pria dewasa, wanita dewasa, dan anak berdiri
    /// sebagai tiga baris terpisah (AC-24).
    /// </summary>
    public enum LabGenderScope
    {
        /// <summary>Berlaku untuk semua jenis kelamin.</summary>
        [Display(Name = "All")]
        All = 1,

        /// <summary>Berlaku untuk pasien laki-laki saja.</summary>
        [Display(Name = "Male")]
        Male = 2,

        /// <summary>Berlaku untuk pasien perempuan saja.</summary>
        [Display(Name = "Female")]
        Female = 3
    }

    /// <summary>
    /// Daur hidup pengajuan perubahan batas kritis sesuai <c>LAB-DEC-023</c> (BR-19) dan
    /// <c>LAB-STATE-v1</c> r2 bagian 4.
    ///
    /// Batas normal boleh diubah kepala instalasi dan langsung berlaku. Batas kritis tidak:
    /// ia menentukan pada angka berapa seorang pasien dianggap terancam, dan itu penilaian
    /// klinis. Karena itu perubahannya berhenti sebagai pengajuan sampai pihak berwenang
    /// memutuskan, dan selama itu batas yang berlaku <b>tidak berubah sama sekali</b>.
    ///
    /// <see cref="Approved"/>, <see cref="Rejected"/>, dan <see cref="Withdrawn"/> adalah
    /// status terminal; memutuskan ulang pengajuan yang sudah terminal ditolak <c>409</c>.
    /// Penegakan transisi beserta larangan menyetujui pengajuan sendiri adalah pekerjaan
    /// service pada <c>BE-LAB-05</c>.
    /// </summary>
    public enum LabBoundChangeStatus
    {
        /// <summary>Diajukan kepala instalasi dan menunggu keputusan pihak klinis.</summary>
        [Display(Name = "Submitted")]
        Submitted = 1,

        /// <summary>Disetujui; batas kritis yang baru mulai berlaku.</summary>
        [Display(Name = "Approved")]
        Approved = 2,

        /// <summary>Ditolak pihak klinis; batas lama tetap berlaku.</summary>
        [Display(Name = "Rejected")]
        Rejected = 3,

        /// <summary>Ditarik oleh pengajunya sendiri sebelum diputuskan.</summary>
        [Display(Name = "Withdrawn")]
        Withdrawn = 4
    }
}
