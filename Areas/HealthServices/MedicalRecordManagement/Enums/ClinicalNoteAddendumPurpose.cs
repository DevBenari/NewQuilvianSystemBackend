namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Maksud satu addendum (`RM-DEC-049`). <see cref="Attestation"/> menyatakan isi catatan
    /// <c>LockedUnsigned</c> benar tanpa mengubah statusnya; addendum lama seluruhnya
    /// <see cref="Correction"/>.
    /// </summary>
    public enum ClinicalNoteAddendumPurpose
    {
        Correction = 1,
        Attestation = 2
    }
}
