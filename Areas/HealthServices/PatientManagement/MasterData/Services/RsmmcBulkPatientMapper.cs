using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Pemetaan baris preview kanonik ke <see cref="MstPatient"/> — kontrak `1.3.0` B10 — beserta
    /// pembanding baca ulang langkah <c>VERIFY_PERSISTED</c> (B4.3 langkah 10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tidak ada nilai yang dikarang: tanpa nama pengganti (`PAT-DEC-030`), tanpa pemetaan otomatis ke
    /// <c>Unknown</c>, tanpa perapian teks, dan tanpa pembangkit <c>PatientCode</c>/MRN (`PAT-DEC-020`,
    /// `021`). Kolom tanpa pemetaan legacy yang disetujui — termasuk <c>Citizenship</c>,
    /// <c>NationalityCountryId</c>, <c>DeliveryCaseId</c>, <c>NewbornRegistrationKeyHash</c>, dan
    /// <c>NewbornRegistrationFingerprint</c> (`PAT-DEC-076`) — dibiarkan NULL.
    /// </para>
    /// <para>
    /// <c>IsMember</c> selalu <c>false</c> dan dikirim eksplisit pada INSERT karena konfigurasi EF
    /// <c>HasDefaultValue(true).HasSentinel(true)</c> (`PAT-SRC-032`); tidak ada UPDATE khusus untuk
    /// memaksanya (`PAT-DEC-075`). Preview dengan <c>is_member</c> bukan <c>false</c> atau
    /// <c>is_active</c> bukan <c>true</c> menyimpang dari `PAT-FACT-074`, sehingga ditolak sebagai nilai
    /// tidak sah alih-alih disimpan berbeda dari yang diverifikasi (`PAT-DEC-041`, AC-33).
    /// </para>
    /// </remarks>
    public static class RsmmcBulkPatientMapper
    {
        public static readonly int FullNameMaxLength = MaxLengthOf(nameof(MstPatient.FullName));

        public static readonly int PatientCodeMaxLength = MaxLengthOf(nameof(MstPatient.PatientCode));

        public static readonly int IdentityTypeMaxLength = MaxLengthOf(nameof(MstPatient.IdentityType));

        public static readonly int IdentityNumberMaxLength = MaxLengthOf(nameof(MstPatient.IdentityNumber));

        public static readonly int PhoneNumberMaxLength = MaxLengthOf(nameof(MstPatient.PhoneNumber));

        public static readonly int AddressMaxLength = MaxLengthOf(nameof(MstPatient.Address));

        public static readonly int QrCodePathMaxLength = MaxLengthOf(nameof(MstPatient.QrCodePath));

        /// <summary>
        /// Nilai preview yang tidak sah menurut B10 dan B3.2 <c>PREVIEW_VALUE_INVALID</c>. Pesannya hanya
        /// menyebut nama kolom, tidak pernah nilainya, supaya aman dimasukkan ke respons dan log.
        /// </summary>
        public static IReadOnlyList<string> FindInvalidValues(RsmmcBulkPreviewRow preview, string plannedQrCodePath)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(preview.FullName))
            {
                problems.Add("full_name kosong");
            }
            else if (preview.FullName.Length > FullNameMaxLength)
            {
                problems.Add("full_name melebihi batas kolom");
            }

            CheckLength(problems, "proposed_patient_code", preview.ProposedPatientCode, PatientCodeMaxLength, required: true);
            CheckLength(problems, "identity_type", preview.IdentityType, IdentityTypeMaxLength, required: false);
            CheckLength(problems, "identity_number", preview.IdentityNumber, IdentityNumberMaxLength, required: false);
            CheckLength(problems, "phone_number", preview.PhoneNumber, PhoneNumberMaxLength, required: false);
            CheckLength(problems, "address", preview.Address, AddressMaxLength, required: false);
            CheckLength(problems, "qr_code_path", plannedQrCodePath, QrCodePathMaxLength, required: true);

            CheckRequiredEnum<PatientType>(problems, "patient_type", preview.PatientType);
            CheckRequiredEnum<PatientStatus>(problems, "patient_status", preview.PatientStatus);
            CheckRequiredEnum<PatientRegistrationSource>(problems, "registration_source", preview.RegistrationSource);
            CheckRequiredEnum<Religion>(problems, "religion", preview.Religion);
            CheckRequiredEnum<MaritalStatus>(problems, "marital_status", preview.MaritalStatus);
            CheckRequiredEnum<BloodType>(problems, "blood_type", preview.BloodType);

            // Gender nullable di MstPatient: NULL tetap NULL; nilai lain wajib anggota enum (PAT-GAP-010).
            if (preview.Gender.HasValue && !Enum.IsDefined(typeof(Gender), preview.Gender.Value))
            {
                problems.Add("gender bukan anggota enum");
            }

            if (preview.IsMember is null)
            {
                problems.Add("is_member NULL");
            }
            else if (preview.IsMember.Value)
            {
                problems.Add("is_member bukan false");
            }

            if (preview.IsActive is null)
            {
                problems.Add("is_active NULL");
            }
            else if (!preview.IsActive.Value)
            {
                problems.Add("is_active bukan true");
            }

            if (preview.IsNewborn is null)
            {
                problems.Add("is_newborn NULL");
            }

            if (preview.IsDeceased is null)
            {
                problems.Add("is_deceased NULL");
            }

            return problems;
        }

        /// <summary>
        /// Membentuk entity pasien baru persis dari preview. Hanya boleh dipanggil untuk baris yang
        /// <see cref="FindInvalidValues"/>-nya kosong.
        /// </summary>
        public static MstPatient ToPatient(
            RsmmcBulkPreviewRow preview,
            string finalMedicalRecordNumber,
            string plannedQrCodePath,
            Guid actorUserId,
            DateTime nowUtc)
        {
            return new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = preview.ProposedPatientCode!,
                MedicalRecordNumber = finalMedicalRecordNumber,
                PatientType = (PatientType)preview.PatientType!.Value,
                PatientStatus = (PatientStatus)preview.PatientStatus!.Value,
                RegistrationSource = (PatientRegistrationSource)preview.RegistrationSource!.Value,
                FullName = preview.FullName!,
                NickName = null,
                BirthPlace = null,
                BirthDate = preview.BirthDate,
                Gender = preview.Gender.HasValue ? (Gender)preview.Gender.Value : null,
                Religion = (Religion)preview.Religion!.Value,
                MaritalStatus = (MaritalStatus)preview.MaritalStatus!.Value,
                BloodType = (BloodType)preview.BloodType!.Value,
                IdentityType = preview.IdentityType,
                IdentityNumber = preview.IdentityNumber,
                PhoneNumber = preview.PhoneNumber,
                WhatsAppNumber = null,
                Email = null,
                Address = preview.Address,
                CountryId = null,
                Citizenship = null,
                NationalityCountryId = null,
                ProvinceId = null,
                CityId = null,
                DistrictId = null,
                PostalCodeId = null,
                PhotoPath = null,
                QrCodePath = plannedQrCodePath,
                IsMember = false,
                DefaultMembershipTierId = null,
                ActivePatientMembershipId = null,
                IsNewborn = preview.IsNewborn!.Value,
                MotherPatientId = null,
                BirthOrder = null,
                DeliveryCaseId = null,
                NewbornRegistrationKeyHash = null,
                NewbornRegistrationFingerprint = null,
                BirthWeightGram = null,
                BirthLengthCm = null,
                BirthTime = null,
                DeliveryMethod = null,
                IsDeceased = preview.IsDeceased!.Value,
                DeceasedDate = null,
                MergedToPatientId = null,
                MergeReason = null,
                Notes = null,
                IsActive = true,
                CreateDateTime = nowUtc,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        /// <summary>
        /// Nama kolom yang nilainya di database berbeda dari entity yang disiapkan. Kosong berarti
        /// baca ulang cocok. Hanya nama kolom yang dikembalikan, tidak pernah nilainya.
        /// </summary>
        public static IReadOnlyList<string> FindPersistedDifferences(MstPatient expected, MstPatient persisted)
        {
            var differences = new List<string>();

            void Check(string name, bool equal)
            {
                if (!equal)
                {
                    differences.Add(name);
                }
            }

            Check(nameof(MstPatient.PatientCode), string.Equals(expected.PatientCode, persisted.PatientCode, StringComparison.Ordinal));
            Check(nameof(MstPatient.MedicalRecordNumber), string.Equals(expected.MedicalRecordNumber, persisted.MedicalRecordNumber, StringComparison.Ordinal));
            Check(nameof(MstPatient.PatientType), expected.PatientType == persisted.PatientType);
            Check(nameof(MstPatient.PatientStatus), expected.PatientStatus == persisted.PatientStatus);
            Check(nameof(MstPatient.RegistrationSource), expected.RegistrationSource == persisted.RegistrationSource);
            Check(nameof(MstPatient.FullName), string.Equals(expected.FullName, persisted.FullName, StringComparison.Ordinal));
            Check(nameof(MstPatient.BirthDate), expected.BirthDate?.Date == persisted.BirthDate?.Date);
            Check(nameof(MstPatient.Gender), expected.Gender == persisted.Gender);
            Check(nameof(MstPatient.Religion), expected.Religion == persisted.Religion);
            Check(nameof(MstPatient.MaritalStatus), expected.MaritalStatus == persisted.MaritalStatus);
            Check(nameof(MstPatient.BloodType), expected.BloodType == persisted.BloodType);
            Check(nameof(MstPatient.IdentityType), string.Equals(expected.IdentityType, persisted.IdentityType, StringComparison.Ordinal));
            Check(nameof(MstPatient.IdentityNumber), string.Equals(expected.IdentityNumber, persisted.IdentityNumber, StringComparison.Ordinal));
            Check(nameof(MstPatient.PhoneNumber), string.Equals(expected.PhoneNumber, persisted.PhoneNumber, StringComparison.Ordinal));
            Check(nameof(MstPatient.Address), string.Equals(expected.Address, persisted.Address, StringComparison.Ordinal));
            Check(nameof(MstPatient.QrCodePath), string.Equals(expected.QrCodePath, persisted.QrCodePath, StringComparison.Ordinal));
            Check(nameof(MstPatient.IsNewborn), expected.IsNewborn == persisted.IsNewborn);
            Check(nameof(MstPatient.IsDeceased), expected.IsDeceased == persisted.IsDeceased);
            Check(nameof(MstPatient.CreateBy), expected.CreateBy == persisted.CreateBy);

            // PAT-DEC-075 dan AC-33: nilai fisik, bukan sekadar nilai entity yang disiapkan.
            Check(nameof(MstPatient.IsMember), !persisted.IsMember);
            Check(nameof(MstPatient.IsActive), persisted.IsActive);
            Check(nameof(MstPatient.IsDelete), !persisted.IsDelete);
            Check(nameof(MstPatient.IsCancel), !persisted.IsCancel);

            // Kolom tanpa sumber preview wajib tetap NULL (B10, PAT-DEC-076).
            Check(nameof(MstPatient.Citizenship), persisted.Citizenship is null);
            Check(nameof(MstPatient.NationalityCountryId), persisted.NationalityCountryId is null);
            Check(nameof(MstPatient.DeliveryCaseId), persisted.DeliveryCaseId is null);
            Check(nameof(MstPatient.NewbornRegistrationKeyHash), persisted.NewbornRegistrationKeyHash is null);
            Check(nameof(MstPatient.NewbornRegistrationFingerprint), persisted.NewbornRegistrationFingerprint is null);
            Check(nameof(MstPatient.NickName), persisted.NickName is null);
            Check(nameof(MstPatient.BirthPlace), persisted.BirthPlace is null);
            Check(nameof(MstPatient.WhatsAppNumber), persisted.WhatsAppNumber is null);
            Check(nameof(MstPatient.Email), persisted.Email is null);
            Check(nameof(MstPatient.CountryId), persisted.CountryId is null);
            Check(nameof(MstPatient.ProvinceId), persisted.ProvinceId is null);
            Check(nameof(MstPatient.CityId), persisted.CityId is null);
            Check(nameof(MstPatient.DistrictId), persisted.DistrictId is null);
            Check(nameof(MstPatient.PostalCodeId), persisted.PostalCodeId is null);
            Check(nameof(MstPatient.PhotoPath), persisted.PhotoPath is null);
            Check(nameof(MstPatient.DefaultMembershipTierId), persisted.DefaultMembershipTierId is null);
            Check(nameof(MstPatient.ActivePatientMembershipId), persisted.ActivePatientMembershipId is null);
            Check(nameof(MstPatient.MotherPatientId), persisted.MotherPatientId is null);
            Check(nameof(MstPatient.BirthOrder), persisted.BirthOrder is null);
            Check(nameof(MstPatient.BirthWeightGram), persisted.BirthWeightGram is null);
            Check(nameof(MstPatient.BirthLengthCm), persisted.BirthLengthCm is null);
            Check(nameof(MstPatient.BirthTime), persisted.BirthTime is null);
            Check(nameof(MstPatient.DeliveryMethod), persisted.DeliveryMethod is null);
            Check(nameof(MstPatient.DeceasedDate), persisted.DeceasedDate is null);
            Check(nameof(MstPatient.MergedToPatientId), persisted.MergedToPatientId is null);
            Check(nameof(MstPatient.MergeReason), persisted.MergeReason is null);
            Check(nameof(MstPatient.Notes), persisted.Notes is null);

            return differences;
        }

        private static void CheckLength(List<string> problems, string column, string? value, int maxLength, bool required)
        {
            if (value is null)
            {
                if (required)
                {
                    problems.Add($"{column} kosong");
                }

                return;
            }

            if (value.Length > maxLength)
            {
                problems.Add($"{column} melebihi batas kolom");
            }
        }

        private static void CheckRequiredEnum<TEnum>(List<string> problems, string column, int? value)
            where TEnum : struct, Enum
        {
            if (value is null)
            {
                problems.Add($"{column} NULL");
                return;
            }

            if (!Enum.IsDefined(typeof(TEnum), value.Value))
            {
                problems.Add($"{column} bukan anggota enum");
            }
        }

        private static int MaxLengthOf(string propertyName) =>
            typeof(MstPatient).GetProperty(propertyName)!.GetCustomAttribute<MaxLengthAttribute>()!.Length;
    }
}
