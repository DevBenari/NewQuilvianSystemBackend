using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;
using EmgVisit = QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models.EmgVisit;
using InpEpisode = QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models.InpEpisode;
using RegPatientEncounter = QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models.RegPatientEncounter;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>Waktu tutup klinis satu kunjungan beserta asalnya.</summary>
    /// <param name="Waktu">UTC.</param>
    /// <param name="Turunan">Benar bila diturunkan dari data lama, bukan dari baris penutupan klinis.</param>
    /// <param name="ClosureId">Id baris <see cref="MrcEncounterClinicalClosure"/>; kosong bila turunan.</param>
    public readonly record struct ClinicalClosureTime(DateTime Waktu, bool Turunan, Guid? ClosureId);

    /// <summary>
    /// Satu-satunya tempat aturan "kapan kunjungan ditutup klinis" (`RM-DEC-051`, desain A4.12).
    /// Dipakai penanda Susulan (`BE-26`) dan penilaian kelengkapan (`BE-39`), supaya keduanya
    /// tidak pernah berbeda pendapat tentang waktu yang sama.
    /// </summary>
    /// <remarks>
    /// Kunjungan yang ditutup sejak `BE-25` punya baris <see cref="MrcEncounterClinicalClosure"/>
    /// yang tidak pernah berubah — termasuk ketika rekonsiliasi IGD menggeser <c>CompletedAt</c>.
    /// Kunjungan yang lebih lama tidak punya baris itu dengan sengaja (`RM-DEC-050`); waktunya
    /// diturunkan dari data lama menurut urutan berikut, ditandai "turunan", dan TIDAK disimpan:
    /// <list type="number">
    /// <item><c>LockedEncounterClosedAt</c> paling awal pada daftar keutuhan kunjungan itu;</item>
    /// <item><c>InpEpisode.ClosedAt</c> untuk kunjungan rawat inap;</item>
    /// <item><c>EmgVisit.VisitCompletedAt</c> untuk kunjungan IGD;</item>
    /// <item><c>RegPatientEncounter.CompletedAt</c>.</item>
    /// </list>
    /// Kunjungan yang tidak punya satu pun belum ditutup.
    /// </remarks>
    public static class ClinicalClosureTimeReader
    {
        public static async Task<Dictionary<Guid, ClinicalClosureTime>> ReadAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<Guid> idKunjungan,
            CancellationToken cancellationToken)
        {
            var daftar = idKunjungan.Where(x => x != Guid.Empty).Distinct().ToList();
            var hasil = new Dictionary<Guid, ClinicalClosureTime>();

            if (daftar.Count == 0)
                return hasil;

            foreach (var baris in await dbContext.Set<MrcEncounterClinicalClosure>()
                         .AsNoTracking()
                         .Where(x => !x.IsDelete && daftar.Contains(x.EncounterId))
                         .Select(x => new { x.Id, x.EncounterId, x.ClinicalClosedAt })
                         .ToListAsync(cancellationToken))
            {
                if (!hasil.TryGetValue(baris.EncounterId, out var ada) || baris.ClinicalClosedAt < ada.Waktu)
                    hasil[baris.EncounterId] = new ClinicalClosureTime(baris.ClinicalClosedAt, false, baris.Id);
            }

            async Task TurunkanAsync(Func<List<Guid>, Task<List<(Guid Kunjungan, DateTime Waktu)>>> baca)
            {
                var sisa = daftar.Where(x => !hasil.ContainsKey(x)).ToList();
                if (sisa.Count == 0)
                    return;

                foreach (var kelompok in (await baca(sisa)).GroupBy(x => x.Kunjungan))
                    hasil[kelompok.Key] = new ClinicalClosureTime(kelompok.Min(x => x.Waktu), true, null);
            }

            await TurunkanAsync(async sisa => (await dbContext.Set<MrcClinicalDocumentIntegrity>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.LockedEncounterClosedAt != null && sisa.Contains(x.EncounterId))
                    .Select(x => new { x.EncounterId, Waktu = x.LockedEncounterClosedAt!.Value })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.EncounterId, x.Waktu))
                .ToList());

            await TurunkanAsync(async sisa => (await dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.ClosedAt != null && sisa.Contains(x.EncounterId))
                    .Select(x => new { x.EncounterId, Waktu = x.ClosedAt!.Value })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.EncounterId, x.Waktu))
                .ToList());

            await TurunkanAsync(async sisa => (await dbContext.Set<EmgVisit>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.VisitCompletedAt != null
                                && x.EncounterId != null && sisa.Contains(x.EncounterId.Value))
                    .Select(x => new { EncounterId = x.EncounterId!.Value, Waktu = x.VisitCompletedAt!.Value })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.EncounterId, x.Waktu))
                .ToList());

            await TurunkanAsync(async sisa => (await dbContext.Set<RegPatientEncounter>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.CompletedAt != null && sisa.Contains(x.Id))
                    .Select(x => new { x.Id, Waktu = x.CompletedAt!.Value })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.Id, x.Waktu))
                .ToList());

            return hasil;
        }
    }
}
