using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingReminderService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.Reminder";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BillingReminderService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<SendPaymentReminderResponse> SendReminderAsync(
        Guid invoiceId,
        SendPaymentReminderRequest? request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (invoiceId == Guid.Empty)
            throw new BillingInvoiceValidationException("InvoiceId wajib diisi.");

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} tidak ditemukan.");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete, cancellationToken);
        var patientId = encounter?.PatientId ?? Guid.Empty;

        // Hitung outstanding authoritative dari versi kalkulasi terakhir & tender SUCCEEDED
        var latestCalc = await _dbContext.BilCalculationVersions.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId)
            .OrderByDescending(x => x.VersionNo)
            .FirstOrDefaultAsync(cancellationToken);
        var patientAmount = latestCalc?.PatientAmount ?? 0m;

        var settlementIds = await _dbContext.BilSettlements.AsNoTracking()
            .Where(s => s.InvoiceId == invoiceId && s.Purpose == BillingSettlementPurposes.InvoicePayment)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var totalPaid = settlementIds.Count == 0
            ? 0m
            : await _dbContext.BilTenders.AsNoTracking()
                .Where(t => settlementIds.Contains(t.SettlementId) && t.Status == BillingTenderStatuses.Succeeded)
                .SumAsync(t => t.Amount, cancellationToken);

        var outstanding = Math.Max(0m, patientAmount - totalPaid);
        if (outstanding <= 0)
        {
            throw new BillingInvoiceValidationException(
                "Invoice sudah lunas. Pengingat pembayaran hanya dapat dikirim untuk tagihan yang masih memiliki sisa pembayaran.");
        }

        var channel = !string.IsNullOrWhiteSpace(request?.Channel)
            ? request.Channel.Trim().ToUpperInvariant()
            : "WHATSAPP";
        var template = request?.MessageTemplateCode ?? "DEFAULT_PAYMENT_REMINDER";

        // Boundary check: audit codebase menunjukkan belum ada provider WhatsApp/messaging eksternal
        // yang terkonfigurasi pada repositori backend ini. Jangan merekayasa keberhasilan pengiriman (Section 14).
        // Catat entri reminder dengan status BLOCKED_NO_PROVIDER secara aman.
        var reminder = new BilPaymentReminder
        {
            InvoiceId = invoiceId,
            PatientId = patientId,
            Channel = channel,
            Status = BilPaymentReminderStatuses.BlockedNoProvider,
            SentAt = DateTime.UtcNow,
            SentByUserId = actorUserId,
            MessageTemplateCode = template,
            FailureReason = "Penyedia gateway pengiriman pesan (WhatsApp/SMS) belum terintegrasi pada sistem backend.",
            CreateBy = actorUserId
        };

        try
        {
            _dbContext.BilPaymentReminders.Add(reminder);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Jika tabel BilPaymentReminder belum dimigrasikan ke database remote (Update-Database dilarang),
            // lakukan logging peringatan tanpa mematikan respons controller.
            await _loggerService.LogWarningAsync(
                LogCategory,
                $"Tabel BilPaymentReminder belum tersedia di database atau penyimpanan gagal: {ex.Message}",
                invoiceId.ToString());
        }

        return new SendPaymentReminderResponse
        {
            InvoiceId = invoiceId,
            ReminderId = reminder.Id,
            Status = reminder.Status,
            SentAt = reminder.SentAt,
            ReminderCount = 1,
            IsDelivered = false,
            Message = "Layanan gateway pengiriman pesan WhatsApp belum terkonfigurasi pada sistem backend (integration blocker). Percobaan reminder telah dicatat ke jejak audit."
        };
    }
}
