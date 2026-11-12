using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public interface ICompensatoryTimeOffExpirationService
    {
        /// <summary>
        /// Menghanguskan tiket CTO jalur modern yang sudah melewati masa berlaku 90 hari.
        /// </summary>
        /// <param name="actorUserId">Pengguna atau aktor sistem yang memicu eksekusi; dicatat pada mutasi buku besar.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        Task<CompensatoryExpirationRunResultDto> ExecuteExpirationRunAsync(Guid actorUserId, CancellationToken cancellationToken = default);
    }
}
