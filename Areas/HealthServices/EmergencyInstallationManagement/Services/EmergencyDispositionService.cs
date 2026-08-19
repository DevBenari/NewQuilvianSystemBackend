using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi keputusan akhir pelayanan IGD dan transisi Draft-Confirmed-Executed.
    /// </summary>
    public class EmergencyDispositionService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmergencyDispositionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyDispositionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.DispositionTypeId == Guid.Empty)
                return "DispositionTypeId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyDispositionStatus), request.DispositionStatus))
                return "Nilai DispositionStatus tidak valid.";

            if (request.IsPatientDeceased && !request.DeathDateTime.HasValue)
                return "DeathDateTime wajib diisi ketika pasien dinyatakan meninggal.";

            var visit = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.EmergencyVisitId && !x.IsDelete,
                    cancellationToken);

            if (visit == null)
                return "EmergencyVisitId tidak ditemukan.";

            if (visit.VisitStatus is EmergencyVisitStatus.Disposed or EmergencyVisitStatus.Cancelled)
                return "Kunjungan IGD sudah ditutup dan tidak dapat menerima disposition baru.";

            var dispositionType = await _dbContext.Set<MstEmergencyDispositionType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.DispositionTypeId && !x.IsDelete && x.IsActive,
                    cancellationToken);

            if (dispositionType == null)
                return "DispositionTypeId tidak ditemukan atau tidak aktif.";

            if (dispositionType.RequiresDestinationServiceUnit &&
                (!request.DestinationServiceUnitId.HasValue || request.DestinationServiceUnitId.Value == Guid.Empty))
                return "DestinationServiceUnitId wajib diisi untuk jenis disposition yang dipilih.";

            if (dispositionType.RequiresReferralFacility && string.IsNullOrWhiteSpace(request.DestinationFacilityName))
                return "DestinationFacilityName wajib diisi untuk jenis disposition rujukan.";

            if (request.DestinationServiceUnitId.HasValue &&
                request.DestinationServiceUnitId.Value != Guid.Empty &&
                !await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.DestinationServiceUnitId.Value && !x.IsDelete, cancellationToken))
                return "DestinationServiceUnitId tidak ditemukan.";

            if (request.DispositionStatus == EmergencyDispositionStatus.Executed)
            {
                var setting = await _dbContext.Set<MstEmergencySetting>()
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderByDescending(x => x.IsDefault)
                    .ThenByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);

                if (setting?.RequireRegistrationCompletionBeforeDisposition == true &&
                    visit.RegistrationStatus != EmergencyRegistrationStatus.Completed)
                    return "Registrasi IGD harus diselesaikan sebelum disposition dieksekusi.";
            }

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyDispositionRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyDispositionRequest)request, cancellationToken);

        /// <summary>
        /// Memeriksa closure gate penyelesaian kunjungan. Mengembalikan pesan penolakan bila
        /// ada syarat yang belum tuntas, atau null bila kunjungan boleh diselesaikan.
        ///
        /// Sesuai IGD-DEC-021, status billing sengaja <b>tidak</b> diperiksa. Tagihan yang
        /// belum final tidak membuat pasien dianggap masih aktif secara klinis.
        /// </summary>
        public async Task<string?> ValidateVisitClosureAsync(
            TrxEmergencyVisit visit,
            CancellationToken cancellationToken = default)
        {
            if (visit.VisitStatus != EmergencyVisitStatus.Disposed)
                return "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan.";

            var adaObservasiAktif = await _dbContext.Set<TrxEmergencyObservation>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.EmergencyVisitId == visit.Id
                        && !x.IsDelete
                        && x.ObservationStatus == EmergencyObservationStatus.Active,
                    cancellationToken);

            if (adaObservasiAktif)
                return "Masih ada observasi yang belum diselesaikan.";

            // Transfer dianggap tuntas hanya bila sudah Completed atau Rejected. Cancelled
            // termasuk belum tuntas menurut validation matrix bagian 3, jadi tidak ikut
            // dikecualikan di sini.
            var adaTransferBelumTuntas = await _dbContext.Set<TrxEmergencyTransfer>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.EmergencyVisitId == visit.Id
                        && !x.IsDelete
                        && x.TransferStatus != EmergencyTransferStatus.Completed
                        && x.TransferStatus != EmergencyTransferStatus.Rejected,
                    cancellationToken);

            if (adaTransferBelumTuntas)
                return "Masih ada proses perpindahan yang belum selesai.";

            return null;
        }

        public bool CanTransition(
            EmergencyDispositionStatus current,
            EmergencyDispositionStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyDispositionStatus.Draft => target is EmergencyDispositionStatus.Confirmed
                    or EmergencyDispositionStatus.Cancelled,
                EmergencyDispositionStatus.Confirmed => target is EmergencyDispositionStatus.Executed
                    or EmergencyDispositionStatus.Cancelled,
                _ => false
            };
        }
    }
}
