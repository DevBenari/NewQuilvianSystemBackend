using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu tindakan Bank Darah atas satu order darah, beserta salinan tarifnya.
    /// Aggregate root <c>BD-AGG-05</c> / <c>BD-DOM-12</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Biaya berasal dari tindakan, bukan dari kantong</b> (<c>DEC-BD-021</c>). Tiga kantong yang
    /// diberikan dalam satu tindakan tetap satu tindakan.
    /// </para>
    ///
    /// <para>
    /// <b>Unit dan kelas pasien bukan isian</b> (<c>DEC-BD-048</c>). Keduanya disalin dari kunjungan
    /// order saat tindakan dicatat, supaya tarif yang terpilih selalu tarif untuk pasien yang
    /// sebenarnya.
    /// </para>
    ///
    /// <para>
    /// <b>Tarif tidak pernah dihitung di sini</b> (<c>VAL-BD-027</c>, <c>DEC-BD-049</c>). Backend memilih
    /// satu baris <c>MstTariff</c>, lalu menyalin kode dan nama tindakan beserta nominal tarifnya.
    /// Salinan itu dibekukan: perubahan data induk sesudahnya tidak mengubah tindakan yang sudah
    /// tercatat (<c>AC-BD-100</c>), mengikuti pola <c>BD-CAP-008</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Tidak ada kolom penagihan.</b> Tabel ini tidak menyimpan status kirim, nomor tagihan,
    /// maupun rujukan Billing — penyalurannya milik <c>BE-BD-013</c> (<c>AC-BD-102</c>).
    /// </para>
    /// </remarks>
    [Table("BbkBloodBankProcedure", Schema = "public")]
    public class BbkBloodBankProcedure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor bisnis tindakan, unik. Diterbitkan <c>NumberSeriesAllocator</c> pada deret
        /// <c>BBK_PROCEDURE</c>.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ProcedureNumber { get; set; } = string.Empty;

        /// <summary>Order darah yang ditindak.</summary>
        [Required]
        public Guid BloodOrderId { get; set; }

        [ForeignKey(nameof(BloodOrderId))]
        public BbkBloodOrder? BloodOrder { get; set; }

        /// <summary>Unit pelayanan — disalin dari kunjungan order (<c>DEC-BD-048</c>).</summary>
        [Required]
        public Guid ServiceUnitId { get; set; }

        [ForeignKey(nameof(ServiceUnitId))]
        public MstServiceUnit? ServiceUnit { get; set; }

        /// <summary>Dokter BDRS penanggung jawab tindakan.</summary>
        [Required]
        public Guid BdrsDoctorId { get; set; }

        [ForeignKey(nameof(BdrsDoctorId))]
        public MstDoctor? BdrsDoctor { get; set; }

        /// <summary>Petugas pencatat. Diturunkan dari pengguna terautentikasi.</summary>
        [Required]
        public Guid PerformedByUserId { get; set; }

        /// <summary>Kelas pasien — disalin dari kunjungan order (<c>DEC-BD-048</c>).</summary>
        [Required]
        public Guid PatientClassId { get; set; }

        [ForeignKey(nameof(PatientClassId))]
        public MstPatientClass? PatientClass { get; set; }

        /// <summary>Tindakan bertarif dari data induk <c>MstProcedure</c>.</summary>
        [Required]
        public Guid ProcedureRefId { get; set; }

        [ForeignKey(nameof(ProcedureRefId))]
        public MstProcedure? ProcedureRef { get; set; }

        /// <summary>Tarif yang dipilih backend (<c>DEC-BD-049</c>).</summary>
        [Required]
        public Guid TariffId { get; set; }

        [ForeignKey(nameof(TariffId))]
        public MstTariff? Tariff { get; set; }

        /// <summary>Salinan kode tindakan saat dicatat.</summary>
        [Required]
        [MaxLength(50)]
        public string ProcedureCodeSnapshot { get; set; } = string.Empty;

        /// <summary>Salinan nama tindakan saat dicatat.</summary>
        [Required]
        [MaxLength(200)]
        public string ProcedureNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Salinan nominal tarif saat dicatat. <b>Bukan tagihan</b> — keputusan menagih milik Billing.
        /// </summary>
        public decimal TariffAmountSnapshot { get; set; }

        /// <summary>
        /// Status tindakan. Dipetakan sebagai concurrency token, sehingga dua penyelesaian serentak
        /// tidak sama-sama tersimpan.
        /// </summary>
        public BbkProcedureStatus ProcedureStatus { get; set; } = BbkProcedureStatus.Recorded;
    }
}
