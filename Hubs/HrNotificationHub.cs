using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Hubs
{
    /// <summary>
    /// Hub SignalR pusat pemberitahuan dalam aplikasi untuk modul SDM (BE-HRD-314, WF-B7).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Klien terhubung ke <c>/hubs/notifications</c> dengan token yang sama seperti hub antrean
    /// (<c>access_token</c> pada query string, lihat konfigurasi JWT di <c>Program.cs</c>). Setiap koneksi
    /// otomatis dimasukkan ke grup per akun, sehingga <c>HrdNotificationService</c> cukup mengirim ke grup
    /// <c>user:{userId}</c> dan semua tab atau perangkat milik pengguna itu menerimanya.
    /// </para>
    /// <para>
    /// Pesan yang dikirim bernama <see cref="NotificationReceivedMethod"/> dengan payload
    /// <c>HrdNotificationDto</c>. Tidak ada pesan dari klien ke server; pembacaan dan penandaan sudah dibaca
    /// tetap lewat endpoint REST supaya tercatat dengan hak akses biasa.
    /// </para>
    /// </remarks>
    [Authorize]
    public class HrNotificationHub : Hub
    {
        public const string RoutePath = "/hubs/notifications";
        public const string NotificationReceivedMethod = "notificationReceived";

        public static string UserGroupName(Guid userId) => $"user:{userId:N}";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId() ?? Guid.Empty;
            if (userId == Guid.Empty)
            {
                throw new HubException("Identitas pengguna pada token tidak valid.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupName(userId));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.GetUserId() ?? Guid.Empty;
            if (userId != Guid.Empty)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroupName(userId));
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
