using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Jawaban Laboratorium atas pertanyaan Registrasi: kunjungan mana yang benar-benar
    /// dilanjutkan di laboratorium (<c>BE-EXT-05</c>, <c>LAB-DEC-054</c>).
    ///
    /// <b>Bagi Laboratorium, "dilanjutkan" berarti ada pesanan pemeriksaan yang terbit atas
    /// kunjungan itu</b> — persis kebalikan dari risiko yang dicatat <c>BR-46</c> butir 3:
    /// pasien yang menekan selesai di kiosk lalu pergi meninggalkan kunjungan
    /// <i>tanpa pemeriksaan</i>.
    ///
    /// <b>Statusnya sengaja tidak ikut disaring.</b> Pesanan yang dibatalkan, ditahan, maupun
    /// ditolak tetap membuktikan pasiennya datang dan dilayani; yang gugur adalah pemeriksaannya,
    /// bukan kedatangannya. Hanya baris terhapus yang diabaikan.
    ///
    /// <b>Berkas ini nol menulis ke data Registrasi.</b> Ia hanya membaca milik Laboratorium
    /// sendiri dan mengembalikan daftar penunjuk, sehingga <c>AC-45</c> — Laboratorium tidak
    /// membentuk maupun mengubah kunjungan — tetap tegak. Penutupannya dikerjakan Registrasi.
    /// </summary>
    public class LabEncounterContinuationProbe : IEncounterContinuationProbe
    {
        private readonly ApplicationDbContext _dbContext;

        public LabEncounterContinuationProbe(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public KioskServiceTarget TargetService => KioskServiceTarget.Laboratory;

        public async Task<IReadOnlySet<Guid>> FindContinuedEncounterIdsAsync(
            IReadOnlyCollection<Guid> encounterIds,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(encounterIds);

            if (encounterIds.Count == 0)
            {
                return new HashSet<Guid>();
            }

            var ids = encounterIds as IList<Guid> ?? encounterIds.ToList();

            var continued = await _dbContext.Set<LabOrder>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.EncounterId) && !x.IsDelete)
                .Select(x => x.EncounterId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return continued.ToHashSet();
        }
    }
}
