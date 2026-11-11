namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public interface IDocumentAutoNumberingService
    {
        /// <summary>
        /// Mengalokasikan nomor urut surat resmi berikutnya secara atomik dengan row-locking anti-lompat (HRS-B3, VR-HRS-10).
        /// </summary>
        /// <param name="documentTypeId">ID tipe dokumen resmi</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Nomor surat resmi berformat lengkap, misal: SKK/RSQ-SDM/XI/2026/00012</returns>
        Task<string> GenerateOfficialDocumentNumberAsync(Guid documentTypeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Menghasilkan format nomor tiket bantuan helpdesk SDM berformat TKT-YYYYMM-XXXXX (HRS-W1)
        /// </summary>
        Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);
    }
}
