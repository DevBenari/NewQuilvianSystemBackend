using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bentuk respons dokumen admisi beserta aksi yang tersedia bagi pengguna — API 12.3
    /// <c>AdmissionDocumentResponse</c> (<c>BE-RWI-194</c>).
    /// </summary>
    /// <remarks>
    /// <c>AvailableActions</c> adalah bantuan tampilan, bukan pengaman: setiap endpoint aksi tetap
    /// memeriksa ulang status dan hak di server. Hak dibaca lewat <c>AccessPermissionService</c> dari
    /// layar Akses Role, tidak pernah dari nama peran.
    /// </remarks>
    public sealed partial class InpAdmissionDocumentService
    {
        internal async Task<AdmissionDocumentResponse> BuildResponseAsync(
            InpAdmissionDocument document,
            InpAdmissionEpisodeGate gate,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var signatures = document.Signatures.Where(x => !x.IsDelete).ToList();

            var users = await _sourceReader.GetUsersAsync(
                new[] { document.CreateBy }.Concat(signatures.Where(x => x.VerifiedByUserId.HasValue).Select(x => x.VerifiedByUserId!.Value)),
                cancellationToken);

            var response = new AdmissionDocumentResponse();
            FillSummary(response, document, users);

            response.RowVersion = document.RowVersion;
            response.IsReadOnly = !gate.IsWritable || document.Status != InpAdmissionDocumentStatus.Draft;
            response.SigningCity = document.SigningCity;
            response.StatementDate = document.StatementDate;
            response.Note = document.Note;
            response.CorrectionReason = document.CorrectionReason;
            response.CancelledReason = document.CancelledReason;

            foreach (var slot in InpAdmissionDocumentRules.RequiredSlots(document.DocumentType))
            {
                var signature = signatures.FirstOrDefault(x => x.Slot == slot);

                response.Slots.Add(new AdmissionSignatureSlotResponse
                {
                    Slot = (int)slot,
                    SlotName = slot.ToString(),
                    Label = InpAdmissionDocumentRules.SlotLabel(document.DocumentType, slot),
                    Signature = signature == null ? null : new AdmissionSignatureResponse
                    {
                        Method = (int)signature.Method,
                        MethodName = signature.Method.ToString(),
                        SignerName = signature.SignerName,
                        SignerPositionName = signature.SignerPositionName,
                        SignerRelationship = signature.SignerRelationship.HasValue ? (int)signature.SignerRelationship.Value : null,
                        SignerRelationshipText = signature.Method == InpAdmissionSignatureMethod.PaperRecorded
                            ? InpAdmissionDocumentRules.RelationshipLabel(signature.SignerRelationship, signature.SignerRelationshipText)
                            : null,
                        SignedAt = signature.SignedAt,
                        VerifiedByName = signature.VerifiedByUserId.HasValue &&
                            users.TryGetValue(signature.VerifiedByUserId.Value, out var verifier)
                            ? verifier.DisplayName
                            : null
                    }
                });
            }

            response.Party = ToPartyResponse(document.Party);
            response.HandoverItems = await BuildHandoverResponsesAsync(document, gate, cancellationToken);
            response.Privacy = ToPrivacyResponse(document);
            response.BeliefItems = ToBeliefResponses(document);
            response.CostDifference = ToCostDifferenceResponse(document.CostDifferenceStatement);
            response.Deposit = document.DepositStatement == null
                ? null
                : new AdmissionDepositResponse { DueAt = document.DepositStatement.DueAt, AmountsHidden = true };

            if (document.Status is InpAdmissionDocumentStatus.AwaitingSignature or InpAdmissionDocumentStatus.Completed)
            {
                response.SourceChangedSinceLock = await HasSourceChangedAsync(document, cancellationToken);

                if (response.SourceChangedSinceLock)
                {
                    response.SourceChangedMessage = "Data pasien telah diperbarui sejak dokumen ini dikunci.";
                }
            }

            response.AvailableActions = await ComputeAvailableActionsAsync(document, signatures, gate, user);

            return response;
        }

        internal static void FillSummary(
            AdmissionDocumentSummaryResponse summary,
            InpAdmissionDocument document,
            IReadOnlyDictionary<Guid, InpAdmissionUserInfo> users)
        {
            summary.Id = document.Id;
            summary.DocumentType = (int)document.DocumentType;
            summary.DocumentTypeName = document.DocumentType.ToString();
            summary.DocumentTypeLabel = InpAdmissionDocumentRules.DocumentName(document.DocumentType);
            summary.Status = (int)document.Status;
            summary.StatusName = document.Status.ToString();
            summary.StatusLabel = InpAdmissionDocumentRules.StatusName(document.Status);
            summary.VersionNo = document.VersionNo;
            summary.PreviousVersionId = document.PreviousVersionId;
            summary.CreatedAt = document.CreateDateTime;
            summary.CreatedByName = users.TryGetValue(document.CreateBy, out var creator) ? creator.DisplayName : null;
            summary.LockedAt = document.LockedAt;
            summary.CompletedAt = document.CompletedAt;
            summary.CancelledAt = document.CancelledAt;
        }

        internal static AdmissionPartyResponse? ToPartyResponse(InpAdmissionDocumentParty? party)
            => party == null
                ? null
                : new AdmissionPartyResponse
                {
                    SourceType = (int)party.SourceType,
                    SourceTypeName = party.SourceType.ToString(),
                    SourceRecordId = party.SourceRecordId,
                    FullName = party.FullName,
                    Relationship = party.Relationship.HasValue ? (int)party.Relationship.Value : null,
                    RelationshipName = party.Relationship?.ToString(),
                    RelationshipText = party.RelationshipText,
                    Address = party.Address,
                    BirthDate = party.BirthDate,
                    Gender = party.Gender.HasValue ? (int)party.Gender.Value : null,
                    Occupation = party.Occupation,
                    IdentityType = party.IdentityType.HasValue ? (int)party.IdentityType.Value : null,
                    IdentityNumber = party.IdentityNumber,
                    MobilePhone = party.MobilePhone,
                    OfficePhone = party.OfficePhone
                };

        internal static AdmissionPrivacyResponse? ToPrivacyResponse(InpAdmissionDocument document)
        {
            if (document.DocumentType != InpAdmissionDocumentType.PrivacyRequest)
            {
                return null;
            }

            var entries = document.PrivacyEntries.Where(x => !x.IsDelete).OrderBy(x => x.LineNo).ToList();

            return new AdmissionPrivacyResponse
            {
                IsTransportPrivacyRequested = document.PrivacyRequest?.IsTransportPrivacyRequested ?? false,
                AllowedVisitors = entries.Where(x => x.EntryType == InpAdmissionPrivacyEntryType.AllowedVisitor).Select(x => x.Text).ToList(),
                SpecialRequests = entries.Where(x => x.EntryType == InpAdmissionPrivacyEntryType.SpecialServiceRequest).Select(x => x.Text).ToList()
            };
        }

        internal static List<AdmissionBeliefItemResponse> ToBeliefResponses(InpAdmissionDocument document)
            => document.BeliefItems
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.ItemNo)
                .Select(x => new AdmissionBeliefItemResponse { ItemNo = x.ItemNo, Text = x.Text })
                .ToList();

        internal static AdmissionCostDifferenceResponse? ToCostDifferenceResponse(InpAdmissionCostDifferenceStatement? statement)
            => statement == null
                ? null
                : new AdmissionCostDifferenceResponse
                {
                    Subject = (int)statement.Subject,
                    SubjectName = statement.Subject.ToString(),
                    SubjectLabel = InpAdmissionDocumentRules.SubjectLabel(statement.Subject, statement.SubjectOtherText),
                    SubjectOtherText = statement.SubjectOtherText
                };

        /// <summary>Butir serah terima beku; saran sistem hanya selama <c>Draft</c>.</summary>
        private async Task<List<AdmissionHandoverItemResponse>> BuildHandoverResponsesAsync(
            InpAdmissionDocument document,
            InpAdmissionEpisodeGate gate,
            CancellationToken cancellationToken)
        {
            var items = document.HandoverItems.Where(x => !x.IsDelete).OrderBy(x => x.LineNo).ToList();

            if (items.Count == 0)
            {
                return new List<AdmissionHandoverItemResponse>();
            }

            Dictionary<MstHandoverSuggestionSource, string>? suggestions = null;
            Dictionary<Guid, MstHandoverSuggestionSource>? sourceByItem = null;

            if (document.Status == InpAdmissionDocumentStatus.Draft)
            {
                var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
                suggestions = await _prefillService.ComputeSuggestionsAsync(
                    document.EpisodeId, gate.EncounterId, InpAdmissionSourceReader.ResolveTimeZone(hospital), cancellationToken);

                var itemIds = items.Select(x => x.ClearanceItemId).ToList();
                sourceByItem = await _dbContext.Set<MstInpatientClearanceItem>()
                    .AsNoTracking()
                    .Where(x => itemIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.HandoverSuggestionSource, cancellationToken);
            }

            return items.Select(x => new AdmissionHandoverItemResponse
            {
                ClearanceItemId = x.ClearanceItemId,
                LineNo = x.LineNo,
                ItemNumber = x.ItemNumberSnapshot,
                ParentItemNumber = x.ParentItemNumberSnapshot,
                Code = x.ItemCodeSnapshot,
                Name = x.ItemNameSnapshot,
                Choice = x.Choice.HasValue ? (int)x.Choice.Value : null,
                Note = x.Note,
                Suggestion = suggestions != null && sourceByItem != null &&
                    sourceByItem.TryGetValue(x.ClearanceItemId, out var source) &&
                    suggestions.TryGetValue(source, out var text)
                    ? new AdmissionSuggestionResponse { Text = text }
                    : null
            }).ToList();
        }

        /// <summary>
        /// Data pasien hidup dibandingkan salinan beku (<c>FR-RWA-124</c>). Data pasien yang gagal
        /// dibaca tidak dianggap berubah.
        /// </summary>
        private async Task<bool> HasSourceChangedAsync(InpAdmissionDocument document, CancellationToken cancellationToken)
        {
            var snapshot = InpAdmissionSnapshotBuilder.Deserialize(document.SnapshotJson);

            if (snapshot == null)
            {
                return false;
            }

            var identity = await _sourceReader.GetPatientIdentityAsync(document.PatientId, cancellationToken);

            if (!identity.IsAvailable)
            {
                return false;
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var today = InpAdmissionText.LocalToday(InpAdmissionSourceReader.ResolveTimeZone(hospital));
            var live = InpAdmissionSnapshotBuilder.BuildPatient(identity.Value!, today);

            return InpAdmissionSnapshotBuilder.PatientChanged(snapshot.Patient, live);
        }

        private async Task<List<string>> ComputeAvailableActionsAsync(
            InpAdmissionDocument document,
            IReadOnlyList<InpAdmissionDocumentSignature> signatures,
            InpAdmissionEpisodeGate gate,
            ClaimsPrincipal user)
        {
            var cache = new Dictionary<string, bool>(StringComparer.Ordinal);

            async Task<bool> Can(string action)
            {
                if (!cache.TryGetValue(action, out var allowed))
                {
                    allowed = await _permissions.HasAccessAsync(user, PermissionResource, action);
                    cache[action] = allowed;
                }

                return allowed;
            }

            var actions = new List<string>();

            // Cetak tetap tersedia pada episode selesai (cetak ulang beralasan, RWI-DEC-240 butir 7).
            if (InpAdmissionDocumentRules.HasAmounts(document.DocumentType))
            {
                if (await Can("ViewAmount") && await Can("Print"))
                {
                    actions.Add("AmountPrint");
                }
            }
            else if (await Can("Print"))
            {
                actions.Add("Print");
            }

            if (!gate.IsWritable)
            {
                return actions;
            }

            var actorUserId = InpatientActorClaims.GetUserId(user);

            switch (document.Status)
            {
                case InpAdmissionDocumentStatus.Draft:
                    if (await Can("Update"))
                    {
                        actions.Add("Update");
                        actions.Add("Lock");

                        if (document.CreateBy == actorUserId)
                        {
                            actions.Add("Discard");
                        }
                    }

                    if (await Can("Cancel"))
                    {
                        actions.Add("Cancel");
                    }

                    break;

                case InpAdmissionDocumentStatus.AwaitingSignature:
                    if (signatures.Count == 0 && await Can("Update"))
                    {
                        actions.Add("Unlock");
                    }

                    var actorSignedStaffSlot = signatures.Any(x => x.SignedByUserId == actorUserId);

                    foreach (var slot in InpAdmissionDocumentRules.RequiredSlots(document.DocumentType))
                    {
                        if (signatures.Any(x => x.Slot == slot))
                        {
                            continue;
                        }

                        if (slot != InpAdmissionSignatureSlot.PatientOrFamily && actorSignedStaffSlot)
                        {
                            continue;
                        }

                        var (permission, action) = SlotAction(slot);

                        if (await Can(permission))
                        {
                            actions.Add(action);
                        }
                    }

                    if (await Can("Cancel"))
                    {
                        actions.Add("Cancel");
                    }

                    break;

                case InpAdmissionDocumentStatus.Completed:
                    if (await Can("Update"))
                    {
                        actions.Add("Revise");
                    }

                    if (await Can("Cancel"))
                    {
                        actions.Add("Cancel");
                    }

                    break;
            }

            return actions;
        }

        /// <summary>Aksi hak akses penjaga slot dan nama aksi tampilannya.</summary>
        internal static (string Permission, string Action) SlotAction(InpAdmissionSignatureSlot slot) => slot switch
        {
            InpAdmissionSignatureSlot.PatientOrFamily => ("Sign", "SignPatientOrFamily"),
            InpAdmissionSignatureSlot.AdmissionOfficer => ("Sign", "SignAdmissionOfficer"),
            InpAdmissionSignatureSlot.CustomerRelationOfficer => ("SignAsCro", "SignCro"),
            InpAdmissionSignatureSlot.ReceivingNurse => ("SignAsNurse", "SignReceivingNurse"),
            _ => ("SignAsHeadNurse", "SignHeadNurse")
        };
    }
}
