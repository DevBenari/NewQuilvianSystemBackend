using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    /// <summary>
    /// Titik perluasan bagi modul di luar Human Resource yang memakai mesin workflow bersama
    /// (`Wfl`) untuk dokumennya sendiri (BE-PBJ-001, PBJ-DEC-019).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="WorkflowReferenceLifecycleService"/> menangani jenis dokumen HR lebih dulu.
    /// Hanya bila tidak ada penangan HR yang cocok, penangan terdaftar ini ditanya satu per satu
    /// lewat <see cref="CanHandle"/>. Penangan pertama yang mengklaim jenis dokumen itu yang
    /// dipakai. Dengan urutan ini perilaku HR tidak berubah.
    /// </para>
    /// <para>
    /// Penangan bekerja dengan pola sinkronisasi yang sama dengan penangan HR: status dokumen
    /// asal diturunkan ulang dari status workflow saat ini, sehingga pemanggilan berulang tidak
    /// menghasilkan efek ganda. Contoh: modul Pengadaan mendaftarkan penangan untuk
    /// <c>ReferenceType</c> berawalan <c>PBJ_</c>; saat instance <c>PBJ_PR</c> disetujui,
    /// penangan itu mengubah permintaan pengadaan menjadi <c>APPROVED</c>.
    /// </para>
    /// <para>
    /// Penangan didaftarkan lewat DI sebagai <c>IWorkflowReferenceLifecycleHandler</c> oleh
    /// modul pemiliknya. Bila tidak ada yang terdaftar, perilaku tetap seperti sebelum
    /// perluasan ini: <c>IsHandled = false</c> tanpa efek.
    /// </para>
    /// </remarks>
    public interface IWorkflowReferenceLifecycleHandler
    {
        /// <summary>
        /// Menjawab apakah penangan ini memiliki jenis dokumen tersebut. Wajib murni dan murah:
        /// tidak menyentuh basis data.
        /// </summary>
        bool CanHandle(string referenceType);

        /// <summary>
        /// Menyelaraskan status dokumen asal dengan status workflow saat ini.
        /// </summary>
        Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(
            TrxWorkflowInstance workflow,
            Guid actorUserId,
            bool allowAutoApply,
            CancellationToken cancellationToken);
    }
}
