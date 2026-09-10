using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Layanan alokasi nomor bisnis untuk modul RegistrationManagement (Patient Encounter dan Payment Source).
    /// Mengelola penomoran yang deterministik, aman terhadap konkurensi dengan pg_advisory_xact_lock pada PostgreSQL,
    /// dan menjaga kompatibilitas format backward (ENC-RSMMC-xxxxx dan EGT-RSMMC-xxxxx).
    /// </summary>
    public class PatientEncounterNumberService
    {
        public const string EncounterCodePrefix = "ENC-RSMMC-";
        public const string PaymentSourceCodePrefix = "EGT-RSMMC-";
        public const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;

        public PatientEncounterNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mengalokasikan nomor kunjungan (EncounterNumber) berikutnya dengan format ENC-RSMMC-00001 dst.
        /// </summary>
        public async Task<string> AllocateEncounterNumberAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext('REG_PATIENT_ENCOUNTER_NUMBER')::bigint)",
                    cancellationToken);
            }

            var existingCodes = await _dbContext.Set<RegPatientEncounter>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.EncounterNumber.StartsWith(EncounterCodePrefix))
                .Select(x => x.EncounterNumber)
                .ToListAsync(cancellationToken);

            var usedNumbers = existingCodes
                .Select(x => x.Replace(EncounterCodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber)) nextNumber++;

            return EncounterCodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        /// <summary>
        /// Mengalokasikan nomor sumber pembayaran (PaymentSourceNumber) berikutnya dengan format EGT-RSMMC-00001 dst.
        /// </summary>
        public async Task<string> AllocatePaymentSourceNumberAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtext('REG_PAYMENT_SOURCE_NUMBER')::bigint)",
                    cancellationToken);
            }

            var existingCodes = await _dbContext.Set<RegPatientEncounterGuarantor>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.PaymentSourceNumber.StartsWith(PaymentSourceCodePrefix))
                .Select(x => x.PaymentSourceNumber)
                .ToListAsync(cancellationToken);

            var usedNumbers = existingCodes
                .Select(x => x.Replace(PaymentSourceCodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber)) nextNumber++;

            return PaymentSourceCodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }
    }
}
