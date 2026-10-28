namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>Jenis satu kekurangan berkas kunjungan (`DC-RM-08`).</summary>
    public enum CompletenessDeficiencyType
    {
        UnsignedDocument = 1,
        PendingResult = 2,
        MissingRequiredDocument = 3,
        SourceUnavailable = 4,
        RuleNotFound = 5
    }
}
