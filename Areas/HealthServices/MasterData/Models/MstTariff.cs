using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstTariff", Schema = "public")]
    public class MstTariff : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TariffCode { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string TariffName { get; set; } = string.Empty;

        [Required]
        public Guid TariffCategoryId { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? PatientClassId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? DrugId { get; set; }

        /// <summary>
        /// Tarif pemakaian alat per kelas — <c>keperawatan</c> kontrak <c>0.6.0</c> kamus data 12.14
        /// (<c>BE-RWI-172</c>, migration <c>K8</c>).
        /// </summary>
        public Guid? MedicalEquipmentId { get; set; }

        /// <summary>
        /// Komponen biaya operasi yang ditagih Kamar Operasi — <c>episode-rawat-inap</c> kontrak
        /// <c>0.10.0</c> (<c>BE-RWI-172</c>, migration <c>E4</c>). Bawaan <c>None</c> untuk seluruh
        /// tarif lama, sehingga nilai tarif tindakan dan obat tidak berubah (<c>RWI-DEC-193</c>).
        /// </summary>
        public MstSurgeryComponentType SurgeryComponentType { get; set; } = MstSurgeryComponentType.None;

        /// <summary>Dasar perhitungan tarif; bawaan <c>PerService</c>.</summary>
        public MstTariffChargeBasis ChargeBasis { get; set; } = MstTariffChargeBasis.PerService;

        /// <summary>Pembulatan unit; hanya berarti bila <see cref="ChargeBasis"/> = <c>PerHour</c>.</summary>
        public MstEquipmentRoundingRule ChargeRounding { get; set; } = MstEquipmentRoundingRule.CeilingWholeUnit;

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        public bool IsSurgeryRelated { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsRegistrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsPackageTariff { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }

        public decimal NormalPrice { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTaxable { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstTariffCategory? TariffCategory { get; set; }
        public MstServiceUnit? ServiceUnit { get; set; }
        public MstClinic? Clinic { get; set; }
        public MstPatientClass? PatientClass { get; set; }
        public MstProcedure? Procedure { get; set; }
        public MstDrug? Drug { get; set; }
        public MstMedicalEquipment? MedicalEquipment { get; set; }
    }
}
