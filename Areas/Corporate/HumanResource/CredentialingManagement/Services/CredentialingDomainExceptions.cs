namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services
{
    /// <summary>
    /// Benturan status pada alur kredensial (mis. permohonan bukan lagi InReview, reviewer sudah menilai,
    /// atau anggota panel yang sudah menilai hendak dikeluarkan). Dipetakan controller ke <c>409 Conflict</c>.
    /// Turunan <see cref="InvalidOperationException"/> agar pemanggil lama yang menangkap tipe dasar tetap aman.
    /// </summary>
    public sealed class CredentialingConflictException : InvalidOperationException
    {
        public CredentialingConflictException(string message) : base(message)
        {
        }
    }
}
