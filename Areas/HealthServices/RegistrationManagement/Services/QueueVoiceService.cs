using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    public class QueueVoiceService
    {
        private const string ConfigSection = "QueueVoice";
        private const string AudioContentType = "audio/mpeg";
        private const string Mp3Extension = ".mp3";
        private const string AudioRoute = "/api/v1/health-services/registration-management/queue-voice/audio";
        private const string DownloadRoute = "/api/v1/health-services/registration-management/queue-voice/download";

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        public QueueVoiceService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<QueueVoiceGenerateResponse> GetOrCreateQueueCallAudioAsync(
            TrxQueue queue,
            string callType,
            bool forceRegenerate = false,
            string? overrideText = null,
            string? overrideVoiceCode = null)
        {
            var normalizedCallType = NormalizeCallType(callType);
            var enabled = GetBoolean("Enabled", false);
            var voiceText = NormalizeVoiceText(overrideText) ?? BuildQueueCallText(queue, normalizedCallType);
            var voiceProfile = await ResolveVoiceProfileAsync(overrideVoiceCode);

            var result = new QueueVoiceGenerateResponse
            {
                Enabled = enabled,
                Generated = false,
                FromCache = false,
                CallType = normalizedCallType,
                VoiceCode = voiceProfile.VoiceCode,
                VoiceName = voiceProfile.VoiceName,
                Gender = voiceProfile.Gender,
                Language = voiceProfile.Language,
                LengthScale = voiceProfile.LengthScale,
                NoiseScale = voiceProfile.NoiseScale,
                NoiseW = voiceProfile.NoiseW,
                Volume = voiceProfile.Volume,
                Text = voiceText,
                ContentType = AudioContentType
            };

            if (!enabled)
            {
                result.ErrorMessage = "QueueVoice.Enabled belum aktif di appsettings.";
                return result;
            }

            try
            {
                var piperExecutablePath = ResolveExecutableOrFilePath(GetSetting("PiperExecutablePath"));
                var piperModelPath = ResolveFilePath(voiceProfile.ModelPath);
                var ffmpegExecutablePath = ResolveExecutableOrFilePath(GetSetting("FfmpegExecutablePath", "ffmpeg"));

                if (string.IsNullOrWhiteSpace(piperExecutablePath) || !CanUseExecutableOrFile(piperExecutablePath))
                {
                    result.ErrorMessage = "PiperExecutablePath belum valid. Pastikan executable Piper tersedia dan path benar.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(piperModelPath) || !File.Exists(piperModelPath))
                {
                    result.ErrorMessage = $"Model voice '{voiceProfile.VoiceCode}' belum valid atau belum tersedia di server: {voiceProfile.ModelPath}.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(ffmpegExecutablePath) || !CanUseExecutableOrFile(ffmpegExecutablePath))
                {
                    result.ErrorMessage = "FfmpegExecutablePath belum valid. Pastikan FFmpeg terinstall atau path benar.";
                    return result;
                }

                var cacheDateKey = DateTime.UtcNow.ToString("yyyyMMdd");
                var audioDirectory = ResolveCacheDirectory(cacheDateKey);
                Directory.CreateDirectory(audioDirectory);

                var safeVoiceCode = SanitizeFileToken(voiceProfile.VoiceCode);
                var textHash = BuildShortSha256Hash($"{normalizedCallType}|{safeVoiceCode}|{voiceText}|{voiceProfile.LengthScale}|{voiceProfile.NoiseScale}|{voiceProfile.NoiseW}|{voiceProfile.Volume}");
                var filePrefix = queue.Id == Guid.Empty ? "preview" : queue.Id.ToString("N");
                var fileName = $"{filePrefix}-{safeVoiceCode}-{textHash}{Mp3Extension}";
                var mp3Path = Path.Combine(audioDirectory, fileName);

                result.FileName = fileName;
                result.DateKey = cacheDateKey;
                result.AudioUrl = BuildAudioUrl(cacheDateKey, fileName);
                result.DownloadUrl = BuildDownloadUrl(cacheDateKey, fileName);

                if (!forceRegenerate && File.Exists(mp3Path) && new FileInfo(mp3Path).Length > 0)
                {
                    result.FromCache = true;
                    result.Generated = false;
                    return result;
                }

                var tempWavPath = Path.Combine(audioDirectory, $"{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid():N}.wav");
                var tempMp3Path = Path.Combine(audioDirectory, $"{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid():N}.tmp.mp3");

                try
                {
                    await GenerateWavWithPiperAsync(piperExecutablePath, piperModelPath, voiceProfile, voiceText, tempWavPath);
                    await ConvertWavToMp3Async(ffmpegExecutablePath, tempWavPath, tempMp3Path, voiceProfile.Volume);

                    if (File.Exists(mp3Path)) File.Delete(mp3Path);
                    File.Move(tempMp3Path, mp3Path);
                    result.Generated = true;
                    result.FromCache = false;
                }
                finally
                {
                    SafeDelete(tempWavPath);
                    SafeDelete(tempMp3Path);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Gagal membuat audio panggilan. {NormalizeProcessError(ex.Message, string.Empty)}";
                return result;
            }
        }

        public async Task<QueueVoiceGenerateResponse> GeneratePreviewAudioAsync(QueueVoicePreviewRequest request)
        {
            var previewQueue = new TrxQueue
            {
                Id = Guid.Empty,
                QueueCode = "A001",
                QueueNumber = 1
            };

            var text = NormalizeVoiceText(request.Text) ?? "Nomor antrean A nol nol satu. Silakan menuju ruang pemeriksaan perawat. Terima kasih.";
            return await GetOrCreateQueueCallAudioAsync(
                previewQueue,
                request.CallType ?? QueueVoiceCallTypes.Preview,
                forceRegenerate: true,
                overrideText: text,
                overrideVoiceCode: request.VoiceCode);
        }

        public async Task<List<QueueVoiceProfileResponse>> GetAvailableVoiceProfilesAsync()
        {
            var dbProfiles = await _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.VoiceName)
                .Select(x => new QueueVoiceProfileResponse
                {
                    Id = x.Id,
                    VoiceCode = x.VoiceCode,
                    VoiceName = x.VoiceName,
                    Gender = x.Gender,
                    Language = x.Language,
                    ModelPath = x.ModelPath,
                    LengthScale = x.LengthScale,
                    NoiseScale = x.NoiseScale,
                    NoiseW = x.NoiseW,
                    Volume = x.Volume,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    Source = "Database"
                })
                .ToListAsync();

            if (dbProfiles.Count > 0)
            {
                return dbProfiles;
            }

            return GetConfigVoiceProfiles()
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.VoiceName)
                .ToList();
        }

        public string? ResolveAudioPath(string dateKey, string fileName)
        {
            if (!IsSafeDateKey(dateKey) || !IsSafeAudioFileName(fileName)) return null;

            var directory = ResolveCacheDirectory(dateKey);
            var path = Path.Combine(directory, fileName);
            var root = ResolveCacheRoot();
            var fullPath = Path.GetFullPath(path);
            var fullRoot = Path.GetFullPath(root);

            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) ? fullPath : null;
        }

        public async Task<QueueVoiceCacheCleanupResponse> DeleteExpiredCacheAsync(int? retentionDays = null)
        {
            var days = retentionDays.GetValueOrDefault(GetInteger("RetentionDays", 7));
            if (days < 1) days = 1;

            var root = ResolveCacheRoot();
            Directory.CreateDirectory(root);

            var threshold = DateTime.UtcNow.AddDays(-days);
            var deletedCount = 0;
            long deletedBytes = 0;

            foreach (var file in Directory.EnumerateFiles(root, "*.mp3", SearchOption.AllDirectories))
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.LastWriteTimeUtc >= threshold) continue;

                    deletedBytes += info.Length;
                    info.Delete();
                    deletedCount++;
                }
                catch
                {
                    // Abaikan file terkunci agar cleanup tidak mengganggu proses panggilan.
                }
            }

            await Task.CompletedTask;

            return new QueueVoiceCacheCleanupResponse
            {
                CacheFolder = root,
                RetentionDays = days,
                DeletedFileCount = deletedCount,
                DeletedBytes = deletedBytes,
                ExecutedAtUtc = DateTime.UtcNow,
                Message = deletedCount == 0
                    ? "Tidak ada file audio panggilan yang perlu dibersihkan."
                    : $"Berhasil menghapus {deletedCount} file audio panggilan lama."
            };
        }

        public bool IsSafeAudioFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) &&
                   Path.GetFileName(fileName) == fileName &&
                   fileName.EndsWith(Mp3Extension, StringComparison.OrdinalIgnoreCase) &&
                   Regex.IsMatch(fileName, "^[a-zA-Z0-9][a-zA-Z0-9_-]{2,120}-[a-zA-Z0-9_-]+-[a-fA-F0-9]{16}\\.mp3$");
        }

        public bool IsSafeDateKey(string dateKey)
        {
            return !string.IsNullOrWhiteSpace(dateKey) && Regex.IsMatch(dateKey, "^[0-9]{8}$");
        }

        private async Task<QueueVoiceProfileResponse> ResolveVoiceProfileAsync(string? requestedVoiceCode)
        {
            var sanitizedRequestedCode = string.IsNullOrWhiteSpace(requestedVoiceCode)
                ? null
                : SanitizeFileToken(requestedVoiceCode);

            var defaultVoiceCode = SanitizeFileToken(GetSetting("DefaultVoiceCode", "id_ID_default"));
            var targetVoiceCode = sanitizedRequestedCode ?? defaultVoiceCode;

            var dbProfile = await _dbContext.Set<MstQueueVoiceProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .Where(x => x.VoiceCode == targetVoiceCode)
                .Select(x => new QueueVoiceProfileResponse
                {
                    Id = x.Id,
                    VoiceCode = x.VoiceCode,
                    VoiceName = x.VoiceName,
                    Gender = x.Gender,
                    Language = x.Language,
                    ModelPath = x.ModelPath,
                    LengthScale = x.LengthScale,
                    NoiseScale = x.NoiseScale,
                    NoiseW = x.NoiseW,
                    Volume = x.Volume,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    Source = "Database"
                })
                .FirstOrDefaultAsync();

            if (dbProfile != null)
            {
                return dbProfile;
            }

            if (string.IsNullOrWhiteSpace(sanitizedRequestedCode))
            {
                dbProfile = await _dbContext.Set<MstQueueVoiceProfile>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.IsDefault)
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new QueueVoiceProfileResponse
                    {
                        Id = x.Id,
                        VoiceCode = x.VoiceCode,
                        VoiceName = x.VoiceName,
                        Gender = x.Gender,
                        Language = x.Language,
                        ModelPath = x.ModelPath,
                        LengthScale = x.LengthScale,
                        NoiseScale = x.NoiseScale,
                        NoiseW = x.NoiseW,
                        Volume = x.Volume,
                        IsDefault = x.IsDefault,
                        IsActive = x.IsActive,
                        SortOrder = x.SortOrder,
                        Description = x.Description,
                        Source = "Database"
                    })
                    .FirstOrDefaultAsync();

                if (dbProfile != null)
                {
                    return dbProfile;
                }
            }

            var configProfiles = GetConfigVoiceProfiles();
            return configProfiles.FirstOrDefault(x => x.VoiceCode == targetVoiceCode && x.IsActive)
                ?? configProfiles.FirstOrDefault(x => x.IsDefault && x.IsActive)
                ?? new QueueVoiceProfileResponse
                {
                    VoiceCode = defaultVoiceCode,
                    VoiceName = "Default Voice",
                    Gender = "Unknown",
                    Language = "id-ID",
                    ModelPath = GetSetting("PiperModelPath", "Storage/PiperVoices/id_ID/default.onnx"),
                    LengthScale = 1.08m,
                    NoiseScale = 0.65m,
                    NoiseW = 0.80m,
                    Volume = 1.15m,
                    IsDefault = true,
                    IsActive = true,
                    Source = "Fallback"
                };
        }

        private List<QueueVoiceProfileResponse> GetConfigVoiceProfiles()
        {
            var profiles = new List<QueueVoiceProfileResponse>();
            var defaultVoiceCode = SanitizeFileToken(GetSetting("DefaultVoiceCode", "id_ID_default"));
            var section = _configuration.GetSection($"{ConfigSection}:Voices");

            foreach (var child in section.GetChildren())
            {
                var voiceCode = SanitizeFileToken(child["VoiceCode"] ?? string.Empty);
                if (string.IsNullOrWhiteSpace(voiceCode)) continue;

                profiles.Add(new QueueVoiceProfileResponse
                {
                    VoiceCode = voiceCode,
                    VoiceName = CleanVoiceText(child["DisplayName"]) ?? voiceCode,
                    Gender = CleanVoiceText(child["Gender"]) ?? "Unknown",
                    Language = CleanVoiceText(child["Language"]) ?? "id-ID",
                    ModelPath = child["ModelPath"] ?? string.Empty,
                    LengthScale = ClampDecimal(ParseDecimal(child["LengthScale"], 1.08m), 0.70m, 1.50m),
                    NoiseScale = ClampDecimal(ParseDecimal(child["NoiseScale"], 0.65m), 0.10m, 1.50m),
                    NoiseW = ClampDecimal(ParseDecimal(child["NoiseW"], 0.80m), 0.10m, 1.50m),
                    Volume = ClampDecimal(ParseDecimal(child["Volume"], 1.15m), 0.50m, 2.00m),
                    IsDefault = string.Equals(voiceCode, defaultVoiceCode, StringComparison.OrdinalIgnoreCase),
                    IsActive = true,
                    SortOrder = profiles.Count,
                    Source = "Configuration"
                });
            }

            if (profiles.Count == 0)
            {
                profiles.Add(new QueueVoiceProfileResponse
                {
                    VoiceCode = defaultVoiceCode,
                    VoiceName = "Default Voice",
                    Gender = "Unknown",
                    Language = "id-ID",
                    ModelPath = GetSetting("PiperModelPath", "Storage/PiperVoices/id_ID/default.onnx"),
                    LengthScale = 1.08m,
                    NoiseScale = 0.65m,
                    NoiseW = 0.80m,
                    Volume = 1.15m,
                    IsDefault = true,
                    IsActive = true,
                    SortOrder = 0,
                    Source = "LegacyConfiguration"
                });
            }

            if (!profiles.Any(x => x.IsDefault))
            {
                profiles[0].IsDefault = true;
            }

            return profiles;
        }

        private string BuildQueueCallText(TrxQueue queue, string callType)
        {
            var defaultTemplate = callType switch
            {
                QueueVoiceCallTypes.Nurse => "Nomor antrean {queueCode}. Atas nama {patientName}. Silakan menuju ruang pemeriksaan perawat. Terima kasih.",
                QueueVoiceCallTypes.Doctor => "Nomor antrean {queueCode}. Atas nama {patientName}. Silakan menuju {clinicName}. {doctorName} telah siap melayani Anda. Terima kasih.",
                QueueVoiceCallTypes.Display => "Nomor antrean {queueCode}. Silakan menuju {serviceUnitName}. Terima kasih.",
                _ => "Nomor antrean {queueCode}. Atas nama {patientName}. Silakan menuju {serviceUnitName}. Terima kasih."
            };

            var templateKey = callType switch
            {
                QueueVoiceCallTypes.Nurse => "NurseCallTemplate",
                QueueVoiceCallTypes.Doctor => "DoctorCallTemplate",
                QueueVoiceCallTypes.Display => "DisplayCallTemplate",
                _ => "CallTemplate"
            };

            var template = GetSetting(templateKey, GetSetting("CallTemplate", defaultTemplate));
            var patientName = NormalizeNameForSpeech(queue.Patient?.FullName) ?? "pasien";
            var queueCode = BuildVoiceQueueCode(queue.QueueCode) ?? BuildVoiceQueueNumber(queue.QueueNumber);
            var clinicName = NormalizeMedicalTerm(queue.Clinic?.ClinicName) ?? "poli tujuan";
            var doctorName = NormalizeDoctorName(queue.Doctor?.FullName) ?? "dokter";
            var serviceUnitName = NormalizeMedicalTerm(queue.ServiceUnit?.ServiceUnitName) ?? "unit layanan";

            return NormalizeVoiceText(template
                .Replace("{queueCode}", queueCode, StringComparison.OrdinalIgnoreCase)
                .Replace("{queueNumber}", NumberToIndonesianWords(queue.QueueNumber), StringComparison.OrdinalIgnoreCase)
                .Replace("{patientName}", patientName, StringComparison.OrdinalIgnoreCase)
                .Replace("{medicalRecordNumber}", SpeakDigits(queue.Patient?.MedicalRecordNumber ?? string.Empty), StringComparison.OrdinalIgnoreCase)
                .Replace("{clinicName}", clinicName, StringComparison.OrdinalIgnoreCase)
                .Replace("{doctorName}", doctorName, StringComparison.OrdinalIgnoreCase)
                .Replace("{serviceUnitName}", serviceUnitName, StringComparison.OrdinalIgnoreCase)) ?? defaultTemplate;
        }

        private async Task GenerateWavWithPiperAsync(
            string piperExecutablePath,
            string piperModelPath,
            QueueVoiceProfileResponse voiceProfile,
            string text,
            string outputWavPath)
        {
            var timeoutSeconds = GetInteger("ProcessTimeoutSeconds", 30);
            var args = new StringBuilder();
            args.Append("-m ").Append(QuoteArgument(piperModelPath));
            args.Append(" -f ").Append(QuoteArgument(outputWavPath));
            args.Append(" --length_scale ").Append(ToInvariantDecimal(voiceProfile.LengthScale));
            args.Append(" --noise_scale ").Append(ToInvariantDecimal(voiceProfile.NoiseScale));
            args.Append(" --noise_w ").Append(ToInvariantDecimal(voiceProfile.NoiseW));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = piperExecutablePath,
                    Arguments = args.ToString(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.StandardInput.WriteLineAsync(text);
            process.StandardInput.Close();

            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();
            var exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));
            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            if (!exited)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new InvalidOperationException("Proses Piper timeout saat membuat WAV.");
            }

            if (process.ExitCode != 0 || !File.Exists(outputWavPath) || new FileInfo(outputWavPath).Length == 0)
            {
                throw new InvalidOperationException($"Piper gagal membuat WAV. {NormalizeProcessError(stdErr, stdOut)}");
            }
        }

        private async Task ConvertWavToMp3Async(string ffmpegExecutablePath, string wavPath, string mp3Path, decimal volume)
        {
            var timeoutSeconds = GetInteger("ProcessTimeoutSeconds", 30);
            var bitrate = SanitizeBitrate(GetSetting("Mp3Bitrate", "128k"));
            var safeVolume = ClampDecimal(volume, 0.50m, 2.00m);
            var audioFilter = $"volume={ToInvariantDecimal(safeVolume)},acompressor=threshold=-18dB:ratio=2:attack=20:release=250";

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegExecutablePath,
                    Arguments = $"-y -i {QuoteArgument(wavPath)} -af {QuoteArgument(audioFilter)} -codec:a libmp3lame -b:a {QuoteArgument(bitrate)} {QuoteArgument(mp3Path)}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();
            var exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));
            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            if (!exited)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new InvalidOperationException("Proses FFmpeg timeout saat mengubah WAV ke MP3.");
            }

            if (process.ExitCode != 0 || !File.Exists(mp3Path) || new FileInfo(mp3Path).Length == 0)
            {
                throw new InvalidOperationException($"FFmpeg gagal mengubah WAV ke MP3. {NormalizeProcessError(stdErr, stdOut)}");
            }
        }

        private string ResolveCacheDirectory(string dateKey)
        {
            return Path.Combine(ResolveCacheRoot(), dateKey[..4], dateKey.Substring(4, 2), dateKey.Substring(6, 2));
        }

        private string ResolveCacheRoot()
        {
            var configuredFolder = GetSetting("CacheFolder", "Storage/QueueVoiceCache");
            return ResolveDirectoryPath(configuredFolder);
        }

        private string ResolveDirectoryPath(string path)
        {
            if (Path.IsPathRooted(path)) return Path.GetFullPath(path);
            return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, path));
        }

        private string ResolveFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (Path.IsPathRooted(path)) return Path.GetFullPath(path);
            return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, path));
        }

        private string ResolveExecutableOrFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (Path.IsPathRooted(path)) return Path.GetFullPath(path);

            var localPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, path));
            return File.Exists(localPath) ? localPath : path;
        }

        private static bool CanUseExecutableOrFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (File.Exists(path)) return true;
            return !Path.IsPathRooted(path) && !path.Contains(Path.DirectorySeparatorChar) && !path.Contains(Path.AltDirectorySeparatorChar);
        }

        private string BuildAudioUrl(string dateKey, string fileName)
        {
            var pathBase = _httpContextAccessor.HttpContext?.Request.PathBase.Value ?? string.Empty;
            return $"{pathBase}{AudioRoute}/{Uri.EscapeDataString(dateKey)}/{Uri.EscapeDataString(fileName)}";
        }

        private string BuildDownloadUrl(string dateKey, string fileName)
        {
            var pathBase = _httpContextAccessor.HttpContext?.Request.PathBase.Value ?? string.Empty;
            return $"{pathBase}{DownloadRoute}/{Uri.EscapeDataString(dateKey)}/{Uri.EscapeDataString(fileName)}";
        }

        private string GetSetting(string key, string fallback = "")
        {
            var value = _configuration[$"{ConfigSection}:{key}"];
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private bool GetBoolean(string key, bool fallback)
        {
            var value = _configuration[$"{ConfigSection}:{key}"];
            return bool.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private int GetInteger(string key, int fallback)
        {
            var value = _configuration[$"{ConfigSection}:{key}"];
            return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
        }

        private static string NormalizeCallType(string? value)
        {
            var normalized = (value ?? QueueVoiceCallTypes.General).Trim().ToLowerInvariant();
            return normalized switch
            {
                QueueVoiceCallTypes.Nurse => QueueVoiceCallTypes.Nurse,
                QueueVoiceCallTypes.Doctor => QueueVoiceCallTypes.Doctor,
                QueueVoiceCallTypes.Display => QueueVoiceCallTypes.Display,
                QueueVoiceCallTypes.Preview => QueueVoiceCallTypes.Preview,
                _ => QueueVoiceCallTypes.General
            };
        }

        private static string? NormalizeVoiceText(string? value)
        {
            var text = CleanVoiceText(value);
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = NormalizeMedicalTerm(text) ?? text;
            text = Regex.Replace(text, @"\s*\.\s*", ". ");
            text = Regex.Replace(text, @"\s*,\s*", ", ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length > 600 ? text[..600] : text;
        }

        private static string? CleanVoiceText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = Regex.Replace(value, @"[\u0000-\u001F\u007F]+", " ");
            text = Regex.Replace(text, @"<[^>]*>", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string? NormalizeNameForSpeech(string? value)
        {
            var text = CleanVoiceText(value);
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = Regex.Replace(text, @"\bTn\.?\b", "Tuan", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bNy\.?\b", "Nyonya", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bNn\.?\b", "Nona", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bAn\.?\b", "Anak", RegexOptions.IgnoreCase);
            return text.Trim();
        }

        private static string? NormalizeDoctorName(string? value)
        {
            var text = CleanVoiceText(value);
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = Regex.Replace(text, @"\bdrg\.?\b", "dokter gigi", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bdr\.?\b", "dokter", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bSp\.?\s*A\b", "Spesialis Anak", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bSp\.?\s*PD\b", "Spesialis Penyakit Dalam", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bSp\.?\s*OG\b", "Spesialis Obstetri dan Ginekologi", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bSp\.?\s*M\b", "Spesialis Mata", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bSp\.?\s*THT\b", "Spesialis T H T", RegexOptions.IgnoreCase);
            return NormalizeMedicalTerm(text) ?? text;
        }

        private static string? NormalizeMedicalTerm(string? value)
        {
            var text = CleanVoiceText(value);
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = Regex.Replace(text, @"\bantrian\b", "antrean", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bIGD\b", "Instalasi Gawat Darurat", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bICU\b", "I C U", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bNICU\b", "N I C U", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bPICU\b", "P I C U", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bTHT\b", "T H T", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bBPJS\b", "B P J S", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bRS\b", "Rumah Sakit", RegexOptions.IgnoreCase);
            return text.Trim();
        }

        private static string? BuildVoiceQueueCode(string? queueCode)
        {
            var cleanCode = CleanVoiceText(queueCode);
            if (string.IsNullOrWhiteSpace(cleanCode)) return null;

            var parts = cleanCode
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (parts.Count >= 2)
            {
                var lastPart = parts[^1];
                if (Regex.IsMatch(lastPart, @"^\d{1,6}$"))
                {
                    var prefixParts = parts.Take(parts.Count - 1).Where(x => !Regex.IsMatch(x, @"^\d{8}$")).ToList();
                    var prefix = NormalizeMedicalTerm(string.Join(" ", prefixParts));
                    var spokenNumber = SpeakDigits(lastPart);
                    if (!string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(spokenNumber))
                    {
                        return $"{prefix} {spokenNumber}";
                    }
                }
            }

            var compactMatch = Regex.Match(cleanCode, @"^([a-zA-Z]+)[\s-]?(\d{1,6})$");
            if (compactMatch.Success)
            {
                var prefix = compactMatch.Groups[1].Value.ToUpperInvariant();
                var number = compactMatch.Groups[2].Value;
                return $"{SpeakLetters(prefix)} {SpeakDigits(number)}";
            }

            return cleanCode;
        }

        private static string BuildVoiceQueueNumber(int queueNumber)
        {
            if (queueNumber <= 0) return "nomor antrean";
            return $"nomor {NumberToIndonesianWords(queueNumber)}";
        }

        private static string SpeakLetters(string value)
        {
            var letters = Regex.Replace(value ?? string.Empty, @"[^a-zA-Z]", string.Empty).ToUpperInvariant();
            return string.Join(" ", letters.Select(x => x.ToString()));
        }

        private static string SpeakDigits(string value)
        {
            var digits = Regex.Replace(value ?? string.Empty, @"\D", string.Empty);
            if (string.IsNullOrWhiteSpace(digits)) return string.Empty;

            var words = digits.Select(digit => digit switch
            {
                '0' => "nol",
                '1' => "satu",
                '2' => "dua",
                '3' => "tiga",
                '4' => "empat",
                '5' => "lima",
                '6' => "enam",
                '7' => "tujuh",
                '8' => "delapan",
                '9' => "sembilan",
                _ => string.Empty
            });

            return string.Join(" ", words.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string NumberToIndonesianWords(int number)
        {
            if (number == 0) return "nol";
            if (number < 0) return "minus " + NumberToIndonesianWords(Math.Abs(number));

            string[] units = { "", "satu", "dua", "tiga", "empat", "lima", "enam", "tujuh", "delapan", "sembilan", "sepuluh", "sebelas" };

            if (number < 12) return units[number];
            if (number < 20) return units[number - 10] + " belas";
            if (number < 100) return units[number / 10] + " puluh" + (number % 10 == 0 ? "" : " " + units[number % 10]);
            if (number < 200) return "seratus" + (number % 100 == 0 ? "" : " " + NumberToIndonesianWords(number % 100));
            if (number < 1000) return units[number / 100] + " ratus" + (number % 100 == 0 ? "" : " " + NumberToIndonesianWords(number % 100));
            if (number < 2000) return "seribu" + (number % 1000 == 0 ? "" : " " + NumberToIndonesianWords(number % 1000));
            if (number < 1000000) return NumberToIndonesianWords(number / 1000) + " ribu" + (number % 1000 == 0 ? "" : " " + NumberToIndonesianWords(number % 1000));

            return SpeakDigits(number.ToString(CultureInfo.InvariantCulture));
        }

        private static string BuildShortSha256Hash(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes).ToLowerInvariant()[..16];
        }

        private static string SanitizeFileToken(string value)
        {
            var token = Regex.Replace(value ?? string.Empty, @"[^a-zA-Z0-9_-]", "_").Trim('_');
            return string.IsNullOrWhiteSpace(token) ? "voice" : token;
        }

        private static string SanitizeBitrate(string value)
        {
            var bitrate = Regex.Replace(value ?? "128k", @"[^0-9kKmM]", string.Empty);
            return string.IsNullOrWhiteSpace(bitrate) ? "128k" : bitrate;
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static string NormalizeProcessError(string stdErr, string stdOut)
        {
            var text = string.IsNullOrWhiteSpace(stdErr) ? stdOut : stdErr;
            text = Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
            return string.IsNullOrWhiteSpace(text) ? "Tidak ada detail error dari proses TTS." : text;
        }

        private static decimal ParseDecimal(string? value, decimal fallback)
        {
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)) return parsed;
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed)) return parsed;
            return fallback;
        }

        private static decimal ClampDecimal(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static string ToInvariantDecimal(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // no-op
            }
        }
    }
}
