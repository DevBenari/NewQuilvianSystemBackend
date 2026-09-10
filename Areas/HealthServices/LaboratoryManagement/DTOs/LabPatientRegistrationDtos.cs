using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring pencarian pasien terdaftar, dipakai sebelum mendaftarkan kunjungan baru.
    ///
    /// Tujuannya satu: mencegah pasien yang sudah punya nomor rekam medis didaftarkan lagi
    /// sebagai pasien baru. Karena itu jalur ini <b>hanya membaca</b>.
    /// </summary>
    public class LabPatientSearchQuery
    {
        /// <summary>
        /// Pencarian bebas pada nomor rekam medis, kode pasien, nama, nomor identitas, dan
        /// nomor telepon.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>Halaman yang diminta, dimulai dari 1.</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Banyaknya baris per halaman. Dibatasi 1 sampai 50.</summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Satu baris hasil pencarian pasien.
    ///
    /// Isinya sengaja dibatasi pada yang diperlukan untuk mengenali pasien di depan loket
    /// laboratorium. Ini bukan jalur pengelolaan data pasien; modul Pasien yang memilikinya.
    /// </summary>
    public class LabPatientSearchResponse
    {
        public Guid PatientId { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }

    /// <summary>
    /// Pendaftaran pasien yang datang langsung ke laboratorium (<c>FR-08.1</c>,
    /// <c>LAB-DEC-032</c>).
    ///
    /// <b>Pasien wajib sudah terdaftar.</b> Tidak ada ruas identitas pasien baru di sini, dan
    /// ketiadaan itu disengaja: membuat data induk pasien adalah pekerjaan modul Pasien, bukan
    /// Laboratorium (<c>AC-45</c>). Petugas mencari pasiennya lewat <c>GET /patient-search</c>
    /// lebih dulu.
    /// </summary>
    public class RegisterLabWalkInRequest
    {
        public Guid PatientId { get; set; }

        /// <summary>Unit layanan laboratorium tempat pasien dilayani.</summary>
        public Guid ServiceUnitId { get; set; }

        /// <summary>
        /// Kunci idempotensi, <b>satu untuk satu percobaan pendaftaran</b> — dibuat layar saat
        /// formulir dibuka, bukan saat tombol ditekan (<c>VAL-45</c>).
        ///
        /// Bedanya menentukan. Bila kunci dibuat saat tombol ditekan, penekanan kedua membawa
        /// kunci yang berbeda dan menghasilkan kunjungan kedua — persis yang hendak dicegah.
        /// </summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        /// <summary>Diisi hanya ketika pembayarannya Tunai.</summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>Wajib ketika pembayarannya Asuransi; kartu milik pasien yang sama.</summary>
        public Guid? PatientInsuranceId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Pendaftaran pasien rujukan luar beserta instansi dan dokter perujuknya (<c>FR-08.3</c>,
    /// <c>LAB-DEC-035</c>).
    ///
    /// <b>Perhatikan apa yang tidak ada di sini:</b> tidak ada ruas nama instansi perujuk, dan
    /// tidak ada ruas nama dokter perujuk. Yang ada hanya penunjuk ke data induk. Itulah
    /// penegakan <c>AC-50</c> yang sesungguhnya — bukan penjaga yang menolak teks bebas,
    /// melainkan bentuk permintaan yang tidak menyediakan tempat untuk menuliskannya.
    /// </summary>
    public class RegisterLabExternalReferralRequest
    {
        public Guid PatientId { get; set; }

        public Guid ServiceUnitId { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        /// <summary>Nomor surat rujukan. Wajib untuk pasien rujukan (<c>VAL-44</c>).</summary>
        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        /// <summary>Penunjuk instansi perujuk, dipilih dari daftar (<c>VAL-43</c>).</summary>
        public Guid? ReferralInstitutionId { get; set; }

        /// <summary>Penunjuk dokter perujuk; wajib berpraktik pada instansi di atas.</summary>
        public Guid? ReferralDoctorId { get; set; }

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public Guid? PaymentMethodId { get; set; }

        public Guid? PatientInsuranceId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Hasil pendaftaran: penunjuk kunjungan yang <b>dibuat Registrasi</b>, beserta identitas
    /// pasien seadanya — cukup untuk langsung membuat pesanan lab pada layar berikutnya.
    /// </summary>
    public class LabRegistrationResultResponse
    {
        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public DateTime EncounterDate { get; set; }

        public string EncounterStatus { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Benar bila permintaan ini adalah pengiriman ulang dan kunjungannya <b>sudah ada</b>
        /// (<c>VAL-45</c>). Layar memperlakukannya sebagai berhasil: hasilnya sama, yaitu satu
        /// kunjungan, bukan dua.
        /// </summary>
        public bool IsReplay { get; set; }
    }
}
