using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed class AssetDailyStatusWorkerOptions
{
    public const string SectionName = "AssetManagement:DailyStatusWorker";

    public bool Enabled { get; set; }

    public string RunAtLocalTime { get; set; } = "00:10";

    public Guid? SystemActorUserId { get; set; }
}

public sealed class AssetDailyStatusWorker : BackgroundService
{
    private static readonly TimeSpan DefaultRunAtLocalTime = new(0, 10, 0);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<AssetDailyStatusWorkerOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssetDailyStatusWorker> _logger;

    public AssetDailyStatusWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<AssetDailyStatusWorkerOptions> options,
        IConfiguration configuration,
        ILogger<AssetDailyStatusWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            _logger.LogInformation("Pekerja harian status pemanfaatan aset dinonaktifkan melalui konfigurasi.");
            return;
        }

        var role = (_configuration["Runtime:Role"] ?? "All").Trim();
        if (!role.Equals("All", StringComparison.OrdinalIgnoreCase) && !role.Equals("Worker", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Pekerja harian status pemanfaatan aset tidak dijalankan pada Runtime:Role {Role}.", role);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_options.CurrentValue.Enabled)
            {
                await RunOnceAsync(stoppingToken);
            }

            try
            {
                await Task.Delay(DelayUntilNextRun(ResolveRunAtLocalTime(_options.CurrentValue.RunAtLocalTime)), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AssetUtilizationService>();
            var today = AssetRegisterSettingReader.Today();
            var result = await service.RunDailyStatusAsync(today, _options.CurrentValue.SystemActorUserId, stoppingToken);
            _logger.LogInformation(
                "Pekerja harian status pemanfaatan aset selesai untuk {Today}: {Due} jatuh tempo, {Started} mulai berjalan, {Ended} berakhir.",
                today,
                result.Due,
                result.Started,
                result.Ended);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Pekerja harian status pemanfaatan aset mengalami error.");
        }
    }

    private static TimeSpan ResolveRunAtLocalTime(string? value) =>
        TimeSpan.TryParseExact(value?.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var parsed) && parsed >= TimeSpan.Zero && parsed < TimeSpan.FromDays(1)
            ? parsed
            : DefaultRunAtLocalTime;

    private static TimeSpan DelayUntilNextRun(TimeSpan runAtLocalTime)
    {
        var localNow = AppDateTimeHelper.LocalNow();
        var next = localNow.Date.Add(runAtLocalTime);
        if (next <= localNow)
        {
            next = next.AddDays(1);
        }

        return next - localNow;
    }
}
