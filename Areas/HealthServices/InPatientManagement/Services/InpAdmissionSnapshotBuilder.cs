using System.Text.Json;
using System.Text.Json.Serialization;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Pembentuk salinan beku dokumen admisi saat dikunci — <c>BE-RWI-194</c>, kamus data 20.2.1,
    /// <c>INV-RWA-10</c>, <c>RWI-DEC-263</c>. <c>SnapshotFormatVersion = 1</c>.
    /// </summary>
    /// <remarks>
    /// Isinya <b>hanya</b> yang dicetak jenis dokumen itu: kop, kode formulir, kota, identitas pasien,
    /// episode, penjamin (Selisih Biaya, Pelunasan Deposit, Estimasi), dan angka deposit (Pelunasan
    /// Deposit). Nomor identitas pasien tidak dibekukan karena tidak dicetak dokumen admisi mana pun.
    ///
    /// <para>
    /// Cetakan <c>AwaitingSignature</c> dan sesudahnya selalu dibentuk dari salinan ini. Contoh:
    /// alamat Tn. Budi diubah di master pasien sesudah Privasi dikunci — cetakan tetap alamat lama
    /// dan layar dokumen menulis "Data pasien telah diperbarui sejak dokumen ini dikunci."
    /// </para>
    /// </remarks>
    public sealed class InpAdmissionSnapshotBuilder
    {
        public const int CurrentFormatVersion = 1;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly InpAdmissionSourceReader _sourceReader;

        public InpAdmissionSnapshotBuilder(InpAdmissionSourceReader sourceReader)
        {
            _sourceReader = sourceReader;
        }

        /// <summary>
        /// Membaca seluruh sumber salinan beku lalu membentuknya. Dipakai saat kunci (disimpan) dan
        /// saat mencetak konsep (data hidup, tidak disimpan).
        /// </summary>
        public async Task<InpAdmissionSnapshotBuild> BuildAsync(
            Guid episodeId,
            Guid patientId,
            Guid encounterId,
            InpAdmissionDocumentType documentType,
            int versionNo,
            string? signingCity,
            InpEpisodeDepositSummaryDto? depositSummary,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var identity = await _sourceReader.GetPatientIdentityAsync(patientId, cancellationToken);
            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var episode = await _sourceReader.GetEpisodeAsync(episodeId, cancellationToken);
            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var guarantor = InpAdmissionDocumentRules.FreezesGuarantor(documentType)
                ? await _sourceReader.GetGuarantorAsync(encounterId, cancellationToken)
                : null;

            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var today = InpAdmissionText.ToLocal(nowUtc, timeZone).Date;

            var snapshot = new InpAdmissionDocumentSnapshot
            {
                FormatVersion = CurrentFormatVersion,
                FrozenAt = nowUtc,
                DocumentVersionNo = versionNo,
                Letterhead = BuildLetterhead(hospital),
                FormCode = FormCodeFor(setting, documentType),
                SigningCity = InpAdmissionText.Clean(signingCity),
                Patient = identity.IsAvailable
                    ? BuildPatient(identity.Value!, today)
                    : new InpAdmissionSnapshotPatient(),
                Episode = episode.IsAvailable
                    ? BuildEpisode(episode.Value!)
                    : new InpAdmissionSnapshotEpisode(),
                Guarantor = guarantor is { IsAvailable: true } ? BuildGuarantor(guarantor.Value!) : null,
                Deposit = documentType == InpAdmissionDocumentType.DepositSettlementStatement && depositSummary != null
                    ? new InpAdmissionSnapshotDeposit
                    {
                        MinimumPolicyAmount = depositSummary.MinimumPolicyAmount,
                        ReceivedAmount = depositSummary.TotalReceived,
                        ShortfallAmount = depositSummary.PolicyShortfallAmount,
                        AmountsReadAt = nowUtc
                    }
                    : null
            };

            return new InpAdmissionSnapshotBuild
            {
                Snapshot = snapshot,
                Identity = identity.Value,
                IsPatientAvailable = identity.IsAvailable,
                IsHospitalProfileAvailable = hospital.IsAvailable,
                IsEpisodeAvailable = episode.IsAvailable,
                IsGuarantorAvailable = guarantor?.IsAvailable ?? true,
                TimeZone = timeZone
            };
        }

        public static string Serialize(InpAdmissionDocumentSnapshot snapshot)
            => JsonSerializer.Serialize(snapshot, JsonOptions);

        /// <summary>Salinan beku tersimpan; kosong bila tidak ada atau tidak terbaca.</summary>
        public static InpAdmissionDocumentSnapshot? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<InpAdmissionDocumentSnapshot>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static InpAdmissionSnapshotLetterhead BuildLetterhead(InpAdmissionSourceResult<HospitalSiteProfile> hospital)
        {
            if (!hospital.IsAvailable)
            {
                // Kop berbaris kosong, tidak pernah nilai yang ditanam (RWI-DEC-247).
                return new InpAdmissionSnapshotLetterhead { IsAvailable = false, UnavailableReason = hospital.Reason };
            }

            var profile = hospital.Value!;

            return new InpAdmissionSnapshotLetterhead
            {
                IsAvailable = true,
                SiteName = profile.SiteName,
                SiteCode = profile.SiteCode,
                AddressLines = profile.AddressLines.ToList(),
                PhoneNumber = profile.PhoneNumber,
                Email = profile.Email
            };
        }

        public static LetterheadResponse ToLetterheadResponse(InpAdmissionSnapshotLetterhead letterhead)
            => new()
            {
                IsAvailable = letterhead.IsAvailable,
                UnavailableReason = letterhead.UnavailableReason,
                SiteName = letterhead.SiteName,
                SiteCode = letterhead.SiteCode,
                AddressLines = letterhead.AddressLines.ToList(),
                PhoneNumber = letterhead.PhoneNumber,
                Email = letterhead.Email
            };

        public static InpAdmissionSnapshotPatient BuildPatient(PatientIdentityProfile identity, DateTime today)
            => new()
            {
                FullName = identity.FullName,
                Salutation = InpWristbandRules.ResolveSalutation(
                    identity.IsNewborn, identity.Gender, identity.MaritalStatus, identity.BirthDate, today),
                MedicalRecordNumber = identity.MedicalRecordNumber,
                BirthDate = identity.BirthDate?.Date,
                Gender = identity.Gender?.ToString(),
                GenderName = InpAdmissionText.GenderName(identity.Gender),
                Religion = InpAdmissionText.ReligionName(identity.Religion),
                Address = ComposeAddress(identity)
            };

        public static InpAdmissionSnapshotEpisode BuildEpisode(InpAdmissionEpisodeContext episode)
            => new()
            {
                EpisodeNumber = episode.EpisodeNumber,
                AdmittedAt = episode.AdmittedAt,
                PatientClassName = episode.DisplayPatientClassName,
                RoomName = episode.CurrentPlacement?.RoomName,
                BedName = episode.CurrentPlacement?.BedName,
                ServiceUnitName = episode.DisplayServiceUnitName,
                AttendingDoctorName = episode.AttendingDoctorName
            };

        public static InpAdmissionSnapshotGuarantor BuildGuarantor(EncounterInsuranceContext context)
            => new()
            {
                PaymentType = context.PaymentType.ToString(),
                PaymentTypeName = context.PaymentTypeName,
                GuarantorName = context.PaymentSourceName ?? context.InsuranceProviderName ?? context.CompanyGuarantorName,
                PolicyNumber = context.PolicyNumber,
                MemberNumber = context.MemberNumber,
                CardNumber = context.CardNumber
            };

        /// <summary>Alamat pasien: jalan, kecamatan, kota — bagian yang kosong dilewati.</summary>
        public static string? ComposeAddress(PatientIdentityProfile identity)
        {
            var parts = new[] { identity.Address, identity.DistrictName, identity.CityName }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .ToList();

            return parts.Count == 0 ? null : string.Join(", ", parts);
        }

        /// <summary>Kode formulir per jenis dari pengaturan; kosong dicetak tanpa kode (<c>RWI-DEC-247</c>).</summary>
        public static string? FormCodeFor(InpatientSettingValues setting, InpAdmissionDocumentType documentType) => documentType switch
        {
            InpAdmissionDocumentType.NewPatientHandover => setting.NewPatientHandoverFormCode,
            InpAdmissionDocumentType.PrivacyRequest => setting.PrivacyRequestFormCode,
            InpAdmissionDocumentType.BeliefValues => setting.BeliefValuesFormCode,
            InpAdmissionDocumentType.CostDifferenceStatement => setting.CostDifferenceFormCode,
            InpAdmissionDocumentType.DepositSettlementStatement => setting.DepositSettlementFormCode,
            InpAdmissionDocumentType.CostEstimate => setting.CostEstimateFormCode,
            _ => null
        };

        /// <summary>
        /// Benar bila data pasien hidup berbeda dari salinan beku pada isian yang dicetak
        /// (<c>FR-RWA-124</c>).
        /// </summary>
        public static bool PatientChanged(InpAdmissionSnapshotPatient frozen, InpAdmissionSnapshotPatient live)
            => !string.Equals(frozen.FullName, live.FullName, StringComparison.Ordinal) ||
               !string.Equals(frozen.MedicalRecordNumber, live.MedicalRecordNumber, StringComparison.Ordinal) ||
               frozen.BirthDate?.Date != live.BirthDate?.Date ||
               !string.Equals(frozen.Gender, live.Gender, StringComparison.Ordinal) ||
               !string.Equals(frozen.Religion, live.Religion, StringComparison.Ordinal) ||
               !string.Equals(frozen.Address, live.Address, StringComparison.Ordinal);
    }

    /// <summary>Hasil pembentukan salinan beku beserta keadaan sumber wajibnya.</summary>
    public sealed class InpAdmissionSnapshotBuild
    {
        public InpAdmissionDocumentSnapshot Snapshot { get; init; } = new();

        public PatientIdentityProfile? Identity { get; init; }

        public bool IsPatientAvailable { get; init; }

        public bool IsHospitalProfileAvailable { get; init; }

        public bool IsEpisodeAvailable { get; init; }

        public bool IsGuarantorAvailable { get; init; }

        public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;

        /// <summary><c>VAL-RWA-27</c>: identitas pasien dan profil rumah sakit wajib terbaca untuk dikunci.</summary>
        public bool CanFreeze => IsPatientAvailable && IsHospitalProfileAvailable && IsEpisodeAvailable;
    }

    /// <summary>Bentuk <c>SnapshotJson</c> versi 1 (kamus data 20.2.1).</summary>
    public sealed class InpAdmissionDocumentSnapshot
    {
        public int FormatVersion { get; set; } = InpAdmissionSnapshotBuilder.CurrentFormatVersion;

        public DateTime FrozenAt { get; set; }

        public int DocumentVersionNo { get; set; }

        public InpAdmissionSnapshotLetterhead Letterhead { get; set; } = new();

        public string? FormCode { get; set; }

        public string? SigningCity { get; set; }

        public InpAdmissionSnapshotPatient Patient { get; set; } = new();

        public InpAdmissionSnapshotEpisode Episode { get; set; } = new();

        public InpAdmissionSnapshotGuarantor? Guarantor { get; set; }

        public InpAdmissionSnapshotDeposit? Deposit { get; set; }
    }

    public sealed class InpAdmissionSnapshotLetterhead
    {
        public bool IsAvailable { get; set; }

        public string? UnavailableReason { get; set; }

        public string? SiteName { get; set; }

        public string? SiteCode { get; set; }

        public List<string> AddressLines { get; set; } = new();

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
    }

    public sealed class InpAdmissionSnapshotPatient
    {
        public string? FullName { get; set; }

        public string? Salutation { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }

        public string? GenderName { get; set; }

        public string? Religion { get; set; }

        public string? Address { get; set; }
    }

    public sealed class InpAdmissionSnapshotEpisode
    {
        public string? EpisodeNumber { get; set; }

        public DateTime? AdmittedAt { get; set; }

        public string? PatientClassName { get; set; }

        public string? RoomName { get; set; }

        public string? BedName { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? AttendingDoctorName { get; set; }
    }

    public sealed class InpAdmissionSnapshotGuarantor
    {
        public string? PaymentType { get; set; }

        public string? PaymentTypeName { get; set; }

        public string? GuarantorName { get; set; }

        public string? PolicyNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? CardNumber { get; set; }
    }

    public sealed class InpAdmissionSnapshotDeposit
    {
        public decimal MinimumPolicyAmount { get; set; }

        public decimal ReceivedAmount { get; set; }

        public decimal ShortfallAmount { get; set; }

        public DateTime AmountsReadAt { get; set; }
    }
}
