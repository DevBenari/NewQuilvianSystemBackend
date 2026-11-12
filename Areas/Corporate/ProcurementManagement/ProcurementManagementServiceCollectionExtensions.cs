using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement
{
    /// <summary>
    /// Registrasi service modul Pengadaan Barang &amp; Jasa (PBJ-BP-001), dipanggil sekali dari
    /// <c>Program.cs</c>. Service tanpa interface, didaftarkan <c>AddScoped</c>, mengikuti pola
    /// modul terdekat. Task berikutnya menambah service-nya di sini, tidak di <c>Program.cs</c>.
    /// </summary>
    public static class ProcurementManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddProcurementManagement(this IServiceCollection services)
        {
            // BE-PBJ-002 — fondasi bersama seluruh sub-modul Pengadaan.
            services.AddScoped<ProcurementDocumentHistoryWriter>();
            services.AddScoped<ProcurementDataScopeService>();

            // BE-PBJ-003 — master Pengadaan (PBJ-DEC-037: satu service per master).
            services.AddScoped<ProcurementItemService>();
            services.AddScoped<ProcurementItemCategoryService>();
            services.AddScoped<ProcurementFundSourceService>();
            services.AddScoped<ProcurementTypeService>();

            // BE-PBJ-004 — lampiran. Penyedia akses per jenis dokumen (IProcurementDocumentAccessProvider)
            // didaftarkan task pemilik dokumennya; tanpa penyedia, lampiran jenis itu ditolak (fail-closed).
            services.AddScoped<ProcurementAttachmentService>();

            // BE-PBJ-005 — pintu ke mesin approval bersama (Wfl) dan penangan lifecycle dokumen PBJ_*.
            // Penerap status per jenis dokumen (IProcurementWorkflowStatusApplier) didaftarkan task pemilik dokumennya.
            services.AddScoped<ProcurementApprovalGateway>();
            services.AddScoped<IWorkflowReferenceLifecycleHandler, ProcurementWorkflowLifecycleHandler>();

            // BE-PBJ-006 — permintaan pengadaan (draf) dan penyedia akses lampiran PBJ_PR.
            services.AddScoped<ProcurementPurchaseRequestService>();
            services.AddScoped<IProcurementDocumentAccessProvider, PurchaseRequestAttachmentAccessProvider>();

            return services;
        }
    }
}
