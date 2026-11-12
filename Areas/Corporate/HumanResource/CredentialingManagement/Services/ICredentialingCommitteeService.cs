using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    public interface ICredentialingCommitteeService
    {
        /// <summary>
        /// Mengambil seluruh data 3 komite kredensial mandiri rumah sakit (Medik, Keperawatan, Nakes Lain) (HRD-DEC-147)
        /// </summary>
        Task<List<CredentialingCommitteeDto>> GetAllCommitteesAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian spesifik satu komite kredensial beserta struktur pejabatnya
        /// </summary>
        Task<CredentialingCommitteeDto> GetCommitteeByCodeAsync(
            string committeeCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil katalog buku putih (whitebook) kompetensi klinis berdasarkan rumpun komite pemohon
        /// </summary>
        Task<List<ClinicalPrivilegeCatalogItemDto>> GetWhitebookCatalogAsync(
            string? committeeCode = null,
            Guid? professionId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Memastikan ketersediaan master 3 komite awal faskes (auto-seeding jika database kosong)
        /// </summary>
        Task EnsureDefaultCommitteesExistAsync(
            Guid systemUserId,
            CancellationToken cancellationToken = default);
    }
}
