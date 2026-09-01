using Microsoft.AspNetCore.SignalR;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Konteks SignalR tiruan yang tidak mengirim apa pun.
    ///
    /// Diperlukan karena beberapa controller menerima layanan realtime pada konstruktornya,
    /// walaupun endpoint yang sedang diuji tidak memakainya. Tanpa tiruan ini, controller itu
    /// tidak dapat dibentuk sama sekali dari uji.
    ///
    /// Sengaja tidak mencatat apa pun. Bila kelak ada uji yang perlu membuktikan pesan realtime
    /// terkirim, tiruan ini perlu diganti yang mencatat panggilannya — bukan dipakai apa adanya
    /// lalu dianggap membuktikan sesuatu.
    /// </summary>
    public sealed class HubContextKosong<THub> : IHubContext<THub> where THub : Hub
    {
        public IHubClients Clients { get; } = new KlienKosong();

        public IGroupManager Groups { get; } = new GrupKosong();

        private sealed class ProksiKosong : IClientProxy
        {
            public Task SendCoreAsync(
                string method,
                object?[] args,
                CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class KlienKosong : IHubClients
        {
            private static readonly IClientProxy Proksi = new ProksiKosong();

            public IClientProxy All => Proksi;

            public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => Proksi;

            public IClientProxy Client(string connectionId) => Proksi;

            public IClientProxy Clients(IReadOnlyList<string> connectionIds) => Proksi;

            public IClientProxy Group(string groupName) => Proksi;

            public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => Proksi;

            public IClientProxy Groups(IReadOnlyList<string> groupNames) => Proksi;

            public IClientProxy User(string userId) => Proksi;

            public IClientProxy Users(IReadOnlyList<string> userIds) => Proksi;
        }

        private sealed class GrupKosong : IGroupManager
        {
            public Task AddToGroupAsync(
                string connectionId,
                string groupName,
                CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task RemoveFromGroupAsync(
                string connectionId,
                string groupName,
                CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
