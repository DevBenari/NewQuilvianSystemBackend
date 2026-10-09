using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Catatan cetak dan data cetak dokumen admisi — <c>BE-RWI-193</c> (catat dan riwayat cetak),
    /// <c>BE-RWI-194</c> (data cetak per status), <c>BE-RWI-196</c>, <c>197</c> (jenis gelang dan IPD
    /// pada catat cetak); validation 15.6, kamus data 20.13.1, <c>INV-RWA-09</c>.
    /// </summary>
    /// <remarks>
    /// <b>Cetak ulang selalu beralasan dan tercatat.</b> Cetakan pertama tidak butuh alasan; cetakan
    /// berikutnya untuk kunci yang sama wajib beralasan; pada episode <c>Closed</c>/<c>Cancelled</c>
    /// setiap cetak wajib beralasan. "Cetakan ke-n" dihitung dari urutan baris, tidak disimpan.
    ///
    /// <para>
    /// Contoh: gelang Tn. Budi dicetak Sari 09.50. Andi mencetak lagi 15.10 tanpa alasan → ditolak
    /// "Pilih alasan cetak ulang."; dengan alasan "rusak" → baris kedua, "Cetakan ke-2, rusak, oleh Andi".
    /// </para>
    ///
    /// <para>
    /// <b>Penanda cetakan dokumen.</b> <c>Draft</c> "KONSEP — BELUM DITANDATANGANI" dari data hidup;
    /// <c>AwaitingSignature</c> "Lembar untuk ditandatangani — versi n" dari salinan beku; <c>Completed</c>
    /// final beserta catatan tanda tangan; <c>Superseded</c> "DIGANTIKAN VERSI n+1"; <c>Cancelled</c>
    /// "DIBATALKAN"; episode batal "ADMISI DIBATALKAN" (<c>RWI-DEC-263</c>, <c>RWI-AC-384</c>).
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionPrintService
    {
        private const string IdempotencyIndexName = "UX_InpAdmissionPrintLog_IdempotencyKey";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionWriteGuard _writeGuard;
        private readonly InpAdmissionSourceReader _sourceReader;
        private readonly InpAdmissionSnapshotBuilder _snapshotBuilder;
        private readonly InpAdmissionDocumentService _documents;
        private readonly AccessPermissionService _permissions;

        public InpAdmissionPrintService(
            ApplicationDbContext dbContext,
            InpAdmissionWriteGuard writeGuard,
            InpAdmissionSourceReader sourceReader,
            InpAdmissionSnapshotBuilder snapshotBuilder,
            InpAdmissionDocumentService documents,
            AccessPermissionService permissions)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _sourceReader = sourceReader;
            _snapshotBuilder = snapshotBuilder;
            _documents = documents;
            _permissions = permissions;
        }

        // =====================================================================
        // Catat cetak (POST /print-logs)
        // =====================================================================

        public async Task<InpAdmissionResult<PrintLogResponse>> RecordPrintAsync(
            Guid episodeId,
            RecordPrintRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var key = InpAdmissionDocumentService.NormalizeIdempotencyKey(idempotencyKey, out var keyInvalid);

            if (keyInvalid)
            {
                return Fail(StatusCodes.Status400BadRequest, "Kunci permintaan tidak sah.");
            }

            if (key != null)
            {
                var replay = await ReplayAsync(episodeId, key, cancellationToken);

                if (replay != null)
                {
                    return replay;
                }
            }

            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<PrintLogResponse>();
            }

            var gate = gateResult.Data!;

            if (!request.PrintKind.HasValue || !Enum.IsDefined(request.PrintKind.Value))
            {
                return Fail(StatusCodes.Status400BadRequest, "Jenis cetakan wajib dipilih.");
            }

            var kind = request.PrintKind.Value;
            var copies = request.Copies ?? 1;

            // VAL-RWA-46: jumlah salinan 1–10.
            if (copies is < 1 or > 10)
            {
                return Fail(StatusCodes.Status400BadRequest, "Jumlah salinan 1 sampai 10.");
            }

            Guid? documentId = null;
            InpAdmissionDocumentStatus? statusAtPrint = null;

            if (kind == InpAdmissionPrintKind.AdmissionDocument)
            {
                if (!request.DocumentId.HasValue || request.DocumentId.Value == Guid.Empty)
                {
                    return Fail(StatusCodes.Status400BadRequest, "Pilih dokumen admisi yang dicetak.");
                }

                var document = await _dbContext.Set<InpAdmissionDocument>()
                    .AsNoTracking()
                    .Where(x => x.Id == request.DocumentId.Value && x.EpisodeId == episodeId && !x.IsDelete)
                    .Select(x => new { x.Id, x.Status })
                    .FirstOrDefaultAsync(cancellationToken);

                if (document == null)
                {
                    return Fail(StatusCodes.Status404NotFound, InpAdmissionDocumentService.NotFoundMessage);
                }

                documentId = document.Id;
                statusAtPrint = document.Status;
            }
            else
            {
                var kindFailure = await ValidatePrintableAsync(gate, kind, cancellationToken);

                if (kindFailure != null)
                {
                    return kindFailure;
                }
            }

            var isReprint = gate.IsClosedOrCancelled ||
                await HasPreviousPrintAsync(episodeId, kind, documentId, statusAtPrint, cancellationToken);

            InpReprintReason? reason = null;
            string? note = null;

            if (isReprint)
            {
                // VAL-RWA-40, 41.
                if (!request.ReprintReason.HasValue || !Enum.IsDefined(request.ReprintReason.Value))
                {
                    return Fail(StatusCodes.Status422UnprocessableEntity, "Pilih alasan cetak ulang.",
                        InpAdmissionCodes.ReprintReasonRequired);
                }

                reason = request.ReprintReason.Value;
                note = InpAdmissionText.Clean(request.ReprintNote);

                if (reason == InpReprintReason.Other && note == null)
                {
                    return Fail(StatusCodes.Status400BadRequest, "Isi keterangan alasan cetak ulang.");
                }

                if (note?.Length > 200)
                {
                    return Fail(StatusCodes.Status400BadRequest, "Keterangan alasan cetak ulang maksimal 200 karakter.");
                }
            }

            var now = DateTime.UtcNow;

            var log = new InpAdmissionPrintLog
            {
                Id = Guid.NewGuid(),
                EpisodeId = episodeId,
                DocumentId = documentId,
                PrintKind = kind,
                DocumentStatusAtPrint = statusAtPrint,
                Copies = copies,
                IsReprint = isReprint,
                ReprintReason = reason,
                ReprintNote = note,
                PrintedByUserId = actorUserId,
                PrintedAt = now,
                IdempotencyKey = key,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<InpAdmissionPrintLog>().Add(log);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (InpAdmissionDocumentService.IsUniqueViolation(exception, out var constraint))
            {
                _dbContext.ChangeTracker.Clear();

                if (constraint == IdempotencyIndexName && key != null)
                {
                    var replay = await ReplayAsync(episodeId, key, cancellationToken);

                    if (replay != null)
                    {
                        return replay;
                    }
                }

                throw;
            }

            var logs = await LoadLogsWithSequenceAsync(episodeId, cancellationToken);
            var saved = logs.First(x => x.Id == log.Id);

            return InpAdmissionResult<PrintLogResponse>.Ok(saved,
                isReprint ? $"Cetak ulang tercatat (cetakan ke-{saved.PrintSequence})." : "Cetakan tercatat.");
        }

        /// <summary>Riwayat cetak episode dengan "cetakan ke-n" per kunci cetakan yang sama.</summary>
        public async Task<InpAdmissionResult<List<PrintLogResponse>>> GetPrintLogsAsync(
            Guid episodeId,
            InpAdmissionPrintKind? kind,
            Guid? documentId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<List<PrintLogResponse>>();
            }

            var logs = await LoadLogsWithSequenceAsync(episodeId, cancellationToken);

            if (kind.HasValue)
            {
                logs = logs.Where(x => x.PrintKind == (int)kind.Value).ToList();
            }

            if (documentId.HasValue)
            {
                logs = logs.Where(x => x.DocumentId == documentId.Value).ToList();
            }

            return InpAdmissionResult<List<PrintLogResponse>>.Ok(
                logs.OrderByDescending(x => x.PrintedAt).ToList(),
                "Riwayat cetak berhasil diambil.");
        }

        // =====================================================================
        // Data cetak dokumen (GET /documents/{id}/print dan /amount-print)
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentPrintResponse>> GetDocumentPrintAsync(
            Guid episodeId,
            Guid documentId,
            bool amountPath,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<AdmissionDocumentPrintResponse>();
            }

            var gate = gateResult.Data!;
            var document = await _documents.LoadAsync(episodeId, documentId, tracked: false, cancellationToken);

            if (document == null)
            {
                return InpAdmissionResult<AdmissionDocumentPrintResponse>.Fail(
                    StatusCodes.Status404NotFound, InpAdmissionDocumentService.NotFoundMessage);
            }

            // VAL-RWA-42: dokumen berupiah hanya lewat /amount-print, tanpa rupiah hanya lewat /print.
            if (InpAdmissionDocumentRules.HasAmounts(document.DocumentType) != amountPath)
            {
                return InpAdmissionResult<AdmissionDocumentPrintResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Dokumen ini dicetak lewat jalur cetak yang lain.",
                    InpAdmissionCodes.WrongPrintPath);
            }

            // VAL-RWA-43 (RWI-DEC-258 butir 3): cetak berupiah juga butuh Print, diperiksa di service
            // karena satu method hanya boleh satu [AccessPermission].
            if (amountPath && !await _permissions.HasAccessAsync(user, InpAdmissionDocumentService.PermissionResource, "Print"))
            {
                return InpAdmissionResult<AdmissionDocumentPrintResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Anda tidak punya hak mencetak dokumen ini.",
                    InpAdmissionCodes.AmountPrintNeedsPrint);
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);

            InpAdmissionDocumentSnapshot? snapshot;
            InpEpisodeDepositSummaryDto? liveDeposit = null;

            if (document.Status == InpAdmissionDocumentStatus.Draft)
            {
                if (amountPath && document.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement)
                {
                    var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
                    liveDeposit = deposit.Value;
                }

                // Konsep dicetak dari data hidup; tidak disimpan.
                var build = await _snapshotBuilder.BuildAsync(
                    episodeId, document.PatientId, gate.EncounterId, document.DocumentType, document.VersionNo,
                    document.SigningCity, liveDeposit, DateTime.UtcNow, cancellationToken);
                snapshot = build.Snapshot;
            }
            else
            {
                snapshot = InpAdmissionSnapshotBuilder.Deserialize(document.SnapshotJson);
            }

            if (snapshot == null)
            {
                return InpAdmissionResult<AdmissionDocumentPrintResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Salinan beku dokumen tidak terbaca, sehingga dokumen tidak dapat dicetak.");
            }

            var signatures = document.Signatures.Where(x => !x.IsDelete).ToList();
            var users = await _sourceReader.GetUsersAsync(
                signatures.Where(x => x.VerifiedByUserId.HasValue).Select(x => x.VerifiedByUserId!.Value),
                cancellationToken);

            var (marker, markerText) = Marker(document);

            int? nextSequence = null;

            if (document.Status != InpAdmissionDocumentStatus.Draft)
            {
                var printed = await _dbContext.Set<InpAdmissionPrintLog>()
                    .AsNoTracking()
                    .CountAsync(x => x.DocumentId == document.Id && !x.IsDelete &&
                        x.DocumentStatusAtPrint == document.Status, cancellationToken);
                nextSequence = printed + 1;
            }

            var response = new AdmissionDocumentPrintResponse
            {
                DocumentId = document.Id,
                DocumentType = (int)document.DocumentType,
                DocumentTypeName = document.DocumentType.ToString(),
                Status = (int)document.Status,
                StatusName = document.Status.ToString(),
                VersionNo = document.VersionNo,
                PrintMarker = marker,
                PrintMarkerText = markerText,
                EpisodeCancelledMarker = gate.Status == InpEpisodeStatus.Cancelled ? "ADMISI DIBATALKAN" : null,
                NextPrintSequence = nextSequence,
                Letterhead = InpAdmissionSnapshotBuilder.ToLetterheadResponse(snapshot.Letterhead),
                FormCode = snapshot.FormCode,
                SigningCity = document.Status == InpAdmissionDocumentStatus.Draft
                    ? document.SigningCity
                    : snapshot.SigningCity ?? document.SigningCity,
                StatementDate = document.StatementDate,
                Patient = new AdmissionPrintPatientResponse
                {
                    FullName = snapshot.Patient.FullName,
                    Salutation = snapshot.Patient.Salutation,
                    MedicalRecordNumber = snapshot.Patient.MedicalRecordNumber,
                    BirthDate = snapshot.Patient.BirthDate,
                    GenderName = snapshot.Patient.GenderName,
                    Religion = snapshot.Patient.Religion,
                    Address = snapshot.Patient.Address
                },
                Episode = new AdmissionPrintEpisodeResponse
                {
                    EpisodeNumber = snapshot.Episode.EpisodeNumber,
                    AdmittedAt = snapshot.Episode.AdmittedAt,
                    PatientClassName = snapshot.Episode.PatientClassName,
                    ServiceUnitName = snapshot.Episode.ServiceUnitName,
                    RoomName = snapshot.Episode.RoomName,
                    BedName = snapshot.Episode.BedName,
                    AttendingDoctorName = snapshot.Episode.AttendingDoctorName
                },
                Guarantor = snapshot.Guarantor == null ? null : new AdmissionPrintGuarantorResponse
                {
                    PaymentTypeName = snapshot.Guarantor.PaymentTypeName,
                    GuarantorName = snapshot.Guarantor.GuarantorName,
                    PolicyNumber = snapshot.Guarantor.PolicyNumber,
                    MemberNumber = snapshot.Guarantor.MemberNumber,
                    CardNumber = snapshot.Guarantor.CardNumber
                },
                Note = document.Note,
                Party = InpAdmissionDocumentService.ToPartyResponse(document.Party),
                HandoverItems = document.HandoverItems
                    .Where(x => !x.IsDelete)
                    .OrderBy(x => x.LineNo)
                    .Select(x => new AdmissionHandoverItemResponse
                    {
                        ClearanceItemId = x.ClearanceItemId,
                        LineNo = x.LineNo,
                        ItemNumber = x.ItemNumberSnapshot,
                        ParentItemNumber = x.ParentItemNumberSnapshot,
                        Code = x.ItemCodeSnapshot,
                        Name = x.ItemNameSnapshot,
                        Choice = x.Choice.HasValue ? (int)x.Choice.Value : null,
                        Note = x.Note
                    })
                    .ToList(),
                Privacy = InpAdmissionDocumentService.ToPrivacyResponse(document),
                BeliefItems = InpAdmissionDocumentService.ToBeliefResponses(document),
                CostDifference = InpAdmissionDocumentService.ToCostDifferenceResponse(document.CostDifferenceStatement)
            };

            if (document.DepositStatement != null)
            {
                var statement = document.DepositStatement;
                var frozen = document.Status != InpAdmissionDocumentStatus.Draft;

                response.Deposit = new AdmissionPrintDepositResponse
                {
                    DueAt = statement.DueAt,
                    DueAtText = statement.DueAt.HasValue
                        ? $"{InpAdmissionText.FormatLongDate(InpAdmissionText.ToLocal(statement.DueAt.Value, timeZone))} pukul {InpAdmissionText.FormatTime(statement.DueAt.Value, timeZone)}"
                        : null
                };

                if (amountPath)
                {
                    var amounts = frozen
                        ? InpAdmissionDocumentService.BuildDepositAmounts(
                            statement.MinimumPolicyAmount, statement.ReceivedAmount, statement.ShortfallAmount,
                            statement.AmountsReadAt, isFrozen: true)
                        : liveDeposit == null
                            ? null
                            : InpAdmissionDocumentService.BuildDepositAmounts(
                                liveDeposit.MinimumPolicyAmount, liveDeposit.TotalReceived,
                                liveDeposit.PolicyShortfallAmount, DateTime.UtcNow, isFrozen: false);

                    response.Deposit.MinimumPolicyAmount = amounts?.MinimumPolicyAmount;
                    response.Deposit.ReceivedAmount = amounts?.ReceivedAmount;
                    response.Deposit.ShortfallAmount = amounts?.ShortfallAmount;
                    response.Deposit.CalculationText = amounts?.CalculationText;
                    response.Deposit.AmountsReadAt = amounts?.ReadAt;
                }
            }

            foreach (var slot in InpAdmissionDocumentRules.RequiredSlots(document.DocumentType))
            {
                var signature = signatures.FirstOrDefault(x => x.Slot == slot);

                response.SignatureLines.Add(new AdmissionSignatureLineResponse
                {
                    Slot = (int)slot,
                    SlotName = slot.ToString(),
                    Label = InpAdmissionDocumentRules.SlotLabel(document.DocumentType, slot),
                    IsSigned = signature != null,
                    Text = signature == null ? null : SignatureText(signature, users, timeZone)
                });
            }

            return InpAdmissionResult<AdmissionDocumentPrintResponse>.Ok(response, "Data cetak dokumen admisi berhasil diambil.");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Gelang, label, dan IPD hanya boleh dicatat bila data wajibnya terbaca; jenis gelang harus
        /// sama dengan hitungan data pasien (<c>VAL-RWA-44</c>, <c>45</c>).
        /// </summary>
        private async Task<InpAdmissionResult<PrintLogResponse>?> ValidatePrintableAsync(
            InpAdmissionEpisodeGate gate,
            InpAdmissionPrintKind kind,
            CancellationToken cancellationToken)
        {
            var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);

            if (!identity.IsAvailable)
            {
                return Fail(StatusCodes.Status422UnprocessableEntity,
                    "Cetak ditahan sampai data wajib terbaca lengkap.", InpAdmissionCodes.BaseDataPrintBlocked);
            }

            if (kind == InpAdmissionPrintKind.InpatientBaseData)
            {
                var episode = await _sourceReader.GetEpisodeAsync(gate.EpisodeId, cancellationToken);

                if (!episode.IsAvailable)
                {
                    return Fail(StatusCodes.Status422UnprocessableEntity,
                        "Cetak ditahan sampai data wajib terbaca lengkap.", InpAdmissionCodes.BaseDataPrintBlocked);
                }
            }

            if (kind is InpAdmissionPrintKind.AdultWristband or InpAdmissionPrintKind.InfantWristband)
            {
                var setting = await _sourceReader.GetSettingAsync(cancellationToken);
                var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
                var today = InpAdmissionText.LocalToday(InpAdmissionSourceReader.ResolveTimeZone(hospital));
                var patient = identity.Value!;

                var expected = InpWristbandRules.ResolveKind(
                    patient.IsNewborn, patient.BirthDate, today, setting.InfantWristbandMaxAgeYears) == InpWristbandKind.Infant
                    ? InpAdmissionPrintKind.InfantWristband
                    : InpAdmissionPrintKind.AdultWristband;

                if (expected != kind)
                {
                    return Fail(StatusCodes.Status422UnprocessableEntity,
                        "Jenis gelang tidak sesuai data pasien. Muat ulang data gelang.", InpAdmissionCodes.WristbandKindMismatch);
                }
            }

            return null;
        }

        /// <summary>Kunci "cetakan yang sama" (kamus data 20.13.1). Cetakan konsep tidak pernah dihitung cetak ulang.</summary>
        private Task<bool> HasPreviousPrintAsync(
            Guid episodeId,
            InpAdmissionPrintKind kind,
            Guid? documentId,
            InpAdmissionDocumentStatus? statusAtPrint,
            CancellationToken cancellationToken)
        {
            if (kind == InpAdmissionPrintKind.AdmissionDocument)
            {
                if (statusAtPrint == InpAdmissionDocumentStatus.Draft)
                {
                    return Task.FromResult(false);
                }

                return _dbContext.Set<InpAdmissionPrintLog>()
                    .AsNoTracking()
                    .AnyAsync(x => x.DocumentId == documentId && x.DocumentStatusAtPrint == statusAtPrint && !x.IsDelete,
                        cancellationToken);
            }

            return _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .AnyAsync(x => x.EpisodeId == episodeId && x.PrintKind == kind && !x.IsDelete, cancellationToken);
        }

        /// <summary>Seluruh log cetak episode beserta urutan per kunci cetakan yang sama.</summary>
        private async Task<List<PrintLogResponse>> LoadLogsWithSequenceAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .OrderBy(x => x.PrintedAt)
                .ThenBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var users = await _sourceReader.GetUsersAsync(rows.Select(x => x.PrintedByUserId), cancellationToken);
            var counters = new Dictionary<string, int>(StringComparer.Ordinal);
            var result = new List<PrintLogResponse>(rows.Count);

            foreach (var row in rows)
            {
                var sequenceKey = row.PrintKind == InpAdmissionPrintKind.AdmissionDocument
                    ? $"{row.PrintKind}|{row.DocumentId}|{row.DocumentStatusAtPrint}"
                    : $"{row.PrintKind}";

                counters[sequenceKey] = counters.TryGetValue(sequenceKey, out var count) ? count + 1 : 1;

                result.Add(new PrintLogResponse
                {
                    Id = row.Id,
                    PrintKind = (int)row.PrintKind,
                    PrintKindName = row.PrintKind.ToString(),
                    DocumentId = row.DocumentId,
                    DocumentStatusAtPrint = row.DocumentStatusAtPrint.HasValue ? (int)row.DocumentStatusAtPrint.Value : null,
                    Copies = row.Copies,
                    IsReprint = row.IsReprint,
                    PrintSequence = counters[sequenceKey],
                    ReprintReason = row.ReprintReason.HasValue ? (int)row.ReprintReason.Value : null,
                    ReprintReasonName = row.ReprintReason?.ToString(),
                    ReprintNote = row.ReprintNote,
                    PrintedByName = users.TryGetValue(row.PrintedByUserId, out var printer) ? printer.DisplayName : null,
                    PrintedAt = row.PrintedAt
                });
            }

            return result;
        }

        private async Task<InpAdmissionResult<PrintLogResponse>?> ReplayAsync(
            Guid episodeId,
            string key,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .Where(x => x.IdempotencyKey == key && !x.IsDelete)
                .Select(x => new { x.Id, x.EpisodeId })
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                return null;
            }

            if (existing.EpisodeId != episodeId)
            {
                return Fail(StatusCodes.Status409Conflict, "Kunci permintaan sudah dipakai untuk permintaan lain.");
            }

            var logs = await LoadLogsWithSequenceAsync(episodeId, cancellationToken);

            return InpAdmissionResult<PrintLogResponse>.Ok(
                logs.First(x => x.Id == existing.Id),
                "Cetakan yang sama sudah tercatat sebelumnya.");
        }

        private static (string Marker, string? Text) Marker(InpAdmissionDocument document) => document.Status switch
        {
            InpAdmissionDocumentStatus.Draft => ("Draft", "KONSEP — BELUM DITANDATANGANI"),
            InpAdmissionDocumentStatus.AwaitingSignature => ("SignatureSheet", $"Lembar untuk ditandatangani — versi {document.VersionNo}"),
            InpAdmissionDocumentStatus.Completed => ("Final", null),
            InpAdmissionDocumentStatus.Superseded => ("Superseded", $"DIGANTIKAN VERSI {document.VersionNo + 1}"),
            _ => ("Cancelled", "DIBATALKAN")
        };

        private static string SignatureText(
            InpAdmissionDocumentSignature signature,
            IReadOnlyDictionary<Guid, InpAdmissionUserInfo> users,
            TimeZoneInfo timeZone)
        {
            var when = InpAdmissionText.FormatDateTime(signature.SignedAt, timeZone);

            if (signature.Method == InpAdmissionSignatureMethod.ElectronicAttestation)
            {
                // Nama di cetakan selalu nama penanda tangan, bukan pencetak (FR-RWA-123).
                return string.IsNullOrWhiteSpace(signature.SignerPositionName)
                    ? $"Ditandatangani secara elektronik oleh {signature.SignerName}, {when}"
                    : $"Ditandatangani secara elektronik oleh {signature.SignerName}, {signature.SignerPositionName}, {when}";
            }

            var relationship = InpAdmissionDocumentRules.RelationshipLabel(signature.SignerRelationship, signature.SignerRelationshipText);
            var verifier = signature.VerifiedByUserId.HasValue && users.TryGetValue(signature.VerifiedByUserId.Value, out var user)
                ? user.DisplayName
                : "petugas";

            return $"Ditandatangani di kertas oleh {signature.SignerName} ({relationship}), {when}, diverifikasi {verifier}";
        }

        private static InpAdmissionResult<PrintLogResponse> Fail(int statusCode, string message, string? code = null)
            => InpAdmissionResult<PrintLogResponse>.Fail(statusCode, message, code);
    }
}
