using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using System.Diagnostics;
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

        public QueueVoiceService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
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
            var voiceCode = SanitizeFileToken(overrideVoiceCode ?? GetSetting("DefaultVoiceCode", "id_ID_default"));

            var result = new QueueVoiceGenerateResponse
            {
                Enabled = enabled,
                Generated = false,
                FromCache = false,
                CallType = normalizedCallType,
                VoiceCode = voiceCode,
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
                var piperModelPath = ResolveFilePath(GetSetting("PiperModelPath"));
                var ffmpegExecutablePath = ResolveExecutableOrFilePath(GetSetting("FfmpegExecutablePath", "ffmpeg"));

                if (string.IsNullOrWhiteSpace(piperExecutablePath) || !CanUseExecutableOrFile(piperExecutablePath))
                {
                    result.ErrorMessage = "PiperExecutablePath belum valid. Pastikan executable Piper sudah tersedia dan path sudah benar.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(piperModelPath) || !File.Exists(piperModelPath))
                {
                    result.ErrorMessage = "PiperModelPath belum valid. Pastikan model Piper voice Indonesia sudah tersedia dan path sudah benar.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(ffmpegExecutablePath) || !CanUseExecutableOrFile(ffmpegExecutablePath))
                {
                    result.ErrorMessage = "FfmpegExecutablePath belum valid. Pastikan FFmpeg sudah terinstall atau path sudah benar.";
                    return result;
                }

                var cacheDateKey = DateTime.UtcNow.ToString("yyyyMMdd");
                var audioDirectory = ResolveCacheDirectory(cacheDateKey);
                Directory.CreateDirectory(audioDirectory);

                var textHash = BuildShortSha256Hash($"{normalizedCallType}|{voiceCode}|{voiceText}");
                var filePrefix = queue.Id == Guid.Empty ? "preview" : queue.Id.ToString("N");
                var fileName = $"{filePrefix}-{voiceCode}-{textHash}{Mp3Extension}";
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
                    await GenerateWavWithPiperAsync(piperExecutablePath, piperModelPath, voiceText, tempWavPath);
                    await ConvertWavToMp3Async(ffmpegExecutablePath, tempWavPath, tempMp3Path);

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
                QueueCode = "A001"
            };

            var text = NormalizeVoiceText(request.Text) ?? "Nomor antrean A nol nol satu, silakan menuju nurse station.";
            return await GetOrCreateQueueCallAudioAsync(
                previewQueue,
                request.CallType ?? QueueVoiceCallTypes.Preview,
                forceRegenerate: true,
                overrideText: text,
                overrideVoiceCode: request.VoiceCode);
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
                    // Abaikan file yang sedang terkunci agar job cleanup tidak menggagalkan proses lain.
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

        private string BuildQueueCallText(TrxQueue queue, string callType)
        {
            var defaultTemplate = "Nomor antrian untuk, {queueCode}, atas nama {patientName}, silakan menuju ruang perawat.";
            var templateKey = callType switch
            {
                QueueVoiceCallTypes.Nurse => "NurseCallTemplate",
                QueueVoiceCallTypes.Doctor => "DoctorCallTemplate",
                QueueVoiceCallTypes.Display => "DisplayCallTemplate",
                _ => "CallTemplate"
            };

            var template = GetSetting(templateKey, GetSetting("CallTemplate", defaultTemplate));

            var patientName = CleanVoiceText(queue.Patient?.FullName) ?? "pasien";

            // QueueCode khusus suara dibuat pendek.
            // Contoh:
            // INTERNA-20260625-003 => INTERNA nol nol tiga
            // UMUM-20260625-001 => UMUM nol nol satu
            var queueCode = BuildVoiceQueueCode(queue.QueueCode)
                ?? BuildVoiceQueueNumber(queue.QueueNumber);

            var clinicName = CleanVoiceText(queue.Clinic?.ClinicName) ?? "poli tujuan";
            var doctorName = CleanVoiceText(queue.Doctor?.FullName) ?? "dokter";
            var serviceUnitName = CleanVoiceText(queue.ServiceUnit?.ServiceUnitName) ?? "unit layanan";

            return template
                .Replace("{queueCode}", queueCode, StringComparison.OrdinalIgnoreCase)
                .Replace("{queueNumber}", queue.QueueNumber.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{patientName}", patientName, StringComparison.OrdinalIgnoreCase)
                .Replace("{medicalRecordNumber}", CleanVoiceText(queue.Patient?.MedicalRecordNumber) ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("{clinicName}", clinicName, StringComparison.OrdinalIgnoreCase)
                .Replace("{doctorName}", doctorName, StringComparison.OrdinalIgnoreCase)
                .Replace("{serviceUnitName}", serviceUnitName, StringComparison.OrdinalIgnoreCase);
        }

        private async Task GenerateWavWithPiperAsync(string piperExecutablePath, string piperModelPath, string text, string outputWavPath)
        {
            var timeoutSeconds = GetInteger("ProcessTimeoutSeconds", 30);
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = piperExecutablePath,
                    Arguments = $"-m {QuoteArgument(piperModelPath)} -f {QuoteArgument(outputWavPath)}",
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

        private async Task ConvertWavToMp3Async(string ffmpegExecutablePath, string wavPath, string mp3Path)
        {
            var timeoutSeconds = GetInteger("ProcessTimeoutSeconds", 30);
            var bitrate = GetSetting("Mp3Bitrate", "128k");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegExecutablePath,
                    Arguments = $"-y -i {QuoteArgument(wavPath)} -codec:a libmp3lame -b:a {QuoteArgument(bitrate)} {QuoteArgument(mp3Path)}",
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
            return text.Length > 600 ? text[..600] : text;
        }

        private static string? CleanVoiceText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = Regex.Replace(value, @"[\u0000-\u001F\u007F]+", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string? BuildVoiceQueueCode(string? queueCode)
        {
            var cleanCode = CleanVoiceText(queueCode);
            if (string.IsNullOrWhiteSpace(cleanCode))
            {
                return null;
            }

            var parts = cleanCode
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            // Format utama:
            // INTERNA-20260625-003
            // UMUM-20260625-001
            // MATA-20260625-008
            if (parts.Count >= 2)
            {
                var lastPart = parts[^1];

                if (Regex.IsMatch(lastPart, @"^\d{1,6}$"))
                {
                    var prefixParts = parts
                        .Take(parts.Count - 1)
                        .Where(x => !Regex.IsMatch(x, @"^\d{8}$"))
                        .ToList();

                    var prefix = CleanVoiceText(string.Join(" ", prefixParts));
                    var spokenNumber = SpeakDigits(lastPart);

                    if (!string.IsNullOrWhiteSpace(prefix) &&
                        !string.IsNullOrWhiteSpace(spokenNumber))
                    {
                        return $"{prefix} {spokenNumber}";
                    }
                }
            }

            // Fallback untuk format seperti A001.
            var compactMatch = Regex.Match(cleanCode, @"^([a-zA-Z]+)[\s-]?(\d{1,6})$");
            if (compactMatch.Success)
            {
                var prefix = compactMatch.Groups[1].Value;
                var number = compactMatch.Groups[2].Value;

                return $"{prefix} {SpeakDigits(number)}";
            }

            return cleanCode;
        }

        private static string BuildVoiceQueueNumber(int queueNumber)
        {
            if (queueNumber <= 0)
            {
                return "nomor antrian";
            }

            return $"nomor {SpeakDigits(queueNumber.ToString("000"))}";
        }

        private static string SpeakDigits(string value)
        {
            var digits = Regex.Replace(value ?? string.Empty, @"\D", string.Empty);

            if (string.IsNullOrWhiteSpace(digits))
            {
                return string.Empty;
            }

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
