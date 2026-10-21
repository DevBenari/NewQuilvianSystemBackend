using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    /// <summary>
    /// Sumber rujukan masuk sebuah kunjungan (<c>RJ-REF-REV-001</c>, Zoom 13 Okt 2026).
    /// </summary>
    /// <remarks>
    /// <b>Eksternal</b>: surat dari fasilitas kesehatan lain, dicatat petugas pendaftaran.
    /// <b>Internal</b>: Surat Rujukan antar-poliklinik yang diterbitkan dokter rumah sakit ini
    /// (<c>CliDoctorCertificate</c>). Contoh: dr. Umum merujuk Tn. Budi ke Poli Penyakit Dalam;
    /// petugas cukup memilih surat itu, tanpa mengetik ulang nomor, diagnosa, atau mengunggah surat.
    /// </remarks>
    public enum ReferralSourceType
    {
        [Display(Name = "Eksternal")]
        External = 1,

        [Display(Name = "Internal")]
        Internal = 2
    }
}
