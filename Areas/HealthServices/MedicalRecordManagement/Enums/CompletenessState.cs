namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Keadaan kelengkapan berkas satu kunjungan (`RM-DEC-045`, `RM-DEC-046`). Urutan prioritas
    /// keadaan utama: <see cref="Undetermined"/> → <see cref="AwaitingResult"/> →
    /// <see cref="Incomplete"/> → <see cref="Complete"/>.
    /// </summary>
    public enum CompletenessState
    {
        Complete = 1,
        Incomplete = 2,
        AwaitingResult = 3,
        Undetermined = 4
    }
}
