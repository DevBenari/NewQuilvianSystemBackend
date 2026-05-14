using DPUruNet;
using DPConstants = DPUruNet.Constants;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Text;

namespace QuilvianSystemBackend.Services.Fingerprint
{
    public class DigitalPersonaFingerprintIdentificationService : IFingerprintIdentificationService
    {
        private const int DefaultMatchThreshold = 21474;

        private readonly ApplicationDbContext _dbContext;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public DigitalPersonaFingerprintIdentificationService(
            ApplicationDbContext dbContext,
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _protector = dataProtectionProvider.CreateProtector("Quilvian.Fingerprint.Template.v1");
            _configuration = configuration;
            _loggerService = loggerService;
        }

        public async Task<FingerprintIdentifyResult> IdentifyAsync(
            byte[] capturedSample,
            int? sampleFormat,
            string? deviceId,
            CancellationToken cancellationToken = default)
        {
            if (capturedSample.Length == 0)
            {
                return FingerprintIdentifyResult.NotMatched("Captured fingerprint kosong.");
            }

            var capturedFmd = TryReadFmd(capturedSample);

            if (capturedFmd == null)
            {
                return FingerprintIdentifyResult.NotMatched(
                    "Captured fingerprint belum berbentuk FMD XML. Pastikan frontend/register mengirim template FMD, bukan raw image/sample."
                );
            }

            var threshold = _configuration.GetValue<int?>("Fingerprint:MatchThreshold")
                ?? DefaultMatchThreshold;

            var credentials = await _dbContext.ApplicationUserFingerprintCredentials
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.RegisteredAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.TemplateDataEncrypted,
                    x.TemplateFormat,
                    x.FingerPosition,
                    x.DeviceId
                })
                .ToListAsync(cancellationToken);

            if (credentials.Count == 0)
            {
                return FingerprintIdentifyResult.NotMatched("Belum ada fingerprint aktif yang terdaftar.");
            }

            Guid? bestUserId = null;
            Guid? bestCredentialId = null;
            int? bestScore = null;

            foreach (var credential in credentials)
            {
                byte[] storedTemplateBytes;

                try
                {
                    storedTemplateBytes = _protector.Unprotect(credential.TemplateDataEncrypted);
                }
                catch
                {
                    await _loggerService.WarningAsync(
                        "Fingerprint",
                        "Identify",
                        "Gagal decrypt stored fingerprint template.",
                        new
                        {
                            FingerprintCredentialId = credential.Id,
                            credential.UserId
                        }
                    );

                    continue;
                }

                var storedFmd = TryReadFmd(storedTemplateBytes);

                if (storedFmd == null)
                {
                    continue;
                }

                CompareResult compareResult;

                try
                {
                    compareResult = Comparison.Compare(
                        capturedFmd,
                        0,
                        storedFmd,
                        0
                    );
                }
                catch
                {
                    continue;
                }

                if (compareResult.ResultCode != DPConstants.ResultCode.DP_SUCCESS)
                {
                    continue;
                }

                var score = compareResult.Score;

                if (score > threshold)
                {
                    continue;
                }

                if (!bestScore.HasValue || score < bestScore.Value)
                {
                    bestScore = score;
                    bestUserId = credential.UserId;
                    bestCredentialId = credential.Id;
                }
            }

            if (!bestUserId.HasValue || !bestCredentialId.HasValue || !bestScore.HasValue)
            {
                return FingerprintIdentifyResult.NotMatched("Fingerprint tidak dikenali.");
            }

            return FingerprintIdentifyResult.Matched(
                bestUserId.Value,
                bestCredentialId.Value,
                bestScore.Value
            );
        }

        private static Fmd? TryReadFmd(byte[] data)
        {
            try
            {
                var text = Encoding.UTF8.GetString(data);

                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                if (!text.Contains("Fmd", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("<FMD", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return Fmd.DeserializeXml(text);
            }
            catch
            {
                return null;
            }
        }
    }
}
