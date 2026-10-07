using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed record BillingDrugReturnRequest(Guid InvoiceId, Guid ItemId, Guid ExpectedRowVersion, int SourceVersion,
    decimal Quantity, decimal UnitPrice, string ReturnNumber, DateTime OccurredAt, Guid CorrelationId, Guid CausationId);

public sealed class BillingDrugReturnService(ApplicationDbContext db, BillingCalculationService calculation, LoggerService logger)
{
    public async Task<Guid?> ApplyAsync(BillingDrugReturnRequest request, Guid key, Guid actorId, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (db.Database.IsRelational())
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [$"BIL_CALCULATION_{request.InvoiceId:N}"], ct);
        var prior = await db.BilInvoiceItems.AsNoTracking().Where(x => x.InvoiceId == request.InvoiceId
            && x.LastIdempotencyKey == key && !x.IsDelete).ToListAsync(ct);
        if (prior.Count > 0) return prior.FirstOrDefault(x => x.Status == BillingInvoiceItemStatuses.Active)?.Id;
        var invoice = await db.BilInvoices.SingleAsync(x => x.Id == request.InvoiceId && !x.IsDelete, ct);
        if (invoice.Status != BillingInvoiceStatuses.Open)
            throw new BillingInvoiceValidationException("Invoice sudah final; retur memerlukan adjustment Billing.");
        if (invoice.RowVersion != request.ExpectedRowVersion)
            throw new BillingInvoiceConflictException("Invoice berubah saat retur diproses; coba ulang.");
        if (await db.BilCalculationVersions.AnyAsync(x => x.InvoiceId == invoice.Id && !x.IsDelete && x.IsLocked, ct))
            throw new BillingInvoiceValidationException("Hitungan terkunci; retur memerlukan adjustment Billing.");
        var original = await db.BilInvoiceItems.SingleAsync(x => x.Id == request.ItemId && !x.IsDelete, ct);
        if (original.InvoiceId != invoice.Id || original.Status != BillingInvoiceItemStatuses.Active
            || request.SourceVersion <= original.SourceVersion || request.Quantity < 0 || request.UnitPrice < 0
            || request.Quantity * request.UnitPrice > original.Quantity * original.UnitPrice)
            throw new BillingInvoiceConflictException("Tagihan sumber retur sudah berubah atau jumlah retur tidak valid.");
        var now = DateTime.UtcNow;
        original.Status = BillingInvoiceItemStatuses.Voided;
        original.VoidReason = $"Retur {request.ReturnNumber}; digantikan revisi sumber {request.SourceVersion}.";
        original.LastIdempotencyKey = key; original.UpdateBy = actorId; original.UpdateDateTime = now;
        // Lepas unique aktif sebelum replacement disimpan, tetap di transaksi yang sama.
        await db.SaveChangesAsync(ct);
        BilInvoiceItem? replacement = null;
        if (request.Quantity > 0)
        {
            replacement = new BilInvoiceItem
            {
                InvoiceId = invoice.Id, SourceDomain = original.SourceDomain, SourceDetailId = original.SourceDetailId,
                SourceVersion = request.SourceVersion, SourceContractVersion = original.SourceContractVersion,
                SourceStatus = original.SourceStatus, SourceOccurredAt = new DateTimeOffset(DateTime.SpecifyKind(request.OccurredAt, DateTimeKind.Utc)),
                CategoryId = original.CategoryId, TariffId = original.TariffId, DescriptionSnapshot = original.DescriptionSnapshot,
                Quantity = request.Quantity, UnitPrice = request.UnitPrice,
                DoctorShare = Math.Min(original.DoctorShare, request.Quantity * request.UnitPrice),
                LastIdempotencyKey = key, LastCorrelationId = request.CorrelationId, LastCausationId = request.CausationId,
                SourcePayloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key.ToString("D")))),
                CreateBy = actorId, CreateDateTime = now
            };
            db.BilInvoiceItems.Add(replacement);
            var payer = await db.BilInvoiceItemPayerAssignments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.InvoiceItemId == original.Id && x.IsActive && !x.IsDelete, ct);
            if (payer != null)
                db.BilInvoiceItemPayerAssignments.Add(new BilInvoiceItemPayerAssignment
                {
                    InvoiceItemId = replacement.Id, EncounterGuarantorId = payer.EncounterGuarantorId,
                    PayerKind = payer.PayerKind, AssignmentSource = payer.AssignmentSource, Reason = payer.Reason,
                    CreateBy = actorId, CreateDateTime = now
                });
            var disposition = await db.BilInvoiceItemBillingDispositions.AsNoTracking()
                .SingleOrDefaultAsync(x => x.InvoiceItemId == original.Id && x.IsActive && !x.IsDelete, ct);
            if (disposition != null)
                db.BilInvoiceItemBillingDispositions.Add(new BilInvoiceItemBillingDisposition
                {
                    InvoiceItemId = replacement.Id, Disposition = disposition.Disposition,
                    DecisionSource = disposition.DecisionSource, Reason = disposition.Reason,
                    CreateBy = actorId, CreateDateTime = now
                });
            await db.SaveChangesAsync(ct);
        }
        invoice.RowVersion = Guid.NewGuid(); invoice.UpdateBy = actorId; invoice.UpdateDateTime = now;
        await calculation.RecalculateAsync(invoice.Id, new RecalculateInvoiceRequest
        {
            ExpectedRowVersion = invoice.RowVersion, Reason = $"Retur {request.ReturnNumber}: revisi jumlah layak kembali."
        }, actorId, ct);
        await tx.CommitAsync(ct);
        await logger.AuditAsync("HealthServices.BillingManagement.Billing", "DrugReturn.Applied",
            "Tagihan dikurangi sesuai retur Farmasi yang layak.",
            new { InvoiceId = invoice.Id, OriginalItemId = original.Id, ReplacementItemId = replacement?.Id, ActorUserId = actorId, request.CausationId });
        return replacement?.Id;
    }
}
