using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Dokter perujuk — dokter <b>di luar</b> rumah sakit ini yang mengirim pasien
    /// (<c>LAB-DEC-035</c>, <c>BE-EXT-02</c>).
    ///
    /// <b>Kenapa tidak memakai data induk dokter yang sudah ada.</b> Dokter pada data induk
    /// rumah sakit adalah dokter rumah sakit ini: ia punya jadwal praktik, menerima jasa medis,
    /// dan dapat menjadi DPJP. Dokter perujuk tidak satu pun dari ketiganya. Menyatukan keduanya
    /// akan mencemari daftar dokter internal dengan nama dari luar — dan nama itu akan muncul
    /// pada pilihan DPJP, jadwal, serta perhitungan jasa medis.
    /// </summary>
    [Table("MstReferralDoctor", Schema = "public")]
    public class MstReferralDoctor : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Instansi tempat dokter ini berpraktik.</summary>
        [Required]
        public Guid ReferralInstitutionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DoctorName { get; set; } = string.Empty;

        /// <summary>
        /// Penanda aktif. Dokter yang tidak lagi merujuk dinonaktifkan, bukan dihapus —
        /// kunjungan lama yang menunjuk ke sini harus tetap dapat dibaca.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public MstReferralInstitution? ReferralInstitution { get; set; }
    }
}
