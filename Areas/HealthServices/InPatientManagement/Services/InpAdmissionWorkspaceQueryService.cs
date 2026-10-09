using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using PatientRelationshipType = QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums.PatientRelationshipType;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Bacaan Workspace PPRI tanpa siklus dokumen — <c>BE-RWI-193</c> (kop), <c>195</c> (ringkasan dan
    /// rupiah header), <c>196</c> (gelang dan label), <c>197</c> (IPD), <c>198</c> (data cetak General
    /// Consent), <c>200</c> (ringkasan hak pasien). Seluruhnya <c>AsNoTracking</c> dan tidak menulis.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada data karangan</b> (<c>INV-RWA-07</c>): nomor kartu kosong tetap kosong, isian IPD
    /// tanpa sumber masuk <c>BlankFields</c> untuk dicetak garis kosong, dan kop tanpa profil rumah
    /// sakit dicetak tanpa identitas. Respons yang dijaga <c>Read</c> tidak pernah memuat rupiah.
    /// </remarks>
    public sealed class InpAdmissionWorkspaceQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionWriteGuard _writeGuard;
        private readonly InpAdmissionSourceReader _sourceReader;
        private readonly InpAdmissionCompletenessEvaluator _completeness;
        private readonly InpPatientLocationQuery _locationQuery;

        public InpAdmissionWorkspaceQueryService(
            ApplicationDbContext dbContext,
            InpAdmissionWriteGuard writeGuard,
            InpAdmissionSourceReader sourceReader,
            InpAdmissionCompletenessEvaluator completeness,
            InpPatientLocationQuery locationQuery)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _sourceReader = sourceReader;
            _completeness = completeness;
            _locationQuery = locationQuery;
        }

        // =====================================================================
        // Kop surat (GET /letterhead) — episode status apa pun, termasuk Draft
        // =====================================================================

        public async Task<InpAdmissionResult<LetterheadResponse>> GetLetterheadAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.LoadAsync(episodeId, cancellationToken);

            if (gate == null)
            {
                return InpAdmissionResult<LetterheadResponse>.Fail(
                    StatusCodes.Status404NotFound, InpAdmissionWriteGuard.EpisodeNotFoundMessage);
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);

            return InpAdmissionResult<LetterheadResponse>.Ok(
                InpAdmissionSnapshotBuilder.ToLetterheadResponse(InpAdmissionSnapshotBuilder.BuildLetterhead(hospital)),
                hospital.IsAvailable ? "Kop surat berhasil diambil." : "Profil rumah sakit tidak tersedia; kop dicetak tanpa identitas.");
        }

        // =====================================================================
        // Ringkasan ruang kerja (GET /summary) — tanpa rupiah
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionWorkspaceSummaryResponse>> GetSummaryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.LoadAsync(episodeId, cancellationToken);

            if (gate == null)
            {
                return InpAdmissionResult<AdmissionWorkspaceSummaryResponse>.Fail(
                    StatusCodes.Status404NotFound, InpAdmissionWriteGuard.EpisodeNotFoundMessage);
            }

            var response = new AdmissionWorkspaceSummaryResponse { EpisodeId = episodeId };

            // G-30: admisi yang belum dikonfirmasi belum membuka Workspace PPRI.
            if (gate.Status == InpEpisodeStatus.Draft)
            {
                response.Availability = "NotYetAdmitted";
                response.Warnings.Add(InpAdmissionWriteGuard.NotAdmittedMessage);
                return InpAdmissionResult<AdmissionWorkspaceSummaryResponse>.Ok(response, "Ringkasan Workspace PPRI berhasil diambil.");
            }

            if (gate.IsClosedOrCancelled)
            {
                response.Availability = "ReadOnly";
                response.ReadOnlyReason = gate.Status == InpEpisodeStatus.Cancelled ? "EpisodeCancelled" : "EpisodeClosed";
            }

            var episode = await _sourceReader.GetEpisodeAsync(episodeId, cancellationToken);
            var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);
            var allergies = await _sourceReader.GetActiveAllergiesAsync(gate.PatientId, cancellationToken);
            var guarantor = await _sourceReader.GetGuarantorAsync(gate.EncounterId, cancellationToken);
            var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var candidates = await _sourceReader.GetPartyCandidatesAsync(gate.PatientId, cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var today = InpAdmissionText.LocalToday(timeZone);

            var completeness = await _completeness.EvaluateAsync(episodeId, gate.EncounterId, guarantor, deposit, cancellationToken);
            var rights = await LoadPatientRightsAsync(episodeId, cancellationToken);

            response.Sources = new AdmissionWorkspaceSourcesResponse
            {
                Patient = identity.State.ToString(),
                Guarantor = guarantor.State.ToString(),
                Allergy = allergies.State.ToString(),
                Deposit = deposit.State.ToString(),
                // Aturan wajib Estimasi Biaya belum dikirim (EPIC-RWA-09, BE-RWI-203).
                OperatingRoom = InpAdmissionSourceState.NotYetAvailable.ToString(),
                HospitalProfile = hospital.State.ToString()
            };

            if (episode.IsAvailable)
            {
                var context = episode.Value!;
                var patient = identity.Value;

                response.Header = new AdmissionWorkspaceHeaderResponse
                {
                    PatientName = patient?.FullName,
                    Salutation = patient == null
                        ? null
                        : InpWristbandRules.ResolveSalutation(patient.IsNewborn, patient.Gender, patient.MaritalStatus, patient.BirthDate, today),
                    MedicalRecordNumber = patient?.MedicalRecordNumber,
                    GenderName = patient == null ? null : InpAdmissionText.GenderName(patient.Gender),
                    BirthDate = patient?.BirthDate,
                    AgeText = patient == null ? null : InpWristbandRules.AgeText(patient.BirthDate, today),
                    EpisodeNumber = context.EpisodeNumber,
                    AdmittedAt = context.AdmittedAt,
                    PatientClassName = context.DisplayPatientClassName,
                    ServiceUnitName = context.DisplayServiceUnitName,
                    RoomName = context.CurrentPlacement?.RoomName,
                    BedName = context.CurrentPlacement?.BedName,
                    IsOccupyingBed = await _locationQuery.HasActivePlacementAsync(episodeId, cancellationToken),
                    AttendingDoctorName = context.AttendingDoctorName,
                    PaymentTypeName = guarantor.Value?.PaymentTypeName,
                    GuarantorName = guarantor.Value == null ? null : GuarantorName(guarantor.Value),
                    CardNumber = guarantor.Value?.CardNumber,
                    PrimaryEmergencyContact = PrimaryContact(candidates),
                    ActiveAllergyNames = allergies.IsAvailable
                        ? allergies.Value!.Select(x => x.AllergenName).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList()
                        : null,
                    RequiresIsolation = context.RequiresIsolation,
                    PatientRightsSummaryText = rights.SummaryText
                };
            }

            response.Menus = completeness.Menus.Select(x => new AdmissionWorkspaceMenuResponse
            {
                Key = x.Key,
                Label = x.Label,
                Badge = x.Badge,
                ActiveDocumentId = x.ActiveDocumentId,
                IsRequired = x.IsRequired
            }).ToList();

            response.Completeness = new AdmissionWorkspaceCompletenessResponse
            {
                CompletedCount = completeness.CompletedCount,
                RequiredCount = completeness.RequiredCount,
                UncountableCount = completeness.UncountableCount,
                MissingNames = completeness.MissingNames.ToList(),
                UncountableNames = completeness.UncountableNames.ToList()
            };

            response.Warnings.AddRange(InpAdmissionCompletenessEvaluator.BuildWarnings(completeness, timeZone));

            return InpAdmissionResult<AdmissionWorkspaceSummaryResponse>.Ok(response, "Ringkasan Workspace PPRI berhasil diambil.");
        }

        /// <summary>Status deposit berupiah dan jatuh tempo terlewati — hanya <c>ViewAmount</c>.</summary>
        public async Task<InpAdmissionResult<AdmissionWorkspaceAmountsResponse>> GetSummaryAmountsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<AdmissionWorkspaceAmountsResponse>();
            }

            var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
            var response = new AdmissionWorkspaceAmountsResponse();

            if (!deposit.IsAvailable)
            {
                // Billing gagal → "Unavailable", angka tidak ditebak (G-45).
                response.DepositStatus = "Unavailable";
                return InpAdmissionResult<AdmissionWorkspaceAmountsResponse>.Ok(response, deposit.Reason ?? "Data deposit tidak dapat dibaca dari kasir.");
            }

            var summary = deposit.Value!;
            response.MinimumPolicyAmount = summary.MinimumPolicyAmount;
            response.ReceivedAmount = summary.TotalReceived;
            response.ShortfallAmount = summary.PolicyShortfallAmount;
            response.ReadAt = DateTime.UtcNow;
            response.DepositStatus = !summary.IsPolicyRequired
                ? "NotRequired"
                : summary.PolicyShortfallAmount > 0 ? "Shortfall" : "Sufficient";

            var completed = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete &&
                    x.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement &&
                    x.Status == InpAdmissionDocumentStatus.Completed &&
                    x.DepositStatement != null && x.DepositStatement.DueAt != null)
                .Select(x => new { x.Id, DueAt = x.DepositStatement!.DueAt })
                .FirstOrDefaultAsync(cancellationToken);

            // RWI-DEC-260: jatuh tempo surat lengkap terlewati dan Billing masih mencatat kekurangan.
            if (completed != null && summary.PolicyShortfallAmount > 0 && DateTime.UtcNow > completed.DueAt!.Value)
            {
                response.OverdueStatement = new AdmissionOverdueStatementResponse
                {
                    DocumentId = completed.Id,
                    DueAt = completed.DueAt.Value,
                    CurrentShortfallAmount = summary.PolicyShortfallAmount
                };
            }

            return InpAdmissionResult<AdmissionWorkspaceAmountsResponse>.Ok(response, "Status deposit berhasil diambil.");
        }

        // =====================================================================
        // General Consent cetak saja (GET /general-consent/print-data) — tanpa tulis apa pun
        // =====================================================================

        public async Task<InpAdmissionResult<GeneralConsentPrintDataResponse>> GetGeneralConsentPrintDataAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<GeneralConsentPrintDataResponse>();
            }

            var gate = gateResult.Data!;
            var episode = await _sourceReader.GetEpisodeAsync(episodeId, cancellationToken);
            var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);

            if (!episode.IsAvailable || !identity.IsAvailable)
            {
                return InpAdmissionResult<GeneralConsentPrintDataResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Data cetak tidak dapat dimuat. Coba lagi.");
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var guarantor = await _sourceReader.GetGuarantorAsync(gate.EncounterId, cancellationToken);
            var candidates = await _sourceReader.GetPartyCandidatesAsync(gate.PatientId, cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var today = InpAdmissionText.LocalToday(timeZone);
            var context = episode.Value!;
            var patient = identity.Value!;
            var (roomType, roomTypeReason) = ResolveRoomType(context);

            var response = new GeneralConsentPrintDataResponse
            {
                Letterhead = InpAdmissionSnapshotBuilder.ToLetterheadResponse(InpAdmissionSnapshotBuilder.BuildLetterhead(hospital)),
                FormCode = setting.GeneralConsentFormCode,
                SigningCity = setting.DocumentSigningCity,
                PrintDate = today,
                Patient = new GeneralConsentPatientResponse
                {
                    FullName = patient.FullName,
                    Salutation = InpWristbandRules.ResolveSalutation(patient.IsNewborn, patient.Gender, patient.MaritalStatus, patient.BirthDate, today),
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    BirthDate = patient.BirthDate,
                    AgeText = InpWristbandRules.AgeText(patient.BirthDate, today),
                    GenderName = InpAdmissionText.GenderName(patient.Gender),
                    PhoneNumber = patient.PhoneNumber,
                    Address = InpAdmissionSnapshotBuilder.ComposeAddress(patient)
                },
                Episode = new GeneralConsentEpisodeResponse
                {
                    EpisodeNumber = context.EpisodeNumber,
                    EncounterNumber = context.EncounterNumber,
                    AdmittedAt = context.AdmittedAt,
                    PatientClassName = context.DisplayPatientClassName,
                    ServiceUnitName = context.DisplayServiceUnitName,
                    RoomName = context.CurrentPlacement?.RoomName,
                    BedName = context.CurrentPlacement?.BedName,
                    AttendingDoctorName = context.AttendingDoctorName
                },
                RoomType = roomType,
                RoomTypeReason = roomTypeReason,
                Guarantor = guarantor.IsAvailable
                    ? new GeneralConsentGuarantorResponse
                    {
                        PaymentTypeName = guarantor.Value!.PaymentTypeName,
                        GuarantorName = GuarantorName(guarantor.Value),
                        CardNumber = guarantor.Value.CardNumber,
                        MemberNumber = guarantor.Value.MemberNumber,
                        PolicyNumber = guarantor.Value.PolicyNumber
                    }
                    : null
            };

            if (!guarantor.IsAvailable)
            {
                response.Warnings.Add(guarantor.Reason ?? "Data penjamin kunjungan tidak dapat dibaca.");
            }

            // "Diri Sendiri" → master pasien; relasi dengan jenis terstruktur; kontak darurat hanya
            // daftar beserta teks hubungannya (RWI-DEC-252). Server tidak memilih diam-diam.
            response.SignerCandidates.Add(new GeneralConsentSignerCandidateResponse
            {
                Source = InpAdmissionPartySource.Patient.ToString(),
                Name = patient.FullName,
                RelationshipType = "Self",
                Address = InpAdmissionSnapshotBuilder.ComposeAddress(patient)
            });

            if (candidates.IsAvailable)
            {
                response.SignerCandidates.AddRange(candidates.Value!.Select(x => new GeneralConsentSignerCandidateResponse
                {
                    Source = x.Source.ToString(),
                    SourceRecordId = x.SourceRecordId,
                    Name = x.Name,
                    RelationshipType = x.RelationshipType?.ToString(),
                    RelationshipText = x.RelationshipText,
                    Address = x.Address
                }));
            }
            else
            {
                response.Warnings.Add("Tidak ditemukan di data wali/kontak darurat. Isi manual.");
            }

            return InpAdmissionResult<GeneralConsentPrintDataResponse>.Ok(response, "Data cetak General Consent berhasil diambil.");
        }

        // =====================================================================
        // Gelang dan label (GET /identity-labels)
        // =====================================================================

        public async Task<InpAdmissionResult<IdentityLabelResponse>> GetIdentityLabelsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<IdentityLabelResponse>();
            }

            var gate = gateResult.Data!;
            var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);

            if (!identity.IsAvailable)
            {
                return InpAdmissionResult<IdentityLabelResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Data gelang tidak dapat dimuat. Cetak ditahan sampai data wajib terbaca lengkap.",
                    InpAdmissionCodes.BaseDataPrintBlocked);
            }

            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var guarantor = await _sourceReader.GetGuarantorAsync(gate.EncounterId, cancellationToken);
            var today = InpAdmissionText.LocalToday(InpAdmissionSourceReader.ResolveTimeZone(hospital));
            var patient = identity.Value!;

            var kind = InpWristbandRules.ResolveKind(patient.IsNewborn, patient.BirthDate, today, setting.InfantWristbandMaxAgeYears);
            var ageText = InpWristbandRules.AgeText(patient.BirthDate, today);
            var hospitalCode = setting.PatientLabelHospitalCode ?? hospital.Value?.SiteCode;
            var name = patient.FullName.Trim().ToUpperInvariant();

            var counts = await _dbContext.Set<InpAdmissionPrintLog>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete &&
                    (x.PrintKind == InpAdmissionPrintKind.AdultWristband ||
                     x.PrintKind == InpAdmissionPrintKind.InfantWristband ||
                     x.PrintKind == InpAdmissionPrintKind.PatientLabel))
                .GroupBy(x => x.PrintKind)
                .Select(g => new { Kind = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int CountOf(InpAdmissionPrintKind printKind) => counts.FirstOrDefault(x => x.Kind == printKind)?.Count ?? 0;

            var response = new IdentityLabelResponse
            {
                Wristband = new IdentityWristbandResponse
                {
                    Kind = kind.ToString(),
                    PrintKind = (int)(kind == InpWristbandKind.Infant
                        ? InpAdmissionPrintKind.InfantWristband
                        : InpAdmissionPrintKind.AdultWristband),
                    DisplayName = InpWristbandRules.BuildDisplayName(
                        patient.FullName, patient.IsNewborn, patient.MotherName, patient.Gender, patient.MaritalStatus, patient.BirthDate, today),
                    BirthDateText = patient.BirthDate.HasValue ? InpAdmissionText.FormatShortDate(patient.BirthDate.Value) : null,
                    AgeText = ageText,
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    QrPayload = patient.QrPayload,
                    SmallLabelCount = kind == InpWristbandKind.Infant ? InpWristbandRules.InfantSmallLabelCount : 0
                },
                PatientLabel = new IdentityPatientLabelResponse
                {
                    HospitalCode = hospitalCode,
                    NameLine = string.IsNullOrWhiteSpace(hospitalCode) ? name : $"{name} / {hospitalCode}",
                    BirthDateShort = patient.BirthDate?.ToString("dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture),
                    GenderAgeText = string.Join(" / ", new[] { InpAdmissionText.GenderInitial(patient.Gender), ageText }
                        .Where(x => !string.IsNullOrWhiteSpace(x))),
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    // RWI-DEC-253: hanya CardNumberSnapshot; tanpa cadangan nomor peserta atau polis.
                    CardNumber = guarantor.IsAvailable ? guarantor.Value!.CardNumber : null,
                    QrPayload = patient.QrPayload
                },
                PrintCounts = new IdentityPrintCountsResponse
                {
                    AdultWristband = CountOf(InpAdmissionPrintKind.AdultWristband),
                    InfantWristband = CountOf(InpAdmissionPrintKind.InfantWristband),
                    PatientLabel = CountOf(InpAdmissionPrintKind.PatientLabel)
                }
            };

            return InpAdmissionResult<IdentityLabelResponse>.Ok(response, "Data gelang dan label berhasil diambil.");
        }

        // =====================================================================
        // Data Dasar Rawat Inap / IPD (GET /base-data, /base-data/amounts)
        // =====================================================================

        public async Task<InpAdmissionResult<InpatientBaseDataResponse>> GetBaseDataAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gateResult = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<InpatientBaseDataResponse>();
            }

            var gate = gateResult.Data!;
            var episode = await _sourceReader.GetEpisodeAsync(episodeId, cancellationToken);
            var identity = await _sourceReader.GetPatientIdentityAsync(gate.PatientId, cancellationToken);
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var guarantor = await _sourceReader.GetGuarantorAsync(gate.EncounterId, cancellationToken);
            var letter = await _sourceReader.GetInpatientReferralLetterAsync(gate.EncounterId, cancellationToken);
            var externalReferral = await _sourceReader.GetExternalReferralAsync(gate.EncounterId, cancellationToken);
            var candidates = await _sourceReader.GetPartyCandidatesAsync(gate.PatientId, cancellationToken);
            var rights = await LoadPatientRightsAsync(episodeId, cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var today = InpAdmissionText.LocalToday(timeZone);

            var response = new InpatientBaseDataResponse
            {
                EpisodeId = episodeId,
                Letterhead = InpAdmissionSnapshotBuilder.ToLetterheadResponse(InpAdmissionSnapshotBuilder.BuildLetterhead(hospital)),
                FormCode = setting.InpatientBaseDataFormCode,
                // FR-RWA-072: cetak ditahan bila identitas pasien atau data episode gagal terbaca.
                CanPrint = identity.IsAvailable && episode.IsAvailable,
                CannotPrintReason = identity.IsAvailable && episode.IsAvailable
                    ? null
                    : "Cetak ditahan sampai data wajib terbaca lengkap."
            };

            var patient = identity.Value;
            var context = episode.Value;
            var payer = guarantor.Value;

            // Isian yang memang tidak punya sumber di Final (RWI-DEC-244): selalu garis kosong.
            response.BlankFields.AddRange(new[]
            {
                "KtpRtRw", "KtpVillage", "DomicileAddress", "Occupation", "OfficeAddress", "OfficePhone",
                "Nationality", "MutationNumber", "InformationRecipient", "DirectorApproval", "SpecialAttention",
                "Cashier", "ResponsiblePersonDomicileAddress"
            });

            // Diagnosis masuk dan dokter perujuk (RWI-DEC-254): surat terbit terbaru, lalu perujuk luar.
            string? referringDoctor = null;
            string? diagnosisAndPlan = null;

            if (letter.IsAvailable && letter.Value != null)
            {
                referringDoctor = InpAdmissionText.Clean(letter.Value.DoctorName);
                diagnosisAndPlan = string.Join(" — ", new[] { letter.Value.ReferralDiagnosis, letter.Value.ReferralReason }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim()));
                diagnosisAndPlan = string.IsNullOrWhiteSpace(diagnosisAndPlan) ? null : diagnosisAndPlan;
            }
            else if (externalReferral.IsAvailable && externalReferral.Value != null)
            {
                referringDoctor = string.IsNullOrWhiteSpace(externalReferral.Value.InstitutionName)
                    ? externalReferral.Value.DoctorName
                    : $"{externalReferral.Value.DoctorName} ({externalReferral.Value.InstitutionName})";
            }

            if (!letter.IsAvailable)
            {
                response.Warnings.Add(letter.Reason ?? "Surat pengantar tidak dapat dimuat.");
            }

            var responsible = ResponsiblePerson(candidates);

            response.Left = new InpatientBaseDataLeftResponse
            {
                PatientName = patient?.FullName,
                MedicalRecordNumber = patient?.MedicalRecordNumber,
                OwnName = patient?.NickName,
                FullName = patient?.FullName,
                IdentityText = patient == null ? null : JoinText(patient.IdentityType, patient.IdentityNumber),
                BirthDate = patient?.BirthDate,
                KtpAddress = patient?.Address,
                KtpDistrict = patient?.DistrictName,
                KtpCity = patient?.CityName,
                KtpPostalCode = patient?.PostalCode,
                Email = patient?.Email,
                Religion = patient == null || patient.Religion == Religion.Unknown ? null : InpAdmissionText.ReligionName(patient.Religion),
                AdmittedAt = context?.AdmittedAt,
                RoomText = context?.CurrentPlacement == null
                    ? null
                    : JoinText(context.CurrentPlacement.RoomName, context.CurrentPlacement.BedName == null ? null : $"Bed {context.CurrentPlacement.BedName}", " / "),
                PatientClassName = context?.DisplayPatientClassName,
                ReferringDoctor = referringDoctor,
                AttendingDoctor = context?.AttendingDoctorName
            };

            response.Right = new InpatientBaseDataRightResponse
            {
                GuarantorName = payer == null ? null : GuarantorName(payer),
                PaymentCategory = payer == null ? null : PaymentCategory(payer.PaymentType),
                CompanyOrInsurerName = payer?.InsuranceProviderName ?? payer?.CompanyGuarantorName,
                MemberNumber = payer?.MemberNumber,
                PolicyNumber = payer?.PolicyNumber,
                ResponsiblePerson = responsible,
                AdmissionDiagnosisAndPlan = diagnosisAndPlan,
                PlannedClassName = context?.DisplayPatientClassName,
                PaymentMethod = payer?.PaymentTypeName,
                AdmissionOfficer = context?.AdmissionConfirmedByName,
                FloorNurse = context?.FloorNurseName,
                RoomTransfers = context == null
                    ? new List<InpatientBaseDataTransferResponse>()
                    : context.Placements.Skip(1).Select(x => new InpatientBaseDataTransferResponse
                    {
                        MovedAt = x.StartDateTime,
                        ServiceUnitName = x.ServiceUnitName,
                        PatientClassName = x.PatientClassName,
                        RoomName = x.RoomName,
                        BedName = x.BedName
                    }).ToList(),
                BeliefValues = rights.BeliefValues?.Items ?? new List<string>(),
                PrivacyText = rights.Privacy == null ? null : PrivacySummary(rights.Privacy)
            };

            response.Footer = new InpatientBaseDataFooterResponse
            {
                HospitalCode = setting.PatientLabelHospitalCode ?? hospital.Value?.SiteCode,
                LetterNumber = context?.EpisodeNumber ?? string.Empty,
                PrintDate = today
            };

            // Isian bersumber yang kebetulan kosong juga dicetak garis kosong, bukan diisi tebakan.
            AddBlankIfEmpty(response.BlankFields, "OwnName", response.Left.OwnName);
            AddBlankIfEmpty(response.BlankFields, "IdentityText", response.Left.IdentityText);
            AddBlankIfEmpty(response.BlankFields, "Email", response.Left.Email);
            AddBlankIfEmpty(response.BlankFields, "Religion", response.Left.Religion);
            AddBlankIfEmpty(response.BlankFields, "ReferringDoctor", response.Left.ReferringDoctor);
            AddBlankIfEmpty(response.BlankFields, "AdmissionDiagnosisAndPlan", response.Right.AdmissionDiagnosisAndPlan);
            AddBlankIfEmpty(response.BlankFields, "MemberNumber", response.Right.MemberNumber);
            AddBlankIfEmpty(response.BlankFields, "PolicyNumber", response.Right.PolicyNumber);
            AddBlankIfEmpty(response.BlankFields, "ResponsiblePerson", responsible?.Name);
            AddBlankIfEmpty(response.BlankFields, "AdmissionOfficer", response.Right.AdmissionOfficer);
            AddBlankIfEmpty(response.BlankFields, "FloorNurse", response.Right.FloorNurse);

            return InpAdmissionResult<InpatientBaseDataResponse>.Ok(response, "Data Dasar Rawat Inap berhasil diambil.");
        }

        /// <summary>
        /// "Rencana @ Kamar (Rp)" — <c>NotYetAvailable</c> sampai Billing menyediakan tarif kamar harian
        /// (<c>RWI-OQ-129</c>, <c>BE-RWI-191</c>); layar menulis "lihat kasir".
        /// </summary>
        public async Task<InpAdmissionResult<InpatientBaseDataAmountsResponse>> GetBaseDataAmountsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<InpatientBaseDataAmountsResponse>();
            }

            var rate = await _sourceReader.GetDailyRoomRateAsync(episodeId, cancellationToken);

            var response = rate.State switch
            {
                InpAdmissionSourceState.Available when rate.Value.HasValue => new InpatientBaseDataAmountsResponse
                {
                    DailyRoomRate = rate.Value,
                    RoomRateState = "Available"
                },
                InpAdmissionSourceState.Available => new InpatientBaseDataAmountsResponse { RoomRateState = "TariffMissing" },
                _ => new InpatientBaseDataAmountsResponse { RoomRateState = "NotYetAvailable" }
            };

            return InpAdmissionResult<InpatientBaseDataAmountsResponse>.Ok(response,
                rate.IsAvailable ? "Tarif kamar berhasil diambil." : "Tarif kamar per hari belum tersedia; tertulis \"lihat kasir\".");
        }

        // =====================================================================
        // Ringkasan hak pasien (GET /patient-rights)
        // =====================================================================

        public async Task<InpAdmissionResult<PatientRightsSummaryResponse>> GetPatientRightsAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<PatientRightsSummaryResponse>();
            }

            return InpAdmissionResult<PatientRightsSummaryResponse>.Ok(
                await LoadPatientRightsAsync(episodeId, cancellationToken),
                "Ringkasan hak pasien berhasil diambil.");
        }

        /// <summary>
        /// Nilai Kepercayaan dan Permintaan Privasi <c>Completed</c> episode ini — versi
        /// <c>Superseded</c> tidak pernah ikut (<c>FR-RWA-112</c>, G-41).
        /// </summary>
        private async Task<PatientRightsSummaryResponse> LoadPatientRightsAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            var documents = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete &&
                    x.Status == InpAdmissionDocumentStatus.Completed &&
                    (x.DocumentType == InpAdmissionDocumentType.BeliefValues ||
                     x.DocumentType == InpAdmissionDocumentType.PrivacyRequest))
                .Select(x => new
                {
                    x.Id,
                    x.DocumentType,
                    x.CompletedAt,
                    IsTransportPrivacyRequested = x.PrivacyRequest != null && x.PrivacyRequest.IsTransportPrivacyRequested,
                    Beliefs = x.BeliefItems.Where(i => !i.IsDelete).OrderBy(i => i.ItemNo).Select(i => i.Text).ToList(),
                    Entries = x.PrivacyEntries.Where(e => !e.IsDelete).OrderBy(e => e.LineNo)
                        .Select(e => new { e.EntryType, e.Text }).ToList()
                })
                .ToListAsync(cancellationToken);

            var response = new PatientRightsSummaryResponse();
            var belief = documents.FirstOrDefault(x => x.DocumentType == InpAdmissionDocumentType.BeliefValues);
            var privacy = documents.FirstOrDefault(x => x.DocumentType == InpAdmissionDocumentType.PrivacyRequest);

            if (belief != null)
            {
                response.BeliefValues = new PatientRightsBeliefResponse
                {
                    DocumentId = belief.Id,
                    CompletedAt = belief.CompletedAt,
                    Items = belief.Beliefs
                };
            }

            if (privacy != null)
            {
                response.Privacy = new PatientRightsPrivacyResponse
                {
                    DocumentId = privacy.Id,
                    IsTransportPrivacyRequested = privacy.IsTransportPrivacyRequested,
                    AllowedVisitorNames = privacy.Entries.Where(x => x.EntryType == InpAdmissionPrivacyEntryType.AllowedVisitor).Select(x => x.Text).ToList(),
                    SpecialRequests = privacy.Entries.Where(x => x.EntryType == InpAdmissionPrivacyEntryType.SpecialServiceRequest).Select(x => x.Text).ToList()
                };
            }

            var parts = new List<string>();

            if (response.BeliefValues != null)
            {
                parts.Add($"Nilai kepercayaan: {response.BeliefValues.Items.Count} butir");
            }

            if (response.Privacy != null)
            {
                parts.Add(PrivacySummary(response.Privacy));
            }

            response.SummaryText = parts.Count == 0 ? null : string.Join("; ", parts);

            return response;
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Tipe kamar Formulir General Consent V1 (<c>RWI-DEC-251</c>): "Khusus" bila episode butuh
        /// isolasi, atau kamar/bed penempatan berjalan bertanda isolasi atau intensif; selain itu "Umum".
        /// Nama kamar dan kelas tidak pernah dipakai menebak.
        /// </summary>
        public static (string RoomType, string Reason) ResolveRoomType(InpAdmissionEpisodeContext context)
        {
            var placement = context.CurrentPlacement;

            if (context.RequiresIsolation)
            {
                return ("Special", "Episode membutuhkan isolasi.");
            }

            if (placement?.IsIntensiveCareBed == true)
            {
                return ("Special", "Bed bertanda perawatan intensif.");
            }

            if (placement?.IsIsolationBed == true)
            {
                return ("Special", "Bed bertanda isolasi.");
            }

            if (placement?.IsIntensiveCareRoom == true)
            {
                return ("Special", "Kamar bertanda perawatan intensif.");
            }

            if (placement?.IsIsolationRoom == true)
            {
                return ("Special", "Kamar bertanda isolasi.");
            }

            return ("General", "Kamar dan bed tidak bertanda isolasi maupun perawatan intensif.");
        }

        private static AdmissionWorkspaceContactResponse? PrimaryContact(
            InpAdmissionSourceResult<IReadOnlyList<PatientPartyCandidate>> candidates)
        {
            if (!candidates.IsAvailable)
            {
                return null;
            }

            var contact = candidates.Value!
                .Where(x => x.Source == PatientPartyCandidateSource.EmergencyContact)
                .OrderByDescending(x => x.IsPrimary)
                .FirstOrDefault()
                ?? candidates.Value!
                    .Where(x => x.IsEmergencyContact)
                    .OrderByDescending(x => x.IsPrimary)
                    .FirstOrDefault();

            return contact == null
                ? null
                : new AdmissionWorkspaceContactResponse
                {
                    Name = contact.Name,
                    RelationshipText = contact.RelationshipText ?? RelationshipTypeLabel(contact.RelationshipType)
                };
        }

        private static InpatientBaseDataResponsiblePersonResponse? ResponsiblePerson(
            InpAdmissionSourceResult<IReadOnlyList<PatientPartyCandidate>> candidates)
        {
            if (!candidates.IsAvailable)
            {
                return null;
            }

            // Penanggung jawab dari relasi atau kontak darurat bertanda penanggung jawab.
            var person = candidates.Value!
                .Where(x => x.IsResponsiblePerson)
                .OrderBy(x => x.Source == PatientPartyCandidateSource.PatientRelationship ? 0 : 1)
                .ThenByDescending(x => x.IsPrimary)
                .FirstOrDefault();

            return person == null
                ? null
                : new InpatientBaseDataResponsiblePersonResponse
                {
                    Name = person.Name,
                    RelationshipText = person.RelationshipText ?? RelationshipTypeLabel(person.RelationshipType),
                    IdentityText = JoinText(person.IdentityType, person.IdentityNumber),
                    Address = person.Address,
                    PhoneNumber = person.PhoneNumber
                };
        }

        private static string? RelationshipTypeLabel(PatientRelationshipType? type)
            => type switch
            {
                null => null,
                PatientRelationshipType.Mother => "ibu",
                PatientRelationshipType.Father => "ayah",
                PatientRelationshipType.Child => "anak",
                PatientRelationshipType.Spouse => "suami/istri",
                PatientRelationshipType.Guardian => "wali",
                PatientRelationshipType.Sibling => "saudara kandung",
                PatientRelationshipType.GrandParent => "kakek/nenek",
                PatientRelationshipType.ResponsiblePerson => "penanggung jawab",
                PatientRelationshipType.EmergencyContact => "kontak darurat",
                _ => "lainnya"
            };

        private static string GuarantorName(EncounterInsuranceContext context)
            => context.PaymentSourceName ?? context.InsuranceProviderName ?? context.CompanyGuarantorName ?? context.PaymentTypeName;

        private static string PaymentCategory(EncounterPaymentType paymentType) => paymentType switch
        {
            EncounterPaymentType.Insurance => "Asuransi",
            EncounterPaymentType.CompanyGuarantor => "Perusahaan",
            _ => "Perorangan"
        };

        private static string PrivacySummary(PatientRightsPrivacyResponse privacy)
        {
            var parts = new List<string>();

            parts.Add(privacy.AllowedVisitorNames.Count > 0
                ? $"Privasi khusus: hanya {privacy.AllowedVisitorNames.Count} kerabat"
                : "Privasi khusus: tanpa daftar kerabat");

            if (privacy.SpecialRequests.Count > 0)
            {
                parts.Add($"{privacy.SpecialRequests.Count} permintaan khusus");
            }

            parts.Add($"privasi transportasi: {(privacy.IsTransportPrivacyRequested ? "Ya" : "Tidak")}");

            return string.Join("; ", parts);
        }

        private static string? JoinText(string? first, string? second, string separator = " ")
        {
            var parts = new[] { first, second }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()).ToList();

            return parts.Count == 0 ? null : string.Join(separator, parts);
        }

        private static void AddBlankIfEmpty(List<string> blankFields, string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value) && !blankFields.Contains(key))
            {
                blankFields.Add(key);
            }
        }
    }
}
