namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Asal data penanda tangan atau deklarer dokumen admisi — backend 13.9 (<c>BE-RWI-192</c>).
    /// Selain <see cref="Manual"/>, server membaca ulang nama, alamat, dan telepon dari service
    /// pemilik datanya dan mengabaikan isian klien.
    /// </summary>
    public enum InpAdmissionPartySource
    {
        /// <summary>Pasien sendiri, dibaca dari master pasien.</summary>
        Patient = 1,

        /// <summary>Relasi pasien yang hubungannya terstruktur.</summary>
        PatientRelationship = 2,

        /// <summary>Kontak darurat pasien; hanya dipilih petugas, tidak pernah dicocokkan teks.</summary>
        EmergencyContact = 3,

        /// <summary>Diketik petugas.</summary>
        Manual = 4
    }
}
