using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Implementasi bawaan aman port IBillingDoctorShareResolver (BKC-DES-094).
/// Selalu mengembalikan status Unavailable tanpa akses basis data, memastikan Billing tetap
/// berjalan normal dan DoctorShare bernilai 0 sebelum modul Medical Fee memasang implementasinya.
/// </summary>
public sealed class NullBillingDoctorShareResolver : IBillingDoctorShareResolver
{
    private static readonly BillingDoctorShareResolution DefaultUnavailable =
        BillingDoctorShareResolution.Unavailable();

    public Task<BillingDoctorShareResolution> ResolveAsync(
        BillingDoctorShareQuery query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DefaultUnavailable);
    }
}

