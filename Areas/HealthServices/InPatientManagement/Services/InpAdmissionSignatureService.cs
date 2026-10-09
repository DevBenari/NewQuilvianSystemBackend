using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Tanda tangan dokumen admisi — <c>BE-RWI-194</c> (fondasi), <c>BE-RWI-199</c> (tiga petugas
    /// serah terima); validation 15.5, <c>INV-RWA-04</c>, <c>INV-RWA-11</c>.
    /// </summary>
    /// <remarks>
    /// Slot pasien/keluarga selalu <b>catatan kertas</b> selama <i>fail-closed</i>: petugas mencatat
    /// nama, hubungan, dan waktu lembar ditandatangani, tanpa gambar (<c>RWI-DEC-230</c>). Slot
    /// petugas adalah <b>atestasi elektronik</b> oleh akun yang benar-benar menandatangani; nama dan
    /// jabatannya dibekukan saat itu (<c>RWI-DEC-237</c>).
    ///
    /// <para>
    /// Aturan yang dijaga di sini, di samping hak akses per slot:
    /// satu slot satu tanda tangan (<c>409 INP-ADM-DOC-032</c>); satu akun satu slot petugas pada
    /// dokumen yang sama (<c>422 INP-ADM-DOC-033</c>, unique index menjadi penjaga terakhir bila dua
    /// permintaan datang bersamaan); slot Perawat penerima hanya sesudah pasien menempati bed
    /// (<c>422 INP-ADM-DOC-034</c>). Mencatat tanda tangan kertas bukan slot petugas, sehingga Sari
    /// boleh memverifikasi kertas Ny. Rina lalu menandatangani slot Petugas PPRI.
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionSignatureService
    {
        private const string SlotIndexName = "UX_InpAdmissionDocumentSignature_Document_Slot";
        private const string SignerIndexName = "UX_InpAdmissionDocumentSignature_Document_Signer";
        private const string IdempotencyIndexName = "UX_InpAdmissionDocumentSignature_IdempotencyKey";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionDocumentService _documents;
        private readonly InpAdmissionSourceReader _sourceReader;
        private readonly InpPatientLocationQuery _locationQuery;

        public InpAdmissionSignatureService(
            ApplicationDbContext dbContext,
            InpAdmissionDocumentService documents,
            InpAdmissionSourceReader sourceReader,
            InpPatientLocationQuery locationQuery)
        {
            _dbContext = dbContext;
            _documents = documents;
            _sourceReader = sourceReader;
            _locationQuery = locationQuery;
        }

        /// <summary>Catatan lembar kertas yang sudah ditandatangani pasien atau keluarga.</summary>
        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> RecordPaperSignatureAsync(
            Guid episodeId,
            Guid documentId,
            RecordPaperSignatureRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var prepared = await PrepareAsync(
                episodeId, documentId, request.RowVersion, InpAdmissionSignatureSlot.PatientOrFamily,
                idempotencyKey, actorUserId, user, cancellationToken);

            if (prepared.Response != null)
            {
                return prepared.Response;
            }

            var (gate, document, key) = (prepared.Gate!, prepared.Document!, prepared.Key);

            // VAL-RWA-35: nama 1–200, hubungan wajib, waktu tidak sebelum kunci dan tidak di masa depan.
            var signerName = InpAdmissionText.Clean(request.SignerName);

            if (signerName == null || !request.SignerRelationship.HasValue ||
                !Enum.IsDefined(request.SignerRelationship.Value))
            {
                return BadRequest("Isi nama dan hubungan penanda tangan.");
            }

            if (signerName.Length > 200)
            {
                return BadRequest("Nama penanda tangan maksimal 200 karakter.");
            }

            var relationshipText = InpAdmissionText.Clean(request.SignerRelationshipText);

            if (relationshipText?.Length > 100)
            {
                return BadRequest("Keterangan hubungan maksimal 100 karakter.");
            }

            if (!request.SignedAt.HasValue)
            {
                return BadRequest("Isi waktu tanda tangan di lembar.");
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var signedAtUtc = ToUtc(request.SignedAt.Value, InpAdmissionSourceReader.ResolveTimeZone(hospital));
            var now = DateTime.UtcNow;

            if ((document.LockedAt.HasValue && signedAtUtc < document.LockedAt.Value) || signedAtUtc > now.AddMinutes(5))
            {
                return BadRequest("Waktu tanda tangan tidak boleh sebelum dokumen dikunci atau di masa depan.");
            }

            var signature = new InpAdmissionDocumentSignature
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Slot = InpAdmissionSignatureSlot.PatientOrFamily,
                Method = InpAdmissionSignatureMethod.PaperRecorded,
                SignerName = signerName,
                SignerRelationship = request.SignerRelationship,
                SignerRelationshipText = relationshipText,
                SignedAt = signedAtUtc,
                SignedByUserId = null,
                VerifiedByUserId = actorUserId,
                RecordedAt = now,
                IdempotencyKey = key,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            return await SaveSignatureAsync(gate, document, signature, key, actorUserId, user, now, cancellationToken);
        }

        /// <summary>Atestasi elektronik satu slot petugas oleh akun yang sedang masuk.</summary>
        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> AttestAsync(
            Guid episodeId,
            Guid documentId,
            InpAdmissionSignatureSlot slot,
            RowVersionRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (slot == InpAdmissionSignatureSlot.PatientOrFamily)
            {
                return BadRequest("Slot pasien/keluarga dicatat dari lembar kertas, bukan atestasi.");
            }

            var prepared = await PrepareAsync(
                episodeId, documentId, request.RowVersion, slot, idempotencyKey, actorUserId, user, cancellationToken);

            if (prepared.Response != null)
            {
                return prepared.Response;
            }

            var (gate, document, key) = (prepared.Gate!, prepared.Document!, prepared.Key);

            // INV-RWA-04 / VAL-RWA-33: satu akun satu slot petugas pada dokumen yang sama.
            if (document.Signatures.Any(x => !x.IsDelete && x.SignedByUserId == actorUserId))
            {
                return SameAccount(document.DocumentType);
            }

            // INV-RWA-11 / VAL-RWA-34: Perawat penerima hanya sesudah pasien menempati bed.
            if (slot == InpAdmissionSignatureSlot.ReceivingNurse &&
                !await _locationQuery.HasActivePlacementAsync(episodeId, cancellationToken))
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Pasien belum menempati tempat tidur.",
                    InpAdmissionCodes.PatientNotInBed);
            }

            var users = await _sourceReader.GetUsersAsync(new[] { actorUserId }, cancellationToken);

            if (!users.TryGetValue(actorUserId, out var signer) || string.IsNullOrWhiteSpace(signer.DisplayName))
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Nama akun penanda tangan tidak terbaca. Lengkapi nama tampilan akun lebih dulu.");
            }

            var now = DateTime.UtcNow;

            var signature = new InpAdmissionDocumentSignature
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                Slot = slot,
                Method = InpAdmissionSignatureMethod.ElectronicAttestation,
                SignerName = Truncate(signer.DisplayName, 200)!,
                SignerPositionName = Truncate(signer.PositionName, 150),
                SignedAt = now,
                SignedByUserId = actorUserId,
                VerifiedByUserId = null,
                RecordedAt = now,
                IdempotencyKey = key,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            return await SaveSignatureAsync(gate, document, signature, key, actorUserId, user, now, cancellationToken);
        }

        /// <summary>
        /// Penjaga bersama setiap tanda tangan: kunci idempoten, episode berjalan, <c>RowVersion</c>,
        /// status <c>AwaitingSignature</c>, slot wajib jenis itu, dan slot masih kosong.
        /// </summary>
        private async Task<PreparedSignature> PrepareAsync(
            Guid episodeId,
            Guid documentId,
            Guid? rowVersion,
            InpAdmissionSignatureSlot slot,
            string? idempotencyKey,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var key = InpAdmissionDocumentService.NormalizeIdempotencyKey(idempotencyKey, out var keyInvalid);

            if (keyInvalid)
            {
                return PreparedSignature.Stop(BadRequest("Kunci permintaan tidak sah."));
            }

            if (key != null)
            {
                var replay = await ReplayAsync(episodeId, documentId, key, user, cancellationToken);

                if (replay != null)
                {
                    return PreparedSignature.Stop(replay);
                }
            }

            var loaded = await _documents.LoadForCommandAsync(episodeId, documentId, rowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return PreparedSignature.Stop(loaded.AsFailure<AdmissionDocumentResponse>());
            }

            var (gate, document) = loaded.Data!;

            if (document.Status == InpAdmissionDocumentStatus.Draft)
            {
                return PreparedSignature.Stop(InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Dokumen belum dikunci oleh petugas admisi, sehingga belum dapat ditandatangani.",
                    InpAdmissionCodes.SignatureNotLocked));
            }

            if (document.Status != InpAdmissionDocumentStatus.AwaitingSignature)
            {
                return PreparedSignature.Stop(InpAdmissionDocumentService.InvalidState(document.Status, "ditandatangani"));
            }

            if (!InpAdmissionDocumentRules.RequiredSlots(document.DocumentType).Contains(slot))
            {
                return PreparedSignature.Stop(InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Kolom tanda tangan ini tidak ada pada {InpAdmissionDocumentRules.DocumentName(document.DocumentType)}.",
                    InpAdmissionCodes.SlotNotRequired));
            }

            var existing = document.Signatures.FirstOrDefault(x => !x.IsDelete && x.Slot == slot);

            if (existing != null)
            {
                return PreparedSignature.Stop(await SlotTakenAsync(existing, cancellationToken));
            }

            return new PreparedSignature { Gate = gate, Document = document, Key = key };
        }

        /// <summary>Menyimpan tanda tangan; slot wajib terakhir menyelesaikan dokumen.</summary>
        private async Task<InpAdmissionResult<AdmissionDocumentResponse>> SaveSignatureAsync(
            InpAdmissionEpisodeGate gate,
            InpAdmissionDocument document,
            InpAdmissionDocumentSignature signature,
            string? key,
            Guid actorUserId,
            ClaimsPrincipal user,
            DateTime now,
            CancellationToken cancellationToken)
        {
            _dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);
            document.Signatures.Add(signature);

            var filledSlots = document.Signatures.Where(x => !x.IsDelete).Select(x => x.Slot).ToHashSet();
            var completed = InpAdmissionDocumentRules.RequiredSlots(document.DocumentType).All(filledSlots.Contains);

            if (completed)
            {
                document.Status = InpAdmissionDocumentStatus.Completed;
                document.CompletedAt = now;
            }

            InpAdmissionDocumentService.Touch(document, actorUserId, now);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return InpAdmissionDocumentService.Stale();
            }
            catch (DbUpdateException exception) when (InpAdmissionDocumentService.IsUniqueViolation(exception, out var constraint))
            {
                _dbContext.ChangeTracker.Clear();

                if (constraint == IdempotencyIndexName && key != null)
                {
                    var replay = await ReplayAsync(gate.EpisodeId, document.Id, key, user, cancellationToken);

                    if (replay != null)
                    {
                        return replay;
                    }
                }

                if (constraint == SignerIndexName)
                {
                    return SameAccount(document.DocumentType);
                }

                // Dua permintaan bersamaan ke slot yang sama: yang kalah ditolak unique index slot.
                var taken = await _dbContext.Set<InpAdmissionDocumentSignature>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DocumentId == document.Id && x.Slot == signature.Slot && !x.IsDelete,
                        cancellationToken);

                return taken != null
                    ? await SlotTakenAsync(taken, cancellationToken)
                    : InpAdmissionDocumentService.Stale();
            }

            return await _documents.ReloadResponseAsync(gate, document.Id, user,
                completed ? "Tanda tangan tercatat. Dokumen lengkap." : "Tanda tangan tercatat.",
                cancellationToken);
        }

        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> ReplayAsync(
            Guid episodeId,
            Guid documentId,
            string key,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<InpAdmissionDocumentSignature>()
                .AsNoTracking()
                .Where(x => x.IdempotencyKey == key && !x.IsDelete)
                .Select(x => new { x.DocumentId })
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                return null;
            }

            if (existing.DocumentId != documentId)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status409Conflict, "Kunci permintaan sudah dipakai untuk permintaan lain.");
            }

            var gate = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => new InpAdmissionEpisodeGate(x.Id, x.PatientId, x.EncounterId, x.EpisodeStatus))
                .FirstOrDefaultAsync(cancellationToken);

            return gate == null
                ? InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status404NotFound, InpAdmissionWriteGuard.EpisodeNotFoundMessage)
                : await _documents.ReloadResponseAsync(gate, documentId, user, "Tanda tangan yang sama sudah tercatat sebelumnya.", cancellationToken);
        }

        private async Task<InpAdmissionResult<AdmissionDocumentResponse>> SlotTakenAsync(
            InpAdmissionDocumentSignature existing,
            CancellationToken cancellationToken)
        {
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var when = InpAdmissionText.FormatDateTime(existing.SignedAt, InpAdmissionSourceReader.ResolveTimeZone(hospital));

            return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                StatusCodes.Status409Conflict,
                $"Kolom ini sudah ditandatangani {existing.SignerName} pada {when}.",
                InpAdmissionCodes.SlotAlreadySigned);
        }

        private static InpAdmissionResult<AdmissionDocumentResponse> SameAccount(InpAdmissionDocumentType documentType)
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                StatusCodes.Status422UnprocessableEntity,
                documentType == InpAdmissionDocumentType.NewPatientHandover
                    ? "Satu petugas tidak boleh menandatangani dua kolom pada serah terima yang sama."
                    : "Satu petugas tidak boleh menandatangani dua kolom pada dokumen yang sama.",
                InpAdmissionCodes.SameAccountTwoSlots);

        private static InpAdmissionResult<AdmissionDocumentResponse> BadRequest(string message)
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, message);

        /// <summary>Waktu tanpa zona dibaca sebagai jam dinding rumah sakit.</summary>
        private static DateTime ToUtc(DateTime value, TimeZoneInfo timeZone) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => TimeZoneInfo.ConvertTimeToUtc(value, timeZone)
        };

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= max ? trimmed : trimmed[..max];
        }

        private sealed class PreparedSignature
        {
            public InpAdmissionResult<AdmissionDocumentResponse>? Response { get; init; }

            public InpAdmissionEpisodeGate? Gate { get; init; }

            public InpAdmissionDocument? Document { get; init; }

            public string? Key { get; init; }

            public static PreparedSignature Stop(InpAdmissionResult<AdmissionDocumentResponse> response)
                => new() { Response = response };
        }
    }
}
