using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Isian bawaan dokumen admisi sebelum dibuat — <c>BE-RWI-194</c>, <c>199</c>, <c>200</c>,
    /// <c>201</c>, <c>202</c>; API 12.2 <c>GET /prefill/{documentType}</c>.
    /// </summary>
    /// <remarks>
    /// Isian bawaan hanya membantu; petugas tetap memilih. Saran Serah Terima dibaca dari sumber saran
    /// butir master (<c>HandoverSuggestionSource</c>), bukan dari nomor butir, dan tidak pernah memilih
    /// otomatis (<c>RWI-DEC-241</c> butir 2). Contoh: butir 1 Tn. Budi bersaran "Sudah — saran sistem
    /// (surat pengantar dr. Andika, 07-10-2026)"; butir 12 tidak pernah bersaran selama General Consent
    /// <i>fail-closed</i>.
    /// </remarks>
    public sealed class InpAdmissionPrefillService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionSourceReader _sourceReader;
        private readonly InpAdmissionWriteGuard _writeGuard;

        public InpAdmissionPrefillService(
            ApplicationDbContext dbContext,
            InpAdmissionSourceReader sourceReader,
            InpAdmissionWriteGuard writeGuard)
        {
            _dbContext = dbContext;
            _sourceReader = sourceReader;
            _writeGuard = writeGuard;
        }

        public async Task<InpAdmissionResult<AdmissionDocumentPrefillResponse>> GetPrefillAsync(
            Guid episodeId,
            InpAdmissionDocumentType documentType,
            CancellationToken cancellationToken = default)
        {
            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<AdmissionDocumentPrefillResponse>();
            }

            if (!Enum.IsDefined(documentType))
            {
                return InpAdmissionResult<AdmissionDocumentPrefillResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Jenis dokumen tidak dikenal.");
            }

            if (!InpAdmissionDocumentRules.IsShipped(documentType))
            {
                return InpAdmissionResult<AdmissionDocumentPrefillResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Estimasi Biaya belum tersedia; menunggu keputusan DEC-INP-020.");
            }

            var gate = gateResult.Data!;
            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var today = InpAdmissionText.LocalToday(timeZone);

            var response = new AdmissionDocumentPrefillResponse
            {
                DocumentType = (int)documentType,
                DocumentTypeName = documentType.ToString(),
                CanCreate = gate.IsWritable,
                SigningCity = setting.DocumentSigningCity,
                StatementDate = InpAdmissionDocumentRules.UsesStatementDate(documentType) ? today : null
            };

            if (!gate.IsWritable)
            {
                Deny(response, "EpisodeNotWritable", InpAdmissionWriteGuard.NotWritableMessage);
            }

            var active = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && x.DocumentType == documentType && !x.IsDelete &&
                    (x.Status == InpAdmissionDocumentStatus.Draft ||
                     x.Status == InpAdmissionDocumentStatus.AwaitingSignature ||
                     x.Status == InpAdmissionDocumentStatus.Completed))
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (active.HasValue)
            {
                Deny(response, "ActiveDocumentExists",
                    $"Sudah ada {InpAdmissionDocumentRules.DocumentName(documentType)} yang aktif untuk episode ini. Buka dokumen itu atau buat versi koreksi.");
                response.ActiveDocumentId = active;
            }

            if (InpAdmissionDocumentRules.HasParty(documentType))
            {
                var candidates = await _sourceReader.GetPartyCandidatesAsync(gate.PatientId, cancellationToken);

                if (candidates.IsAvailable)
                {
                    response.PartyCandidates = candidates.Value!.Select(ToCandidateResponse).ToList();
                }
                else
                {
                    // Daftar kosong dengan pesan; petugas tetap dapat mengisi manual (INT-RWA-02).
                    response.Warnings.Add(candidates.Reason ?? "Data wali dan kontak darurat tidak dapat dimuat. Isi manual.");
                }
            }

            switch (documentType)
            {
                case InpAdmissionDocumentType.NewPatientHandover:
                    var lines = await BuildHandoverLinesAsync(cancellationToken);

                    if (lines.Count == 0)
                    {
                        Deny(response, "HandoverItemsMissing", "Butir serah terima belum diatur. Hubungi admin.");
                    }

                    var suggestions = await ComputeSuggestionsAsync(episodeId, gate.EncounterId, timeZone, cancellationToken);
                    response.HandoverItems = lines.Select(x => ToHandoverResponse(x, suggestions)).ToList();
                    break;

                case InpAdmissionDocumentType.BeliefValues:
                    var previous = await FindPreviousBeliefDocumentAsync(gate.PatientId, episodeId, cancellationToken);

                    if (previous != null)
                    {
                        response.PreviousBeliefDocumentId = previous.Value.DocumentId;
                        response.PreviousBeliefItems = previous.Value.Items;
                    }

                    break;

                case InpAdmissionDocumentType.CostDifferenceStatement:
                    var guarantor = await _sourceReader.GetGuarantorAsync(gate.EncounterId, cancellationToken);

                    if (!guarantor.IsAvailable)
                    {
                        Deny(response, "GuarantorUnavailable",
                            "Data penjamin kunjungan tidak dapat dibaca. Selisih Biaya belum dapat dibuat; coba lagi.");
                    }
                    else if (guarantor.Value!.PaymentType is not (EncounterPaymentType.Insurance or EncounterPaymentType.CompanyGuarantor))
                    {
                        Deny(response, "NotRequiredForPayer", "Selisih Biaya hanya untuk pasien dengan penjamin asuransi atau perusahaan.");
                    }

                    var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);

                    if (identity.IsAvailable)
                    {
                        response.PatientAsDeclarer = BuildPatientAsDeclarer(identity.Value!);
                    }

                    break;

                case InpAdmissionDocumentType.DepositSettlementStatement:
                    var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);

                    if (!deposit.IsAvailable)
                    {
                        Deny(response, "DepositUnavailable", "Data deposit tidak dapat dibaca dari kasir. Coba lagi beberapa saat.");
                    }
                    else
                    {
                        if (deposit.Value!.PolicyShortfallAmount <= 0)
                        {
                            Deny(response, "NoDepositShortfall",
                                "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan.");
                        }

                        response.DefaultDueDate = InpDepositDueDateCalculator.DefaultDueDate(today, deposit.Value.FollowUpIntervalDays);
                        response.MaxDueDate = InpDepositDueDateCalculator.MaxDueDate(today, deposit.Value.FollowUpIntervalDays);
                    }

                    break;
            }

            return InpAdmissionResult<AdmissionDocumentPrefillResponse>.Ok(response, "Isian bawaan dokumen admisi berhasil diambil.");
        }

        /// <summary>
        /// Butir aktif jenis serah terima dalam urutan lembar: butir utama menurut <c>SortOrder</c>,
        /// masing-masing langsung diikuti sub-butirnya. Butir utama bernomor 1..n; sub-butir tidak
        /// bernomor dan menyebut nomor induknya.
        /// </summary>
        public async Task<List<InpAdmissionHandoverLine>> BuildHandoverLinesAsync(CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ChecklistType == MstClearanceChecklistType.NewPatientHandover)
                .Select(x => new
                {
                    x.Id,
                    x.ItemCode,
                    x.ItemName,
                    x.SortOrder,
                    x.ParentItemId,
                    x.HandoverSuggestionSource
                })
                .ToListAsync(cancellationToken);

            var ids = items.Select(x => x.Id).ToHashSet();

            // Sub-butir yang induknya tidak aktif dicetak sebagai butir utama, bukan dihilangkan.
            var roots = items
                .Where(x => !x.ParentItemId.HasValue || !ids.Contains(x.ParentItemId.Value))
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ItemName)
                .ToList();

            var lines = new List<InpAdmissionHandoverLine>();
            var itemNumber = 0;
            var lineNo = 0;

            foreach (var root in roots)
            {
                itemNumber++;
                lines.Add(new InpAdmissionHandoverLine(root.Id, ++lineNo, itemNumber, null, root.ItemCode, root.ItemName,
                    root.HandoverSuggestionSource));

                foreach (var child in items
                    .Where(x => x.ParentItemId == root.Id)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.ItemName))
                {
                    lines.Add(new InpAdmissionHandoverLine(child.Id, ++lineNo, null, itemNumber, child.ItemCode, child.ItemName,
                        child.HandoverSuggestionSource));
                }
            }

            return lines;
        }

        /// <summary>
        /// Saran sistem per sumber saran (<c>RWI-DEC-241</c>, <c>262</c>). Estimasi Biaya diam sampai
        /// <c>BE-RWI-203</c>; Label/General Consent tanpa saran selama <i>fail-closed</i>.
        /// </summary>
        public async Task<Dictionary<MstHandoverSuggestionSource, string>> ComputeSuggestionsAsync(
            Guid episodeId,
            Guid encounterId,
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default)
        {
            var suggestions = new Dictionary<MstHandoverSuggestionSource, string>();

            var letter = await _sourceReader.GetInpatientReferralLetterAsync(encounterId, cancellationToken);

            if (letter.IsAvailable && letter.Value != null)
            {
                var doctor = string.IsNullOrWhiteSpace(letter.Value.DoctorName) ? "dokter" : letter.Value.DoctorName.Trim();
                suggestions[MstHandoverSuggestionSource.ReferralLetter] =
                    $"Sudah — saran sistem (surat pengantar {doctor}, {InpAdmissionText.FormatDate(letter.Value.IssuedDate)})";
            }

            var completedDepositAt = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete &&
                    x.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement &&
                    x.Status == InpAdmissionDocumentStatus.Completed)
                .Select(x => x.CompletedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (completedDepositAt.HasValue)
            {
                suggestions[MstHandoverSuggestionSource.DepositStatementCompleted] =
                    $"Sudah — saran sistem (pernyataan pelunasan deposit lengkap {InpAdmissionText.FormatDateTime(completedDepositAt.Value, timeZone)})";
            }

            var firstPrints = await _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete &&
                    (x.PrintKind == InpAdmissionPrintKind.InpatientBaseData ||
                     x.PrintKind == InpAdmissionPrintKind.AdultWristband ||
                     x.PrintKind == InpAdmissionPrintKind.InfantWristband))
                .OrderBy(x => x.PrintedAt)
                .Select(x => new { x.PrintKind, x.PrintedAt, x.PrintedByUserId })
                .ToListAsync(cancellationToken);

            var users = await _sourceReader.GetUsersAsync(firstPrints.Select(x => x.PrintedByUserId), cancellationToken);

            string PrintedBy(Guid userId)
                => users.TryGetValue(userId, out var user) && !string.IsNullOrWhiteSpace(user.DisplayName)
                    ? $" oleh {user.DisplayName}"
                    : string.Empty;

            var baseData = firstPrints.FirstOrDefault(x => x.PrintKind == InpAdmissionPrintKind.InpatientBaseData);

            if (baseData != null)
            {
                suggestions[MstHandoverSuggestionSource.BaseDataPrinted] =
                    $"Sudah — saran sistem (IPD dicetak {InpAdmissionText.FormatDateTime(baseData.PrintedAt, timeZone)}{PrintedBy(baseData.PrintedByUserId)})";
            }

            var wristband = firstPrints.FirstOrDefault(x =>
                x.PrintKind is InpAdmissionPrintKind.AdultWristband or InpAdmissionPrintKind.InfantWristband);

            if (wristband != null)
            {
                suggestions[MstHandoverSuggestionSource.WristbandPrinted] =
                    $"Sudah — saran sistem (gelang dicetak {InpAdmissionText.FormatDateTime(wristband.PrintedAt, timeZone)}{PrintedBy(wristband.PrintedByUserId)})";
            }

            return suggestions;
        }

        /// <summary>
        /// Nilai Kepercayaan <c>Completed</c> terakhir pasien pada episode lain (<c>RWI-DEC-242</c>).
        /// Butirnya menjadi konsep episode baru dan tidak terhitung lengkap sampai ditandatangani ulang.
        /// </summary>
        public async Task<(Guid DocumentId, List<string> Items)?> FindPreviousBeliefDocumentAsync(
            Guid patientId,
            Guid currentEpisodeId,
            CancellationToken cancellationToken = default)
        {
            var previous = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId && x.EpisodeId != currentEpisodeId && !x.IsDelete &&
                    x.DocumentType == InpAdmissionDocumentType.BeliefValues &&
                    x.Status == InpAdmissionDocumentStatus.Completed)
                .OrderByDescending(x => x.CompletedAt)
                .Select(x => new
                {
                    x.Id,
                    Items = x.BeliefItems.Where(i => !i.IsDelete).OrderBy(i => i.ItemNo).Select(i => i.Text).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            return previous == null ? null : (previous.Id, previous.Items);
        }

        /// <summary>Deklarer "diri saya sendiri" dari master pasien (<c>BE-RWI-201</c>).</summary>
        public static AdmissionPartyResponse BuildPatientAsDeclarer(PatientIdentityProfile identity)
        {
            var phone = InpAdmissionText.NormalizePhone(identity.PhoneNumber);
            var identityType = MapIdentityType(identity.IdentityType);

            return new AdmissionPartyResponse
            {
                SourceType = (int)InpAdmissionPartySource.Patient,
                SourceTypeName = InpAdmissionPartySource.Patient.ToString(),
                FullName = identity.FullName,
                Relationship = (int)InpAdmissionPartyRelationship.Self,
                RelationshipName = InpAdmissionPartyRelationship.Self.ToString(),
                Address = InpAdmissionSnapshotBuilder.ComposeAddress(identity),
                BirthDate = identity.BirthDate,
                Gender = identity.Gender.HasValue ? (int)identity.Gender.Value : null,
                IdentityType = identityType.HasValue ? (int)identityType.Value : null,
                IdentityNumber = identity.IdentityNumber,
                MobilePhone = InpAdmissionText.IsValidPhone(phone, 13) ? phone : null
            };
        }

        /// <summary>Jenis tanda pengenal master pasien (teks bebas) ke pilihan V1; tidak dikenal → kosong.</summary>
        public static InpAdmissionPartyIdentityType? MapIdentityType(string? value)
        {
            var normalized = value?.Trim().ToUpperInvariant().Replace(" ", string.Empty);

            return normalized switch
            {
                "KTP" or "NIK" or "EKTP" or "E-KTP" => InpAdmissionPartyIdentityType.Ktp,
                "SIM" => InpAdmissionPartyIdentityType.Sim,
                "PASPOR" or "PASSPORT" => InpAdmissionPartyIdentityType.Passport,
                "IDCARD" or "KARTUIDENTITAS" => InpAdmissionPartyIdentityType.IdCard,
                _ => null
            };
        }

        public static AdmissionPartyCandidateResponse ToCandidateResponse(PatientPartyCandidate candidate)
            => new()
            {
                Source = candidate.Source.ToString(),
                SourceRecordId = candidate.SourceRecordId,
                Name = candidate.Name,
                RelationshipType = candidate.RelationshipType?.ToString(),
                RelationshipText = candidate.RelationshipText,
                Address = candidate.Address,
                PhoneNumber = candidate.PhoneNumber,
                IsPrimary = candidate.IsPrimary,
                IsResponsiblePerson = candidate.IsResponsiblePerson
            };

        public static AdmissionHandoverItemResponse ToHandoverResponse(
            InpAdmissionHandoverLine line,
            IReadOnlyDictionary<MstHandoverSuggestionSource, string>? suggestions)
            => new()
            {
                ClearanceItemId = line.ClearanceItemId,
                LineNo = line.LineNo,
                ItemNumber = line.ItemNumber,
                ParentItemNumber = line.ParentItemNumber,
                Code = line.Code,
                Name = line.Name,
                Suggestion = suggestions != null && suggestions.TryGetValue(line.SuggestionSource, out var text)
                    ? new AdmissionSuggestionResponse { Text = text }
                    : null
            };

        private static void Deny(AdmissionDocumentPrefillResponse response, string code, string reason)
        {
            response.CanCreate = false;

            // Alasan pertama yang paling menentukan dipertahankan.
            if (response.CannotCreateReasonCode == null)
            {
                response.CannotCreateReasonCode = code;
                response.CannotCreateReason = reason;
            }
        }
    }

    /// <summary>Satu baris serah terima beserta sumber sarannya.</summary>
    public sealed record InpAdmissionHandoverLine(
        Guid ClearanceItemId,
        int LineNo,
        int? ItemNumber,
        int? ParentItemNumber,
        string Code,
        string Name,
        MstHandoverSuggestionSource SuggestionSource);
}
