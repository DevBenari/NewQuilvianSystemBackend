namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan tenggat satu pengkajian — <c>BE-RWI-058</c>, <c>FR-KEP-010</c>, <c>FR-KEP-011</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><c>NotMonitored</c> bukan sama dengan <c>OnTime</c>, dan bukan pula <c>Late</c>.</b>
    /// Ia menyatakan bahwa rumah sakit memang belum menetapkan batas waktunya, sehingga
    /// pertanyaan "terlambat atau tidak" belum punya jawaban. Menyamakannya dengan tepat waktu
    /// akan menyembunyikan kenyataan bahwa kepatuhan pengkajian sama sekali belum dipantau —
    /// <c>VAL-KEP-17</c>, <c>FR-KEP-025</c>.
    /// </para>
    /// <para>
    /// Nilainya dipersistensi <b>tidak</b> di tabel mana pun; ia dihitung saat dibaca dari
    /// <c>DueAt</c> yang sudah tersimpan pada pengkajian. Karena <c>DueAt</c> distempel memakai
    /// kebijakan yang berlaku saat pengkajian dibuat, mengubah kebijakan hari ini tidak pernah
    /// mengubah penilaian pengkajian yang lalu — <c>AC-CAP012-04</c>.
    /// </para>
    /// </remarks>
    public enum AssessmentDueState
    {
        /// <summary>Belum ada kebijakan batas waktu yang berlaku, sehingga tidak dipantau.</summary>
        NotMonitored = 0,

        /// <summary>Sudah selesai, dan selesainya tidak melewati tenggat.</summary>
        OnTime = 1,

        /// <summary>Belum selesai, dan tenggatnya belum lewat.</summary>
        Pending = 2,

        /// <summary>Melewati tenggat: selesai terlambat, atau belum selesai sampai tenggat lewat.</summary>
        Late = 3
    }
}
