using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Service penyimpanan dan pembacaan Formulir Persetujuan Rawat Inap & Tanda Tangan Digital (BE-RWI-205).
    /// Mengimplementasikan penegakan invariant penanda tangan bayi baru lahir (VAL-ADM-05, RWI-DEC-273).
    /// </summary>
    public class InpatientAdmissionConsentService
    {
        private readonly ApplicationDbContext _dbContext;

        public InpatientAdmissionConsentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyimpan formulir persetujuan dan tanda tangan digital ke episode rawat inap.
        /// </summary>
        public async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> SaveConsentAsync(
            Guid episodeId,
            InpatientAdmissionConsentCreateDto request,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            // 1. Validasi Keberadaan Episode
            var episode = await _dbContext.Set<InpEpisode>()
                .Include(e => e.Patient)
                .FirstOrDefaultAsync(e => e.Id == episodeId && !e.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status404NotFound, "Episode rawat inap tidak ditemukan.");
            }

            // 2. VAL-ADM-04: Pernyataan persetujuan umum (12 klausul) & tanda tangan digital wajib dilengkapi
            if (request.AgreedClauses == null || request.AgreedClauses.Count < 12 ||
                string.IsNullOrWhiteSpace(request.SignatureImageBase64) ||
                string.IsNullOrWhiteSpace(request.SignerName) ||
                request.SignerName.Trim().Length < 3)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pernyataan persetujuan umum dan tanda tangan digital wajib dilengkapi sebelum cetak.",
                    "VAL-ADM-04");
            }

            // 3. VAL-ADM-05 & RWI-DEC-273: Penanda tangan Bayi Baru Lahir dilarang diri sendiri, wajib orang tua atau wali
            var isBaby = string.Equals(request.PatientCategory, "BayiBaruLahir", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(request.PatientCategory, "baby", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(request.PatientCategory, "neonatus", StringComparison.OrdinalIgnoreCase) ||
                         episode.MotherEpisodeId.HasValue ||
                         (episode.Patient?.IsNewborn ?? false);

            var isSelfSigner = string.Equals(request.SignerType, "Self", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(request.SignerType, "Pasien", StringComparison.OrdinalIgnoreCase) ||
                               string.Equals(request.SignerRelationship, "Self", StringComparison.OrdinalIgnoreCase);

            if (isBaby && isSelfSigner)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Pasien bayi baru lahir tidak dapat menandatangani sendiri; penanda tangan wajib orang tua atau wali yang sah.",
                    "BAYI_PENANDATANGAN_WAJIB_WALI");
            }

            // 4. Cari dokumen persetujuan rawat inap aktif yang sudah ada untuk episode ini
            var existingDoc = await _dbContext.Set<InpAdmissionDocument>()
                .Include(d => d.Party)
                .Include(d => d.Signatures)
                .FirstOrDefaultAsync(d => d.EpisodeId == episodeId &&
                                          d.DocumentType == InpAdmissionDocumentType.GeneralConsent &&
                                          !d.IsDelete, cancellationToken);

            var now = DateTime.UtcNow;
            var docId = existingDoc?.Id ?? Guid.NewGuid();
            var docNumber = $"GCR-{now:yyyyMM}-{(episode.EpisodeNumber.Length > 4 ? episode.EpisodeNumber[^4..] : Guid.NewGuid().ToString("N")[..4])}";

            var responseData = new InpatientAdmissionConsentResponse
            {
                ConsentId = docId,
                EpisodeId = episodeId,
                DocumentNumber = docNumber,
                PatientCategory = request.PatientCategory,
                SignerType = request.SignerType,
                SignerName = request.SignerName.Trim(),
                SignerRelationship = request.SignerRelationship,
                SignerRelationshipText = request.SignerRelationshipText,
                SignerPhoneNumber = request.SignerPhoneNumber,
                SignerIdentityNumber = request.SignerIdentityNumber,
                AgreedClauses = request.AgreedClauses,
                SignatureImageBase64 = request.SignatureImageBase64,
                SignedAt = request.SignedAt ?? now,
                Status = "Signed",
                IsSigned = true
            };

            var snapshotJson = JsonSerializer.Serialize(responseData);

            if (existingDoc == null)
            {
                var doc = new InpAdmissionDocument
                {
                    Id = docId,
                    EpisodeId = episodeId,
                    PatientId = episode.PatientId,
                    DocumentType = InpAdmissionDocumentType.GeneralConsent,
                    Status = InpAdmissionDocumentStatus.Completed,
                    VersionNo = 1,
                    StatementDate = now.Date,
                    LockedAt = now,
                    LockedByUserId = actorUserId,
                    CompletedAt = now,
                    SnapshotJson = snapshotJson,
                    SnapshotFormatVersion = 1,
                    RowVersion = Guid.NewGuid(),
                    CreatedBy = actorUserId?.ToString() ?? "SYSTEM",
                    CreatedAt = now
                };

                // Catat Party (Pihak Penanda Tangan)
                var party = new InpAdmissionDocumentParty
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    FullName = request.SignerName.Trim(),
                    SourceType = isSelfSigner ? InpAdmissionPartySource.Patient : InpAdmissionPartySource.Manual,
                    Relationship = MapRelationship(request.SignerRelationship),
                    RelationshipText = request.SignerRelationshipText ?? request.SignerRelationship,
                    MobilePhone = request.SignerPhoneNumber != null && request.SignerPhoneNumber.Length > 13
                        ? request.SignerPhoneNumber[..13]
                        : request.SignerPhoneNumber,
                    IdentityNumber = request.SignerIdentityNumber,
                    CreatedBy = actorUserId?.ToString() ?? "SYSTEM",
                    CreatedAt = now
                };

                // Catat Signature (Slot Pasien/Keluarga)
                var signature = new InpAdmissionDocumentSignature
                {
                    Id = Guid.NewGuid(),
                    DocumentId = docId,
                    Slot = InpAdmissionSignatureSlot.PatientOrFamily,
                    Method = InpAdmissionSignatureMethod.PaperRecorded,
                    SignerName = request.SignerName.Trim(),
                    SignerRelationship = MapRelationship(request.SignerRelationship),
                    SignerRelationshipText = request.SignerRelationshipText ?? request.SignerRelationship,
                    SignedAt = request.SignedAt ?? now,
                    RecordedAt = now,
                    VerifiedByUserId = actorUserId,
                    CreatedBy = actorUserId?.ToString() ?? "SYSTEM",
                    CreatedAt = now
                };

                _dbContext.Set<InpAdmissionDocument>().Add(doc);
                _dbContext.Set<InpAdmissionDocumentParty>().Add(party);
                _dbContext.Set<InpAdmissionDocumentSignature>().Add(signature);
            }
            else
            {
                // Update dokumen yang sudah ada
                existingDoc.Status = InpAdmissionDocumentStatus.Completed;
                existingDoc.CompletedAt = now;
                existingDoc.LockedAt ??= now;
                existingDoc.LockedByUserId ??= actorUserId;
                existingDoc.SnapshotJson = snapshotJson;
                existingDoc.SnapshotFormatVersion = 1;
                existingDoc.RowVersion = Guid.NewGuid();
                existingDoc.UpdatedBy = actorUserId?.ToString() ?? "SYSTEM";
                existingDoc.UpdatedAt = now;

                if (existingDoc.Party != null)
                {
                    existingDoc.Party.FullName = request.SignerName.Trim();
                    existingDoc.Party.Relationship = MapRelationship(request.SignerRelationship);
                    existingDoc.Party.RelationshipText = request.SignerRelationshipText ?? request.SignerRelationship;
                    existingDoc.Party.MobilePhone = request.SignerPhoneNumber != null && request.SignerPhoneNumber.Length > 13
                        ? request.SignerPhoneNumber[..13]
                        : request.SignerPhoneNumber;
                    existingDoc.Party.IdentityNumber = request.SignerIdentityNumber;
                    existingDoc.Party.UpdatedBy = actorUserId?.ToString() ?? "SYSTEM";
                    existingDoc.Party.UpdatedAt = now;
                }

                var sig = existingDoc.Signatures?.FirstOrDefault(s => s.Slot == InpAdmissionSignatureSlot.PatientOrFamily && !s.IsDelete);
                if (sig != null)
                {
                    sig.SignerName = request.SignerName.Trim();
                    sig.SignerRelationship = MapRelationship(request.SignerRelationship);
                    sig.SignerRelationshipText = request.SignerRelationshipText ?? request.SignerRelationship;
                    sig.SignedAt = request.SignedAt ?? now;
                    sig.RecordedAt = now;
                    sig.VerifiedByUserId = actorUserId;
                    sig.UpdatedBy = actorUserId?.ToString() ?? "SYSTEM";
                    sig.UpdatedAt = now;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                responseData, "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan.");
        }

        /// <summary>
        /// Mengambil formulir persetujuan rawat inap dan citra tanda tangan digital dari episode rawat inap.
        /// </summary>
        public async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> GetConsentAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var doc = await _dbContext.Set<InpAdmissionDocument>()
                .Include(d => d.Party)
                .Include(d => d.Signatures)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.EpisodeId == episodeId &&
                                          d.DocumentType == InpAdmissionDocumentType.GeneralConsent &&
                                          !d.IsDelete, cancellationToken);

            if (doc == null || string.IsNullOrWhiteSpace(doc.SnapshotJson))
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status404NotFound, "Formulir persetujuan rawat inap belum diisi atau tidak ditemukan.");
            }

            try
            {
                var response = JsonSerializer.Deserialize<InpatientAdmissionConsentResponse>(doc.SnapshotJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response != null)
                {
                    response.ConsentId = doc.Id;
                    response.EpisodeId = episodeId;
                    response.Status = doc.Status.ToString();
                    response.IsSigned = true;
                    return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                        response, "Formulir persetujuan rawat inap berhasil dimuat.");
                }
            }
            catch
            {
                // Fallback bila deserialisasi snapshot gagal
            }

            var fallbackResponse = new InpatientAdmissionConsentResponse
            {
                ConsentId = doc.Id,
                EpisodeId = episodeId,
                DocumentNumber = $"GCR-{doc.StatementDate:yyyyMM}-{doc.Id.ToString("N")[..4]}",
                SignerName = doc.Party?.FullName ?? "Pasien / Keluarga",
                SignerRelationshipText = doc.Party?.RelationshipText,
                SignerPhoneNumber = doc.Party?.MobilePhone,
                SignerIdentityNumber = doc.Party?.IdentityNumber,
                Status = doc.Status.ToString(),
                IsSigned = true
            };

            return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                fallbackResponse, "Formulir persetujuan rawat inap berhasil dimuat.");
        }

        private static InpAdmissionPartyRelationship? MapRelationship(string? relationship)
        {
            if (string.IsNullOrWhiteSpace(relationship)) return InpAdmissionPartyRelationship.Other;

            return relationship.Trim().ToLowerInvariant() switch
            {
                "self" or "pasien" or "dirisendiri" => InpAdmissionPartyRelationship.Self,
                "spouse" or "suami" or "istri" => InpAdmissionPartyRelationship.Spouse,
                "child" or "anak" => InpAdmissionPartyRelationship.Child,
                "parent" or "orangtua" or "ayah" or "ibu" => InpAdmissionPartyRelationship.Parent,
                "sibling" or "saudara" => InpAdmissionPartyRelationship.Sibling,
                "guardian" or "wali" => InpAdmissionPartyRelationship.Guardian,
                _ => InpAdmissionPartyRelationship.Other
            };
        }
    }
}
