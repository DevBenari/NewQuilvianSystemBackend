using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Aturan tetap per jenis dokumen admisi: nama, slot wajib beserta label cetak V1, dan jenis yang
    /// memuat rupiah — <c>02-backend-architecture.md</c> 13.9 (<c>BE-RWI-193</c>, <c>BE-RWI-194</c>).
    /// </summary>
    /// <remarks>
    /// Contoh: Serah Terima Pasien Baru wajib tiga slot — Admission, CRO, Perawat — sehingga
    /// mencoba menandatangani slot Kepala Ruangan ditolak <c>422 INP-ADM-DOC-031</c>. Pelunasan
    /// Deposit memuat rupiah, sehingga hanya dicetak lewat jalur berupiah (<c>RWI-DEC-258</c>).
    /// </remarks>
    public static class InpAdmissionDocumentRules
    {
        /// <summary>Jenis dokumen yang sudah dikirim pada gelombang MVP; Estimasi Biaya menunggu <c>DEC-INP-020</c>.</summary>
        public static bool IsShipped(InpAdmissionDocumentType type)
            => type is InpAdmissionDocumentType.NewPatientHandover
                or InpAdmissionDocumentType.PrivacyRequest
                or InpAdmissionDocumentType.BeliefValues
                or InpAdmissionDocumentType.CostDifferenceStatement
                or InpAdmissionDocumentType.DepositSettlementStatement;

        /// <summary>Dokumen berupiah hanya dicetak lewat <c>/amount-print</c> (<c>VAL-RWA-42</c>).</summary>
        public static bool HasAmounts(InpAdmissionDocumentType type)
            => type is InpAdmissionDocumentType.DepositSettlementStatement or InpAdmissionDocumentType.CostEstimate;

        /// <summary>Jenis yang punya penanda tangan atau deklarer (<c>InpAdmissionDocumentParty</c>).</summary>
        public static bool HasParty(InpAdmissionDocumentType type)
            => type is InpAdmissionDocumentType.PrivacyRequest
                or InpAdmissionDocumentType.BeliefValues
                or InpAdmissionDocumentType.CostDifferenceStatement
                or InpAdmissionDocumentType.DepositSettlementStatement;

        /// <summary>Jenis yang salinan bekunya memuat penjamin (kamus data 20.2.1).</summary>
        public static bool FreezesGuarantor(InpAdmissionDocumentType type)
            => type is InpAdmissionDocumentType.CostDifferenceStatement
                or InpAdmissionDocumentType.DepositSettlementStatement
                or InpAdmissionDocumentType.CostEstimate;

        /// <summary>Jenis yang "Tanggal" formulirnya wajib dan berbawaan hari ini.</summary>
        public static bool UsesStatementDate(InpAdmissionDocumentType type) => HasParty(type);

        public static IReadOnlyList<InpAdmissionSignatureSlot> RequiredSlots(InpAdmissionDocumentType type) => type switch
        {
            InpAdmissionDocumentType.NewPatientHandover => new[]
            {
                InpAdmissionSignatureSlot.AdmissionOfficer,
                InpAdmissionSignatureSlot.CustomerRelationOfficer,
                InpAdmissionSignatureSlot.ReceivingNurse
            },
            InpAdmissionDocumentType.PrivacyRequest => new[]
            {
                InpAdmissionSignatureSlot.PatientOrFamily,
                InpAdmissionSignatureSlot.HeadNurse
            },
            InpAdmissionDocumentType.BeliefValues => new[]
            {
                InpAdmissionSignatureSlot.PatientOrFamily
            },
            _ => new[]
            {
                InpAdmissionSignatureSlot.PatientOrFamily,
                InpAdmissionSignatureSlot.AdmissionOfficer
            }
        };

        /// <summary>Label cetak V1 slot tanda tangan per jenis (13.9).</summary>
        public static string SlotLabel(InpAdmissionDocumentType type, InpAdmissionSignatureSlot slot) => (type, slot) switch
        {
            (InpAdmissionDocumentType.NewPatientHandover, InpAdmissionSignatureSlot.AdmissionOfficer) => "Admission",
            (InpAdmissionDocumentType.NewPatientHandover, InpAdmissionSignatureSlot.CustomerRelationOfficer) => "CRO",
            (InpAdmissionDocumentType.NewPatientHandover, InpAdmissionSignatureSlot.ReceivingNurse) => "Perawat",
            (InpAdmissionDocumentType.PrivacyRequest, InpAdmissionSignatureSlot.PatientOrFamily) => "Pasien / Keluarga Pasien",
            (InpAdmissionDocumentType.PrivacyRequest, InpAdmissionSignatureSlot.HeadNurse) => "Kepala Ruangan",
            (InpAdmissionDocumentType.BeliefValues, InpAdmissionSignatureSlot.PatientOrFamily) => "Tanda Tangan",
            (InpAdmissionDocumentType.CostDifferenceStatement, InpAdmissionSignatureSlot.PatientOrFamily) => "Yang Membuat Pernyataan",
            (InpAdmissionDocumentType.CostDifferenceStatement, InpAdmissionSignatureSlot.AdmissionOfficer) => "Mengetahui Petugas PPRI",
            (InpAdmissionDocumentType.DepositSettlementStatement, InpAdmissionSignatureSlot.PatientOrFamily) => "Yang menyatakan",
            (InpAdmissionDocumentType.DepositSettlementStatement, InpAdmissionSignatureSlot.AdmissionOfficer) => "Yang menyetujui",
            (InpAdmissionDocumentType.CostEstimate, InpAdmissionSignatureSlot.PatientOrFamily) => "Pasien / keluarga pasien",
            (InpAdmissionDocumentType.CostEstimate, InpAdmissionSignatureSlot.AdmissionOfficer) => "Petugas PPRI / Admission",
            (_, InpAdmissionSignatureSlot.PatientOrFamily) => "Pasien / Keluarga",
            (_, InpAdmissionSignatureSlot.AdmissionOfficer) => "Petugas PPRI",
            (_, InpAdmissionSignatureSlot.CustomerRelationOfficer) => "CRO",
            (_, InpAdmissionSignatureSlot.ReceivingNurse) => "Perawat",
            _ => "Kepala Ruangan"
        };

        /// <summary>Nama dokumen untuk pesan, misalnya "Sudah ada Selisih Biaya yang aktif …".</summary>
        public static string DocumentName(InpAdmissionDocumentType type) => type switch
        {
            InpAdmissionDocumentType.NewPatientHandover => "Serah Terima Pasien Baru",
            InpAdmissionDocumentType.PrivacyRequest => "Permintaan Privasi",
            InpAdmissionDocumentType.BeliefValues => "Nilai Kepercayaan",
            InpAdmissionDocumentType.CostDifferenceStatement => "Selisih Biaya",
            InpAdmissionDocumentType.DepositSettlementStatement => "Pelunasan Deposit",
            InpAdmissionDocumentType.CostEstimate => "Estimasi Biaya",
            _ => "Dokumen admisi"
        };

        /// <summary>Nama status untuk pesan, misalnya "Dokumen berstatus Lengkap tidak dapat diubah."</summary>
        public static string StatusName(InpAdmissionDocumentStatus status) => status switch
        {
            InpAdmissionDocumentStatus.Draft => "Konsep",
            InpAdmissionDocumentStatus.AwaitingSignature => "Menunggu tanda tangan",
            InpAdmissionDocumentStatus.Completed => "Lengkap",
            InpAdmissionDocumentStatus.Superseded => "Digantikan",
            InpAdmissionDocumentStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        /// <summary>Teks hubungan penanda tangan untuk cetakan, misalnya "(suami/istri)".</summary>
        public static string RelationshipLabel(InpAdmissionPartyRelationship? relationship, string? otherText) => relationship switch
        {
            InpAdmissionPartyRelationship.Self => "diri sendiri",
            InpAdmissionPartyRelationship.Spouse => "suami/istri",
            InpAdmissionPartyRelationship.Child => "anak",
            InpAdmissionPartyRelationship.Parent => "orang tua",
            InpAdmissionPartyRelationship.Sibling => "saudara kandung",
            InpAdmissionPartyRelationship.Guardian => "wali",
            _ => string.IsNullOrWhiteSpace(otherText) ? "lainnya" : otherText.Trim()
        };

        public static string SubjectLabel(InpCostDifferenceSubject subject, string? otherText) => subject switch
        {
            InpCostDifferenceSubject.Self => "diri saya sendiri",
            InpCostDifferenceSubject.Wife => "istri saya",
            InpCostDifferenceSubject.Husband => "suami saya",
            InpCostDifferenceSubject.Child => "anak saya",
            _ => string.IsNullOrWhiteSpace(otherText) ? "saudara kandung lainnya" : $"saudara kandung lainnya ({otherText.Trim()})"
        };
    }
}
