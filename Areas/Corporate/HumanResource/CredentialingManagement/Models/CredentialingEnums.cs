namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    public enum ClinicalCommitteeType
    {
        MedicalCommittee = 1,       // Komite Medik (Dokter Spesialis & Dokter Umum)
        NursingCommittee = 2,       // Komite Keperawatan (Perawat & Bidan)
        HealthProfessionalCommittee = 3 // Komite Tenaga Kesehatan Lainnya (Apoteker, Radiografer, ATLM, Fisioterapis, Gizi)
    }

    public enum SupervisionLevel
    {
        Level1_Independent = 1,     // Mandiri sepenuhnya (Full Clinical Privilege)
        Level2_UnderSupervision = 2, // Di bawah supervisi / Co-signature dalam 24 jam
        Level3_Proctored = 3,       // Di bawah bimbingan langsung (Proctoring/Mentoring)
        Level4_NotPermitted = 4     // Tidak diizinkan melakukan tindakan klinis
    }

    public enum PrivilegeNature
    {
        Regular = 1,                // Kewenangan Klinis Biasa (Siklus 3 Tahun)
        Provisional = 2,            // Kewenangan Klinis Sementara (Masa Uji Coba / Tamu)
        Emergency = 3               // Kewenangan Klinis Darurat (Life-Saving Emergency / Bencana)
    }

    public enum LicenseTypeCategory
    {
        STR = 1,                    // Surat Tanda Registrasi (Dapat Seumur Hidup sesuai UU Kes 2023)
        SIP = 2                     // Surat Izin Praktik (Wajib 5 Tahun per Faskes, Maksimal 3 Tempat)
    }

    public enum ExpiryRadarLevel
    {
        Normal = 0,                 // > 90 hari sebelum kedaluwarsa
        H90 = 1,                    // 61 - 90 hari (Peringatan Awal ESS)
        H60 = 2,                    // 31 - 60 hari (Peringatan Kedua Unit & Nakes)
        H30 = 3,                    // 8 - 30 hari (Peringatan Kritis Komite)
        H7 = 4,                     // 1 - 7 hari (Peringatan Darurat Direktur)
        ExpiredH0 = 5               // 0 hari / Lewat (Fail-Closed Block Order SIMRS & Penjadwalan)
    }

    /// <summary>
    /// FIN-HRD-09-BE-001: Adapter semantik tunggal untuk SupervisionLevel.
    /// Mencegah ambiguitas penomoran level antara legacy numerik dan semantik rumah sakit.
    /// </summary>
    public static class SupervisionLevelHelper
    {
        public const string SemanticIndependent = "INDEPENDENT";
        public const string SemanticSupervised = "SUPERVISED";
        public const string SemanticProctored = "PROCTORED";
        public const string SemanticDenied = "DENIED";

        public const string CodeIndependent = "Level1_Independent";
        public const string CodeSupervised = "Level2_UnderSupervision";
        public const string CodeProctored = "Level3_Proctored";
        public const string CodeDenied = "Level4_NotPermitted";

        public static string ToSemanticCode(this SupervisionLevel level) => level switch
        {
            SupervisionLevel.Level1_Independent => SemanticIndependent,
            SupervisionLevel.Level2_UnderSupervision => SemanticSupervised,
            SupervisionLevel.Level3_Proctored => SemanticProctored,
            SupervisionLevel.Level4_NotPermitted => SemanticDenied,
            _ => SemanticDenied
        };

        public static string ToIndonesianLabel(this SupervisionLevel level) => level switch
        {
            SupervisionLevel.Level1_Independent => "Mandiri",
            SupervisionLevel.Level2_UnderSupervision => "Supervisi",
            SupervisionLevel.Level3_Proctored => "Bimbingan Langsung (Proctored)",
            SupervisionLevel.Level4_NotPermitted => "Tidak Diizinkan",
            _ => "Tidak Diketahui"
        };

        public static SupervisionLevel FromSemanticCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return SupervisionLevel.Level4_NotPermitted;

            var upper = code.Trim().ToUpperInvariant();
            return upper switch
            {
                SemanticIndependent or "LEVEL1_INDEPENDENT" or "MANDIRI" or "1" => SupervisionLevel.Level1_Independent,
                SemanticSupervised or "LEVEL2_UNDERSUPERVISION" or "SUPERVISI" or "2" => SupervisionLevel.Level2_UnderSupervision,
                SemanticProctored or "LEVEL3_PROCTORED" or "PROCTORED" or "BIMBINGAN" or "3" => SupervisionLevel.Level3_Proctored,
                SemanticDenied or "LEVEL4_NOTPERMITTED" or "TIDAK DIIZINKAN" or "DITOLAK" or "4" => SupervisionLevel.Level4_NotPermitted,
                _ => SupervisionLevel.Level4_NotPermitted
            };
        }
    }
}
