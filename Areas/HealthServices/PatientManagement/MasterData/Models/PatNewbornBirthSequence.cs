using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

/// <summary>
/// Pencacah urutan kelahiran per kasus persalinan. Nilainya dinaikkan di dalam transaksi
/// pembuatan pasien bayi dan dilindungi advisory lock PostgreSQL.
/// </summary>
public sealed class PatNewbornBirthSequence : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryCaseId { get; set; }
    public int LastAllocatedBirthOrder { get; set; }
}
