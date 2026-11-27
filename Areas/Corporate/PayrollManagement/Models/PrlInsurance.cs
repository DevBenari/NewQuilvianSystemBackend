using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    [Table("PrlInsurance", Schema = "public")]
    public class PrlInsurance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        // LEGACY_PERSISTED (PAY-BE-FIX-007): kolom BPJS di bawah (kepesertaan, nomor peserta, tarif per pegawai, program) adalah data
        // historis. Tidak dibaca mesin hitung, tidak ditampilkan, dan tidak ditulis oleh kontrak profil asuransi; dipertahankan karena
        // penghapusan kolom/indeks butuh migration. Hanya kolom asuransi swasta dan kepesertaan yang aktif.
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(200)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public decimal BpjsHealthEmployeeRate { get; set; } = 0m;
        public decimal BpjsHealthEmployerRate { get; set; } = 0m;
        public decimal BpjsEmploymentEmployeeRate { get; set; } = 0m;
        public decimal BpjsEmploymentEmployerRate { get; set; } = 0m;
        public decimal PrivateInsuranceEmployeeContribution { get; set; } = 0m;
        public decimal PrivateInsuranceEmployerContribution { get; set; } = 0m;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        // ---- Kepesertaan iuran yang ditetapkan HRD (DEC-PRL-TAX-002/005). Semua flag false BUKAN bukti tidak wajib mengikuti: status
        // harus eksplisit. Kolom nullable: kosong berarti BELUM dikonfigurasi. Kini hanya berlaku untuk asuransi swasta. ----

        /// <summary>ACTIVE, NOT_PARTICIPATING, atau NOT_APPLICABLE (InsuranceParticipationStatuses).</summary>
        [MaxLength(30)]
        public string? ParticipationStatus { get; set; }

        /// <summary>Dasar tidak mengikuti program atau tidak berlaku untuk jenis penerima; wajib selain ACTIVE.</summary>
        [MaxLength(500)]
        public string? ParticipationBasis { get; set; }

        /// <summary>LEGACY_PERSISTED: program BPJS Ketenagakerjaan historis, dipisah koma (JHT, JP, JKK, JKM). Tidak dipakai lagi.</summary>
        [MaxLength(50)]
        public string? BpjsEmploymentPrograms { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
