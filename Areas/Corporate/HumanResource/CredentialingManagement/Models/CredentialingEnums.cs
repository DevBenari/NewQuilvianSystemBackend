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
}
