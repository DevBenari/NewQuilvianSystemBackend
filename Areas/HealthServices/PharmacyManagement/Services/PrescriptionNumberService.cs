using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionNumberService
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateAsync(
            DateTime serviceDate,
            CancellationToken cancellationToken = default)
        {
            var prefix = $"RX-{serviceDate:yyyyMMdd}";

            var existingNumbers = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .Where(x => x.PrescriptionNumber.StartsWith(prefix))
                .Select(x => x.PrescriptionNumber)
                .ToListAsync(cancellationToken);

            var maxSequence = 0;

            foreach (var number in existingNumbers)
            {
                var lastSeparator = number.LastIndexOf('-');
                if (lastSeparator < 0 || lastSeparator == number.Length - 1)
                    continue;

                if (int.TryParse(number[(lastSeparator + 1)..], out var sequence) &&
                    sequence > maxSequence)
                {
                    maxSequence = sequence;
                }
            }

            return $"{prefix}-{maxSequence + 1:D5}";
        }
    }
}
