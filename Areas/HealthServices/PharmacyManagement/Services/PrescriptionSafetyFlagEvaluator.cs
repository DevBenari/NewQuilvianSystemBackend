using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    /// <summary>
    /// Nama penanda butir resep bermasalah — api-contract 0.6.0 bagian 12.7.
    /// </summary>
    public static class PrescriptionSafetyFlags
    {
        /// <summary>Obat bentrok dengan alergi aktif pasien.</summary>
        public const string AllergyConflict = "AllergyConflict";

        /// <summary>Obat tidak ada, nonaktif, tidak dapat diresepkan, atau tarifnya tidak dapat dihitung.</summary>
        public const string Unavailable = "Unavailable";
    }

    /// <summary>
    /// Pemeriksa ulang butir resep terhadap alergi aktif pasien dan ketersediaan obat pada saat
    /// dipakai — <c>BE-RWI-105</c>, <c>FR-DOK-090</c>, <c>VAL-DOK-57</c>, <c>RWI-DEC-122</c> butir (4)
    /// dan (5).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Alergi.</b> Butir dianggap bentrok bila pasien punya alergi berstatus <c>Active</c> yang
    /// menunjuk obat yang sama (<c>DrugId</c>), atau yang nama alergennya sama dengan nama dagang
    /// maupun nama generik obat itu. Contoh: Budi alergi "Paracetamol" → butir Paracetamol 500 mg
    /// bertanda <c>AllergyConflict</c>.
    /// </para>
    /// <para>
    /// <b>Ketersediaan.</b> Obat yang terhapus, nonaktif, atau tidak dapat diresepkan bertanda
    /// <c>Unavailable</c>.
    /// </para>
    /// <para>
    /// Pemeriksaan ini tidak menyimpan penanda apa pun: kamus data 0.5 tidak menyediakan kolomnya,
    /// sehingga penanda selalu dihitung ulang dari keadaan data terkini, baik saat template dipakai
    /// maupun saat draft resep disimpan.
    /// </para>
    /// </remarks>
    public static class PrescriptionSafetyFlagEvaluator
    {
        public static async Task<Dictionary<Guid, List<string>>> EvaluateAsync(
            ApplicationDbContext dbContext,
            Guid patientId,
            IReadOnlyCollection<Guid> drugIds,
            CancellationToken cancellationToken = default)
        {
            var hasil = drugIds.Distinct().ToDictionary(x => x, _ => new List<string>());

            if (hasil.Count == 0)
                return hasil;

            var ids = hasil.Keys.ToList();

            var drugs = await dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.DrugName, x.GenericName, x.IsActive, x.IsPrescribable, x.IsDelete })
                .ToListAsync(cancellationToken);

            var alergi = patientId == Guid.Empty
                ? new List<AllergyRow>()
                : await dbContext.Set<TrxPatientAllergy>()
                    .AsNoTracking()
                    .Where(x => x.PatientId == patientId &&
                                !x.IsDelete &&
                                x.AllergyStatus == PatientAllergyStatus.Active)
                    .Select(x => new AllergyRow(x.DrugId, x.AllergenName))
                    .ToListAsync(cancellationToken);

            var namaAlergen = alergi
                .Where(x => !string.IsNullOrWhiteSpace(x.AllergenName))
                .Select(x => x.AllergenName!.Trim().ToLowerInvariant())
                .ToHashSet();

            var drugAlergen = alergi
                .Where(x => x.DrugId.HasValue)
                .Select(x => x.DrugId!.Value)
                .ToHashSet();

            foreach (var id in ids)
            {
                var drug = drugs.FirstOrDefault(x => x.Id == id);

                if (drug == null || drug.IsDelete || !drug.IsActive || !drug.IsPrescribable)
                    hasil[id].Add(PrescriptionSafetyFlags.Unavailable);

                var bentrok = drugAlergen.Contains(id) ||
                    (drug != null && (
                        namaAlergen.Contains((drug.DrugName ?? string.Empty).Trim().ToLowerInvariant()) ||
                        (!string.IsNullOrWhiteSpace(drug.GenericName) &&
                         namaAlergen.Contains(drug.GenericName.Trim().ToLowerInvariant()))));

                if (bentrok)
                    hasil[id].Add(PrescriptionSafetyFlags.AllergyConflict);
            }

            return hasil;
        }

        private sealed record AllergyRow(Guid? DrugId, string? AllergenName);
    }
}
