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
        /// Kunci idempotensi yang ditetapkan modul pemanggil, satu per percobaan pendaftaran.
        ///
        /// Boleh kosong, dan bila kosong tidak ada perlindungan pengiriman ulang sama sekali —
        /// karena itu pemanggil yang punya tombol Simpan <b>wajib</b> mengisinya.
        /// </summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public EncounterPaymentType PaymentType { get; set; } = EncounterPaymentType.Cash;

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
        /// Benar bila kunjungan ini <b>sudah ada sebelumnya</b> dan dikenali lewat kunci
        /// idempotensi yang sama — petugas menekan Simpan dua kali (<c>VAL-45</c>).
        ///
        /// Pemanggil memperlakukannya sebagai keberhasilan, bukan kesalahan: hasil akhirnya
        /// sama persis, yaitu satu kunjungan.
        /// </summary>
        public bool IsReplay { get; set; }
    }
}
