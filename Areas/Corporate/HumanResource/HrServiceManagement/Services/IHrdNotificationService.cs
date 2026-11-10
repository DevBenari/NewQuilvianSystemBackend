using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    /// <summary>
    /// Pusat pemberitahuan dalam aplikasi untuk modul SDM (BE-HRD-314, WF-B7, INT-HR-95).
    /// </summary>
    /// <remarks>
    /// Setiap metode <c>Notify*</c> menyimpan satu baris <c>HrdNotification</c> per penerima lalu mendorongnya
    /// secara realtime lewat SignalR bila penerima sedang terhubung. Nilai kembali adalah jumlah pemberitahuan
    /// yang benar-benar dibuat; nol berarti tidak ada penerima yang cocok atau semua tertahan kunci anti-duplikat.
    /// Penerima "jabatan" tidak ditentukan dari nama peran yang ditulis di kode, melainkan dari hak akses yang
    /// diberikan admin lewat layar Akses Role (lihat <see cref="NotifyPermissionHoldersAsync"/>).
    /// </remarks>
    public interface IHrdNotificationService
    {
        /// <summary>Kirim ke satu akun.</summary>
        Task<int> NotifyUserAsync(Guid userId, HrdNotificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Kirim ke beberapa akun sekaligus.</summary>
        Task<int> NotifyUsersAsync(IEnumerable<Guid> userIds, HrdNotificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Kirim ke akun yang terhubung dengan profil tenaga kerja (<c>MstWorkforceProfile.Id</c>).</summary>
        Task<int> NotifyWorkforceProfileAsync(Guid workforceProfileId, HrdNotificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Kirim ke akun yang terhubung dengan data karyawan (<c>MstEmployee.Id</c>).</summary>
        Task<int> NotifyEmployeeAsync(Guid employeeId, HrdNotificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kirim ke semua akun aktif yang jabatan dan unitnya diberi hak akses <c>controllerName : actionName</c>
        /// pada layar Akses Role. Inilah cara memberi tahu "petugas SDM", "komite", atau "admin IT" tanpa
        /// menulis nama peran di kode.
        /// </summary>
        Task<int> NotifyPermissionHoldersAsync(string controllerName, string actionName, HrdNotificationRequest request, CancellationToken cancellationToken = default);

        Task<PagedResult<HrdNotificationDto>> GetMyNotificationsAsync(Guid userId, bool? unreadOnly, string? category, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<HrdNotificationUnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<HrdNotificationMarkReadResultDto> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

        Task<HrdNotificationMarkReadResultDto> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
