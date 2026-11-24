namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    /// <summary>
    /// Penjaga integrasi Data Induk dengan Pemberhentian &amp; Pensiun dan skorsing (BE-HRD-401).
    /// </summary>
    public interface IWorkforceLifecycleGuardService
    {
        /// <summary>True bila pemberhentian orang ini sudah final dan tanggal efektifnya sudah lewat.</summary>
        Task<bool> IsSeparationFinalizedAsync(Guid workforceProfileId, CancellationToken cancellationToken = default);

        /// <summary>True bila akun login ini sedang dikunci karena skorsing disiplin.</summary>
        Task<bool> HasActiveSuspensionLockAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
