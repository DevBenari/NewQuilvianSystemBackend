using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Permintaan pendaftaran kunjungan yang datang dari modul lain (<c>INT-05</c>).
    ///
    /// <b>Ini bukan DTO endpoint HTTP.</b> Ia kontrak pemanggilan dalam proses antara modul
    /// pemakai — saat ini Laboratorium (<c>BE-LAB-08</c>) — dengan Registrasi sebagai pemilik
    /// tabel kunjungan. Modul pemakai mengisi formulirnya, Registrasi yang membuat
    /// kunjungannya. Batas itulah yang dijaga <c>AC-45</c>.
    ///
    /// Bentuk ruasnya mengikuti tabel pemetaan pada
    /// <c>contracts/integration-contract.md</c> bagian 2b.
    /// </summary>
    public class EncounterIntakeRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        /// <summary>
        /// Jenis kunjungan. Bawaan <c>Outpatient</c> adalah kunjungan penunjang tanpa antrean —
        /// perilaku pemanggil lama (Laboratorium, Radiologi) dan tidak berubah.
        ///
        /// <c>MedicalCheckup</c> (<c>INT-MCU-02</c>, <c>BE-MCU-001</c>) membuat kunjungan di klinik
        /// MCU; antrean, skrining perawat, dan kebutuhan dokternya mengikuti master klinik, sama
        /// seperti jalur loket. Jenis lain ditolak.
        /// </summary>
        public EncounterType EncounterType { get; set; } = EncounterType.Outpatient;

        /// <summary>
        /// Wajib untuk <c>MedicalCheckup</c>: klinik bertipe Medical Check-Up pada
        /// <c>ServiceUnitId</c> yang sama. Tidak berlaku untuk <c>Outpatient</c>.
        /// </summary>
        public Guid? ClinicId { get; set; }

        /// <summary>
        /// Dokter MCU pemeriksa. Wajib bila klinik MCU mewajibkan dokter, karena nurse station
        /// menolak menyelesaikan skrining antrean yang belum punya dokter tujuan. Tidak berlaku
        /// untuk <c>Outpatient</c>.
        /// </summary>
        public Guid? DoctorId { get; set; }

        /// <summary>
        /// Kunci idempotensi yang ditetapkan modul pemanggil, satu per percobaan pendaftaran.
        ///
        /// Boleh kosong, dan bila kosong tidak ada perlindungan pengiriman ulang sama sekali —
        /// karena itu pemanggil yang punya tombol Simpan <b>wajib</b> mengisinya.
        /// </summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

        public int Priority { get; set; } = 1;

        public bool IsPrimary { get; set; } = true;

        /// <summary>Diisi hanya ketika <c>PaymentType</c> bernilai <c>Cash</c>.</summary>
        public Guid? PaymentMethodId { get; set; }

        /// <summary>
        /// Wajib ketika <c>PaymentType</c> bernilai <c>Insurance</c>, dan wajib merupakan
        /// kartu milik <c>PatientId</c> yang sama.
        /// </summary>
        public Guid? PatientInsuranceId { get; set; }

        public bool IsReferral { get; set; } = false;

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        /// <summary>
        /// Penunjuk instansi perujuk. <b>Penunjuk, bukan nama</b> — tidak ada ruas teks bebas
        /// untuk nama instansi di mana pun pada kontrak ini, dan ketiadaan itulah yang
        /// menegakkan <c>AC-50</c> beserta <c>VAL-43</c>.
        /// </summary>
        public Guid? ReferralInstitutionId { get; set; }

        /// <summary>Penunjuk dokter perujuk; wajib berpraktik pada instansi di atas.</summary>
        public Guid? ReferralDoctorId { get; set; }

        /// <summary>
        /// <c>DEC-FRJ-001</c>: wajibkan instansi mitra layak (aktif, mitra, perjanjian berlaku hari
        /// ini). Diisi pemanggil dari alur Rawat Jalan; bawaan <c>false</c> menjaga perilaku lama.
        /// </summary>
        public bool RequirePartnerEligibility { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Jawaban Registrasi atas satu permintaan pendaftaran (<c>INT-05</c>).
    ///
    /// Isinya sengaja sedikit: cukup untuk membuat pesanan lab pada layar berikutnya, dan tidak
    /// lebih. Identitas pasien selengkapnya dibaca sendiri oleh modul pemanggil dari data induk
    /// pasien, karena pembacaan itu memang haknya (<c>CAP-09</c>) dan menyalinnya ke sini hanya
    /// akan menambah jalur yang harus dijaga tetap selaras.
    /// </summary>
    public class EncounterIntakeResult
    {
        public Guid EncounterId { get; set; }

        public string EncounterNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public DateTime EncounterDate { get; set; }

        public EncounterStatus EncounterStatus { get; set; }

        /// <summary>
        /// Antrean yang dibuat bersama kunjungan. Kosong untuk kunjungan penunjang, dan untuk
        /// klinik yang tidak mewajibkan antrean.
        /// </summary>
        public Guid? QueueId { get; set; }

        public string? QueueCode { get; set; }

        /// <summary>
        /// Benar bila kunjungan ini <b>sudah ada sebelumnya</b> dan dikenali lewat kunci
        /// idempotensi yang sama — petugas menekan Simpan dua kali (<c>VAL-45</c>).
        ///
        /// Pemanggil memperlakukannya sebagai keberhasilan, bukan kesalahan: hasil akhirnya
        /// sama persis, yaitu satu kunjungan.
        /// </summary>
        public bool IsReplay { get; set; }
    }
}
