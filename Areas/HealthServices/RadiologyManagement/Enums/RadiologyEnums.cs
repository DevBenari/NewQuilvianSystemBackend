using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums
{
    /// <summary>
    /// Siklus hidup pesanan radiologi sesuai <c>RJ-BIL-GATE-DEC-004</c>.
    ///
    /// Seluruh nilai berasal dari requirement yang sudah dikunci pemilik. Tidak ada satu pun
    /// status destruktif: pembatalan dan penolakan adalah status tersendiri, bukan penghapusan
    /// baris — acceptance criteria 1 <c>GATE-DEC-004</c> melarangnya.
    /// </summary>
    public enum RadOrderStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Requested")]
        Requested = 2,

        [Display(Name = "Accepted")]
        Accepted = 3,

        [Display(Name = "Scheduled")]
        Scheduled = 4,

        [Display(Name = "In Progress")]
        InProgress = 5,

        [Display(Name = "Completed")]
        Completed = 6,

        [Display(Name = "On Hold")]
        OnHold = 7,

        [Display(Name = "Cancel Requested")]
        CancelRequested = 8,

        [Display(Name = "Cancelled")]
        Cancelled = 9,

        [Display(Name = "Rejected")]
        Rejected = 10
    }

    /// <summary>
    /// Siklus hidup study/acquisition sesuai <c>RJ-BIL-GATE-DEC-004</c>.
    ///
    /// <see cref="PatientVerified"/> dan <see cref="SafetyCleared"/> sengaja dipisah. Identitas
    /// yang benar tidak dengan sendirinya berarti pemeriksaannya aman dilakukan: pasien yang
    /// benar tetap dapat sedang hamil, membawa implan logam, atau alergi kontras. Melebur
    /// keduanya akan membuat satu centang menutup dua pertanyaan yang berbeda.
    ///
    /// <see cref="Acquired"/> dan <see cref="QualityAccepted"/> juga dipisah, dan pemisahan itu
    /// yang menentukan uang: pemeriksaan yang sudah dilakukan belum tentu menghasilkan citra
    /// yang dapat dipakai, dan hanya yang dapat dipakai yang menjadi dasar kelayakan tagih.
    /// </summary>
    public enum RadStudyStatus
    {
        [Display(Name = "Planned")]
        Planned = 1,

        [Display(Name = "Patient Verified")]
        PatientVerified = 2,

        [Display(Name = "Safety Cleared")]
        SafetyCleared = 3,

        [Display(Name = "Acquisition Started")]
        AcquisitionStarted = 4,

        [Display(Name = "Acquired")]
        Acquired = 5,

        [Display(Name = "Quality Accepted")]
        QualityAccepted = 6,

        [Display(Name = "On Hold")]
        OnHold = 7,

        [Display(Name = "Aborted")]
        Aborted = 8,

        [Display(Name = "Quality Rejected")]
        QualityRejected = 9,

        [Display(Name = "Repeat Required")]
        RepeatRequired = 10,

        [Display(Name = "Cancelled")]
        Cancelled = 11
    }

    /// <summary>
    /// Keadaan satu butir gerbang keselamatan pada sebuah study.
    ///
    /// <see cref="NotApplicable"/> bukan sinonim <see cref="Passed"/>. Yang pertama berarti
    /// pertanyaannya memang tidak berlaku — misalnya skrining kehamilan pada pasien laki-laki;
    /// yang kedua berarti pertanyaannya berlaku dan jawabannya aman. Keduanya sama-sama
    /// meloloskan acquisition, tetapi jejak auditnya harus dapat dibedakan.
    /// </summary>
    public enum RadSafetyCheckState
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Passed")]
        Passed = 2,

        [Display(Name = "Failed")]
        Failed = 3,

        [Display(Name = "Not Applicable")]
        NotApplicable = 4
    }

    /// <summary>
    /// Sebab pengulangan study.
    ///
    /// Nilai ini menentukan siapa yang menanggung akibatnya. <c>GATE-DEC-004</c> menyatakan
    /// pengulangan karena kesalahan internal rumah sakit **tidak** menambah tagihan pasien
    /// secara otomatis, sedangkan kebutuhan klinis baru memerlukan order yang sah. Keputusan
    /// finansialnya tetap milik Billing; Radiologi hanya menyerahkan sebabnya.
    /// </summary>
    public enum RadRepeatCause
    {
        [Display(Name = "Internal Hospital Error")]
        InternalHospitalError = 1,

        [Display(Name = "Patient Condition")]
        PatientCondition = 2,

        [Display(Name = "External Cause")]
        ExternalCause = 3,

        [Display(Name = "New Clinical Requirement")]
        NewClinicalRequirement = 4
    }

    /// <summary>Objek yang berpindah status pada satu baris riwayat.</summary>
    public enum RadTransitionScope
    {
        [Display(Name = "Rad Order")]
        RadOrder = 1,

        [Display(Name = "Rad Study")]
        RadStudy = 2
    }

    /// <summary>
    /// Jenis bahan yang benar-benar terpakai pada sebuah acquisition.
    ///
    /// Dicatat sebagai fakta konsumsi, bukan sebagai nominal. <c>GATE-DEC-004</c> menuntut
    /// acquisition yang dibatalkan di tengah jalan tidak otomatis menjadi tagihan penuh maupun
    /// pembatalan penuh; yang menentukan adalah apa yang benar-benar terpakai, dan penilaiannya
    /// milik Billing.
    /// </summary>
    public enum RadConsumptionItemType
    {
        [Display(Name = "Contrast")]
        Contrast = 1,

        [Display(Name = "Material")]
        Material = 2,

        [Display(Name = "Film")]
        Film = 3,

        [Display(Name = "Medication")]
        Medication = 4,

        [Display(Name = "Other")]
        Other = 5
    }

    /// <summary>
    /// Sebab pembatalan atau penghentian acquisition di tengah jalan.
    /// </summary>
    public enum RadAbortCause
    {
        [Display(Name = "Patient Condition")]
        PatientCondition = 1,

        [Display(Name = "Patient Refusal")]
        PatientRefusal = 2,

        [Display(Name = "Equipment Failure")]
        EquipmentFailure = 3,

        [Display(Name = "Safety Concern")]
        SafetyConcern = 4,

        [Display(Name = "Internal Hospital Error")]
        InternalHospitalError = 5,

        [Display(Name = "External Cause")]
        ExternalCause = 6
    }

    /// <summary>
    /// Keadaan sebuah aturan keselamatan pada siklus pengesahannya, sesuai
    /// <c>RAD-DEC-005</c>.
    ///
    /// Hanya <see cref="Active"/> yang ikut dinilai gerbang keselamatan. Draf dan pengajuan
    /// yang belum disahkan sengaja tidak berpengaruh apa pun: aturan yang belum disetujui
    /// penanggung jawab klinis tidak boleh menentukan seorang pasien aman disinari atau tidak.
    ///
    /// Pemisahan <see cref="Draft"/> dari <see cref="Inactive"/> juga disengaja. Yang pertama
    /// berarti aturannya sedang disusun dan belum pernah berlaku; yang kedua berarti aturannya
    /// pernah berlaku lalu dihentikan. Study lama yang lolos memakai aturan yang kini
    /// <see cref="Inactive"/> tetap sah, karena versinya sudah dibekukan pada study tersebut.
    /// </summary>
    public enum RadSafetyRuleStatus
    {
        [Display(Name = "Draft")]
        Draft = 1,

        [Display(Name = "Pending Approval")]
        PendingApproval = 2,

        [Display(Name = "Active")]
        Active = 3,

        [Display(Name = "Inactive")]
        Inactive = 4
    }

    /// <summary>
    /// Siklus hidup hasil bacaan radiologi sesuai <c>RAD-STATE-001</c> bagian 3.
    ///
    /// <b>Tidak ada status terminal.</b> Bacaan yang sudah dirilis selalu dapat diamandemen,
    /// karena kekeliruan bacaan dapat ditemukan berbulan-bulan kemudian — dan menutup jalan
    /// koreksi berarti memaksa orang memperbaikinya di luar sistem.
    ///
    /// Empat status terakhir mengulang tiga status pertama untuk koreksi. Dipisahkan, bukan
    /// dipakai ulang, supaya sebuah bacaan yang sedang dikoreksi tetap dapat dibedakan dari
    /// bacaan yang belum pernah dirilis sama sekali — bagi dokter pengirim, keduanya sangat
    /// berbeda artinya.
    /// </summary>
    public enum RadReportStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Drafted")]
        Drafted = 2,

        [Display(Name = "Validated")]
        Validated = 3,

        [Display(Name = "Released")]
        Released = 4,

        [Display(Name = "Amendment Drafted")]
        AmendmentDrafted = 5,

        [Display(Name = "Amendment Validated")]
        AmendmentValidated = 6,

        [Display(Name = "Amendment Released")]
        AmendmentReleased = 7
    }

    /// <summary>
    /// Keadaan satu versi hasil bacaan sesuai <c>RAD-STATE-001</c> bagian 4.
    ///
    /// <see cref="Released"/> membekukan isinya. <see cref="Superseded"/> berarti versi ini
    /// pernah berlaku lalu digantikan — <b>isinya tetap utuh dan tidak boleh berubah satu
    /// huruf pun</b>. Mengembalikan <see cref="Superseded"/> menjadi <see cref="Released"/>
    /// tidak sah: koreksi atas koreksi membuat versi baru lagi, bukan menghidupkan versi lama.
    /// </summary>
    public enum RadReportVersionStatus
    {
        [Display(Name = "Drafted")]
        Drafted = 1,

        [Display(Name = "Validated")]
        Validated = 2,

        [Display(Name = "Released")]
        Released = 3,

        [Display(Name = "Superseded")]
        Superseded = 4
    }

    /// <summary>
    /// Peran penulis draf pada saat draf itu ditulis, sesuai <c>RAD-DEC-003</c>.
    ///
    /// <b>Nilainya dibekukan, bukan dibaca ulang dari peran pengguna saat pengesahan.</b>
    /// Seorang residen yang kemudian menjadi dokter radiolog tetap tidak boleh mengesahkan
    /// draf yang ia tulis semasa menjadi residen — yang dinilai adalah keadaan saat bacaan itu
    /// disusun, bukan jabatan hari ini.
    ///
    /// Hanya <see cref="Radiologist"/> yang boleh mengesahkan drafnya sendiri.
    /// <see cref="AiAssisted"/> tidak pernah boleh mengesahkan apa pun: pengesah wajib manusia.
    /// </summary>
    public enum RadReportAuthorRole
    {
        [Display(Name = "Radiologist")]
        Radiologist = 1,

        [Display(Name = "Resident")]
        Resident = 2,

        [Display(Name = "Radiographer")]
        Radiographer = 3,

        [Display(Name = "AI Assisted")]
        AiAssisted = 4
    }
}
