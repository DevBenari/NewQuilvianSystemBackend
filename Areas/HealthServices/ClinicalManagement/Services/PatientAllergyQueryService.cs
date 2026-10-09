using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Pemilik aturan "alergi aktif pasien" — <c>BE-RWI-189</c>, kontrak <c>episode-rawat-inap</c>
    /// <c>0.11.0</c> backend 13.6, <c>INT-RWA-03</c> (<c>RWI-DEC-264</c>).
    /// </summary>
    /// <remarks>
    /// Aturannya dipindahkan apa adanya dari <c>PatientAllergyController.GetActiveAlerts</c>, dan
    /// controller itu kini memanggil service ini, sehingga aturan "alergi aktif" tidak punya dua
    /// salinan. Alergi aktif = belum dihapus, aktif, alert menyala, dan status <c>Active</c>;
    /// diurutkan mengancam jiwa, risiko tinggi, keparahan, lalu nama alergen.
    /// </remarks>
    public sealed class PatientAllergyQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public PatientAllergyQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<PatientAllergyAlertResponse>> GetActiveAlertsAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<TrxPatientAllergy>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsActive &&
                    x.IsAlertEnabled &&
                    x.AllergyStatus == PatientAllergyStatus.Active)
                .OrderByDescending(x => x.IsLifeThreatening)
                .ThenByDescending(x => x.IsHighRisk)
                .ThenByDescending(x => x.Severity)
                .ThenBy(x => x.AllergenName)
                .Select(x => new PatientAllergyAlertResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    AllergyRecordNumber = x.AllergyRecordNumber,
                    AllergyCategory = x.AllergyCategory,
                    DrugId = x.DrugId,
                    AllergenCode = x.AllergenCode,
                    AllergenName = x.AllergenName,
                    AllergenGroupName = x.AllergenGroupName,
                    ReactionType = x.ReactionType,
                    ReactionDescription = x.ReactionDescription,
                    Severity = x.Severity,
                    Certainty = x.Certainty,
                    IsHighRisk = x.IsHighRisk,
                    IsLifeThreatening = x.IsLifeThreatening,
                    PatientSafetyNote = x.PatientSafetyNote
                })
                .ToListAsync(cancellationToken);
        }
    }
}
