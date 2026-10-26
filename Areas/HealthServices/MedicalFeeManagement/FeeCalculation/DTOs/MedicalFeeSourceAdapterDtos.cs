using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    /// <summary>
    /// Item layanan hasil adaptasi dari berbagai modul sumber (Kamar Operasi, Poliklinik/Rawat Inap, Laboratorium, Billing)
    /// yang telah dinormalisasi untuk dikonsumsi oleh mesin perhitungan jasa medis (MDF-INTEGRATION-1.0 §2).
    /// </summary>
    public class AdaptedServiceItem
    {
        /// <summary>Domain sumber layanan: OPERATING_ROOM, PROCEDURE, LABORATORY, ADHOC, ADHOC_CATALOG.</summary>
        public string SourceDomain { get; set; } = string.Empty;

        /// <summary>
        /// ID unik rincian pada modul sumber.
        /// Untuk Kamar Operasi: '{OprCaseId}/{OprTeamMemberId}'.
        /// Untuk Prosedur Klinis: '{TrxPatientProcedureId}'.
        /// Untuk Laboratorium: '{LabOrderId}'.
        /// </summary>
        public string SourceDetailId { get; set; } = string.Empty;

        /// <summary>Tautan ke baris tagihan pasien (BilInvoiceItem.Id) yang sudah difinalisasi.</summary>
        public Guid? InvoiceItemId { get; set; }

        /// <summary>FK ke tarif layanan (MstTariff.Id), jika ada.</summary>
        public Guid? TariffId { get; set; }

        /// <summary>FK ke kategori tarif (MstTariffCategory.Id), jika ada.</summary>
        public Guid? TariffCategoryId { get; set; }

        /// <summary>Deskripsi layanan atau nama tindakan.</summary>
        public string ServiceDescription { get; set; } = string.Empty;

        /// <summary>Tanggal pelaksanaan layanan/tindakan.</summary>
        public DateOnly ServiceDate { get; set; }

        /// <summary>Nilai kotor baris tagihan (Quantity x UnitPrice) sebelum diskon apapun (MDF-DES-012, MF-DEC-013).</summary>
        public decimal BaseAmount { get; set; }

        // =====================================================================
        // Identitas Penerima Hak Jasa
        // =====================================================================

        /// <summary>Jenis penerima: Doctor atau Workforce (MdfPayeeTypes).</summary>
        public string PayeeType { get; set; } = MdfPayeeTypes.Doctor;

        /// <summary>
        /// ID penerima hak jasa (MstDoctor.Id atau MstWorkforceProfile.Id).
        /// Bernilai null jika pelaksana belum diinput pada modul sumber (misal LabOrder tanpa ExaminerDoctorId).
        /// </summary>
        public Guid? PayeeReferenceId { get; set; }

        /// <summary>Nama penerima hak jasa (dokter atau tenaga kerja) untuk keperluan tampilan audit.</summary>
        public string PayeeName { get; set; } = string.Empty;

        // =====================================================================
        // Peran Jasa Medis
        // =====================================================================

        /// <summary>
        /// FK ke peran jasa medis (MstMedicalFeeRole.Id).
        /// Pada kamar operasi dipetakan dari OprTeamRole; pada klinis/lab menggunakan peran utama (IsPrimaryRole = true).
        /// </summary>
        public Guid? RoleId { get; set; }

        /// <summary>Kode peran jasa medis (contoh: 'SP-BEDAH', 'ANESTESI', 'PELAKSANA-UTAMA').</summary>
        public string? RoleCode { get; set; }

        /// <summary>Nama peran jasa medis.</summary>
        public string? RoleName { get; set; }

        /// <summary>Apakah peran ini merupakan peran pelaksana utama.</summary>
        public bool IsPrimaryRole { get; set; }

        // =====================================================================
        // Status Deteksi Layanan Belum Terhitung (Unresolved)
        // =====================================================================

        /// <summary>Menandakan apakah pelaksana tindakan kosong (MDF_VAL 5.1 -> PERFORMER_MISSING).</summary>
        public bool IsPerformerMissing => !PayeeReferenceId.HasValue || PayeeReferenceId.Value == Guid.Empty;

        /// <summary>Menandakan apakah peran tidak terpetakan (MDF_VAL 5.5 -> ROLE_UNMAPPED).</summary>
        public bool IsRoleUnmapped => !RoleId.HasValue || RoleId.Value == Guid.Empty;

        /// <summary>Kode alasan jika layanan belum dapat dihitung (MdfUnresolvedReasons).</summary>
        public string? UnresolvedReason { get; set; }

        /// <summary>Penjelasan detail jika layanan belum dapat dihitung.</summary>
        public string? UnresolvedReasonDetail { get; set; }
    }
}

