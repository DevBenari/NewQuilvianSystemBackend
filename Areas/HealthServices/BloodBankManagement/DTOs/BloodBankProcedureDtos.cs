using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>Mencatat satu tindakan Bank Darah atas satu order darah.</summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya tiga isian, dan itu disengaja.</b> Unit dan kelas pasien diambil dari kunjungan order
    /// (<c>DEC-BD-048</c>); tarif dan nominalnya dipilih backend dari data induk (<c>DEC-BD-049</c>,
    /// <c>VAL-BD-027</c>); petugas pencatat diambil dari akun yang login. Tidak ada satu field pun
    /// yang memungkinkan client menentukan harga, unit, maupun kelas.
    /// </para>
    /// </remarks>
    public class CreateBloodBankProcedureRequest
    {
        /// <summary>Order darah yang ditindak (<c>VAL-BD-026</c>).</summary>
        [Required]
        public Guid BloodOrderId { get; set; }

        /// <summary>Tindakan bertarif dari data induk <c>MstProcedure</c>.</summary>
        [Required]
        public Guid ProcedureRefId { get; set; }

        /// <summary>Dokter BDRS penanggung jawab tindakan.</summary>
        [Required]
        public Guid BdrsDoctorId { get; set; }
    }

    /// <summary>Satu baris pada daftar tindakan Bank Darah.</summary>
    public class BloodBankProcedureListDto
    {
        public Guid Id { get; set; }
        public string ProcedureNumber { get; set; } = string.Empty;

        public Guid BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public Guid ProcedureRefId { get; set; }
        public string ProcedureCodeSnapshot { get; set; } = string.Empty;
        public string ProcedureNameSnapshot { get; set; } = string.Empty;
        public decimal TariffAmountSnapshot { get; set; }

        public Guid BdrsDoctorId { get; set; }
        public string? BdrsDoctorName { get; set; }

        public BbkProcedureStatus ProcedureStatus { get; set; }
        public string ProcedureStatusLabel { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>Detail satu tindakan beserta konteks, salinan tarif, dan riwayatnya.</summary>
    public class BloodBankProcedureDetailDto
    {
        public Guid Id { get; set; }
        public string ProcedureNumber { get; set; } = string.Empty;

        public Guid BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        /// <summary>Unit dari kunjungan order (<c>DEC-BD-048</c>).</summary>
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        /// <summary>Kelas pasien dari kunjungan order (<c>DEC-BD-048</c>).</summary>
        public Guid PatientClassId { get; set; }
        public string? PatientClassName { get; set; }

        public Guid BdrsDoctorId { get; set; }
        public string? BdrsDoctorName { get; set; }
        public Guid PerformedByUserId { get; set; }

        public Guid ProcedureRefId { get; set; }

        /// <summary>Tarif yang dipilih backend (<c>DEC-BD-049</c>).</summary>
        public Guid TariffId { get; set; }

        /// <summary>Salinan beku saat dicatat — tidak mengikuti perubahan data induk (<c>AC-BD-100</c>).</summary>
        public string ProcedureCodeSnapshot { get; set; } = string.Empty;
        public string ProcedureNameSnapshot { get; set; } = string.Empty;
        public decimal TariffAmountSnapshot { get; set; }

        public BbkProcedureStatus ProcedureStatus { get; set; }
        public string ProcedureStatusLabel { get; set; } = string.Empty;

        /// <summary>Waktu dan pelaku penyelesaian, diturunkan dari riwayat <c>Complete</c>.</summary>
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }

        public List<BloodBankTransitionDto> Transitions { get; set; } = new();

        /// <summary>Aksi yang layak dicoba. <b>Kelayakan status saja</b>, bukan kewenangan.</summary>
        public List<string> AvailableActions { get; set; } = new();

        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }
}
