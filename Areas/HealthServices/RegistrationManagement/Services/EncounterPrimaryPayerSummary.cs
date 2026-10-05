using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// RJ-DOC-REV-BE-001. Ringkasan penjamin utama kunjungan untuk layar skrining perawat dan
    /// workspace dokter. Sumbernya sumber pembayaran aktif kunjungan
    /// (<see cref="RegPatientEncounterGuarantor"/>) — baris yang sama yang dipakai Billing
    /// (MPY-ENC-PAYER-001). <see cref="RegPatientEncounter.PaymentType"/> hanya dipakai bila
    /// kunjungan belum punya sumber pembayaran.
    /// </summary>
    public sealed record EncounterPrimaryPayerSummary(
        EncounterPaymentType PaymentType,
        string PrimaryGuarantorName,
        string PrimaryGuarantorTypeName,
        bool IsInsurancePatient,
        bool IsCompanyPatient)
    {
        public static EncounterPrimaryPayerSummary From(RegPatientEncounter? encounter)
        {
            var source = encounter?.PaymentSource;
            var paymentType = ResolvePaymentType(encounter, source);

            var typeName = paymentType switch
            {
                EncounterPaymentType.Insurance => "Asuransi",
                EncounterPaymentType.CompanyGuarantor => "Penjamin Perusahaan",
                _ => "Tunai / Pribadi"
            };

            var name = paymentType switch
            {
                EncounterPaymentType.Insurance =>
                    FirstText(source?.InsuranceProvider?.InsuranceProviderName, source?.PaymentSourceNameSnapshot),
                EncounterPaymentType.CompanyGuarantor =>
                    FirstText(source?.CompanyGuarantor?.CompanyGuarantorName, source?.PaymentSourceNameSnapshot),
                _ => FirstText(source?.PaymentSourceNameSnapshot, encounter?.PaymentMethod?.PaymentMethodName)
            };

            return new EncounterPrimaryPayerSummary(
                paymentType,
                name ?? typeName,
                typeName,
                paymentType == EncounterPaymentType.Insurance,
                paymentType == EncounterPaymentType.CompanyGuarantor);
        }

        /// <summary>
        /// Jenis pada sumber pembayaran menang. Baris lama dapat bertanda <c>Cash</c> padahal sudah
        /// menunjuk kartu asuransi/perusahaan; rujukan kartunya lebih dipercaya daripada tandanya.
        /// </summary>
        private static EncounterPaymentType ResolvePaymentType(
            RegPatientEncounter? encounter,
            RegPatientEncounterGuarantor? source)
        {
            if (source != null && source.PaymentType != EncounterPaymentType.Cash)
                return source.PaymentType;

            if (source?.PatientInsuranceId != null || source?.InsuranceProviderId != null)
                return EncounterPaymentType.Insurance;

            if (source?.PatientCompanyGuarantorId != null || source?.CompanyGuarantorId != null)
                return EncounterPaymentType.CompanyGuarantor;

            return source?.PaymentType ?? encounter?.PaymentType ?? EncounterPaymentType.Cash;
        }

        private static string? FirstText(params string?[] values) =>
            values.Select(x => x?.Trim()).FirstOrDefault(x => !string.IsNullOrEmpty(x));
    }
}
