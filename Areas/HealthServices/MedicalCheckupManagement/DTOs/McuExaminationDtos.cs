using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Examination (BE-MCU-021, BE-MCU-022, BE-MCU-023)
    // =====================================================================

    public class StartMcuExaminationRequest
    {
        /// <summary><c>Version</c> order yang dibaca petugas (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    public class AddMcuComponentRequest
    {
        public McuComponentType ComponentType { get; set; }

        /// <summary>Prosedur ber-<c>IsLaboratory</c> (Laboratorium) atau ber-<c>IsRadiology</c> (Radiologi).</summary>
        public Guid? ProcedureId { get; set; }

        /// <summary>Jenis Pemeriksaan MCU; wajib untuk komponen Pemeriksaan MCU.</summary>
        public Guid? ExaminationTypeId { get; set; }

        /// <summary>Wajib untuk komponen Radiologi.</summary>
        public Guid? RadModalityId { get; set; }
    }

    public class RescheduleMcuComponentRequest
    {
        /// <summary>Tanggal susulan; sesudah hari ini dan MCU buka. Tidak memakai kuota (<c>MCU-DEC-029</c>).</summary>
        public DateOnly ScheduledDate { get; set; }

        /// <summary><c>Version</c> order yang dibaca petugas (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    public class CancelMcuComponentRequest
    {
        /// <summary>Wajib (<c>VAL-MCU-40</c>), contoh <i>Kehamilan</i>.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary><c>Version</c> order yang dibaca petugas (<c>VAL-MCU-46</c>).</summary>
        public int Version { get; set; }
    }

    public class SaveMcuExaminationResultRequest
    {
        /// <summary>Nilai per isian. Isian yang tidak dikirim tidak berubah; nilai kosong mengosongkan isian.</summary>
        public List<McuExaminationResultValueRequest> Values { get; set; } = new();

        public string? Conclusion { get; set; }

        /// <summary><c>true</c> = <i>Simpan &amp; Selesaikan</i>: isian wajib diperiksa (<c>VAL-MCU-42</c>), komponen <i>Selesai</i>.</summary>
        public bool Complete { get; set; }
    }

    public class McuExaminationResultValueRequest
    {
        public Guid ExaminationFieldId { get; set; }

        /// <summary>Angka boleh memakai koma atau titik (<c>6,4</c> dan <c>6.4</c> disimpan <c>6.4</c>); Ya/Tidak = <c>Ya</c> atau <c>Tidak</c>.</summary>
        public string? Value { get; set; }
    }

    public class McuExaminationResultResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public Guid? ExaminationTypeId { get; set; }
        public McuOrderComponentStatus ComponentStatus { get; set; }
        public string ComponentStatusLabel { get; set; } = string.Empty;

        /// <summary>Skrining awal kunjungan sudah selesai (<c>VAL-MCU-36</c>).</summary>
        public bool IsScreeningCompleted { get; set; }

        /// <summary>Alasan isian hanya dapat dibaca; <c>null</c> bila dapat diisi.</summary>
        public string? ReadOnlyReason { get; set; }

        public string? Conclusion { get; set; }
        public string? FilledByName { get; set; }
        public DateTime? FilledAt { get; set; }
        public string? PerformingDoctorName { get; set; }
        public List<McuExaminationResultFieldResponse> Fields { get; set; } = new();
    }

    public class McuExaminationResultFieldResponse
    {
        public Guid ExaminationFieldId { get; set; }
        public int FieldNumber { get; set; }
        public string FieldCode { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public McuExaminationFieldType FieldType { get; set; }
        public string FieldTypeLabel { get; set; } = string.Empty;
        public string? UnitName { get; set; }
        public List<string> Options { get; set; } = new();
        public bool IsRequired { get; set; }
        public string? Value { get; set; }
    }
}
