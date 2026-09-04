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
}
