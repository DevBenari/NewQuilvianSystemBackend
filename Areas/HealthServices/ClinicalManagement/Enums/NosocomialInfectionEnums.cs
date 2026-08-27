namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Jenis infeksi terkait pelayanan kesehatan yang lazim disurveilans di rumah sakit
    /// Indonesia. Penamaan mengikuti istilah yang dipakai tim PPI, dengan padanan
    /// internasionalnya dicantumkan supaya laporan lintas standar tetap dapat dicocokkan.
    /// </summary>
    public enum NosocomialInfectionType
    {
        Unknown = 0,

        /// <summary>Infeksi daerah operasi (surgical site infection).</summary>
        SurgicalSiteInfection = 1,

        /// <summary>Infeksi saluran kemih terkait kateter (CAUTI).</summary>
        UrinaryTractInfection = 2,

        /// <summary>Infeksi aliran darah primer terkait kateter vena sentral (CLABSI).</summary>
        BloodstreamInfection = 3,

        /// <summary>Pneumonia terkait ventilator (VAP).</summary>
        VentilatorAssociatedPneumonia = 4,

        /// <summary>Pneumonia didapat di rumah sakit tanpa ventilator (HAP).</summary>
        HospitalAcquiredPneumonia = 5,

        /// <summary>Plebitis pada lokasi pemasangan infus perifer.</summary>
        Phlebitis = 6,

        /// <summary>Luka tekan atau dekubitus.</summary>
        PressureUlcer = 7,

        /// <summary>Infeksi saluran cerna, termasuk Clostridioides difficile.</summary>
        GastrointestinalInfection = 8,

        Other = 99
    }

    /// <summary>
    /// Tahap penetapan sebuah kejadian infeksi.
    /// </summary>
    /// <remarks>
    /// Dipisahkan dari jenis infeksi karena keduanya berubah pada waktu yang berbeda: jenis
    /// ditetapkan sekali saat kejadian dicurigai, sedangkan status bergerak seiring hasil
    /// pemeriksaan penunjang dan verifikasi tim PPI.
    /// </remarks>
    public enum NosocomialInfectionStatus
    {
        /// <summary>Baru dicurigai, kriteria belum lengkap.</summary>
        Suspected = 1,

        /// <summary>Kriteria terpenuhi dan sudah diverifikasi tim PPI.</summary>
        Confirmed = 2,

        /// <summary>Setelah ditelusuri ternyata bukan infeksi terkait pelayanan.</summary>
        RuledOut = 3,

        /// <summary>Infeksi terkonfirmasi yang sudah teratasi.</summary>
        Resolved = 4,

        /// <summary>Dibatalkan karena salah input.</summary>
        Cancelled = 5
    }

    /// <summary>
    /// Asal infeksi, yang menentukan apakah kejadian ini dihitung sebagai indikator mutu
    /// rumah sakit atau tidak.
    /// </summary>
    /// <remarks>
    /// Batas 48 jam adalah pembeda yang lazim dipakai: infeksi yang tanda-tandanya sudah ada
    /// saat pasien tiba dibawa dari luar, bukan didapat di rumah sakit. Menyimpannya sebagai
    /// kolom tersendiri — bukan menghitungnya ulang setiap kali laporan dibuat — membuat
    /// keputusan petugas saat itu tetap dapat ditelusuri walaupun tanggal masuk kelak
    /// dikoreksi.
    /// </remarks>
    public enum NosocomialInfectionOnsetCategory
    {
        Unknown = 0,

        /// <summary>Sudah ada atau dalam masa inkubasi saat pasien tiba.</summary>
        PresentOnAdmission = 1,

        /// <summary>Muncul lebih dari 48 jam setelah pasien dirawat.</summary>
        HealthcareAssociated = 2
    }
}
